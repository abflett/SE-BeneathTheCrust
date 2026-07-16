# Mapping Format

Working Knowledge layer mappings live in:

```text
Data/WorkingKnowledge/block_mappings.txt
```

Each non-comment line maps one Space Engineers block definition to one Working Knowledge schematic group:

```text
Type/Subtype = working.knowledge.schematic.id
```

Example:

```text
CubeBlock/ExampleIndustrialTruss = structure.industrial
InteriorLight/ExampleTrussLight = utility.interior_lighting
```

The right side may also be an ID from the same layer's versioned `schematic_groups.txt` file.

## Built-In Overrides

A normal mapping can replace a built-in Working Knowledge assignment. Authors may prefix an intentional remap with `override` to document their intent:

```text
override BatteryBlock/LargeBlockBatteryBlock = example.power_storage
```

The prefix does not change priority. If multiple loaded layers claim the same block, the last valid mapping in mod load order wins. `/wk admin audit` records the assignment history and winner.

## Block IDs

The block ID comes from the source block mod's SBC definition.

This SBC:

```xml
<Id>
  <TypeId>MyObjectBuilder_CubeBlock</TypeId>
  <SubtypeId>ExampleIndustrialTruss</SubtypeId>
</Id>
```

maps as:

```text
CubeBlock/ExampleIndustrialTruss = structure.industrial
```

Remove the `MyObjectBuilder_` prefix from the type.

If the source block uses a different `TypeId`, use that type instead:

```xml
<TypeId>MyObjectBuilder_InteriorLight</TypeId>
<SubtypeId>ExampleTrussLight</SubtypeId>
```

maps as:

```text
InteriorLight/ExampleTrussLight = utility.interior_lighting
```

The script reads this automatically. Manual authors only need to copy the type and subtype carefully.

## Comments

Lines beginning with `#` are comments:

```text
# Truss blocks
CubeBlock/ExampleIndustrialTruss = structure.industrial
```

## Matching ResearchBlocks.sbc

Every mapped block from another mod should also have a matching entry in:

```text
Data/ResearchBlocks.sbc
```

If a layer only reassigns blocks already registered by Working Knowledge, the base mod supplies those entries and the layer can omit this file.

Example:

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />
  <UnlockedByGroups />
</ResearchBlock>
```

Leave `UnlockedByGroups` empty. Working Knowledge binds mapped blocks to the correct generated unlocker groups at runtime.

Run `Validate.ps1 -LayerPath <layer folder>` after editing. To preview load-order winners, pass paths in the same order as the world mod list: `Validate.ps1 -LayerPath @(<first>, <second>)`. In game, `/wk admin audit` confirms the actual session order and loaded definitions.
