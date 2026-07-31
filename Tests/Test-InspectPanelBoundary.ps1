param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $root 'Source'
$runtimePath = Join-Path `
    $root `
    'Source\Runtime\WeaponReadoutRuntime.cs'
$gizmoPath = Join-Path `
    $root `
    'Source\UI\WeaponReadoutGizmo.cs'
$languagePath = Join-Path `
    $root `
    'Languages\English\Keyed\SOS2WeaponReadouts.xml'

$productionSource = @(
    Get-ChildItem -LiteralPath $sourceRoot -Filter '*.cs' -File -Recurse |
        ForEach-Object {
            [System.IO.File]::ReadAllText($_.FullName)
        }) -join [Environment]::NewLine
$runtimeSource = [System.IO.File]::ReadAllText($runtimePath)
$gizmoSource = [System.IO.File]::ReadAllText($gizmoPath)
$languageSource = [System.IO.File]::ReadAllText($languagePath)

$failures = [System.Collections.Generic.List[string]]::new()
if ($productionSource -match
    'GetInspectString|AppendInspectReadout|TurretInspectPatch')
{
    $failures.Add(
        'Built-weapon readouts must not extend the inspect string.')
}
if ($productionSource -notmatch
    'TurretGizmosMethod|AppendSelectedWeaponGizmo')
{
    $failures.Add(
        'The SOS2 turret gizmo integration patch is missing.')
}
if ($gizmoSource -notmatch
    'override\s+GizmoResult\s+GizmoOnGUI')
{
    $failures.Add(
        'The selected-weapon readout must render as a legal GizmoOnGUI.')
}
if ($runtimeSource -notmatch
    'AppendSelectedWeaponGizmo')
{
    $failures.Add(
        'Runtime integration does not append the selected-weapon gizmo.')
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

if ($failures.Count -gt 0)
{
    foreach ($failure in $failures)
    {
        Write-Error $failure
    }
    exit 1
}

Write-Output (
    'PASS: built-weapon readouts use a current-heat gizmo without ' +
    'extending the inspect panel.')
