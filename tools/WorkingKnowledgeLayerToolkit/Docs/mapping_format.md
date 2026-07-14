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

## Explicit Built-In Overrides

A normal mapping cannot replace a built-in Working Knowledge mapping. Prefix an intentional remap with `override`:

```text
override BatteryBlock/LargeBlockBatteryBlock = example.power_storage
```

Use the prefix only when the block is already cataloged by Working Knowledge. If multiple loaded layers claim the same block, every claim is rejected so load order cannot silently select a winner.

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

Every mapped block should also have a matching entry in:

```text
Data/ResearchBlocks.sbc
```

Example:

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />
  <UnlockedByGroups />
</ResearchBlock>
```

Leave `UnlockedByGroups` empty. Working Knowledge binds mapped blocks to the correct generated unlocker groups at runtime.

Run `Validate.ps1 -LayerPath <layer folder>` after editing. In game, `/wk admin audit` reports loaded-definition and cross-layer conflicts that an offline validator cannot see.
