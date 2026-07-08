[CmdletBinding()]
param(
    [string] $SpaceEngineersData = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Data',
    [string] $CatalogPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'mods\WorkingKnowledge\Data\Scripts\WorkingKnowledge\Application\Research\Catalog\ResearchCatalog.generated.cs'),
    [string] $RewardTablePath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'mods\WorkingKnowledge\Data\Scripts\WorkingKnowledge\Application\Balance\SchematicWorkRewardTable.cs'),
    [string] $ReportPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'docs\WorkingKnowledge\generated\balance_report.md'),
    [string] $BlockRewardReportPath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'docs\WorkingKnowledge\generated\block_reward_report.md'),
    [switch] $UpdateSource
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$LargeGridBaseWorkReward = 0.21
$DefaultSmallGridBaseWorkReward = 0.0857
$DesiredSmallGridRewardRatio = 0.25
$MinimumSmallGridBaseWorkReward = 0.08
$MaximumSmallGridBaseWorkReward = 0.12
$MinimumTargetBlocks = 6.0
$MaximumTargetBlocks = 12.0
$MinimumReferenceBuildSeconds = 2.0
$MaximumReferenceBuildSeconds = 300.0
$MinimumBuildTimeFactor = 0.5
$MaximumBuildTimeFactor = 4.0
$LargeGridNormalAnchorPercentile = 1.0
$NormalLargeGridAnchorMaximumVolume = 1.0
$FallbackLargeGridAnchorMaximumVolume = 8.0

# Mixed-size families need a stable "normal" block to anchor the table.
# Otherwise tiny utility blocks, decorative containers, or alternate-shape variants can
# pull the anchor time away from the block players expect to be baseline.
$RepresentativeBlockKeyOverridesByResearchId = @{
    'communications' = @{
        Large = @('RadioAntenna/LargeBlockRadioAntenna')
        Small = @('RadioAntenna/SmallBlockRadioAntenna')
    }
    'fundamentals' = @{
        Large = @('CubeBlock/LargeBlockArmorBlock')
        Small = @('CubeBlock/SmallBlockArmorBlock')
    }
    'logistics.cargo_storage' = @{
        Large = @('CargoContainer/LargeBlockSmallContainer')
        Small = @('CargoContainer/SmallBlockMediumContainer')
    }
    'logistics.conveyor_network' = @{
        Large = @('Conveyor/LargeBlockConveyor')
        Small = @('Conveyor/SmallBlockConveyor')
    }
    'mechanical.systems' = @{
        Large = @('MotorAdvancedStator/LargeHinge')
        Small = @('MotorAdvancedStator/SmallHinge')
    }
    'power.battery' = @{
        Large = @('BatteryBlock/LargeBlockBatteryBlock')
        Small = @('BatteryBlock/SmallBlockBatteryBlock')
    }
    'power.reactor' = @{
        Large = @('Reactor/LargeBlockSmallGenerator')
        Small = @('Reactor/SmallBlockSmallGenerator')
    }
    'propulsion.atmospheric_thruster' = @{
        Large = @('Thrust/LargeBlockSmallAtmosphericThrust')
        Small = @('Thrust/SmallBlockSmallAtmosphericThrust')
    }
    'propulsion.hydrogen_thruster' = @{
        Large = @('Thrust/LargeBlockSmallHydrogenThrust')
        Small = @('Thrust/SmallBlockSmallHydrogenThrust')
    }
    'propulsion.ion_thruster' = @{
        Large = @('Thrust/LargeBlockSmallThrust')
        Small = @('Thrust/SmallBlockSmallThrust')
    }
    'prototech.thruster' = @{
        Large = @('Thrust/LargeBlockPrototechThruster')
        Small = @('Thrust/SmallBlockPrototechThruster')
    }
    'structure.industrial' = @{
        Large = @('CubeBlock/TrussFloor')
        Small = @('CubeBlock/SmallGridBeamBlockHalf')
    }
}

function Get-Text {
    param([AllowNull()][object] $Node)

    if ($null -eq $Node) {
        return ''
    }

    return ([string] $Node).Trim()
}

function Get-Number {
    param([AllowNull()][object] $Value, [double] $Default = 0.0)

    if ($null -eq $Value) {
        return $Default
    }

    $text = ([string] $Value).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $Default
    }

    return [double]::Parse($text, [System.Globalization.CultureInfo]::InvariantCulture)
}

function Format-Number {
    param([double] $Value, [string] $Format = '0.###')

    return $Value.ToString($Format, [System.Globalization.CultureInfo]::InvariantCulture)
}

function Set-TextFileNoBom {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][AllowEmptyString()][AllowEmptyCollection()][string[]] $Lines
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($Path, $Lines, $encoding)
}

function Set-RawTextFileNoBom {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][string] $Content
    )

    $encoding = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $encoding)
}

function Get-DefinitionId {
    param([Parameter(Mandatory = $true)][object] $IdNode)

    $type = Get-Text $IdNode.TypeId
    $subtype = Get-Text $IdNode.SubtypeId
    if ([string]::IsNullOrWhiteSpace($type) -and $IdNode.PSObject.Properties['Type']) {
        $type = Get-Text $IdNode.Type
    }
    if ([string]::IsNullOrWhiteSpace($subtype) -and $IdNode.PSObject.Properties['Subtype']) {
        $subtype = Get-Text $IdNode.Subtype
    }

    if ([string]::IsNullOrWhiteSpace($type)) {
        return $null
    }
    if ($type.StartsWith('MyObjectBuilder_', [System.StringComparison]::Ordinal)) {
        $type = $type.Substring('MyObjectBuilder_'.Length)
    }

    return [pscustomobject]@{
        Type = $type
        Subtype = $subtype
        Key = "$type/$subtype"
    }
}

function Get-XmlFiles {
    param([Parameter(Mandatory = $true)][string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Path not found: $Path"
    }

    return @(Get-ChildItem -LiteralPath $Path -Recurse -File -Filter '*.sbc')
}

function Get-CatalogData {
    param([Parameter(Mandatory = $true)][string] $Path)

    $content = Get-Content -LiteralPath $Path -Raw
    $metadataMatch = [regex]::Match($content, 'private const string ResearchMetadataData =\s*@\"\r?\n(?<data>.*?)\r?\n\";', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    $entryMatch = [regex]::Match($content, 'private const string EntryData =\s*@\"\r?\n(?<data>.*?)\r?\n\";', [System.Text.RegularExpressions.RegexOptions]::Singleline)
    if (-not $metadataMatch.Success -or -not $entryMatch.Success) {
        throw "Could not parse generated research catalog: $Path"
    }

    $metadata = [System.Collections.Generic.List[object]]::new()
    foreach ($line in $metadataMatch.Groups['data'].Value.Split([char[]]@("`r", "`n"), [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $fields = $line.Split('|')
        if ($fields.Length -ne 5) {
            throw "Invalid catalog metadata line: $line"
        }

        $metadata.Add([pscustomobject]@{
            ResearchId = $fields[0]
            DisplayName = $fields[1]
            GroupSubtype = $fields[2]
            UnlockerSubtype = $fields[3]
            Tier = $fields[4]
        }) | Out-Null
    }

    $entries = [System.Collections.Generic.List[object]]::new()
    foreach ($line in $entryMatch.Groups['data'].Value.Split([char[]]@("`r", "`n"), [System.StringSplitOptions]::RemoveEmptyEntries)) {
        $fields = $line.Split('|')
        if ($fields.Length -ne 2) {
            throw "Invalid catalog entry line: $line"
        }

        $entries.Add([pscustomobject]@{
            BlockKey = $fields[0]
            ResearchId = $fields[1]
        }) | Out-Null
    }

    return [pscustomobject]@{
        Metadata = @($metadata)
        Entries = @($entries)
    }
}

function Get-BlueprintItems {
    param([AllowNull()][object] $Container)

    $items = [System.Collections.Generic.List[object]]::new()
    if ($null -eq $Container) {
        return @()
    }

    if ($Container.PSObject.Properties['Item']) {
        foreach ($item in @($Container.Item)) {
            if ($null -eq $item) {
                continue
            }

            $items.Add([pscustomobject]@{
                TypeId = Get-Text $item.TypeId
                SubtypeId = Get-Text $item.SubtypeId
                Amount = Get-Number $item.Amount 1.0
            }) | Out-Null
        }
    }

    return @($items)
}

function Get-BlueprintResultItems {
    param([Parameter(Mandatory = $true)][object] $Blueprint)

    $items = [System.Collections.Generic.List[object]]::new()
    if ($Blueprint.PSObject.Properties['Result'] -and $null -ne $Blueprint.Result) {
        foreach ($item in @($Blueprint.Result)) {
            $items.Add([pscustomobject]@{
                TypeId = Get-Text $item.TypeId
                SubtypeId = Get-Text $item.SubtypeId
                Amount = Get-Number $item.Amount 1.0
            }) | Out-Null
        }
    }

    if ($Blueprint.PSObject.Properties['Results'] -and $null -ne $Blueprint.Results) {
        foreach ($item in Get-BlueprintItems $Blueprint.Results) {
            $items.Add($item) | Out-Null
        }
    }

    return @($items)
}

function Get-Blueprints {
    param([Parameter(Mandatory = $true)][string] $DataRoot)

    $blueprints = [System.Collections.Generic.List[object]]::new()
    foreach ($file in Get-XmlFiles $DataRoot) {
        if ($file.Name -notlike 'Blueprint*.sbc') {
            continue
        }

        try {
            [xml] $xml = Get-Content -LiteralPath $file.FullName -Raw
        }
        catch {
            Write-Warning "Skipping unreadable blueprint file: $($file.FullName)"
            continue
        }

        if (-not $xml.Definitions -or -not $xml.Definitions.PSObject.Properties['Blueprints']) {
            continue
        }
        if (-not $xml.Definitions.Blueprints.PSObject.Properties['Blueprint']) {
            continue
        }

        foreach ($definition in @($xml.Definitions.Blueprints.Blueprint)) {
            if ($null -eq $definition -or $null -eq $definition.Id) {
                continue
            }

            $id = Get-DefinitionId $definition.Id
            if ($null -eq $id) {
                continue
            }

            $blueprints.Add([pscustomobject]@{
                Id = $id
                Prerequisites = @(Get-BlueprintItems $definition.Prerequisites)
                Results = @(Get-BlueprintResultItems $definition)
                TimeSeconds = Get-Number $definition.BaseProductionTimeInSeconds 0.0
                File = $file.Name
            }) | Out-Null
        }
    }

    return @($blueprints)
}

function Get-IngotComplexityBySubtype {
    param([Parameter(Mandatory = $true)][object[]] $Blueprints)

    $weights = @{}
    foreach ($blueprint in $Blueprints) {
        if ($blueprint.Prerequisites.Count -ne 1) {
            continue
        }

        $input = $blueprint.Prerequisites[0]
        if ($input.TypeId -ne 'Ore' -or $input.Amount -le 0.0) {
            continue
        }

        foreach ($result in $blueprint.Results) {
            if ($result.TypeId -ne 'Ingot' -or $result.Amount -le 0.0) {
                continue
            }

            if ($result.SubtypeId -ne $input.SubtypeId) {
                continue
            }

            $yieldPerOre = $result.Amount / $input.Amount
            if ($yieldPerOre -le 0.0) {
                continue
            }

            $raw = (1.0 / $yieldPerOre) * (1.0 + [Math]::Max(0.0, $blueprint.TimeSeconds))
            $score = [Math]::Max(1.0, 1.0 + [Math]::Log($raw, 2.0))
            if (-not $weights.ContainsKey($result.SubtypeId) -or $score -lt $weights[$result.SubtypeId]) {
                $weights[$result.SubtypeId] = $score
            }
        }
    }

    if (-not $weights.ContainsKey('Stone')) {
        $weights['Stone'] = 1.0
    }
    if (-not $weights.ContainsKey('Scrap')) {
        if ($weights.ContainsKey('Iron')) {
            $weights['Scrap'] = $weights['Iron']
        }
        else {
            $weights['Scrap'] = 1.5
        }
    }

    return $weights
}

function Get-ComponentComplexityBySubtype {
    param(
        [Parameter(Mandatory = $true)][object[]] $Blueprints,
        [Parameter(Mandatory = $true)][hashtable] $IngotComplexityBySubtype
    )

    $scores = @{}
    foreach ($blueprint in $Blueprints) {
        foreach ($result in $blueprint.Results) {
            if ($result.TypeId -ne 'Component' -or $result.Amount -le 0.0) {
                continue
            }

            $totalAmount = 0.0
            $weightedIngot = 0.0
            $maxIngot = 1.0
            foreach ($input in $blueprint.Prerequisites) {
                if ($input.Amount -le 0.0) {
                    continue
                }

                $perComponentAmount = $input.Amount / $result.Amount
                $inputScore = 4.0
                if ($input.TypeId -eq 'Ingot') {
                    if ($IngotComplexityBySubtype.ContainsKey($input.SubtypeId)) {
                        $inputScore = $IngotComplexityBySubtype[$input.SubtypeId]
                    }
                }
                elseif ($input.TypeId -eq 'Component' -and $scores.ContainsKey($input.SubtypeId)) {
                    $inputScore = $scores[$input.SubtypeId].Score
                }

                $totalAmount += $perComponentAmount
                $weightedIngot += $perComponentAmount * $inputScore
                if ($inputScore -gt $maxIngot) {
                    $maxIngot = $inputScore
                }
            }

            if ($totalAmount -le 0.0) {
                continue
            }

            $averageIngot = $weightedIngot / $totalAmount
            $assemblySeconds = [Math]::Max(0.0, $blueprint.TimeSeconds / $result.Amount)
            $score =
                1.0 +
                $averageIngot +
                ([Math]::Log(1.0 + $assemblySeconds, 2.0) * 1.35) +
                ([Math]::Log(1.0 + $totalAmount, 2.0) * 0.12) +
                ([Math]::Max(0.0, $maxIngot - $averageIngot) * 0.25)

            if (-not $scores.ContainsKey($result.SubtypeId) -or $score -lt $scores[$result.SubtypeId].Score) {
                $scores[$result.SubtypeId] = [pscustomobject]@{
                    Component = $result.SubtypeId
                    Score = $score
                    Blueprint = $blueprint.Id.Subtype
                    AssemblySeconds = $assemblySeconds
                    AverageIngotScore = $averageIngot
                    MaxIngotScore = $maxIngot
                    TotalIngredientAmount = $totalAmount
                }
            }
        }
    }

    return $scores
}

function Get-CubeBlockDefinitions {
    param(
        [Parameter(Mandatory = $true)][string] $DataRoot,
        [Parameter(Mandatory = $true)][hashtable] $ComponentComplexityBySubtype
    )

    $definitions = @{}
    $cubeBlockRoot = Join-Path $DataRoot 'CubeBlocks'
    foreach ($file in Get-XmlFiles $cubeBlockRoot) {
        try {
            [xml] $xml = Get-Content -LiteralPath $file.FullName -Raw
        }
        catch {
            Write-Warning "Skipping unreadable cube block file: $($file.FullName)"
            continue
        }

        if (-not $xml.Definitions -or -not $xml.Definitions.PSObject.Properties['CubeBlocks']) {
            continue
        }
        if (-not $xml.Definitions.CubeBlocks.PSObject.Properties['Definition']) {
            continue
        }

        foreach ($definition in @($xml.Definitions.CubeBlocks.Definition)) {
            if ($null -eq $definition -or $null -eq $definition.Id) {
                continue
            }

            $id = Get-DefinitionId $definition.Id
            if ($null -eq $id -or $definitions.ContainsKey($id.Key)) {
                continue
            }

            if ($definition.PSObject.Properties['Public'] -and [string] $definition.Public -ieq 'false') {
                continue
            }

            $componentCount = 0.0
            $componentScoreTotal = 0.0
            $uniqueComponents = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
            if ($definition.PSObject.Properties['Components'] -and $definition.Components.PSObject.Properties['Component']) {
                foreach ($component in @($definition.Components.Component)) {
                    $subtype = Get-Text $component.Subtype
                    $count = Get-Number $component.Count 0.0
                    if ([string]::IsNullOrWhiteSpace($subtype) -or $count -le 0.0) {
                        continue
                    }

                    $componentScore = 5.0
                    if ($ComponentComplexityBySubtype.ContainsKey($subtype)) {
                        $componentScore = $ComponentComplexityBySubtype[$subtype].Score
                    }
                    elseif ($subtype -match 'Proto|Super|Reactor|Zone|Quantum') {
                        $componentScore = 9.0
                    }

                    $componentCount += $count
                    $componentScoreTotal += $count * $componentScore
                    $null = $uniqueComponents.Add($subtype)
                }
            }

            $baseComplexity = if ($componentCount -gt 0.0) { $componentScoreTotal / $componentCount } else { 4.0 }
            $diversityBonus = [Math]::Log(1.0 + [Math]::Max(1, $uniqueComponents.Count), 2.0) * 0.3
            $buildSeconds = 0.0
            if ($definition.PSObject.Properties['BuildTimeSeconds']) {
                $buildSeconds = Get-Number $definition.BuildTimeSeconds 0.0
            }

            $cubeSize = ''
            if ($definition.PSObject.Properties['CubeSize']) {
                $cubeSize = Get-Text $definition.CubeSize
            }

            $blockPairName = ''
            if ($definition.PSObject.Properties['BlockPairName']) {
                $blockPairName = Get-Text $definition.BlockPairName
            }

            $displayName = ''
            if ($definition.PSObject.Properties['DisplayName']) {
                $displayName = Get-Text $definition.DisplayName
            }

            $sizeX = 1.0
            $sizeY = 1.0
            $sizeZ = 1.0
            if ($definition.PSObject.Properties['Size']) {
                if ($definition.Size.PSObject.Properties['x']) { $sizeX = Get-Number $definition.Size.x 1.0 }
                if ($definition.Size.PSObject.Properties['y']) { $sizeY = Get-Number $definition.Size.y 1.0 }
                if ($definition.Size.PSObject.Properties['z']) { $sizeZ = Get-Number $definition.Size.z 1.0 }
            }

            $definitions[$id.Key] = [pscustomobject]@{
                Key = $id.Key
                Type = $id.Type
                Subtype = $id.Subtype
                BlockPairName = $blockPairName
                DisplayName = $displayName
                CubeSize = $cubeSize
                Size = ('{0}x{1}x{2}' -f (Format-Number $sizeX '0.#'), (Format-Number $sizeY '0.#'), (Format-Number $sizeZ '0.#'))
                BlockVolume = [Math]::Max(1.0, $sizeX * $sizeY * $sizeZ)
                BuildSeconds = $buildSeconds
                ComponentCount = $componentCount
                ComponentTypes = $uniqueComponents.Count
                Complexity = $baseComplexity + $diversityBonus
                File = $file.Name
            }
        }
    }

    return $definitions
}

function Get-Percentile {
    param([Parameter(Mandatory = $true)][double[]] $Values, [double] $Percentile)

    if ($Values.Count -eq 0) {
        return 0.0
    }

    $sorted = @($Values | Sort-Object)
    if ($sorted.Count -eq 1) {
        return [double] $sorted[0]
    }

    $rank = [Math]::Max(0.0, [Math]::Min(1.0, $Percentile)) * ($sorted.Count - 1)
    $lower = [int] [Math]::Floor($rank)
    $upper = [int] [Math]::Ceiling($rank)
    if ($lower -eq $upper) {
        return [double] $sorted[$lower]
    }

    $weight = $rank - $lower
    return ([double] $sorted[$lower] * (1.0 - $weight)) + ([double] $sorted[$upper] * $weight)
}

function Get-RepresentativeBuildSeconds {
    param([Parameter(Mandatory = $true)][object[]] $Definitions)

    $largeBuildSeconds = Get-HighNormalLargeGridBuildSeconds -Definitions $Definitions
    if ($largeBuildSeconds -gt 0.0) {
        return $largeBuildSeconds
    }

    $pool = @(
        $Definitions | Where-Object { $_.BuildSeconds -gt 0.0 }
    )
    if ($pool.Count -eq 0) {
        return 8.0
    }

    return Get-Percentile -Values ([double[]] @($pool | ForEach-Object { [double] $_.BuildSeconds })) -Percentile 0.25
}

function Get-RepresentativeBuildSecondsForResearchId {
    param(
        [Parameter(Mandatory = $true)][string] $ResearchId,
        [Parameter(Mandatory = $true)][object[]] $Definitions,
        [Parameter(Mandatory = $true)][string] $CubeSize
    )

    $override = Get-RepresentativeBuildSecondsOverride -ResearchId $ResearchId -Definitions $Definitions -CubeSize $CubeSize
    if ($override -gt 0.0) {
        return $override
    }

    if ($CubeSize -eq 'Large') {
        return Get-HighNormalLargeGridBuildSeconds -Definitions $Definitions
    }

    return Get-RepresentativeBuildSecondsForCubeSize -Definitions $Definitions -CubeSize $CubeSize
}

function Get-RepresentativeBuildSecondsOverride {
    param(
        [Parameter(Mandatory = $true)][string] $ResearchId,
        [Parameter(Mandatory = $true)][object[]] $Definitions,
        [Parameter(Mandatory = $true)][string] $CubeSize
    )

    if (-not $RepresentativeBlockKeyOverridesByResearchId.ContainsKey($ResearchId)) {
        return 0.0
    }

    $gridOverrides = $RepresentativeBlockKeyOverridesByResearchId[$ResearchId]
    if (-not $gridOverrides.ContainsKey($CubeSize)) {
        return 0.0
    }

    $buildSeconds = [System.Collections.Generic.List[double]]::new()
    foreach ($key in @($gridOverrides[$CubeSize])) {
        foreach ($definition in @($Definitions | Where-Object { $_.Key -eq $key -and $_.BuildSeconds -gt 0.0 })) {
            $buildSeconds.Add([double] $definition.BuildSeconds) | Out-Null
        }
    }

    if ($buildSeconds.Count -eq 0) {
        return 0.0
    }

    return Get-Median -Values ([double[]] $buildSeconds.ToArray())
}

function Get-HighNormalLargeGridBuildSeconds {
    param([Parameter(Mandatory = $true)][object[]] $Definitions)

    $pool = @(Get-NormalLargeGridAnchorDefinitions -Definitions $Definitions)
    if ($pool.Count -eq 0) {
        return 0.0
    }

    return Get-Percentile -Values ([double[]] @($pool | ForEach-Object { [double] $_.BuildSeconds })) -Percentile $LargeGridNormalAnchorPercentile
}

function Get-NormalLargeGridAnchorDefinitions {
    param([Parameter(Mandatory = $true)][object[]] $Definitions)

    $largeDefinitions = @($Definitions | Where-Object { $_.CubeSize -eq 'Large' -and $_.BuildSeconds -gt 0.0 })
    if ($largeDefinitions.Count -eq 0) {
        return @()
    }

    $unitDefinitions = @($largeDefinitions | Where-Object { $_.BlockVolume -le $NormalLargeGridAnchorMaximumVolume })
    if ($unitDefinitions.Count -gt 0) {
        return @($unitDefinitions)
    }

    $compactDefinitions = @($largeDefinitions | Where-Object { $_.BlockVolume -le $FallbackLargeGridAnchorMaximumVolume })
    if ($compactDefinitions.Count -gt 0) {
        return @($compactDefinitions)
    }

    return @($largeDefinitions)
}

function Get-RepresentativeBuildSecondsForCubeSize {
    param(
        [Parameter(Mandatory = $true)][object[]] $Definitions,
        [Parameter(Mandatory = $true)][string] $CubeSize
    )

    $pool = @($Definitions | Where-Object { $_.CubeSize -eq $CubeSize -and $_.BuildSeconds -gt 0.0 })
    if ($pool.Count -eq 0) {
        return 0.0
    }

    return Get-Percentile -Values ([double[]] @($pool | ForEach-Object { [double] $_.BuildSeconds })) -Percentile 0.25
}

function Get-Median {
    param([Parameter(Mandatory = $true)][double[]] $Values)

    return Get-Percentile -Values $Values -Percentile 0.5
}

function Get-BuildTimeFactor {
    param([double] $ActualBuildSeconds, [double] $ReferenceBuildSeconds)

    if ($ActualBuildSeconds -le 0.0 -or $ReferenceBuildSeconds -le 0.0) {
        return 1.0
    }

    $factor = [Math]::Sqrt($ActualBuildSeconds / $ReferenceBuildSeconds)
    if ($factor -lt $MinimumBuildTimeFactor) {
        return $MinimumBuildTimeFactor
    }

    if ($factor -gt $MaximumBuildTimeFactor) {
        return $MaximumBuildTimeFactor
    }

    return $factor
}

function Get-ResearchProgressAfterBlocks {
    param([double] $FullBlockReward, [double] $Blocks)

    $progress = 0.0
    $wholeBlocks = [int] [Math]::Floor($Blocks)
    $partialBlock = $Blocks - $wholeBlocks

    for ($i = 0; $i -lt $wholeBlocks; $i++) {
        $progress += $FullBlockReward * (1.0 - (0.5 * $progress))
        if ($progress -ge 1.0) {
            return $progress
        }
    }

    if ($partialBlock -gt 0.000001) {
        $progress += ($FullBlockReward * $partialBlock) * (1.0 - (0.5 * $progress))
    }

    return $progress
}

function Get-RequiredFullBlockReward {
    param([double] $TargetBlocks)

    $low = 0.001
    $high = 1.0
    for ($i = 0; $i -lt 80; $i++) {
        $mid = ($low + $high) / 2.0
        $progress = Get-ResearchProgressAfterBlocks -FullBlockReward $mid -Blocks $TargetBlocks
        if ($progress -ge 1.0) {
            $high = $mid
        }
        else {
            $low = $mid
        }
    }

    return $high
}

function Get-GroupBalanceRows {
    param(
        [Parameter(Mandatory = $true)][object[]] $CatalogMetadata,
        [Parameter(Mandatory = $true)][object[]] $CatalogEntries,
        [Parameter(Mandatory = $true)][hashtable] $DefinitionByKey
    )

    $definitionsByResearchId = @{}
    foreach ($entry in $CatalogEntries) {
        if (-not $DefinitionByKey.ContainsKey($entry.BlockKey)) {
            continue
        }

        if (-not $definitionsByResearchId.ContainsKey($entry.ResearchId)) {
            $definitionsByResearchId[$entry.ResearchId] = [System.Collections.Generic.List[object]]::new()
        }

        $definitionsByResearchId[$entry.ResearchId].Add($DefinitionByKey[$entry.BlockKey]) | Out-Null
    }

    $rawRows = [System.Collections.Generic.List[object]]::new()
    foreach ($metadata in $CatalogMetadata) {
        if (-not $definitionsByResearchId.ContainsKey($metadata.ResearchId)) {
            continue
        }

        $defs = @($definitionsByResearchId[$metadata.ResearchId])
        $complexityPool = @($defs | Where-Object { $_.CubeSize -eq 'Large' })
        if ($complexityPool.Count -eq 0) {
            $complexityPool = $defs
        }

        $complexity = Get-Median -Values ([double[]] @($complexityPool | ForEach-Object { [double] $_.Complexity }))
        $representativeBuildSeconds = Get-RepresentativeBuildSecondsForResearchId -ResearchId $metadata.ResearchId -Definitions $defs -CubeSize 'Large'
        $smallRepresentativeBuildSeconds = Get-RepresentativeBuildSecondsForResearchId -ResearchId $metadata.ResearchId -Definitions $defs -CubeSize 'Small'

        $rawRows.Add([pscustomobject]@{
            ResearchId = $metadata.ResearchId
            DisplayName = $metadata.DisplayName
            Tier = $metadata.Tier
            BlockCount = $defs.Count
            LargeBlockCount = @($defs | Where-Object { $_.CubeSize -eq 'Large' }).Count
            SmallBlockCount = @($defs | Where-Object { $_.CubeSize -eq 'Small' }).Count
            Complexity = $complexity
            RepresentativeBuildSeconds = $representativeBuildSeconds
            SmallRepresentativeBuildSeconds = $smallRepresentativeBuildSeconds
        }) | Out-Null
    }

    $complexities = [double[]] @($rawRows | ForEach-Object { [double] $_.Complexity })
    $lowComplexity = Get-Percentile -Values $complexities -Percentile 0.05
    $highComplexity = Get-Percentile -Values $complexities -Percentile 0.95
    $span = [Math]::Max(0.0001, $highComplexity - $lowComplexity)

    $rows = [System.Collections.Generic.List[object]]::new()
    foreach ($row in $rawRows) {
        $normalized = ($row.Complexity - $lowComplexity) / $span
        if ($normalized -lt 0.0) { $normalized = 0.0 }
        if ($normalized -gt 1.0) { $normalized = 1.0 }

        $targetBlocks = $MinimumTargetBlocks + (($MaximumTargetBlocks - $MinimumTargetBlocks) * $normalized)
        $requiredReward = Get-RequiredFullBlockReward -TargetBlocks $targetBlocks
        $referenceBuildSeconds = $row.RepresentativeBuildSeconds * [Math]::Pow($LargeGridBaseWorkReward / $requiredReward, 2.0)
        if ($referenceBuildSeconds -lt $MinimumReferenceBuildSeconds) { $referenceBuildSeconds = $MinimumReferenceBuildSeconds }
        if ($referenceBuildSeconds -gt $MaximumReferenceBuildSeconds) { $referenceBuildSeconds = $MaximumReferenceBuildSeconds }

        $smallGridBaseWorkReward = $DefaultSmallGridBaseWorkReward
        $smallGridRewardRatio = 0.0
        if ($row.SmallRepresentativeBuildSeconds -gt 0.0) {
            $smallBuildTimeFactor = Get-BuildTimeFactor -ActualBuildSeconds $row.SmallRepresentativeBuildSeconds -ReferenceBuildSeconds $referenceBuildSeconds
            if ($smallBuildTimeFactor -gt 0.0) {
                $smallGridBaseWorkReward = ($requiredReward * $DesiredSmallGridRewardRatio) / $smallBuildTimeFactor
                if ($smallGridBaseWorkReward -lt $MinimumSmallGridBaseWorkReward) { $smallGridBaseWorkReward = $MinimumSmallGridBaseWorkReward }
                if ($smallGridBaseWorkReward -gt $MaximumSmallGridBaseWorkReward) { $smallGridBaseWorkReward = $MaximumSmallGridBaseWorkReward }

                $smallGridRewardRatio = ($smallGridBaseWorkReward * $smallBuildTimeFactor) / $requiredReward
            }
        }

        $rows.Add([pscustomobject]@{
            ResearchId = $row.ResearchId
            DisplayName = $row.DisplayName
            Tier = $row.Tier
            BlockCount = $row.BlockCount
            LargeBlockCount = $row.LargeBlockCount
            SmallBlockCount = $row.SmallBlockCount
            Complexity = $row.Complexity
            NormalizedComplexity = $normalized
            TargetBlocks = $targetBlocks
            RequiredFullBlockReward = $requiredReward
            RepresentativeBuildSeconds = $row.RepresentativeBuildSeconds
            SmallRepresentativeBuildSeconds = $row.SmallRepresentativeBuildSeconds
            SmallGridBaseWorkReward = $smallGridBaseWorkReward
            SmallGridRewardRatio = $smallGridRewardRatio
            ReferenceBuildSeconds = $referenceBuildSeconds
        }) | Out-Null
    }

    return @($rows)
}

function Write-BalanceReport {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][object[]] $Rows,
        [Parameter(Mandatory = $true)][hashtable] $ComponentScores
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('# Working Knowledge Balance Report') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('Generated by `tools/generate-working-knowledge-balance.ps1`. Values are baked into `SchematicWorkRewardTable.cs`; runtime does not calculate component complexity.') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('## Method') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('- Derive ingot complexity from vanilla ore yield and refining time.') | Out-Null
    $lines.Add('- Derive component complexity from vanilla component recipes and assembly time.') | Out-Null
    $lines.Add('- Derive block complexity from the average sophistication of its components, with a small diversity bonus.') | Out-Null
    $lines.Add('- Derive schematic complexity from the median large-grid block complexity in each schematic family.') | Out-Null
    $lines.Add('- Map schematic complexity to a research target between 6 and 12 normal-anchor full large-block grinds.') | Out-Null
    $lines.Add('- Use the highest build time among normal 1x1x1 large-grid blocks as each family anchor so the most rewarding normal variant lands on target while oversized blocks still reward extra work.') | Out-Null
    $lines.Add('- If a family has no 1x1x1 large-grid block, fall back to compact large-grid blocks before considering oversized variants.') | Out-Null
    $lines.Add('- Override anchor build times for known mixed-size families so normal vanilla variants anchor the target instead of decorative, tiny, or large alternate variants.') | Out-Null
    $lines.Add('- Solve each small-grid base reward so a small-grid anchor block pays roughly 25% of the large-grid anchor block after build-time scaling.') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('## Schematic Targets') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('| Research ID | Complexity | Target Blocks | Large Anchor Seconds | Small Anchor Seconds | Small Base | Small/Large Reward | Reference Build Seconds | Large Blocks | Small Blocks |') | Out-Null
    $lines.Add('|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|') | Out-Null
    foreach ($row in $Rows) {
        $smallRepresentative = if ($row.SmallRepresentativeBuildSeconds -gt 0.0) { Format-Number $row.SmallRepresentativeBuildSeconds '0.0' } else { '-' }
        $smallRatio = if ($row.SmallGridRewardRatio -gt 0.0) { Format-Number $row.SmallGridRewardRatio '0.00' } else { '-' }
        $lines.Add(('| `{0}` | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} |' -f
            $row.ResearchId,
            (Format-Number $row.Complexity '0.00'),
            (Format-Number $row.TargetBlocks '0.0'),
            (Format-Number $row.RepresentativeBuildSeconds '0.0'),
            $smallRepresentative,
            (Format-Number $row.SmallGridBaseWorkReward '0.####'),
            $smallRatio,
            (Format-Number $row.ReferenceBuildSeconds '0.0'),
            $row.LargeBlockCount,
            $row.SmallBlockCount)) | Out-Null
    }

    $lines.Add('') | Out-Null
    $lines.Add('## Component Complexity') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('| Component | Score | Blueprint | Assembly Seconds |') | Out-Null
    $lines.Add('|---|---:|---|---:|') | Out-Null
    foreach ($component in @($ComponentScores.Values | Sort-Object Score, Component)) {
        $lines.Add(('| `{0}` | {1} | `{2}` | {3} |' -f
            $component.Component,
            (Format-Number $component.Score '0.00'),
            $component.Blueprint,
            (Format-Number $component.AssemblySeconds '0.##'))) | Out-Null
    }

    Set-TextFileNoBom -Path $Path -Lines ([string[]] $lines)
}

function Escape-MarkdownTableText {
    param([AllowNull()][object] $Value)

    if ($null -eq $Value) {
        return ''
    }

    return ([string] $Value).Replace('|', '\|')
}

function Get-ResearchBlocksToUnlock {
    param([double] $FullBlockReward)

    if ($FullBlockReward -le 0.0) {
        return [double]::PositiveInfinity
    }

    $low = 0.0
    $high = 1.0
    while ((Get-ResearchProgressAfterBlocks -FullBlockReward $FullBlockReward -Blocks $high) -lt 1.0 -and $high -lt 4096.0) {
        $high *= 2.0
    }

    if ($high -ge 4096.0) {
        return [double]::PositiveInfinity
    }

    for ($i = 0; $i -lt 64; $i++) {
        $mid = ($low + $high) / 2.0
        if ((Get-ResearchProgressAfterBlocks -FullBlockReward $FullBlockReward -Blocks $mid) -ge 1.0) {
            $high = $mid
        }
        else {
            $low = $mid
        }
    }

    return $high
}

function Get-RewardReview {
    param(
        [Parameter(Mandatory = $true)][object] $Definition,
        [double] $FullWorkReward,
        [double] $ExpectedGridReward,
        [double] $ResearchBlocksToUnlock
    )

    $relative = if ($ExpectedGridReward -gt 0.0) { $FullWorkReward / $ExpectedGridReward } else { 1.0 }
    $notes = [System.Collections.Generic.List[string]]::new()
    $status = 'ok'
    $isSmallGrid = $Definition.CubeSize -eq 'Small'
    $isLargeBlockShape = $Definition.BlockVolume -ge 18.0
    $isTinyOrFast = $Definition.BlockVolume -le 1.0 -or $Definition.BuildSeconds -le 5.0
    $isLongBuild = $Definition.BuildSeconds -ge 45.0

    if ($ResearchBlocksToUnlock -lt 3.0) {
        if ($isLongBuild -or $isLargeBlockShape) {
            $status = 'expected'
            $notes.Add('very fast unlock, but large/long-build block') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('very fast unlock without a large/long-build explanation') | Out-Null
        }
    }
    elseif (-not $isSmallGrid -and $ResearchBlocksToUnlock -lt 4.0) {
        if ($isLongBuild -or $isLargeBlockShape) {
            $status = 'expected'
            $notes.Add('fast large-grid unlock from large/long-build variant') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('fast large-grid unlock') | Out-Null
        }
    }
    elseif ($isSmallGrid -and $ResearchBlocksToUnlock -lt 10.0) {
        if ($isLongBuild -or $isLargeBlockShape) {
            $status = 'expected'
            $notes.Add('fast small-grid unlock from large/long-build variant') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('fast small-grid unlock') | Out-Null
        }
    }

    if (-not $isSmallGrid -and $ResearchBlocksToUnlock -gt 24.0) {
        if ($isTinyOrFast) {
            if ($status -eq 'ok') { $status = 'expected' }
            $notes.Add('slow large-grid unlock, but tiny/fast decorative-style block') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('slow large-grid unlock') | Out-Null
        }
    }
    elseif ($isSmallGrid -and $ResearchBlocksToUnlock -gt 96.0) {
        if ($isTinyOrFast) {
            if ($status -eq 'ok') { $status = 'expected' }
            $notes.Add('slow small-grid unlock, but tiny/fast block') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('slow small-grid unlock') | Out-Null
        }
    }

    if ($relative -gt 2.25) {
        if ($isLongBuild -or $isLargeBlockShape) {
            if ($status -eq 'ok') { $status = 'expected' }
            $notes.Add('high reward versus family anchor, explained by size/time') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('high reward versus family anchor') | Out-Null
        }
    }
    elseif ($relative -lt 0.4) {
        if ($isTinyOrFast) {
            if ($status -eq 'ok') { $status = 'expected' }
            $notes.Add('low reward versus family anchor, explained by tiny/fast block') | Out-Null
        }
        else {
            $status = 'review'
            $notes.Add('low reward versus family anchor') | Out-Null
        }
    }

    if ($notes.Count -eq 0) {
        $notes.Add('within expected range') | Out-Null
    }

    return [pscustomobject]@{
        Status = $status
        RelativeToExpected = $relative
        Notes = ($notes -join '; ')
    }
}

function Get-BlockRewardAnalysisRows {
    param(
        [Parameter(Mandatory = $true)][object[]] $BalanceRows,
        [Parameter(Mandatory = $true)][object[]] $CatalogEntries,
        [Parameter(Mandatory = $true)][hashtable] $DefinitionByKey
    )

    $balanceByResearchId = @{}
    foreach ($row in $BalanceRows) {
        $balanceByResearchId[$row.ResearchId] = $row
    }

    $analysisRows = [System.Collections.Generic.List[object]]::new()
    foreach ($entry in $CatalogEntries) {
        if (-not $DefinitionByKey.ContainsKey($entry.BlockKey)) {
            $analysisRows.Add([pscustomobject]@{
                ResearchId = $entry.ResearchId
                BlockKey = $entry.BlockKey
                Type = ''
                Subtype = ''
                BlockPairName = ''
                CubeSize = ''
                Size = ''
                BlockVolume = 0.0
                BuildSeconds = 0.0
                ComponentCount = 0.0
                Complexity = 0.0
                FullWorkReward = 0.0
                ResearchBlocksToUnlock = [double]::PositiveInfinity
                RelativeToRepresentative = 0.0
                RelativeToExpectedGrid = 0.0
                Status = 'review'
                Notes = 'catalog entry did not resolve to a public cube block definition'
            }) | Out-Null
            continue
        }

        if (-not $balanceByResearchId.ContainsKey($entry.ResearchId)) {
            continue
        }

        $definition = $DefinitionByKey[$entry.BlockKey]
        $balance = $balanceByResearchId[$entry.ResearchId]
        $isSmallGrid = $definition.CubeSize -eq 'Small'
        $baseWorkReward = if ($isSmallGrid) { $balance.SmallGridBaseWorkReward } else { $LargeGridBaseWorkReward }
        $buildTimeFactor = Get-BuildTimeFactor -ActualBuildSeconds $definition.BuildSeconds -ReferenceBuildSeconds $balance.ReferenceBuildSeconds
        $fullWorkReward = $baseWorkReward * $buildTimeFactor
        $researchBlocksToUnlock = Get-ResearchBlocksToUnlock -FullBlockReward $fullWorkReward
        $representativeReward = [Math]::Max(0.000001, $balance.RequiredFullBlockReward)
        $expectedGridReward = if ($isSmallGrid) { $representativeReward * $DesiredSmallGridRewardRatio } else { $representativeReward }
        $review = Get-RewardReview -Definition $definition -FullWorkReward $fullWorkReward -ExpectedGridReward $expectedGridReward -ResearchBlocksToUnlock $researchBlocksToUnlock

        $analysisRows.Add([pscustomobject]@{
            ResearchId = $entry.ResearchId
            BlockKey = $entry.BlockKey
            Type = $definition.Type
            Subtype = $definition.Subtype
            BlockPairName = $definition.BlockPairName
            CubeSize = $definition.CubeSize
            Size = $definition.Size
            BlockVolume = $definition.BlockVolume
            BuildSeconds = $definition.BuildSeconds
            ComponentCount = $definition.ComponentCount
            Complexity = $definition.Complexity
            FullWorkReward = $fullWorkReward
            ResearchBlocksToUnlock = $researchBlocksToUnlock
            RelativeToRepresentative = $fullWorkReward / $representativeReward
            RelativeToExpectedGrid = $review.RelativeToExpected
            Status = $review.Status
            Notes = $review.Notes
        }) | Out-Null
    }

    return @($analysisRows)
}

function Get-DistributionSummary {
    param([Parameter(Mandatory = $true)][double[]] $Values)

    $finite = [double[]] @($Values | Where-Object { -not [double]::IsInfinity($_) -and -not [double]::IsNaN($_) })
    if ($finite.Count -eq 0) {
        return [pscustomobject]@{
            Minimum = 0.0
            P05 = 0.0
            P25 = 0.0
            Median = 0.0
            P75 = 0.0
            P95 = 0.0
            Maximum = 0.0
        }
    }

    return [pscustomobject]@{
        Minimum = Get-Percentile -Values $finite -Percentile 0.0
        P05 = Get-Percentile -Values $finite -Percentile 0.05
        P25 = Get-Percentile -Values $finite -Percentile 0.25
        Median = Get-Percentile -Values $finite -Percentile 0.5
        P75 = Get-Percentile -Values $finite -Percentile 0.75
        P95 = Get-Percentile -Values $finite -Percentile 0.95
        Maximum = Get-Percentile -Values $finite -Percentile 1.0
    }
}

function Add-BlockRewardReportRows {
    param(
        [Parameter(Mandatory = $true)] $ReportLines,
        [AllowEmptyCollection()][object[]] $Rows,
        [Parameter(Mandatory = $true)][string] $Heading
    )

    $ReportLines.Add("## $Heading") | Out-Null
    $ReportLines.Add('') | Out-Null
    if ($Rows.Count -eq 0) {
        $ReportLines.Add('No rows.') | Out-Null
        $ReportLines.Add('') | Out-Null
        return
    }

    $ReportLines.Add('| Status | Research ID | Grid | Block | Pair | Size | Build Seconds | Reward | Repeated Grinds | Grid Ratio | Notes |') | Out-Null
    $ReportLines.Add('|---|---|---|---|---|---:|---:|---:|---:|---:|---|') | Out-Null
    foreach ($row in $Rows) {
        $ReportLines.Add(('| {0} | `{1}` | {2} | `{3}` | {4} | {5} | {6} | {7} | {8} | {9} | {10} |' -f
            $row.Status,
            (Escape-MarkdownTableText $row.ResearchId),
            (Escape-MarkdownTableText $row.CubeSize),
            (Escape-MarkdownTableText $row.BlockKey),
            (Escape-MarkdownTableText $row.BlockPairName),
            (Escape-MarkdownTableText $row.Size),
            (Format-Number $row.BuildSeconds '0.#'),
            (Format-Number $row.FullWorkReward '0.0000'),
            (Format-Number $row.ResearchBlocksToUnlock '0.0'),
            (Format-Number $row.RelativeToExpectedGrid '0.00'),
            (Escape-MarkdownTableText $row.Notes))) | Out-Null
    }
    $ReportLines.Add('') | Out-Null
}

function Write-BlockRewardReport {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][object[]] $Rows
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $rewardDistribution = Get-DistributionSummary -Values ([double[]] @($Rows | ForEach-Object { [double] $_.FullWorkReward }))
    $unlockDistribution = Get-DistributionSummary -Values ([double[]] @($Rows | ForEach-Object { [double] $_.ResearchBlocksToUnlock }))
    $reviewRows = @($Rows | Where-Object { $_.Status -eq 'review' } | Sort-Object ResearchBlocksToUnlock, ResearchId, BlockKey)
    $expectedRows = @($Rows | Where-Object { $_.Status -eq 'expected' } | Sort-Object ResearchBlocksToUnlock, ResearchId, BlockKey)
    $unresolvedRows = @($Rows | Where-Object { [string]::IsNullOrWhiteSpace($_.CubeSize) })
    $fastestRows = @($Rows | Where-Object { -not [double]::IsInfinity($_.ResearchBlocksToUnlock) } | Sort-Object ResearchBlocksToUnlock, ResearchId, BlockKey | Select-Object -First 40)
    $slowestRows = @($Rows | Where-Object { -not [double]::IsInfinity($_.ResearchBlocksToUnlock) } | Sort-Object @{ Expression = 'ResearchBlocksToUnlock'; Descending = $true }, ResearchId, BlockKey | Select-Object -First 40)

    $lines.Add('# Working Knowledge Block Reward Audit') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('Generated by `tools/generate-working-knowledge-balance.ps1`. This report evaluates every generated research catalog block against the current schematic work reward table. It does not change tuning.') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('## Summary') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add(('- Cataloged block rows: {0}' -f $Rows.Count)) | Out-Null
    $lines.Add(('- Unresolved catalog rows: {0}' -f $unresolvedRows.Count)) | Out-Null
    $lines.Add(('- Rows marked `review`: {0}' -f $reviewRows.Count)) | Out-Null
    $lines.Add(('- Rows marked `expected`: {0}' -f $expectedRows.Count)) | Out-Null
    $lines.Add('- `Reward` is the medium-difficulty full-grind work reward before the research curve is applied.') | Out-Null
    $lines.Add('- `Repeated Grinds` estimates how many full grinds of that same block would unlock the schematic at medium settings with the current research curve.') | Out-Null
    $lines.Add('- `Grid Ratio` compares the block against its expected family anchor: `1.00` for the large-grid anchor block, `1.00` for the small-grid anchor block after the intended 0.25 small-grid factor.') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('## Distribution') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('| Metric | Min | P05 | P25 | Median | P75 | P95 | Max |') | Out-Null
    $lines.Add('|---|---:|---:|---:|---:|---:|---:|---:|') | Out-Null
    $lines.Add(('| Full Work Reward | {0} | {1} | {2} | {3} | {4} | {5} | {6} |' -f
        (Format-Number $rewardDistribution.Minimum '0.0000'),
        (Format-Number $rewardDistribution.P05 '0.0000'),
        (Format-Number $rewardDistribution.P25 '0.0000'),
        (Format-Number $rewardDistribution.Median '0.0000'),
        (Format-Number $rewardDistribution.P75 '0.0000'),
        (Format-Number $rewardDistribution.P95 '0.0000'),
        (Format-Number $rewardDistribution.Maximum '0.0000'))) | Out-Null
    $lines.Add(('| Repeated Grinds | {0} | {1} | {2} | {3} | {4} | {5} | {6} |' -f
        (Format-Number $unlockDistribution.Minimum '0.0'),
        (Format-Number $unlockDistribution.P05 '0.0'),
        (Format-Number $unlockDistribution.P25 '0.0'),
        (Format-Number $unlockDistribution.Median '0.0'),
        (Format-Number $unlockDistribution.P75 '0.0'),
        (Format-Number $unlockDistribution.P95 '0.0'),
        (Format-Number $unlockDistribution.Maximum '0.0'))) | Out-Null
    $lines.Add('') | Out-Null

    Add-BlockRewardReportRows -ReportLines $lines -Rows @($reviewRows | Select-Object -First 120) -Heading 'Review Candidates'
    Add-BlockRewardReportRows -ReportLines $lines -Rows @($expectedRows | Select-Object -First 120) -Heading 'Expected Outliers'
    Add-BlockRewardReportRows -ReportLines $lines -Rows $fastestRows -Heading 'Fastest Repeated Unlocks'
    Add-BlockRewardReportRows -ReportLines $lines -Rows $slowestRows -Heading 'Slowest Repeated Unlocks'

    $lines.Add('## Family Summary') | Out-Null
    $lines.Add('') | Out-Null
    $lines.Add('| Research ID | Blocks | Review | Expected | Min Reward | Max Reward | Min Grinds | Max Grinds |') | Out-Null
    $lines.Add('|---|---:|---:|---:|---:|---:|---:|---:|') | Out-Null
    foreach ($group in @($Rows | Group-Object ResearchId | Sort-Object Name)) {
        $groupRows = @($group.Group)
        $groupReward = Get-DistributionSummary -Values ([double[]] @($groupRows | ForEach-Object { [double] $_.FullWorkReward }))
        $groupUnlock = Get-DistributionSummary -Values ([double[]] @($groupRows | ForEach-Object { [double] $_.ResearchBlocksToUnlock }))
        $lines.Add(('| `{0}` | {1} | {2} | {3} | {4} | {5} | {6} | {7} |' -f
            (Escape-MarkdownTableText $group.Name),
            $groupRows.Count,
            @($groupRows | Where-Object { $_.Status -eq 'review' }).Count,
            @($groupRows | Where-Object { $_.Status -eq 'expected' }).Count,
            (Format-Number $groupReward.Minimum '0.0000'),
            (Format-Number $groupReward.Maximum '0.0000'),
            (Format-Number $groupUnlock.Minimum '0.0'),
            (Format-Number $groupUnlock.Maximum '0.0'))) | Out-Null
    }
    $lines.Add('') | Out-Null

    Add-BlockRewardReportRows -ReportLines $lines -Rows (@($Rows | Sort-Object ResearchId, CubeSize, Type, Subtype)) -Heading 'All Cataloged Blocks'

    Set-TextFileNoBom -Path $Path -Lines ([string[]] $lines)
}

function Update-RewardTable {
    param(
        [Parameter(Mandatory = $true)][string] $Path,
        [Parameter(Mandatory = $true)][object[]] $Rows
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($row in $Rows) {
        $smallComment = if ($row.SmallRepresentativeBuildSeconds -gt 0.0) {
            ', small {0}s x{1}' -f (Format-Number $row.SmallRepresentativeBuildSeconds '0.#'), (Format-Number $row.SmallGridRewardRatio '0.00')
        }
        else {
            ''
        }

        $lines.Add(('            Reward("{0}", {1}, {2}, {3}), // target {4} blocks, complexity {5}, anchor {6}s{7}' -f
            $row.ResearchId,
            (Format-Number $LargeGridBaseWorkReward '0.####'),
            (Format-Number $row.SmallGridBaseWorkReward '0.####'),
            (Format-Number $row.ReferenceBuildSeconds '0.###'),
            (Format-Number $row.TargetBlocks '0.0'),
            (Format-Number $row.Complexity '0.00'),
            (Format-Number $row.RepresentativeBuildSeconds '0.#'),
            $smallComment)) | Out-Null
    }

    $rowsText = ($lines -join [Environment]::NewLine) + [Environment]::NewLine
    $content = Get-Content -LiteralPath $Path -Raw
    $pattern = '(?s)(        private static readonly SchematicWorkReward\[\] Rewards = new SchematicWorkReward\[\]\s*\{\r?\n)(.*?)(        \};)'
    $regex = [regex] $pattern
    if (-not $regex.IsMatch($content)) {
        throw "Could not find SchematicWorkRewardTable Rewards array in: $Path"
    }

    $updated = $regex.Replace($content, {
        param($match)
        return $match.Groups[1].Value + $rowsText + $match.Groups[3].Value
    }, 1)
    $updated = $updated.TrimEnd("`r", "`n") + [Environment]::NewLine

    Set-RawTextFileNoBom -Path $Path -Content $updated
}

$catalog = Get-CatalogData -Path $CatalogPath
$blueprints = Get-Blueprints -DataRoot $SpaceEngineersData
$ingotScores = Get-IngotComplexityBySubtype -Blueprints $blueprints
$componentScores = Get-ComponentComplexityBySubtype -Blueprints $blueprints -IngotComplexityBySubtype $ingotScores
$definitions = Get-CubeBlockDefinitions -DataRoot $SpaceEngineersData -ComponentComplexityBySubtype $componentScores
$rows = Get-GroupBalanceRows -CatalogMetadata $catalog.Metadata -CatalogEntries $catalog.Entries -DefinitionByKey $definitions
$blockRewardRows = Get-BlockRewardAnalysisRows -BalanceRows $rows -CatalogEntries $catalog.Entries -DefinitionByKey $definitions

Write-BalanceReport -Path $ReportPath -Rows $rows -ComponentScores $componentScores
Write-BlockRewardReport -Path $BlockRewardReportPath -Rows $blockRewardRows

if ($UpdateSource) {
    Update-RewardTable -Path $RewardTablePath -Rows $rows
}

Write-Host ("Generated Working Knowledge balance report: {0}" -f $ReportPath)
Write-Host ("Generated Working Knowledge block reward audit: {0}" -f $BlockRewardReportPath)
if ($UpdateSource) {
    Write-Host ("Updated schematic work reward table: {0}" -f $RewardTablePath)
}
