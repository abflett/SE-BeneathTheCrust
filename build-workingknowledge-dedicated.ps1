[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $DedicatedDataRoot = (Join-Path $env:APPDATA 'SpaceEngineersDedicated'),
    [string] $ClientModsRoot = (Join-Path $env:APPDATA 'SpaceEngineers\Mods'),
    [string] $ClientWorkshopRoot = 'C:\Program Files (x86)\Steam\steamapps\workshop\content\244850',
    [string] $WorldPath,
    [switch] $NoClean,
    [switch] $SkipCompile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$localModName = 'Working Knowledge'
$workshopModId = '3758066250'
$dedicatedConfigName = 'SpaceEngineers-Dedicated.cfg'

function Resolve-ContainedPath {
    param(
        [Parameter(Mandatory = $true)][string] $Parent,
        [Parameter(Mandatory = $true)][string] $Child
    )

    $parentFull = [System.IO.Path]::GetFullPath($Parent).TrimEnd('\', '/')
    $childFull = [System.IO.Path]::GetFullPath($Child).TrimEnd('\', '/')
    $prefix = $parentFull + [System.IO.Path]::DirectorySeparatorChar
    if (-not $childFull.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to operate outside the expected root. Root: $parentFull Child: $childFull"
    }

    return $childFull
}

function Get-ConfiguredWorldPath {
    param([Parameter(Mandatory = $true)][string] $ConfigPath)

    [xml] $config = Get-Content -LiteralPath $ConfigPath -Raw
    $loadWorld = [string] $config.MyConfigDedicated.LoadWorld
    if ([string]::IsNullOrWhiteSpace($loadWorld)) {
        throw "Dedicated config does not contain a LoadWorld path: $ConfigPath"
    }

    if ([System.IO.Path]::GetExtension($loadWorld)) {
        return Split-Path -Parent $loadWorld
    }

    return $loadWorld
}

function Set-WorkshopWorkingKnowledgeReference {
    param([Parameter(Mandatory = $true)][string] $CheckpointPath)

    [xml] $document = Get-Content -LiteralPath $CheckpointPath -Raw
    $root = $document.DocumentElement
    if ($null -eq $root) {
        throw "Checkpoint has no document root: $CheckpointPath"
    }

    $mods = $root.SelectSingleNode('Mods')
    if ($null -eq $mods) {
        $mods = $document.CreateElement('Mods')
        [void] $root.AppendChild($mods)
    }

    $matchingItems = @($mods.SelectNodes('ModItem') | Where-Object {
        $nameNode = $_.SelectSingleNode('Name')
        $idNode = $_.SelectSingleNode('PublishedFileId')
        $name = if ($null -eq $nameNode) { '' } else { [string] $nameNode.InnerText }
        $publishedFileId = if ($null -eq $idNode) { '' } else { [string] $idNode.InnerText }
        $name.Equals($localModName, [System.StringComparison]::OrdinalIgnoreCase) -or
        $name.Equals($workshopModId + '.sbm', [System.StringComparison]::OrdinalIgnoreCase) -or
        $publishedFileId -eq $workshopModId
    })

    foreach ($item in $matchingItems) {
        [void] $mods.RemoveChild($item)
    }

    $workshopItem = $document.CreateElement('ModItem')
    $workshopItem.SetAttribute('FriendlyName', $localModName)

    $nameNode = $document.CreateElement('Name')
    $nameNode.InnerText = $workshopModId + '.sbm'
    [void] $workshopItem.AppendChild($nameNode)

    $idNode = $document.CreateElement('PublishedFileId')
    $idNode.InnerText = $workshopModId
    [void] $workshopItem.AppendChild($idNode)

    $serviceNode = $document.CreateElement('PublishedServiceName')
    $serviceNode.InnerText = 'Steam'
    [void] $workshopItem.AppendChild($serviceNode)

    if ($mods.HasChildNodes) {
        [void] $mods.InsertBefore($workshopItem, $mods.FirstChild)
    }
    else {
        [void] $mods.AppendChild($workshopItem)
    }

    $backupPath = $CheckpointPath + '.before-wk-local'
    if (-not (Test-Path -LiteralPath $backupPath)) {
        Copy-Item -LiteralPath $CheckpointPath -Destination $backupPath
        Write-Host "Backed up checkpoint config -> $backupPath"
    }

    $settings = [System.Xml.XmlWriterSettings]::new()
    $settings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $settings.Indent = $true
    $settings.NewLineChars = "`r`n"
    $settings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

    $writer = [System.Xml.XmlWriter]::Create($CheckpointPath, $settings)
    try {
        $document.Save($writer)
    }
    finally {
        $writer.Dispose()
    }

    Write-Host "Configured Workshop identity $workshopModId in $CheckpointPath"
}

function Copy-DevelopmentBuildToWorkshopCache {
    param(
        [Parameter(Mandatory = $true)][string] $SourceModPath,
        [Parameter(Mandatory = $true)][string] $WorkshopRoot
    )

    if (-not (Test-Path -LiteralPath $WorkshopRoot -PathType Container)) {
        New-Item -ItemType Directory -Path $WorkshopRoot -Force | Out-Null
    }

    $resolvedWorkshopRoot = (Resolve-Path -LiteralPath $WorkshopRoot).Path
    $targetPath = Resolve-ContainedPath -Parent $resolvedWorkshopRoot -Child (Join-Path $resolvedWorkshopRoot $workshopModId)
    $backupPath = Resolve-ContainedPath -Parent $resolvedWorkshopRoot -Child (Join-Path $resolvedWorkshopRoot ($workshopModId + '.before-wk-local'))

    if ((Test-Path -LiteralPath $targetPath -PathType Container) -and
        -not (Test-Path -LiteralPath $backupPath)) {
        Copy-Item -LiteralPath $targetPath -Destination $backupPath -Recurse
        Write-Host "Backed up Workshop cache -> $backupPath"
    }

    if (Test-Path -LiteralPath $targetPath) {
        Remove-Item -LiteralPath $targetPath -Recurse -Force
    }

    New-Item -ItemType Directory -Path $targetPath -Force | Out-Null
    Get-ChildItem -LiteralPath $SourceModPath -Force | Copy-Item -Destination $targetPath -Recurse -Force
    Write-Host "Overlaid development build -> $targetPath"
}

$runningServer = Get-Process -Name 'SpaceEngineersDedicated' -ErrorAction SilentlyContinue
if ($runningServer) {
    throw 'Stop SpaceEngineersDedicated before deploying or changing its active world.'
}

$dedicatedDataFull = [System.IO.Path]::GetFullPath($DedicatedDataRoot).TrimEnd('\', '/')
if (-not (Test-Path -LiteralPath $dedicatedDataFull -PathType Container)) {
    throw "Dedicated data root was not found: $dedicatedDataFull"
}

if ([string]::IsNullOrWhiteSpace($WorldPath)) {
    $dedicatedConfigPath = Join-Path $dedicatedDataFull $dedicatedConfigName
    if (-not (Test-Path -LiteralPath $dedicatedConfigPath -PathType Leaf)) {
        throw "Dedicated config was not found: $dedicatedConfigPath"
    }

    $WorldPath = Get-ConfiguredWorldPath -ConfigPath $dedicatedConfigPath
}

$worldFull = Resolve-ContainedPath -Parent $dedicatedDataFull -Child $WorldPath
if (-not (Test-Path -LiteralPath $worldFull -PathType Container)) {
    throw "Dedicated world folder was not found: $worldFull"
}

$checkpointPaths = @(
    (Join-Path $worldFull 'Sandbox.sbc'),
    (Join-Path $worldFull 'Sandbox_config.sbc')
)
foreach ($checkpointPath in $checkpointPaths) {
    if (-not (Test-Path -LiteralPath $checkpointPath -PathType Leaf)) {
        throw "Dedicated checkpoint file was not found: $checkpointPath"
    }
}

if (-not $SkipCompile) {
    & (Join-Path $PSScriptRoot 'tools\compile-mod-scripts.ps1') -ModName WkKn
}

$buildParameters = @{
    ModName = 'WkKn'
    DestinationRoot = $ClientModsRoot
}
if ($NoClean) {
    $buildParameters.NoClean = $true
}

if ($PSCmdlet.ShouldProcess($ClientModsRoot, 'Deploy the Working Knowledge development source')) {
    & (Join-Path $PSScriptRoot 'build.ps1') @buildParameters
}

$sourceModPath = Join-Path $ClientModsRoot $localModName
if (-not (Test-Path -LiteralPath $sourceModPath -PathType Container)) {
    throw "Client development deployment was not found: $sourceModPath"
}

$dedicatedWorkshopRoot = Join-Path $dedicatedDataFull 'content\244850'
foreach ($workshopRoot in @($ClientWorkshopRoot, $dedicatedWorkshopRoot)) {
    if ($PSCmdlet.ShouldProcess($workshopRoot, "Overlay Workshop cache $workshopModId with the local development build")) {
        Copy-DevelopmentBuildToWorkshopCache -SourceModPath $sourceModPath -WorkshopRoot $workshopRoot
    }
}

foreach ($checkpointPath in $checkpointPaths) {
    if ($PSCmdlet.ShouldProcess($checkpointPath, "Keep Workshop identity $workshopModId while using the development cache overlay")) {
        Set-WorkshopWorkingKnowledgeReference -CheckpointPath $checkpointPath
    }
}

Write-Host 'Dedicated Working Knowledge development overlay is ready.'
Write-Host "World: $worldFull"
Write-Host "Source deployment: $sourceModPath"
Write-Host "Client cache: $(Join-Path $ClientWorkshopRoot $workshopModId)"
Write-Host "Server cache: $(Join-Path $dedicatedWorkshopRoot $workshopModId)"
Write-Warning 'Steam may replace a development cache overlay when the Workshop item updates or its files are verified. Stop the server and rerun this helper before each unpublished dedicated-server test build.'
