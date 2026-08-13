using Verse;
#if SOS2_WEAPON_READOUTS_LEGACY_SPINE_SETTINGS
using Spine.Api;
#endif

namespace SOS2WeaponReadouts.Settings
{
    /// <summary>
    /// Persists player choices for which readout surfaces and details are
    /// enabled.
    /// </summary>
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
#if SOS2_WEAPON_READOUTS_LEGACY_SPINE_SETTINGS
            SpineApi.Settings.Scribe(
                this,
                SOS2WeaponReadoutsSettingsRegistry.Definitions);
#else
            SOS2WeaponReadoutsSettingsRegistry.Schema.Scribe(this);
#endif
            base.ExposeData();
        }
    }
}
