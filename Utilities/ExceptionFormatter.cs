using System;

namespace LenovoBatteryTray.Utilities
{
    internal static class ExceptionFormatter
    {
        public static string GetUserMessage(Exception exception)
        {
            if (exception == null)
            {
                return LocalizationManager.Text("Error.Unknown");
            }

            var current = exception;

            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }
    }
}
