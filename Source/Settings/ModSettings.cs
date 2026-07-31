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
            Scribe_Values.Look(
                ref FeatureEnabled,
                "featureEnabled",
                true);
            Scribe_Values.Look(
                ref ShowInDescriptions,
                "showInDescriptions",
                true);
            Scribe_Values.Look(
                ref ShowSelectedWeaponReadout,
                "showInInspectPane",
                true);
            Scribe_Values.Look(
                ref ShowElectricalDraw,
                "showElectricalDraw",
                true);
            Scribe_Values.Look(
                ref ShowNetworkComparison,
                "showNetworkComparison",
                true);
            Scribe_Values.Look(
                ref ShowPlacementWarnings,
                "showPlacementWarnings",
                true);
            base.ExposeData();
        }
    }
}
