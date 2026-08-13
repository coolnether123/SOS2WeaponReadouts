# SOS2 Weapon Readouts historical fixture

This is a separately loadable developer mod for the declared RimWorld 1.1,
1.2, 1.3, and 1.4 branches. It is not a shipping assembly and has no
RimWorld Agent reference. A `GameComponent` schedules it after a map is loaded;
the fixture then creates the historical SOS2 hull/core/laser/heatsink/capacitor
layout, invokes the production readout entry points, orders a real turret
attack, observes heat/power/projectile state, and removes its own things.

The fixture resolves SOS2 types and the optional historical ship-cache type at
runtime. This keeps `ShipMapComp` and other branch-specific identities out of
the fixture build contract while still requiring the exact loaded SOS2
assembly and historical defs at runtime.

It also finds the loaded production mod instance, mutates all six production
settings through `WriteSettings`, and records a two-process persistence state
in the lane Config directory. The second process must be started from a copy
of the stopped first process's Config directory. A failed assertion writes a
`result=FAIL` evidence file and a `Log.Error` entry.

Build with the documented `Invoke-RimWorldBuild.ps1` command and stage only
the requested fixture assembly into an isolated lane. Never include this
directory in a release package.
