using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using SOS2WeaponReadouts.Bootstrap;
using Spine.UI.ContextualSettings;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.UI
{
    internal static class InformationCardContext
    {
        private static readonly ConditionalWeakTable<StatDrawEntry, Marker>
            Entries = new ConditionalWeakTable<StatDrawEntry, Marker>();

        internal static void Register(StatDrawEntry entry)
        {
            if (entry != null)
            {
                Entries.GetValue(entry, _ => new Marker());
            }
        }

        internal static bool Contains(StatDrawEntry entry) =>
            entry != null && Entries.TryGetValue(entry, out _);

        private sealed class Marker
        {
        }
    }

    [HarmonyPatch(typeof(StatDrawEntry), nameof(StatDrawEntry.Draw))]
    internal static class InformationCardContextPatch
    {
        private static void Prefix(
            StatDrawEntry __instance,
            float x,
            float y,
            float width,
            string valueCached)
        {
            if (!InformationCardContext.Contains(__instance))
            {
                return;
            }

            float valueWidth = width * 0.45f;
            string value = valueCached ?? __instance.ValueString;
            Rect row = new Rect(
                8f,
                y,
                width,
                Text.CalcHeight(value, valueWidth));
            SOS2WeaponReadoutsMod.ContextualSettings?.Bind(
                row,
                ContextualSettingsTarget.Exact(
                    "readout.infoCard",
                    "general.header"),
                ContextualSettingsBindingOptions.HintOnly(priority: 10));
        }
    }
}
