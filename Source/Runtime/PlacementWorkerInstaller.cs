using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SOS2WeaponReadouts.Diagnostics;
using SOS2WeaponReadouts.UI;
using Verse;

namespace SOS2WeaponReadouts.Runtime
{
    /// <summary>
    /// Attaches the non-blocking preview only to definitions recognized by the
    /// negotiated SOS2 adapter.
    /// </summary>
    internal static class PlacementWorkerInstaller
    {
        public static void Install()
        {
            var adapter = WeaponReadoutRuntime.Adapter;
            if (adapter == null || !adapter.Status.IsSupported)
            {
                return;
            }

            var installedCount = 0;
            foreach (var definition in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!adapter.IsWeaponDefinition(definition))
                {
                    continue;
                }

                if (definition.placeWorkers == null)
                {
                    definition.placeWorkers = new List<Type>();
                }
                if (definition.placeWorkers.Any(
                    worker => worker ==
                        typeof(WeaponReadoutPlaceWorker)))
                {
                    continue;
                }

                definition.placeWorkers.Add(
                    typeof(WeaponReadoutPlaceWorker));
                // RimWorld caches worker instances, so the definition mutation
                // is invisible until that private cache is invalidated.
                AccessTools.Field(
                    typeof(BuildableDef),
                    "placeWorkersInstantiatedInt")
                    ?.SetValue(definition, null);
                installedCount++;
            }

            CompatibilityDiagnostics.ReportOnce(
                "placement-workers",
                "Added non-blocking placement readouts to " +
                installedCount + " SOS2 weapon definitions.",
                false);
        }
    }
}
