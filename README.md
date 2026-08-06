# SOS2 Weapon Readouts

SOS2 Weapon Readouts adds missing per-shot information to Save Our Ship 2
weapon descriptions, the existing selected-weapon heat line, and placement
previews. It never changes weapon balance, placement validity, firing logic, or
save data.

## Player features

- Exact heat generated per firing cycle in SOS2 heat units (HU).
- Exact electrical draw per firing cycle in descriptions and placement
  previews where SOS2 does not already show it.
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
- Harmony
- SpineLib
- Save Our Ship 2 and its required Vehicle Framework dependency

## Installation

SpineLib is available on the [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3778463813); repository builds are at [coolnether123/Spine](https://github.com/coolnether123/Spine).

Copy `SOS2WeaponReadouts` and `Spine` into RimWorld's `Mods` directory, install
Harmony, Vehicle Framework, and Save Our Ship 2 from their normal sources, and
enable the dependencies before SOS2 Weapon Readouts. No other mod is required.

The mod is safe to add to or remove from an existing save because it stores
only global UI preferences and does not add game-state objects.

See [architecture](docs/architecture.md),
[API investigation](docs/research/api-investigation.md), and
[verification](docs/verification.md) for engineering details. The developer
firing fixture lives outside version control; it is a local harness aid, not
part of the mod.
