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
        internal static readonly SettingsSchema<
            SOS2WeaponReadoutsSettings> Schema =
            new SettingsSchema<SOS2WeaponReadoutsSettings>(
                SettingsSchemaConventions.LowerCamelCase);

        static SOS2WeaponReadoutsSettingsRegistry()
        {
            var general = Schema.Section(
                "general.header",
                "General",
                "SOS2WR.Settings.General");
            general.Toggle("feature.enabled", settings => settings.FeatureEnabled,
                "Enable weapon readouts")
                .Localized("SOS2WR.Settings.Enabled", "SOS2WR.Settings.Enabled.Tooltip");
            general.Toggle("readout.infoCard", settings => settings.ShowInDescriptions,
                "Show information-card readout")
                .Localized("SOS2WR.Settings.Descriptions", "SOS2WR.Settings.Descriptions.Tooltip");
            general.Toggle("readout.live", settings => settings.ShowSelectedWeaponReadout,
                "Show heat per shot on selected weapons").ScribeAs("showInInspectPane")
                .Localized("SOS2WR.Settings.Inspect", "SOS2WR.Settings.Inspect.Tooltip");
            general.Toggle("readout.electrical", settings => settings.ShowElectricalDraw,
                "Show electrical draw")
                .Localized("SOS2WR.Settings.Electrical", "SOS2WR.Settings.Electrical.Tooltip");
            general.Toggle("readout.network", settings => settings.ShowNetworkComparison,
                "Show network comparison")
                .Localized("SOS2WR.Settings.Network", "SOS2WR.Settings.Network.Tooltip");
            general.Toggle("readout.placement", settings => settings.ShowPlacementWarnings,
                "Show placement preview")
                .Localized("SOS2WR.Settings.Placement", "SOS2WR.Settings.Placement.Tooltip");
            var compatibility = Schema.Section(
                "compatibility.header", "Compatibility", "SOS2WR.Settings.Compatibility");
            compatibility.Custom("compatibility.summary", DrawCompatibility);
        }

        private static bool DrawCompatibility(
            Rect rect,
            string label,
            string tooltip,
            SOS2WeaponReadoutsSettings settings,
            bool disabled)
        {
            Widgets.Label(rect, WeaponReadoutRuntime.CompatibilitySummary);
            return false;
        }
    }
}
