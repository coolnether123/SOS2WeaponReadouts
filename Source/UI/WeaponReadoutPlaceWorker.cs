using UnityEngine;
using Verse;
using SOS2WeaponReadouts.Runtime;

namespace SOS2WeaponReadouts.UI
{
    public sealed class WeaponReadoutPlaceWorker : PlaceWorker
    {
        public override void DrawGhost(
            ThingDef definition,
            IntVec3 center,
            Rot4 rotation,
            Color ghostColor,
            Thing thing = null)
        {
            var map = Find.CurrentMap;
            if (!WeaponReadoutRuntime.TryCreatePlacementReadout(
                definition,
                center,
                rotation,
                map,
                out var text,
                out var warning))
            {
                return;
            }

            GenMapUI.DrawThingLabel(
                GenMapUI.LabelDrawPosFor(center),
                text,
                warning ? ColorLibrary.RedReadable : Color.white);
        }
    }
}
