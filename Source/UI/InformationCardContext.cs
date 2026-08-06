using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using SOS2WeaponReadouts.Bootstrap;
using Spine.UI.ContextualSettings;
using UnityEngine;
using Verse;

namespace SOS2WeaponReadouts.UI
{
    /// <summary>
    /// Marks only this mod's generated stat rows so contextual settings never
    /// bind to unrelated information-card content.
    /// </summary>
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

        /// <summary>
        /// Provides a weak-table value without retaining state or extending a
        /// stat row's lifetime.
        /// </summary>
        private sealed class Marker
        {
        }
    }

    /// <summary>
    /// Binds Alt-click navigation to the exact generated row while RimWorld is
    /// drawing its final rectangle.
    /// </summary>
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
