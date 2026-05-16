using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Win32;
using LenovoBatteryTray.Battery;

namespace LenovoBatteryTray.Utilities
{
    internal static class LocalizationManager
    {
        private const string RegistryKeyPath = @"Software\LenovoBatteryTray";
        private const string LanguageValueName = "Language";

        private static readonly IDictionary<string, string> English = new Dictionary<string, string>
        {
            { "Menu.Normal", "Normal / Full Charge" },
            { "Menu.Storage", "Battery Conservation" },
            { "Menu.Quick", "Quick Charge" },
            { "Menu.Refresh", "Refresh Status" },
            { "Menu.Startup", "Start with Windows" },
            { "Menu.Language", "Language" },
            { "Menu.English", "English" },
            { "Menu.Turkish", "Turkish" },
            { "Menu.Exit", "Exit" },
            { "Mode.Normal", "Normal" },
            { "Mode.Storage", "Conservation" },
            { "Mode.Quick", "Quick Charge" },
            { "Mode.Unknown", "Unknown" },
            { "Tooltip", "Lenovo Battery: {0}" },
            { "Balloon.ModeChanged", "Mode changed: {0}" },
            { "Error.RequiresX64", "This application must run as x64." },
            { "Error.ReadModeFailed", "Could not read battery mode." },
            { "Error.SetModeFailed", "Could not change battery mode." },
            { "Error.StartupFailed", "Could not change the Windows startup setting." },
            { "Error.LenovoDllsNotFound", "Lenovo Vantage IdeaNotebookAddin DLLs were not found. Make sure Lenovo Vantage is installed and up to date." },
            { "Error.UnknownCannotBeSet", "Unknown mode cannot be set." },
            { "Error.ReadModeInvocation", "An error occurred while reading Lenovo battery mode." },
            { "Error.SetModeInvocation", "An error occurred while changing Lenovo battery mode." },
            { "Error.MissingAgentMethods", "One of the required BatteryAgent methods was not found." },
            { "Error.NullAgent", "BatteryAgent.GetInstance returned null." },
            { "Error.RunKeyOpenFailed", "The Windows Run registry key could not be opened." },
            { "Error.Unknown", "Unknown error." }
        };

        private static readonly IDictionary<string, string> Turkish = new Dictionary<string, string>
        {
            { "Menu.Normal", "Normal / Full Şarj" },
            { "Menu.Storage", "Pil Koruma / Tasarruf" },
            { "Menu.Quick", "Hızlı Şarj" },
            { "Menu.Refresh", "Durumu Yenile" },
            { "Menu.Startup", "Windows ile başlat" },
            { "Menu.Language", "Dil" },
            { "Menu.English", "İngilizce" },
            { "Menu.Turkish", "Türkçe" },
            { "Menu.Exit", "Çıkış" },
            { "Mode.Normal", "Normal" },
            { "Mode.Storage", "Pil Koruma" },
            { "Mode.Quick", "Hızlı Şarj" },
            { "Mode.Unknown", "Bilinmiyor" },
            { "Tooltip", "Lenovo Battery: {0}" },
            { "Balloon.ModeChanged", "Mod ayarlandı: {0}" },
            { "Error.RequiresX64", "Bu uygulama x64 çalışmalıdır." },
            { "Error.ReadModeFailed", "Pil modu okunamadı." },
            { "Error.SetModeFailed", "Pil modu değiştirilemedi." },
            { "Error.StartupFailed", "Windows ile başlat ayarı değiştirilemedi." },
            { "Error.LenovoDllsNotFound", "Lenovo Vantage IdeaNotebookAddin DLL’leri bulunamadı. Lenovo Vantage’ın kurulu ve güncel olduğundan emin olun." },
            { "Error.UnknownCannotBeSet", "Unknown modu ayarlanamaz." },
            { "Error.ReadModeInvocation", "Lenovo pil modu okunurken hata oluştu." },
            { "Error.SetModeInvocation", "Lenovo pil modu değiştirilirken hata oluştu." },
            { "Error.MissingAgentMethods", "BatteryAgent üzerinde gerekli methodlardan biri bulunamadı." },
            { "Error.NullAgent", "BatteryAgent.GetInstance null döndürdü." },
            { "Error.RunKeyOpenFailed", "Windows Run registry anahtarı açılamadı." },
            { "Error.Unknown", "Bilinmeyen hata." }
        };

        public static AppLanguage CurrentLanguage { get; private set; }

        public static void Initialize()
        {
            CurrentLanguage = LoadSavedLanguage() ?? GetSystemLanguage();
            AppLogger.Info("Language initialized: " + CurrentLanguage);
        }

        public static void SetLanguage(AppLanguage language)
        {
            CurrentLanguage = language;
            SaveLanguage(language);
            AppLogger.Info("Language changed: " + CurrentLanguage);
        }

        public static string Text(string key)
        {
            string value;

            if (GetDictionary(CurrentLanguage).TryGetValue(key, out value))
            {
                return value;
            }

            if (English.TryGetValue(key, out value))
            {
                return value;
            }

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, Text(key), args);
        }

        public static string ModeName(LenovoBatteryMode mode)
        {
            switch (mode)
            {
                case LenovoBatteryMode.Normal:
                    return Text("Mode.Normal");
                case LenovoBatteryMode.Storage:
                    return Text("Mode.Storage");
                case LenovoBatteryMode.Quick:
                    return Text("Mode.Quick");
                default:
                    return Text("Mode.Unknown");
            }
        }

        private static IDictionary<string, string> GetDictionary(AppLanguage language)
        {
            return language == AppLanguage.Turkish ? Turkish : English;
        }

        private static AppLanguage GetSystemLanguage()
        {
            return string.Equals(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, "tr", StringComparison.OrdinalIgnoreCase)
                ? AppLanguage.Turkish
                : AppLanguage.English;
        }

        private static AppLanguage? LoadSavedLanguage()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
            {
                var value = key == null ? null : key.GetValue(LanguageValueName) as string;
                AppLanguage language;

                if (Enum.TryParse(value, true, out language))
                {
                    return language;
                }
            }

            return null;
        }

        private static void SaveLanguage(AppLanguage language)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath))
            {
                if (key != null)
                {
                    key.SetValue(LanguageValueName, language.ToString(), RegistryValueKind.String);
                }
            }
        }
    }
}
