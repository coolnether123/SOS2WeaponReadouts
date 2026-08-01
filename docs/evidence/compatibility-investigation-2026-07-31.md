# SOS2 Weapon Readouts compatibility investigation — 2026-07-31

## Scope and result

This is an evidence-only investigation of the current SOS2 Weapon Readouts repository build. No compatibility code, test, or external mod was changed. The canonical runtime was RimWorld 1.6.4871 rev573 on the isolated H-drive harness host with Core only and development mode enabled. All valid sessions used their own profile, log, IPC, and evidence directories and were stopped by exact session ID.

The baseline is compatible and the displayed laser-turret costs were proven against a real shot: the readout predicted 30 HU and 80 Wd, and firing changed the network by exactly 30 heat and 80 stored power. Insufficient-power and disconnected-network attempts did not launch a projectile and changed neither value. The information card, selected-weapon gizmo, and placement preview were exercised independently without the former illegal `GUI.DrawTexture` call or another hidden exception.

RimHUD and Dubs Mint Menus did not conflict on the surfaces exercised. Architect Icons plus Better Architect Menu reached startup and installed patches cleanly, but map generation had not completed before the placement request, so in-map coexistence remains inconclusive. Combat Extended has a precise compatibility boundary: its SOS2 patch replaces ordinary SOS2 ship-turret instances with `CombatExtended.Compatibility.SOS2Compat.Building_ShipTurretCE`. SOS2 Weapon Readouts currently recognizes and patches `SaveOurShip2.Building_ShipTurret`, so CE-converted standard and point-defense turrets lose the selected gizmo, info-card cost line, and placement readout. CE deliberately leaves spinal weapons on the ordinary SOS2 class and those definition readouts remained present. This is a patch-required class-boundary issue, not a generic claim that all CE/SOS2 combinations are incompatible.

## Tested inputs and provenance

The “download date” below is the acquisition time where this investigation downloaded the asset, otherwise the local repository or Workshop cache timestamp available on this machine. A cache timestamp is not represented as a publisher release date.

| Mod | Package ID | Source/version tested | Local acquisition/cache date |
|---|---|---|---|
| SOS2 Weapon Readouts | `CoolNether123.SOS2WeaponReadouts` | repository `main`, commit `464741a17f0911c7f9ebd874f59bc52122d8c600` | current repository build on 2026-07-31 |
| Spine | `CoolNether123.Spine` | repository commit `f63a595ffc2a1ad7e60a6eec7b1d69fb48bf18b5` | current repository build on 2026-07-31 |
| Save Our Ship 2 | `kentington.saveourship2` | GitHub commit `296ba9a2bec124981cff46e557a07934702a210b` from `https://github.com/Bqr1s/SaveOurShip2.git`; 1.6 assembly SHA-256 `ACF42144F4340D24D63E2695FC5D6BC94BC48E14D7E158DF7B7E43D078EF2DAE` | staged locally 2026-07-30 |
| Vehicle Framework | `SmashPhil.VehicleFramework` | release `1.6.2144`; source commit `1e8ec34e1b6a481255d70bcaa54fbd05b44b49dc`; runtime DLL SHA-256 `CCEB717375D6B907318D59D05CBE64EA1E2396E9A1AC66E18B59584A1042409E` | staged locally 2026-07-30 14:10:37 -05:00 |
| Combat Extended | `CETeam.CombatExtended` | official release `v1.6.7.3.0-hotfix.1`, About version `16.7.3.0`; release ZIP SHA-256 `BF82201A548CD6743F0C0DBE87C543EBFCFB3B266197F8E6DD5B683B8E6336E8` | downloaded 2026-07-31 19:40 -05:00 |
| RimHUD | `Jaxe.RimHUD` | Workshop item `1508850027`, described version `1.17.5` | local Workshop folder last modified 2025-09-28 13:16:44 -05:00 |
| Dubs Mint Menus | `Dubwise.DubsMintMenus` | Workshop item `1446523594`, version `1.3.1247` | local Workshop folder last modified 2025-07-13 17:48:10 -05:00 |
| Architect Icons | `com.bymarcin.ArchitectIcons` | Workshop item `1195427067`, described version `1.9` | local Workshop folder last modified 2025-07-13 17:48:10 -05:00 |
| Better Architect Menu | `ferny.BetterArchitect` | Workshop item `3563882422`, 1.6 metadata | local Workshop folder last modified 2026-07-28 18:24:49 -05:00 |

Every valid lane used `H:\Games\RimWorld1-6-4871Win64\RimWorldWin64.exe`, build `1.6.4871 rev573`, Core only. The game assembly registered by the harness has SHA-256 `4A170804FBFEFABDB620D8914E584E58F822A58C6E304DCB76A67003588DAB28`.

## Combination records

### Target with required closure — compatible

Session: `SOS2WeaponReadouts-dbab02fa329d43d6bdb987968a3a213d`

Load order: Core → Harmony → RimWorld Agent → Spine → Vehicle Framework → Save Our Ship 2 → SOS2 Weapon Readouts → developer firing fixture.

Scenario: generate an isolated map and complete SOS2 laser network; inspect selected gizmo, info card, and placement preview; fire a real laser; repeat with insufficient stored power and with a disconnected network; toggle `ShowSelectedWeaponReadout`, persist it, save, reload, verify it remains false, then restore it.

Result and evidence:

- A real shot invoked one burst, one cast, and one `Bullet_Ground_Laser` launch. Heat moved 0 → 30 HU and stored power 995.979 → 915.979, exactly matching the 30 HU / 80 Wd readout.
- Insufficient power and a disconnected network produced zero casts, zero projectile launches, zero heat delta, and zero power delta.
- The selected gizmo showed current heat/capacity and heat per shot without adding an inspect-pane scrollbar. SOS2's existing `Energy to fire: 80 Wd` remained the owner of electrical cost in the inspect pane.
- The info card contained one semantic cost line, `SOS2 weapon costs 30 HU / 80 Wd`; no duplicate was visible.
- The placement preview showed heat, electrical cost, and current network state, with no `GUI functions from inside OnGUI` exception.
- Harmony ownership was narrow: `Verse.ThingDef.SpecialDisplayStats` postfix and `SaveOurShip2.Building_ShipTurret.GetGizmos` postfix. The target owner had two installed runtime patches.
- The setting survived reload. The target stores no map/save gameplay component of its own. Removal from a copied save was not completed and remains pending.

Primary paths:

- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-dbab02fa329d43d6bdb987968a3a213d\ipc\evidence\sos2wr\firing-proof.txt` — SHA-256 `5D2035A5E0371919F0B78E73A34EDEE42C1221BC9C3245ED6E850996EBF39BC9`
- `...\ipc\captures\baseline-selected-laser-gizmo-20260801-005101-897.png` — SHA-256 `C983F798A0587D74E3381361F5965A7A09B4EEFB53DD82ED8E57A6FA48061425`
- `...\ipc\captures\baseline-laser-info-20260801-005111-056.png` — SHA-256 `AD9276C7FED2CE3C99CA14F0E0785641FA1612C07F6AB222F564CFC32ED2B640`
- `...\ipc\captures\baseline-laser-placement-20260801-005139-746.png` — SHA-256 `9B9A59DACD9844FA39C11B1D55BA3D61BECC7A92CAFC73C2DADEFC7ABD5D73D2`

One attempted rerun of the single-use fixture in the same lane was rejected by its write-new artifact guard and is not gameplay evidence. That is a fixture/harness limitation, not a target-mod failure.

### Combat Extended in its declared order — patch required for CE-converted turrets

Session: `SOS2WeaponReadouts-dbd94c9d8dce4f9b9bb551733b3553b7`

Load order: Core → Harmony → RimWorld Agent → Spine → Vehicle Framework → Save Our Ship 2 → SOS2 Weapon Readouts → Combat Extended → developer firing fixture. CE declares that it loads after SOS2, and this order satisfies that constraint.

Scenario: start with CE's supplied SOS2 compatibility assembly; inspect an ordinary laser turret, its selection, info card and placement; inspect a spinal barrel, a spinal capacitor/emitter definition, and a torpedo launcher; attempt the real-fire fixture.

Result:

- CE changed `ShipTurret_Laser` to runtime class `CombatExtended.Compatibility.SOS2Compat.Building_ShipTurretCE`.
- The CE-converted standard laser showed neither the target gizmo nor its info-card/placement cost lines. The target's current `IsWeaponDefinition` and `Building_ShipTurret.GetGizmos` boundary does not cover the CE surrogate class.
- CE's Save Our Ship 2 XML deliberately restores spinal weapons to `SaveOurShip2.Building_ShipTurret`; the spinal barrel info card retained `SOS2 weapon costs 270 HU / 720 Wd`.
- The spinal capacitor/emitter object is not itself a weapon and correctly showed no weapon-cost line.
- The torpedo definition retained `SOS2 weapon costs 0 HU / 0 Wd` because its SOS2 special class is not converted by CE.
- The firing fixture cast its spawned object to `SaveOurShip2.Building_ShipTurret`, so it failed immediately against CE's surrogate type. Cleanup completed (`removed=152`, cache removed, zero ships, zero cleanup errors). Actual CE projectile cost comparison is therefore inconclusive.

Smallest defensible response: add explicit CE-surrogate support in SOS2 Weapon Readouts or an optional CE compatibility assembly, after verifying CE's public/runtime fields and real firing. This behavior is SOS2/CE domain-specific and does **not** belong in Spine. Do not report generic Combat Extended incompatibility: retained ordinary SOS2 subclasses already work on the definition surface.

Evidence:

- `...\SOS2WeaponReadouts-dbd94c9d8dce4f9b9bb551733b3553b7\ipc\captures\ce-standard-selected-20260801-005623-585.png` — SHA-256 `9166632C0F04BF246D0AC4F70072F1176D7775CA817BB4247420414BC549FCA9`
- `...\ipc\captures\ce-standard-info-20260801-005628-955.png` — SHA-256 `254175B3D10EC9EBD3D42281E350BD9563D770E25856D659F26991655A189A78`
- `...\ipc\captures\ce-standard-placement-20260801-005749-827.png` — SHA-256 `9FB94850AE87F2E85C902295D715375E7877FF92FDF570E6A505CF6A0D6BCCBB`
- `...\ipc\captures\ce-spinal-barrel-info-20260801-005737-835.png` — SHA-256 `040F58D89D25D464DE0A6967A90C02461CA7D39BB1E740A021F81ACE0D8809F0`
- `...\ipc\captures\ce-torpedo-info-20260801-005711-257.png` — SHA-256 `EB9DA7316F85E7591E085048B9964C50B31088CBFF404F797D0E1E79881A52AA`

### Combat Extended before SOS2 — expected hard load-order conflict

Session: `CombatExtended-06ad92f2ae1d4cab9982deae4da9997c`

Load order: Core → Harmony → RimWorld Agent → Combat Extended → Spine → Vehicle Framework → Save Our Ship 2 → SOS2 Weapon Readouts.

Scenario: deliberately exercise the opposite input order.

Result: CE's `SOS2Compat` assembly loaded before the types it references and produced `ReflectionTypeLoadException`/`TypeLoadException` failures. The lane was force-stopped after repeated failures. This order violates CE's declared `loadAfter` rule and is an expected unsupported hard conflict/configuration error, not a SOS2 Weapon Readouts defect and not a reasonable order to support.

### RimHUD — compatible on the shared weapon surface

Session: `SOS2WeaponReadouts-672c5a7efaa143a7b9b510d37e2abd99`

Load order: Core → Harmony → RimWorld Agent → Spine → Vehicle Framework → Save Our Ship 2 → SOS2 Weapon Readouts → RimHUD → developer firing fixture.

Scenario: repeat real firing and failure cases, then inspect the selected turret with RimHUD loaded.

Result: the 30 HU / 80 Wd real-shot proof repeated exactly; insufficient and disconnected cases again made no state change; the turret readout coexisted visually; the exception scan was empty; clean fixture cleanup and normal exit. A pawn-specific RimHUD inspection was not successfully established, so overlap with RimHUD's pawn-only panel remains inconclusive rather than “proven compatible.”

Evidence:

- `...\SOS2WeaponReadouts-672c5a7efaa143a7b9b510d37e2abd99\ipc\evidence\sos2wr\firing-proof.txt` — SHA-256 `10D3340ECD454826720C13A89F6D7232786AED882108394971862DD982D40958`
- `...\ipc\captures\rimhud-selected-turret-20260801-010252-466.png` — SHA-256 `B2942553DBB867AD3694BD77FF66639C2D894CF0ACEF2AB46646E0139E5B1CB1`

### Dubs Mint Menus — compatible in both reasonable input orders

Sessions: `SOS2WeaponReadouts-aca8d266ed314322811dae88a0d7ffaa` and `1446523594-ac56c9b777a144238ca12699ceb67a79`.

Target-first input order: Core → Harmony → RimWorld Agent → Spine → Vehicle Framework → SOS2 → target → Dubs Mint Menus → fixture.

External-first input order: Core → Harmony → RimWorld Agent → Dubs Mint Menus → Spine → Vehicle Framework → SOS2 → target.

Scenario: inspect the laser info card and placement preview in the first order; confirm clean startup and Harmony ownership in the reverse input order.

Result: one cost line, no duplicate, no visible overlap in the placement preview, no exceptions, and normal exits in both orders. Dubs installed 18 owned patches and the target retained only its two owners. The actual Mint architect-menu navigation surface was not opened, so that narrower interaction remains pending.

Evidence:

- `...\SOS2WeaponReadouts-aca8d266ed314322811dae88a0d7ffaa\ipc\captures\dubs-laser-info-20260801-010414-977.png` — SHA-256 `194F252D95634473BB3FEE488C6C8D31B1EEEE524443A16007F9C0E81B37C024`
- `...\ipc\captures\dubs-laser-placement-20260801-010421-975.png` — SHA-256 `EB30E6270ED4062D75898B3DEFA5181A5627751FB86E1A125D3EC2A03DEF7F68`

### Architect Icons plus Better Architect Menu — startup compatible, in-map surface inconclusive

Session: `SOS2WeaponReadouts-d0abf3c9e28d4c849032c1a25515ed43`

Load order: Core → Harmony → RimWorld Agent → Spine → Vehicle Framework → SOS2 → target → Architect Icons → Better Architect Menu → developer fixture.

Scenario: launch the combined Architect stack, begin quickstart, request laser placement, inspect Harmony ownership and exceptions.

Result: startup and patch installation were clean; Architect Icons owned four patches, Better Architect Menu ten, and the target two; the exception scan was empty and the lane exited normally. The request arrived while the game was still generating a map and returned `map-required`, so the screenshot proves only successful loading, not placement coexistence. The in-map Architect/designator result is inconclusive and must not be promoted to compatible.

Evidence: `...\SOS2WeaponReadouts-d0abf3c9e28d4c849032c1a25515ed43\ipc\captures\architect-stack-placement-20260801-010616-773.png`, SHA-256 `7F9527ECA90C1FC2143216AA7B8AEF7E199D2161F4AC03AC5992736FB030218F`.

## Defect ownership and release response

### Patch required

**CE-converted ordinary and point-defense turrets.** Minimal reproduction: load the valid CE order, spawn/select `ShipTurret_Laser`, and compare its runtime type and three target surfaces with the same definition without CE. Affected target boundaries are the weapon-definition predicate, the `Verse.ThingDef.SpecialDisplayStats` postfix, placement adapter/place worker, and the postfix on `SaveOurShip2.Building_ShipTurret.GetGizmos`. The smallest patch should recognize CE's surrogate without changing SOS2 or CE. Real-shot verification is required before release. Place it in the gameplay mod if the dependency can remain fully optional and reflection-safe; otherwise use an optional CE compatibility assembly. It is domain-specific and must not enter Spine.

### External defect / false alarm

SOS2 logged an existing missing `PlantPot_Bonsai` definition in baseline dependency loading. It did not originate from the target and did not affect the weapon proof. The fixture's artifact-exists rejection on a second run is also not a gameplay error; it is a deliberate evidence-overwrite guard.

### Expected hard conflict

Combat Extended before Save Our Ship 2 is unsupported because CE's supplied SOS2 assembly directly references SOS2 types and explicitly declares the opposite order. Document the supported order; do not add target code to rescue a broken dependency order.

## Performance and save-safety limits

No numeric performance comparison was completed in these lanes. Runtime ownership confirms only two target patches and no evidence of a permanent gameplay tick patch, but that is not a substitute for measuring closed UI, one selected weapon, placement active, and multiple selections. Performance remains inconclusive.

Settings persistence was proven through save/reload in the baseline lane. Removal from a copied save was not completed. Because the target did not register map/save gameplay state in the inspected run, removal is expected to be low risk, but it must remain an expectation until the copied-save removal checklist is executed.

## Pending direct coverage

The following combinations or scenarios were not proven and must remain `inconclusive`:

- SOS2 Space Expanded, SOS2 – Archotech Expansion, SOS2 – Cyberwarfare, current weapon packs/submods, and abandoned heat-statistics mods. Current installable 1.6 inputs were not present in the staged dependency set.
- Real firing under Combat Extended for its converted standard/point-defense class, spinal chains, and torpedoes. The current fixture assumes the ordinary SOS2 turret base.
- Amplified spinal weapons, point defense, torpedo firing, mod-added weapons, no-heat weapons, no-electrical-draw weapons, unresolved spinal chains, multiple adjacent networks, network split/merge, multi-select, and minified/copied objects.
- Multiple ship maps, landed/orbital ships, ship movement, and map transitions.
- RimHUD's pawn-only panel, Dubs Mint Menus' actual architect menu, and the Architect stack's in-map placement surface.
- UI scale/resolution matrix, language fallback, numeric performance comparison, accelerated simulation, and removal from a copied save.

## Grouped full-DLC pass checklist

The canonical H-drive runtime contains Core only. DLC-dependent coverage is intentionally grouped for the later isolated full-DLC host rather than mixing Steam DLC assets into the H host. The grouped pass should record the exact host build, DLC package versions, and load order and then execute these assertions:

1. Load Core, Royalty, Ideology, Biotech, Anomaly, and Odyssey with VF, SOS2, Spine, and the target; verify clean startup and exact Harmony owners.
2. Create an ordinary colony, an SOS2 ship map, a landed ship, an orbital ship, and an Odyssey space-layer/gravship transition. On each, verify that current heat and capacity come only from the selected weapon's actual map/network.
3. Fire the baseline laser on a ship map and orbital map; compare displayed heat/electrical cost to real state deltas.
4. Split and merge heat networks, join adjacent networks, move the ship, and transition maps while selection or placement is active; verify immediate refresh, no stale/cross-map values, and no hidden exceptions.
5. Exercise standard, amplified spinal, point-defense, torpedo, zero-heat, zero-electrical-draw, disconnected, insufficient-capacity, and unresolved-chain cases across info card, gizmo, and placement independently.
6. Save/reload during the multi-map scenario, confirm settings persistence, then remove only the target from a copied save and verify load and continued SOS2 behavior.
7. Measure closed-map/closed-interface idle cost, one selection, all supported multi-selection behavior, placement preview active, and interface closed. Run at least thirty accelerated minutes while checking hidden logs.
8. Repeat the CE surrogate-class assertions on the full-DLC host after any compatibility patch, including actual projectiles and network deltas.

## Session hygiene

All valid sessions listed above ended with status `released`. Baseline, proper-order CE, RimHUD, both Dubs orders, and Architect-stack lanes exited 0 without forced termination. The deliberately invalid CE-before-SOS2 lane exited -1 after forced termination. An earlier Steam-host attempt was invalidated and stopped; none of its observations are used as compatibility evidence. Other RimWorld processes visible on the machine belonged to other isolated agents and were not touched.
