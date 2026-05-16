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

            normalMenuItem = new ToolStripMenuItem("Normal / Full Şarj", TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Normal), (sender, args) => SetMode(LenovoBatteryMode.Normal));
            storageMenuItem = new ToolStripMenuItem("Pil Koruma / Tasarruf", TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Storage), (sender, args) => SetMode(LenovoBatteryMode.Storage));
            quickMenuItem = new ToolStripMenuItem("Hızlı Şarj", TrayIconFactory.CreateMenuImage(LenovoBatteryMode.Quick), (sender, args) => SetMode(LenovoBatteryMode.Quick));
            refreshMenuItem = new ToolStripMenuItem("Durumu Yenile", TrayIconFactory.CreateRefreshImage(), (sender, args) => RefreshMode(true));
            startupMenuItem = new ToolStripMenuItem("Windows ile başlat", TrayIconFactory.CreateStartupImage(), (sender, args) => ToggleStartup());
            exitMenuItem = new ToolStripMenuItem("Çıkış", TrayIconFactory.CreateExitImage(), (sender, args) => ExitApplication());

            var menu = new ContextMenuStrip();
            menu.Items.Add(normalMenuItem);
            menu.Items.Add(storageMenuItem);
            menu.Items.Add(quickMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(refreshMenuItem);
            menu.Items.Add(startupMenuItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitMenuItem);
            menu.Opening += (sender, args) => UpdateMenuState();

            currentTrayIcon = TrayIconFactory.CreateIcon(currentMode);
            notifyIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Icon = currentTrayIcon,
                Text = "Lenovo Battery: Bilinmiyor",
                Visible = true
            };

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
                    "Mod ayarlandı: " + GetDisplayName(currentMode),
                    ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                HandleError("Pil modu değiştirilemedi.", ex);
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
                    HandleError("Pil modu okunamadı.", ex);
                }
                else
                {
                    AppLogger.Error("Pil modu okunamadı.", ex);
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
                HandleError("Windows ile başlat ayarı değiştirilemedi.", ex);
            }
        }

        private void UpdateMenuState()
        {
            normalMenuItem.Checked = currentMode == LenovoBatteryMode.Normal;
            storageMenuItem.Checked = currentMode == LenovoBatteryMode.Storage;
            quickMenuItem.Checked = currentMode == LenovoBatteryMode.Quick;
            startupMenuItem.Checked = startupManager.IsEnabled();
            notifyIcon.Text = TrimTooltip("Lenovo Battery: " + GetDisplayName(currentMode));
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

        private static string GetDisplayName(LenovoBatteryMode mode)
        {
            switch (mode)
            {
                case LenovoBatteryMode.Normal:
                    return "Normal";
                case LenovoBatteryMode.Storage:
                    return "Pil Koruma";
                case LenovoBatteryMode.Quick:
                    return "Hızlı Şarj";
                default:
                    return "Bilinmiyor";
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
