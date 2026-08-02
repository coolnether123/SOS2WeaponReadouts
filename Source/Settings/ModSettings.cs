using Spine.Api;
using Verse;

namespace SOS2WeaponReadouts.Settings
{
    public sealed class SOS2WeaponReadoutsSettings : ModSettings
    {
        public bool FeatureEnabled = true;
        public bool ShowInDescriptions = true;
        public bool ShowSelectedWeaponReadout = true;
        public bool ShowElectricalDraw = true;
        public bool ShowNetworkComparison = true;
        public bool ShowPlacementWarnings = true;

        public override void ExposeData()
        {
            SpineApi.Settings.Scribe(
                this,
                SOS2WeaponReadoutsSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
