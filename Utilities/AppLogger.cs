using System;
using System.IO;

namespace LenovoBatteryTray.Utilities
{
    internal static class AppLogger
    {
        private static readonly object SyncRoot = new object();
        private static string logFilePath;

        public static void Initialize()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logDirectory = Path.Combine(appData, "LenovoBatteryTray", "logs");
            Directory.CreateDirectory(logDirectory);
            logFilePath = Path.Combine(logDirectory, "app.log");
            Info("Logger initialized.");
        }

        public static void Info(string message)
        {
            Write("INFO", message, null);
        }

        public static void Error(string message, Exception exception)
        {
            Write("ERROR", message, exception);
        }

        private static void Write(string level, string message, Exception exception)
        {
            try
            {
                if (string.IsNullOrEmpty(logFilePath))
                {
                    return;
                }

                lock (SyncRoot)
                {
                    File.AppendAllText(
                        logFilePath,
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                        + " [" + level + "] "
                        + message
                        + Environment.NewLine
                        + (exception == null ? string.Empty : exception + Environment.NewLine));
                }
            }
            catch
            {
            }
        }
    }
}
