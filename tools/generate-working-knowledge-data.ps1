[CmdletBinding()]
param(
    [string] $SpaceEngineersData = 'C:\Program Files (x86)\Steam\steamapps\common\SpaceEngineers\Content\Data',
    [string] $OutputData = (Join-Path (Split-Path -Parent $PSScriptRoot) 'mods\WorkingKnowledge\Data'),
    [string] $DocsSchematicGroupsPath = '',
    [int] $VanillaResearchGroupCount = 32
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptOutput = Join-Path $OutputData 'Scripts\WorkingKnowledge'
$GeneratedCatalogDirectory = Join-Path $ScriptOutput 'Application\Research\Catalog'
$GeneratedCatalogPath = Join-Path $GeneratedCatalogDirectory 'ResearchCatalog.generated.cs'
$FundamentalUnlockerSubtype = 'WkKnUnlocker_fundamentals'
$UnlockerComponentSubtype = 'WkKnDataFragmentComponent'
$UnlockerComponentCount = 100
$UnlockerModel = 'Models\Items\Datapad_Item.mwm'

function Escape-Xml {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Value)
    return [System.Security.SecurityElement]::Escape($Value)
}

function Escape-CSharp {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Value)
    return $Value.Replace('\', '\\').Replace('"', '\"')
}

function Format-CatalogDataField {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string] $Value)

    if ($Value.Contains('|') -or $Value.Contains("`r") -or $Value.Contains("`n")) {
        throw "Generated catalog field contains an unsupported delimiter: $Value"
    }

    return $Value.Replace('"', '""')
}

function Escape-Json {
    param([AllowEmptyString()][string] $Value)

    if ($null -eq $Value) {
        return ''
    }

    return $Value.Replace('\', '\\').Replace('"', '\"')
}

function Convert-ToSafeToken {
    param([Parameter(Mandatory = $true)][string] $Value)

    $token = $Value -replace '[^A-Za-z0-9]+', '_'
    $token = $token.Trim('_')
    if ([string]::IsNullOrWhiteSpace($token)) {
        return 'unknown'
    }

    return $token
}

function Convert-ToTitle {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    $words = $ResearchId.Split([char[]]@('.', '_'), [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object {
        if ($_.Length -le 1) {
            $_.ToUpperInvariant()
        }
        else {
            $_.Substring(0, 1).ToUpperInvariant() + $_.Substring(1)
        }
    }

    return ($words -join ' ')
}

function Get-SchematicConsumableSubtype {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    return 'WkKnSchematic_' + (Convert-ToSafeToken $ResearchId)
}

function Get-SchematicConsumableDisplayName {
    param([Parameter(Mandatory = $true)][string] $Title)

    $name = $Title
    if ($name.EndsWith(' Schematics', [System.StringComparison]::OrdinalIgnoreCase)) {
        $name = $name.Substring(0, $name.Length - ' Schematics'.Length)
    }

    return "$name Data Schematic"
}

function Get-SchematicSortOrder {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    switch ($ResearchId) {
        'fundamentals' { return 10 }
        'armor.light' { return 20 }
        'armor.heavy' { return 30 }
        'production.basic' { return 40 }
        'tools.drill' { return 50 }
        'tools.welder' { return 60 }
        'tools.grinder' { return 70 }
        'logistics.cargo_storage' { return 80 }
        'logistics.conveyor_network' { return 90 }
        'logistics.cargo_transfer' { return 100 }
        'gas.processing' { return 110 }
        'gas.storage' { return 120 }
        'power.renewable' { return 130 }
        'power.battery' { return 140 }
        'power.hydrogen_engine' { return 150 }
        'power.reactor' { return 160 }
        'production.food' { return 170 }
        'production.advanced' { return 180 }
        'control.interfaces' { return 190 }
        'control.stations' { return 200 }
        'communications' { return 210 }
        'automation.logic' { return 220 }
        'automation.ai_control' { return 230 }
        'utility.display_systems' { return 240 }
        'utility.interior_lighting' { return 250 }
        'utility.directed_lighting' { return 260 }
        'mechanical.systems' { return 270 }
        'mechanical.wheel_systems' { return 280 }
        'utility.gravity' { return 290 }
        'utility.jump_drive' { return 300 }
        'propulsion.atmospheric_thruster' { return 310 }
        'propulsion.hydrogen_thruster' { return 320 }
        'propulsion.ion_thruster' { return 330 }
        'weapons.fixed_weapon' { return 340 }
        'weapons.turret' { return 350 }
        'life_support' { return 360 }
        'economy.station_services' { return 370 }
        'structure.interior' { return 380 }
        'structure.passage' { return 390 }
        'structure.door' { return 400 }
        'structure.hangar_gate' { return 410 }
        'structure.window' { return 420 }
        'structure.industrial' { return 430 }
        'structure.bridge' { return 440 }
        'decor.habitat_fixtures' { return 450 }
        'decor.decorative_fixtures' { return 460 }
        'decor.signage' { return 470 }
        'scenario.mission_systems' { return 480 }
        'prototech.assembler' { return 900 }
        'prototech.drill' { return 910 }
        'prototech.battery' { return 920 }
        'prototech.gyroscope' { return 930 }
        'prototech.reactor' { return 940 }
        'prototech.refinery' { return 950 }
        'prototech.jump_drive' { return 960 }
        'prototech.thruster' { return 970 }
    }

    if ($ResearchId -like 'prototech.*') {
        return 990
    }

    return 800
}

function Get-SchematicTier {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    switch ($ResearchId) {
        'armor.light' { return 'Common' }
        'production.basic' { return 'Common' }
        'logistics.cargo_storage' { return 'Common' }
        'logistics.conveyor_network' { return 'Common' }
        'logistics.cargo_transfer' { return 'Common' }
        'gas.processing' { return 'Common' }
        'gas.storage' { return 'Common' }
        'power.renewable' { return 'Common' }
        'power.battery' { return 'Common' }
        'control.stations' { return 'Common' }
        'communications' { return 'Common' }
        'utility.interior_lighting' { return 'Common' }
        'utility.directed_lighting' { return 'Common' }
        'structure.interior' { return 'Common' }
        'structure.door' { return 'Common' }
        'structure.industrial' { return 'Common' }
        'structure.window' { return 'Common' }
        'decor.signage' { return 'Common' }
        'production.food' { return 'Common' }
        'mechanical.systems' { return 'Common' }
        'mechanical.wheel_systems' { return 'Common' }
        'decor.habitat_fixtures' { return 'Uncommon' }
        'structure.passage' { return 'Uncommon' }
        'control.interfaces' { return 'Uncommon' }
        'armor.heavy' { return 'Uncommon' }
        'decor.decorative_fixtures' { return 'Uncommon' }
        'structure.bridge' { return 'Uncommon' }
        'tools.drill' { return 'Uncommon' }
        'tools.welder' { return 'Uncommon' }
        'tools.grinder' { return 'Uncommon' }
        'power.hydrogen_engine' { return 'Uncommon' }
        'automation.logic' { return 'Uncommon' }
        'utility.display_systems' { return 'Uncommon' }
        'propulsion.atmospheric_thruster' { return 'Uncommon' }
        'propulsion.hydrogen_thruster' { return 'Uncommon' }
        'weapons.fixed_weapon' { return 'Uncommon' }
        'life_support' { return 'Uncommon' }
        'structure.hangar_gate' { return 'Uncommon' }
        'economy.station_services' { return 'Rare' }
        'production.advanced' { return 'Rare' }
        'power.reactor' { return 'Rare' }
        'automation.ai_control' { return 'Rare' }
        'utility.gravity' { return 'Rare' }
        'utility.jump_drive' { return 'Rare' }
        'propulsion.ion_thruster' { return 'Rare' }
        'weapons.turret' { return 'Rare' }
        'prototech.assembler' { return 'Prototech' }
        'prototech.drill' { return 'Prototech' }
        'prototech.battery' { return 'Prototech' }
        'prototech.gyroscope' { return 'Prototech' }
        'prototech.reactor' { return 'Prototech' }
        'prototech.refinery' { return 'Prototech' }
        'prototech.jump_drive' { return 'Prototech' }
        'prototech.thruster' { return 'Prototech' }
        default { return 'None' }
    }
}

function Get-SchematicTitle {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    switch ($ResearchId) {
        'fundamentals' { return 'Fundamental Schematics' }
        'armor.heavy' { return 'Heavy Armor Schematics' }
        'armor.light' { return 'Light Armor Schematics' }
        'communications' { return 'Communications Schematics' }
        'automation.ai_control' { return 'AI Control Schematics' }
        'automation.logic' { return 'Automation Logic Schematics' }
        'control.stations' { return 'Control Station Schematics' }
        'control.interfaces' { return 'Control Interface Schematics' }
        'decor.habitat_fixtures' { return 'Habitat Fixture Schematics' }
        'decor.decorative_fixtures' { return 'Decorative Fixture Schematics' }
        'decor.signage' { return 'Signage Schematics' }
        'economy.station_services' { return 'Station Services Schematics' }
        'gas.processing' { return 'Gas Processing Schematics' }
        'gas.storage' { return 'Gas Storage Schematics' }
        'life_support' { return 'Life Support Schematics' }
        'logistics.cargo_transfer' { return 'Cargo Transfer Schematics' }
        'logistics.conveyor_network' { return 'Conveyor Network Schematics' }
        'logistics.cargo_storage' { return 'Cargo Storage Schematics' }
        'mechanical.systems' { return 'Mechanical Systems Schematics' }
        'mechanical.wheel_systems' { return 'Wheel Systems Schematics' }
        'power.battery' { return 'Battery Schematics' }
        'power.hydrogen_engine' { return 'Hydrogen Engine Schematics' }
        'power.reactor' { return 'Reactor Schematics' }
        'power.renewable' { return 'Renewable Power Schematics' }
        'production.advanced' { return 'Advanced Production Schematics' }
        'production.basic' { return 'Basic Production Schematics' }
        'production.food' { return 'Food Production Schematics' }
        'propulsion.atmospheric_thruster' { return 'Atmospheric Thruster Schematics' }
        'propulsion.hydrogen_thruster' { return 'Hydrogen Thruster Schematics' }
        'propulsion.ion_thruster' { return 'Ion Thruster Schematics' }
        'prototech.battery' { return 'Prototech Battery Schematics' }
        'prototech.gyroscope' { return 'Prototech Gyroscope Schematics' }
        'prototech.jump_drive' { return 'Prototech Jump Drive Schematics' }
        'prototech.refinery' { return 'Prototech Refinery Schematics' }
        'prototech.thruster' { return 'Prototech Thruster Schematics' }
        'prototech.assembler' { return 'Prototech Assembler Schematics' }
        'prototech.drill' { return 'Prototech Drill Schematics' }
        'prototech.reactor' { return 'Prototech Reactor Schematics' }
        'structure.door' { return 'Door Schematics' }
        'structure.hangar_gate' { return 'Hangar Gate Schematics' }
        'structure.industrial' { return 'Industrial Structure Schematics' }
        'structure.interior' { return 'Interior Structure Schematics' }
        'structure.bridge' { return 'Bridge Structure Schematics' }
        'structure.passage' { return 'Passage Schematics' }
        'structure.window' { return 'Window Schematics' }
        'scenario.mission_systems' { return 'Mission System Schematics' }
        'tools.drill' { return 'Drill Schematics' }
        'tools.grinder' { return 'Grinder Schematics' }
        'tools.welder' { return 'Welder Schematics' }
        'utility.display_systems' { return 'Display Systems Schematics' }
        'utility.gravity' { return 'Gravity Schematics' }
        'utility.jump_drive' { return 'Jump Drive Schematics' }
        'utility.interior_lighting' { return 'Interior Lighting Schematics' }
        'utility.directed_lighting' { return 'Directed Lighting Schematics' }
        'weapons.fixed_weapon' { return 'Fixed Weapon Schematics' }
        'weapons.turret' { return 'Turret Schematics' }
        default { return (Convert-ToTitle $ResearchId) + ' Schematics' }
    }
}

function Get-SchematicDescription {
    param([Parameter(Mandatory = $true)][string] $ResearchId)

    switch ($ResearchId) {
        'fundamentals' { return 'Fundamental schematics for basic hull shapes and merge systems issued to new engineers.' }
        'armor.heavy' { return 'Schematics for durable heavy armor hull forms used when protection matters more than mass.' }
        'armor.light' { return 'Schematics for lightweight armor hull forms used to shape ships and stations.' }
        'communications' { return 'Schematics for antennas, beacons, cameras, broadcast controllers, remote control systems, and grid-locating instruments.' }
        'automation.ai_control' { return 'Schematics for autonomous flight, combat, pathing, and task-control systems.' }
        'automation.logic' { return 'Schematics for timers, sensors, programmable systems, event controllers, action relays, turret controllers, and automation controls.' }
        'control.stations' { return 'Schematics for cockpits, helms, control stations, and crew seats used to pilot ships and rovers.' }
        'control.interfaces' { return 'Schematics for button panels, consoles, projectors, and terminal fixtures used to operate grid systems.' }
        'decor.habitat_fixtures' { return 'Schematics for crew quarters, kitchens, bathrooms, seating, storage furniture, armories, and everyday habitat fixtures.' }
        'decor.decorative_fixtures' { return 'Schematics for decorative fixtures, conduits, pipes, access panels, audio equipment, statues, plushies, and training props.' }
        'decor.signage' { return 'Schematics for warning signs, symbols, banners, and display markings.' }
        'economy.station_services' { return 'Schematics for contracts, stores, vending, safe zones, ATMs, and station service terminals.' }
        'gas.processing' { return 'Schematics for vents, oxygen farms, and oxygen/hydrogen generators that manage breathable air and fuel production.' }
        'gas.storage' { return 'Schematics for oxygen and hydrogen storage tanks.' }
        'life_support' { return 'Schematics for medical rooms, refill stations, cryo chambers, first aid cabinets, and crew sustainment systems.' }
        'logistics.cargo_transfer' { return 'Schematics for connectors, collectors, ejectors, and transfer ports used to dock and move cargo.' }
        'logistics.conveyor_network' { return 'Schematics for conveyor tubes, junctions, sorters, frames, and pipe networks.' }
        'logistics.cargo_storage' { return 'Schematics for cargo containers, shelves, crates, freight containers, barrels, and storage fixtures.' }
        'mechanical.systems' { return 'Schematics for pistons, rotors, hinges, gyroscopes, landing gear, and magnetic plates.' }
        'mechanical.wheel_systems' { return 'Schematics for wheel and suspension assemblies used by rovers and ground vehicles.' }
        'power.battery' { return 'Schematics for rechargeable power storage systems.' }
        'power.hydrogen_engine' { return 'Schematics for hydrogen engines that convert stored hydrogen into electrical power.' }
        'power.reactor' { return 'Schematics for compact reactor systems used for high-output power generation.' }
        'power.renewable' { return 'Schematics for solar panels and wind turbines used for renewable power generation.' }
        'production.advanced' { return 'Schematics for assemblers, refineries, and upgrade modules used in advanced production chains.' }
        'production.basic' { return 'Schematics for basic refining, assembling, and survival production equipment.' }
        'production.food' { return 'Schematics for food processors, farms, and irrigation equipment used to produce consumables.' }
        'propulsion.atmospheric_thruster' { return 'Schematics for atmospheric thrusters designed for flight inside planetary atmospheres.' }
        'propulsion.hydrogen_thruster' { return 'Schematics for hydrogen thrusters designed for powerful thrust in atmosphere or vacuum using stored hydrogen.' }
        'propulsion.ion_thruster' { return 'Schematics for ion thrusters designed for efficient propulsion in vacuum.' }
        'prototech.battery' { return 'Schematics for rare prototech battery systems.' }
        'prototech.gyroscope' { return 'Schematics for rare prototech gyroscope systems.' }
        'prototech.jump_drive' { return 'Schematics for rare prototech jump drive systems.' }
        'prototech.refinery' { return 'Schematics for rare prototech refinery systems.' }
        'prototech.thruster' { return 'Schematics for rare prototech thruster systems.' }
        'prototech.assembler' { return 'Schematics for rare prototech assembly systems.' }
        'prototech.drill' { return 'Schematics for rare prototech mining systems.' }
        'prototech.reactor' { return 'Schematics for rare prototech reactor systems.' }
        'structure.door' { return 'Schematics for doors, hatches, gates, and access ways used to seal or control movement through interiors.' }
        'structure.hangar_gate' { return 'Schematics for hangar doors, blast door segments, and large sealed gates used to protect bays and vehicle access.' }
        'structure.industrial' { return 'Schematics for catwalks, railings, ladders, beams, truss frames, scaffolds, platforms, and industrial support structures.' }
        'structure.interior' { return 'Schematics for interior walls, floors, stairs, ramps, columns, trim, embrasures, and room shell pieces.' }
        'structure.bridge' { return 'Schematics for modular bridge floors, corners, slopes, and raised transition pieces.' }
        'structure.passage' { return 'Schematics for corridors, passages, ducts, and walk-through structural modules.' }
        'structure.window' { return 'Schematics for windows and transparent structural panels used to enclose rooms while preserving visibility.' }
        'scenario.mission_systems' { return 'Schematics for mission interface systems used by scripted scenarios and guided objectives.' }
        'tools.drill' { return 'Schematics for ship drills used to mine stone, ore, and voxel material.' }
        'tools.grinder' { return 'Schematics for ship grinders used to salvage structures and recover usable parts.' }
        'tools.welder' { return 'Schematics for ship welders used to construct and repair placed frames and projections.' }
        'utility.display_systems' { return 'Schematics for LCDs, text panels, billboards, holographic displays, and visual information surfaces.' }
        'utility.gravity' { return 'Schematics for gravity generators, artificial mass systems, and gravity-responsive test masses.' }
        'utility.jump_drive' { return 'Schematics for jump drives used for long-distance grid travel.' }
        'utility.interior_lighting' { return 'Schematics for interior lights, neon fixtures, troffers, and panel lighting.' }
        'utility.directed_lighting' { return 'Schematics for spotlights, floodlights, searchlights, and rotating warning lights.' }
        'weapons.fixed_weapon' { return 'Schematics for fixed guns, launchers, warheads, and explosive ordnance.' }
        'weapons.turret' { return 'Schematics for automated and manually controlled turret weapons.' }
        default { return 'Recovered schematics for this equipment family.' }
    }
}

function Test-Any {
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Value,
        [Parameter(Mandatory = $true)][string[]] $Patterns
    )

    foreach ($pattern in $Patterns) {
        if ($Value -match $pattern) {
            return $true
        }
    }

    return $false
}

function Get-DefinitionId {
    param([Parameter(Mandatory = $true)] $IdNode)

    $type = $null
    $subtype = $null

    if ($IdNode.PSObject.Properties['Type'] -and $IdNode.PSObject.Properties['Subtype']) {
        $type = [string] $IdNode.Type
        $subtype = [string] $IdNode.Subtype
    }
    else {
        if ($IdNode.PSObject.Properties['TypeId']) {
            $type = [string] $IdNode.TypeId
        }

        if ($IdNode.PSObject.Properties['SubtypeId']) {
            $subtype = [string] $IdNode.SubtypeId
        }
    }

    if ([string]::IsNullOrWhiteSpace($type)) {
        return $null
    }

    if ($null -eq $subtype) {
        $subtype = ''
    }

    $type = $type.Replace('MyObjectBuilder_', '').Trim()
    $subtype = $subtype.Trim()

    return [pscustomobject]@{
        Type = $type
        Subtype = $subtype
        Key = "$type/$subtype"
    }
}

function Get-ResearchId {
    param([Parameter(Mandatory = $true)] $Block)

    $type = ([string] $Block.Type).ToLowerInvariant()
    $subtype = ([string] $Block.Subtype).ToLowerInvariant()
    $text = "$type $subtype"

    $blastDoorSubtypes = @(
        'armorcenter',
        'armorcorner',
        'armorinvcorner',
        'armorside',
        'smallarmorcenter',
        'smallarmorcorner',
        'smallarmorinvcorner',
        'smallarmorside'
    )
    if ($type -eq 'cubeblock' -and $blastDoorSubtypes -contains $subtype) { return 'structure.hangar_gate' }

    $fundamentalArmorCubeSubtypes = @(
        'largeblockarmorblock',
        'largeblockarmorslope',
        'largeblockarmorcorner',
        'largeblockarmorcornerinv',
        'largeblockarmorcornersquare',
        'largeblockarmorcornersquareinverted',
        'smallblockarmorblock',
        'smallblockarmorslope',
        'smallblockarmorcorner',
        'smallblockarmorcornerinv',
        'smallblockarmorcornersquare',
        'smallblockarmorcornersquareinverted'
    )
    if ($type -eq 'cubeblock' -and $fundamentalArmorCubeSubtypes -contains $subtype) { return 'fundamentals' }
    if ($type -eq 'mergeblock' -and @('largeshipmergeblock', 'smallshipmergeblock', 'smallshipsmallmergeblock') -contains $subtype) { return 'fundamentals' }
    if ($type -eq 'landinggear' -and @('largeblocksmallmagneticplate', 'smallblocksmallmagneticplate') -contains $subtype) { return 'fundamentals' }

    if ($subtype -match 'prototech') {
        switch -Regex ($subtype) {
            'largeblockprototechbattery|smallblockprototechbattery' { return 'prototech.battery' }
            'largeprototechreactor' { return 'prototech.reactor' }
            'largeprototechassembler' { return 'prototech.assembler' }
            'largeprototechrefinery|smallprototechrefinery' { return 'prototech.refinery' }
            'largeblockprototechthruster|smallblockprototechthruster' { return 'prototech.thruster' }
            'largeprototechjumpdrive|smallprototechjumpdrive' { return 'prototech.jump_drive' }
            'largeblockprototechgyro|smallblockprototechgyro' { return 'prototech.gyroscope' }
            'largeblockprototechdrill' { return 'prototech.drill' }
            default { return 'prototech.' + (Convert-ToSafeToken $subtype).ToLowerInvariant() }
        }
    }

    if ($type -eq 'basicmissionblock') { return 'automation.ai_control' }
    if (Test-Any $subtype @('transponder')) { return 'automation.logic' }
    if (Test-Any $subtype @('consolemodule')) { return 'control.interfaces' }
    if (Test-Any $subtype @('modularbridge')) { return 'structure.bridge' }
    if (Test-Any $subtype @('airduct')) { return 'structure.passage' }
    if (Test-Any $subtype @('firecover', 'coverwall')) { return 'structure.industrial' }
    if (Test-Any $subtype @('conduit', '^largeblockpipes', 'pipeworkblock')) { return 'decor.decorative_fixtures' }
    if ($type -ne 'door' -and (Test-Any $subtype @('corridor'))) { return 'structure.passage' }
    if (Test-Any $subtype @('labcabinet', 'labcornerdesk', 'labdesk', 'labsink', 'labequipment', 'microscope', 'vivarium', 'largefreezer')) { return 'decor.habitat_fixtures' }
    if (Test-Any $subtype @('firstaidcabinet')) { return 'life_support' }
    if (Test-Any $subtype @('planter', 'aquarium', 'terrarium')) { return 'decor.habitat_fixtures' }
    if (Test-Any $subtype @('desk')) { return 'decor.habitat_fixtures' }
    if (Test-Any $subtype @('floorplansign')) { return 'decor.signage' }
    if (Test-Any $subtype @('floorpassage')) { return 'structure.interior' }
    if ($subtype -match '^(largeblock|smallblock)floor') { return 'structure.interior' }
    if (Test-Any $subtype @('cylindricalcolumn')) { return 'structure.interior' }
    if (Test-Any $subtype @('trusspillar', 'largeinteriorpillar')) { return 'structure.industrial' }
    if (Test-Any $subtype @('passagescifi')) { return 'structure.passage' }
    if (Test-Any $subtype @('interiorwall', 'scifiwall', 'angledinteriorwall', 'largeblockinsetwall')) { return 'structure.interior' }
    if (Test-Any $subtype @('kitchen', 'barcounter')) { return 'decor.habitat_fixtures' }
    if (Test-Any $subtype @('lockerroom', 'lockers', 'weaponrack', 'insetbookshelf')) { return 'decor.habitat_fixtures' }
    if ($subtype -eq 'trussladder') { return 'structure.industrial' }
    if (Test-Any $subtype @('largeblockstructural')) { return 'structure.industrial' }
    if (Test-Any $subtype @('windowwall', '^viewport[0-9]+$', 'largeblocknarrowviewport')) { return 'structure.window' }

    if ($subtype -match 'heavy.*armor|heavyarmor|armor.*heavy|panelheavy') { return 'armor.heavy' }
    if ($text -match 'armor|armorpanel') { return 'armor.light' }

    if ($type -eq 'thrust' -and $subtype -match 'atmospheric') { return 'propulsion.atmospheric_thruster' }
    if ($type -eq 'thrust' -and $subtype -match 'hydrogen') { return 'propulsion.hydrogen_thruster' }
    if ($type -eq 'thrust') { return 'propulsion.ion_thruster' }
    if (Test-Any $text @('gyroscope', '\bgyro\b')) { return 'mechanical.systems' }

    if ($type -eq 'airvent') { return 'gas.processing' }
    if ($type -eq 'oxygengenerator') {
        if (Test-Any $subtype @('irrigation')) { return 'production.food' }
        return 'gas.processing'
    }
    if ($type -eq 'oxygentank') { return 'gas.storage' }
    if ($type -eq 'oxygenfarm') { return 'gas.processing' }

    if ($type -eq 'buttonpanel') {
        if (Test-Any $subtype @('accesspanel')) { return 'decor.decorative_fixtures' }
        return 'control.interfaces'
    }
    if ($type -eq 'terminalblock') {
        if (Test-Any $subtype @('freezer')) { return 'production.food' }
        if (Test-Any $subtype @('firstaid')) { return 'life_support' }
        if (Test-Any $subtype @('crate')) { return 'logistics.cargo_storage' }
        if (Test-Any $subtype @('accesspanel')) { return 'decor.decorative_fixtures' }
        return 'control.interfaces'
    }
    if ($type -eq 'remotecontrol') { return 'communications' }
    if ($type -eq 'cockpit') {
        if (Test-Any $subtype @('bathroom', 'toilet')) { return 'decor.habitat_fixtures' }
        if (Test-Any $subtype @('lab')) { return 'decor.habitat_fixtures' }
        if (Test-Any $subtype @('couch', 'desk', 'plant', 'consolemodule')) { return 'decor.habitat_fixtures' }
        if (Test-Any $subtype @('passenger', 'bench')) { return 'decor.habitat_fixtures' }
        return 'control.stations'
    }
    if ($type -eq 'cryochamber') {
        if (Test-Any $subtype @('bed', 'bunk')) { return 'decor.habitat_fixtures' }
        return 'life_support'
    }

    if (Test-Any $text @('flightmovementblock', 'defensivecombatblock', 'offensivecombatblock', 'aiblock', 'pathrecorderblock')) { return 'automation.ai_control' }
    if (Test-Any $text @('programmableblock', 'timerblock', 'sensorblock', 'eventcontroller')) { return 'automation.logic' }

    if (Test-Any $text @('basicassembler', 'basicrefinery', 'blast furnace', 'survival')) { return 'production.basic' }
    if (Test-Any $text @('foodprocessor', 'algaefarm', 'farmplot')) { return 'production.food' }
    if (Test-Any $text @('assembler', 'refinery', 'upgrade')) { return 'production.advanced' }

    if ($type -eq 'batteryblock') { return 'power.battery' }
    if (Test-Any $text @('reactor')) { return 'power.reactor' }
    if (Test-Any $text @('solar')) { return 'power.renewable' }
    if (Test-Any $text @('windturbine')) { return 'power.renewable' }
    if (Test-Any $text @('hydrogenengine')) { return 'power.hydrogen_engine' }

    if ($type -eq 'collector' -or (Test-Any $text @('ejector'))) { return 'logistics.cargo_transfer' }
    if ($type -eq 'shipconnector') { return 'logistics.cargo_transfer' }
    if ($type -eq 'conveyorconnector' -or $type -eq 'conveyorsorter' -or $type -eq 'conveyor') { return 'logistics.conveyor_network' }
    if ($type -eq 'cargocontainer') {
        if (Test-Any $subtype @('bookshelf', 'cabinet', 'desk', 'lockerroom', 'lockers', 'weaponrack')) { return 'decor.habitat_fixtures' }
        return 'logistics.cargo_storage'
    }
    if (Test-Any $text @('inventory', 'container')) { return 'logistics.cargo_storage' }

    if ($type -eq 'motorsuspension' -or $type -eq 'wheel') {
        return 'mechanical.wheel_systems'
    }
    if (Test-Any $text @('piston')) { return 'mechanical.systems' }
    if (Test-Any $text @('motorstator', 'stator', 'rotor', 'hinge')) { return 'mechanical.systems' }

    if (Test-Any $text @('shipwelder', 'welder')) { return 'tools.welder' }
    if (Test-Any $text @('shipgrinder', 'grinder')) { return 'tools.grinder' }
    if (Test-Any $text @('drill')) { return 'tools.drill' }

    if (Test-Any $text @('targetdummy')) { return 'decor.decorative_fixtures' }
    if (Test-Any $text @('turretcontrolblock')) { return 'automation.logic' }
    if (Test-Any $text @('warhead')) { return 'weapons.fixed_weapon' }
    if (Test-Any $text @('decoy')) { return 'communications' }
    if (Test-Any $text @('interiorturret', 'gatlingturret', 'missileturret', 'calibreturret', '\bturret\b')) { return 'weapons.turret' }
    if (Test-Any $text @('missilelauncher', 'gatlinggun', 'autocannon', 'artillery', 'assaultcannon', 'railgun', 'launcher')) { return 'weapons.fixed_weapon' }

    if (Test-Any $text @('antenna', 'beacon', 'broadcastcontroller', 'laserantenna', 'radio', 'remotecontrol', 'transponderblock')) { return 'communications' }
    if (Test-Any $text @('medical', 'survivalkit', 'refillstation')) { return 'life_support' }
    if (Test-Any $text @('contract', 'storeblock', 'safezone', 'vending', 'atm', 'servicesterminal')) { return 'economy.station_services' }
    if (Test-Any $text @('landinggear', 'magneticplate')) { return 'mechanical.systems' }
    if (Test-Any $text @('heatvent', 'exhaustblock')) { return 'decor.decorative_fixtures' }
    if (Test-Any $text @('soundblock', 'jukebox')) { return 'decor.decorative_fixtures' }
    if (Test-Any $text @('camera')) { return 'communications' }
    if (Test-Any $text @('projector')) { return 'control.interfaces' }
    if (Test-Any $text @('jumpdrive')) { return 'utility.jump_drive' }
    if (Test-Any $text @('gravitygenerator')) { return 'utility.gravity' }
    if (Test-Any $text @('virtualmass', 'spaceball')) { return 'utility.gravity' }
    if (Test-Any $text @('parachute')) { return 'automation.logic' }
    if (Test-Any $text @('oredetector')) { return 'communications' }
    if (Test-Any $text @('labequipment', 'microscope')) { return 'decor.habitat_fixtures' }
    if (Test-Any $text @('billboard')) { return 'utility.display_systems' }
    if ($type -eq 'textpanel' -and $subtype -eq 'smallblockconsolemodulescreens') { return 'decor.habitat_fixtures' }
    if (Test-Any $text @('lcd', 'textpanel', 'transparentlcd', 'jumbotron')) { return 'utility.display_systems' }

    if (Test-Any $text @('neon', 'emissive')) { return 'utility.interior_lighting' }
    if ($type -eq 'searchlight') { return 'utility.directed_lighting' }
    if ($type -eq 'reflectorlight') { return 'utility.directed_lighting' }
    if (Test-Any $text @('light')) { return 'utility.interior_lighting' }
    if (Test-Any $text @('narrowviewport', '\bviewport')) { return 'structure.window' }
    if (Test-Any $text @('barredwindow')) { return 'structure.window' }
    if (Test-Any $text @('extendedwindow', 'bridgewindow', 'windowwall')) { return 'structure.window' }
    if ($subtype -eq 'halfwindowround') { return 'structure.window' }
    if (Test-Any $text @('roundwindow', 'windowround')) { return 'structure.window' }
    if (Test-Any $text @('smallwindow', 'window', 'viewglass', 'transparent')) { return 'structure.window' }
    if (Test-Any $text @('hangardoor')) { return 'structure.hangar_gate' }
    if (Test-Any $text @('slidedoor', '\bdoor\b', 'gate')) { return 'structure.door' }

    if (Test-Any $text @('storagebin')) { return 'logistics.cargo_storage' }
    if (Test-Any $text @('conveyorcap', 'conveyorpipecap')) { return 'logistics.conveyor_network' }
    if (Test-Any $text @('floorplansign')) { return 'decor.signage' }
    if (Test-Any $text @('warningsign')) { return 'decor.signage' }
    if (Test-Any $text @('largesymbol', 'smallsymbol')) { return 'decor.signage' }
    if (Test-Any $text @('sign', 'banner')) { return 'decor.signage' }

    if (Test-Any $text @('ladder')) { return 'structure.industrial' }
    if (Test-Any $subtype @('largeramp', 'largestairs')) { return 'structure.industrial' }
    if (Test-Any $text @('catwalk', 'railing', '\brail\b', 'grated.*stairs')) { return 'structure.industrial' }
    if (Test-Any $text @('truss')) { return 'structure.industrial' }
    if (Test-Any $text @('beam', 'pillar', 'column', 'scaffold', 'supportbeam', 'structural')) { return 'structure.industrial' }
    if (Test-Any $text @('modularbridge')) { return 'structure.bridge' }
    if (Test-Any $text @('corridor')) { return 'structure.passage' }
    if (Test-Any $text @('passage')) { return 'structure.passage' }
    if (Test-Any $text @('airduct')) { return 'structure.passage' }
    if (Test-Any $text @('pipework', 'conduit', 'largeblockpipes')) { return 'decor.decorative_fixtures' }
    if (Test-Any $text @('floor', 'slab')) { return 'structure.interior' }
    if (Test-Any $text @('roundedge', 'halfedge')) { return 'armor.light' }
    if (Test-Any $text @('coverwall', 'firecover')) { return 'structure.industrial' }
    if (Test-Any $text @('interior', 'wall', 'embrasure')) { return 'structure.interior' }

    if (Test-Any $text @('emotioncontrollerblock')) { return 'utility.display_systems' }
    if (Test-Any $text @('freight', 'barrel', 'storageshelf')) { return 'logistics.cargo_storage' }
    if (Test-Any $text @('decorative', 'emote', 'figurine', 'plushie', 'statue', 'tree', 'plant', 'dead')) { return 'decor.decorative_fixtures' }
    if (Test-Any $text @('lab', 'vivarium')) { return 'decor.habitat_fixtures' }
    if (Test-Any $text @('toilet', 'shower', 'kitchen')) { return 'decor.habitat_fixtures' }
    if (Test-Any $text @('desk', 'bed', 'planter', 'couch', 'chair', 'console', 'shelf', 'locker', 'store', 'bar', 'stowage')) { return 'decor.habitat_fixtures' }
    if (Test-Any $text @('basicmissionblock')) { return 'scenario.mission_systems' }

    return 'misc.' + (Convert-ToSafeToken $type).ToLowerInvariant()
}

function Get-CubeBlockIds {
    param([Parameter(Mandatory = $true)][string] $DataRoot)

    $ids = [ordered]@{}
    $files = Get-ChildItem -LiteralPath $DataRoot -Recurse -File | Where-Object { $_.Extension -ieq '.sbc' }

    foreach ($file in $files) {
        try {
            [xml] $xml = Get-Content -LiteralPath $file.FullName -Raw
        }
        catch {
            Write-Warning "Skipping unreadable SBC: $($file.FullName)"
            continue
        }

        if (-not $xml.Definitions -or -not $xml.Definitions.PSObject.Properties['CubeBlocks']) {
            continue
        }

        if (-not $xml.Definitions.CubeBlocks.PSObject.Properties['Definition']) {
            continue
        }

        foreach ($definition in @($xml.Definitions.CubeBlocks.Definition)) {
            if (-not $definition -or -not $definition.Id) {
                continue
            }

            if ($definition.PSObject.Properties['Public'] -and [string] $definition.Public -ieq 'false') {
                continue
            }

            $id = Get-DefinitionId -IdNode $definition.Id
            if ($null -eq $id) {
                continue
            }

            if (-not $ids.Contains($id.Key)) {
                $ids.Add($id.Key, $id)
            }
        }
    }

    return @($ids.Values | Sort-Object Type,Subtype)
}

function New-ResearchCatalog {
    param([Parameter(Mandatory = $true)] [object[]] $Blocks)

    $entries = [System.Collections.Generic.List[object]]::new()
    $groups = [ordered]@{}

    foreach ($block in $Blocks) {
        $researchId = Get-ResearchId -Block $block
        $token = Convert-ToSafeToken $researchId
        $groupSubtype = "WkKn_$token"
        $unlockerSubtype = "WkKnUnlocker_$token"
        if ($researchId -eq 'fundamentals') {
            $groupSubtype = '0'
            $unlockerSubtype = $FundamentalUnlockerSubtype
        }

        if (-not $groups.Contains($researchId)) {
            $groups.Add($researchId, [pscustomobject]@{
                ResearchId = $researchId
                Title = Get-SchematicTitle $researchId
                Description = Get-SchematicDescription $researchId
                GroupSubtype = $groupSubtype
                UnlockerSubtype = $unlockerSubtype
                Tier = Get-SchematicTier $researchId
            })
        }

        $entries.Add([pscustomobject]@{
            Block = $block
            ResearchId = $researchId
            Title = Get-SchematicTitle $researchId
            GroupSubtype = $groupSubtype
            UnlockerSubtype = $unlockerSubtype
            Tier = Get-SchematicTier $researchId
        })
    }

    return [pscustomobject]@{
        Entries = @($entries | Sort-Object { $_.Block.Type }, { $_.Block.Subtype })
        Groups = @($groups.Values | Sort-Object { Get-SchematicSortOrder $_.ResearchId }, ResearchId)
    }
}

function Write-ResearchBlocks {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Entries,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <ResearchBlocks>')

    foreach ($entry in $Entries) {
        $block = $entry.Block
        $lines.Add('    <ResearchBlock xsi:type="ResearchBlock">')
        $lines.Add("      <Id Type=""$(Escape-Xml $block.Type)"" Subtype=""$(Escape-Xml $block.Subtype)"" />")
        $lines.Add('      <UnlockedByGroups />')
        $lines.Add('    </ResearchBlock>')
    }

    $lines.Add('  </ResearchBlocks>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Write-VanillaResearchGroups {
    param(
        [Parameter(Mandatory = $true)] [int] $Count,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <ResearchGroups>')

    for ($i = 0; $i -lt $Count; $i++) {
        $lines.Add('    <ResearchGroup xsi:type="ResearchGroup">')
        $lines.Add("      <Id Type=""MyObjectBuilder_ResearchGroupDefinition"" Subtype=""$i"" />")
        $lines.Add('      <Members>')
        if ($i -eq 0) {
            $lines.Add("        <BlockId Type=""CubeBlock"" Subtype=""$FundamentalUnlockerSubtype"" />")
        }
        $lines.Add('      </Members>')
        $lines.Add('    </ResearchGroup>')
    }

    $lines.Add('  </ResearchGroups>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Write-UnlockerResearchGroups {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Groups,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <ResearchGroups>')

    foreach ($group in $Groups) {
        if ($group.ResearchId -eq 'fundamentals') {
            continue
        }

        $lines.Add('    <ResearchGroup xsi:type="ResearchGroup">')
        $lines.Add("      <Id Type=""MyObjectBuilder_ResearchGroupDefinition"" Subtype=""$(Escape-Xml $group.GroupSubtype)"" />")
        $lines.Add('      <Members />')
        $lines.Add('    </ResearchGroup>')
    }

    $lines.Add('  </ResearchGroups>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Write-UnlockerBlocks {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Groups,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $unlockers = [System.Collections.Generic.List[object]]::new()
    $unlockers.Add([pscustomobject]@{
        UnlockerSubtype = $FundamentalUnlockerSubtype
        Title = Get-SchematicTitle 'fundamentals'
        Description = Get-SchematicDescription 'fundamentals'
        ResearchId = 'fundamentals'
    })

    foreach ($group in $Groups) {
        if ($group.ResearchId -eq 'fundamentals') {
            continue
        }

        $unlockers.Add($group)
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <CubeBlocks>')

    foreach ($unlocker in $unlockers) {
        $subtype = $unlocker.UnlockerSubtype
        $displayName = $unlocker.Title
        $description = $unlocker.Description
        $lines.Add('    <Definition>')
        $lines.Add('      <Id>')
        $lines.Add('        <TypeId>CubeBlock</TypeId>')
        $lines.Add("        <SubtypeId>$(Escape-Xml $subtype)</SubtypeId>")
        $lines.Add('      </Id>')
        $lines.Add("      <DisplayName>$(Escape-Xml $displayName)</DisplayName>")
        $lines.Add('      <Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon>')
        $lines.Add('      <Public>true</Public>')
        $lines.Add('      <GuiVisible>false</GuiVisible>')
        $lines.Add("      <Description>$(Escape-Xml $description)</Description>")
        $lines.Add("      <BlockPairName>$(Escape-Xml $subtype)</BlockPairName>")
        $lines.Add('      <CubeSize>Small</CubeSize>')
        $lines.Add('      <BlockTopology>TriangleMesh</BlockTopology>')
        $lines.Add('      <Size x="1" y="1" z="1" />')
        $lines.Add('      <ModelOffset x="0" y="0" z="0" />')
        $lines.Add("      <Model>$UnlockerModel</Model>")
        $lines.Add('      <Components>')
        $lines.Add("        <Component Subtype=""$UnlockerComponentSubtype"" Count=""$UnlockerComponentCount"" />")
        $lines.Add('      </Components>')
        $lines.Add("      <CriticalComponent Subtype=""$UnlockerComponentSubtype"" Index=""0"" />")
        $lines.Add('      <MountPoints>')
        $lines.Add('        <MountPoint Side="Front" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('        <MountPoint Side="Back" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('        <MountPoint Side="Left" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('        <MountPoint Side="Right" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('        <MountPoint Side="Bottom" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('        <MountPoint Side="Top" StartX="0" StartY="0" EndX="1" EndY="1" />')
        $lines.Add('      </MountPoints>')
        $lines.Add('      <BuildProgressModels>')
        $lines.Add("        <Model BuildPercentUpperBound=""1.00"" File=""$UnlockerModel"" />")
        $lines.Add('      </BuildProgressModels>')
        $lines.Add('      <DeformationRatio>0.32</DeformationRatio>')
        $lines.Add('      <EdgeType>Light</EdgeType>')
        $lines.Add('      <BuildTimeSeconds>3</BuildTimeSeconds>')
        $lines.Add('      <PCU>1</PCU>')
        $lines.Add('      <IsAirTight>false</IsAirTight>')
        $lines.Add('    </Definition>')
    }

    $lines.Add('  </CubeBlocks>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Write-ResearchSchematicConsumables {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Groups,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <PhysicalItems>')

    foreach ($group in $Groups) {
        $subtype = Get-SchematicConsumableSubtype $group.ResearchId
        $displayName = Get-SchematicConsumableDisplayName $group.Title
        $description = "A durable data schematic that can teach $($group.Title). It is returned after use so it can be shared."

        $lines.Add('    <PhysicalItem xsi:type="MyObjectBuilder_ConsumableItemDefinition">')
        $lines.Add('      <Id>')
        $lines.Add('        <TypeId>ConsumableItem</TypeId>')
        $lines.Add("        <SubtypeId>$(Escape-Xml $subtype)</SubtypeId>")
        $lines.Add('      </Id>')
        $lines.Add("      <DisplayName>$(Escape-Xml $displayName)</DisplayName>")
        $lines.Add("      <Description>$(Escape-Xml $description)</Description>")
        $lines.Add('      <Icon>Textures\GUI\Icons\Items\Datapad_Item.dds</Icon>')
        $lines.Add('      <Size>')
        $lines.Add('        <X>0.2</X>')
        $lines.Add('        <Y>0.1</Y>')
        $lines.Add('        <Z>0.2</Z>')
        $lines.Add('      </Size>')
        $lines.Add('      <Mass>0.2</Mass>')
        $lines.Add('      <Volume>0.05</Volume>')
        $lines.Add('      <Model>Models\Items\Datapad_Item.mwm</Model>')
        $lines.Add('      <PhysicalMaterial>Metal</PhysicalMaterial>')
        $lines.Add('      <Stats>')
        $lines.Add('        <Stat Name="RadiationImmunity" Value="1" Time="1" />')
        $lines.Add('      </Stats>')
        $lines.Add('      <UseSound>PlayUsePowerKit</UseSound>')
        $lines.Add('      <DepositAllEnabled>false</DepositAllEnabled>')
        $lines.Add('      <CanPlayerOrder>false</CanPlayerOrder>')
        $lines.Add('      <CanPlayerOffer>false</CanPlayerOffer>')
        $lines.Add('    </PhysicalItem>')
    }

    $lines.Add('  </PhysicalItems>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Write-GeneratedCatalog {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Entries,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('// <auto-generated />')
    $lines.Add('namespace WkKn')
    $lines.Add('{')
    $lines.Add('    internal static partial class ResearchCatalog')
    $lines.Add('    {')
    $lines.Add('        private const string ResearchMetadataData =')
    $lines.Add('@"')

    $metadata = [ordered]@{}
    foreach ($entry in $Entries) {
        if (-not $metadata.Contains($entry.ResearchId)) {
            $metadata.Add($entry.ResearchId, $entry)
        }
    }

    foreach ($entry in $metadata.Values) {
        $fields = @(
            (Format-CatalogDataField $entry.ResearchId),
            (Format-CatalogDataField $entry.Title),
            (Format-CatalogDataField $entry.GroupSubtype),
            (Format-CatalogDataField $entry.UnlockerSubtype),
            (Format-CatalogDataField $entry.Tier)
        )

        $lines.Add($fields -join '|')
    }

    $lines.Add('";')
    $lines.Add('')
    $lines.Add('        private const string EntryData =')
    $lines.Add('@"')

    foreach ($entry in $Entries) {
        $block = $entry.Block
        $blockKey = "$($block.Type)/$($block.Subtype)"
        $fields = @(
            (Format-CatalogDataField $blockKey),
            (Format-CatalogDataField $entry.ResearchId)
        )

        $lines.Add($fields -join '|')
    }

    $lines.Add('";')
    $lines.Add('    }')
    $lines.Add('}')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function Convert-ToDocsBlockId {
    param([Parameter(Mandatory = $true)] $Block)

    if ([string]::IsNullOrWhiteSpace([string] $Block.Subtype)) {
        return [string] $Block.Type
    }

    return "$($Block.Type).$($Block.Subtype)"
}

function Get-ExistingDocsBlockNames {
    param([Parameter(Mandatory = $true)] [string] $Path)

    $names = @{}
    if (-not (Test-Path -LiteralPath $Path)) {
        return $names
    }

    try {
        $groups = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    }
    catch {
        Write-Warning "Could not read existing schematic docs JSON for block names: $Path"
        return $names
    }

    foreach ($group in @($groups)) {
        if ($null -eq $group -or -not $group.PSObject.Properties['blocks']) {
            continue
        }

        foreach ($block in @($group.blocks)) {
            if ($null -eq $block -or -not $block.PSObject.Properties['id'] -or -not $block.PSObject.Properties['name']) {
                continue
            }

            $id = [string] $block.id
            $name = [string] $block.name
            if (-not [string]::IsNullOrWhiteSpace($id) -and -not [string]::IsNullOrWhiteSpace($name) -and -not $names.ContainsKey($id)) {
                $names.Add($id, $name)
            }
        }
    }

    return $names
}

function Write-DocsSchematicGroups {
    param(
        [Parameter(Mandatory = $true)] [object[]] $Entries,
        [Parameter(Mandatory = $true)] [object[]] $Groups,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $existingNames = Get-ExistingDocsBlockNames -Path $Path
    $groupsByResearchId = [ordered]@{}

    foreach ($entry in $Entries) {
        if (-not $groupsByResearchId.Contains($entry.ResearchId)) {
            $groupsByResearchId.Add($entry.ResearchId, [pscustomobject]@{
                Name = $entry.Title
                Blocks = [System.Collections.Generic.List[object]]::new()
            })
        }

        $blockId = Convert-ToDocsBlockId -Block $entry.Block
        $blockName = $entry.Block.Subtype
        if ($existingNames.ContainsKey($blockId)) {
            $blockName = $existingNames[$blockId]
        }
        elseif ([string]::IsNullOrWhiteSpace($blockName)) {
            $blockName = $entry.Block.Type
        }

        $groupsByResearchId[$entry.ResearchId].Blocks.Add([pscustomobject]@{
            Id = $blockId
            Name = $blockName
        })
    }

    $orderedGroups = [System.Collections.Generic.List[object]]::new()
    $addedResearchIds = @{}
    foreach ($group in $Groups) {
        if ($groupsByResearchId.Contains($group.ResearchId)) {
            $orderedGroups.Add($groupsByResearchId[$group.ResearchId])
            $addedResearchIds[$group.ResearchId] = $true
        }
    }

    foreach ($researchId in $groupsByResearchId.Keys) {
        if (-not $addedResearchIds.ContainsKey($researchId)) {
            $orderedGroups.Add($groupsByResearchId[$researchId])
        }
    }

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('[')

    $groupIndex = 0
    foreach ($group in $orderedGroups) {
        $lines.Add('  {')
        $lines.Add("    ""name"": ""$(Escape-Json $group.Name)"",")
        $lines.Add('    "notes": "",')
        $lines.Add('    "blocks": [')

        for ($i = 0; $i -lt $group.Blocks.Count; $i++) {
            $block = $group.Blocks[$i]
            $suffix = if ($i -lt $group.Blocks.Count - 1) { ',' } else { '' }
            $lines.Add("      { ""id"": ""$(Escape-Json $block.Id)"", ""name"": ""$(Escape-Json $block.Name)"", ""notes"": """" }$suffix")
        }

        $lines.Add('    ]')
        $groupSuffix = if ($groupIndex -lt $orderedGroups.Count - 1) { ',' } else { '' }
        $lines.Add("  }$groupSuffix")
        $groupIndex++
    }

    $lines.Add(']')
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($Path, [string[]] $lines, $utf8NoBom)
}

function New-VariantBlock {
    param(
        [Parameter(Mandatory = $true)] [string] $Type,
        [Parameter(Mandatory = $true)] [string] $Subtype
    )

    return [pscustomobject]@{
        Type = $Type
        Subtype = $Subtype
    }
}

function New-VariantGroup {
    param(
        [Parameter(Mandatory = $true)] [string] $Subtype,
        [Parameter(Mandatory = $true)] [string] $Icon,
        [Parameter(Mandatory = $true)] [string] $DisplayName,
        [Parameter(Mandatory = $true)] [string] $Description,
        [Parameter(Mandatory = $true)] [object[]] $Blocks
    )

    return [pscustomobject]@{
        Subtype = $Subtype
        Icon = $Icon
        DisplayName = $DisplayName
        Description = $Description
        Blocks = $Blocks
    }
}

function Get-ResearchVariantGroups {
    return @(
        New-VariantGroup -Subtype 'BasicAssemblerGroup' -Icon 'Textures\GUI\Icons\Cubes\basicAssembler.dds' -DisplayName 'DisplayName_Block_BasicAssembler' -Description 'Description_AssemblerBasic' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Assembler' 'BasicAssembler'
        )
        New-VariantGroup -Subtype 'AssemblerGroup' -Icon 'Textures\GUI\Icons\Cubes\assembler.dds' -DisplayName 'DisplayName_Block_Assembler' -Description 'Description_Assembler' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Assembler' 'LargeAssembler'
            New-VariantBlock 'MyObjectBuilder_Assembler' 'LargeAssemblerIndustrial'
        )
        New-VariantGroup -Subtype 'DecorativeUtilityGroup' -Icon 'Textures\GUI\Icons\Cubes\LabEquipment.dds' -DisplayName 'DisplayName_Block_LabEquipment' -Description 'Description_LabEquipment' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_LCDPanelsBlock' 'LabEquipment'
            New-VariantBlock 'MyObjectBuilder_LCDPanelsBlock' 'LabEquipment1'
            New-VariantBlock 'MyObjectBuilder_InteriorLight' 'LabEquipment2'
            New-VariantBlock 'MyObjectBuilder_LCDPanelsBlock' 'LabEquipment3'
        )
        New-VariantGroup -Subtype 'MedicalGroup' -Icon 'Textures\GUI\Icons\Cubes\medical_room.dds' -DisplayName 'DisplayName_Block_MedicalRoom' -Description 'Description_MedicalRoom' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_MedicalRoom' 'LargeMedicalRoom'
            New-VariantBlock 'MyObjectBuilder_MedicalRoom' 'LargeMedicalRoomReskin'
            New-VariantBlock 'MyObjectBuilder_LCDPanelsBlock' 'MedicalStation'
        )
        New-VariantGroup -Subtype 'ShipToolGroup' -Icon 'Textures\GUI\Icons\Cubes\Grinder.dds' -DisplayName 'DisplayName_Block_ShipGrinder' -Description 'Description_ShipGrinder' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_ShipGrinder' 'LargeShipGrinder'
            New-VariantBlock 'MyObjectBuilder_ShipGrinder' 'SmallShipGrinder'
            New-VariantBlock 'MyObjectBuilder_ShipGrinder' 'LargeShipGrinderReskin'
            New-VariantBlock 'MyObjectBuilder_ShipGrinder' 'SmallShipGrinderReskin'
        )
        New-VariantGroup -Subtype 'ShipWelderGroup' -Icon 'Textures\GUI\Icons\Cubes\Welder.dds' -DisplayName 'DisplayName_Block_ShipWelder' -Description 'Description_ShipWelder' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_ShipWelder' 'LargeShipWelder'
            New-VariantBlock 'MyObjectBuilder_ShipWelder' 'SmallShipWelder'
            New-VariantBlock 'MyObjectBuilder_ShipWelder' 'LargeShipWelderReskin'
            New-VariantBlock 'MyObjectBuilder_ShipWelder' 'SmallShipWelderReskin'
        )
        New-VariantGroup -Subtype 'ShipDrillGroup' -Icon 'Textures\GUI\Icons\Cubes\drill.dds' -DisplayName 'DisplayName_Block_Drill' -Description 'Description_Drill' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Drill' 'SmallBlockDrill'
            New-VariantBlock 'MyObjectBuilder_Drill' 'LargeBlockDrill'
            New-VariantBlock 'MyObjectBuilder_Drill' 'SmallBlockDrillReskin'
            New-VariantBlock 'MyObjectBuilder_Drill' 'LargeBlockDrillReskin'
        )
        New-VariantGroup -Subtype 'PrototechGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_JumpDrive.dds' -DisplayName 'DisplayName_Block_PrototechJumpDrive' -Description 'Description_Prototech_JumpDrive' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_JumpDrive' 'LargePrototechJumpDrive'
            New-VariantBlock 'MyObjectBuilder_JumpDrive' 'SmallPrototechJumpDrive'
        )
        New-VariantGroup -Subtype 'PrototechThrusterGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_Thruster_Large.dds' -DisplayName 'DisplayName_Block_PrototechThruster' -Description 'Description_Prototech_Thrust' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Thrust' 'LargeBlockPrototechThruster'
            New-VariantBlock 'MyObjectBuilder_Thrust' 'SmallBlockPrototechThruster'
        )
        New-VariantGroup -Subtype 'PrototechRefineryGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_Refinery.dds' -DisplayName 'DisplayName_Block_PrototechRefinery' -Description 'Description_Prototech_Refinery' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Refinery' 'LargePrototechRefinery'
            New-VariantBlock 'MyObjectBuilder_Refinery' 'SmallPrototechRefinery'
        )
        New-VariantGroup -Subtype 'PrototechAssemblerGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_Assembler.dds' -DisplayName 'DisplayName_Block_PrototechAssembler' -Description 'Description_PrototechAssembler' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Assembler' 'LargePrototechAssembler'
        )
        New-VariantGroup -Subtype 'PrototechGyroGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_Gyroscope_large.dds' -DisplayName 'DisplayName_Block_PrototechGyroscope' -Description 'Description_Prototech_Gyroscope' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Gyro' 'LargeBlockPrototechGyro'
            New-VariantBlock 'MyObjectBuilder_Gyro' 'SmallBlockPrototechGyro'
        )
        New-VariantGroup -Subtype 'PrototechBatteryGroup' -Icon 'Textures\GUI\Icons\Cubes\PrototechBattery.dds' -DisplayName 'DisplayName_Block_PrototechBattery' -Description 'Description_Prototech_Battery' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_BatteryBlock' 'LargeBlockPrototechBattery'
            New-VariantBlock 'MyObjectBuilder_BatteryBlock' 'SmallBlockPrototechBattery'
        )
        New-VariantGroup -Subtype 'PrototechDrillGroup' -Icon 'Textures\GUI\Icons\Cubes\PrototechDrill.dds' -DisplayName 'DisplayName_Block_PrototechDrill' -Description 'Description_Prototech_Drill' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_Drill' 'LargeBlockPrototechDrill'
        )
        New-VariantGroup -Subtype 'PrototechReactorGroup' -Icon 'Textures\GUI\Icons\Cubes\Prototech_Reactor.dds' -DisplayName 'DisplayName_Block_PrototechReactor' -Description 'Description_PrototechReactor' -Blocks @(
            New-VariantBlock 'MyObjectBuilder_HydrogenEngine' 'LargePrototechReactor'
        )
    )
}

function Write-ResearchBlockVariantGroups {
    param([Parameter(Mandatory = $true)] [string] $Path)

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add('<?xml version="1.0" encoding="utf-8"?>')
    $lines.Add('<Definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">')
    $lines.Add('  <BlockVariantGroups>')

    foreach ($group in (Get-ResearchVariantGroups)) {
        $lines.Add('    <BlockVariantGroup>')
        $lines.Add("      <Id Type=""MyObjectBuilder_BlockVariantGroup"" Subtype=""$(Escape-Xml $group.Subtype)"" />")
        $lines.Add("      <Icon>$(Escape-Xml $group.Icon)</Icon>")
        $lines.Add("      <DisplayName>$(Escape-Xml $group.DisplayName)</DisplayName>")
        $lines.Add("      <Description>$(Escape-Xml $group.Description)</Description>")
        $lines.Add('      <Blocks>')
        foreach ($block in $group.Blocks) {
            $lines.Add("        <Block Type=""$(Escape-Xml $block.Type)"" Subtype=""$(Escape-Xml $block.Subtype)"" />")
        }
        $lines.Add('      </Blocks>')
        $lines.Add('    </BlockVariantGroup>')
    }

    $lines.Add('  </BlockVariantGroups>')
    $lines.Add('</Definitions>')
    Set-Content -LiteralPath $Path -Value $lines -Encoding UTF8
}

function New-RadialCubeBlockItem {
    param(
        [Parameter(Mandatory = $true)] [System.Xml.XmlDocument] $Document,
        [Parameter(Mandatory = $true)] [string] $Subtype
    )

    $item = $Document.CreateElement('Item')
    $typeAttribute = $Document.CreateAttribute('xsi', 'type', 'http://www.w3.org/2001/XMLSchema-instance')
    $typeAttribute.Value = 'MyObjectBuilder_RadialMenuItemCubeBlock'
    [void] $item.Attributes.Append($typeAttribute)

    $id = $Document.CreateElement('Id')
    $idType = $Document.CreateAttribute('Type')
    $idType.Value = 'MyObjectBuilder_BlockVariantGroup'
    [void] $id.Attributes.Append($idType)

    $idSubtype = $Document.CreateAttribute('Subtype')
    $idSubtype.Value = $Subtype
    [void] $id.Attributes.Append($idSubtype)

    [void] $item.AppendChild($id)
    return $item
}

function Find-RadialSectionByLabel {
    param(
        [Parameter(Mandatory = $true)] [System.Xml.XmlElement] $Menu,
        [Parameter(Mandatory = $true)] [string] $Label
    )

    foreach ($section in $Menu.GetElementsByTagName('Section')) {
        if ($section['Label'] -and $section['Label'].InnerText -eq $Label) {
            return [System.Xml.XmlElement] $section
        }
    }

    return $null
}

function New-RadialSection {
    param(
        [Parameter(Mandatory = $true)] [System.Xml.XmlDocument] $Document,
        [Parameter(Mandatory = $true)] [string] $Label,
        [Parameter(Mandatory = $true)] [string[]] $Subtypes
    )

    $section = $Document.CreateElement('Section')
    $labelNode = $Document.CreateElement('Label')
    $labelNode.InnerText = $Label
    [void] $section.AppendChild($labelNode)

    $itemsNode = $Document.CreateElement('Items')
    foreach ($subtype in $Subtypes) {
        [void] $itemsNode.AppendChild((New-RadialCubeBlockItem -Document $Document -Subtype $subtype))
    }
    [void] $section.AppendChild($itemsNode)

    return $section
}

function Set-RadialSectionItems {
    param(
        [Parameter(Mandatory = $true)] [System.Xml.XmlDocument] $Document,
        [Parameter(Mandatory = $true)] [System.Xml.XmlElement] $Section,
        [Parameter(Mandatory = $true)] [string] $Label,
        [Parameter(Mandatory = $true)] [string[]] $Subtypes
    )

    $labelNode = $Section['Label']
    if ($null -eq $labelNode) {
        $labelNode = $Document.CreateElement('Label')
        [void] $Section.PrependChild($labelNode)
    }
    $labelNode.InnerText = $Label

    $itemsNode = $Section['Items']
    if ($null -eq $itemsNode) {
        $itemsNode = $Document.CreateElement('Items')
        [void] $Section.AppendChild($itemsNode)
    }

    while ($itemsNode.HasChildNodes) {
        [void] $itemsNode.RemoveChild($itemsNode.FirstChild)
    }

    foreach ($subtype in $Subtypes) {
        [void] $itemsNode.AppendChild((New-RadialCubeBlockItem -Document $Document -Subtype $subtype))
    }
}

function Split-RadialSections {
    param(
        [Parameter(Mandatory = $true)] [System.Xml.XmlDocument] $Document,
        [Parameter(Mandatory = $true)] [System.Xml.XmlElement] $Menu,
        [int] $MaxItems = 8
    )

    $sectionsNode = $Menu['Sections']
    if ($null -eq $sectionsNode) {
        return
    }

    $sections = [System.Collections.Generic.List[System.Xml.XmlElement]]::new()
    foreach ($child in $sectionsNode.ChildNodes) {
        if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and $child.Name -eq 'Section') {
            $sections.Add($child)
        }
    }

    foreach ($section in $sections) {
        $itemsNode = $section['Items']
        if ($null -eq $itemsNode) {
            continue
        }

        $items = [System.Collections.Generic.List[System.Xml.XmlElement]]::new()
        foreach ($item in $itemsNode.GetElementsByTagName('Item')) {
            $items.Add($item)
        }

        if ($items.Count -le $MaxItems) {
            continue
        }

        while ($itemsNode.HasChildNodes) {
            [void] $itemsNode.RemoveChild($itemsNode.FirstChild)
        }

        $limit = [Math]::Min($MaxItems, $items.Count)
        for ($i = 0; $i -lt $limit; $i++) {
            [void] $itemsNode.AppendChild($items[$i])
        }

        $insertAfter = $section
        for ($start = $MaxItems; $start -lt $items.Count; $start += $MaxItems) {
            $newSection = $Document.CreateElement('Section')
            [void] $newSection.AppendChild($section['Label'].CloneNode($true))

            $newItems = $Document.CreateElement('Items')
            $end = [Math]::Min($start + $MaxItems, $items.Count)
            for ($i = $start; $i -lt $end; $i++) {
                [void] $newItems.AppendChild($items[$i])
            }

            [void] $newSection.AppendChild($newItems)
            if ($null -ne $insertAfter.NextSibling) {
                [void] $sectionsNode.InsertBefore($newSection, $insertAfter.NextSibling)
            }
            else {
                [void] $sectionsNode.AppendChild($newSection)
            }
            $insertAfter = $newSection
        }
    }
}

function Write-ResearchRadialMenu {
    param(
        [Parameter(Mandatory = $true)] [string] $SourcePath,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $source = New-Object System.Xml.XmlDocument
    $source.PreserveWhitespace = $false
    $source.Load($SourcePath)

    $toolbar = $null
    foreach ($menu in $source.GetElementsByTagName('RadialMenu')) {
        if ($menu['Id'] -and $menu['Id'].GetAttribute('Subtype') -eq 'Toolbar') {
            $toolbar = $menu
            break
        }
    }

    if ($null -eq $toolbar) {
        throw "Toolbar radial menu was not found in $SourcePath"
    }

    $output = New-Object System.Xml.XmlDocument
    $declaration = $output.CreateXmlDeclaration('1.0', 'utf-8', $null)
    [void] $output.AppendChild($declaration)

    $definitions = $output.CreateElement('Definitions')
    $xsd = $output.CreateAttribute('xmlns', 'xsd', 'http://www.w3.org/2000/xmlns/')
    $xsd.Value = 'http://www.w3.org/2001/XMLSchema'
    [void] $definitions.Attributes.Append($xsd)
    $xsi = $output.CreateAttribute('xmlns', 'xsi', 'http://www.w3.org/2000/xmlns/')
    $xsi.Value = 'http://www.w3.org/2001/XMLSchema-instance'
    [void] $definitions.Attributes.Append($xsi)
    [void] $output.AppendChild($definitions)

    $radialMenus = $output.CreateElement('RadialMenus')
    [void] $definitions.AppendChild($radialMenus)

    $importedToolbar = [System.Xml.XmlElement] $output.ImportNode($toolbar, $true)
    [void] $radialMenus.AppendChild($importedToolbar)

    $productionSection = Find-RadialSectionByLabel -Menu $importedToolbar -Label 'RadialMenuGroupTitle_ProductionMedical'
    if ($null -ne $productionSection) {
        Set-RadialSectionItems -Document $output -Section $productionSection -Label 'RadialMenuGroupTitle_Production' -Subtypes @(
            'RefineryGroup',
            'BasicAssemblerGroup',
            'AssemblerGroup',
            'UpgradeModuleGroup',
            'GasGeneratorGroup',
            'FoodProductionGroup'
        )

        $medicalSection = New-RadialSection -Document $output -Label 'RadialMenuGroupTitle_ProductionMedical' -Subtypes @(
            'SurvivalKitGroup',
            'MedicalGroup',
            'CryoGroup'
        )
        if ($null -ne $productionSection.NextSibling) {
            [void] $productionSection.ParentNode.InsertBefore($medicalSection, $productionSection.NextSibling)
        }
        else {
            [void] $productionSection.ParentNode.AppendChild($medicalSection)
        }
    }

    $utilitySection = Find-RadialSectionByLabel -Menu $importedToolbar -Label 'RadialMenuGroupTitle_Utility1'
    if ($null -ne $utilitySection) {
        Set-RadialSectionItems -Document $output -Section $utilitySection -Label 'RadialMenuGroupTitle_Utility1' -Subtypes @(
            'ShipToolGroup',
            'ShipWelderGroup',
            'ShipDrillGroup',
            'ProjectorGroup',
            'CameraGroup',
            'LightingGroup',
            'SpotlightGroup',
            'HeatVentGroup'
        )
    }

    $utilityTwoSection = Find-RadialSectionByLabel -Menu $importedToolbar -Label 'RadialMenuGroupTitle_Utility2'
    if ($null -ne $utilityTwoSection) {
        Set-RadialSectionItems -Document $output -Section $utilityTwoSection -Label 'RadialMenuGroupTitle_Utility2' -Subtypes @(
            'SoundGroup',
            'OreDetectorGroup',
            'BeaconGroup',
            'AntennaGroup',
            'BroadcastControllerGroup',
            'SafeZoneGroup',
            'StoreGroup'
        )
    }

    $hitechSection = Find-RadialSectionByLabel -Menu $importedToolbar -Label 'RadialMenuGroupTitle_Hitech'
    if ($null -ne $hitechSection) {
        Set-RadialSectionItems -Document $output -Section $hitechSection -Label 'RadialMenuGroupTitle_Hitech' -Subtypes @(
            'JumpDriveGroup',
            'GravityGroup',
            'VirtualMassGroup'
        )

        $prototechSection = New-RadialSection -Document $output -Label 'RadialMenuGroupTitle_Hitech' -Subtypes @(
            'PrototechGroup',
            'PrototechThrusterGroup',
            'PrototechRefineryGroup',
            'PrototechAssemblerGroup',
            'PrototechGyroGroup',
            'PrototechBatteryGroup',
            'PrototechDrillGroup',
            'PrototechReactorGroup'
        )
        if ($null -ne $hitechSection.NextSibling) {
            [void] $hitechSection.ParentNode.InsertBefore($prototechSection, $hitechSection.NextSibling)
        }
        else {
            [void] $hitechSection.ParentNode.AppendChild($prototechSection)
        }
    }

    Split-RadialSections -Document $output -Menu $importedToolbar -MaxItems 8

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
    $writer = [System.Xml.XmlWriter]::Create($Path, $settings)
    try {
        $output.Save($writer)
    }
    finally {
        $writer.Close()
    }
}

if (-not (Test-Path -LiteralPath $SpaceEngineersData)) {
    throw "Space Engineers data folder not found: $SpaceEngineersData"
}

if (-not (Test-Path -LiteralPath $OutputData)) {
    New-Item -ItemType Directory -Path $OutputData | Out-Null
}

if (-not (Test-Path -LiteralPath $ScriptOutput)) {
    New-Item -ItemType Directory -Path $ScriptOutput | Out-Null
}

if (-not (Test-Path -LiteralPath $GeneratedCatalogDirectory)) {
    New-Item -ItemType Directory -Path $GeneratedCatalogDirectory -Force | Out-Null
}

$blocks = Get-CubeBlockIds -DataRoot $SpaceEngineersData
$catalog = New-ResearchCatalog -Blocks $blocks

Write-ResearchBlocks -Entries $catalog.Entries -Path (Join-Path $OutputData 'ResearchBlocks.sbc')
Write-VanillaResearchGroups -Count $VanillaResearchGroupCount -Path (Join-Path $OutputData 'ResearchGroups.sbc')
Write-UnlockerResearchGroups -Groups $catalog.Groups -Path (Join-Path $OutputData 'ResearchUnlockerGroups.sbc')
Write-UnlockerBlocks -Groups $catalog.Groups -Path (Join-Path $OutputData 'ResearchUnlockers.sbc')
Write-ResearchSchematicConsumables -Groups $catalog.Groups -Path (Join-Path $OutputData 'PhysicalItems_ResearchSchematics.sbc')
Write-GeneratedCatalog -Entries $catalog.Entries -Path $GeneratedCatalogPath
Write-ResearchBlockVariantGroups -Path (Join-Path $OutputData 'BlockVariantGroups_Research.sbc')
Write-ResearchRadialMenu -SourcePath (Join-Path $SpaceEngineersData 'RadialMenu.sbc') -Path (Join-Path $OutputData 'RadialMenu_Research.sbc')

$wroteDocs = $false
if (-not [string]::IsNullOrWhiteSpace($DocsSchematicGroupsPath)) {
    $docsSchematicGroupsDirectory = Split-Path -Parent $DocsSchematicGroupsPath
    if (-not [string]::IsNullOrWhiteSpace($docsSchematicGroupsDirectory) -and -not (Test-Path -LiteralPath $docsSchematicGroupsDirectory)) {
        New-Item -ItemType Directory -Path $docsSchematicGroupsDirectory -Force | Out-Null
    }

    Write-DocsSchematicGroups -Entries $catalog.Entries -Groups $catalog.Groups -Path $DocsSchematicGroupsPath
    $wroteDocs = $true
}

$summary = "Generated Working Knowledge data for $($catalog.Entries.Count) cube blocks, $($catalog.Groups.Count) research groups, and $($catalog.Groups.Count) hidden unlockers"
if ($wroteDocs) {
    $summary += ", plus schematic docs"
}

Write-Host "$summary."
