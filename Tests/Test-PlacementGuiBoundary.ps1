param()

$ErrorActionPreference = 'Stop'

$sourcePath = Join-Path `
    (Split-Path -Parent $PSScriptRoot) `
    'Source\UI\WeaponReadoutPlaceWorker.cs'
$source = [System.IO.File]::ReadAllText($sourcePath)

$failures = [System.Collections.Generic.List[string]]::new()
if ($source -notmatch
    'override\s+void\s+DrawPlaceMouseAttachments\s*\(')
{
    $failures.Add(
        'Placement readouts must use PlaceWorker.DrawPlaceMouseAttachments.')
}
if ($source -match 'override\s+void\s+DrawGhost\s*\(')
{
    $failures.Add(
        'PlaceWorker.DrawGhost runs outside OnGUI and must not draw readout UI.')
}
if ($source -match 'SelectedUpdate\s*\(')
{
    $failures.Add(
        'Placement readout UI must not run from a SelectedUpdate path.')
}
if ($source -match
    '\bGenMapUI\b|\bGUI\s*\.\s*DrawTexture\s*\(')
{
    $failures.Add(
        'Placement readouts must not call GenMapUI or GUI.DrawTexture.')
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
    'PASS: placement readout GUI is confined to ' +
    'PlaceWorker.DrawPlaceMouseAttachments.')
