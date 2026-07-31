using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Runtime;

namespace SOS2WeaponReadouts.UI
{
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

            foreach (var line in text.Split('\n'))
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
