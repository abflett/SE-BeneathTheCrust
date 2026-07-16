[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ToolkitRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$Validator = Join-Path $ToolkitRoot 'Validate.ps1'
$TestRoot = Join-Path ([System.IO.Path]::GetTempPath()) ('WkKnLayerTests-' + [Guid]::NewGuid().ToString('N'))

function Write-Utf8NoBom {
    param([string] $Path, [string] $Text)
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}

function New-TestLayer {
    param(
        [string] $Name,
        [string] $ResearchId,
        [string] $DisplayName,
        [string] $Tier,
        [string] $GroupSubtype,
        [string] $UnlockerSubtype,
        [string] $MappingResearchId,
        [switch] $Definitions,
        [switch] $OmitResearchBlocks
    )
    $root = Join-Path $TestRoot $Name
    $mapping = "BatteryBlock/LargeBlockBatteryBlock = $MappingResearchId"
    Write-Utf8NoBom (Join-Path $root 'Data\WorkingKnowledge\block_mappings.txt') $mapping
    if (-not $OmitResearchBlocks) {
        Write-Utf8NoBom (Join-Path $root 'Data\ResearchBlocks.sbc') @"
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><ResearchBlocks>
  <ResearchBlock xsi:type="ResearchBlock"><Id Type="BatteryBlock" Subtype="LargeBlockBatteryBlock" /><UnlockedByGroups /></ResearchBlock>
</ResearchBlocks></Definitions>
"@
    }
    if (-not [string]::IsNullOrWhiteSpace($ResearchId)) {
        Write-Utf8NoBom (Join-Path $root 'Data\WorkingKnowledge\schematic_groups.txt') @"
version = 1
$ResearchId | $DisplayName | $Tier | $GroupSubtype | $UnlockerSubtype | Test description for $DisplayName.
"@
    }
    if ($Definitions) {
        $itemSubtype = 'WkKnSchematic_' + (($ResearchId -replace '[^A-Za-z0-9]+', '_').Trim('_'))
        Write-Utf8NoBom (Join-Path $root 'Data\ResearchUnlockerGroups.sbc') @"
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><ResearchGroups>
  <ResearchGroup xsi:type="ResearchGroup"><Id Type="MyObjectBuilder_ResearchGroupDefinition" Subtype="$GroupSubtype" /><Members /></ResearchGroup>
</ResearchGroups></Definitions>
"@
        Write-Utf8NoBom (Join-Path $root 'Data\ResearchUnlockers.sbc') @"
<Definitions><CubeBlocks><Definition><Id><TypeId>CubeBlock</TypeId><SubtypeId>$UnlockerSubtype</SubtypeId></Id></Definition></CubeBlocks></Definitions>
"@
        Write-Utf8NoBom (Join-Path $root 'Data\PhysicalItems_ResearchSchematics.sbc') @"
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><PhysicalItems>
  <PhysicalItem xsi:type="MyObjectBuilder_ConsumableItemDefinition"><Id><TypeId>ConsumableItem</TypeId><SubtypeId>$itemSubtype</SubtypeId></Id></PhysicalItem>
</PhysicalItems></Definitions>
"@
    }
    return $root
}

function Assert-Equal {
    param([object] $Actual, [object] $Expected, [string] $Message)
    if ($Actual -ne $Expected) { throw "$Message Expected '$Expected', got '$Actual'." }
}

try {
    $hard = New-TestLayer 'HardArmor' 'armor.hard' 'Hard Armor Schematics' 'Uncommon' 'WkKnLayer_HardArmor' 'WkKnUnlocker_HardArmor' 'armor.hard' -Definitions
    $dense = New-TestLayer 'DenseArmor' 'armor.dense' 'Dense Armor Schematics' 'Rare' 'WkKnLayer_DenseArmor' 'WkKnUnlocker_DenseArmor' 'armor.dense' -Definitions
    $broken = New-TestLayer 'BrokenLateLayer' '' '' '' '' '' 'missing.group'
    $incomplete = New-TestLayer 'IncompleteGroup' 'armor.incomplete' 'Incomplete Armor Schematics' 'Rare' 'WkKnLayer_Incomplete' 'WkKnUnlocker_Incomplete' 'armor.incomplete'
    $rename = New-TestLayer 'HardArmorRename' 'armor.hard' 'Reinforced Hard Armor Schematics' 'Rare' 'WkKnLayer_HardArmor' 'WkKnUnlocker_HardArmor' 'armor.hard'
    $brokenRename = New-TestLayer 'BrokenHardArmorRename' 'armor.hard' 'Broken Hard Armor Schematics' 'Rare' 'WkKnLayer_MissingHardArmor' 'WkKnUnlocker_MissingHardArmor' 'armor.hard'
    $sharedWiring = New-TestLayer 'SharedWiringLater' 'armor.shared' 'Shared Wiring Armor Schematics' 'Rare' 'WkKnLayer_HardArmor' 'WkKnUnlocker_HardArmor' 'armor.shared' -Definitions
    $baseOnlyRemap = New-TestLayer 'BaseOnlyRemap' '' '' '' '' '' 'armor.heavy' -OmitResearchBlocks

    $hardThenDense = & $Validator -LayerPath @($hard, $dense) -PassThru
    Assert-Equal $hardThenDense.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.dense' 'Later Dense Armor mapping should win.'

    $denseThenHard = & $Validator -LayerPath @($dense, $hard) -PassThru
    Assert-Equal $denseThenHard.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.hard' 'Reversed priority should make Hard Armor win.'

    $hardThenBroken = & $Validator -LayerPath @($hard, $broken) -PassThru
    Assert-Equal $hardThenBroken.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.hard' 'Invalid higher-priority mapping should preserve the next valid winner.'

    $hardThenIncomplete = & $Validator -LayerPath @($hard, $incomplete) -PassThru
    Assert-Equal $hardThenIncomplete.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.hard' 'Higher-priority mapping to an incomplete group should preserve the next valid winner.'

    $renamed = & $Validator -LayerPath @($hard, $rename) -PassThru
    Assert-Equal $renamed.ActiveGroups['armor.hard'].DisplayName 'Reinforced Hard Armor Schematics' 'Later metadata for the same group ID should win.'
    Assert-Equal $renamed.ActiveGroups['armor.hard'].Tier 'Rare' 'Later tier for the same group ID should win.'

    $brokenRenameResult = & $Validator -LayerPath @($hard, $brokenRename) -PassThru
    Assert-Equal $brokenRenameResult.ActiveGroups['armor.hard'].DisplayName 'Hard Armor Schematics' 'An incomplete higher-priority declaration of the same group ID should preserve the next valid group.'

    $shared = & $Validator -LayerPath @($hard, $sharedWiring) -PassThru
    Assert-Equal $shared.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.shared' 'Higher-priority group should own shared definition IDs.'
    if ($shared.ActiveGroups.ContainsKey('armor.hard')) { throw 'Lower-priority group sharing definition IDs should be inactive.' }

    $baseOnly = & $Validator -LayerPath @($baseOnlyRemap) -PassThru
    Assert-Equal $baseOnly.WinningMappings['BatteryBlock/LargeBlockBatteryBlock'].ResearchId 'armor.heavy' 'A base block remap should not require a duplicate ResearchBlocks.sbc entry.'

    Write-Host 'Working Knowledge layer-priority resolution tests passed.'
}
finally {
    if (Test-Path -LiteralPath $TestRoot) { Remove-Item -LiteralPath $TestRoot -Recurse -Force }
}
