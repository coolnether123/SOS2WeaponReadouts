using Verse;
using SOS2WeaponReadouts.Settings;
using SOS2WeaponReadouts.Runtime;
using Spine.Api;
using Spine.UI.SettingsFramework;

namespace SOS2WeaponReadouts.Bootstrap
{
    public sealed class SOS2WeaponReadoutsMod :
        SpineMod<SOS2WeaponReadoutsSettings>
    {
        public SOS2WeaponReadoutsMod(ModContentPack content)
            : base(
                content,
                "CoolNether123.SOS2WeaponReadouts",
                new SemanticVersion(1, 0, 0),
                SOS2WeaponReadoutsSettingsRegistry.Definitions,
                SpineCapability.HarmonyPatching,
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
