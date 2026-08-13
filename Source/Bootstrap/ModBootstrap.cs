using Verse;
using SOS2WeaponReadouts.Settings;
using SOS2WeaponReadouts.Runtime;
using Spine.Api;
using Spine.UI.SettingsFramework;

namespace SOS2WeaponReadouts.Bootstrap
{
    /// <summary>
    /// Connects the mod to Spine so settings and runtime integration share one
    /// lifecycle owner.
    /// </summary>
    public sealed class SOS2WeaponReadoutsMod :
        SpineMod<SOS2WeaponReadoutsSettings>
    {
        public SOS2WeaponReadoutsMod(ModContentPack content)
            : base(
                content,
                "CoolNether123.SOS2WeaponReadouts",
                new SemanticVersion(1, 1, 0),
                SOS2WeaponReadoutsSettingsRegistry.Schema.Definitions,
                SpineCapability.HarmonyPatching |
                SpineCapability.SettingsSchema,
                new ModSettingsPageOptions { RowHeight = 36f })
        {
            WeaponReadoutRuntime.Initialize(content);
        }

        protected override string SettingsCategoryLabel =>
            "SOS2 Weapon Readouts";

        public override void WriteSettings()
        {
            base.WriteSettings();
            WeaponReadoutRuntime.NotifySettingsChanged();
        }

    }
}
