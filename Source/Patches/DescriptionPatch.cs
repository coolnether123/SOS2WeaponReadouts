using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using SOS2WeaponReadouts.Runtime;
using Verse;

namespace SOS2WeaponReadouts.Patches
{
    /// <summary>
    /// Adds missing weapon costs where RimWorld actually builds information
    /// card statistics.
    /// </summary>
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
