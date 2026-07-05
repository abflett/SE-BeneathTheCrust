[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string[]] $ModName = @(),
    [string] $DestinationRoot = (Join-Path $env:APPDATA 'SpaceEngineers\Mods'),
    [switch] $NoClean
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-OrCreateDirectory {
    param([Parameter(Mandatory = $true)][string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }

    return (Resolve-Path -LiteralPath $Path).Path
}

function Assert-NotDriveRoot {
    param([Parameter(Mandatory = $true)][string] $Path)

    $fullPath = [System.IO.Path]::GetFullPath($Path).TrimEnd('\', '/')
    $rootPath = [System.IO.Path]::GetPathRoot($fullPath).TrimEnd('\', '/')

    if ($fullPath -eq $rootPath) {
        throw "Refusing to use drive root as deployment destination: $Path"
    }
}

function Assert-ChildPath {
    param(
        [Parameter(Mandatory = $true)][string] $Parent,
        [Parameter(Mandatory = $true)][string] $Child
    )

    $parentFull = [System.IO.Path]::GetFullPath($Parent).TrimEnd('\', '/')
    $childFull = [System.IO.Path]::GetFullPath($Child).TrimEnd('\', '/')
    $prefix = $parentFull + [System.IO.Path]::DirectorySeparatorChar

    if (-not $childFull.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to operate outside deployment root. Parent: $parentFull Child: $childFull"
    }
}

function Sync-PublishMetadataFromDeploy {
    param(
        [Parameter(Mandatory = $true)][string] $SourceModPath,
        [Parameter(Mandatory = $true)][string] $TargetModPath,
        [Parameter(Mandatory = $true)][string[]] $FileNames
    )

    if (-not (Test-Path -LiteralPath $TargetModPath)) {
        return
    }

    foreach ($fileName in $FileNames) {
        $deployedFilePath = Join-Path $TargetModPath $fileName
        if (-not (Test-Path -LiteralPath $deployedFilePath -PathType Leaf)) {
            continue
        }

        $sourceFilePath = Join-Path $SourceModPath $fileName
        Copy-Item -LiteralPath $deployedFilePath -Destination $sourceFilePath -Force
        Write-Host "Preserved publish metadata $fileName -> $sourceFilePath"
    }
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceRoot = Join-Path $repoRoot 'mods'

if (-not (Test-Path -LiteralPath $sourceRoot)) {
    throw "Could not find mods folder: $sourceRoot"
}

Assert-NotDriveRoot -Path $DestinationRoot
$resolvedDestinationRoot = Resolve-OrCreateDirectory -Path $DestinationRoot

$availableMods = Get-ChildItem -LiteralPath $sourceRoot -Directory | Where-Object {
    (Test-Path -LiteralPath (Join-Path $_.FullName 'Data')) -or
    (Test-Path -LiteralPath (Join-Path $_.FullName 'modinfo.sbc'))
}

if ($ModName.Count -gt 0) {
    $modNameAliases = @{
        'Working-Knowledge' = 'WorkingKnowledge'
        'Working Knowledge' = 'WorkingKnowledge'
        'WkKn' = 'WorkingKnowledge'
    }

    $requested = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $ModName) {
        $canonicalName = $name
        if ($modNameAliases.ContainsKey($name)) {
            $canonicalName = $modNameAliases[$name]
        }

        [void] $requested.Add($canonicalName)
    }

    $availableMods = $availableMods | Where-Object { $requested.Contains($_.Name) }

    foreach ($name in $requested) {
        if (-not ($availableMods | Where-Object { $_.Name -ieq $name })) {
            throw "Requested mod was not found under mods/: $name"
        }
    }
}

if (-not $availableMods) {
    throw "No standalone mod folders found under: $sourceRoot"
}

$deployNames = @{
    'WorkingKnowledge' = 'Working Knowledge'
}

$publishMetadataFiles = @(
    'metadata.mod',
    'modinfo.sbmi'
)

foreach ($mod in $availableMods) {
    $deployName = $mod.Name
    if ($deployNames.ContainsKey($mod.Name)) {
        $deployName = $deployNames[$mod.Name]
    }

    $targetPath = Join-Path $resolvedDestinationRoot $deployName
    Assert-ChildPath -Parent $resolvedDestinationRoot -Child $targetPath

    if ($PSCmdlet.ShouldProcess($targetPath, "Deploy local Space Engineers mod '$deployName' from '$($mod.Name)'")) {
        Sync-PublishMetadataFromDeploy -SourceModPath $mod.FullName -TargetModPath $targetPath -FileNames $publishMetadataFiles

        $legacyTargetPath = Join-Path $resolvedDestinationRoot $mod.Name
        if ($legacyTargetPath -ne $targetPath -and (Test-Path -LiteralPath $legacyTargetPath) -and -not $NoClean) {
            $resolvedLegacyTargetPath = (Resolve-Path -LiteralPath $legacyTargetPath).Path
            Assert-ChildPath -Parent $resolvedDestinationRoot -Child $resolvedLegacyTargetPath
            Remove-Item -LiteralPath $resolvedLegacyTargetPath -Recurse -Force
        }

        if ((Test-Path -LiteralPath $targetPath) -and -not $NoClean) {
            $resolvedTargetPath = (Resolve-Path -LiteralPath $targetPath).Path
            Assert-ChildPath -Parent $resolvedDestinationRoot -Child $resolvedTargetPath
            Remove-Item -LiteralPath $resolvedTargetPath -Recurse -Force
        }

        New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
        Get-ChildItem -LiteralPath $mod.FullName -Force | Copy-Item -Destination $targetPath -Recurse -Force

        Write-Host "Deployed $deployName -> $targetPath"
    }
}
