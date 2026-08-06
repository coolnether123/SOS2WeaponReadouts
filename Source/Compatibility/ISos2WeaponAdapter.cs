using System.Reflection;
using SOS2WeaponReadouts.Domain;
using Verse;

namespace SOS2WeaponReadouts.Compatibility
{
    /// <summary>
    /// Shields the rest of the mod from SOS2 API details and optional weapon
    /// shapes.
    /// </summary>
    public interface ISos2WeaponAdapter
    {
        CompatibilityStatus Status { get; }

        MethodInfo HeatInspectStringMethod { get; }

        bool IsWeaponDefinition(ThingDef definition);

        bool TryReadDefinition(
            ThingDef definition,
            out WeaponReadout readout);

        bool TryReadPlaced(
            object building,
            out WeaponReadout readout);

        bool TryReadPlacement(
            ThingDef definition,
            IntVec3 center,
            Rot4 rotation,
            Map map,
            out WeaponReadout readout);
    }
}
