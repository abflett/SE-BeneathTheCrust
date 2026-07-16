[CmdletBinding()]
param(
    [string] $OutputRoot = (Join-Path $env:APPDATA 'SpaceEngineers\Mods')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$OutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)

function Write-Utf8NoBom {
    param([string] $Path, [string] $Text)
    $directory = Split-Path -Parent $Path
    if (-not (Test-Path -LiteralPath $directory)) { New-Item -ItemType Directory -Path $directory | Out-Null }
    [System.IO.File]::WriteAllText($Path, $Text, [System.Text.UTF8Encoding]::new($false))
}

function New-ConflictLayer {
    param(
        [string] $FolderName,
        [string] $DisplayName,
        [string] $ResearchId,
        [string] $GroupDisplayName,
        [string] $Tier,
        [string] $GroupSubtype,
        [string] $UnlockerSubtype,
        [string] $Description
    )
    $root = Join-Path $OutputRoot $FolderName
    $itemSubtype = 'WkKnSchematic_' + (($ResearchId -replace '[^A-Za-z0-9]+', '_').Trim('_'))
    Write-Utf8NoBom (Join-Path $root 'modinfo.sbc') @"
<?xml version="1.0" encoding="utf-8"?>
<ModItem><Name>$DisplayName</Name><Description>Local Working Knowledge 0.13.0 conflict test layer.</Description><Version>1.0.0</Version><Makers>Beneath the Crust Test</Makers><IncludeInExport>True</IncludeInExport></ModItem>
"@
    Write-Utf8NoBom (Join-Path $root 'Data\WorkingKnowledge\schematic_groups.txt') @"
version = 1
$ResearchId | $GroupDisplayName | $Tier | $GroupSubtype | $UnlockerSubtype | $Description
"@
    Write-Utf8NoBom (Join-Path $root 'Data\WorkingKnowledge\block_mappings.txt') @"
override CubeBlock/LargeHeavyBlockArmorBlock = $ResearchId
override CubeBlock/SmallHeavyBlockArmorBlock = $ResearchId
"@
    Write-Utf8NoBom (Join-Path $root 'Data\ResearchBlocks.sbc') @"
<?xml version="1.0" encoding="utf-8"?>
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><ResearchBlocks>
  <ResearchBlock xsi:type="ResearchBlock"><Id Type="CubeBlock" Subtype="LargeHeavyBlockArmorBlock" /><UnlockedByGroups /></ResearchBlock>
  <ResearchBlock xsi:type="ResearchBlock"><Id Type="CubeBlock" Subtype="SmallHeavyBlockArmorBlock" /><UnlockedByGroups /></ResearchBlock>
</ResearchBlocks></Definitions>
"@
    Write-Utf8NoBom (Join-Path $root 'Data\ResearchUnlockerGroups.sbc') @"
<?xml version="1.0" encoding="utf-8"?>
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><ResearchGroups>
  <ResearchGroup xsi:type="ResearchGroup"><Id Type="MyObjectBuilder_ResearchGroupDefinition" Subtype="$GroupSubtype" /><Members /></ResearchGroup>
</ResearchGroups></Definitions>
"@
    Write-Utf8NoBom (Join-Path $root 'Data\ResearchUnlockers.sbc') @"
<?xml version="1.0" encoding="utf-8"?>
<Definitions><CubeBlocks><Definition>
  <Id><TypeId>CubeBlock</TypeId><SubtypeId>$UnlockerSubtype</SubtypeId></Id>
  <DisplayName>$GroupDisplayName</DisplayName><Description>$Description</Description>
  <Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon><Public>true</Public><GuiVisible>false</GuiVisible>
  <BlockPairName>$UnlockerSubtype</BlockPairName><CubeSize>Small</CubeSize><BlockTopology>TriangleMesh</BlockTopology>
  <Size x="1" y="1" z="1" /><ModelOffset x="0" y="0" z="0" /><Model>Models\Items\Datapad_Item.mwm</Model>
  <Components><Component Subtype="WkKnDataFragmentComponent" Count="100" /></Components><CriticalComponent Subtype="WkKnDataFragmentComponent" Index="0" />
  <MountPoints>
    <MountPoint Side="Front" StartX="0" StartY="0" EndX="1" EndY="1" /><MountPoint Side="Back" StartX="0" StartY="0" EndX="1" EndY="1" />
    <MountPoint Side="Left" StartX="0" StartY="0" EndX="1" EndY="1" /><MountPoint Side="Right" StartX="0" StartY="0" EndX="1" EndY="1" />
    <MountPoint Side="Bottom" StartX="0" StartY="0" EndX="1" EndY="1" /><MountPoint Side="Top" StartX="0" StartY="0" EndX="1" EndY="1" />
  </MountPoints>
  <BuildProgressModels><Model BuildPercentUpperBound="1.00" File="Models\Items\Datapad_Item.mwm" /></BuildProgressModels>
  <DeformationRatio>0.32</DeformationRatio><EdgeType>Light</EdgeType><BuildTimeSeconds>3</BuildTimeSeconds><PCU>1</PCU><IsAirTight>false</IsAirTight>
</Definition></CubeBlocks></Definitions>
"@
    Write-Utf8NoBom (Join-Path $root 'Data\PhysicalItems_ResearchSchematics.sbc') @"
<?xml version="1.0" encoding="utf-8"?>
<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><PhysicalItems>
  <PhysicalItem xsi:type="MyObjectBuilder_ConsumableItemDefinition">
    <Id><TypeId>ConsumableItem</TypeId><SubtypeId>$itemSubtype</SubtypeId></Id><DisplayName>$GroupDisplayName Data Schematic</DisplayName>
    <Description>$Description This durable test schematic is returned after use.</Description><Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon>
    <Size><X>0.2</X><Y>0.1</Y><Z>0.2</Z></Size><Mass>0.2</Mass><Volume>0.05</Volume><Model>Models\Items\Datapad_Item.mwm</Model>
    <PhysicalMaterial>Metal</PhysicalMaterial><Stats><Stat Name="RadiationImmunity" Value="1" Time="1" /></Stats><UseSound>PlayUsePowerKit</UseSound>
    <DepositAllEnabled>false</DepositAllEnabled><CanPlayerOrder>false</CanPlayerOrder><CanPlayerOffer>false</CanPlayerOffer>
  </PhysicalItem>
</PhysicalItems></Definitions>
"@
    return $root
}

$hard = New-ConflictLayer 'WKL Test - Hard Armor' 'WKL Test - Hard Armor' 'test.armor.hard' 'Hard Armor Schematics' 'Uncommon' 'WkKnLayer_Test_HardArmor' 'WkKnUnlocker_Test_HardArmor' 'Test schematics for reinforced heavy armor.'
$dense = New-ConflictLayer 'WKL Test - Dense Armor' 'WKL Test - Dense Armor' 'test.armor.dense' 'Dense Armor Schematics' 'Rare' 'WkKnLayer_Test_DenseArmor' 'WkKnUnlocker_Test_DenseArmor' 'Test schematics for extremely dense heavy armor.'

Write-Host "Deployed conflict test layers:"
Write-Host "  $hard"
Write-Host "  $dense"
Write-Host 'Add Working Knowledge first, then both test layers. The later test layer in the world mod list should win.'
