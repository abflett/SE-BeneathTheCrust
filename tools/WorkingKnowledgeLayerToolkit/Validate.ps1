[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string[]] $LayerPath,
    [switch] $PassThru
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ScriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

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
    return ($type -replace '^MyObjectBuilder_', '') + '/' + $subtype
}

function Add-HistoryValue {
    param([hashtable] $Table, [string] $Key, [object] $Value)
    if (-not $Table.ContainsKey($Key)) {
        $Table[$Key] = [System.Collections.Generic.List[object]]::new()
    }
    $Table[$Key].Add($Value) | Out-Null
}

function Read-DefinitionIds {
    param(
        [string] $Path,
        [string] $ContainerName,
        [string] $DefinitionName
    )
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return @() }
    [xml] $xml = Get-Content -LiteralPath $Path -Raw
    return @($xml.SelectNodes("//*[local-name()='$ContainerName']/*[local-name()='$DefinitionName']/*[local-name()='Id']") |
        ForEach-Object { (Get-DefinitionIdKey $_).Split('/')[1] })
}

$warnings = [System.Collections.Generic.List[string]]::new()
$notices = [System.Collections.Generic.List[string]]::new()
$errors = [System.Collections.Generic.List[string]]::new()
$groupClaims = [System.Collections.Generic.List[object]]::new()
$mappingClaims = [System.Collections.Generic.List[object]]::new()
$availableGroups = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$availableUnlockers = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$availableItems = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
$knownBlockKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)

$rawBuiltInGroups = Get-Content -LiteralPath (Join-Path $ScriptRoot 'Data\schematic_groups.json') -Raw | ConvertFrom-Json
$builtInGroups = @($rawBuiltInGroups | ForEach-Object { $_ })
$builtInLine = 0
foreach ($group in $builtInGroups) {
    $id = [string] $group.id
    $token = Get-SafeSubtypeToken $id
    $groupSubtype = if ($id -ieq 'fundamentals') { '0' } else { "WkKn_$token" }
    $unlockerSubtype = "WkKnUnlocker_$token"
    $itemSubtype = "WkKnSchematic_$token"
    [void] $availableGroups.Add($groupSubtype)
    [void] $availableUnlockers.Add($unlockerSubtype)
    [void] $availableItems.Add($itemSubtype)
    $groupClaims.Add([pscustomobject]@{
        Id = $id; DisplayName = [string] $group.displayName; Description = ''; Tier = [string] $group.tier
        GroupSubtype = $groupSubtype; UnlockerSubtype = $unlockerSubtype; ItemSubtype = $itemSubtype
        LoadIndex = -1; Line = ++$builtInLine; Source = 'Working Knowledge built-in catalog'; IsBuiltIn = $true
        ClaimKey = "builtin:$id"
    }) | Out-Null
}
foreach ($key in Get-Content -LiteralPath (Join-Path $ScriptRoot 'Data\working_knowledge_block_keys.txt')) {
    if (-not [string]::IsNullOrWhiteSpace($key) -and -not $key.TrimStart().StartsWith('#')) {
        [void] $knownBlockKeys.Add($key.Trim())
    }
}

$layerRoots = @($LayerPath | ForEach-Object { [System.IO.Path]::GetFullPath($_) })
for ($loadIndex = 0; $loadIndex -lt $layerRoots.Count; $loadIndex++) {
    $layerRoot = $layerRoots[$loadIndex]
    $mappingPath = Join-Path $layerRoot 'Data\WorkingKnowledge\block_mappings.txt'
    $researchBlocksPath = Join-Path $layerRoot 'Data\ResearchBlocks.sbc'
    $groupPath = Join-Path $layerRoot 'Data\WorkingKnowledge\schematic_groups.txt'
    $layerName = Split-Path -Leaf $layerRoot
    $sourcePrefix = "$layerName (load position $($loadIndex + 1))"
    if (-not (Test-Path -LiteralPath $mappingPath -PathType Leaf)) { throw "Missing mapping file: $mappingPath" }

    foreach ($id in Read-DefinitionIds (Join-Path $layerRoot 'Data\ResearchUnlockerGroups.sbc') 'ResearchGroups' 'ResearchGroup') {
        [void] $availableGroups.Add($id)
    }
    foreach ($id in Read-DefinitionIds (Join-Path $layerRoot 'Data\ResearchUnlockers.sbc') 'CubeBlocks' 'Definition') {
        [void] $availableUnlockers.Add($id)
    }
    foreach ($id in Read-DefinitionIds (Join-Path $layerRoot 'Data\PhysicalItems_ResearchSchematics.sbc') 'PhysicalItems' 'PhysicalItem') {
        [void] $availableItems.Add($id)
    }

    if (Test-Path -LiteralPath $groupPath -PathType Leaf) {
        $contentRows = [System.Collections.Generic.List[object]]::new()
        $lines = @(Get-ContentLines $groupPath)
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = (($lines[$i] -split '#', 2)[0]).Trim()
            if (-not [string]::IsNullOrWhiteSpace($line)) {
                $contentRows.Add([pscustomobject]@{ Line = $i + 1; Text = $line }) | Out-Null
            }
        }
        if ($contentRows.Count -eq 0 -or $contentRows[0].Text -notmatch '^version\s*=\s*1$') {
            throw "$groupPath must begin with version = 1."
        }

        foreach ($row in @($contentRows | Select-Object -Skip 1)) {
            $fields = @($row.Text.Split('|') | ForEach-Object { $_.Trim() })
            if ($fields.Count -ne 5 -and $fields.Count -ne 6) {
                throw "Invalid group in $groupPath line $($row.Line): expected five fields plus an optional description."
            }
            $id, $displayName, $tier, $groupSubtype, $unlockerSubtype = $fields[0..4]
            $description = if ($fields.Count -eq 6) { $fields[5] } else { '' }
            if ($id -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9._-]*[A-Za-z0-9])?$') {
                throw "Invalid schematic ID '$id' in $groupPath line $($row.Line)."
            }
            if ([string]::IsNullOrWhiteSpace($displayName) -or @('Common', 'Uncommon', 'Rare', 'Prototech', 'None') -notcontains $tier) {
                throw "Invalid display name or tier for '$id' in $groupPath line $($row.Line)."
            }
            if ($groupSubtype -notmatch '^[A-Za-z0-9_]+$' -or $unlockerSubtype -notmatch '^WkKnUnlocker_[A-Za-z0-9_]+$') {
                throw "Invalid group or unlocker subtype for '$id' in $groupPath line $($row.Line)."
            }
            $groupClaims.Add([pscustomobject]@{
                Id = $id; DisplayName = $displayName; Description = $description; Tier = $tier
                GroupSubtype = $groupSubtype; UnlockerSubtype = $unlockerSubtype
                ItemSubtype = 'WkKnSchematic_' + (Get-SafeSubtypeToken $id)
                LoadIndex = $loadIndex; Line = $row.Line; Source = "$sourcePrefix line $($row.Line)"; IsBuiltIn = $false
                ClaimKey = "$loadIndex`:$($row.Line):$id"
            }) | Out-Null
        }
    }

    $researchBlockKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    if (Test-Path -LiteralPath $researchBlocksPath -PathType Leaf) {
        [xml] $researchXml = Get-Content -LiteralPath $researchBlocksPath -Raw
        foreach ($idNode in @($researchXml.SelectNodes('//*[local-name()="ResearchBlock"]/*[local-name()="Id"]'))) {
            [void] $researchBlockKeys.Add((Get-DefinitionIdKey $idNode))
        }
    }

    $mappingLines = @(Get-ContentLines $mappingPath)
    for ($i = 0; $i -lt $mappingLines.Count; $i++) {
        $line = (($mappingLines[$i] -split '#', 2)[0]).Trim()
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $isOverride = $line -match '^override\s+'
        if ($isOverride) { $line = $line -replace '^override\s+', '' }
        if ($line -notmatch '^(?<block>[^/\s]+/[^/\s]+)\s*=\s*(?<group>[A-Za-z0-9._-]+)$') {
            throw "Invalid mapping in $mappingPath line $($i + 1): expected [override] Type/Subtype = schematic.id."
        }
        $blockKey = $Matches.block
        $researchId = $Matches.group
        if (-not $knownBlockKeys.Contains($blockKey) -and -not $researchBlockKeys.Contains($blockKey)) {
            throw "Missing ResearchBlocks.sbc entry for '$blockKey' in $layerName."
        }
        $mappingClaims.Add([pscustomobject]@{
            BlockKey = $blockKey; ResearchId = $researchId; IsOverride = $isOverride
            LoadIndex = $loadIndex; Line = $i + 1; Source = "$sourcePrefix line $($i + 1)"
        }) | Out-Null
    }
}

$groupHistory = @{}
$winningGroups = @{}
foreach ($claim in @($groupClaims | Sort-Object LoadIndex, Line, Id)) {
    Add-HistoryValue $groupHistory $claim.Id $claim
}
foreach ($id in $groupHistory.Keys) {
    $history = @($groupHistory[$id])
    $valid = @()
    foreach ($claim in $history) {
        $blockingMissing = @()
        if (-not $availableGroups.Contains($claim.GroupSubtype)) { $blockingMissing += "research group $($claim.GroupSubtype)" }
        if (-not $availableUnlockers.Contains($claim.UnlockerSubtype)) { $blockingMissing += "unlocker $($claim.UnlockerSubtype)" }
        if (@($blockingMissing).Count -gt 0) {
            $errors.Add("Skipped group '$($claim.Id)' from $($claim.Source) because it is missing: $($blockingMissing -join ', ').") | Out-Null
            continue
        }
        $valid += $claim
    }
    if ($valid.Count -eq 0) { continue }
    $winner = $valid[-1]
    $winningGroups[$id] = $winner
    if ($history.Count -gt 1) {
        $message = "Group '$id' was declared $($history.Count) times; $($winner.Source) is the last valid declaration and wins."
        $warnings.Add($message) | Out-Null
    }
}

$groupOwner = @{}; $unlockerOwner = @{}; $itemOwner = @{}
foreach ($claim in @($winningGroups.Values | Sort-Object LoadIndex, Line, Id)) {
    $groupOwner[$claim.GroupSubtype] = $claim
    $unlockerOwner[$claim.UnlockerSubtype] = $claim
    $itemOwner[$claim.ItemSubtype] = $claim
}
$activeGroups = @{}
foreach ($claim in @($winningGroups.Values | Sort-Object LoadIndex, Line, Id)) {
    $ownsDefinitions = $groupOwner[$claim.GroupSubtype].ClaimKey -eq $claim.ClaimKey -and
        $unlockerOwner[$claim.UnlockerSubtype].ClaimKey -eq $claim.ClaimKey -and
        $itemOwner[$claim.ItemSubtype].ClaimKey -eq $claim.ClaimKey
    if (-not $ownsDefinitions) {
        $warnings.Add("Group '$($claim.Id)' from $($claim.Source) is inactive because a later group owns one of its definition IDs.") | Out-Null
        continue
    }
    $activeGroups[$claim.Id] = $claim
    if (-not $availableItems.Contains($claim.ItemSubtype)) {
        $errors.Add("Group '$($claim.Id)' from $($claim.Source) is missing exact schematic $($claim.ItemSubtype); block work and fragments remain valid in game.") | Out-Null
    }
}

$mappingHistory = @{}
foreach ($claim in @($mappingClaims | Sort-Object LoadIndex, Line)) {
    Add-HistoryValue $mappingHistory $claim.BlockKey $claim
}
$winningMappings = @{}
foreach ($blockKey in $mappingHistory.Keys) {
    $valid = @()
    foreach ($claim in @($mappingHistory[$blockKey])) {
        if ($activeGroups.ContainsKey($claim.ResearchId)) { $valid += $claim }
        else { $warnings.Add("Skipped $($claim.Source) mapping for $blockKey because '$($claim.ResearchId)' is unknown or inactive.") | Out-Null }
    }
    if ($valid.Count -eq 0) { continue }
    $winner = $valid[-1]
    $winningMappings[$blockKey] = $winner
    if ($valid.Count -gt 1) {
        $historyText = ($valid | ForEach-Object { "$($_.Source) -> $($_.ResearchId)" }) -join '; '
        $warnings.Add("$blockKey was assigned by $($valid.Count) layers; $($winner.Source) -> '$($winner.ResearchId)' wins. History: $historyText.") | Out-Null
    }
    if ($knownBlockKeys.Contains($blockKey)) {
        $notices.Add("$blockKey replaces its built-in Working Knowledge assignment with '$($winner.ResearchId)' from $($winner.Source).") | Out-Null
    }
}

$result = [pscustomobject]@{
    LayerPaths = $layerRoots
    ActiveGroups = $activeGroups
    WinningMappings = $winningMappings
    Warnings = @($warnings | Sort-Object -Unique)
    Notices = @($notices | Sort-Object -Unique)
    Errors = @($errors | Sort-Object -Unique)
}

Write-Host "Validated Working Knowledge layer stack ($($layerRoots.Count) layer(s))."
Write-Host "Mappings: $($mappingClaims.Count); winners: $($winningMappings.Count); active layer groups: $(@($activeGroups.Values | Where-Object { -not $_.IsBuiltIn }).Count)."
foreach ($warning in $result.Warnings) { Write-Host "WARNING: $warning" }
foreach ($notice in $result.Notices) { Write-Host "NOTICE: $notice" }
foreach ($errorMessage in $result.Errors) { Write-Host "ERROR: $errorMessage" }

if ($PassThru) { Write-Output $result }
elseif ($result.Errors.Count -gt 0) { throw "Layer validation failed with $($result.Errors.Count) error(s)." }
