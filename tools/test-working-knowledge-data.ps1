[CmdletBinding()]
param(
    [string] $SpaceEngineersData = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Data'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$data = Join-Path $repoRoot 'mods/WorkingKnowledge/Data'
$catalog = Get-Content (Join-Path $data 'Scripts/WorkingKnowledge/Application/Research/Catalog/ResearchCatalog.generated.cs') -Raw
$entries = @{}
foreach ($match in [regex]::Matches($catalog, '(?m)^([^|\r\n]+/[^|\r\n]*)\|([^|\r\n]+)\r?$')) {
    $entries[$match.Groups[1].Value] = $match.Groups[2].Value
}
if ($entries.Count -eq 0) { throw 'No catalog entries found.' }

function Assert-Family([string] $Key, [string] $Expected) {
    if ($entries[$Key] -cne $Expected) { throw "$Key should use $Expected; found '$($entries[$Key])'." }
}

# Check the public installed blocks as well as their generated research definitions.
[xml] $research = Get-Content (Join-Path $data 'ResearchBlocks.sbc') -Raw
$researchIds = @{}
foreach ($block in $research.Definitions.ResearchBlocks.ResearchBlock) {
    $researchIds["$($block.Id.Type)/$($block.Id.Subtype)"] = $true
}
$publicCount = 0
foreach ($file in Get-ChildItem (Join-Path $SpaceEngineersData 'CubeBlocks') -Filter *.sbc) {
    [xml] $document = Get-Content $file.FullName -Raw
    foreach ($block in $document.SelectNodes('/Definitions/CubeBlocks/Definition')) {
        if ($block['Public'] -and $block['Public'].InnerText -ieq 'false') { continue }
        $id = $block['Id']
        $type = if ($id['TypeId']) { $id['TypeId'].InnerText } else { $id['Type'].InnerText }
        $subtype = if ($id['SubtypeId']) { $id['SubtypeId'].InnerText } elseif ($id['Subtype']) { $id['Subtype'].InnerText } else { '' }
        $key = $type.Replace('MyObjectBuilder_', '') + '/' + $subtype
        if (-not $entries.ContainsKey($key) -or -not $researchIds.ContainsKey($key)) { throw "Public block missing catalog/research entry: $key" }
        $publicCount++
    }
}

foreach ($size in @('LargeBlock', 'SmallBlock')) {
    Assert-Family "FunctionalBlock/${size}PrototechMicroEmitter" 'prototech.microwave_emitter'
    Assert-Family "FunctionalBlock/${size}SeparationCharge" 'mechanical.systems'
    Assert-Family "Cockpit/${size}JumpSeat" 'decor.habitat_fixtures'
    Assert-Family "SmallMissileLauncher/${size}TorpedoLauncher" 'weapons.fixed_weapon'
    Assert-Family "LaserAntenna/${size}LaserAntennaType2" 'communications'
}
Assert-Family 'Cockpit/LargeBlockJumpSeatL' 'decor.habitat_fixtures'
Assert-Family 'Cockpit/LargeBlockJumpSeatR' 'decor.habitat_fixtures'
Assert-Family 'Cockpit/LargeBlockCaptainChair' 'control.stations'
Assert-Family 'Cockpit/FighterCockpitType2' 'control.stations'
Assert-Family 'SmallMissileLauncher/LargeInsetFlareLauncher' 'weapons.fixed_weapon'
Assert-Family 'SmallMissileLauncher/SmallInsetFlareLauncher' 'weapons.fixed_weapon'
foreach ($key in @($entries.Keys)) {
    if ($key -match '^CubeBlock/(LargeBlock|SmallBlock)Intake') { Assert-Family $key 'decor.decorative_fixtures' }
}

# Our replacement cycling groups must never join different research families.
[xml] $variants = Get-Content (Join-Path $data 'BlockVariantGroups_Research.sbc') -Raw
foreach ($group in $variants.Definitions.BlockVariantGroups.BlockVariantGroup) {
    $families = @($group.Blocks.Block | ForEach-Object {
        $key = $_.Type.Replace('MyObjectBuilder_', '') + '/' + $_.Subtype
        if (-not $entries.ContainsKey($key)) { throw "Variant block missing catalog entry: $key" }
        $entries[$key]
    } | Sort-Object -Unique)
    if ($families.Count -ne 1) { throw "Mixed-family cycling group: $($group.Id.Subtype)" }
}
[xml] $radial = Get-Content (Join-Path $data 'RadialMenu_Research.sbc') -Raw
foreach ($subtype in @('SeparationChargeGroup', 'PrototechMicrowaveEmitterGroup')) {
    if (-not $radial.SelectSingleNode("//Item/Id[@Subtype='$subtype']")) { throw "Missing radial entry: $subtype" }
}

[xml] $unlockers = Get-Content (Join-Path $data 'ResearchUnlockers.sbc') -Raw
foreach ($block in $unlockers.Definitions.CubeBlocks.Definition) {
    if ($block.Public -ine 'true' -or $block.GuiVisible -ine 'false' -or ($block['Enabled'] -and $block['Enabled'].InnerText -ieq 'false')) {
        throw "Unlocker must remain loaded and public for the research graph: $($block.Id.SubtypeId)"
    }
}
foreach ($name in @('PhysicalItems.sbc', 'PhysicalItems_ResearchSchematics.sbc')) {
    [xml] $items = Get-Content (Join-Path $data $name) -Raw
    foreach ($item in $items.Definitions.PhysicalItems.PhysicalItem) {
        if ($item['Stats'] -or $item.Id.TypeId -cne 'ConsumableItem' -or $item.UseSound -cne 'PlayUsePowerKit') {
            throw "Unexpected research consumable definition: $($item.Id.SubtypeId)"
        }
    }
}
Write-Host "Validated $publicCount installed public block definitions, family assignments, cycling groups, radial entries, loaded unlockers, and stat-free consumables."
