$ErrorActionPreference = 'Stop'

$fixtureRoot = Join-Path $PSScriptRoot `
    '..\Developer\SOS2WeaponReadouts.TestFixture\Source'
$extension = Get-Content -Raw -LiteralPath (
    Join-Path $fixtureRoot 'Sos2WeaponFiringFixtureExtension.cs')
$probe = Get-Content -Raw -LiteralPath (
    Join-Path $fixtureRoot 'FiringProbe.cs')
$handle = Get-Content -Raw -LiteralPath (
    Join-Path $fixtureRoot 'TestTurretHandle.cs')

if ($extension -match '\(Building_ShipTurret\)SpawnBuilding')
{
    throw 'The firing fixture still casts spawned weapons to the native SOS2 turret.'
}
if ($handle -notmatch 'Building_ShipTurretCE' -or
    $handle -notmatch 'TryGetComp<CompShipHeat>')
{
    throw 'The fixture handle does not recognize the CE surrogate through the shared heat component.'
}
if ($probe -notmatch 'Verb_ShootShipCE' -or
    $probe -notmatch 'Building_ShipTurretCE')
{
    throw 'The firing probe does not patch CE cast and BeginBurst boundaries.'
}

Write-Output 'PASS: developer firing fixture accepts native and CE SOS2 turret shapes without a production CE dependency.'
