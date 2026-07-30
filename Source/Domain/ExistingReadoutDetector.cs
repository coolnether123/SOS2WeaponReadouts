using System;

namespace SOS2WeaponReadouts.Domain
{
    public static class ExistingReadoutDetector
    {
        public static bool HasHeatPerShot(string text)
        {
            return ContainsCostLine(text, "heat", "hu");
        }

        public static bool HasElectricalPerShot(string text)
        {
            return AnyLineMatches(
                text,
                line =>
                {
                    var hasElectricalTerm =
                        line.Contains("energy") ||
                        line.Contains("electrical") ||
                        line.Contains("power");
                    return hasElectricalTerm &&
                        line.Contains("wd") &&
                        HasFiringTerm(line);
                });
        }

        public static bool HasThermalNetworkStatus(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.ToLowerInvariant();
            return normalized.Contains("thermal network") ||
                normalized.Contains("heat network");
        }

        public static bool HasBridgeConnectionStatus(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.ToLowerInvariant();
            return normalized.Contains("bridge") &&
                (normalized.Contains("heat network") ||
                 normalized.Contains("thermal network"));
        }

        public static bool HasNetworkComparison(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.ToLowerInvariant();
            return normalized.Contains("heat after") ||
                normalized.Contains("headroom after") ||
                normalized.Contains("insufficient heat capacity");
        }

        private static bool ContainsCostLine(
            string text,
            string subject,
            string unit)
        {
            return AnyLineMatches(
                text,
                line => line.Contains(subject) &&
                    line.Contains(unit) &&
                    HasFiringTerm(line));
        }

        private static bool HasFiringTerm(string text)
        {
            return text.Contains("per shot") ||
                text.Contains("per firing") ||
                text.Contains("to fire");
        }

        private static bool AnyLineMatches(
            string text,
            Func<string, bool> predicate)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var lines = text.Split(
                new[] { "\r\n", "\n", "\r" },
                StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (predicate(line.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
