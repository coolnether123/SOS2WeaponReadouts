# Harmony patches

## ThingDef.SpecialDisplayStats

Reason: RimWorld's information card enumerates definition stats and does not
consume a patched `DescriptionDetailed` value on this path. An enumerable
postfix preserves every existing stat and adds a weapon-cost row only after
the adapter recognizes an SOS2 weapon.

Risk controls:

- No prefix cancellation; existing entries are materialized and preserved in
  their original order.
- Non-SOS2 definitions return the same entries.
- Existing heat/electrical entries are detected line by line before appending.
- Exceptions are caught at the runtime boundary and logged once.

## SaveOurShip2.CompShipHeat.CompInspectStringExtra

Reason: SOS2 already owns the selected weapon's live network-heat line. A
postfix adds only the missing compact per-shot suffix to that first line. The
target is resolved by the validated adapter, so an incompatible API prevents
patch installation.

Risk controls:

- Postfix only.
- Non-weapon heat components return their original text unchanged.
- Native and CE-surrogate weapons share the same adapter readout path.
- The suffix contains per-shot heat only, reuses the line's HU unit, and adds
  no newline.
- No firing, ticking, power, heat, or placement method is patched.

Placement integration uses RimWorld's standard `PlaceWorker` definition list,
not Harmony. It only draws text from the supported
`DrawPlaceMouseAttachments` OnGUI callback and never changes `AllowsPlacing`.
`DrawGhost` remains unimplemented because RimWorld invokes that path from
`Designator_Place.SelectedUpdate`, where immediate-mode GUI calls are illegal.
