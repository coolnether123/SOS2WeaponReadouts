using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorldAgent.Extensions;
using SaveOurShip2;
using SOS2WeaponReadouts.Runtime;
using SOS2WeaponReadouts.TestFixture.Domain;
using Verse;

namespace SOS2WeaponReadouts.TestFixture
{
    public sealed class Sos2WeaponFiringFixtureExtension
        : IRimWorldAgentExtension
    {
        private const string ToolId = "sos2wr-firing-fixture";
        private const string HarmonyId =
            "CoolNether123.SOS2WeaponReadouts.TestFixture";
        private readonly List<Thing> spawned = new List<Thing>();
        private Building_ShipBridge connectedCore;
        private Building_ShipTurret connectedTurret;
        private Building_ShipTurret disconnectedTurret;
        private string lastReport = "fixture has not run";
        private FixtureStage stage;
        private int stageFrames;
        private FixtureLayout activeLayout;
        private AgentExtensionServices services;
        private FiringResult successfulResult;
        private FiringResult insufficientResult;
        private string connectedDisplayBefore;
        private float disconnectedHeatBefore;
        private float disconnectedPowerBefore;

        public Sos2WeaponFiringFixtureExtension()
        {
            new Harmony(HarmonyId).PatchAll(
                typeof(Sos2WeaponFiringFixtureExtension).Assembly);
        }

        public string Id =>
            "CoolNether123.SOS2WeaponReadouts.TestFixture";

        public int AbiVersion => AgentExtensionAbi.CurrentVersion;

        public IEnumerable<AgentToolDefinition> GetTools()
        {
            yield return new AgentToolDefinition(
                ToolId,
                "mod-fixtures",
                ToolId + " <run|status|cleanup>",
                "Construct, fire, and verify a valid SOS2 laser ship fixture.",
                true);
        }

        public string Execute(
            string toolId,
            string[] args,
            AgentToolContext context)
        {
            if (!string.Equals(
                toolId,
                ToolId,
                StringComparison.Ordinal))
            {
                return "unknown fixture tool: " + toolId;
            }

            string action = args != null && args.Length > 0
                ? args[0].ToLowerInvariant()
                : "status";
            switch (action)
            {
                case "run":
                    return Run(context);
                case "status":
                    return lastReport;
                case "cleanup":
                    return Cleanup();
                default:
                    return "usage: " + ToolId +
                        " <run|status|cleanup>";
            }
        }

        public void OnFrame(AgentFrameContext context)
        {
            if (stage == FixtureStage.Idle ||
                stage == FixtureStage.Complete)
            {
                return;
            }

            try
            {
                AdvanceFixture(context);
            }
            catch (Exception exception)
            {
                FailAndCleanup(exception);
            }
        }

        public void OnMapLoaded(AgentMapContext context)
        {
            Cleanup();
            lastReport = "map changed; fixture has not run on this map";
        }

        public void OnShutdown(AgentShutdownContext context)
        {
            Cleanup();
        }

        private string Run(AgentToolContext context)
        {
            try
            {
                Cleanup();
                Map map = Find.CurrentMap;
                if (map == null)
                {
                    return "fixture failed: no current map";
                }

                activeLayout = FixtureLayout.Find(map);
                services = context?.Services;
                SpawnFixture(map, activeLayout);
                stage = FixtureStage.WaitingForNetworks;
                stageFrames = 0;
                lastReport =
                    "fixture queued; stage=" + stage +
                    "; use status until result=PASS";
                return lastReport;
            }
            catch (Exception exception)
            {
                return FailAndCleanup(exception);
            }
        }

        private void AdvanceFixture(AgentFrameContext context)
        {
            if (!context.HasMap || Find.CurrentMap == null)
            {
                throw new InvalidOperationException(
                    "current map became unavailable during fixture execution.");
            }

            stageFrames++;
            if (stageFrames > 600)
            {
                throw new TimeoutException(
                    "fixture stage timed out: " + stage + ". " +
                    DescribeNetwork("connected", connectedTurret) + " " +
                    DescribeNetwork(
                        "disconnected",
                        disconnectedTurret));
            }

            switch (stage)
            {
                case FixtureStage.WaitingForNetworks:
                    Find.TickManager.DoSingleTick();
                    if (NetworksReady())
                    {
                        PrepareConnectedFire();
                        MoveTo(FixtureStage.WaitingForConnectedFire);
                    }
                    break;
                case FixtureStage.WaitingForConnectedFire:
                    Find.TickManager.DoSingleTick();
                    if (FiringProbe
                        .Snapshot(connectedTurret)
                        .CastShotCount > 0)
                    {
                        successfulResult =
                            CompleteConnectedFire();
                        MoveTo(FixtureStage.WaitingForBurstEnd);
                    }
                    break;
                case FixtureStage.WaitingForBurstEnd:
                    Find.TickManager.DoSingleTick();
                    if (connectedTurret.AttackVerb.state !=
                        VerbState.Bursting)
                    {
                        PrepareInsufficientPowerFire();
                        MoveTo(
                            FixtureStage
                                .WaitingForInsufficientPower);
                    }
                    break;
                case FixtureStage.WaitingForInsufficientPower:
                    Find.TickManager.DoSingleTick();
                    if (FiringProbe
                        .Snapshot(connectedTurret)
                        .BeginBurstCount > 0)
                    {
                        insufficientResult =
                            CompleteInsufficientPowerFire();
                        PrepareDisconnectedFire();
                        MoveTo(
                            FixtureStage
                                .WaitingForDisconnectedSuppression);
                    }
                    break;
                case FixtureStage.WaitingForDisconnectedSuppression:
                    Find.TickManager.DoSingleTick();
                    if (stageFrames >= 45)
                    {
                        FiringResult disconnected =
                            CompleteDisconnectedFire();
                        CompleteRun(disconnected);
                    }
                    break;
            }

            if (stage != FixtureStage.Complete)
            {
                lastReport =
                    "fixture running; stage=" + stage +
                    "; stageFrames=" + stageFrames;
            }
        }

        private void MoveTo(FixtureStage next)
        {
            stage = next;
            stageFrames = 0;
        }

        private void CompleteRun(FiringResult disconnected)
        {
            lastReport = BuildReport(
                Find.CurrentMap,
                activeLayout,
                successfulResult,
                insufficientResult,
                disconnected);
            if (services?.Artifacts != null)
            {
                AgentArtifact artifact =
                    services.Artifacts.WriteNew(
                        "sos2wr/firing-proof.txt",
                        Encoding.UTF8.GetBytes(lastReport));
                lastReport += Environment.NewLine +
                    "artifact=" + artifact.AbsolutePath +
                    Environment.NewLine +
                    "artifactSha256=" + artifact.Sha256;
            }

            stage = FixtureStage.Complete;
            stageFrames = 0;
        }

        private string FailAndCleanup(Exception exception)
        {
            string failure =
                "fixture failed: " + exception.GetType().FullName +
                ": " + exception.Message +
                Environment.NewLine + exception.StackTrace;
            string cleanup = Cleanup();
            lastReport = failure + Environment.NewLine + cleanup;
            return lastReport;
        }

        private void SpawnFixture(Map map, FixtureLayout layout)
        {
            ThingDef hullDef = RequireDef("ShipHullTile");
            foreach (IntVec3 cell in layout.HullCells)
            {
                Spawn(hullDef, cell, map);
            }

            connectedCore =
                (Building_ShipBridge)SpawnBuilding(
                    RequireDef("Ship_ComputerCore"),
                    layout.Core,
                    map);
            connectedTurret = (Building_ShipTurret)SpawnBuilding(
                RequireDef("ShipTurret_Laser"),
                layout.ConnectedTurret,
                map);
            SpawnBuilding(
                RequireDef("ShipHeatsink"),
                layout.ConnectedSink,
                map);
            SpawnBuilding(
                RequireDef("ShipCapacitorSmall"),
                layout.ConnectedCapacitor,
                map);

            disconnectedTurret =
                (Building_ShipTurret)SpawnBuilding(
                    RequireDef("ShipTurret_Laser"),
                    layout.DisconnectedTurret,
                    map);
            SpawnBuilding(
                RequireDef("ShipHeatsink"),
                layout.DisconnectedSink,
                map);
            SpawnBuilding(
                RequireDef("ShipCapacitorSmall"),
                layout.DisconnectedCapacitor,
                map);

            map.GetComponent<ShipMapComp>().RecacheFromRoots();
        }

        private void PrepareConnectedFire()
        {
            Charge(connectedTurret, 1f);
            Charge(disconnectedTurret, 1f);
            Find.TickManager.DoSingleTick();
            Charge(connectedTurret, 1f);
            Charge(disconnectedTurret, 1f);

            if (!connectedTurret.ConnectedToBridge)
            {
                throw new InvalidOperationException(
                    "connected turret did not register its AI core.");
            }

            if (disconnectedTurret.ConnectedToBridge)
            {
                throw new InvalidOperationException(
                    "disconnected turret unexpectedly registered a bridge.");
            }

            connectedTurret.holdFire = false;
            connectedTurret.burstCooldownTicksLeft = 0;
            FiringProbe.Reset(connectedTurret);
            connectedDisplayBefore =
                WeaponReadoutRuntime.AppendInspectReadout(
                    connectedTurret,
                    string.Empty);
            connectedTurret.OrderAttack(
                new LocalTargetInfo(
                    activeLayout.ConnectedTarget));
        }

        private bool NetworksReady()
        {
            return HasReadyNetwork(connectedTurret) &&
                HasReadyNetwork(disconnectedTurret) &&
                ReferenceEquals(
                    connectedTurret.heatComp.myNet,
                    connectedTurret
                        .heatComp
                        .myNet
                        .AICores
                        .FirstOrDefault()
                        ?.heatComp
                        ?.myNet);
        }

        private static bool HasReadyNetwork(
            Building_ShipTurret turret)
        {
            return turret != null &&
                turret.Spawned &&
                turret.heatComp?.myNet != null &&
                turret.heatComp.myNet.StorageCapacity >= 200f &&
                turret.powerComp?.PowerNet != null &&
                turret.powerComp.PowerNet.batteryComps.Any();
        }

        private static string DescribeNetwork(
            string name,
            Building_ShipTurret turret)
        {
            if (turret == null)
            {
                return name + "={turret:null}";
            }

            ShipHeatNet heat = turret.heatComp?.myNet;
            PowerNet power = turret.powerComp?.PowerNet;
            return name + "={" +
                "spawned:" + turret.Spawned +
                ",heat:" + (heat != null) +
                ",capacity:" +
                (heat == null
                    ? "<none>"
                    : Format(heat.StorageCapacity)) +
                ",aiCores:" +
                (heat == null ? -1 : heat.AICores.Count) +
                ",connected:" + turret.ConnectedToBridge +
                ",power:" + (power != null) +
                ",batteries:" +
                (power == null
                    ? -1
                    : power.batteryComps.Count) +
                ",stored:" +
                (power == null
                    ? "<none>"
                    : Format(power.CurrentStoredEnergy())) +
                "}";
        }

        private FiringResult CompleteConnectedFire()
        {
            ProbeRecord record =
                FiringProbe.Snapshot(connectedTurret);
            string error =
                FiringProofEvaluator.ValidateSuccessfulFire(
                    record.HeatBefore,
                    record.HeatAfter,
                    record.PowerBefore,
                    record.PowerAfter,
                    connectedTurret.HeatToFire,
                    connectedTurret.EnergyToFire,
                    record.BeginBurstCount,
                    record.CastShotCount,
                    record.ProjectileLaunchCount);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException(
                    "connected firing proof failed: " + error);
            }

            return new FiringResult(
                "connected",
                connectedTurret,
                record,
                connectedDisplayBefore,
                WeaponReadoutRuntime.AppendInspectReadout(
                    connectedTurret,
                    string.Empty),
                true,
                connectedTurret.ConnectedToBridge,
                connectedTurret.Active);
        }

        private void PrepareInsufficientPowerFire()
        {
            connectedTurret.holdFire = true;
            connectedTurret.ResetForcedTarget();
            connectedTurret.burstCooldownTicksLeft = 0;
            ChargeToEnergy(
                connectedTurret,
                connectedTurret.EnergyToFire - 1f);
            connectedTurret.holdFire = false;
            FiringProbe.Reset(connectedTurret);
            connectedTurret.OrderAttack(
                new LocalTargetInfo(
                    activeLayout.InsufficientTarget));
        }

        private FiringResult CompleteInsufficientPowerFire()
        {
            ProbeRecord record =
                FiringProbe.Snapshot(connectedTurret);
            string error =
                FiringProofEvaluator.ValidateSuppressedFire(
                    record.HeatBefore,
                    record.HeatAfter,
                    record.PowerBefore,
                    record.PowerAfter,
                    record.CastShotCount);
            if (!string.IsNullOrEmpty(error) ||
                record.BeginBurstCount < 1)
            {
                throw new InvalidOperationException(
                    "insufficient-power proof failed: " +
                    (string.IsNullOrEmpty(error)
                        ? "BeginBurst was not observed."
                        : error));
            }

            return new FiringResult(
                "insufficient-power",
                connectedTurret,
                record,
                WeaponReadoutRuntime.AppendInspectReadout(
                    connectedTurret,
                    string.Empty),
                WeaponReadoutRuntime.AppendInspectReadout(
                    connectedTurret,
                    string.Empty),
                false,
                connectedTurret.ConnectedToBridge,
                connectedTurret.Active);
        }

        private void PrepareDisconnectedFire()
        {
            Charge(disconnectedTurret, 1f);
            disconnectedTurret.holdFire = false;
            disconnectedTurret.burstCooldownTicksLeft = 0;
            FiringProbe.Reset(disconnectedTurret);
            disconnectedHeatBefore =
                disconnectedTurret.heatComp.myNet.StorageUsed;
            disconnectedPowerBefore =
                disconnectedTurret
                    .powerComp
                    .PowerNet
                    .CurrentStoredEnergy();
            disconnectedTurret.OrderAttack(
                new LocalTargetInfo(
                    activeLayout.DisconnectedTarget));
        }

        private FiringResult CompleteDisconnectedFire()
        {
            ProbeRecord record =
                FiringProbe.Snapshot(disconnectedTurret);
            record.HeatBefore = disconnectedHeatBefore;
            record.HeatAfter =
                disconnectedTurret.heatComp.myNet.StorageUsed;
            record.PowerBefore = disconnectedPowerBefore;
            record.PowerAfter =
                disconnectedTurret
                    .powerComp
                    .PowerNet
                    .CurrentStoredEnergy();
            string error =
                FiringProofEvaluator.ValidateSuppressedFire(
                    record.HeatBefore,
                    record.HeatAfter,
                    record.PowerBefore,
                    record.PowerAfter,
                    record.CastShotCount);
            if (!string.IsNullOrEmpty(error) ||
                record.BeginBurstCount != 0 ||
                disconnectedTurret.ConnectedToBridge)
            {
                throw new InvalidOperationException(
                    "disconnected proof failed: " +
                    (string.IsNullOrEmpty(error)
                        ? "disconnected turret entered BeginBurst."
                        : error));
            }

            string display =
                WeaponReadoutRuntime.AppendInspectReadout(
                    disconnectedTurret,
                    string.Empty);
            return new FiringResult(
                "disconnected",
                disconnectedTurret,
                record,
                display,
                display,
                false,
                disconnectedTurret.ConnectedToBridge,
                disconnectedTurret.Active);
        }

        private static void Charge(
            Building_ShipTurret turret,
            float percent)
        {
            foreach (CompPowerBattery battery in
                turret.powerComp.PowerNet.batteryComps)
            {
                battery.SetStoredEnergyPct(percent);
            }
        }

        private static void ChargeToEnergy(
            Building_ShipTurret turret,
            float energy)
        {
            IList<CompPowerBattery> batteries =
                turret.powerComp.PowerNet.batteryComps;
            float totalCapacity =
                batteries.Sum(battery =>
                    battery.Props.storedEnergyMax);
            float percent = totalCapacity <= 0f
                ? 0f
                : energy / totalCapacity;
            foreach (CompPowerBattery battery in batteries)
            {
                battery.SetStoredEnergyPct(percent);
            }
        }

        private Building SpawnBuilding(
            ThingDef definition,
            IntVec3 position,
            Map map)
        {
            return (Building)Spawn(definition, position, map);
        }

        private Thing Spawn(
            ThingDef definition,
            IntVec3 position,
            Map map)
        {
            Thing thing = ThingMaker.MakeThing(definition);
            if (definition.CanHaveFaction)
            {
                thing.SetFaction(Faction.OfPlayer);
            }

            Thing spawnedThing =
                GenSpawn.Spawn(thing, position, map, Rot4.North);
            spawned.Add(spawnedThing);
            CompFlickable flickable =
                (spawnedThing as ThingWithComps)
                    ?.TryGetComp<CompFlickable>();
            if (flickable != null)
            {
                flickable.SwitchIsOn = true;
            }

            return spawnedThing;
        }

        private static ThingDef RequireDef(string defName)
        {
            ThingDef definition =
                DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (definition == null)
            {
                throw new InvalidOperationException(
                    "required ThingDef is missing: " + defName);
            }

            return definition;
        }

        private string Cleanup()
        {
            FiringProbe.Forget(connectedTurret);
            FiringProbe.Forget(disconnectedTurret);
            List<string> errors = new List<string>();
            Map fixtureMap = spawned
                .Where(thing => thing != null && thing.Spawned)
                .Select(thing => thing.Map)
                .FirstOrDefault(map => map != null);
            ShipMapComp mapComponent = null;
            bool cacheRemoved = false;
            int shipsRemaining = -1;

            if (fixtureMap != null)
            {
                try
                {
                    mapComponent =
                        fixtureMap.GetComponent<ShipMapComp>();
                    int shipIndex = connectedCore?.ShipIndex ?? -1;
                    if (shipIndex < 0 &&
                        activeLayout != null)
                    {
                        shipIndex = mapComponent.ShipIndexOnVec(
                            activeLayout.Core);
                    }

                    if (shipIndex >= 0 &&
                        mapComponent.ShipsOnMap.ContainsKey(
                            shipIndex))
                    {
                        mapComponent.RemoveShipFromCache(shipIndex);
                        cacheRemoved = true;
                    }
                }
                catch (Exception exception)
                {
                    errors.Add(
                        "cache removal: " +
                        FlattenException(exception));
                }
            }

            int removed = 0;
            for (int index = spawned.Count - 1;
                index >= 0;
                index--)
            {
                Thing thing = spawned[index];
                if (thing != null && !thing.Destroyed)
                {
                    try
                    {
                        thing.Destroy(DestroyMode.Vanish);
                        removed++;
                    }
                    catch (Exception exception)
                    {
                        errors.Add(
                            "destroy " +
                            (thing.def?.defName ?? "<unknown>") +
                            ": " +
                            FlattenException(exception));
                    }
                }
            }

            if (mapComponent != null)
            {
                try
                {
                    mapComponent.RecacheFromRoots();
                    shipsRemaining =
                        mapComponent.ShipsOnMap.Count;
                    if (shipsRemaining != 0)
                    {
                        errors.Add(
                            "cache verification: expected zero ships, found " +
                            shipsRemaining + ".");
                    }
                }
                catch (Exception exception)
                {
                    errors.Add(
                        "cache verification: " +
                        FlattenException(exception));
                }
            }

            spawned.Clear();
            connectedCore = null;
            connectedTurret = null;
            disconnectedTurret = null;
            stage = FixtureStage.Idle;
            stageFrames = 0;
            activeLayout = null;
            services = null;
            successfulResult = null;
            insufficientResult = null;
            connectedDisplayBefore = null;
            lastReport =
                "fixture cleaned up; removed=" + removed +
                "; cacheRemoved=" + cacheRemoved +
                "; shipsRemaining=" + shipsRemaining +
                "; cleanupErrors=" + errors.Count;
            if (errors.Count > 0)
            {
                lastReport += Environment.NewLine +
                    string.Join(
                        Environment.NewLine,
                        errors);
            }

            return lastReport;
        }

        private static string FlattenException(
            Exception exception)
        {
            return (exception.GetType().FullName + ": " +
                exception.Message)
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private enum FixtureStage
        {
            Idle,
            WaitingForNetworks,
            WaitingForConnectedFire,
            WaitingForBurstEnd,
            WaitingForInsufficientPower,
            WaitingForDisconnectedSuppression,
            Complete
        }

        private static string BuildReport(
            Map map,
            FixtureLayout layout,
            FiringResult successful,
            FiringResult insufficient,
            FiringResult disconnected)
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("result=PASS");
            report.AppendLine("fixture=SOS2 real ground-defense firing");
            report.AppendLine("map=" + map);
            report.AppendLine(
                "shipIndex=" +
                successful.Turret
                    .heatComp
                    .myNet
                    .AICores
                    .First()
                    .ShipIndex);
            report.AppendLine("core=" + layout.Core);
            AppendResult(report, successful);
            AppendResult(report, insufficient);
            AppendResult(report, disconnected);
            return report.ToString().TrimEnd();
        }

        private static void AppendResult(
            StringBuilder report,
            FiringResult result)
        {
            string prefix = result.Name + ".";
            report.AppendLine(
                prefix + "expectedToFire=" +
                result.ExpectedToFire);
            report.AppendLine(
                prefix + "connectedToBridge=" +
                result.ConnectedToBridge);
            report.AppendLine(
                prefix + "activeAfter=" + result.Active);
            report.AppendLine(
                prefix + "beginBurstCount=" +
                result.Record.BeginBurstCount);
            report.AppendLine(
                prefix + "castShotCount=" +
                result.Record.CastShotCount);
            report.AppendLine(
                prefix + "projectileLaunchCount=" +
                result.Record.ProjectileLaunchCount);
            report.AppendLine(
                prefix + "verbProjectile=" +
                result.Record.VerbProjectileDef);
            report.AppendLine(
                prefix + "projectile=" +
                result.Record.LastProjectileDef);
            report.AppendLine(
                prefix + "target=" +
                result.Record.LastTarget);
            report.AppendLine(
                prefix + "heatBeforeHU=" +
                Format(result.Record.HeatBefore));
            report.AppendLine(
                prefix + "heatAfterHU=" +
                Format(result.Record.HeatAfter));
            report.AppendLine(
                prefix + "heatDeltaHU=" +
                Format(
                    result.Record.HeatAfter -
                    result.Record.HeatBefore));
            report.AppendLine(
                prefix + "powerBeforeWd=" +
                Format(result.Record.PowerBefore));
            report.AppendLine(
                prefix + "powerAfterWd=" +
                Format(result.Record.PowerAfter));
            report.AppendLine(
                prefix + "powerDeltaWd=" +
                Format(
                    result.Record.PowerBefore -
                    result.Record.PowerAfter));
            report.AppendLine(
                prefix + "displayBefore=" +
                Flatten(result.DisplayBefore));
            report.AppendLine(
                prefix + "displayAfter=" +
                Flatten(result.DisplayAfter));
        }

        private static string Flatten(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", " | ");
        }

        private static string Format(float value)
        {
            return value.ToString(
                "0.###",
                CultureInfo.InvariantCulture);
        }

        private sealed class FiringResult
        {
            public FiringResult(
                string name,
                Building_ShipTurret turret,
                ProbeRecord record,
                string displayBefore,
                string displayAfter,
                bool expectedToFire,
                bool connectedToBridge,
                bool active)
            {
                Name = name;
                Turret = turret;
                Record = record;
                DisplayBefore = displayBefore;
                DisplayAfter = displayAfter;
                ExpectedToFire = expectedToFire;
                ConnectedToBridge = connectedToBridge;
                Active = active;
            }

            public string Name { get; }
            public ProbeRecord Record { get; }
            public string DisplayBefore { get; }
            public string DisplayAfter { get; }
            public bool ExpectedToFire { get; }
            public bool ConnectedToBridge { get; }
            public bool Active { get; }
            public Building_ShipTurret Turret { get; }
        }

        private sealed class FixtureLayout
        {
            private FixtureLayout(IntVec3 anchor)
            {
                Core = anchor + new IntVec3(2, 0, 2);
                ConnectedTurret =
                    anchor + new IntVec3(4, 0, 2);
                ConnectedSink =
                    anchor + new IntVec3(6, 0, 2);
                ConnectedCapacitor =
                    anchor + new IntVec3(4, 0, 4);
                DisconnectedTurret =
                    anchor + new IntVec3(4, 0, 10);
                DisconnectedSink =
                    anchor + new IntVec3(6, 0, 10);
                DisconnectedCapacitor =
                    anchor + new IntVec3(4, 0, 12);
                ConnectedTarget =
                    ConnectedTurret + new IntVec3(40, 0, 0);
                InsufficientTarget =
                    ConnectedTurret + new IntVec3(41, 0, 0);
                DisconnectedTarget =
                    DisconnectedTurret + new IntVec3(40, 0, 0);
                HullCells = CellRect
                    .FromLimits(
                        anchor.x,
                        anchor.z,
                        anchor.x + 9,
                        anchor.z + 14)
                    .Cells
                    .ToArray();
            }

            public IntVec3 Core { get; }
            public IntVec3 ConnectedTurret { get; }
            public IntVec3 ConnectedSink { get; }
            public IntVec3 ConnectedCapacitor { get; }
            public IntVec3 DisconnectedTurret { get; }
            public IntVec3 DisconnectedSink { get; }
            public IntVec3 DisconnectedCapacitor { get; }
            public IntVec3 ConnectedTarget { get; }
            public IntVec3 InsufficientTarget { get; }
            public IntVec3 DisconnectedTarget { get; }
            public IReadOnlyList<IntVec3> HullCells { get; }

            public static FixtureLayout Find(Map map)
            {
                for (int z = 15; z <= map.Size.z - 30; z += 3)
                {
                    for (int x = 15;
                        x <= map.Size.x - 60;
                        x += 3)
                    {
                        FixtureLayout candidate =
                            new FixtureLayout(
                                new IntVec3(x, 0, z));
                        if (candidate.IsClear(map))
                        {
                            return candidate;
                        }
                    }
                }

                throw new InvalidOperationException(
                    "no clear footprint was found for the SOS2 fixture.");
            }

            private bool IsClear(Map map)
            {
                if (!ConnectedTarget.InBounds(map) ||
                    !InsufficientTarget.InBounds(map) ||
                    !DisconnectedTarget.InBounds(map))
                {
                    return false;
                }

                return HullCells.All(cell =>
                    cell.InBounds(map) &&
                    cell.GetEdifice(map) == null &&
                    cell.GetThingList(map).All(
                        thing =>
                            thing.def.category !=
                                ThingCategory.Pawn &&
                            thing.def.category !=
                                ThingCategory.Building));
            }
        }
    }
}
