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

Final result: `PASS: 13 SOS2 weapon readout contracts`.

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

## Controlled RimWorld 1.6 evidence

The final hardened DLL was exercised in isolated harness lane
`SOS2WeaponReadouts-07c64530436b4b9eadb88cb8513325e7`, which stopped cleanly
with exit code 0 and no forced termination.

- Active stack: Harmony, RimWorld Agent, Spine, Vehicle Framework, Save Our
  Ship 2, and SOS2 Weapon Readouts.
- Startup validated SOS2's 1.6 API and attached non-blocking placement
  readouts to 17 SOS2 weapon definitions.
- The laser information card visibly showed `SOS2 weapon costs: 30 HU / 80 Wd`.
- A real spawned laser, thermal conduits, 200 HU heatsink, and pilot console
  formed one SOS2 heat network. The final inspect pane visibly showed
  `Heat generated per shot: 30 HU`; the preceding run also showed
  `Network heat after shot: 30 / 200 HU`.
- SOS2's own `Energy to fire: 80 Wd` remained authoritative and was not
  duplicated.
- The settings window showed all five enabled display/compatibility controls.
- A normal save/load round trip completed with load generation 1 and the
  spawned turret persisted.
- A 600-tick synchronous sample completed in 0.283603 seconds
  (2115.63 ticks/second). Source inspection confirms no tick/update component
  or per-frame Harmony patch.

Captures:

- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-212bfc208f27495ba2040ccc5865e559\ipc\captures\sos2-laser-info-corrected-20260730-201138-866.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-212bfc208f27495ba2040ccc5865e559\ipc\captures\sos2-readout-settings-20260730-201326-536.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-212bfc208f27495ba2040ccc5865e559\ipc\captures\sos2-laser-connected-network-rebuilt-20260730-201805-308.png`
- `C:\Users\PrecisionX\AppData\Local\Temp\RimWorldAgentTasks\1.6\SOS2WeaponReadouts-07c64530436b4b9eadb88cb8513325e7\ipc\captures\sos2-final-hardened-connected-20260730-202558-428.png`

One dependency-owned startup error remains:
`Failed to find Verse.ThingDef named PlantPot_Bonsai`. The reference originates
inside SOS2; this mod does not suppress or modify dependency errors. The
previous false Vehicle Framework requirement error is absent.
