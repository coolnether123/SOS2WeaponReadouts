using System.Collections.Generic;
using SOS2WeaponReadouts.Runtime;
using Spine.UI.SettingsFramework;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.Settings
{
    internal static class SOS2WeaponReadoutsSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                Header("general.header", "General", "SOS2WR.Settings.General", 0),
                Toggle("feature.enabled", nameof(SOS2WeaponReadoutsSettings.FeatureEnabled),
                    "Enable weapon readouts", "SOS2WR.Settings.Enabled", "SOS2WR.Settings.Enabled.Tooltip", true, 10),
                Toggle("readout.infoCard", nameof(SOS2WeaponReadoutsSettings.ShowInDescriptions),
                    "Show information-card readout", "SOS2WR.Settings.Descriptions", null, true, 20),
                Toggle("readout.live", nameof(SOS2WeaponReadoutsSettings.ShowSelectedWeaponReadout),
                    "Show selected-weapon readout", "SOS2WR.Settings.Inspect", null, true, 30),
                Toggle("readout.electrical", nameof(SOS2WeaponReadoutsSettings.ShowElectricalDraw),
                    "Show electrical draw", "SOS2WR.Settings.Electrical", null, true, 40),
                Toggle("readout.network", nameof(SOS2WeaponReadoutsSettings.ShowNetworkComparison),
                    "Show network comparison", "SOS2WR.Settings.Network", null, true, 50),
                Toggle("readout.placement", nameof(SOS2WeaponReadoutsSettings.ShowPlacementWarnings),
                    "Show placement preview", "SOS2WR.Settings.Placement", null, true, 60),
                Header("compatibility.header", "Compatibility", "SOS2WR.Settings.Compatibility", 100),
                new SettingDefinition
                {
                    Id = "compatibility.summary",
                    Type = SettingType.Custom,
                    Label = string.Empty,
                    LabelKey = string.Empty,
                    SortOrder = 110,
                    ShowInSimpleView = true,
                    CustomDrawer = DrawCompatibility
                }
            };

        internal static readonly SettingsHierarchy Hierarchy =
            new SettingsHierarchy(Definitions);

        private static SettingDefinition Header(
            string id,
            string label,
            string labelKey,
            int order) =>
            new SettingDefinition
            {
                Id = id,
                Type = SettingType.Header,
                Label = label,
                LabelKey = labelKey,
                SortOrder = order,
                ShowInSimpleView = true
            };

        private static SettingDefinition Toggle(
            string id,
            string field,
            string label,
            string labelKey,
            string tooltipKey,
            bool defaultValue,
            int order) =>
            new SettingDefinition
            {
                Id = id,
                FieldName = field,
                Type = SettingType.Bool,
                Label = label,
                LabelKey = labelKey,
                TooltipKey = tooltipKey,
                DefaultValue = defaultValue,
                SortOrder = order,
                ShowInSimpleView = true
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
