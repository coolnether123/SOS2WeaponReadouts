using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SOS2WeaponReadouts.Domain;
using Verse;

namespace SOS2WeaponReadouts.Compatibility
{
    internal sealed class Sos2V16Adapter : ISos2WeaponAdapter
    {
        private const string CeSurrogateTypeName =
            "CombatExtended.Compatibility.SOS2Compat.Building_ShipTurretCE";

        private readonly IReadOnlyList<WeaponTypeBinding> weaponTypes;
        private readonly Type heatCompType;
        private readonly Type heatPropsType;
        private readonly FieldInfo heatCompNetwork;
        private readonly PropertyInfo heatCompProps;
        private readonly FieldInfo heatPerPulse;
        private readonly FieldInfo energyToFire;
        private readonly PropertyInfo networkCapacity;
        private readonly PropertyInfo networkUsed;
        private readonly FieldInfo pilotConsoles;
        private readonly FieldInfo aiCores;
        private readonly FieldInfo tacticalConsoles;

        public Sos2V16Adapter()
        {
            var turretType = Sos2AdapterFactory.FindRequiredType(
                "SaveOurShip2.Building_ShipTurret");
            heatCompType = Sos2AdapterFactory.FindRequiredType(
                "SaveOurShip2.CompShipHeat");
            heatPropsType = Sos2AdapterFactory.FindRequiredType(
                "SaveOurShip2.CompProps_ShipHeat");
            var heatNetworkType = Sos2AdapterFactory.FindRequiredType(
                "SaveOurShip2.ShipHeatNet");

            var bindings = new List<WeaponTypeBinding>
            {
                WeaponTypeBinding.CreateSos2(turretType)
            };
            var ceSurrogateType = Sos2AdapterFactory.FindOptionalType(
                CeSurrogateTypeName);
            if (ceSurrogateType != null)
            {
                bindings.Add(WeaponTypeBinding.CreateCeSurrogate(
                    ceSurrogateType));
            }
            weaponTypes = bindings;
            TurretGizmosMethods = bindings
                .Select(binding => binding.GetGizmos)
                .Distinct()
                .ToList();

            heatCompNetwork = RequireField(heatCompType, "myNet");
            heatCompProps = RequireProperty(heatCompType, "Props");
            heatPerPulse = RequireField(heatPropsType, "heatPerPulse");
            energyToFire = RequireField(heatPropsType, "energyToFire");

            networkCapacity = RequireProperty(
                heatNetworkType,
                "StorageCapacity");
            networkUsed = RequireProperty(
                heatNetworkType,
                "StorageUsed");
            pilotConsoles = RequireField(heatNetworkType, "PilCons");
            aiCores = RequireField(heatNetworkType, "AICores");
            tacticalConsoles = RequireField(
                heatNetworkType,
                "TacCons");

            var assemblyName = turretType.Assembly.GetName();
            Status = new CompatibilityStatus(
                CompatibilityState.Supported,
                "SOS2 " + assemblyName.Version +
                " exposes the supported RimWorld 1.6 weapon API" +
                (ceSurrogateType == null
                    ? "."
                    : ", including Combat Extended surrogate turrets."));
        }

        public CompatibilityStatus Status { get; }

        public IReadOnlyList<MethodInfo> TurretGizmosMethods { get; }

        public bool IsWeaponDefinition(ThingDef definition)
        {
            return definition?.thingClass != null &&
                FindWeaponType(definition.thingClass) != null &&
                FindHeatProperties(definition) != null;
        }

        public bool TryReadDefinition(
            ThingDef definition,
            out WeaponReadout readout)
        {
            readout = null;
            var properties = FindHeatProperties(definition);
            if (!IsWeaponDefinition(definition) || properties == null)
            {
                return false;
            }

            readout = ShotCostCalculator.FromDefinition(
                ReadSingle(heatPerPulse, properties),
                ReadSingle(energyToFire, properties));
            return true;
        }

        public bool TryReadPlaced(
            object building,
            out WeaponReadout readout)
        {
            readout = null;
            var binding = FindWeaponType(building?.GetType());
            if (binding == null)
            {
                return false;
            }

            var heatComp = binding.HeatComp.GetValue(building);
            var properties = heatComp == null
                ? null
                : heatCompProps.GetValue(heatComp, null);
            if (properties == null)
            {
                return false;
            }

            var network = heatCompNetwork.GetValue(heatComp);
            var networkReadout = ReadNetwork(
                network,
                binding.ConnectedToBridge == null
                    ? HasBridge(network)
                    : ReadBoolean(
                        binding.ConnectedToBridge,
                        building));
            var hasUnresolvedSpinalAmplifiers =
                binding.SpinalComp != null &&
                binding.AmplifierCount != null &&
                binding.SpinalComp.GetValue(building) != null &&
                Convert.ToInt32(
                    binding.AmplifierCount.GetValue(building)) < 0;

            readout = new WeaponReadout(
                ReadSingle(binding.HeatToFire, building),
                ReadSingle(binding.EnergyToFire, building),
                !hasUnresolvedSpinalAmplifiers,
                networkReadout);
            return true;
        }

        public bool TryReadPlacement(
            ThingDef definition,
            IntVec3 center,
            Rot4 rotation,
            Map map,
            out WeaponReadout readout)
        {
            readout = null;
            if (map == null ||
                !TryReadDefinition(definition, out var definitionReadout))
            {
                return false;
            }

            var networks = new HashSet<object>();
            foreach (var cell in GenAdj.CellsAdjacentCardinal(
                center,
                rotation,
                definition.Size))
            {
                if (!cell.InBounds(map))
                {
                    continue;
                }

                foreach (var thing in cell.GetThingList(map))
                {
                    var heatComp = FindHeatComponent(
                        thing as ThingWithComps);
                    var network = heatComp == null
                        ? null
                        : heatCompNetwork.GetValue(heatComp);
                    if (network != null)
                    {
                        networks.Add(network);
                    }
                }
            }

            var capacity = networks.Sum(ReadNetworkCapacity);
            var used = networks.Sum(ReadNetworkUsed);
            var bridgeConnected = networks.Any(HasBridge);
            readout = new WeaponReadout(
                definitionReadout.HeatPerShot,
                definitionReadout.ElectricalDrawPerShot,
                true,
                new NetworkReadout(
                    networks.Count > 0,
                    bridgeConnected,
                    capacity,
                    used));
            return true;
        }

        private object FindHeatProperties(ThingDef definition)
        {
            return definition?.comps?.FirstOrDefault(
                properties => properties != null &&
                    heatPropsType.IsInstanceOfType(properties));
        }

        private WeaponTypeBinding FindWeaponType(Type candidate)
        {
            return candidate == null
                ? null
                : weaponTypes.FirstOrDefault(
                    binding => binding.Type.IsAssignableFrom(candidate));
        }

        private object FindHeatComponent(ThingWithComps thing)
        {
            return thing?.AllComps?.FirstOrDefault(
                component => component != null &&
                    heatCompType.IsInstanceOfType(component));
        }

        private NetworkReadout ReadNetwork(
            object network,
            bool bridgeConnected)
        {
            if (network == null)
            {
                return new NetworkReadout(false, false, 0f, 0f);
            }

            return new NetworkReadout(
                true,
                bridgeConnected,
                ReadNetworkCapacity(network),
                ReadNetworkUsed(network));
        }

        private float ReadNetworkCapacity(object network)
        {
            return ReadSingle(networkCapacity, network);
        }

        private float ReadNetworkUsed(object network)
        {
            return ReadSingle(networkUsed, network);
        }

        private bool HasBridge(object network)
        {
            return ReadCount(pilotConsoles.GetValue(network)) > 0 ||
                ReadCount(aiCores.GetValue(network)) > 0 ||
                ReadCount(tacticalConsoles.GetValue(network)) > 0;
        }

        private static int ReadCount(object collection)
        {
            if (collection == null)
            {
                return 0;
            }

            var count = collection.GetType().GetProperty("Count");
            return count == null
                ? 0
                : Convert.ToInt32(count.GetValue(collection, null));
        }

        private static float ReadSingle(
            MemberInfo member,
            object instance)
        {
            object value;
            if (member is FieldInfo field)
            {
                value = field.GetValue(instance);
            }
            else
            {
                value = ((PropertyInfo)member).GetValue(instance, null);
            }

            return Convert.ToSingle(value);
        }

        private static bool ReadBoolean(
            PropertyInfo property,
            object instance)
        {
            return Convert.ToBoolean(
                property.GetValue(instance, null));
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            return type.GetField(
                name,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance) ??
                throw new MissingMemberException(type.FullName, name);
        }

        private static PropertyInfo RequireProperty(
            Type type,
            string name)
        {
            return type.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance) ??
                throw new MissingMemberException(type.FullName, name);
        }

        private static MethodInfo RequireMethod(Type type, string name)
        {
            return type.GetMethod(
                name,
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null) ??
                throw new MissingMemberException(type.FullName, name);
        }

        private sealed class WeaponTypeBinding
        {
            private WeaponTypeBinding(
                Type type,
                FieldInfo heatComp,
                PropertyInfo heatToFire,
                PropertyInfo energyToFire,
                MethodInfo getGizmos,
                PropertyInfo connectedToBridge,
                FieldInfo amplifierCount,
                FieldInfo spinalComp)
            {
                Type = type;
                HeatComp = heatComp;
                HeatToFire = heatToFire;
                EnergyToFire = energyToFire;
                GetGizmos = getGizmos;
                ConnectedToBridge = connectedToBridge;
                AmplifierCount = amplifierCount;
                SpinalComp = spinalComp;
            }

            public Type Type { get; }
            public FieldInfo HeatComp { get; }
            public PropertyInfo HeatToFire { get; }
            public PropertyInfo EnergyToFire { get; }
            public MethodInfo GetGizmos { get; }
            public PropertyInfo ConnectedToBridge { get; }
            public FieldInfo AmplifierCount { get; }
            public FieldInfo SpinalComp { get; }

            public static WeaponTypeBinding CreateSos2(Type type)
            {
                return new WeaponTypeBinding(
                    type,
                    RequireField(type, "heatComp"),
                    RequireProperty(type, "HeatToFire"),
                    RequireProperty(type, "EnergyToFire"),
                    RequireMethod(type, "GetGizmos"),
                    RequireProperty(type, "ConnectedToBridge"),
                    RequireField(type, "AmplifierCount"),
                    RequireField(type, "spinalComp"));
            }

            public static WeaponTypeBinding CreateCeSurrogate(Type type)
            {
                return new WeaponTypeBinding(
                    type,
                    RequireField(type, "heatComp"),
                    RequireProperty(type, "HeatToFire"),
                    RequireProperty(type, "EnergyToFire"),
                    RequireMethod(type, "GetGizmos"),
                    null,
                    null,
                    null);
            }
        }
    }
}
