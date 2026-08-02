# Architecture

## Boundaries

`Compatibility/ISos2WeaponAdapter.cs` is the only contract through which the
rest of the mod asks SOS2 questions. `Sos2V16Adapter` validates and caches the
public 1.6 API members at startup, then returns immutable domain models. SOS2
types and source are never copied or exposed outside this adapter.

The compiled assembly deliberately has no static reference to
`ShipsHaveInsides.dll`. This lets the normal mod assembly load even when SOS2
is missing or incompatible, after which the adapter reports one controlled
compatibility message and disables integration.

`Domain/` owns the SOS2 firing formula, immutable readout values, semantic
duplicate detection, number formatting, and line composition. These classes
are pure C# and covered by the standalone test executable.

`Runtime/WeaponReadoutRuntime.cs` coordinates settings, adapter calls,
localization, and error isolation. Every runtime entry point is demand-driven:
definition stats are evaluated when an info card enumerates them, selected
weapon values are evaluated when SOS2 builds its heat-component inspect text, and placement values are
evaluated only while its designator is active. No map, world, or per-frame
component runs while the UI is closed.

`Patches/` contains two narrow Harmony postfixes:

- `ThingDef.SpecialDisplayStats` appends one weapon-stat row only for
  definitions accepted by the SOS2 adapter.
- SOS2 `CompShipHeat.CompInspectStringExtra` adds `(+X/shot)` to the end of
  the existing network-heat line for weapon parents only; it never adds a row.

`UI/WeaponReadoutPlaceWorker.cs` is attached only to SOS2 weapon definitions
after definition loading. It appends heat, electrical, and network lines
through RimWorld's supported `PlaceWorker.DrawPlaceMouseAttachments` OnGUI
callback. It does not draw from `DrawGhost`/`SelectedUpdate` and never rejects
placement. Settings use Spine's shared settings widgets instead of another
framework.

## Developer fixture boundary

`Developer/SOS2WeaponReadouts.TestFixture` is a separate RimWorld mod and
assembly. It is loaded only when an agent passes its root through the harness
`-AdditionalModPaths` option. It constructs and removes real SOS2 game objects,
observes firing calls, and writes proof artifacts. The production mod has no
reference to RimWorld Agent or the fixture. The shared
`New-RwtReleasePackage` command stages only the explicit `About`,
`1.6/Assemblies/SOS2WeaponReadouts.dll`, and `Languages` runtime allowlist.
That mechanically excludes the entire `Developer/` tree, along with source,
tests, engineering evidence, build logs, symbol files, and nested build
outputs, from the distributable folder.

The fixture advances from `IRimWorldAgentExtension.OnFrame` because SOS2
rebuilds heat grids in its frame-driven `ShipMapComp.MapComponentUpdate`.
Cleanup removes the fixture ship from SOS2's cache before destroying its parts,
continues through individual teardown errors, recaches, and requires zero ships
remaining.

## Dependency direction

UI and patches → runtime → adapter interface/domain. The reflection-backed SOS2
adapter depends on the domain but no domain type depends on RimWorld, Harmony,
SOS2, or Vehicle Framework.

## One-caller helpers

Private reflection readers inside `Sos2V16Adapter` have one or a few callers
because they isolate unsafe API-boundary conversion in the adapter. They
should remain local until a second compatibility adapter needs the same
operation; promoting them to shared infrastructure now would create a
speculative abstraction.

`FiringProofEvaluator` is intentionally consumed by only the developer fixture
and its linked pure tests. It is repository-local test infrastructure, not a
production abstraction; moving it into the shipping DLL would violate the
fixture boundary.
