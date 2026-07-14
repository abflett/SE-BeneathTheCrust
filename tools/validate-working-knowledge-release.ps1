[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $ExpectedVersion,
    [switch] $SkipCompile
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$modRoot = Join-Path $repoRoot 'mods\WorkingKnowledge'
$modInfoPath = Join-Path $modRoot 'modinfo.sbc'
$thumbPath = Join-Path $modRoot 'thumb.jpg'

if (-not (Test-Path -LiteralPath $modInfoPath -PathType Leaf)) {
    throw "Working Knowledge modinfo.sbc was not found: $modInfoPath"
}

[xml] $modInfo = Get-Content -LiteralPath $modInfoPath -Raw
$actualVersion = [string] $modInfo.ModItem.Version
if ($actualVersion -ne $ExpectedVersion) {
    throw "Expected Working Knowledge version $ExpectedVersion but modinfo.sbc contains $actualVersion."
}

$xmlFiles = Get-ChildItem -LiteralPath $modRoot -Recurse -File | Where-Object {
    $_.Extension -in @('.sbc', '.sbmi', '.mod')
}
foreach ($file in $xmlFiles) {
    try {
        [xml] (Get-Content -LiteralPath $file.FullName -Raw) | Out-Null
    }
    catch {
        throw "Invalid XML in $($file.FullName): $($_.Exception.Message)"
    }
}

if (-not (Test-Path -LiteralPath $thumbPath -PathType Leaf)) {
    throw "Working Knowledge thumbnail was not found: $thumbPath"
}

$thumb = Get-Item -LiteralPath $thumbPath
if ($thumb.Length -ge 1MB) {
    throw "Working Knowledge thumbnail must stay under 1 MB; current size is $($thumb.Length) bytes."
}

$changelog = Get-Content -LiteralPath (Join-Path $repoRoot 'docs\WorkingKnowledge\changelog.md') -Raw
if ($changelog -notmatch [regex]::Escape("## $ExpectedVersion")) {
    throw "Working Knowledge changelog has no $ExpectedVersion heading."
}

$versionParts = $ExpectedVersion.Split('.')
if ($versionParts.Length -lt 2) {
    throw "ExpectedVersion must contain at least major and feature numbers."
}

$featureLine = $versionParts[0] + '.' + $versionParts[1]
$rootReadme = Get-Content -LiteralPath (Join-Path $repoRoot 'README.md') -Raw
$modReadme = Get-Content -LiteralPath (Join-Path $modRoot 'README.md') -Raw
if ($rootReadme -notmatch [regex]::Escape($featureLine)) {
    throw "Root README does not mention the current $featureLine release line."
}
if ($modReadme -notmatch [regex]::Escape($featureLine + '.x')) {
    throw "Working Knowledge README does not mention the current $featureLine.x release series."
}

if (-not $SkipCompile) {
    & (Join-Path $repoRoot 'tools\compile-mod-scripts.ps1') -ModName WkKn
    if ($LASTEXITCODE -ne 0) {
        throw "Working Knowledge script compilation failed."
    }
}

$layerValidator = Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Validate.ps1'
& $layerValidator -LayerPath (Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\ExampleMod')
& $layerValidator -LayerPath (Join-Path $repoRoot 'mods\WKL-ARCTrussSystem')

Write-Host "Validated Working Knowledge $ExpectedVersion release files successfully."
Write-Host "Parsed $($xmlFiles.Count) XML files; thumbnail is $($thumb.Length) bytes."
