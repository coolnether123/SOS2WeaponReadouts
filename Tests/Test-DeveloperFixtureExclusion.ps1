$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$buildManifest = Join-Path $repoRoot 'Engineering\build.json'
$manifest = Get-Content -LiteralPath $buildManifest -Raw | ConvertFrom-Json
$includePaths = @($manifest.releaseIncludePaths)

if ($includePaths | Where-Object {
        $_ -match '(^|[\\/])Developer([\\/]|$)' -or
        $_ -match 'HistoricalFixture'
    })
{
    throw 'Release allowlist includes the developer historical fixture.'
}

$fixtureAbout = Join-Path `
    $repoRoot `
    'Developer\SOS2WeaponReadouts.HistoricalFixture\About\About.xml'
if (-not (Test-Path -LiteralPath $fixtureAbout -PathType Leaf))
{
    throw 'Developer fixture About.xml is missing.'
}

Write-Output 'PASS: release allowlist excludes the historical developer fixture.'
