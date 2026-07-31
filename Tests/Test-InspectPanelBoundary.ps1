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
if ($gizmoSource -notmatch
    'DataRowHeight\s*=\s*(?<height>[0-9.]+)f' -or
    [float]$Matches['height'] -lt 18)
{
    $failures.Add(
        'Gizmo data rows must leave at least 18 pixels for font descenders.')
}
if ($gizmoSource -match
    'labels\.ElectricalDrawPerShot|readout\.ElectricalDrawPerShot')
{
    $failures.Add(
        'The selected-weapon gizmo must not repeat SOS2 electrical draw.')
}
if ($gizmoSource -notmatch
    'ShowElectricalDraw\s*=\s*false')
{
    $failures.Add(
        'The selected-weapon tooltip must suppress duplicate electrical draw.')
}
if ($gizmoSource -match
    'Gizmo\.Title|SOS2 weapon readout(?!s)' -or
    $languageSource -match
    'SOS2WR\.Gizmo\.Title|>SOS2 weapon readout<')
{
    $failures.Add(
        'The selected SOS2 gun must not repeat a redundant mod title.')
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
