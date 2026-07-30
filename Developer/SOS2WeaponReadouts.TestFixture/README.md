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
dev-run sos2wr-firing-fixture cleanup
```

`run` is asynchronous: poll `status` until it returns `result=PASS`. It builds
a real SOS2 ship and connected/disconnected laser networks, fires through the
ordinary SOS2 attack path, observes burst/cast/projectile calls, and verifies
exact heat and power deltas. `cleanup` must report
`cacheRemoved=True; shipsRemaining=0; cleanupErrors=0` before the lane stops.

The fixture uses one repository-local proof evaluator that is linked into the
pure test executable. That one-consumer helper is deliberate test
infrastructure; it should not be moved into the shipping DLL.
