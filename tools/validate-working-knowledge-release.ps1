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

$catalogPath = Join-Path $modRoot 'Data\Scripts\WorkingKnowledge\Application\Research\Catalog\ResearchCatalog.generated.cs'
$catalogText = Get-Content -LiteralPath $catalogPath -Raw
$metadataMatch = [regex]::Match(
    $catalogText,
    'private const string ResearchMetadataData\s*=\s*@"\r?\n(?<Data>.*?)\r?\n";',
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)
if (-not $metadataMatch.Success) {
    throw "Could not read built-in schematic metadata from $catalogPath."
}

$baseGroups = @{}
foreach ($line in ($metadataMatch.Groups['Data'].Value -split '\r?\n')) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }
    $fields = $line.Split('|')
    if ($fields.Count -ne 5) {
        throw "Malformed built-in schematic metadata row: $line"
    }
    if ($baseGroups.ContainsKey($fields[0])) {
        throw "Duplicate built-in schematic ID in generated catalog: $($fields[0])"
    }
    $baseGroups[$fields[0]] = [pscustomobject]@{
        DisplayName = $fields[1]
        Tier = $fields[4]
    }
}

$toolkitGroupsPath = Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Data\schematic_groups.json'
$rawToolkitGroups = Get-Content -LiteralPath $toolkitGroupsPath -Raw | ConvertFrom-Json
$toolkitGroups = @($rawToolkitGroups | ForEach-Object { $_ })
$toolkitGroupsById = @{}
foreach ($group in $toolkitGroups) {
    $id = [string] $group.id
    if ($toolkitGroupsById.ContainsKey($id)) {
        throw "Duplicate built-in schematic ID in Toolkit catalog: $id"
    }
    $toolkitGroupsById[$id] = $group
}

$missingToolkitGroups = @($baseGroups.Keys | Where-Object { -not $toolkitGroupsById.ContainsKey($_) } | Sort-Object)
$extraToolkitGroups = @($toolkitGroupsById.Keys | Where-Object { -not $baseGroups.ContainsKey($_) } | Sort-Object)
if ($missingToolkitGroups.Count -gt 0) {
    throw "Toolkit catalog is missing built-in schematic groups: $($missingToolkitGroups -join ', ')"
}
if ($extraToolkitGroups.Count -gt 0) {
    throw "Toolkit catalog contains groups that are not active built-ins: $($extraToolkitGroups -join ', ')"
}

foreach ($id in $baseGroups.Keys) {
    $base = $baseGroups[$id]
    $toolkit = $toolkitGroupsById[$id]
    if ([string] $toolkit.displayName -cne $base.DisplayName) {
        throw "Toolkit display name for '$id' is '$($toolkit.displayName)' instead of '$($base.DisplayName)'."
    }
    if ([string] $toolkit.tier -cne $base.Tier) {
        throw "Toolkit tier for '$id' is '$($toolkit.tier)' instead of '$($base.Tier)'."
    }
}

$prototechGroupCount = @($baseGroups.Keys | Where-Object { $_ -like 'prototech.*' }).Count
Write-Host "Validated Toolkit catalog parity for $($baseGroups.Count) built-in schematic groups, including $prototechGroupCount Prototech groups."

if (-not $SkipCompile) {
    & (Join-Path $repoRoot 'tools\compile-mod-scripts.ps1') -ModName WkKn
    if ($LASTEXITCODE -ne 0) {
        throw "Working Knowledge script compilation failed."
    }
}

$layerValidator = Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Validate.ps1'
& (Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Start.ps1') -SelfTest
& $layerValidator -LayerPath (Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\ExampleMod')
& $layerValidator -LayerPath (Join-Path $repoRoot 'mods\WKL-ARCTrussSystem')
Write-Host 'Running isolated layer priority and fallback fixtures; expected invalid-case diagnostics are suppressed.'
& (Join-Path $repoRoot 'tools\WorkingKnowledgeLayerToolkit\Tests\Test-LayerResolution.ps1')
& (Join-Path $repoRoot 'tools\package-working-knowledge-layer-toolkit.ps1')

Write-Host "Validated Working Knowledge $ExpectedVersion release files successfully."
Write-Host "Parsed $($xmlFiles.Count) XML files; thumbnail is $($thumb.Length) bytes."
