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

## SaveOurShip2.Building_ShipTurret.GetInspectString

Reason: SOS2's public turret class owns the built-weapon inspect string and
does not expose an append event. The target is resolved by the validated
adapter, so an incompatible API prevents patch installation.

Risk controls:

- Postfix only.
- SOS2's result remains authoritative.
- Current SOS2 energy, network, and bridge lines are not duplicated.
- No firing, ticking, power, heat, or placement method is patched.

Placement integration uses RimWorld's standard `PlaceWorker` definition list,
not Harmony. It only draws text and never changes `AllowsPlacing`.
