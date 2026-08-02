# SOS2 Weapon Readouts developer fixture

This directory is a separate developer-only RimWorld mod. It is never part of
the production assembly or release package.

Build it after the production DLL with the RimWorld 1.6 managed assemblies,
Harmony, RimWorld Agent, SOS2, and Vehicle Framework reference properties in
`Source/SOS2WeaponReadouts.TestFixture.csproj`. Start an isolated lane with the
production mod as `-ModPath` and this directory as an explicit
`-AdditionalModPaths` value.

```text
dev-run sos2wr-firing-fixture run
dev-run sos2wr-firing-fixture status
dev-run sos2wr-firing-fixture select-placement ShipTurret_Laser
dev-run sos2wr-firing-fixture cleanup
```

`run` is asynchronous: poll `status` until it returns `result=PASS`. It builds
a real SOS2 ship and connected/disconnected laser networks, fires through the
ordinary SOS2 attack path, observes burst/cast/projectile calls, and verifies
exact heat and power deltas. The same fixture accepts SOS2's native turret and
Combat Extended's optional `Building_ShipTurretCE` surrogate through their
shared heat, power, verb, and targeting shape; the developer DLL does not take
a build-time dependency on Combat Extended. `cleanup` must report
`cacheRemoved=True; shipsRemaining=0; cleanupErrors=0` before the lane stops.

`select-placement` selects RimWorld's ordinary build designator for the
requested ThingDef. It exists only to exercise placement UI in an isolated
fixture lane; the production mod does not expose or depend on this command.

The fixture uses one repository-local proof evaluator that is linked into the
pure test executable. That one-consumer helper is deliberate test
infrastructure; it should not be moved into the shipping DLL.
