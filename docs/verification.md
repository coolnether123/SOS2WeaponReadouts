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

The fixture proof evaluator also covers successful launch evidence, exact
resource deltas, suppressed-fire behavior, and resource-mismatch rejection.

Final result: `PASS: 16 SOS2 weapon readout contracts`.

## Pinned SOS2 API contract

```powershell
powershell -ExecutionPolicy Bypass -File Tests\Test-Sos2Api.ps1 `
  -Sos2Assembly A:\Dev\RimWorld\Dependencies\SaveOurShip2\1.6\Assemblies\ShipsHaveInsides.dll `
  -VehicleAssemblies A:\Dev\RimWorld\Dependencies\Runtime\VehicleFramework\1.6\Assemblies `
  -ManagedAssemblies H:\Games\RimWorld1-6-4871Win64\RimWorldWin64_Data\Managed
```

This loads metadata from the actual staged assembly and verifies every type,
field, property, and method used by the compatibility adapter.

Final result: pass against SOS2 assembly SHA-256
`ACF42144F4340D24D63E2695FC5D6BC94BC48E14D7E158DF7B7E43D078EF2DAE`.

## Central build

```powershell
& A:\Dev\RimWorld\Worktrees\RimWorld-Tooling\phase-a\tools\Invoke-RimWorldBuild.ps1 `
  -Project A:\Dev\RimWorld\Mods\SOS2WeaponReadouts\Source\Mod.csproj `
  -Configuration Release -Version 1.6 -Engine DotNet `
  -OutputRoot <isolated-artifact-path> `
  -Dependency harmony,spine,vehicle-framework,save-our-ship-2
```

## Package validation

```powershell
Import-Module A:\Dev\RimWorld\Worktrees\RimWorld-Tooling\phase-a\modules\RimWorld.Tooling.Build\RimWorld.Tooling.Build.psd1
Test-RwtPackage `
  -ModRoot A:\Dev\RimWorld\Mods\SOS2WeaponReadouts `
  -Version 1.6 `
  -ExpectedAssemblyName SOS2WeaponReadouts
```

The final distributable was then staged through the shared allowlist-based
release command:

```powershell
New-RwtReleasePackage `
  -ModRoot A:\Dev\RimWorld\Mods\SOS2WeaponReadouts `
  -DestinationRoot A:\Dev\RimWorld\Releases\1.6\2026-07-30-program-final\SOS2WeaponReadouts `
  -Version 1.6 `
  -IncludePath About,1.6\Assemblies\SOS2WeaponReadouts.dll,Languages `
  -ExpectedAssemblyName SOS2WeaponReadouts
```

Final result: `RWT-BUILD-RELEASE-PACKAGE-VALID`. The staged folder contains
exactly three files: the production DLL, `About.xml`, and English keyed
translations. `Developer`, `Source`, `Tests`, `docs`, `Engineering`, and
`AGENTS.md` are absent. The complete five-mod release manifest is
`A:\Dev\RimWorld\Releases\1.6\2026-07-30-program-final\release-manifest.json`.
Its current SHA-256 is recorded in the generalized tooling status at
`A:\Dev\RimWorld\Worktrees\RimWorld-Tooling\phase-a\docs\verification\phase-a-status.json`;
the central record avoids a self-referential documentation commit changing the
source HEAD after packaging.

## Controlled RimWorld 1.6 evidence

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
- The production inspect readout showed those same per-shot values and the
  real network's post-shot comparison.
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
`C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-99829488b1ac4ba28cc0b8c334081434\ipc\evidence\sos2wr\firing-proof.txt`
with SHA-256
`C240520A0D50EA232C1A24B00EEC783EBCCE0EC5C817E1C3CD56D92FE2C1A735`.
The selected-turret information-card capture is
`C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-99829488b1ac4ba28cc0b8c334081434\ipc\captures\sos2wr-final-real-fire-info-20260730-221554-448.png`
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

- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-laser-20260730-205454-973.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-railgun-20260730-205459-037.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-info-plasma-20260730-205503-306.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-insufficient-capacity-20260730-205658-980.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-capacity-transition-20260730-205758-594.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-8049c41a740e4c23b16768b59d3be015\ipc\captures\clean-after-load-20260730-205831-184.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-4306cf88364e468faa34f1bce3a4eb6f\ipc\captures\final-rebuilt-spine-smoke-laser-ready-20260730-211012-968.png`

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

## Review limitations

- The insufficient-capacity formatter branch is covered by a pure contract.
  Runtime firing evidence exercises SOS2's actual insufficient-power
  suppression, but the harness does not capture every scroll position of the
  inspect pane.
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
