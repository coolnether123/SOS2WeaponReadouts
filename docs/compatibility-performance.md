# Compatibility and performance

## Compatibility

The supported target is RimWorld 1.6 with the pinned SOS2 stable API. Public
members are resolved once and cached. Incompatible SOS2 builds disable all
integration and report one warning rather than causing repeating exceptions.
Because SOS2 is a declared required dependency, RimWorld normally prevents the
mod from loading when SOS2 is absent; the unavailable adapter is defensive
startup behavior rather than the normal missing-dependency user experience.

Current known limitation: Combat Extended's optional SOS2 surrogate turret
class is not patched. Supporting it safely requires validating CE's separate
compatibility assembly and is outside the required SOS2 dependency scope.

Future SOS2 releases that add equivalent text are handled semantically: heat,
electrical, connection, and current network capacity fields are independently
suppressed when already present.

## Performance

- No `GameComponent`, `MapComponent`, tick hook, update hook, or global draw
  hook is added.
- Type/member reflection happens once during adapter construction.
- Definition cost reads occur only when an information card enumerates stats.
- Placed cost reads occur only when RimWorld requests the selected SOS2
  turret's gizmos.
- Cardinal cell/network scanning occurs only for the currently active SOS2
  weapon placement designator during RimWorld's ordinary OnGUI mouse-attachment
  pass.
- The settings window reads status text only while open.

Consequently, the mod adds no work per frame while its UI surfaces are closed.
The frame-driven developer firing fixture is a separately loaded test mod and
is not present in production profiles or release packages.
