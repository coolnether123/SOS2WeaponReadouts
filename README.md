# SOS2 Weapon Readouts

SOS2 Weapon Readouts adds missing per-shot information to Save Our Ship 2
weapon descriptions, the existing selected-weapon heat line, and placement
previews. It never changes weapon balance, placement validity, firing logic, or
save data.

## Player features

- Exact heat generated per firing cycle in SOS2 heat units (HU).
- Exact electrical draw per firing cycle in descriptions and placement previews
  where SOS2 does not already show it.
- A compact `(+X/shot)` suffix on SOS2's existing selected-weapon network heat
  line. It reuses the line's HU unit and adds no redundant panel or extra row.
- Clear disconnected-network and missing-bridge information.
- Non-blocking placement readouts and safety warnings.
- Independent toggles for information cards, the selected-weapon heat suffix,
  electrical values, network comparison, and placement previews.
- Semantic deduplication when SOS2 already supplies a field.

Alt-click the placement preview or added information-card row to open and
highlight its setting. The Alt-click does not place or cancel a designator or
alter targeting.

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [Spine](https://github.com/coolnether123/Spine) — the shared runtime used by
  CoolNether123 mods
- [Save Our Ship 2](https://github.com/Bqr1s/SaveOurShip2)

Save Our Ship 2 requires Vehicle Framework. Install SOS2 from its normal source
and it will pull in that requirement; SOS2 Weapon Readouts declares only SOS2
itself and does not duplicate SOS2's own dependency list.

## Installation

Install Harmony, Spine, Vehicle Framework, and Save Our Ship 2 from their
normal sources, copy `SOS2WeaponReadouts` into RimWorld's `Mods` folder, then
enable every dependency before SOS2 Weapon Readouts.

The mod is safe to add to or remove from an existing save because it stores
only global UI preferences and does not add game-state objects.

## Documentation

- [Architecture](docs/architecture.md)
- [Compatibility and performance](docs/compatibility-performance.md)
- [Patch inventory](docs/patches.md)
- [SOS2 API investigation](docs/research/api-investigation.md)
- [Verification record](docs/verification.md)

## Developer fixture

The live firing probe and debug actions are isolated in
`Developer/SOS2WeaponReadouts.TestFixture`, a separately loadable developer
mod documented in
[its own README](Developer/SOS2WeaponReadouts.TestFixture/README.md). Build and
load that folder only for harness verification; it is never part of the
distributed package.

## License

Released under the [MIT License](LICENSE). Harmony, Spine, and Save Our Ship 2
are used under their own licenses.
