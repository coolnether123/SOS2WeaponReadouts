param(
    [Parameter(Mandatory = $true)]
    [string]$Sos2Assembly,

    [Parameter(Mandatory = $true)]
    [string]$VehicleAssemblies,

    [Parameter(Mandatory = $true)]
    [string]$ManagedAssemblies
)

$ErrorActionPreference = 'Stop'

function Assert-Member
{
    param(
        [Parameter(Mandatory = $true)][Type]$Type,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Field', 'Property', 'Method')]
        [string]$Kind
    )

    $flags = [Reflection.BindingFlags]'Public,NonPublic,Instance'
    $member = switch ($Kind)
    {
        'Field' { $Type.GetField($Name, $flags) }
        'Property' { $Type.GetProperty($Name, $flags) }
        'Method'
        {
            $Type.GetMethod(
                $Name,
                $flags,
                $null,
                [Type[]]@(),
                $null)
        }
    }

    if ($null -eq $member)
    {
        throw "SOS2 API missing $Kind $($Type.FullName).$Name"
    }
}

$sos2Path = [IO.Path]::GetFullPath($Sos2Assembly)
$vehicleRoot = [IO.Path]::GetFullPath($VehicleAssemblies)
$managedRoot = [IO.Path]::GetFullPath($ManagedAssemblies)
foreach ($path in @($sos2Path, $vehicleRoot, $managedRoot))
{
    if (-not (Test-Path -LiteralPath $path))
    {
        throw "Required API probe path does not exist: $path"
    }
}

$sos2Root = Split-Path -Parent $sos2Path
$resolver = [ResolveEventHandler] {
    param($sender, $eventArgs)

    $fileName = ([Reflection.AssemblyName]$eventArgs.Name).Name + '.dll'
    foreach ($root in @($sos2Root, $vehicleRoot, $managedRoot))
    {
        $candidate = Join-Path $root $fileName
        if (Test-Path -LiteralPath $candidate -PathType Leaf)
        {
            return [Reflection.Assembly]::ReflectionOnlyLoadFrom(
                $candidate)
        }
    }

    return $null
}

[AppDomain]::CurrentDomain.add_ReflectionOnlyAssemblyResolve($resolver)
try
{
    $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom($sos2Path)
    $expected = @{
        Turret = 'SaveOurShip2.Building_ShipTurret'
        HeatComp = 'SaveOurShip2.CompShipHeat'
        HeatProperties = 'SaveOurShip2.CompProps_ShipHeat'
        HeatNetwork = 'SaveOurShip2.ShipHeatNet'
    }
    $types = @{}
    foreach ($key in $expected.Keys)
    {
        $type = $assembly.GetType($expected[$key], $false, $false)
        if ($null -eq $type)
        {
            throw "SOS2 API missing type $($expected[$key])"
        }
        $types[$key] = $type
    }

    foreach ($name in @(
        'heatComp',
        'AmplifierCount',
        'spinalComp'))
    {
        Assert-Member $types.Turret $name Field
    }
    foreach ($name in @(
        'ConnectedToBridge',
        'HeatToFire',
        'EnergyToFire'))
    {
        Assert-Member $types.Turret $name Property
    }
    Assert-Member $types.Turret 'GetGizmos' Method

    Assert-Member $types.HeatComp 'myNet' Field
    Assert-Member $types.HeatComp 'Props' Property
    Assert-Member $types.HeatProperties 'heatPerPulse' Field
    Assert-Member $types.HeatProperties 'energyToFire' Field

    foreach ($name in @('PilCons', 'AICores', 'TacCons'))
    {
        Assert-Member $types.HeatNetwork $name Field
    }
    foreach ($name in @('StorageCapacity', 'StorageUsed'))
    {
        Assert-Member $types.HeatNetwork $name Property
    }

    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $sos2Path).Hash
    Write-Output (
        "PASS: SOS2 1.6 adapter API shape; assemblySha256=$hash")
}
finally
{
    [AppDomain]::CurrentDomain.remove_ReflectionOnlyAssemblyResolve(
        $resolver)
}
