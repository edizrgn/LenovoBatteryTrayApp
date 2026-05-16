using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace LenovoBatteryTray.Utilities
{
    internal sealed class StartupManager
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private readonly string valueName;

        public StartupManager(string valueName)
        {
            this.valueName = valueName;
        }

        public bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                var value = key == null ? null : key.GetValue(valueName) as string;
                return string.Equals(value, GetStartupCommand(), StringComparison.OrdinalIgnoreCase);
            }
        }

        public void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RunKeyPath))
            {
                if (key == null)
                {
                    throw new InvalidOperationException(LocalizationManager.Text("Error.RunKeyOpenFailed"));
                }

                if (enabled)
                {
                    key.SetValue(valueName, GetStartupCommand(), RegistryValueKind.String);
                }
                else
                {
                    key.DeleteValue(valueName, false);
                }
            }
        }

        private static string GetStartupCommand()
        {
            return "\"" + Application.ExecutablePath + "\"";
        }
    }
}
