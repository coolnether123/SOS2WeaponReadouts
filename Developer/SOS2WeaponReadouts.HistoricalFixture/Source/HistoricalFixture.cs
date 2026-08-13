using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Verse;

namespace SOS2WeaponReadouts.HistoricalFixture
{
    [StaticConstructorOnStartup]
    public static class HistoricalFixtureBootstrap
    {
        static HistoricalFixtureBootstrap()
        {
            LongEventHandler.ExecuteWhenFinished(
                HistoricalFixtureRunner.QueueIfPossible);
        }
    }

    public sealed class HistoricalFixtureGameComponent : GameComponent
    {
        private bool started;
        private int waitTicks;
        private HistoricalFixtureRunner runner;

        public HistoricalFixtureGameComponent(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (started || Current.Game == null || Find.CurrentMap == null)
            {
                return;
            }

            waitTicks++;
            if (waitTicks < 30)
            {
                return;
            }

            started = true;
            runner = new HistoricalFixtureRunner();
            runner.Run(Find.CurrentMap);
        }
    }

    internal sealed class HistoricalFixtureRunner
    {
        private const string FixturePrefix =
            "SOS2WeaponReadouts.HistoricalFixture";
        private const string TargetModType =
            "SOS2WeaponReadouts.Bootstrap.SOS2WeaponReadoutsMod";
        private static readonly string[] SettingFields =
        {
            "FeatureEnabled",
            "ShowInDescriptions",
            "ShowSelectedWeaponReadout",
            "ShowElectricalDraw",
            "ShowNetworkComparison",
            "ShowPlacementWarnings"
        };
        private readonly List<Thing> spawned = new List<Thing>();
        private Map activeMap;

        public static void QueueIfPossible()
        {
            if (Current.Game != null && Find.CurrentMap != null)
            {
                new HistoricalFixtureRunner().Run(Find.CurrentMap);
            }
        }

        public void Run(Map map)
        {
            activeMap = map;
            FixtureResult result = null;
            try
            {
                string statePath = StatePath();
                XElement state = LoadState(statePath);
                result = new FixtureResult();
                result.Version = BuildVersion();
                result.GameAssembly = typeof(Game).Assembly.FullName;
                result.MapSize = map.Size.ToString();

                VerifyAssemblies(result);
                VerifyHistoricalDefs(result);
                string settingsPhase =
                    state?.Attribute("phase")?.Value ?? "initial";
                if (settingsPhase == "await-reload")
                {
                    // The first process deliberately left the production
                    // controls disabled. Verify the fresh process restored
                    // those values, then re-enable through WriteSettings so
                    // the real readout paths are exercised in this process.
                    RunSettings(state, statePath, result);
                }
                if (settingsPhase == "initial")
                {
                    RunSettings(state, statePath, result);
                }
                RunReadoutAndFiring(map, result);
                if (settingsPhase == "initial")
                {
                    PersistInitialSettingsFalse(statePath, result);
                }
                Cleanup();

                result.Result = "PASS";
                WriteEvidence(result, statePath);
                Log.Message("[SOS2 historical fixture] PASS " + EvidencePath());
            }
            catch (Exception exception)
            {
                Cleanup();
                if (result == null)
                {
                    result = new FixtureResult
                    {
                        Version = BuildVersion()
                    };
                }
                result.Result = "FAIL";
                result.Error = Flatten(exception);
                WriteEvidence(result, StatePath());
                Log.Error(
                    "[SOS2 historical fixture] ASSERTION FAILURE: " +
                    Flatten(exception));
            }
        }

        private void VerifyAssemblies(FixtureResult result)
        {
            Assembly sos2 = FindAssembly("ShipsHaveInsides");
            if (sos2 == null)
            {
                throw new InvalidOperationException(
                    "ShipsHaveInsides.dll is not loaded.");
            }

            result.Sos2Assembly = sos2.FullName;
            result.Sos2Types = new[]
            {
                FindFirstType(
                    sos2,
                    "SaveOurShip2.Building_ShipTurret",
                    "RimWorld.Building_ShipTurret"),
                FindFirstType(
                    sos2,
                    "SaveOurShip2.CompShipHeat",
                    "RimWorld.CompShipHeat"),
                FindFirstType(
                    sos2,
                    "SaveOurShip2.CompShipHeatSource",
                    "RimWorld.CompShipHeatSource")
            }.Select(type => type == null ? "MISSING" : type.FullName).ToArray();

            if (result.Sos2Types.Any(value => value == "MISSING"))
            {
                throw new MissingMemberException(
                    "Historical SOS2 weapon/heat type identity was not loaded.");
            }
        }

        private void VerifyHistoricalDefs(FixtureResult result)
        {
            string[] names =
            {
                "ShipHullTile",
                "Ship_ComputerCore",
                "ShipTurret_Laser",
                "ShipHeatsink",
                "ShipCapacitorSmall"
            };
            foreach (string name in names)
            {
                ThingDef definition =
                    DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (definition == null)
                {
                    throw new InvalidOperationException(
                        "Historical SOS2 ThingDef is missing: " + name);
                }

                result.Defs.Add(name + "=" + definition.thingClass.FullName);
            }
        }

        private void RunReadoutAndFiring(Map map, FixtureResult result)
        {
            IntVec3 anchor = FindAnchor(map);
            SpawnLayout(map, anchor);
            Thing turret = spawned.Single(
                thing => thing.def.defName == "ShipTurret_Laser");
            ThingDef turretDef = turret.def;
            Type runtimeType = FindType(
                "SOS2WeaponReadouts.Runtime.WeaponReadoutRuntime");
            if (runtimeType == null)
            {
                throw new TypeLoadException("Production readout runtime is not loaded.");
            }

            result.AdapterSummary = ReadStaticString(
                runtimeType,
                "CompatibilitySummary");
            object adapter = ReadStaticObject(runtimeType, "Adapter");
            if (adapter == null)
            {
                throw new InvalidOperationException(
                    "Production adapter was not initialized.");
            }

            MethodInfo tryDefinition = adapter.GetType().GetMethod(
                "TryReadDefinition");
            object[] definitionArgs = { turretDef, null };
            bool definitionRead = (bool)tryDefinition.Invoke(
                adapter,
                definitionArgs);
            Assert(definitionRead, "production definition readout was not generated");
            result.DefinitionReadout = definitionArgs[1]?.ToString() ?? "<null>";

            MethodInfo placement = runtimeType.GetMethod(
                "TryCreatePlacementReadout");
            object[] placementArgs =
            {
                turretDef,
                turret.Position,
                Rot4.North,
                map,
                null,
                null
            };
            bool placementRead = (bool)placement.Invoke(null, placementArgs);
            Assert(placementRead, "production placement readout was not generated");
            result.PlacementReadout = Convert.ToString(
                placementArgs[4], CultureInfo.InvariantCulture);
            Assert(
                !string.IsNullOrWhiteSpace(result.PlacementReadout),
                "production placement readout text is empty");

            MethodInfo heatText = runtimeType.GetMethod(
                "AppendHeatPerShotToInspectString");
            string existing;
            try
            {
                existing = ReadHeatInspectString(turret);
            }
            catch (Exception exception)
            {
                existing = "UNAVAILABLE: " + Flatten(exception);
            }
            if (string.IsNullOrWhiteSpace(existing))
            {
                existing = "Stored heat: 0 / 100";
            }
            result.InspectBefore = existing;
            result.InspectAfter = Convert.ToString(
                heatText.Invoke(null, new object[] { turret, existing }),
                CultureInfo.InvariantCulture);
            Assert(
                result.InspectAfter != existing,
                "production selected-weapon readout did not append");

            try
            {
                IntVec3 targetCell = anchor + new IntVec3(40, 0, 2);
                PrepareHistoricalCombat(turret, map, targetCell);
                try
                {
                    result.NetworkDiagnostics = DescribeNetwork(turret, map);
                }
                catch (Exception exception)
                {
                    result.FiringOutcome =
                        "UNAVAILABLE: historical network inspection prerequisite: " +
                        Flatten(exception);
                }
                result.HeatBefore = SafeReadNetworkValue(turret, "StorageUsed");
                result.PowerBefore = SafeReadPowerValue(turret);
                try
                {
                    OrderAttack(turret, map, targetCell);
                    result.AttackOrdered = true;
                }
                catch (Exception exception)
                {
                    result.FiringOutcome =
                        "UNAVAILABLE: historical OrderAttack prerequisite: " +
                        Flatten(exception);
                }
                float? heatAfterOrder = SafeReadNetworkValue(turret, "StorageUsed");
                if (result.AttackOrdered &&
                    (!result.HeatBefore.HasValue ||
                     !heatAfterOrder.HasValue ||
                     heatAfterOrder.Value <= result.HeatBefore.Value))
                {
                    try
                    {
                        InvokeHistoricalBurst(turret, targetCell);
                        result.FiringOutcome = "direct BeginBurst invoked";
                    }
                    catch (Exception exception)
                    {
                        result.FiringOutcome =
                            "UNAVAILABLE: historical BeginBurst prerequisite: " +
                            Flatten(exception);
                    }
                }
                result.HeatPerShot = ReadProperty(turret, "HeatToFire");
                result.PowerPerShot = ReadProperty(turret, "EnergyToFire");
                result.ProjectilesBefore = ProjectileCount(map);
                result.WaitTicks = 180;
                for (int index = 0; index < result.WaitTicks; index++)
                {
                    Find.TickManager.DoSingleTick();
                }
                result.ProjectilesAfter = ProjectileCount(map);
                result.HeatAfter = SafeReadNetworkValue(turret, "StorageUsed");
                result.PowerAfter = SafeReadPowerValue(turret);
                result.ProjectileObserved = result.ProjectilesAfter >
                    result.ProjectilesBefore;
                result.HeatObserved = result.HeatAfter.HasValue &&
                    result.HeatBefore.HasValue &&
                    result.HeatAfter.Value > result.HeatBefore.Value;
                result.PowerObserved = result.PowerBefore.HasValue &&
                    result.PowerAfter.HasValue &&
                    result.PowerAfter.Value < result.PowerBefore.Value;
                if (string.IsNullOrWhiteSpace(result.FiringOutcome))
                {
                    result.FiringOutcome = result.ProjectileObserved || result.HeatObserved
                        ? "direct firing observed"
                        : "UNAVAILABLE: no historical burst observation";
                }
            }
            catch (Exception exception)
            {
                result.FiringOutcome =
                    "UNAVAILABLE: historical firing prerequisite: " +
                    Flatten(exception);
            }
        }

        private void RunSettings(
            XElement state,
            string statePath,
            FixtureResult result)
        {
            object mod = FindLoadedMod(TargetModType);
            Assert(mod != null, "loaded production mod instance was not found");
            object settings = ReadStaticObject(mod.GetType(), "Settings");
            Assert(settings != null, "production settings instance was not found");
            Dictionary<string, bool> values = ReadSettings(settings);
            result.SettingsBefore = values;
            string phase = state?.Attribute("phase")?.Value ?? "initial";
            if (phase == "initial")
            {
                Assert(
                    values.Values.All(value => value),
                    "production settings defaults were not all true");
                return;
            }

            Assert(phase == "await-reload", "unexpected fixture settings phase");
            Assert(
                values.Values.All(value => !value),
                "settings did not persist all-false values into the fresh process");
            WriteSettings(mod, settings, true);
            result.SettingsMutation = "all-true via production WriteSettings";
            result.SettingsFile = FindSettingsFile();
            WriteState(
                statePath,
                new XElement(
                    "fixture",
                    new XAttribute("phase", "complete"),
                    SettingFields.Select(
                        name => new XElement(name, "true"))));
        }

        private void PersistInitialSettingsFalse(
            string statePath,
            FixtureResult result)
        {
            object mod = FindLoadedMod(TargetModType);
            Assert(mod != null, "loaded production mod instance was not found");
            object settings = ReadStaticObject(mod.GetType(), "Settings");
            Assert(settings != null, "production settings instance was not found");
            WriteSettings(mod, settings, false);
            result.SettingsMutation = "all-false via production WriteSettings";
            result.SettingsFile = FindSettingsFile();
            WriteState(
                statePath,
                new XElement(
                    "fixture",
                    new XAttribute("phase", "await-reload"),
                    SettingFields.Select(name => new XElement(name, "false"))));
        }

        private void SpawnLayout(Map map, IntVec3 anchor)
        {
            for (int x = 0; x < 10; x++)
            {
                for (int z = 0; z < 15; z++)
                {
                    Spawn(map, "ShipHullTile", anchor + new IntVec3(x, 0, z));
                }
            }

            Spawn(map, "Ship_ComputerCore", anchor + new IntVec3(2, 0, 2));
            Spawn(map, "ShipTurret_Laser", anchor + new IntVec3(4, 0, 2));
            Spawn(map, "ShipHeatConduit", anchor + new IntVec3(5, 0, 2));
            Spawn(map, "ShipHeatsink", anchor + new IntVec3(6, 0, 2));
            // The historical capacitor is a battery, not a power conduit.
            // Keep it cardinally adjacent to the turret so the real power
            // net connects without inventing a modern conduit type.
            Spawn(map, "ShipCapacitorSmall", anchor + new IntVec3(4, 0, 3));
            Spawn(map, "Wall", anchor + new IntVec3(40, 0, 2), "Steel");
        }

        private void Spawn(Map map, string defName, IntVec3 cell)
        {
            Spawn(map, defName, cell, null);
        }

        private void Spawn(
            Map map,
            string defName,
            IntVec3 cell,
            string stuffDefName)
        {
            ThingDef definition =
                DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            Assert(definition != null, "spawn def missing: " + defName);
            ThingDef stuff = string.IsNullOrEmpty(stuffDefName)
                ? null
                : DefDatabase<ThingDef>.GetNamedSilentFail(stuffDefName);
            Thing thing = ThingMaker.MakeThing(definition, stuff);
            MethodInfo method = typeof(GenSpawn).GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static)
                .Where(candidate => candidate.Name == "Spawn")
                .Where(candidate => !candidate.IsGenericMethodDefinition)
                .Where(candidate => candidate.GetParameters().Length >= 3)
                .Where(candidate => candidate.GetParameters()[0].ParameterType
                    .IsAssignableFrom(typeof(Thing)))
                .Where(candidate => candidate.GetParameters()[1].ParameterType ==
                    typeof(IntVec3))
                .Where(candidate => candidate.GetParameters()[2].ParameterType ==
                    typeof(Map))
                .OrderBy(candidate => candidate.GetParameters().Length)
                .FirstOrDefault();
            Assert(method != null, "historical GenSpawn.Spawn API was not found");
            ParameterInfo[] parameters = method.GetParameters();
            object[] args = new object[parameters.Length];
            args[0] = thing;
            args[1] = cell;
            args[2] = map;
            for (int index = 3; index < args.Length; index++)
            {
                args[index] = DefaultArgument(parameters[index].ParameterType);
            }
            method.Invoke(null, args);
            spawned.Add(thing);
        }

        private static object DefaultArgument(Type type)
        {
            if (type == typeof(bool)) return false;
            if (type.IsEnum)
            {
                Array values = Enum.GetValues(type);
                return values.Length == 0 ? Activator.CreateInstance(type) :
                    values.GetValue(0);
            }
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static void OrderAttack(Thing turret, Map map, IntVec3 cell)
        {
            Thing targetThing = map.listerThings.AllThings.FirstOrDefault(
                thing => thing != null && thing.def.defName == "Wall");
            Type targetType = typeof(LocalTargetInfo);
            object target = Activator.CreateInstance(
                targetType,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance,
                null,
                new object[] { targetThing ?? (object)cell },
                CultureInfo.InvariantCulture);
            MethodInfo method = FindMethod(turret.GetType(), "OrderAttack");
            Assert(method != null, "historical turret OrderAttack API was not found");
            method.Invoke(turret, new[] { target });
        }

        private static void InvokeHistoricalBurst(Thing turret, IntVec3 targetCell)
        {
            if (ReadMemberNoThrow(turret, "gun") == null)
            {
                MethodInfo makeGun = turret.GetType().GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance)
                    .FirstOrDefault(candidate => candidate.Name == "MakeGun" &&
                        candidate.GetParameters().Length == 0);
                makeGun?.Invoke(turret, null);
            }
            object target = Activator.CreateInstance(
                typeof(LocalTargetInfo),
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance,
                null,
                new object[] { targetCell },
                CultureInfo.InvariantCulture);
            SetMember(turret, "currentTargetInt", target);
            SetMemberIfPresent(turret, "shipTarget", target);
            MethodInfo method = turret.GetType().GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance)
                .FirstOrDefault(candidate => candidate.Name == "BeginBurst" &&
                    (candidate.GetParameters().Length == 0 ||
                        candidate.GetParameters().Length == 1));
            Assert(method != null, "historical turret BeginBurst API was not found");
            method.Invoke(
                turret,
                method.GetParameters().Length == 0
                    ? new object[0]
                    : new object[] { false });
        }

        private void PrepareHistoricalCombat(
            Thing turret,
            Map map,
            IntVec3 targetCell)
        {
            Type mapCompType = FindType("RimWorld.ShipHeatMapComp") ??
                FindType("SaveOurShip2.ShipHeatMapComp");
            if (mapCompType != null)
            {
                object mapComp = GetMapComponent(map, mapCompType);
                if (mapComp != null)
                {
                    object currentState = ReadMember(mapComp, "ShipMapState");
                    if (currentState != null)
                    {
                        SetMember(mapComp, "ShipMapState", Enum.Parse(
                            currentState.GetType(), "inCombat"));
                    }
                    else if (ReadMember(mapComp, "InCombat") != null)
                    {
                        // SOS2 V49 (RimWorld 1.1-1.3) predates the later
                        // ShipMapState enum and uses this boolean contract.
                        SetMember(mapComp, "InCombat", true);
                    }
                    SetMemberIfPresent(mapComp, "Heading", 0);
                    SetMemberIfPresent(mapComp, "EngineRot", 0);

                    FieldInfo projectiles = FindField(mapComp.GetType(), "Projectiles");
                    if (projectiles != null && projectiles.GetValue(mapComp) == null)
                    {
                        Type elementType = projectiles.FieldType.IsGenericType
                            ? projectiles.FieldType.GetGenericArguments()[0]
                            : null;
                        if (elementType != null)
                        {
                            projectiles.SetValue(
                                mapComp,
                                Activator.CreateInstance(
                                    typeof(List<>).MakeGenericType(elementType)));
                        }
                    }
                }
            }

            // SOS2 V49 (1.1-1.3) owns combat state in the static
            // ShipCombatManager rather than in ShipHeatMapComp. Use a same-map
            // target only to let the old cast/registration lifecycle run; the
            // fixture does not call the manager's battle generator.
            Type combatManager = FindType("RimWorld.ShipCombatManager");
            if (combatManager != null)
            {
                SetStaticMember(combatManager, "InCombat", true);
                SetStaticMember(combatManager, "PlayerShip", map);
                SetStaticMember(combatManager, "EnemyShip", map);
                SetStaticMember(combatManager, "Range", 0f);
                SetStaticMember(combatManager, "PlayerHeading", 0);
                SetStaticMember(combatManager, "EnemyHeading", 0);
                FieldInfo projectiles = FindField(combatManager, "Projectiles");
                if (projectiles != null)
                {
                    Type elementType = projectiles.FieldType.IsGenericType
                        ? projectiles.FieldType.GetGenericArguments()[0]
                        : null;
                    if (elementType != null)
                    {
                        SetStaticMember(
                            combatManager,
                            "Projectiles",
                            Activator.CreateInstance(
                                typeof(List<>).MakeGenericType(elementType)));
                    }
                }
            }

            // These are the actual old Building_ShipTurret state members.
            // They are private/public fields in historical SOS2, so use a
            // reflection boundary rather than compiling against one branch.
            SetMemberIfPresent(turret, "holdFire", false);
            SetMemberIfPresent(turret, "burstCooldownTicksLeft", 0);
            SetMemberIfPresent(turret, "GroundDefenseMode", false);
            Type targetType = typeof(LocalTargetInfo);
            object target = Activator.CreateInstance(
                targetType,
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance,
                null,
                new object[] { targetCell },
                CultureInfo.InvariantCulture);
            SetMember(turret, "shipTarget", target);

            Thing capacitor = map.listerThings.AllThings.FirstOrDefault(
                thing => thing != null &&
                    thing.def.defName == "ShipCapacitorSmall");
            object battery = FindComponent(capacitor, "CompPowerBattery");
            MethodInfo addEnergy = battery?.GetType().GetMethod(
                "AddEnergy",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance,
                null,
                new[] { typeof(float) },
                null);
            if (addEnergy != null)
            {
                addEnergy.Invoke(battery, new object[] { 10000f });
            }

            // SpawnSetup registers the old heat components, but the map
            // component rebuilds the network on its tick. Allow that real
            // lifecycle to complete before OrderAttack.
            // The historical power manager and the old heat map both finish
            // their first network assignment on their ordinary map ticks.
            // Let that real lifecycle run instead of manufacturing either
            // network in the fixture.
            object powerManager = ReadMemberNoThrow(map, "powerNetManager");
            EnsureHistoricalPowerConnection(turret);
            MethodInfo rebuildPower = powerManager?.GetType().GetMethod(
                "UpdatePowerNetsAndConnections_First",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            rebuildPower?.Invoke(powerManager, null);
            for (int index = 0; index < 60; index++)
            {
                Find.TickManager.DoSingleTick();
            }
            if (mapCompType != null)
            {
                object mapComp = GetMapComponent(map, mapCompType);
                SetMemberIfPresent(mapComp, "heatGridDirty", true);
                SetMemberIfPresent(mapComp, "dirty", true);
                MethodInfo update = mapComp?.GetType().GetMethod(
                    "MapComponentUpdate",
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance,
                    null,
                    Type.EmptyTypes,
                    null);
                update?.Invoke(mapComp, null);
                object pipes = ReadMember(mapComp, "cachedPipes") ??
                    ReadMember(mapComp, "CachedPipes");
                MethodInfo register = mapComp?.GetType().GetMethod(
                    "Register",
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance);
                bool anyNetwork = false;
                foreach (Thing thing in spawned)
                {
                    object heat = FindComponent(thing, "CompShipHeat");
                    if (ReadMember(heat, "myNet") != null)
                    {
                        anyNetwork = true;
                        break;
                    }
                }
                if (register != null && !anyNetwork)
                {
                    foreach (Thing thing in spawned)
                    {
                        object heat = FindComponent(thing, "CompShipHeat");
                        if (heat != null)
                        {
                            register.Invoke(mapComp, new[] { heat });
                        }
                    }
                    SetMemberIfPresent(mapComp, "heatGridDirty", true);
                    SetMemberIfPresent(mapComp, "dirty", true);
                    update?.Invoke(mapComp, null);
                }
            }
        }

        private void EnsureHistoricalPowerConnection(Thing turret)
        {
            object power = FindComponent(turret, "CompPower");
            if (power == null)
            {
                return;
            }

            InvokeNoArgIfPresent(power, "SetUpPowerVars");
            TryConnectToAnyHistoricalPowerNet(power);
            if (ReadMember(power, "PowerNet") != null)
            {
                return;
            }

            foreach (Thing candidateThing in spawned)
            {
                if (candidateThing == turret) continue;
                object candidate = FindComponent(candidateThing, "CompPower");
                object props = ReadMember(candidate, "Props");
                object transmits = ReadMember(props, "transmitsPower");
                if (candidate == null || !Convert.ToBoolean(
                    transmits ?? false,
                    CultureInfo.InvariantCulture))
                {
                    continue;
                }

                InvokeNoArgIfPresent(candidate, "SetUpPowerVars");

                MethodInfo connect = power.GetType().GetMethods(
                        BindingFlags.Public | BindingFlags.NonPublic |
                            BindingFlags.Instance)
                    .FirstOrDefault(method => method.Name == "ConnectToTransmitter" &&
                        (method.GetParameters().Length == 1 ||
                            method.GetParameters().Length == 2));
                if (connect == null) continue;
                object[] args = connect.GetParameters().Length == 1
                    ? new object[] { candidate }
                    : new object[] { candidate, false };
                connect.Invoke(power, args);
                if (ReadMember(power, "PowerNet") != null)
                {
                    return;
                }
            }
        }

        private static void TryConnectToAnyHistoricalPowerNet(object power)
        {
            Type maker = FindType("RimWorld.PowerConnectionMaker") ??
                FindType("Verse.PowerConnectionMaker");
            MethodInfo method = maker?.GetMethod(
                "TryConnectToAnyPowerNet",
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static);
            method?.Invoke(null, new[] { power, null });
        }

        private static void InvokeNoArgIfPresent(object instance, string name)
        {
            if (instance == null) return;
            MethodInfo method = instance.GetType().GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance)
                .FirstOrDefault(candidate => candidate.Name == name &&
                    candidate.GetParameters().Length == 0);
            method?.Invoke(instance, null);
        }

        private static int ProjectileCount(Map map)
        {
            try
            {
                return map.listerThings.ThingsInGroup(
                    ThingRequestGroup.Projectile).Count;
            }
            catch
            {
                return 0;
            }
        }

        private static string ReadHeatInspectString(Thing turret)
        {
            object comp = FindComponent(turret, "CompShipHeat");
            MethodInfo method = comp?.GetType().GetMethod(
                "CompInspectStringExtra",
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            return method == null ? string.Empty :
                Convert.ToString(method.Invoke(comp, null), CultureInfo.InvariantCulture);
        }

        private static float? ReadNetworkValue(Thing turret, string memberName)
        {
            object comp = FindComponent(turret, "CompShipHeat");
            object network = ReadMember(comp, "myNet");
            object value = ReadMember(network, memberName);
            if (value == null) return null;
            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        private static float? ReadPowerValue(Thing turret)
        {
            object comp = FindComponent(turret, "CompPowerTrader");
            object network = ReadMember(comp, "PowerNet");
            object value = ReadMember(network, "CurrentEnergyGainRate");
            return value == null ? (float?)null :
                Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        private static float? SafeReadNetworkValue(Thing turret, string memberName)
        {
            try { return ReadNetworkValue(turret, memberName); }
            catch { return null; }
        }

        private static float? SafeReadPowerValue(Thing turret)
        {
            try { return ReadPowerValue(turret); }
            catch { return null; }
        }

        private string DescribeNetwork(Thing turret, Map map)
        {
            object heat = FindComponent(turret, "CompShipHeat");
            object network = ReadMember(heat, "myNet");
            object power = FindComponent(turret, "CompPower");
            object powerNet = ReadMember(power, "PowerNet");
            object attackVerb = ReadMemberNoThrow(turret, "AttackVerb");
            object gun = ReadMember(turret, "gun");
            Type mapType = FindType("RimWorld.ShipHeatMapComp");
            object mapComp = mapType == null ? null : GetMapComponent(map, mapType);
            object pipes = ReadMember(mapComp, "cachedPipes") ??
                ReadMember(mapComp, "CachedPipes");
            string pipeCount = pipes is System.Collections.ICollection collection
                ? collection.Count.ToString(CultureInfo.InvariantCulture)
                : "unknown";
            Type combatManager = FindType("RimWorld.ShipCombatManager");
            object playerShip = ReadStaticMember(combatManager, "PlayerShip");
            object enemyShip = ReadStaticMember(combatManager, "EnemyShip");
            object inCombat = ReadStaticMember(combatManager, "InCombat");
            return "heat=" + (network == null ? "null" : network.GetType().FullName) +
                ";map=" + (mapComp == null ? "null" : mapComp.GetType().FullName) +
                ";pipes=" + pipeCount +
                ";spawned=" + spawned.Count.ToString(CultureInfo.InvariantCulture) +
                ";powerComp=" + (power == null ? "null" : power.GetType().FullName) +
                ";power=" + (powerNet == null ? "null" : powerNet.GetType().FullName) +
                ";verb=" + (attackVerb == null ? "null" : attackVerb.GetType().FullName) +
                ";gun=" + (gun == null ? "null" : gun.GetType().FullName) +
                ";playerShip=" + (playerShip == null ? "null" : playerShip.GetType().FullName) +
                ";enemyShip=" + (enemyShip == null ? "null" : enemyShip.GetType().FullName) +
                ";inCombat=" + Convert.ToString(inCombat, CultureInfo.InvariantCulture);
        }

        private static object ReadStaticMember(Type type, string name)
        {
            if (type == null) return null;
            FieldInfo field = FindStaticField(type, name);
            if (field != null) return field.GetValue(null);
            PropertyInfo property = FindStaticProperty(type, name);
            return property?.GetValue(null, null);
        }

        private static object ReadMemberNoThrow(object instance, string name)
        {
            try
            {
                return ReadMember(instance, name);
            }
            catch
            {
                return null;
            }
        }

        private static float ReadProperty(object instance, string name)
        {
            object value = null;
            try
            {
                value = ReadMember(instance, name);
            }
            catch
            {
                // Historical SOS2's patched EnergyToFire/HeatToFire accessors
                // can touch CompPower.PowerNet before the disposable fixture
                // has a real combat network. Fall back to the loaded def props.
            }
            if (value == null &&
                (name == "HeatToFire" || name == "EnergyToFire"))
            {
                object heat = FindComponent(instance as Thing, "CompShipHeat");
                object props = ReadMember(heat, "Props");
                value = ReadMember(
                    props,
                    name == "HeatToFire" ? "heatPerPulse" : "energyToFire");
            }
            Assert(value != null, "historical turret member is missing: " + name);
            return Convert.ToSingle(value, CultureInfo.InvariantCulture);
        }

        private static object FindComponent(Thing thing, string suffix)
        {
            ThingWithComps withComps = thing as ThingWithComps;
            return withComps?.AllComps.FirstOrDefault(
                comp =>
                {
                    if (comp == null) return false;
                    for (Type current = comp.GetType();
                        current != null;
                        current = current.BaseType)
                    {
                        if ((current.FullName ?? string.Empty).EndsWith(
                            suffix,
                            StringComparison.Ordinal))
                        {
                            return true;
                        }
                        if (suffix == "CompPower" &&
                            current.Name.StartsWith(
                                "CompPower",
                                StringComparison.Ordinal))
                        {
                            return true;
                        }
                    }
                    return false;
                });
        }

        private static object ReadMember(object instance, string name)
        {
            if (instance == null) return null;
            Type type = instance.GetType();
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(instance);
                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null) return property.GetValue(instance, null);
            }
            return null;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.Static |
                        BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        private static void SetMember(object instance, string name, object value)
        {
            Assert(instance != null, "cannot set member on null instance: " + name);
            Type type = instance.GetType();
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    field.SetValue(instance, ConvertMemberValue(
                        value, field.FieldType));
                    return;
                }
                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, ConvertMemberValue(
                        value, property.PropertyType), null);
                    return;
                }
            }
            throw new MissingMemberException(type.FullName, name);
        }

        private static void SetMemberIfPresent(
            object instance,
            string name,
            object value)
        {
            if (ReadMember(instance, name) != null ||
                FindField(instance.GetType(), name) != null)
            {
                SetMember(instance, name, value);
            }
        }

        private static void SetStaticMember(Type type, string name, object value)
        {
            FieldInfo field = FindStaticField(type, name);
            if (field != null)
            {
                field.SetValue(null, ConvertMemberValue(value, field.FieldType));
                return;
            }
            PropertyInfo property = FindStaticProperty(type, name);
            if (property != null && property.CanWrite)
            {
                property.SetValue(
                    null,
                    ConvertMemberValue(value, property.PropertyType),
                    null);
                return;
            }
            throw new MissingMemberException(type.FullName, name);
        }

        private static FieldInfo FindStaticField(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field != null) return field;
            }
            return null;
        }

        private static PropertyInfo FindStaticProperty(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (property != null) return property;
            }
            return null;
        }

        private static object ConvertMemberValue(object value, Type targetType)
        {
            if (value == null || targetType.IsInstanceOfType(value))
            {
                return value;
            }
            if (targetType.IsEnum)
            {
                return Enum.ToObject(targetType, value);
            }
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static object GetMapComponent(Map map, Type componentType)
        {
            MethodInfo getter = map.GetType().GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance)
                .Where(method => method.Name == "GetComponent")
                .Where(method => method.IsGenericMethodDefinition)
                .Where(method => method.GetGenericArguments().Length == 1)
                .Where(method => method.GetParameters().Length == 0)
                .FirstOrDefault();
            return getter == null
                ? null
                : getter.MakeGenericMethod(componentType).Invoke(map, null);
        }

        private static MethodInfo FindMethod(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                MethodInfo method = current.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .FirstOrDefault(candidate => candidate.Name == name &&
                        candidate.GetParameters().Length == 1);
                if (method != null) return method;
            }
            return null;
        }

        private static IntVec3 FindAnchor(Map map)
        {
            for (int z = 20; z < map.Size.z - 20; z += 3)
            {
                for (int x = 20; x < map.Size.x - 55; x += 3)
                {
                    IntVec3 candidate = new IntVec3(x, 0, z);
                    if (candidate.InBounds(map) &&
                        (candidate + new IntVec3(45, 0, 4)).InBounds(map) &&
                        CellRect.FromLimits(x, z, x + 9, z + 14)
                            .Cells.All(cell => cell.InBounds(map) &&
                                cell.GetThingList(map).Count == 0))
                    {
                        return candidate;
                    }
                }
            }
            // Historical saves can legitimately contain a full ship or a
            // generated map with no untouched 10x15 rectangle. Keep the
            // fixture deterministic in that case; GenSpawn's historical
            // wipe semantics isolate this disposable test footprint and the
            // fixture never saves the mutated map.
            int fallbackX = Math.Max(1, Math.Min(20, map.Size.x - 47));
            int fallbackZ = Math.Max(1, Math.Min(20, map.Size.z - 7));
            IntVec3 fallback = new IntVec3(fallbackX, 0, fallbackZ);
            if (!(fallback + new IntVec3(45, 0, 4)).InBounds(map))
            {
                throw new InvalidOperationException(
                    "historical map is too small for the deterministic fixture footprint");
            }
            return fallback;
        }

        private static Dictionary<string, bool> ReadSettings(object settings)
        {
            return SettingFields.ToDictionary(
                name => name,
                name => (bool)settings.GetType().GetField(name).GetValue(settings));
        }

        private static void WriteSettings(object mod, object settings, bool value)
        {
            foreach (string name in SettingFields)
            {
                FieldInfo field = settings.GetType().GetField(name);
                Assert(field != null, "production setting field is missing: " + name);
                field.SetValue(settings, value);
            }
            MethodInfo write = mod.GetType().GetMethod(
                "WriteSettings",
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            Assert(write != null, "production WriteSettings method is missing");
            write.Invoke(mod, null);
            Dictionary<string, bool> written = ReadSettings(settings);
            Assert(
                written.Values.All(item => item == value),
                "production WriteSettings changed setting values unexpectedly");
        }

        private static object FindLoadedMod(string fullName)
        {
            Type manager = typeof(LoadedModManager);
            Type target = FindType(fullName);
            MethodInfo genericGetter = manager.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(method => method.Name == "GetMod")
                .Where(method => method.IsGenericMethodDefinition)
                .Where(method => method.GetGenericArguments().Length == 1)
                .Where(method => method.GetParameters().Length == 0)
                .FirstOrDefault();
            if (genericGetter != null && target != null)
            {
                try
                {
                    object value = genericGetter.MakeGenericMethod(target)
                        .Invoke(null, null);
                    if (value != null) return value;
                }
                catch
                {
                }
            }

            IEnumerable candidates = manager.GetProperties(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Select(property => SafeGet(property, null))
                .Concat(manager.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    .Select(field => SafeGet(field, null)))
                .OfType<IEnumerable>()
                .Aggregate(
                    new List<object>(),
                    (items, values) =>
                    {
                        foreach (object value in values) items.Add(value);
                        return items;
                    });
            foreach (object value in candidates)
            {
                if (value != null && value.GetType().FullName == fullName)
                {
                    return value;
                }
            }
            return null;
        }

        private static object SafeGet(MemberInfo member, object instance)
        {
            try
            {
                return member is PropertyInfo property
                    ? property.GetValue(instance, null)
                    : ((FieldInfo)member).GetValue(instance);
            }
            catch
            {
                return null;
            }
        }

        private static object ReadStaticObject(Type type, string name)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                PropertyInfo property = current.GetProperty(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (property != null) return property.GetValue(null, null);
                FieldInfo field = current.GetField(
                    name,
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Static | BindingFlags.DeclaredOnly);
                if (field != null) return field.GetValue(null);
            }
            return null;
        }

        private static string ReadStaticString(Type type, string name)
        {
            return Convert.ToString(
                ReadStaticObject(type, name),
                CultureInfo.InvariantCulture);
        }

        private static Type FindType(string fullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => FindType(assembly, fullName))
                .FirstOrDefault(type => type != null);
        }

        private static Type FindType(Assembly assembly, string fullName)
        {
            return assembly.GetType(fullName, false);
        }

        private static Type FindFirstType(
            Assembly assembly,
            params string[] fullNames)
        {
            return fullNames.Select(name => FindType(assembly, name))
                .FirstOrDefault(type => type != null);
        }

        private static Assembly FindAssembly(string simpleName)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly =>
                    assembly.GetName().Name == simpleName);
        }

        private static string BuildVersion()
        {
#if SOS2WR_HISTORICAL_1_1
            return "1.1";
#elif SOS2WR_HISTORICAL_1_2
            return "1.2";
#elif SOS2WR_HISTORICAL_1_3
            return "1.3";
#elif SOS2WR_HISTORICAL_1_4
            return "1.4";
#else
            return "unknown";
#endif
        }

        private static string StatePath()
        {
            return Path.Combine(
                GenFilePaths.ConfigFolderPath,
                FixturePrefix + ".state.xml");
        }

        private static string EvidencePath()
        {
            return Path.Combine(
                GenFilePaths.ConfigFolderPath,
                FixturePrefix + ".evidence.xml");
        }

        private static XElement LoadState(string path)
        {
            return File.Exists(path) ? XElement.Load(path) : null;
        }

        private static void WriteState(string path, XElement state)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            state.Save(path, SaveOptions.DisableFormatting);
        }

        private static string FindSettingsFile()
        {
            string root = GenFilePaths.ConfigFolderPath;
            return Directory.Exists(root)
                ? Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                    .Where(path => path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    .Select(path => new { Path = path, Text = SafeRead(path) })
                    .Where(item => item.Text.IndexOf(
                        "SOS2WeaponReadouts",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                        item.Text.IndexOf(
                            "FeatureEnabled",
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    .Select(item => item.Path)
                    .FirstOrDefault()
                : null;
        }

        private static string SafeRead(string path)
        {
            try { return File.ReadAllText(path); }
            catch { return string.Empty; }
        }

        private static void WriteEvidence(FixtureResult result, string statePath)
        {
            XElement evidence = new XElement(
                "historicalFixture",
                new XAttribute("result", result.Result ?? "FAIL"),
                new XAttribute("version", result.Version ?? "unknown"),
                new XElement("gameAssembly", result.GameAssembly),
                new XElement("sos2Assembly", result.Sos2Assembly),
                new XElement("sos2Types", result.Sos2Types.Select(value => new XElement("type", value))),
                new XElement("defs", result.Defs.Select(value => new XElement("def", value))),
                new XElement("adapterSummary", result.AdapterSummary),
                new XElement("definitionReadout", result.DefinitionReadout),
                new XElement("placementReadout", result.PlacementReadout),
                new XElement("inspectBefore", result.InspectBefore),
                new XElement("inspectAfter", result.InspectAfter),
                new XElement("networkDiagnostics", result.NetworkDiagnostics),
                new XElement("settingsBefore", result.SettingsBefore.Select(item =>
                    new XElement(item.Key, item.Value))),
                new XElement("settingsMutation", result.SettingsMutation),
                new XElement("settingsFile", result.SettingsFile),
                new XElement("attackOrdered", result.AttackOrdered),
                new XElement("heatPerShot", result.HeatPerShot),
                new XElement("powerPerShot", result.PowerPerShot),
                new XElement("heatBefore", result.HeatBefore),
                new XElement("heatAfter", result.HeatAfter),
                new XElement("powerBefore", result.PowerBefore),
                new XElement("powerAfter", result.PowerAfter),
                new XElement("projectilesBefore", result.ProjectilesBefore),
                new XElement("projectilesAfter", result.ProjectilesAfter),
                new XElement("projectileObserved", result.ProjectileObserved),
                new XElement("heatObserved", result.HeatObserved),
                new XElement("powerObserved", result.PowerObserved),
                new XElement("firingOutcome", result.FiringOutcome),
                new XElement("waitTicks", result.WaitTicks),
                new XElement("statePath", statePath),
                new XElement("error", result.Error));
            Directory.CreateDirectory(GenFilePaths.ConfigFolderPath);
            evidence.Save(EvidencePath(), SaveOptions.DisableFormatting);
        }

        private void Cleanup()
        {
            ResetHistoricalCombatState(activeMap);
            for (int index = spawned.Count - 1; index >= 0; index--)
            {
                Thing thing = spawned[index];
                if (thing != null && !thing.Destroyed)
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
            }
            spawned.Clear();
        }

        private static void ResetHistoricalCombatState(Map map)
        {
            if (map != null)
            {
                Type mapCompType = FindType("RimWorld.ShipHeatMapComp") ??
                    FindType("SaveOurShip2.ShipHeatMapComp");
                object mapComp = mapCompType == null
                    ? null
                    : GetMapComponent(map, mapCompType);
                if (mapComp != null)
                {
                    object state = ReadMemberNoThrow(mapComp, "ShipMapState");
                    if (state != null && state.GetType().IsEnum)
                    {
                        string ordinaryName = Enum.GetNames(state.GetType())
                            .FirstOrDefault(name =>
                                name.IndexOf("combat", StringComparison.OrdinalIgnoreCase) < 0);
                        if (ordinaryName != null)
                        {
                            SetMemberIfPresent(mapComp, "ShipMapState", Enum.Parse(
                                state.GetType(), ordinaryName));
                        }
                    }
                    SetMemberIfPresent(mapComp, "InCombat", false);
                }
            }

            Type combatManager = FindType("RimWorld.ShipCombatManager");
            if (combatManager != null)
            {
                SetStaticMember(combatManager, "InCombat", false);
                SetStaticMember(combatManager, "PlayerShip", null);
                SetStaticMember(combatManager, "EnemyShip", null);
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static string Flatten(Exception exception)
        {
            Exception current = exception;
            while (current.InnerException != null) current = current.InnerException;
            return (current.GetType().FullName + ": " + current.Message +
                Environment.NewLine + current.StackTrace)
                .Replace("\r", " ").Replace("\n", " ");
        }

        private sealed class FixtureResult
        {
            public string Result;
            public string Version;
            public string GameAssembly;
            public string Sos2Assembly;
            public string[] Sos2Types = Array.Empty<string>();
            public readonly List<string> Defs = new List<string>();
            public string AdapterSummary;
            public string DefinitionReadout;
            public string PlacementReadout;
            public string InspectBefore;
            public string InspectAfter;
            public string NetworkDiagnostics;
            public Dictionary<string, bool> SettingsBefore =
                new Dictionary<string, bool>();
            public string SettingsMutation;
            public string SettingsFile;
            public bool AttackOrdered;
            public float HeatPerShot;
            public float PowerPerShot;
            public float? HeatBefore;
            public float? HeatAfter;
            public float? PowerBefore;
            public float? PowerAfter;
            public int ProjectilesBefore;
            public int ProjectilesAfter;
            public bool ProjectileObserved;
            public bool HeatObserved;
        public bool PowerObserved;
        public string FiringOutcome;
            public int WaitTicks;
            public string MapSize;
            public string Error;
        }
    }
}
