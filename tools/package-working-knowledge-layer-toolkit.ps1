[CmdletBinding()]
param(
    [string] $Version,
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$toolsRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $toolsRoot
$toolkitRoot = Join-Path $toolsRoot 'WorkingKnowledgeLayerToolkit'
$versionPath = Join-Path $toolkitRoot 'VERSION.txt'
$tempRoot = Join-Path $repoRoot '.tmp'
$scratchRoot = Join-Path $tempRoot 'package-working-knowledge-layer-toolkit'
$stageParent = Join-Path $scratchRoot 'stage'
$stageToolkitRoot = Join-Path $stageParent 'WorkingKnowledgeLayerToolkit'
$extractRoot = Join-Path $scratchRoot 'extracted'

function Assert-ChildPath {
    param(
        [Parameter(Mandatory = $true)][string] $Parent,
        [Parameter(Mandatory = $true)][string] $Child
    )

    $parentFull = [System.IO.Path]::GetFullPath($Parent).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    $childFull = [System.IO.Path]::GetFullPath($Child)
    if (-not $childFull.StartsWith($parentFull, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to use path outside $parentFull`: $childFull"
    }
}

if (-not (Test-Path -LiteralPath $versionPath -PathType Leaf)) {
    throw "Toolkit version file was not found: $versionPath"
}

$declaredVersion = (Get-Content -LiteralPath $versionPath -Raw).Trim()
if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = $declaredVersion
}
if ($Version -ne $declaredVersion) {
    throw "Requested toolkit version $Version does not match VERSION.txt ($declaredVersion)."
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw "Toolkit version must use major.minor.patch form: $Version"
}

$toolkitReadme = Get-Content -LiteralPath (Join-Path $toolkitRoot 'README.md') -Raw
if ($toolkitReadme -notmatch [regex]::Escape("Version **$Version**")) {
    throw "Toolkit README does not show version $Version."
}
$toolkitChangelog = Get-Content -LiteralPath (Join-Path $toolkitRoot 'CHANGELOG.md') -Raw
if ($toolkitChangelog -notmatch [regex]::Escape("## $Version")) {
    throw "Toolkit changelog has no $Version heading."
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $tempRoot ("releases\WorkingKnowledgeLayerToolkit-{0}.zip" -f $Version)
}
$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

Assert-ChildPath -Parent $tempRoot -Child $scratchRoot
$scratchFull = [System.IO.Path]::GetFullPath($scratchRoot).TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
if ($OutputPath.StartsWith($scratchFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw 'OutputPath must not be inside the disposable packaging scratch folder.'
}

if (Test-Path -LiteralPath $scratchRoot) {
    Remove-Item -LiteralPath $scratchRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stageToolkitRoot -Force | Out-Null

try {
    foreach ($item in Get-ChildItem -LiteralPath $toolkitRoot -Force) {
        Copy-Item -LiteralPath $item.FullName -Destination $stageToolkitRoot -Recurse -Force
    }

    $forbiddenItems = @(Get-ChildItem -LiteralPath $stageToolkitRoot -Recurse -Force | Where-Object {
        $_.Name -in @('.git', '.tmp') -or $_.Extension -in @('.log', '.zip')
    })
    if ($forbiddenItems.Count -gt 0) {
        throw "Toolkit package contains forbidden local artifacts: $($forbiddenItems.FullName -join ', ')"
    }

    $requiredPackagePaths = @(
        'VERSION.txt',
        'CHANGELOG.md',
        'Start.bat',
        'Start.ps1',
        'Validate.ps1',
        'QUICKSTART.md',
        'Docs\publishing_layers.md',
        'Data\schematic_groups.json',
        'ExampleMod\Data\WorkingKnowledge\block_mappings.txt'
    )
    foreach ($relativePath in $requiredPackagePaths) {
        if (-not (Test-Path -LiteralPath (Join-Path $stageToolkitRoot $relativePath))) {
            throw "Required toolkit package file is missing: $relativePath"
        }
    }

    $archiveParent = Split-Path -Parent $OutputPath
    if (-not (Test-Path -LiteralPath $archiveParent)) {
        New-Item -ItemType Directory -Path $archiveParent -Force | Out-Null
    }
    if (Test-Path -LiteralPath $OutputPath) {
        Remove-Item -LiteralPath $OutputPath -Force
    }

    Compress-Archive -LiteralPath $stageToolkitRoot -DestinationPath $OutputPath -CompressionLevel Optimal
    Expand-Archive -LiteralPath $OutputPath -DestinationPath $extractRoot -Force

    $packagedToolkitRoot = Join-Path $extractRoot 'WorkingKnowledgeLayerToolkit'
    foreach ($relativePath in $requiredPackagePaths) {
        if (-not (Test-Path -LiteralPath (Join-Path $packagedToolkitRoot $relativePath))) {
            throw "Archive layout is missing: WorkingKnowledgeLayerToolkit\$relativePath"
        }
    }

    $packagedVersion = (Get-Content -LiteralPath (Join-Path $packagedToolkitRoot 'VERSION.txt') -Raw).Trim()
    if ($packagedVersion -ne $Version) {
        throw "Packaged VERSION.txt contains $packagedVersion instead of $Version."
    }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $packagedToolkitRoot 'Start.ps1') -SelfTest
    if ($LASTEXITCODE -ne 0) {
        throw 'Packaged toolkit generator self-test failed.'
    }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File (Join-Path $packagedToolkitRoot 'Validate.ps1') -LayerPath (Join-Path $packagedToolkitRoot 'ExampleMod')
    if ($LASTEXITCODE -ne 0) {
        throw 'Packaged toolkit example validation failed.'
    }

    $archive = Get-Item -LiteralPath $OutputPath
    Write-Host "Packaged and validated Working Knowledge Layer Toolkit $Version."
    Write-Host "Archive: $($archive.FullName)"
    Write-Host "Size: $($archive.Length) bytes"
}
finally {
    if (Test-Path -LiteralPath $scratchRoot) {
        Assert-ChildPath -Parent $tempRoot -Child $scratchRoot
        Remove-Item -LiteralPath $scratchRoot -Recurse -Force
    }
}
