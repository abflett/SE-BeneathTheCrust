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

A mapping-only layer always needs `block_mappings.txt`. It also needs `ResearchBlocks.sbc` when it introduces blocks that Working Knowledge does not already register. Existing layers remain valid without changes.

`Data/ResearchBlocks.sbc` tells vanilla Space Engineers that new target blocks are progression-aware research blocks. A layer that only moves blocks already registered by Working Knowledge can rely on the base definitions and omit this file. A layer for blocks from another mod must include it so vanilla progression can expose those blocks as part of a research unlock path.

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

A layer that defines custom schematic groups also includes:

```text
MyLayerMod/
  Data/
    PhysicalItems_ResearchSchematics.sbc
    ResearchUnlockerGroups.sbc
    ResearchUnlockers.sbc
    WorkingKnowledge/
      schematic_groups.txt
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

Working Knowledge mappings are the base assignments. Any loaded layer can replace them with a normal mapping. The optional `override` prefix documents that the remap is intentional:

```text
override CubeBlock/LargeBlockBatteryBlock = example.power_storage
```

If more than one layer claims the same block, the last valid mapping in Space Engineers mod load order wins. Reversing the two layers reverses the winner. A mapping to an unknown or inactive group is skipped, leaving the previous valid assignment active.

## Custom Group Format

Custom groups live in `Data/WorkingKnowledge/schematic_groups.txt`. The first content line is the format version, followed by one row per group. A sixth description field is optional:

```text
version = 1
# stable id | display name | tier | research group subtype | unlocker subtype | optional description
example.power_storage | Example Power Storage Schematics | Uncommon | WkKnLayer_Example_power_storage | WkKnUnlocker_Example_power_storage | Schematics for reinforced power-storage systems.
```

Valid tiers are `Common`, `Uncommon`, `Rare`, `Prototech`, and `None`. The schematic ID is the persisted research and Proficiency key. Never rename a published ID. The group and unlocker subtypes must match the layer's `.sbc` definitions, must be globally unique, and should also remain stable. Unlocker subtypes must begin with `WkKnUnlocker_`.

The toolkit generates:

- The empty vanilla research group used for runtime binding.
- The hidden unlocker cube block used for vanilla progression completion.
- A durable exact Data Schematic named from the stable ID.

Custom groups automatically flow through the runtime catalog, so mapped blocks appear in research, Proficiency, commands, Pedestal/LCD/HUD displays, fragment rewards, exact schematic rewards, and persisted stores.

## Load-Order Rules

Working Knowledge records every claim and resolves it in load order:

- A later valid declaration of the same group ID replaces its display name, description, tier, and declared wiring. If its required wiring is missing, it is skipped and the previous valid declaration remains active.
- A later valid block mapping replaces the built-in or earlier layer assignment.
- If different group IDs share a research-group, unlocker, or exact-schematic definition ID, only the later group remains active.
- A mapping targeting an unknown, displaced, or incomplete group is skipped and the previous valid block assignment remains.
- Missing block definitions, or missing `ResearchBlocks.sbc` entries for blocks not already registered by Working Knowledge, prevent that mapping from activating in game.

Run `/wk admin audit` after changing layer order. Expected base overrides are notices in the log. Multi-layer winners, risky group redefinitions, skipped claims, and missing definitions are warnings in chat, `SpaceEngineers.log`, and F11.

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

Add one `ResearchBlock` entry per supported block that Working Knowledge does not already register:

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

The toolkit script scans a selected source mod, finds public cube block definitions, can define custom schematic groups, asks for a default group, lets the user change outliers, and generates a complete layer mod folder. It can also include already-cataloged blocks as explicit overrides.

The toolkit also includes:

- `QUICKSTART.md` - normal script-driven workflow.
- `ExampleMod/` - complete copyable example for manual authoring.
- `Docs/mapping_format.md` - mapping file rules.
- `Docs/schematic_groups.md` - current schematic group IDs and usage hints.
- `Data/Template/` - internal templates used by the script.
- `Validate.ps1` - checks one layer or an ordered stack, resolves load-order winners, and validates custom metadata and definitions.

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

- The layer has `ResearchBlocks.sbc` entries for every mapped block not already registered by Working Knowledge.
- Every mapped block ID exists in the source block mod.
- Every schematic ID in `block_mappings.txt` appears in the current schematic ID table.
- Every custom group has stable metadata plus matching research-group, unlocker, and exact-schematic definitions.
- Intended built-in remaps may use the optional `override` prefix for readability.
- The layer is loaded with Working Knowledge and the source block mod.
- In game, the blocks appear under the intended Working Knowledge schematic family in progression.
- Low-Proficiency construction, repair, grinding, and salvage behavior applies to the layer blocks.
- An admin can run `/wk admin audit` and see every expected mapping active with no issues.
- `Validate.ps1 -LayerPath <layer folder>` succeeds before publishing.

Audit details also appear in `SpaceEngineers.log` and the F11 mod-error screen. Existing layer files do not need changes for the audit.

## Real Example

See:

- [WKL-ARCTrussSystem mod root](../../mods/WKL-ARCTrussSystem/README.md)
- [WKL - ARC Truss System changelog](../WKL-ARCTrussSystem/changelog.md)
- [WKL - ARC Truss System Workshop description](../WKL-ARCTrussSystem/workshop_description_bbcode.txt)
