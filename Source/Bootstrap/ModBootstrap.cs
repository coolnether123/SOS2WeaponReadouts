using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Compatibility;
using SOS2WeaponReadouts.Patches;
using SOS2WeaponReadouts.Settings;

namespace SOS2WeaponReadouts.Bootstrap
{
    public sealed class SOS2WeaponReadoutsMod : Mod
    {
        private readonly SOS2WeaponReadoutsSettings settings;

        public SOS2WeaponReadoutsMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<SOS2WeaponReadoutsSettings>();
            CompatibilityRegistry.InitializeAll();
            PatchInstaller.InstallAll();
        }

        public override string SettingsCategory()
        {
            return "SOS2 Weapon Readouts";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "Feature enabled",
                ref settings.FeatureEnabled);
            listing.End();
        }
    }
}
