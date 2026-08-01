using Spine.UI.SettingsFramework;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.Settings
{
    internal sealed class SOS2WeaponReadoutsSettingsUi
    {
        private readonly SettingsListDrawer drawer =
            new SettingsListDrawer(SOS2WeaponReadoutsSettingsRegistry.Hierarchy)
            {
                SimpleLabel = "Simple",
                AdvancedLabel = "Advanced",
                NoResultsLabel = "No settings match",
                ResetToDefaultLabel = "Reset to default",
                GetLabel = definition => TranslateOrFallback(
                    definition.LabelKey,
                    definition.Label),
                GetTooltip = definition => TranslateOrFallback(
                    definition.TooltipKey,
                    definition.Tooltip),
                RowHeight = 36f
            };
        private SettingsViewMode viewMode = SettingsViewMode.Simple;

        internal SettingsListDrawer Drawer => drawer;

        internal void Draw(Rect rect, SOS2WeaponReadoutsSettings settings)
        {
            drawer.Draw(rect, settings, ref viewMode, settings.Write);
        }

        private static string TranslateOrFallback(
            string key,
            string fallback) =>
            string.IsNullOrEmpty(key)
                ? fallback ?? string.Empty
                : key.Translate().ToString();
    }
}
