# SOS2 Weapon Readouts

SOS2 Weapon Readouts adds the missing per-shot thermal and electrical costs to
Save Our Ship 2 weapon descriptions, selected-weapon controls, and placement
previews. It never changes weapon balance, placement validity, firing logic, or
save data.

## Player features

- Exact heat generated per firing cycle in SOS2 heat units (HU).
- Exact electrical draw per firing cycle in watt-days (Wd).
- Compact selected-weapon display with current network heat/capacity.
- Clear disconnected-network and missing-bridge information.
- Non-blocking placement readouts and safety warnings.
- Independent toggles for descriptions, the selected-weapon display, electrical values,
  network comparison, and placement previews.
- Semantic deduplication when SOS2 already supplies a field.

## Requirements

- RimWorld 1.6
- Harmony
- Spine
- Save Our Ship 2 and its required Vehicle Framework dependency

## Installation

Spine does not yet have a public Workshop or download URL, so this verified
build is distributed in the local collection at
`A:\Dev\RimWorld\Releases\1.6\2026-07-30-program-final`. Copy
`SOS2WeaponReadouts` and `Spine` into RimWorld's `Mods` directory, install
Harmony, Vehicle Framework, and Save Our Ship 2 from their normal sources, and
enable the dependencies before SOS2 Weapon Readouts. No other gameplay mod in
the collection is required. The repository's `Developer` fixture is never
part of the distributed folder.

The mod is safe to add to or remove from an existing save because it stores
only global UI preferences and does not add game-state objects.

See [architecture](docs/architecture.md), [API investigation](docs/research/api-investigation.md),
[verification](docs/verification.md), and the
[developer firing fixture](Developer/SOS2WeaponReadouts.TestFixture/README.md)
for engineering details.
