# Verification record

This file records reproducible commands and the final controlled-harness
evidence.

## Pure contracts

```powershell
dotnet run --project Tests\Mod.Tests.csproj -c Release
```

Coverage includes the SOS2 `×3` heat formula, amplifier scaling, zero-cost
weapons, semantic future/current-SOS2 deduplication, disconnected and missing
bridge states, post-shot capacity, insufficient capacity, unresolved spinal
values, display toggles, deterministic number formatting, and the regression
where separate SOS2 grid-heat and energy lines must not hide heat per shot.
It also verifies the compact per-shot suffix is merged into a generated
network line and recognized semantically on later passes. A three-digit
layout contract bounds the full pinned English SOS2 line to 61 characters and
requires `(+999/shot)` to be no wider than the previously proven live suffix.

The fixture proof evaluator also covers successful launch evidence, exact
resource deltas, suppressed-fire behavior, and resource-mismatch rejection.

Final result: `PASS: 19 SOS2 weapon readout contracts`.

## Placement GUI boundary

```powershell
powershell -ExecutionPolicy Bypass -File Tests\Test-PlacementGuiBoundary.ps1
```

This structural regression gate requires placement readouts to use
`PlaceWorker.DrawPlaceMouseAttachments` and fails if the worker regains a
`DrawGhost` or `SelectedUpdate` path, `GenMapUI`, or `GUI.DrawTexture`.

Final result:
`PASS: placement readout GUI is confined to PlaceWorker.DrawPlaceMouseAttachments.`

## Inspect-panel boundary

```powershell
powershell -ExecutionPolicy Bypass -File Tests\Test-InspectPanelBoundary.ps1
```

This structural regression gate scans all production C# source. It requires
the adapter-owned `CompShipHeat.CompInspectStringExtra` boundary, main-thread
patch installation, and complete absence of the removed selected-gizmo path.
It also rejects a broad `Thing.GetInspectString` patch, extra inspect lines,
or restored player-facing "heat after shot" wording.

Final result: `PASS: selected weapons decorate SOS2's heat line with a compact
per-shot suffix; the old gizmo path is absent.`

## Pinned SOS2 API contract

```powershell
powershell -ExecutionPolicy Bypass -File Tests\Test-Sos2Api.ps1 `
  -Sos2Assembly <sos2-source-root>\1.6\Assemblies\ShipsHaveInsides.dll `
  -VehicleAssemblies <vehicle-framework-root>\1.6\Assemblies `
  -ManagedAssemblies <rimworld-install>\RimWorldWin64_Data\Managed
```

This loads metadata from the actual staged assembly and verifies every type,
field, property, and method used by the compatibility adapter, including
`CompInspectStringExtra`. The optional CE probe verifies the surrogate's
`heatComp`, `HeatToFire`, and `EnergyToFire` members.

Final result: pass against SOS2 assembly SHA-256
`ACF42144F4340D24D63E2695FC5D6BC94BC48E14D7E158DF7B7E43D078EF2DAE`.

## Central build

```powershell
& <rimworld-tooling-root>\tools\Invoke-RimWorldBuild.ps1 `
  -Project <repo-root>\Source\Mod.csproj `
  -Configuration Release -Version 1.6 -Engine DotNet `
  -OutputRoot <isolated-artifact-path> `
  -Dependency harmony,spine,vehicle-framework,save-our-ship-2
```

## Package validation

```powershell
Import-Module <rimworld-tooling-root>\modules\RimWorld.Tooling.Build\RimWorld.Tooling.Build.psd1
Test-RwtPackage `
  -ModRoot <repo-root> `
  -Version 1.6 `
  -ExpectedAssemblyName SOS2WeaponReadouts
```

The final distributable was then staged through the shared allowlist-based
release command:

```powershell
New-RwtReleasePackage `
  -ModRoot <repo-root> `
  -DestinationRoot <release-root>\1.6\2026-07-30-program-final\SOS2WeaponReadouts `
  -Version 1.6 `
  -IncludePath About,1.6\Assemblies\SOS2WeaponReadouts.dll,Languages `
  -ExpectedAssemblyName SOS2WeaponReadouts
```

Final result: `RWT-BUILD-RELEASE-PACKAGE-VALID`. The staged folder contains
exactly three files: the production DLL, `About.xml`, and English keyed
translations. `Developer`, `Source`, `Tests`, `docs`, `Engineering`, and
`AGENTS.md` are absent. The complete five-mod release manifest is
`<release-root>\1.6\2026-07-30-program-final\release-manifest.json`.
Its current SHA-256 is recorded in the generalized tooling status at
`<rimworld-tooling-root>\docs\verification\phase-a-status.json`;
the central record avoids a self-referential documentation commit changing the
source HEAD after packaging.

## Controlled RimWorld 1.6 evidence

### Placement OnGUI regression

Final isolated lane
`SOS2WeaponReadouts-ea0865c2c2484285a3516382004eee2a` loaded the production
mod plus the separate developer fixture. After the fixture built and fired its
real SOS2 laser network, its developer-only `select-placement
ShipTurret_Laser` action selected RimWorld's ordinary `Designator_Build`.
With the pointer over the connected fixture, the historical placement ghost visibly
showed `Heat generated per shot: 30 HU`, `Electrical draw per shot: 80 Wd`,
and `Network heat after shot: 30.11 / 200 HU`.

That capture predates the current-heat UI revision. It remains valid evidence
for the legal OnGUI placement boundary only; its projected-heat wording is
superseded and is not accepted as current player-facing UI evidence.

The in-game and output-log scans contained zero exception matches and zero
matches for `You can only call GUI functions from inside OnGUI`. The only
pre-shutdown error remained SOS2's known missing `PlantPot_Bonsai` definition.
Cleanup reported
`removed=157; cacheRemoved=True; shipsRemaining=0; cleanupErrors=0`, no
detach warning appeared, and map inspection found zero remaining laser
turrets. A post-cleanup 600-tick sample completed in `0.256683` seconds
(`2337.52 ticks/second`). The lane stopped normally with exit code 0 and no
forced termination.

Capture:
`<harness-evidence-root>\SOS2WeaponReadouts-ea0865c2c2484285a3516382004eee2a\ipc\captures\placement-readout-legal-ongui-20260731-013045-278.png`

Capture SHA-256:
`0451D23B9819B48A6288C6B07E4383A3B48A7E408A88194F8071A00F24AB0C69`

### Real SOS2 firing proof

Final isolated lane
`SOS2WeaponReadouts-99829488b1ac4ba28cc0b8c334081434` loaded the production
mod plus the separate developer fixture. The fixture built a valid 150-cell
SOS2 hull with a computer core, connected laser, heat sink, capacitor, and a
separate thermally disconnected laser network.

- Connected fire observed one `Building_ShipTurret.BeginBurst`, one
  `Verb_LaunchProjectileShip.TryCastShot`, and one actual
  `Projectile.Launch` of `Bullet_Ground_Laser`.
- Heat changed from `0` to `30 HU`; stored power changed from `999.894` to
  `919.894 Wd`, exactly matching the weapon's `30 HU / 80 Wd` firing costs.
- The then-current production inspect readout showed those same per-shot
  values. That UI path is historical and has since been replaced by the
  compact suffix on SOS2's existing network-heat line.
- With only `78.925 Wd` available, the turret entered `BeginBurst` but produced
  zero casts, zero projectile launches, zero heat increase, and zero
  weapon-scale power draw.
- The disconnected turret produced zero bursts, casts, launches, or heat and
  reported the missing bridge/core state.
- Cleanup reported
  `removed=157; cacheRemoved=True; shipsRemaining=0; cleanupErrors=0`.
  No `Ship was detached from bridge` warning was emitted, and post-cleanup
  map inspection found zero laser turrets and zero hull tiles.
- A clean save/load completed at load generation 1, stayed paused, contained
  no fixture turret, and reset fixture status for the loaded map.
- A post-cleanup 600-tick sample completed in `0.207521` seconds
  (`2891.27 ticks/second`).

The firing artifact is
`<harness-evidence-root>\SOS2WeaponReadouts-99829488b1ac4ba28cc0b8c334081434\ipc\evidence\sos2wr\firing-proof.txt`
with SHA-256
`C240520A0D50EA232C1A24B00EEC783EBCCE0EC5C817E1C3CD56D92FE2C1A735`.
The selected-turret information-card capture is
`<harness-evidence-root>\SOS2WeaponReadouts-99829488b1ac4ba28cc0b8c334081434\ipc\captures\sos2wr-final-real-fire-info-20260730-221554-448.png`
with SHA-256
`843852915DC7C48BC7CA5C12B3D306827E29023953E984F45BA39D901F0B4320`.
The lane stopped normally with exit code 0 and no forced termination.

Production-only removal lane
`SOS2WeaponReadouts-bc3ed2fdd65b4f7c9451bc2bea1b7df0` omitted the fixture
path. Its active-mod list contained SOS2 Weapon Readouts but not the test
fixture, the `mod-fixtures` category was empty, and invoking
`sos2wr-firing-fixture` returned `unknown tool`. This confirms the fixture is
not discoverable in an ordinary production profile. The lane stopped normally
with exit code 0 and no forced termination.

The committed DLL was re-exercised in isolated harness lane
`SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015`. The lane stopped with
exit code 0 and no forced termination.

After Spine was hardened, final dependency smoke lane
`SOS2WeaponReadouts-4306cf88364e468faa34f1bce3a4eb6f` staged Spine commit
`650fb95835d187777fae314e1de361b8991b33ee` and DLL SHA-256
`2441959E82AA5CAC5C96E7456213B21D1FB67881E314F85F54373A4DB8C0E2AA`.
It also staged the rebuilt SOS2 Weapon Readouts DLL with SHA-256
`7C8295D45273CBCDC8C44183DD9016FDEB2AAF307D3D083823E706310AF1D971`.
The laser card still visibly showed `30 HU / 80 Wd`; the pre-shutdown scan
again contained no runtime exception beyond the known SOS2 missing-def line.
This final lane stopped with exit code 0 and no forced termination.

- Active stack: Harmony, RimWorld Agent, Spine, Vehicle Framework, Save Our
  Ship 2, and SOS2 Weapon Readouts.
- Startup validated SOS2's 1.6 API and attached non-blocking placement
  readouts to 17 SOS2 weapon definitions.
- Information cards covered three representative SOS2 weapon families:
  laser `30 HU / 80 Wd`, railgun `15 HU / 60 Wd`, and plasma
  `45 HU / 50 Wd`. A spinal capacitor, which is not itself a weapon, remained
  unchanged.
- A valid fixture used only a real laser, thermal conduit, and heatsink. The
  connected SOS2 network changed from 0 HU capacity to 200 HU capacity after
  the heatsink was added. No bridge or pilot console was generic-spawned.
- SOS2's own `Energy to fire: 80 Wd` remained authoritative and was not
  duplicated.
- A normal save/load round trip completed with load generation 1 and the
  spawned turret persisted.
- A 600-tick synchronous sample completed in 0.265391 seconds
  (2260.82 ticks/second). Source inspection confirms no tick/update component
  or per-frame Harmony patch.

Captures:

- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-laser-20260730-205454-973.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-railgun-20260730-205459-037.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-plasma-20260730-205503-306.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-insufficient-capacity-20260730-205658-980.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-capacity-transition-20260730-205758-594.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-after-load-20260730-205831-184.png`
- `<harness-evidence-root>\SOS2WeaponReadouts-4306cf88364e468faa34f1bce3a4eb6f\ipc\captures\final-rebuilt-spine-smoke-laser-ready-20260730-211012-968.png`

The pre-shutdown error/exception scan contains only the dependency-owned
`Failed to find Verse.ThingDef named PlantPot_Bonsai` startup line. The
reference originates inside SOS2; this mod does not suppress or modify
dependency errors. There are no SOS2 Weapon Readouts exceptions, Harmony patch
failures, or Vehicle Framework requirement errors. RimWorld also reports a
nonfatal metadata warning because the local-only Spine dependency has no
public download URL. After orderly shutdown, Vehicle Framework's SmashTools
worker logs a `ThreadAbortException` while the runtime tears down its
dedicated thread; it occurs after the harness shutdown request, not during the
test.

### Historical current-heat gizmo and descender proof

Commit `d50085fa0e97f7ba517570310dab919301823eb0` removes the
built-weapon inspect-string patch and appends a fixed-height selected-turret
gizmo instead. Its data rows reserve 18 pixels for RimWorld's Tiny font. The
clean centralized build reproduced shipping DLL SHA-256
`E322069CE9DABDD7CAF2E85A21F8C998E36A05BE15CA2A533BC39E08F1DA23CE`.

Combined four-mod lane
`SOS2WeaponReadouts-2e992ce2752649e595585a7a8cdc5ab0` loaded Filter Signals,
Prisoner Interaction Timer, SOS2 Weapon Readouts, and Faction Lens
with developer mode enabled. The real firing fixture passed. With its
connected laser selected, the gizmo visibly showed current heat/capacity,
heat generated per shot, and electrical draw per shot. The bottom-left inspect
panel contained only SOS2's existing information and had no scrollbar. A
zoomed review confirmed the `g` descender in “generated” was intact.

Capture:
`<harness-evidence-root>\SOS2WeaponReadouts-2e992ce2752649e595585a7a8cdc5ab0\ipc\captures\sos2-current-heat-gizmo-descenders-final-20260731-022714-293.png`

Capture SHA-256:
`3BC8FFCF9B80E61D7E33097B98E01599392C79F66FE599AD442FB9CE45A16964`

The live log contained zero `Exception in UIRootUpdate`, illegal-OnGUI,
SOS2 Weapon Readouts exception, or Harmony-failure matches.

This section predates the compact native-line integration below. It is kept as
historical evidence only; the custom gizmo renderer and patch were removed.

### Compact native heat-line proof

The centralized Release build produced shipping DLL SHA-256
`6D5D13BAD03E394B48A75E3B353A90DE00F8D8E7D149A26CBCBB836AAD137936`.
Package validation returned `RWT-BUILD-PACKAGE-VALID` for RimWorld 1.6.

Full-stack CE lane
`SOS2WeaponReadouts-81f3da5ca6aa4c67829e1c35bbc7b45f` loaded Spine, all four
new gameplay mods, Better Work Tab, Vehicle Framework, SOS2, and Combat
Extended. Harmony reported three SOS2 Weapon Readouts-owned patched methods,
down from five after the native and CE gizmo patches were removed.
The selected `CombatExtended.Compatibility.SOS2Compat.Building_ShipTurretCE`
laser displayed SOS2's existing first line as
`Grid heat stored/capacity: 0 HU / 0 HU (0) (+30/shot)`. No additional
readout gizmo or line was present.

Capture:
`<harness-evidence-root>\SOS2WeaponReadouts-81f3da5ca6aa4c67829e1c35bbc7b45f\ipc\captures\bounded-heat-suffix-clean-final-20260801-172150-657.png`

Capture SHA-256:
`6667B92A90381DF9E52D36C244AB9AA7CE131F68159633C2E8D466B968043CD7`

Parallel native lane
`SOS2WeaponReadouts-f8b59e5375b6497e91596135f7392ca6` loaded the same suite
without CE. The spawned class was `SaveOurShip2.Building_ShipTurret`, and its
existing heat line carried the same `(+30 HU/shot)` suffix with no added gizmo
or line.

That native capture used the earlier, longer suffix and therefore remains a
conservative width proof. The native/CE shape probe and shared
`CompShipHeat.CompInspectStringExtra` patch confirm the shorter current suffix
uses the same native boundary.

Capture:
`<harness-evidence-root>\SOS2WeaponReadouts-f8b59e5375b6497e91596135f7392ca6\ipc\captures\compact-native-heat-line-final-20260801-160621-658.png`

Capture SHA-256:
`C02EAE08195AE924FCD36C736EB91E63CC795F1FEE5FDC740F3BCAE64D34721D`

The native and latest CE lanes had zero patch-install failures, zero
off-main-thread graphic loads, and zero target-mod exception matches. The one
`PlantPot_Bonsai` match per lane is the already documented external SOS2
definition defect. The native-only lane stopped normally; the lean CE
full-stack lane was intentionally left open for interactive inspection.

In the live CE lane, the renamed `Show heat per shot on selected weapons`
checkbox changed from on to off through the ordinary settings UI; the suffix
disappeared immediately while SOS2's native heat line remained. Restoring the
checkbox brought the suffix back without reselecting the gun or reopening the
game.

## Review limitations

- The insufficient-capacity formatter branch is covered by a pure contract.
  Runtime firing evidence exercises SOS2's actual insufficient-power
  suppression. The inspect-panel boundary test mechanically limits selected-
  weapon integration to a suffix on SOS2's first component line and prevents
  this mod from adding another inspect line.
- Missing SOS2 is prevented by the declared required dependency. The dormant
  fallback adapter returns no readouts when SOS2 is inactive, and incompatible
  API shape is caught before patch installation. These paths were inspected
  but were not tested by shipping a deliberately missing or corrupted
  dependency.

An earlier lane,
`SOS2WeaponReadouts-07c64530436b4b9eadb88cb8513325e7`, is explicitly rejected
as release evidence. It generic-spawned `ShipPilotSeatMini` without the ship
state SOS2 requires and produced `Building_ShipBridge.GetInspectString` and
`Tick` null-reference exceptions. None of its captures support this release.

## Release-candidate revalidation — 2026-08-02

The final centralized build completed with zero warnings and zero errors.
`Test-RwtPackage` returned `RWT-BUILD-PACKAGE-VALID`. The production folder
contains one 35,328-byte `SOS2WeaponReadouts.dll`, SHA-256
`E695108534C9D313232D4B70AC58DC57792FD7B1E622ACD1D2464958E8D77400`.
The developer-only firing fixture was rebuilt separately with SHA-256
`A388E1BA100FF63EED4AE7303AA1B55424C865A12B097A2965183DEA585978C5`;
it is not part of the release package.

Current full-stack lane
`coolnether-suite-1d0030fad02c44449ce4fbf96ac7908a` exercised a real connected
SOS2 laser. `BeginBurst`, `CastShot`, and `Projectile.Launch` each occurred;
network heat changed from 0 to 30 HU and stored energy changed by exactly
80 Wd. The insufficient-power and disconnected controls launched no projectile
and changed no heat. The fixture result was `PASS`; its artifact SHA-256 was
`CDF98C849E92536B2DF1A7232710970777A444E2D9FA63D808C53E4DA1325EA6`.

The selected connected weapon displayed the compact `(+30/shot)` suffix on
SOS2's existing `Grid heat stored/capacity` line with no additional line,
gizmo, scrollbar, or overlap.

Capture:
`<harness-evidence-root>\coolnether-suite-1d0030fad02c44449ce4fbf96ac7908a\ipc\captures\old-four-rc-sos2-current-shot-20260802-225602-636.png`

Capture SHA-256:
`DC296CF3933A13128DD2CD4375FAD9842D72EE2FF0D2DFD52A117071D08AC649`.

Cleanup passed with `cacheRemoved=True`, `shipsRemaining=0`, and
`cleanupErrors=0`. The final pre-shutdown scan found no matching Player.log
error, and the harness stopped normally without forced termination.
