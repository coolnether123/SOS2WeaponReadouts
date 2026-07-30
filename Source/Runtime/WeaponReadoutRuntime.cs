using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SOS2WeaponReadouts.Bootstrap;
using SOS2WeaponReadouts.Compatibility;
using SOS2WeaponReadouts.Diagnostics;
using SOS2WeaponReadouts.Domain;
using SOS2WeaponReadouts.UI;
using Verse;

namespace SOS2WeaponReadouts.Runtime
{
    public static class WeaponReadoutRuntime
    {
        private const string HarmonyId =
            "CoolNether123.SOS2WeaponReadouts";
        private static bool initialized;

        public static ISos2WeaponAdapter Adapter { get; private set; }

        public static string CompatibilitySummary =>
            Adapter?.Status.Detail ??
            "SOS2 adapter has not initialized.";

        public static void Initialize(ModContentPack content)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            Adapter = Sos2AdapterFactory.Create();
            CompatibilityDiagnostics.ReportOnce(
                "compatibility",
                Adapter.Status.Detail,
                !Adapter.Status.IsSupported);
            if (!Adapter.Status.IsSupported)
            {
                return;
            }

            try
            {
                new Harmony(HarmonyId).PatchAll(
                    Assembly.GetExecutingAssembly());
                LongEventHandler.ExecuteWhenFinished(
                    PlacementWorkerInstaller.Install);
            }
            catch (Exception exception)
            {
                CompatibilityDiagnostics.ReportExceptionOnce(
                    "Harmony and placement integration",
                    exception);
            }
        }

        public static void NotifySettingsChanged()
        {
            // Runtime consumers read the persisted settings directly, so
            // toggles apply immediately without rebuilding caches.
        }

        public static IEnumerable<StatDrawEntry> AppendDefinitionStats(
            ThingDef definition,
            IEnumerable<StatDrawEntry> existing)
        {
            var entries = (existing ?? Enumerable.Empty<StatDrawEntry>())
                .ToList();
            var settings = SOS2WeaponReadoutsMod.Instance?.Settings;
            if (settings == null ||
                !settings.FeatureEnabled ||
                !settings.ShowInDescriptions ||
                Adapter == null)
            {
                return entries;
            }

            try
            {
                if (!Adapter.TryReadDefinition(
                    definition,
                    out var readout))
                {
                    return entries;
                }

                var existingText = string.Join(
                    "\n",
                    entries.Select(
                        entry => entry.LabelCap + ": " +
                            entry.ValueString));
                var lines = ReadoutFormatter.BuildMissingLines(
                    existingText,
                    readout,
                    CreatePresentation(settings),
                    ReadoutLocalizer.CreateLabels(readout));
                if (lines.Count == 0)
                {
                    return entries;
                }

                var values = lines.Select(line =>
                {
                    var separator = line.IndexOf(
                        ": ",
                        StringComparison.Ordinal);
                    return separator < 0
                        ? line
                        : line.Substring(separator + 2);
                });
                entries.Add(new StatDrawEntry(
                    StatCategoryDefOf.Weapon_Ranged,
                    "SOS2WR.Readout.Section".Translate(),
                    string.Join(" / ", values),
                    string.Join("\n", lines),
                    4900));
                return entries;
            }
            catch (Exception exception)
            {
                CompatibilityDiagnostics.ReportExceptionOnce(
                    "definition stats readout",
                    exception);
                return entries;
            }
        }

        public static string AppendInspectReadout(
            object building,
            string existing)
        {
            var settings = SOS2WeaponReadoutsMod.Instance?.Settings;
            if (settings == null ||
                !settings.FeatureEnabled ||
                !settings.ShowInInspectPane ||
                Adapter == null)
            {
                return existing;
            }

            try
            {
                if (!Adapter.TryReadPlaced(
                    building,
                    out var readout))
                {
                    return existing;
                }

                return ReadoutFormatter.AppendMissing(
                    existing,
                    readout,
                    CreatePresentation(settings),
                    ReadoutLocalizer.CreateLabels(readout),
                    false);
            }
            catch (Exception exception)
            {
                CompatibilityDiagnostics.ReportExceptionOnce(
                    "inspect readout",
                    exception);
                return existing;
            }
        }

        public static bool TryCreatePlacementReadout(
            ThingDef definition,
            IntVec3 center,
            Rot4 rotation,
            Map map,
            out string text,
            out bool warning)
        {
            text = string.Empty;
            warning = false;
            var settings = SOS2WeaponReadoutsMod.Instance?.Settings;
            if (settings == null ||
                !settings.FeatureEnabled ||
                !settings.ShowPlacementWarnings ||
                Adapter == null)
            {
                return false;
            }

            try
            {
                if (!Adapter.TryReadPlacement(
                    definition,
                    center,
                    rotation,
                    map,
                    out var readout))
                {
                    return false;
                }

                var labels = ReadoutLocalizer.CreateLabels(readout);
                var lines = ReadoutFormatter.BuildMissingLines(
                    string.Empty,
                    readout,
                    CreatePresentation(settings),
                    labels);
                text = string.Join("\n", lines);
                warning = readout.Network != null &&
                    (!readout.Network.ThermalNetworkConnected ||
                     !readout.Network.BridgeConnected ||
                     readout.Network.Used + readout.HeatPerShot >
                        readout.Network.Capacity + 0.001f);
                return !string.IsNullOrWhiteSpace(text);
            }
            catch (Exception exception)
            {
                CompatibilityDiagnostics.ReportExceptionOnce(
                    "placement preview",
                    exception);
                return false;
            }
        }

        private static ReadoutPresentation CreatePresentation(
            Settings.SOS2WeaponReadoutsSettings settings)
        {
            return new ReadoutPresentation
            {
                ShowElectricalDraw = settings.ShowElectricalDraw,
                ShowNetworkComparison =
                    settings.ShowNetworkComparison
            };
        }
    }
}
