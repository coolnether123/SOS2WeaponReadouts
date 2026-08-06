using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Runtime;
using SOS2WeaponReadouts.Bootstrap;
using Spine.UI.ContextualSettings;

namespace SOS2WeaponReadouts.UI
{
    /// <summary>
    /// Presents non-blocking weapon costs during legal placement-time OnGUI
    /// drawing without affecting whether a designator may place.
    /// </summary>
    public sealed class WeaponReadoutPlaceWorker : PlaceWorker
    {
        public override void DrawPlaceMouseAttachments(
            float curX,
            ref float curY,
            BuildableDef definition,
            IntVec3 center,
            Rot4 rotation)
        {
            var map = Find.CurrentMap;
            if (!WeaponReadoutRuntime.TryCreatePlacementReadout(
                definition as ThingDef,
                center,
                rotation,
                map,
                out var text,
                out var warning))
            {
                return;
            }

            string[] lines = text.Split('\n');
            float width = 0f;
            for (int index = 0; index < lines.Length; index++)
            {
                width = Mathf.Max(
                    width,
                    Text.CalcSize(lines[index]).x);
            }

            Rect previewRect = new Rect(
                curX,
                curY,
                Mathf.Min(width + 8f, 480f),
                Text.LineHeight * lines.Length);
            if (SOS2WeaponReadoutsMod.ContextualSettings?.Bind(
                previewRect,
                ContextualSettingsTarget.Exact(
                    "readout.placement",
                    "general.header"),
                ContextualSettingsBindingOptions.HintOnly(
                    priority: warning ? 20 : 10)) == true)
            {
                return;
            }

            foreach (var line in lines)
            {
                DrawTextLine(
                    curX,
                    ref curY,
                    warning
                        ? line.Colorize(ColorLibrary.RedReadable)
                        : line);
            }
        }
    }
}
