[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string] $LayerPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$layerRoot = [System.IO.Path]::GetFullPath($LayerPath)
$mappingPath = Join-Path $layerRoot 'Data\WorkingKnowledge\block_mappings.txt'
$researchBlocksPath = Join-Path $layerRoot 'Data\ResearchBlocks.sbc'
$groupPath = Join-Path $layerRoot 'Data\WorkingKnowledge\schematic_groups.txt'

function Get-SafeSubtypeToken {
    param([Parameter(Mandatory = $true)][string] $Value)
    return (($Value -replace '[^A-Za-z0-9]+', '_').Trim('_'))
}

function Get-ContentLines {
    param([Parameter(Mandatory = $true)][string] $Path)
    return @([System.IO.File]::ReadAllLines($Path))
}

function Get-DefinitionIdKey {
    param([Parameter(Mandatory = $true)][System.Xml.XmlNode] $Id)
    $type = if ($Id.Attributes['Type']) { $Id.Attributes['Type'].Value } else { [string] $Id.TypeId }
    $subtype = if ($Id.Attributes['Subtype']) { $Id.Attributes['Subtype'].Value } else { [string] $Id.SubtypeId }
    $type = $type -replace '^MyObjectBuilder_', ''
    return "$type/$subtype"
}

if (-not (Test-Path -LiteralPath $mappingPath -PathType Leaf)) {
    throw "Missing mapping file: $mappingPath"
}
if (-not (Test-Path -LiteralPath $researchBlocksPath -PathType Leaf)) {
    throw "Missing research block definitions: $researchBlocksPath"
}

$rawBuiltInGroups = Get-Content -LiteralPath (Join-Path $ScriptRoot 'Data\schematic_groups.json') -Raw | ConvertFrom-Json
$builtInGroups = @($rawBuiltInGroups | ForEach-Object { $_ })
$groupsById = @{}
$definitionOwners = @{}
foreach ($group in $builtInGroups) {
    $id = [string] $group.id
    $groupsById[$id] = $group
    $token = Get-SafeSubtypeToken $id
    $builtInGroupSubtype = if ($id -ieq 'fundamentals') { '0' } else { "WkKn_$token" }
    $definitionOwners["group:$builtInGroupSubtype"] = $id
    $definitionOwners["unlocker:WkKnUnlocker_$token"] = $id
    $definitionOwners["item:WkKnSchematic_$token"] = $id
}

$customGroups = [System.Collections.Generic.List[object]]::new()
if (Test-Path -LiteralPath $groupPath -PathType Leaf) {
    $lines = Get-ContentLines $groupPath
    $contentRows = @()
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = (($lines[$i] -split '#', 2)[0]).Trim()
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $contentRows += [pscustomobject]@{ Line = $i + 1; Text = $line }
        }
    }
    if ($contentRows.Count -eq 0 -or $contentRows[0].Text -notmatch '^version\s*=\s*1$') {
        throw 'schematic_groups.txt must begin with version = 1.'
    }

    foreach ($row in @($contentRows | Select-Object -Skip 1)) {
        $fields = @($row.Text.Split('|') | ForEach-Object { $_.Trim() })
        if ($fields.Count -ne 5) {
            throw "Invalid custom group on line $($row.Line): expected five pipe-separated fields."
        }
        $id, $displayName, $tier, $groupSubtype, $unlockerSubtype = $fields
        if ($id -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?$') {
            throw "Invalid custom schematic ID '$id' on line $($row.Line)."
        }
        if ([string]::IsNullOrWhiteSpace($displayName) -or @('Common', 'Uncommon', 'Rare', 'Prototech', 'None') -notcontains $tier) {
            throw "Invalid display name or tier for '$id' on line $($row.Line)."
        }
        if ($groupSubtype -notmatch '^[A-Za-z0-9_]+$' -or $unlockerSubtype -notmatch '^WkKnUnlocker_[A-Za-z0-9_]+$') {
            throw "Invalid group or unlocker subtype for '$id' on line $($row.Line)."
        }
        if ($groupsById.ContainsKey($id)) {
            throw "Duplicate or built-in-conflicting schematic ID '$id'."
        }

        $itemSubtype = 'WkKnSchematic_' + (Get-SafeSubtypeToken $id)
        foreach ($definitionKey in @("group:$groupSubtype", "unlocker:$unlockerSubtype", "item:$itemSubtype")) {
            if ($definitionOwners.ContainsKey($definitionKey)) {
                throw "Definition collision for '$id': $definitionKey is already owned by '$($definitionOwners[$definitionKey])'."
            }
            $definitionOwners[$definitionKey] = $id
        }

        $group = [pscustomobject]@{
            id = $id; displayName = $displayName; tier = $tier
            groupSubtype = $groupSubtype; unlockerSubtype = $unlockerSubtype; itemSubtype = $itemSubtype
        }
        $customGroups.Add($group) | Out-Null
        $groupsById[$id] = $group
    }
}

$mappings = [System.Collections.Generic.List[object]]::new()
$mappingKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$knownBlockKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($key in Get-Content -LiteralPath (Join-Path $ScriptRoot 'Data\working_knowledge_block_keys.txt')) {
    if (-not [string]::IsNullOrWhiteSpace($key) -and -not $key.TrimStart().StartsWith('#')) { [void] $knownBlockKeys.Add($key.Trim()) }
}

$mappingLines = Get-ContentLines $mappingPath
for ($i = 0; $i -lt $mappingLines.Count; $i++) {
    $line = (($mappingLines[$i] -split '#', 2)[0]).Trim()
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $isOverride = $line -match '^override\s+'
    if ($isOverride) { $line = $line -replace '^override\s+', '' }
    if ($line -notmatch '^(?<block>[^/\s]+/[^/\s]+)\s*=\s*(?<group>[A-Za-z0-9._-]+)$') {
        throw "Invalid mapping on line $($i + 1): expected [override] Type/Subtype = schematic.id."
    }
    $blockKey = $Matches.block
    $researchId = $Matches.group
    if (-not $mappingKeys.Add($blockKey)) { throw "Conflicting duplicate mapping for '$blockKey'." }
    if (-not $groupsById.ContainsKey($researchId)) { throw "Unknown schematic ID '$researchId' for '$blockKey'." }
    if ($knownBlockKeys.Contains($blockKey) -and -not $isOverride) {
        throw "'$blockKey' has a built-in mapping; prefix the line with 'override' to remap it."
    }
    $mappings.Add([pscustomobject]@{ BlockKey = $blockKey; ResearchId = $researchId; IsOverride = $isOverride }) | Out-Null
}
if ($mappings.Count -eq 0) { throw 'The layer contains no block mappings.' }

[xml] $researchXml = Get-Content -LiteralPath $researchBlocksPath -Raw
$researchBlockKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
foreach ($id in @($researchXml.SelectNodes('//*[local-name()="ResearchBlock"]/*[local-name()="Id"]'))) {
    [void] $researchBlockKeys.Add((Get-DefinitionIdKey $id))
}
foreach ($mapping in $mappings) {
    if (-not $researchBlockKeys.Contains($mapping.BlockKey)) {
        throw "Missing ResearchBlocks.sbc entry for '$($mapping.BlockKey)'."
    }
}

if ($customGroups.Count -gt 0) {
    $definitionFiles = @{
        Groups = Join-Path $layerRoot 'Data\ResearchUnlockerGroups.sbc'
        Unlockers = Join-Path $layerRoot 'Data\ResearchUnlockers.sbc'
        Items = Join-Path $layerRoot 'Data\PhysicalItems_ResearchSchematics.sbc'
    }
    foreach ($path in $definitionFiles.Values) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing custom group definition file: $path" }
    }
    [xml] $groupXml = Get-Content -LiteralPath $definitionFiles.Groups -Raw
    [xml] $unlockerXml = Get-Content -LiteralPath $definitionFiles.Unlockers -Raw
    [xml] $itemXml = Get-Content -LiteralPath $definitionFiles.Items -Raw
    $groupIds = @($groupXml.SelectNodes('//*[local-name()="ResearchGroup"]/*[local-name()="Id"]') | ForEach-Object { (Get-DefinitionIdKey $_).Split('/')[1] })
    $unlockerIds = @($unlockerXml.SelectNodes('//*[local-name()="CubeBlocks"]/*/*[local-name()="Id"]') | ForEach-Object { (Get-DefinitionIdKey $_).Split('/')[1] })
    $itemIds = @($itemXml.SelectNodes('//*[local-name()="PhysicalItems"]/*/*[local-name()="Id"]') | ForEach-Object { (Get-DefinitionIdKey $_).Split('/')[1] })
    foreach ($group in $customGroups) {
        if ($groupIds -notcontains $group.groupSubtype) { throw "Missing research group definition '$($group.groupSubtype)'." }
        if ($unlockerIds -notcontains $group.unlockerSubtype) { throw "Missing unlocker definition '$($group.unlockerSubtype)'." }
        if ($itemIds -notcontains $group.itemSubtype) { throw "Missing exact Data Schematic definition '$($group.itemSubtype)'." }
    }
}

Write-Host "Validated Working Knowledge layer: $layerRoot"
Write-Host "Mappings: $($mappings.Count); explicit overrides: $(@($mappings | Where-Object IsOverride).Count); custom groups: $($customGroups.Count)."
