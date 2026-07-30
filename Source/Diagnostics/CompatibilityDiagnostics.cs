using System;
using System.Collections.Generic;
using Verse;

namespace SOS2WeaponReadouts.Diagnostics
{
    public static class CompatibilityDiagnostics
    {
        private static readonly HashSet<string> ReportedKeys =
            new HashSet<string>(StringComparer.Ordinal);

        public static void ReportOnce(
            string key,
            string message,
            bool warning)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                !ReportedKeys.Add(key))
            {
                return;
            }

            var formatted = "[SOS2 Weapon Readouts] " + message;
            if (warning)
            {
                Log.Warning(formatted);
            }
            else
            {
                Log.Message(formatted);
            }
        }

        public static void ReportExceptionOnce(
            string operation,
            Exception exception)
        {
            ReportOnce(
                "exception:" + operation,
                operation + " failed and has been disabled for this " +
                "session: " + exception.GetType().Name + ": " +
                exception.Message,
                true);
        }
    }
}
