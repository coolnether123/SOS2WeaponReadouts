# SOS2 Weapon Readouts Agent Guide

Own only this repository and keep every SOS2/Combat Extended access behind one
adapter. Prefer public SOS2 members, add only missing definition text, and keep
optional reflected CE shapes inside that adapter. Missing CE remains a normal
lane; a changed required SOS2 boundary fails compatibility negotiation instead
of creating a second presentation system.

Definition readouts belong on `ThingDef.SpecialDisplayStats`. Placement uses a
non-blocking `PlaceWorker`; immediate-mode GUI belongs only in
`DrawPlaceMouseAttachments`, never `DrawGhost` or
`Designator.SelectedUpdate`. Built-weapon values append one compact suffix to
SOS2's existing first heat-network line—never another gizmo/inspect fallback,
duplicate electrical draw, or projected heat value.

Use `SpineMod<SOS2WeaponReadoutsSettings>` and bind only UI rectangles and
`StatDrawEntry` values owned by this mod. Contextual input must be consumed
before placement/selection and never patch unrelated information rows.

Metadata, dependency manifests, `Tools\CascadeManifest.json`, project files,
and the complete `Tests` directory define support and verification. Run all
pure/API/UI boundary contracts, then a fresh harness lane with the full
dependency closure.

Actual-fire verification belongs only in
`Developer\SOS2WeaponReadouts.TestFixture`. Use its documented command/status
and cleanup flow; cleanup must complete before stopping the lane. Do not add
fixture or agent code to the shipping assembly, and do not replace the real
frame-driven SOS2 network fixture with a generic spawned surrogate.

Follow `A:\Dev\RimWorld\AGENTS.md` for shared build/runtime/evidence rules.
