[CmdletBinding()]
param(
    [string[]] $ModName = @(),
    [string] $SpaceEngineersRoot = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers',
    [string] $OutputRoot = (Join-Path ([System.IO.Path]::GetTempPath()) 'SE-BeneathTheCrustScriptCompile'),
    [switch] $KeepOutput
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-ModNames {
    param([string[]] $Names)

    $aliases = @{
        'Working-Knowledge' = 'WorkingKnowledge'
        'Working Knowledge' = 'WorkingKnowledge'
        'WkKn' = 'WorkingKnowledge'
        'Worldwrite' = 'Worldwright'
        'Worldwright Scenario Tools' = 'Worldwright'
        'Ww' = 'Worldwright'
    }

    $resolved = New-Object 'System.Collections.Generic.List[string]'
    foreach ($name in $Names) {
        if ($aliases.ContainsKey($name)) {
            $resolved.Add($aliases[$name])
        }
        else {
            $resolved.Add($name)
        }
    }

    return $resolved
}

function Get-ReferencePath {
    param(
        [Parameter(Mandatory = $true)][string] $Bin64,
        [Parameter(Mandatory = $true)][string] $Name
    )

    $path = Join-Path $Bin64 $Name
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required Space Engineers assembly was not found: $path"
    }

    return $path
}

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$modsRoot = Join-Path $repoRoot 'mods'
$bin64 = Join-Path $SpaceEngineersRoot 'Bin64'
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'

if (-not (Test-Path -LiteralPath $modsRoot)) {
    throw "Could not find mods folder: $modsRoot"
}

if (-not (Test-Path -LiteralPath $bin64)) {
    throw "Could not find Space Engineers Bin64 folder: $bin64"
}

if (-not (Test-Path -LiteralPath $csc)) {
    throw "Could not find .NET Framework C# compiler: $csc"
}

$mods = Get-ChildItem -LiteralPath $modsRoot -Directory | Where-Object {
    Test-Path -LiteralPath (Join-Path $_.FullName 'Data\Scripts')
}

if ($ModName.Count -gt 0) {
    $requestedNames = Resolve-ModNames -Names $ModName
    $requested = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $requestedNames) {
        [void] $requested.Add($name)
    }

    $mods = $mods | Where-Object { $requested.Contains($_.Name) }

    foreach ($name in $requested) {
        if (-not ($mods | Where-Object { $_.Name -ieq $name })) {
            throw "Requested mod script folder was not found under mods/: $name"
        }
    }
}

if (-not $mods) {
    throw "No mod script folders found under: $modsRoot"
}

$referenceNames = @(
    'Sandbox.Common.dll',
    'Sandbox.Game.dll',
    'Sandbox.Graphics.dll',
    'SpaceEngineers.Game.dll',
    'SpaceEngineers.ObjectBuilders.dll',
    'VRage.dll',
    'VRage.Game.dll',
    'VRage.Input.dll',
    'VRage.Library.dll',
    'VRage.Math.dll',
    'VRage.Render.dll',
    'ProtoBuf.Net.dll',
    'netstandard.dll'
)

$references = foreach ($name in $referenceNames) {
    '/reference:' + (Get-ReferencePath -Bin64 $bin64 -Name $name)
}

if (Test-Path -LiteralPath $OutputRoot) {
    Remove-Item -LiteralPath $OutputRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputRoot -Force | Out-Null

try {
    foreach ($mod in $mods) {
        $scriptsRoot = Join-Path $mod.FullName 'Data\Scripts'
        $sources = Get-ChildItem -LiteralPath $scriptsRoot -Recurse -Filter *.cs | Select-Object -ExpandProperty FullName
        if (-not $sources) {
            Write-Host "No C# script files found for $($mod.Name); skipping."
            continue
        }

        $prohibitedSymbols = @(
            'Sandbox.Game.Entities.Cube.MySlimBlock',
            'Sandbox.Game.World.MySession',
            'CreativeToolsEnabled',
            'DecreaseMountLevelToDesiredRatio',
            'GetStockpileStamp',
            'VRage.Audio.MyGuiSounds',
            'MyGuiSounds',
            'VRage.Game.Entity.MyInventoryBase',
            'Sandbox.Game.Gui.MyHud',
            'MyHud.Chat',
            'MyHudChat',
            'MyHudControlChat',
            'MyChatVisibilityEnum',
            'System.Reflection',
            'BindingFlags',
            'PropertyInfo',
            'FieldInfo',
            'Type.GetType'
        )

        foreach ($symbol in $prohibitedSymbols) {
            $matches = Select-String -LiteralPath $sources -SimpleMatch -Pattern $symbol
            if ($matches) {
                foreach ($match in $matches) {
                    Write-Error "$($match.Path):$($match.LineNumber): known in-game mod whitelist rejection: $symbol"
                }

                throw "Known prohibited Space Engineers mod API symbol found in $($mod.Name): $symbol"
            }
        }

        $out = Join-Path $OutputRoot ($mod.Name + '.ScriptCompile.dll')
        & $csc /nologo /target:library /out:$out $references $sources
        if ($LASTEXITCODE -ne 0) {
            throw "Script compile failed for $($mod.Name)."
        }

        if ($KeepOutput) {
            Write-Host "Compiled $($mod.Name) scripts -> $out"
        }
        else {
            Write-Host "Compiled $($mod.Name) scripts successfully."
        }
    }
}
finally {
    if (-not $KeepOutput -and (Test-Path -LiteralPath $OutputRoot)) {
        Remove-Item -LiteralPath $OutputRoot -Recurse -Force
    }
}
