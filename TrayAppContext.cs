using System;
using System.Drawing;
using System.Windows.Forms;
using LenovoBatteryTray.Battery;
using LenovoBatteryTray.Utilities;

namespace LenovoBatteryTray
{
    internal sealed class TrayAppContext : ApplicationContext
    {
        private readonly IBatteryModeController controller;
        private readonly NotifyIcon notifyIcon;
        private readonly StartupManager startupManager;
        private readonly ToolStripMenuItem normalMenuItem;
        private readonly ToolStripMenuItem storageMenuItem;
        private readonly ToolStripMenuItem quickMenuItem;
        private readonly ToolStripMenuItem startupMenuItem;
        private readonly ToolStripMenuItem refreshMenuItem;
        private readonly ToolStripMenuItem languageMenuItem;
        private readonly ToolStripMenuItem englishMenuItem;
        private readonly ToolStripMenuItem turkishMenuItem;
        private readonly ToolStripMenuItem exitMenuItem;

        private LenovoBatteryMode currentMode = LenovoBatteryMode.Unknown;
        private Icon currentTrayIcon;

        public TrayAppContext(IBatteryModeController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }

            this.controller = controller;
            startupManager = new StartupManager("LenovoBatteryTray");

            normalMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Normal), (sender, args) => SetMode(LenovoBatteryMode.Normal));
            storageMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Storage), (sender, args) => SetMode(LenovoBatteryMode.Storage));
            quickMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Quick), (sender, args) => SetMode(LenovoBatteryMode.Quick));
            refreshMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateRefreshImage(), (sender, args) => RefreshMode(true));
            startupMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateStartupImage(), (sender, args) => ToggleStartup());
            languageMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateLanguageImage());
            englishMenuItem = new ToolStripMenuItem(string.Empty, null, (sender, args) => ChangeLanguage(AppLanguage.English));
            turkishMenuItem = new ToolStripMenuItem(string.Empty, null, (sender, args) => ChangeLanguage(AppLanguage.Turkish));
            exitMenuItem = new ToolStripMenuItem(string.Empty, TrayIconFactory.CreateExitImage(), (sender, args) => ExitApplication());

            languageMenuItem.DropDownItems.Add(englishMenuItem);
            languageMenuItem.DropDownItems.Add(turkishMenuItem);

            var menu = new ContextMenuStrip();
            menu.Items.Add(normalMenuItem);
            menu.Items.Add(storageMenuItem);
            menu.Items.Add(quickMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(refreshMenuItem);
            menu.Items.Add(startupMenuItem);
            menu.Items.Add(languageMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitMenuItem);
            menu.Opening += (sender, args) => UpdateMenuState();

            currentTrayIcon = TrayIconFactory.CreateIcon(currentMode);
            notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Icon = currentTrayIcon,
                Text = TrimTooltip(LocalizationManager.Format("Tooltip", LocalizationManager.ModeName(currentMode))),
                Visible = true
            };

            ApplyLanguageTexts();
            RefreshMode(true);
        }

        private void SetMode(LenovoBatteryMode mode)
        {
            try
            {
                AppLogger.Info("SetMode requested: " + mode);
                controller.SetMode(mode);
                currentMode = controller.GetMode();
                AppLogger.Info("SetMode verified mode: " + currentMode);
                UpdateMenuState();

                notifyIcon.ShowBalloonTip(
                    2500,
                    "Lenovo Battery",
                    LocalizationManager.Format("Balloon.ModeChanged", LocalizationManager.ModeName(currentMode)),
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                HandleError(LocalizationManager.Text("Error.SetModeFailed"), ex);
            }
        }

        private void RefreshMode(bool showErrors)
        {
            try
            {
                currentMode = controller.GetMode();
                AppLogger.Info("RefreshMode result: " + currentMode);
                UpdateMenuState();
            }
            catch (Exception ex)
            {
                currentMode = LenovoBatteryMode.Unknown;
                UpdateMenuState();

                if (showErrors)
                {
                    HandleError(LocalizationManager.Text("Error.ReadModeFailed"), ex);
                }
                else
                {
                    AppLogger.Error(LocalizationManager.Text("Error.ReadModeFailed"), ex);
                }
            }
        }

        private void ToggleStartup()
        {
            try
            {
                var enable = !startupManager.IsEnabled();
                startupManager.SetEnabled(enable);
                UpdateMenuState();
                AppLogger.Info("Startup enabled: " + enable);
            }
            catch (Exception ex)
            {
                HandleError(LocalizationManager.Text("Error.StartupFailed"), ex);
            }
        }

        private void ChangeLanguage(AppLanguage language)
        {
            LocalizationManager.SetLanguage(language);
            ApplyLanguageTexts();
            UpdateMenuState();
        }

        private void ApplyLanguageTexts()
        {
            normalMenuItem.Text = LocalizationManager.Text("Menu.Normal");
            storageMenuItem.Text = LocalizationManager.Text("Menu.Storage");
            quickMenuItem.Text = LocalizationManager.Text("Menu.Quick");
            refreshMenuItem.Text = LocalizationManager.Text("Menu.Refresh");
            startupMenuItem.Text = LocalizationManager.Text("Menu.Startup");
            languageMenuItem.Text = LocalizationManager.Text("Menu.Language");
            englishMenuItem.Text = LocalizationManager.Text("Menu.English");
            turkishMenuItem.Text = LocalizationManager.Text("Menu.Turkish");
            exitMenuItem.Text = LocalizationManager.Text("Menu.Exit");
        }

        private void UpdateMenuState()
        {
            normalMenuItem.Checked = currentMode == LenovoBatteryMode.Normal;
            storageMenuItem.Checked = currentMode == LenovoBatteryMode.Storage;
            quickMenuItem.Checked = currentMode == LenovoBatteryMode.Quick;
            startupMenuItem.Checked = startupManager.IsEnabled();
            englishMenuItem.Checked = LocalizationManager.CurrentLanguage == AppLanguage.English;
            turkishMenuItem.Checked = LocalizationManager.CurrentLanguage == AppLanguage.Turkish;
            notifyIcon.Text = TrimTooltip(LocalizationManager.Format("Tooltip", LocalizationManager.ModeName(currentMode)));
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            var nextIcon = TrayIconFactory.CreateIcon(currentMode);
            var previousIcon = currentTrayIcon;

            currentTrayIcon = nextIcon;
            notifyIcon.Icon = nextIcon;

            if (previousIcon != null)
            {
                previousIcon.Dispose();
            }
        }

        private static string TrimTooltip(string text)
        {
            if (text.Length <= 63)
            {
                return text;
            }

            return text.Substring(0, 63);
        }

        private void HandleError(string message, Exception ex)
        {
            AppLogger.Error(message, ex);
            MessageBox.Show(
                message + Environment.NewLine + Environment.NewLine + ExceptionFormatter.GetUserMessage(ex),
                "Lenovo Battery Tray",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            ExitThread();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (notifyIcon != null)
                {
                    notifyIcon.Dispose();
                }

                if (currentTrayIcon != null)
                {
                    currentTrayIcon.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}
