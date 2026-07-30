using Verse;

namespace SOS2WeaponReadouts.Settings
{
    public sealed class SOS2WeaponReadoutsSettings : ModSettings
    {
        public bool FeatureEnabled;

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref FeatureEnabled,
                "featureEnabled",
                false);
            base.ExposeData();
        }
    }
}
