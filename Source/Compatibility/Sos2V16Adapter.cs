using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SOS2WeaponReadouts.Domain;
using Verse;

namespace SOS2WeaponReadouts.Compatibility
{
    /// <summary>
    /// Translates the supported SOS2 1.6 runtime API into stable readout domain
    /// values without creating a hard assembly dependency.
    /// </summary>
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
            HeatInspectStringMethod = heatCompType.GetMethod(
                "CompInspectStringExtra",
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);

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
            Version assemblyVersion = assemblyName.Version;
            string versionText = assemblyVersion != null &&
                (assemblyVersion.Major != 0 ||
                 assemblyVersion.Minor != 0 ||
                 assemblyVersion.Build > 0 ||
                 assemblyVersion.Revision > 0)
                    ? " " + assemblyVersion
                    : string.Empty;
            Status = new CompatibilityStatus(
                CompatibilityState.Supported,
                "SOS2" + versionText +
                " exposes the supported RimWorld 1.6 weapon API" +
                (ceSurrogateType == null
                    ? "."
                    : ", including Combat Extended surrogate turrets."));
        }

        public CompatibilityStatus Status { get; }

        public MethodInfo HeatInspectStringMethod { get; }

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
            // SOS2 uses a negative amplifier count until spinal weapon values
            // settle, so presenting those transient costs would be misleading.
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

            // Several adjacent cells can expose the same heat grid. Identity
            // deduplication prevents counting its capacity more than once.
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

            // The SOS2 collection types stay behind the reflection boundary,
            // so only their common Count shape is allowed to escape it.
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

        /// <summary>
        /// Captures the validated member shape for each supported turret family
        /// so hot read paths do not repeat reflection discovery.
        /// </summary>
        private sealed class WeaponTypeBinding
        {
            private WeaponTypeBinding(
                Type type,
                FieldInfo heatComp,
                PropertyInfo heatToFire,
                PropertyInfo energyToFire,
                PropertyInfo connectedToBridge,
                FieldInfo amplifierCount,
                FieldInfo spinalComp)
            {
                Type = type;
                HeatComp = heatComp;
                HeatToFire = heatToFire;
                EnergyToFire = energyToFire;
                ConnectedToBridge = connectedToBridge;
                AmplifierCount = amplifierCount;
                SpinalComp = spinalComp;
            }

            public Type Type { get; }
            public FieldInfo HeatComp { get; }
            public PropertyInfo HeatToFire { get; }
            public PropertyInfo EnergyToFire { get; }
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
                    null,
                    null,
                    null);
            }
        }
    }
}
