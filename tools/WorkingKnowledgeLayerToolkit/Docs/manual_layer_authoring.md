# Manual Layer Authoring

Use this path when you do not want to run `Start.ps1`.

## 1. Copy The Example

Copy:

```text
ExampleMod/
```

Rename the copied folder to your layer name, such as:

```text
WKL-MyBlockPack
```

## 2. Edit modinfo.sbc

Set the mod name, description, version, and author.

Recommended name format:

```text
Working Knowledge Layer - My Block Pack
```

## 3. Add ResearchBlocks.sbc Entries

For each source block, add one entry:

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleBlock" />
  <UnlockedByGroups />
</ResearchBlock>
```

## 4. Add block_mappings.txt Entries

For each source block, add one mapping:

```text
CubeBlock/ExampleBlock = structure.industrial
```

Use [schematic_groups.md](schematic_groups.md) to pick IDs.

## 5. Test Load Order

Use this load order:

1. Working Knowledge
2. The source block mod
3. Your layer

In game, check that the source blocks appear under the intended Working Knowledge schematic family and that construction, grinding, repair, and salvage behavior follows that family.
