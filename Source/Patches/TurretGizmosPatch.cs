using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SOS2WeaponReadouts.Runtime;
using Verse;

namespace SOS2WeaponReadouts.Patches
{
    [HarmonyPatch]
    internal static class TurretGizmosPatch
    {
        private static bool Prepare()
        {
            return WeaponReadoutRuntime.Adapter?.TurretGizmosMethod != null;
        }

        private static MethodBase TargetMethod()
        {
            return WeaponReadoutRuntime.Adapter?.TurretGizmosMethod;
        }

        private static void Postfix(
            object __instance,
            ref IEnumerable<Gizmo> __result)
        {
            __result = WeaponReadoutRuntime.AppendSelectedWeaponGizmo(
                __instance,
                __result);
        }
    }
}
