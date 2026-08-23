# SOS2 Weapon Readouts

SOS2 Weapon Readouts adds the per-shot numbers missing from Save Our Ship 2
weapon descriptions, placement previews, and the heat line shown for a
selected weapon. It never changes weapon balance, placement rules, firing, or
save data.

## Player features

- Heat generated per firing cycle in SOS2 heat units (HU).
- Electrical draw per firing cycle in descriptions and placement previews
  when SOS2 does not already show it.
- A compact `(+X/shot)` suffix on SOS2's existing selected-weapon network heat
  line. It reuses the line's HU unit and adds no redundant panel or extra row.
- Warnings for disconnected networks and missing bridges.
- Placement readouts that never block construction.
- Independent toggles for information cards, the selected-weapon heat suffix,
  electrical values, network comparison, and placement previews.
- No duplicate fields when SOS2 already shows the same information.

Alt-click a placement preview or an added information-card row to open its
setting. This will not place anything, cancel the designator, or change the
target.

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

The mod stores only global display preferences, so you can add or remove it
from an existing save.

See [architecture](docs/architecture.md),
[API investigation](docs/research/api-investigation.md), and
[verification](docs/verification.md) for engineering details. The developer
firing fixture lives outside version control; it is a local harness aid, not
part of the mod.
