using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Settings;
using SOS2WeaponReadouts.Runtime;
using Spine.Api;
using Spine.UI.ContextualSettings;

namespace SOS2WeaponReadouts.Bootstrap
{
    public sealed class SOS2WeaponReadoutsMod : Mod
    {
        public static SOS2WeaponReadoutsMod Instance { get; private set; }

        public SOS2WeaponReadoutsSettings Settings { get; }
        private readonly SOS2WeaponReadoutsSettingsUi settingsUi =
            new SOS2WeaponReadoutsSettingsUi();
        private static IContextualSettingsLease contextualSettingsLease;

        public SOS2WeaponReadoutsMod(ModContentPack content)
            : base(content)
        {
            SpineApi.Runtime.Require(new SpineRequirement(
                "CoolNether123.SOS2WeaponReadouts",
                new SemanticVersion(1, 1, 0),
                SpineCapability.Settings |
                SpineCapability.ContextualSettings));

            Instance = this;
            Settings = GetSettings<SOS2WeaponReadoutsSettings>();
            if (contextualSettingsLease == null)
            {
                contextualSettingsLease = SpineApi.ContextualSettings.Acquire(
                    "CoolNether123.SOS2WeaponReadouts",
                    this,
                    settingsUi.Drawer,
                    Settings);
            }
            WeaponReadoutRuntime.Initialize(content);
        }

        internal static IContextualSettingsLease ContextualSettings =>
            contextualSettingsLease;

        public override string SettingsCategory()
        {
            return "SOS2 Weapon Readouts";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settingsUi.Draw(inRect, Settings);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            WeaponReadoutRuntime.NotifySettingsChanged();
        }
    }
}
