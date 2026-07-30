# SOS2 Weapon Readouts

SOS2 Weapon Readouts adds the missing per-shot thermal and electrical costs to
Save Our Ship 2 weapon descriptions, built-weapon inspection, and placement
previews. It never changes weapon balance, placement validity, firing logic, or
save data.

## Player features

- Exact heat generated per firing cycle in SOS2 heat units (HU).
- Exact electrical draw per firing cycle in watt-days (Wd).
- Built-weapon post-shot heat/capacity comparison.
- Clear disconnected-network and missing-bridge information.
- Non-blocking placement readouts and safety warnings.
- Independent toggles for descriptions, inspect output, electrical values,
  network comparison, and placement previews.
- Semantic deduplication when SOS2 already supplies a field.

## Requirements

- RimWorld 1.6
- Harmony
- Spine
- Save Our Ship 2 and its required Vehicle Framework dependency

The mod is safe to add to or remove from an existing save because it stores
only global UI preferences and does not add game-state objects.

See [architecture](docs/architecture.md), [API investigation](docs/research/api-investigation.md),
[verification](docs/verification.md), and the
[developer firing fixture](Developer/SOS2WeaponReadouts.TestFixture/README.md)
for engineering details.
