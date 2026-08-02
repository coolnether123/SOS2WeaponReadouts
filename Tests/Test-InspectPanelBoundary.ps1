param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $root 'Source'
$runtimePath = Join-Path `
    $root `
    'Source\Runtime\WeaponReadoutRuntime.cs'
$inspectPatchPath = Join-Path `
    $root `
    'Source\Patches\HeatInspectStringPatch.cs'
$adapterPath = Join-Path `
    $root `
    'Source\Compatibility\Sos2V16Adapter.cs'
$gizmoPath = Join-Path $root 'Source\UI\WeaponReadoutGizmo.cs'
$gizmoPatchPath = Join-Path `
    $root `
    'Source\Patches\TurretGizmosPatch.cs'
$languagePath = Join-Path `
    $root `
    'Languages\English\Keyed\SOS2WeaponReadouts.xml'

$productionSource = @(
    Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' -File -Recurse |
        ForEach-Object {
            [System.IO.File]::ReadAllText($_.FullName)
        }) -join [Environment]::NewLine
$runtimeSource = [System.IO.File]::ReadAllText($runtimePath)
$inspectPatchSource = [System.IO.File]::ReadAllText($inspectPatchPath)
$adapterSource = [System.IO.File]::ReadAllText($adapterPath)
$inspectMethodSource = [regex]::Match(
    $runtimeSource,
    'AppendHeatPerShotToInspectString[\s\S]*?' +
    '(?=public\s+static\s+bool\s+TryCreatePlacementReadout)').Value
$languageSource = [System.IO.File]::ReadAllText($languagePath)

$failures = [System.Collections.Generic.List[string]]::new()
if ($productionSource -match
    '\bGetInspectString\b|AppendInspectReadout|TurretInspectPatch')
{
    $failures.Add(
        'Built-weapon readouts must not patch Thing.GetInspectString.')
}
if ((Test-Path -LiteralPath $gizmoPath) -or
    (Test-Path -LiteralPath $gizmoPatchPath) -or
    $productionSource -match
    'TurretGizmosMethods|AppendSelectedWeaponGizmo|WeaponReadoutGizmo')
{
    $failures.Add(
        'The removed selected-weapon gizmo path must not return.')
}
if ($runtimeSource -notmatch
    'AppendHeatPerShotToInspectString' -or
    $inspectPatchSource -notmatch
    'HeatInspectStringMethod' -or
    $adapterSource -notmatch
    'CompInspectStringExtra')
{
    $failures.Add(
        'The native SOS2 heat line integration is missing.')
}
if ($runtimeSource -notmatch
    'ExecuteWhenFinished\s*\(\s*\(\)\s*=>' -or
    $runtimeSource -notmatch
    'ExecuteWhenFinished[\s\S]{0,500}PatchAll')
{
    $failures.Add(
        'Harmony patching must wait for the main-thread long-event boundary.')
}
if ($inspectMethodSource -match
    'Environment\.NewLine|"\\n"|"\\r"')
{
    $failures.Add(
        'The selected-weapon suffix must not add an inspect-panel line.')
}
if ($languageSource -match
    'HeatAfterShot|Network heat after shot')
{
    $failures.Add(
        'Player-facing post-shot heat wording must not return.')
}
if ($languageSource -notmatch
    'Current network heat')
{
    $failures.Add(
        'The current network heat label is missing.')
}
if ($languageSource -notmatch
    '<SOS2WR\.Readout\.HeatPerShotCompact>\(\+\{0\}/shot\)' -or
    $languageSource -match
    'HeatPerShotCompact>[^<]*HU')
{
    $failures.Add(
        'The compact suffix must reuse the HU context and stay width-bounded.')
}

if ($failures.Count -gt 0)
{
    foreach ($failure in $failures)
    {
        Write-Error $failure
    }
    exit 1
}

Write-Output (
    'PASS: selected weapons decorate SOS2''s heat line with a compact ' +
    'per-shot suffix; the old gizmo path is absent.')
