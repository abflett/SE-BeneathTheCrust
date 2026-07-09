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
