using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using SaveOurShip2;
using Verse;

namespace SOS2WeaponReadouts.TestFixture
{
    /// <summary>
    /// Exercises the shared SOS2 weapon shape without taking a build-time
    /// dependency on Combat Extended's optional surrogate assembly.
    /// </summary>
    internal sealed class TestTurretHandle
    {
        private const BindingFlags InstanceFlags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        private readonly FieldInfo holdFire;
        private readonly FieldInfo burstCooldownTicksLeft;
        private readonly PropertyInfo attackVerb;
        private readonly PropertyInfo active;
        private readonly PropertyInfo connectedToBridge;
        private readonly PropertyInfo heatToFire;
        private readonly PropertyInfo energyToFire;
        private readonly MethodInfo orderAttack;
        private readonly MethodInfo resetForcedTarget;

        private TestTurretHandle(ThingWithComps thing)
        {
            Thing = thing ?? throw new ArgumentNullException(nameof(thing));
            Type type = thing.GetType();
            holdFire = RequireField(type, "holdFire");
            burstCooldownTicksLeft =
                RequireField(type, "burstCooldownTicksLeft");
            attackVerb = RequireProperty(type, "AttackVerb");
            active = RequireProperty(type, "Active");
            connectedToBridge = FindProperty(type, "ConnectedToBridge");
            heatToFire = RequireProperty(type, "HeatToFire");
            energyToFire = RequireProperty(type, "EnergyToFire");
            orderAttack = RequireMethod(
                type,
                "OrderAttack",
                typeof(LocalTargetInfo));
            resetForcedTarget = RequireMethod(
                type,
                "ResetForcedTarget");
        }

        public ThingWithComps Thing { get; }

        public CompShipHeat HeatComp =>
            Thing.TryGetComp<CompShipHeat>();

        public CompPowerTrader PowerComp =>
            Thing.TryGetComp<CompPowerTrader>();

        public Verb AttackVerb =>
            (Verb)attackVerb.GetValue(Thing, null);

        public bool Active =>
            Convert.ToBoolean(active.GetValue(Thing, null));

        public bool ConnectedToBridge
        {
            get
            {
                if (connectedToBridge != null)
                {
                    return Convert.ToBoolean(
                        connectedToBridge.GetValue(Thing, null));
                }

                ShipHeatNet network = HeatComp?.myNet;
                return network != null &&
                    (network.PilCons.Any() ||
                     network.AICores.Any() ||
                     network.TacCons.Any());
            }
        }

        public float HeatToFire =>
            Convert.ToSingle(heatToFire.GetValue(Thing, null));

        public float EnergyToFire =>
            Convert.ToSingle(energyToFire.GetValue(Thing, null));

        public bool Spawned => Thing.Spawned;

        public static TestTurretHandle Create(Building building)
        {
            ThingWithComps thing = building as ThingWithComps;
            if (thing == null)
            {
                throw new InvalidOperationException(
                    "SOS2 weapon fixture did not spawn a ThingWithComps: " +
                    (building?.GetType().FullName ?? "<null>"));
            }

            string typeName = thing.GetType().FullName;
            if (typeName != "SaveOurShip2.Building_ShipTurret" &&
                typeName !=
                    "CombatExtended.Compatibility.SOS2Compat." +
                    "Building_ShipTurretCE")
            {
                throw new InvalidOperationException(
                    "unsupported SOS2 weapon fixture type: " + typeName);
            }

            return new TestTurretHandle(thing);
        }

        public void SetHoldFire(bool value)
        {
            holdFire.SetValue(Thing, value);
        }

        public void SetBurstCooldown(int value)
        {
            burstCooldownTicksLeft.SetValue(Thing, value);
        }

        public void OrderAttack(LocalTargetInfo target)
        {
            orderAttack.Invoke(Thing, new object[] { target });
        }

        public void ResetForcedTarget()
        {
            resetForcedTarget.Invoke(Thing, Array.Empty<object>());
        }

        private static FieldInfo RequireField(Type type, string name)
        {
            return FindField(type, name) ??
                throw new MissingFieldException(type.FullName, name);
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type;
                current != null;
                current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    InstanceFlags | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        private static PropertyInfo RequireProperty(
            Type type,
            string name)
        {
            return FindProperty(type, name) ??
                throw new MissingMemberException(type.FullName, name);
        }

        private static PropertyInfo FindProperty(Type type, string name)
        {
            for (Type current = type;
                current != null;
                current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(
                    name,
                    InstanceFlags | BindingFlags.DeclaredOnly);
                if (property != null)
                {
                    return property;
                }
            }

            return null;
        }

        private static MethodInfo RequireMethod(
            Type type,
            string name,
            params Type[] parameters)
        {
            for (Type current = type;
                current != null;
                current = current.BaseType)
            {
                MethodInfo method = current.GetMethod(
                    name,
                    InstanceFlags | BindingFlags.DeclaredOnly,
                    null,
                    parameters,
                    null);
                if (method != null)
                {
                    return method;
                }
            }

            throw new MissingMethodException(type.FullName, name);
        }
    }
}
