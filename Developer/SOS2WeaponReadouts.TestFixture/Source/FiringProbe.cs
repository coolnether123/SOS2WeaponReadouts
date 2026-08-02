using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using SaveOurShip2;
using Verse;

namespace SOS2WeaponReadouts.TestFixture
{
    internal static class FiringProbe
    {
        private static readonly Dictionary<ThingWithComps, ProbeRecord>
            Records = new Dictionary<ThingWithComps, ProbeRecord>();

        internal static void Reset(TestTurretHandle turret)
        {
            Records[turret.Thing] = new ProbeRecord();
        }

        internal static ProbeRecord Snapshot(TestTurretHandle turret)
        {
            ProbeRecord record;
            return Records.TryGetValue(turret.Thing, out record)
                ? record.Copy()
                : new ProbeRecord();
        }

        internal static void Forget(TestTurretHandle turret)
        {
            if (turret != null)
            {
                Records.Remove(turret.Thing);
            }
        }

        internal static void ObserveCast(Verb verb)
        {
            ThingWithComps turret = verb.caster as ThingWithComps;
            ProbeRecord record;
            if (turret != null &&
                Records.TryGetValue(turret, out record))
            {
                record.CastShotCount++;
                PropertyInfo projectile = AccessTools.Property(
                    verb.GetType(),
                    "Projectile");
                record.VerbProjectileDef =
                    (projectile?.GetValue(verb, null) as ThingDef)
                        ?.defName ?? "<none>";
                record.LastTarget =
                    verb.CurrentTarget.Cell.ToString();
            }
        }

        internal static void ObserveProjectileLaunch(
            Projectile projectile,
            Thing launcher)
        {
            ThingWithComps turret = launcher as ThingWithComps;
            ProbeRecord record;
            if (turret != null &&
                Records.TryGetValue(turret, out record))
            {
                record.ProjectileLaunchCount++;
                record.LastProjectileDef =
                    projectile?.def?.defName ?? "<none>";
            }
        }

        internal static void ObserveBeginBurstBefore(object instance)
        {
            ThingWithComps turret = instance as ThingWithComps;
            ProbeRecord record;
            if (turret == null ||
                !Records.TryGetValue(turret, out record))
            {
                return;
            }

            record.BeginBurstCount++;
            CompShipHeat heatComp = turret.TryGetComp<CompShipHeat>();
            CompPowerTrader powerComp =
                turret.TryGetComp<CompPowerTrader>();
            record.HeatBefore = heatComp.myNet.StorageUsed;
            record.PowerBefore =
                powerComp.PowerNet.CurrentStoredEnergy();
        }

        internal static void ObserveBeginBurstAfter(object instance)
        {
            ThingWithComps turret = instance as ThingWithComps;
            ProbeRecord record;
            if (turret == null ||
                !Records.TryGetValue(turret, out record))
            {
                return;
            }

            CompShipHeat heatComp = turret.TryGetComp<CompShipHeat>();
            CompPowerTrader powerComp =
                turret.TryGetComp<CompPowerTrader>();
            record.HeatAfter = heatComp.myNet.StorageUsed;
            record.PowerAfter =
                powerComp.PowerNet.CurrentStoredEnergy();
        }
    }

    [HarmonyPatch(
        typeof(Projectile),
        nameof(Projectile.Launch),
        new[]
        {
            typeof(Thing),
            typeof(LocalTargetInfo),
            typeof(LocalTargetInfo),
            typeof(ProjectileHitFlags),
            typeof(bool),
            typeof(Thing)
        })]
    internal static class ProjectileLaunchProbePatch
    {
        private static void Prefix(
            Projectile __instance,
            Thing launcher)
        {
            FiringProbe.ObserveProjectileLaunch(
                __instance,
                launcher);
        }
    }

    [HarmonyPatch]
    internal static class LaunchProjectileProbePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo native = AccessTools.Method(
                typeof(Verb_LaunchProjectileShip),
                "TryCastShot");
            if (native != null)
            {
                yield return native;
            }

            Type ceVerb = AccessTools.TypeByName(
                "CombatExtended.Compatibility.SOS2Compat.Verb_ShootShipCE");
            MethodInfo ce = ceVerb == null
                ? null
                : AccessTools.Method(ceVerb, "TryCastShot");
            if (ce != null)
            {
                yield return ce;
            }
        }

        private static void Prefix(Verb __instance)
        {
            FiringProbe.ObserveCast(__instance);
        }
    }

    [HarmonyPatch]
    internal static class BeginBurstProbePatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            MethodInfo native = AccessTools.Method(
                typeof(Building_ShipTurret),
                nameof(Building_ShipTurret.BeginBurst));
            if (native != null)
            {
                yield return native;
            }

            Type ceTurret = AccessTools.TypeByName(
                "CombatExtended.Compatibility.SOS2Compat." +
                "Building_ShipTurretCE");
            MethodInfo ce = ceTurret == null
                ? null
                : AccessTools.Method(ceTurret, "BeginBurst");
            if (ce != null)
            {
                yield return ce;
            }
        }

        private static void Prefix(object __instance)
        {
            FiringProbe.ObserveBeginBurstBefore(__instance);
        }

        private static void Postfix(object __instance)
        {
            FiringProbe.ObserveBeginBurstAfter(__instance);
        }
    }

    internal sealed class ProbeRecord
    {
        public int BeginBurstCount;
        public int CastShotCount;
        public int ProjectileLaunchCount;
        public float HeatBefore;
        public float HeatAfter;
        public float PowerBefore;
        public float PowerAfter;
        public string VerbProjectileDef = "<none>";
        public string LastProjectileDef = "<none>";
        public string LastTarget = "<none>";

        public ProbeRecord Copy()
        {
            return (ProbeRecord)MemberwiseClone();
        }
    }
}
