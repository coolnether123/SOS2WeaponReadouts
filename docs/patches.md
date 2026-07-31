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

## SaveOurShip2.Building_ShipTurret.GetGizmos

Reason: the standard selected-object gizmo area can show live weapon values
without lengthening RimWorld's bottom-left inspect panel. The target is
resolved by the validated adapter, so an incompatible API prevents patch
installation.

Risk controls:

- Postfix only.
- SOS2's existing gizmos remain authoritative and in their original order.
- One fixed-width readout gizmo is appended only for a recognized weapon.
- It reports current network heat/capacity and per-shot costs, never projected
  after-shot heat.
- No firing, ticking, power, heat, or placement method is patched.

Placement integration uses RimWorld's standard `PlaceWorker` definition list,
not Harmony. It only draws text from the supported
`DrawPlaceMouseAttachments` OnGUI callback and never changes `AllowsPlacing`.
`DrawGhost` remains unimplemented because RimWorld invokes that path from
`Designator_Place.SelectedUpdate`, where immediate-mode GUI calls are illegal.
