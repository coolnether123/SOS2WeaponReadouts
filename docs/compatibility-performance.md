# Compatibility and performance

## Compatibility

The supported target is RimWorld 1.6 with the pinned SOS2 stable API. Public
members are resolved once and cached. Incompatible SOS2 builds disable all
integration and report one warning rather than causing repeating exceptions.
Because SOS2 is a declared required dependency, RimWorld normally prevents the
mod from loading when SOS2 is absent; the unavailable adapter is defensive
startup behavior rather than the normal missing-dependency user experience.

Combat Extended's optional SOS2 surrogate turret is supported when CE's
`SOS2Compat` assembly is present. The adapter discovers that class without a
hard CE dependency, validates its heat and firing-cost shape, and patches its
SOS2 heat-component inspect line alongside native SOS2 turrets. Definition
information and placement readouts use the same shared calculation path. Combat Extended
must remain after Save Our Ship 2 in the load order, as declared by CE itself.

Future SOS2 releases that add equivalent text are handled semantically: heat,
electrical, connection, and current network capacity fields are independently
suppressed when already present.

## Performance

- No `GameComponent`, `MapComponent`, tick hook, update hook, or global draw
  hook is added.
- Type/member reflection happens once during adapter construction.
- Definition cost reads occur only when an information card enumerates stats.
- Placed cost reads occur only when SOS2 requests a heat component's inspect
  text; non-weapon parents are rejected before any weapon-cost reflection.
- Cardinal cell/network scanning occurs only for the currently active SOS2
  weapon placement designator during RimWorld's ordinary OnGUI mouse-attachment
  pass.
- The settings window reads status text only while open.

Consequently, the mod adds no work per frame while its UI surfaces are closed.
The frame-driven developer firing fixture is a separately loaded test mod and
is not present in production profiles or release packages.
