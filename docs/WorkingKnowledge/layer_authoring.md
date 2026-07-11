# Working Knowledge Layer Authoring

Working Knowledge layers are small compatibility mods that assign blocks from another mod into Working Knowledge schematic families.

Use a layer when a block mod should participate in Working Knowledge research, Proficiency, welding, grinding, repair, and salvage behavior without adding those block definitions directly to the main Working Knowledge catalog.

The user-facing package for creating layers is the [Working Knowledge Layer Toolkit](../../tools/WorkingKnowledgeLayerToolkit/README.md). It is designed to be zipped and released as a standalone download so users can run an interactive script, copy an example mod, or read the included docs without needing this repository.

## Recommended Shape

Use this naming pattern:

- Folder: `mods/WKL-ExampleBlockMod`
- Mod name: `Working Knowledge Layer - Example Block Mod`
- Workshop title: `Working Knowledge Layer - Example Block Mod`

For public Workshop layers, set Workshop requirements for:

- Working Knowledge
- The source block mod

Steam should resolve downstream requirements such as Text HUD API through Working Knowledge.

## Required Files

A layer mod needs two data files.

`Data/ResearchBlocks.sbc` tells vanilla Space Engineers that the target blocks are progression-aware research blocks. Without this file, Working Knowledge can still know the block ID exists in a text mapping, but vanilla progression will not expose the block correctly as part of a research unlock path.

`Data/WorkingKnowledge/block_mappings.txt` tells Working Knowledge which schematic family each block should use.

Minimal folder:

```text
MyLayerMod/
  modinfo.sbc
  Data/
    ResearchBlocks.sbc
    WorkingKnowledge/
      block_mappings.txt
```

## Mapping Format

`block_mappings.txt` is intentionally flat and line-based:

```text
# comments start with #
Type/Subtype = working.knowledge.schematic.id
```

Example:

```text
CubeBlock/Arc_Truss_I = structure.industrial
InteriorLight/Arc_Truss_LF = utility.interior_lighting
```

The `Type/Subtype` value must match the block definition ID after removing the `MyObjectBuilder_` prefix from the type.

For example, this SBC block ID:

```xml
<Id>
  <TypeId>MyObjectBuilder_CubeBlock</TypeId>
  <SubtypeId>ExampleBlock</SubtypeId>
</Id>
```

maps as:

```text
CubeBlock/ExampleBlock = structure.industrial
```

## ResearchBlocks.sbc Format

Add one `ResearchBlock` entry per supported block:

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleBlock" />
  <UnlockedByGroups />
</ResearchBlock>
```

Leave `UnlockedByGroups` empty in the layer. Working Knowledge rebuilds the loaded research definitions at runtime and binds the block to the correct generated unlocker group.

## Current Schematic IDs

| Schematic ID | Display Name | Tier |
|---|---|---|
| `armor.heavy` | Heavy Armor Schematics | Uncommon |
| `armor.light` | Light Armor Schematics | Common |
| `automation.ai_control` | AI Control Schematics | Rare |
| `automation.logic` | Automation Logic Schematics | Uncommon |
| `communications` | Communications Schematics | Common |
| `control.interfaces` | Control Interface Schematics | Uncommon |
| `control.stations` | Control Station Schematics | Common |
| `decor.decorative_fixtures` | Decorative Fixture Schematics | Uncommon |
| `decor.habitat_fixtures` | Habitat Fixture Schematics | Uncommon |
| `decor.signage` | Signage Schematics | Common |
| `economy.station_services` | Station Services Schematics | Rare |
| `fundamentals` | Fundamental Schematics | None |
| `gas.processing` | Gas Processing Schematics | Common |
| `gas.storage` | Gas Storage Schematics | Common |
| `life_support` | Life Support Schematics | Uncommon |
| `logistics.cargo_storage` | Cargo Storage Schematics | Common |
| `logistics.cargo_transfer` | Cargo Transfer Schematics | Common |
| `logistics.conveyor_network` | Conveyor Network Schematics | Common |
| `mechanical.systems` | Mechanical Systems Schematics | Common |
| `mechanical.wheel_systems` | Wheel Systems Schematics | Common |
| `power.battery` | Battery Schematics | Common |
| `power.hydrogen_engine` | Hydrogen Engine Schematics | Uncommon |
| `power.reactor` | Reactor Schematics | Rare |
| `power.renewable` | Renewable Power Schematics | Common |
| `production.advanced` | Advanced Production Schematics | Rare |
| `production.basic` | Basic Production Schematics | Common |
| `production.food` | Food Production Schematics | Common |
| `propulsion.atmospheric_thruster` | Atmospheric Thruster Schematics | Uncommon |
| `propulsion.hydrogen_thruster` | Hydrogen Thruster Schematics | Uncommon |
| `propulsion.ion_thruster` | Ion Thruster Schematics | Rare |
| `prototech.assembler` | Prototech Assembler Schematics | Prototech |
| `prototech.battery` | Prototech Battery Schematics | Prototech |
| `prototech.drill` | Prototech Drill Schematics | Prototech |
| `prototech.gyroscope` | Prototech Gyroscope Schematics | Prototech |
| `prototech.jump_drive` | Prototech Jump Drive Schematics | Prototech |
| `prototech.reactor` | Prototech Reactor Schematics | Prototech |
| `prototech.refinery` | Prototech Refinery Schematics | Prototech |
| `prototech.thruster` | Prototech Thruster Schematics | Prototech |
| `structure.bridge` | Bridge Structure Schematics | Uncommon |
| `structure.door` | Door Schematics | Common |
| `structure.hangar_gate` | Hangar Gate Schematics | Uncommon |
| `structure.industrial` | Industrial Structure Schematics | Common |
| `structure.interior` | Interior Structure Schematics | Common |
| `structure.passage` | Passage Schematics | Uncommon |
| `structure.window` | Window Schematics | Common |
| `tools.drill` | Drill Schematics | Uncommon |
| `tools.grinder` | Grinder Schematics | Uncommon |
| `tools.welder` | Welder Schematics | Uncommon |
| `utility.directed_lighting` | Directed Lighting Schematics | Common |
| `utility.display_systems` | Display Systems Schematics | Uncommon |
| `utility.gravity` | Gravity Schematics | Rare |
| `utility.interior_lighting` | Interior Lighting Schematics | Common |
| `utility.jump_drive` | Jump Drive Schematics | Rare |
| `weapons.fixed_weapon` | Fixed Weapon Schematics | Uncommon |
| `weapons.turret` | Turret Schematics | Rare |

## Tooling

Preferred user-facing workflow:

```powershell
cd tools\WorkingKnowledgeLayerToolkit
.\Start.ps1
```

The toolkit script scans a selected source mod, finds public cube block definitions, asks for a default schematic group, lets the user override outliers, and generates a complete layer mod folder.

The toolkit also includes:

- `QUICKSTART.md` - normal script-driven workflow.
- `ExampleMod/` - complete copyable example for manual authoring.
- `Docs/mapping_format.md` - mapping file rules.
- `Docs/schematic_groups.md` - current schematic group IDs and usage hints.
- `Data/Template/` - internal templates used by the script.

Repository-maintenance helpers still exist for batch or scripted work:

```powershell
.\tools\inspect-space-engineers-block-mod.ps1 -ModPath "C:\Path\To\SomeBlockMod"

.\tools\new-working-knowledge-layer.ps1 `
  -LayerName "Working Knowledge Layer - Example Block Mod" `
  -SourceModName "Example Block Mod" `
  -FolderName "WKL-ExampleBlockMod" `
  -BlockListPath ".\example-blocks.txt" `
  -DefaultSchematicId "structure.industrial"
```

For the repository generator, the block list can contain either plain IDs:

```text
CubeBlock/ExampleTruss
InteriorLight/ExampleLight
```

or explicit mappings:

```text
CubeBlock/ExampleTruss = structure.industrial
InteriorLight/ExampleLight = utility.interior_lighting
```

## Validation Checklist

- The layer has `ResearchBlocks.sbc` entries for every mapped block.
- Every mapped block ID exists in the source block mod.
- Every schematic ID in `block_mappings.txt` appears in the current schematic ID table.
- The layer is loaded with Working Knowledge and the source block mod.
- In game, the blocks appear under the intended Working Knowledge schematic family in progression.
- Low-Proficiency construction, repair, grinding, and salvage behavior applies to the layer blocks.
- An admin can run `/wk admin audit` and see every expected mapping active with no issues.

Audit details also appear in `SpaceEngineers.log` and the F11 mod-error screen. Existing layer files do not need changes for the audit.

## Real Example

See:

- [WKL-ARCTrussSystem mod root](../../mods/WKL-ARCTrussSystem/README.md)
- [WKL - ARC Truss System changelog](../WKL-ARCTrussSystem/changelog.md)
- [WKL - ARC Truss System Workshop description](../WKL-ARCTrussSystem/workshop_description_bbcode.txt)
