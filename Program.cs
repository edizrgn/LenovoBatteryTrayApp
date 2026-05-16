using System;
using System.Windows.Forms;
using LenovoBatteryTray.Battery;
using LenovoBatteryTray.Utilities;

namespace LenovoBatteryTray
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            AppLogger.Initialize();
            LocalizationManager.Initialize();

            using (var singleInstance = new SingleInstance("Local\\LenovoBatteryTray"))
            {
                if (!singleInstance.IsFirstInstance)
                {
                    return;
                }

                if (!Environment.Is64BitProcess)
                {
                    var message = LocalizationManager.Text("Error.RequiresX64");
                    AppLogger.Error(message, null);
                    MessageBox.Show(message, "Lenovo Battery Tray", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var resolver = new LenovoVantagePathResolver();
                var controller = new LenovoVantageBatteryModeController(resolver);

                Application.Run(new TrayAppContext(controller));
            }
        }
    }
}
