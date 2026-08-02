using System.Reflection;
using HarmonyLib;
using SOS2WeaponReadouts.Runtime;
using Verse;

namespace SOS2WeaponReadouts.Patches
{
    [HarmonyPatch]
    internal static class HeatInspectStringPatch
    {
        private static bool Prepare()
        {
            return WeaponReadoutRuntime.Adapter
                ?.HeatInspectStringMethod != null;
        }

        private static MethodBase TargetMethod()
        {
            return WeaponReadoutRuntime.Adapter
                ?.HeatInspectStringMethod;
        }

        private static void Postfix(
            object __instance,
            ref string __result)
        {
            var component = __instance as ThingComp;
            __result = WeaponReadoutRuntime
                .AppendHeatPerShotToInspectString(
                    component?.parent,
                    __result);
        }
    }
}
