# Editing Generated Layers

Most users should run `Start.ps1` first. The script scans the source mod, finds block IDs, asks for schematic groups, and writes the layer files for you.

After generation, mapping-only layers have three files most users may need to review:

```text
modinfo.sbc
Data/ResearchBlocks.sbc
Data/WorkingKnowledge/block_mappings.txt
```

Layers with custom groups also have `schematic_groups.txt` plus three generated `.sbc` definition files. Prefer rerunning the toolkit when adding a group; manual editing requires keeping all four files aligned.

## modinfo.sbc

`modinfo.sbc` is the layer mod's name and metadata.

The most useful fields are:

```xml
<Name>Working Knowledge Layer - Example Block Mod</Name>
<Makers>Your Name</Makers>
```

`Name` is what players see in the Space Engineers mod list. Recommended format:

```text
Working Knowledge Layer - Name Of Source Mod
```

`Makers` is the author or alias for the layer. If you used the script, it asks for this value. If you copied `ExampleMod`, replace `Your Name`.

The other fields can usually stay as generated:

```xml
<Description>Adds Working Knowledge progression support for Example Block Mod blocks.</Description>
<Version>1.0.0</Version>
<IncludeInExport>True</IncludeInExport>
```

## ResearchBlocks.sbc

`ResearchBlocks.sbc` tells Space Engineers vanilla progression that blocks introduced by another mod can be unlocked by research groups. A hand-authored layer that only reassigns blocks already registered by Working Knowledge can rely on the base definitions and omit this file.

It looks more intimidating than it is. Each block is one repeated entry:

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />
  <UnlockedByGroups />
</ResearchBlock>
```

The important line is:

```xml
<Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />
```

Those values come from the source mod's block definition:

```xml
<Id>
  <TypeId>MyObjectBuilder_CubeBlock</TypeId>
  <SubtypeId>ExampleIndustrialTruss</SubtypeId>
</Id>
```

Use `TypeId` for `Type`, but remove the `MyObjectBuilder_` prefix.

Use `SubtypeId` for `Subtype`.

So:

```text
MyObjectBuilder_CubeBlock + ExampleIndustrialTruss
```

becomes:

```xml
<Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />
```

Leave this line empty:

```xml
<UnlockedByGroups />
```

Working Knowledge fills in the real unlock group at runtime.

## block_mappings.txt

`block_mappings.txt` tells Working Knowledge which schematic family each block belongs to.

Each line has this shape:

```text
Type/Subtype = schematic.group.id
```

Example:

```text
CubeBlock/ExampleIndustrialTruss = structure.industrial
```

The left side is the same block ID used in `ResearchBlocks.sbc`, written with a slash:

```text
CubeBlock/ExampleIndustrialTruss
```

The right side is a Working Knowledge schematic group:

```text
structure.industrial
```

Use [Schematic Groups](schematic_groups.md) to choose valid IDs.

To document an intentional replacement of a built-in mapping, add the optional prefix:

```text
override BatteryBlock/LargeBlockBatteryBlock = example.power_storage
```

## schematic_groups.txt

This optional versioned file defines groups owned by the layer:

```text
version = 1
example.power_storage | Example Power Storage Schematics | Uncommon | WkKnLayer_Example_power_storage | WkKnUnlocker_Example_power_storage
```

The first five fields are stable ID, display name, tier, vanilla research-group subtype, and hidden unlocker subtype. A trailing description is optional. The stable ID is stored in saves. Renaming it after publication creates a different schematic and leaves the old saved record inactive; redefining the same ID in a later layer preserves that identity while replacing its metadata.

Keep the generated `ResearchUnlockerGroups.sbc`, `ResearchUnlockers.sbc`, and `PhysicalItems_ResearchSchematics.sbc` entries synchronized with this metadata.

## When Is The Type Not CubeBlock?

The type is whatever the source block definition says after removing `MyObjectBuilder_`.

Examples:

```xml
<TypeId>MyObjectBuilder_CubeBlock</TypeId>
```

becomes:

```text
CubeBlock
```

```xml
<TypeId>MyObjectBuilder_InteriorLight</TypeId>
```

becomes:

```text
InteriorLight
```

```xml
<TypeId>MyObjectBuilder_CargoContainer</TypeId>
```

becomes:

```text
CargoContainer
```

The script handles this automatically. You only need to know this if you are editing by hand.

## How The Files Work Together

For every supported block, the layer should have:

```text
Data/ResearchBlocks.sbc
  <Id Type="CubeBlock" Subtype="ExampleIndustrialTruss" />

Data/WorkingKnowledge/block_mappings.txt
  CubeBlock/ExampleIndustrialTruss = structure.industrial
```

If a block from another mod appears in `block_mappings.txt` but not `ResearchBlocks.sbc`, Working Knowledge can understand the mapping, but vanilla progression may not expose the block correctly. Blocks already registered by Working Knowledge do not need a duplicate entry.

If a block appears in `ResearchBlocks.sbc` but not `block_mappings.txt`, vanilla progression knows the block exists, but Working Knowledge does not know which schematic family to use.

## Most Users Should Not Need This

The script creates these files for you. Manual editing is mainly for:

- changing a few outlier blocks to a better schematic group
- updating the layer after the source mod changes block IDs
- building a layer by copying `ExampleMod`

After any edit, run `Validate.ps1 -LayerPath <layer folder>`. Pass several paths to preview their load-order winners, then use `/wk admin audit` in a test world.
