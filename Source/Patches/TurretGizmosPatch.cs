using System.Collections.Generic;
using System.Linq;
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
            return WeaponReadoutRuntime.Adapter?.TurretGizmosMethods?.Count > 0;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            return WeaponReadoutRuntime.Adapter?.TurretGizmosMethods
                ?.Cast<MethodBase>() ??
                Enumerable.Empty<MethodBase>();
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
