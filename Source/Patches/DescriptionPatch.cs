using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using SOS2WeaponReadouts.Runtime;
using Verse;

namespace SOS2WeaponReadouts.Patches
{
    [HarmonyPatch(
        typeof(ThingDef),
        nameof(ThingDef.SpecialDisplayStats))]
    internal static class DefinitionStatsPatch
    {
        private static IEnumerable<StatDrawEntry> Postfix(
            IEnumerable<StatDrawEntry> __result,
            ThingDef __instance,
            StatRequest req)
        {
            return WeaponReadoutRuntime.AppendDefinitionStats(
                __instance,
                __result);
        }
    }
}
