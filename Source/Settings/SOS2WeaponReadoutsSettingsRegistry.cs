using System.Collections.Generic;
using SOS2WeaponReadouts.Runtime;
using Spine.UI.SettingsFramework;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.Settings
{
    /// <summary>
    /// Defines the shared Spine settings page so persistence, navigation, and
    /// contextual links use the same stable identifiers.
    /// </summary>
    internal static class SOS2WeaponReadoutsSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                SettingDefinitions.Header(
                    "general.header", "General", "SOS2WR.Settings.General"),
                SettingDefinitions.Toggle(
                    "feature.enabled", nameof(SOS2WeaponReadoutsSettings.FeatureEnabled),
                    "Enable weapon readouts", "SOS2WR.Settings.Enabled",
                    tooltipKey: "SOS2WR.Settings.Enabled.Tooltip",
                    scribeKey: "featureEnabled"),
                SettingDefinitions.Toggle(
                    "readout.infoCard", nameof(SOS2WeaponReadoutsSettings.ShowInDescriptions),
                    "Show information-card readout", "SOS2WR.Settings.Descriptions",
                    tooltipKey: "SOS2WR.Settings.Descriptions.Tooltip",
                    scribeKey: "showInDescriptions"),
                SettingDefinitions.Toggle(
                    "readout.live", nameof(SOS2WeaponReadoutsSettings.ShowSelectedWeaponReadout),
                    "Show heat per shot on selected weapons", "SOS2WR.Settings.Inspect",
                    tooltipKey: "SOS2WR.Settings.Inspect.Tooltip",
                    scribeKey: "showInInspectPane"),
                SettingDefinitions.Toggle(
                    "readout.electrical", nameof(SOS2WeaponReadoutsSettings.ShowElectricalDraw),
                    "Show electrical draw", "SOS2WR.Settings.Electrical",
                    tooltipKey: "SOS2WR.Settings.Electrical.Tooltip",
                    scribeKey: "showElectricalDraw"),
                SettingDefinitions.Toggle(
                    "readout.network", nameof(SOS2WeaponReadoutsSettings.ShowNetworkComparison),
                    "Show network comparison", "SOS2WR.Settings.Network",
                    tooltipKey: "SOS2WR.Settings.Network.Tooltip",
                    scribeKey: "showNetworkComparison"),
                SettingDefinitions.Toggle(
                    "readout.placement", nameof(SOS2WeaponReadoutsSettings.ShowPlacementWarnings),
                    "Show placement preview", "SOS2WR.Settings.Placement",
                    tooltipKey: "SOS2WR.Settings.Placement.Tooltip",
                    scribeKey: "showPlacementWarnings"),
                SettingDefinitions.Header(
                    "compatibility.header", "Compatibility", "SOS2WR.Settings.Compatibility"),
                SettingDefinitions.Custom(
                    "compatibility.summary", DrawCompatibility)
            };

        private static bool DrawCompatibility(
            Rect rect,
            string label,
            string tooltip,
            object settings,
            bool disabled)
        {
            Widgets.Label(rect, WeaponReadoutRuntime.CompatibilitySummary);
            return false;
        }
    }
}
