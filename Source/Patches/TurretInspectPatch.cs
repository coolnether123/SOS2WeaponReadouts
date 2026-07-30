using System.Reflection;
using HarmonyLib;
using SOS2WeaponReadouts.Runtime;

namespace SOS2WeaponReadouts.Patches
{
    [HarmonyPatch]
    internal static class TurretInspectPatch
    {
        private static bool Prepare()
        {
            return WeaponReadoutRuntime.Adapter?.TurretInspectMethod != null;
        }

        private static MethodBase TargetMethod()
        {
            return WeaponReadoutRuntime.Adapter?.TurretInspectMethod;
        }

        private static void Postfix(
            object __instance,
            ref string __result)
        {
            __result = WeaponReadoutRuntime.AppendInspectReadout(
                __instance,
                __result);
        }
    }
}
