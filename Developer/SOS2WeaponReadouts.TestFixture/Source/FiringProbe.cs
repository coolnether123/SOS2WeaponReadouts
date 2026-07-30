using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using SaveOurShip2;
using Verse;

namespace SOS2WeaponReadouts.TestFixture
{
    public static class FiringProbe
    {
        private static readonly Dictionary<Building_ShipTurret, ProbeRecord>
            Records = new Dictionary<Building_ShipTurret, ProbeRecord>();

        public static void Reset(Building_ShipTurret turret)
        {
            Records[turret] = new ProbeRecord();
        }

        public static ProbeRecord Snapshot(Building_ShipTurret turret)
        {
            ProbeRecord record;
            return Records.TryGetValue(turret, out record)
                ? record.Copy()
                : new ProbeRecord();
        }

        public static void Forget(Building_ShipTurret turret)
        {
            if (turret != null)
            {
                Records.Remove(turret);
            }
        }

        public static void ObserveCast(
            Verb_LaunchProjectileShip verb)
        {
            Building_ShipTurret turret =
                verb.caster as Building_ShipTurret;
            ProbeRecord record;
            if (turret != null &&
                Records.TryGetValue(turret, out record))
            {
                record.CastShotCount++;
                record.VerbProjectileDef =
                    verb.Projectile?.defName ?? "<none>";
                record.LastTarget =
                    verb.CurrentTarget.Cell.ToString();
            }
        }

        public static void ObserveProjectileLaunch(
            Projectile projectile,
            Thing launcher)
        {
            Building_ShipTurret turret =
                launcher as Building_ShipTurret;
            ProbeRecord record;
            if (turret != null &&
                Records.TryGetValue(turret, out record))
            {
                record.ProjectileLaunchCount++;
                record.LastProjectileDef =
                    projectile?.def?.defName ?? "<none>";
            }
        }

        public static void ObserveBeginBurstBefore(
            Building_ShipTurret __instance)
        {
            ProbeRecord record;
            if (!Records.TryGetValue(__instance, out record))
            {
                return;
            }

            record.BeginBurstCount++;
            record.HeatBefore = __instance.heatComp.myNet.StorageUsed;
            record.PowerBefore =
                __instance.powerComp.PowerNet.CurrentStoredEnergy();
        }

        public static void ObserveBeginBurstAfter(
            Building_ShipTurret __instance)
        {
            ProbeRecord record;
            if (!Records.TryGetValue(__instance, out record))
            {
                return;
            }

            record.HeatAfter = __instance.heatComp.myNet.StorageUsed;
            record.PowerAfter =
                __instance.powerComp.PowerNet.CurrentStoredEnergy();
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
        private static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Verb_LaunchProjectileShip),
                "TryCastShot");
        }

        private static void Prefix(
            Verb_LaunchProjectileShip __instance)
        {
            FiringProbe.ObserveCast(__instance);
        }
    }

    [HarmonyPatch(
        typeof(Building_ShipTurret),
        nameof(Building_ShipTurret.BeginBurst))]
    internal static class BeginBurstProbePatch
    {
        private static void Prefix(
            Building_ShipTurret __instance)
        {
            FiringProbe.ObserveBeginBurstBefore(__instance);
        }

        private static void Postfix(
            Building_ShipTurret __instance)
        {
            FiringProbe.ObserveBeginBurstAfter(__instance);
        }
    }

    public sealed class ProbeRecord
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
