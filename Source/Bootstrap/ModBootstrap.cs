using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Settings;
using SOS2WeaponReadouts.Runtime;
using Spine.UI.WidgetExtensions;

namespace SOS2WeaponReadouts.Bootstrap
{
    public sealed class SOS2WeaponReadoutsMod : Mod
    {
        public static SOS2WeaponReadoutsMod Instance { get; private set; }

        public SOS2WeaponReadoutsSettings Settings { get; }

        public SOS2WeaponReadoutsMod(ModContentPack content)
            : base(content)
        {
            Instance = this;
            Settings = GetSettings<SOS2WeaponReadoutsSettings>();
            WeaponReadoutRuntime.Initialize(content);
        }

        public override string SettingsCategory()
        {
            return "SOS2 Weapon Readouts";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            RimworldSettingsWidgets.SectionHeader(
                listing,
                "SOS2WR.Settings.General".Translate());
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Enabled".Translate(),
                ref Settings.FeatureEnabled,
                "SOS2WR.Settings.Enabled.Tooltip".Translate());
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Descriptions".Translate(),
                ref Settings.ShowInDescriptions);
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Inspect".Translate(),
                ref Settings.ShowInInspectPane);
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Electrical".Translate(),
                ref Settings.ShowElectricalDraw);
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Network".Translate(),
                ref Settings.ShowNetworkComparison);
            listing.CheckboxLabeled(
                "SOS2WR.Settings.Placement".Translate(),
                ref Settings.ShowPlacementWarnings);

            RimworldSettingsWidgets.SectionHeader(
                listing,
                "SOS2WR.Settings.Compatibility".Translate());
            listing.Label(WeaponReadoutRuntime.CompatibilitySummary);
            listing.End();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            WeaponReadoutRuntime.NotifySettingsChanged();
        }
    }
}
