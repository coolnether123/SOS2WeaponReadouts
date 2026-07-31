using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SOS2WeaponReadouts.Domain
{
    public static class ReadoutFormatter
    {
        public static string AppendMissing(
            string existing,
            WeaponReadout readout,
            ReadoutPresentation presentation,
            ReadoutLabels labels,
            bool includeSectionHeading)
        {
            if (readout == null ||
                presentation == null ||
                labels == null)
            {
                return existing ?? string.Empty;
            }

            var additions = BuildMissingLines(
                existing,
                readout,
                presentation,
                labels).ToList();
            if (additions.Count == 0)
            {
                return existing ?? string.Empty;
            }

            if (includeSectionHeading &&
                !string.IsNullOrWhiteSpace(labels.Section))
            {
                additions.Insert(0, labels.Section);
            }

            var prefix = (existing ?? string.Empty).TrimEnd();
            if (prefix.Length == 0)
            {
                return string.Join(Environment.NewLine, additions);
            }

            return prefix + Environment.NewLine +
                string.Join(Environment.NewLine, additions);
        }

        public static IReadOnlyList<string> BuildMissingLines(
            string existing,
            WeaponReadout readout,
            ReadoutPresentation presentation,
            ReadoutLabels labels)
        {
            var result = new List<string>();
            if (!readout.DynamicValuesAvailable)
            {
                AddNonBlank(result, labels.ValuesUnavailable);
                return result;
            }

            if (!ExistingReadoutDetector.HasHeatPerShot(existing) &&
                !ContainsIgnoreCase(existing, labels.HeatPerShot))
            {
                result.Add(
                    labels.HeatPerShot + ": " +
                    FormatNumber(readout.HeatPerShot) + " " +
                    labels.HeatUnits);
            }

            if (presentation.ShowElectricalDraw &&
                !ExistingReadoutDetector.HasElectricalPerShot(existing) &&
                !ContainsIgnoreCase(
                    existing,
                    labels.ElectricalDrawPerShot) &&
                !ContainsIgnoreCase(
                    existing,
                    labels.ExistingElectricalLine))
            {
                result.Add(
                    labels.ElectricalDrawPerShot + ": " +
                    FormatNumber(readout.ElectricalDrawPerShot) + " " +
                    labels.ElectricalUnits);
            }

            AppendNetworkLines(
                result,
                existing,
                readout,
                presentation,
                labels);
            return result;
        }

        public static string FormatNumber(float value)
        {
            var rounded = Math.Round(value);
            if (Math.Abs(value - rounded) < 0.005f)
            {
                return rounded.ToString(
                    "0",
                    CultureInfo.InvariantCulture);
            }

            return value.ToString(
                "0.##",
                CultureInfo.InvariantCulture);
        }

        private static void AppendNetworkLines(
            ICollection<string> result,
            string existing,
            WeaponReadout readout,
            ReadoutPresentation presentation,
            ReadoutLabels labels)
        {
            var network = readout.Network;
            if (network == null)
            {
                return;
            }

            if (!network.ThermalNetworkConnected)
            {
                if (!ExistingReadoutDetector.HasThermalNetworkStatus(
                        existing) &&
                    !ContainsIgnoreCase(
                        existing,
                        labels.ExistingThermalDisconnected))
                {
                    AddNonBlank(result, labels.ThermalDisconnected);
                }
                return;
            }

            if (!network.BridgeConnected &&
                !ExistingReadoutDetector.HasBridgeConnectionStatus(
                    existing) &&
                !ContainsIgnoreCase(
                    existing,
                    labels.ExistingBridgeDisconnected) &&
                !ContainsIgnoreCase(existing, labels.BridgeDisconnected))
            {
                AddNonBlank(result, labels.BridgeDisconnected);
            }

            if (!presentation.ShowNetworkComparison ||
                ExistingReadoutDetector.HasCurrentNetworkHeat(existing) ||
                ContainsIgnoreCase(existing, labels.CurrentHeat))
            {
                return;
            }

            result.Add(
                labels.CurrentHeat + ": " +
                FormatNumber(network.Used) + " / " +
                FormatNumber(network.Capacity) + " " +
                labels.HeatUnits);

            if (network.Used + readout.HeatPerShot >
                    network.Capacity + 0.001f &&
                !ContainsIgnoreCase(existing, labels.HeatInsufficient))
            {
                AddNonBlank(result, labels.HeatInsufficient);
            }
        }

        private static bool ContainsIgnoreCase(
            string haystack,
            string needle)
        {
            return !string.IsNullOrWhiteSpace(haystack) &&
                !string.IsNullOrWhiteSpace(needle) &&
                haystack.IndexOf(
                    needle,
                    StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AddNonBlank(
            ICollection<string> target,
            string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                target.Add(value);
            }
        }
    }
}
