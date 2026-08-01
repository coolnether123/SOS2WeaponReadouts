# SOS2 Weapon Readouts compatibility matrix — 2026-07-31

Runtime for every valid row: RimWorld 1.6.4871 rev573, Core only, isolated H-drive harness host, development mode enabled. Full details, load orders, versions, scenarios, hashes, and limits are in `compatibility-investigation-2026-07-31.md`.

| Combination / surface | Classification | Evidence summary | Evidence session |
|---|---|---|---|
| Required closure: Spine + VF + SOS2 + target | **Compatible** | Real laser shot matched 30 HU / 80 Wd exactly; disconnected/insufficient cases did not fire; info, gizmo, placement, settings reload, and hidden-exception scan passed | `SOS2WeaponReadouts-dbab02fa329d43d6bdb987968a3a213d` |
| RimHUD + target, selected turret surface | **Compatible** | Repeated real-shot proof; readout coexisted; zero exceptions | `SOS2WeaponReadouts-672c5a7efaa143a7b9b510d37e2abd99` |
| RimHUD pawn-only surface | **Inconclusive** | A valid pawn/RimHUD panel was not established | same lane |
| Dubs Mint Menus + target, target-first input | **Compatible** | One info-card cost line, clean placement, zero exceptions | `SOS2WeaponReadouts-aca8d266ed314322811dae88a0d7ffaa` |
| Dubs Mint Menus + target, Dubs-first input | **Compatible** | Clean startup and Harmony ownership; normal exit | `1446523594-ac56c9b777a144238ca12699ceb67a79` |
| Dubs actual Mint architect menu interaction | **Inconclusive** | Downstream designator worked; Mint menu itself was not navigated | Dubs lanes |
| Architect Icons + Better Architect Menu startup | **Compatible** | Clean patch ownership, no exceptions, normal exit | `SOS2WeaponReadouts-d0abf3c9e28d4c849032c1a25515ed43` |
| Architect Icons + Better Architect in-map placement | **Inconclusive** | Placement command arrived during map generation and returned `map-required` | same lane |
| CE-converted standard and point-defense turrets | **Patch required** | CE surrogate class falls outside target's ordinary SOS2 class boundary; gizmo/info/placement absent | `SOS2WeaponReadouts-dbd94c9d8dce4f9b9bb551733b3553b7` |
| CE + spinal barrel definition readout | **Compatible with documented limitation** | Cost line retained because CE restores ordinary SOS2 class; real firing not proven | same lane |
| CE + torpedo definition readout | **Compatible with documented limitation** | 0 HU / 0 Wd line retained; real firing not proven | same lane |
| CE before SOS2 | **Unsupported hard conflict** | Violates CE `loadAfter`; CE SOS2 assembly failed type loading | `CombatExtended-06ad92f2ae1d4cab9982deae4da9997c` |
| SOS2 dependency `PlantPot_Bonsai` warning | **External-mod defect / false alarm** | Present without target ownership; did not affect firing proof | baseline lane |
| Second fixture run in same lane | **Integration opportunity / false alarm** | Deliberate write-new evidence guard; not gameplay behavior | baseline lane |
| SOS2 Space Expanded | **Inconclusive** | Current input not staged/tested | — |
| SOS2 – Archotech Expansion | **Inconclusive** | Current input not staged/tested | — |
| SOS2 – Cyberwarfare | **Inconclusive** | Current input not staged/tested | — |
| Current SOS2 weapon packs/submods | **Inconclusive** | Inputs not staged/tested | — |
| Older heat-statistics mods | **Inconclusive** | Abandoned/current 1.6 input not staged; no support promised | — |
| Full-DLC/Odyssey multi-map behavior | **Inconclusive; grouped pass pending** | Canonical H host is Core-only; checklist is in the report | — |
| Save/reload settings | **Compatible** | Disabled setting remained disabled after load and was restored | baseline lane |
| Removal from copied save | **Inconclusive** | Not executed | — |
| Numeric performance comparison | **Inconclusive** | Not measured; patch ownership alone is insufficient | — |

## Release triage

1. **Release blocker candidate:** CE-converted standard/point-defense turret surfaces are missing. Verify actual CE firing, then add the smallest optional surrogate adapter in the target or an optional compatibility assembly—not Spine.
2. **Before-release validation:** run the grouped full-DLC/Odyssey multi-map checklist and copied-save removal test.
3. **Before-release validation:** complete numeric selected/placement/closed-UI performance measurements and the UI scale/resolution pass.
4. **Post-release integration candidates:** current SOS2 submods/weapon packs and semantic duplicate checks against any maintained heat-statistics replacement, once concrete 1.6 inputs are identified.
5. **Document, do not patch:** Combat Extended must load after SOS2.
