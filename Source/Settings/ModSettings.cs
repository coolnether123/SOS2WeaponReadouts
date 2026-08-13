using Verse;

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
            SOS2WeaponReadoutsSettingsRegistry.Schema.Scribe(this);
            base.ExposeData();
        }
    }
}
