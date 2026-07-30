# SOS2 1.6 API investigation

Investigated source:

- Repository: `A:\Dev\RimWorld\Dependencies\SaveOurShip2`
- Branch: `stable`
- Commit: `296ba9a2bec124981cff46e557a07934702a210b`
- Runtime assembly:
  `A:\Dev\RimWorld\Dependencies\SaveOurShip2\1.6\Assemblies\ShipsHaveInsides.dll`
- Assembly SHA-256:
  `ACF42144F4340D24D63E2695FC5D6BC94BC48E14D7E158DF7B7E43D078EF2DAE`

The assembly declares version `0.0.0.0`, so that value cannot identify a
supported release. The adapter instead verifies the complete public member
shape used by the mod. A missing type or member produces an unsupported-API
status before patches are applied.

## Authoritative firing values

`CompProps_ShipHeat.heatPerPulse` and `energyToFire` are the definition inputs.
`Building_ShipTurret.HeatToFire` computes:

`heatPerPulse × (1 + AmplifierDamageBonus) × 3`

`Building_ShipTurret.EnergyToFire` computes:

`energyToFire × (1 + AmplifierDamageBonus)`

`BeginBurst` passes `HeatToFire` to `CompShipHeat.AddHeatToNetwork` once and
draws `EnergyToFire` proportionally from connected batteries. Therefore the UI
uses "heat generated per shot" in HU and "electrical draw per shot" in Wd; it
does not describe electrical energy as generated.

Unresolved spinal weapons have `spinalComp != null` and
`AmplifierCount == -1`. Their dynamic values are deliberately shown as
unavailable instead of presenting the unamplified definition as a live value.

## Network and connection values

Placed readouts use public members:

- `CompShipHeat.myNet`
- `ShipHeatNet.StorageCapacity`
- `ShipHeatNet.StorageUsed`
- `Building_ShipTurret.ConnectedToBridge`

Placement preview follows SOS2's cardinal-adjacency network construction. It
collects unique adjacent `CompShipHeat.myNet` objects and combines their
capacity and current heat because placement would merge those networks.
Bridge availability is derived from the public `PilCons`, `TacCons`, and
`AICores` collections.

## Existing SOS2 output

Current `CompShipHeat.CompInspectStringExtra` already supplies energy-to-fire
and stored-heat/capacity text. `Building_ShipTurret.GetInspectString` already
supplies a bridge disconnection warning. The formatter recognizes those
semantics and omits duplicate fields while still adding per-shot heat and
post-shot capacity comparison.
