using System.Collections.Generic;
using System.Reflection;
using SOS2WeaponReadouts.Domain;
using Verse;

namespace SOS2WeaponReadouts.Compatibility
{
    internal sealed class UnavailableSos2Adapter : ISos2WeaponAdapter
    {
        public UnavailableSos2Adapter(CompatibilityStatus status)
        {
            Status = status;
        }

        public CompatibilityStatus Status { get; }

        public IReadOnlyList<MethodInfo> TurretGizmosMethods =>
            System.Array.Empty<MethodInfo>();

        public bool IsWeaponDefinition(ThingDef definition)
        {
            return false;
        }

        public bool TryReadDefinition(
            ThingDef definition,
            out WeaponReadout readout)
        {
            readout = null;
            return false;
        }

        public bool TryReadPlaced(
            object building,
            out WeaponReadout readout)
        {
            readout = null;
            return false;
        }

        public bool TryReadPlacement(
            ThingDef definition,
            IntVec3 center,
            Rot4 rotation,
            Map map,
            out WeaponReadout readout)
        {
            readout = null;
            return false;
        }
    }
}
