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

The most important fields are:

```xml
<Name>Working Knowledge Layer - My Block Pack</Name>
<Makers>Your Name</Makers>
```

`Name` is shown in the Space Engineers mod list. `Makers` should be your author name or alias.

## 3. Add ResearchBlocks.sbc Entries

For each source block not already registered by Working Knowledge, add one entry. A layer that only moves existing Working Knowledge blocks can omit this file.

```xml
<ResearchBlock xsi:type="ResearchBlock">
  <Id Type="CubeBlock" Subtype="ExampleBlock" />
  <UnlockedByGroups />
</ResearchBlock>
```

The `Type` and `Subtype` values come from the source mod's SBC block definition:

```xml
<Id>
  <TypeId>MyObjectBuilder_CubeBlock</TypeId>
  <SubtypeId>ExampleBlock</SubtypeId>
</Id>
```

Remove the `MyObjectBuilder_` prefix from `TypeId`. In this example, the research ID becomes:

```xml
<Id Type="CubeBlock" Subtype="ExampleBlock" />
```

Leave `UnlockedByGroups` empty. Working Knowledge binds the real unlock group at runtime.

## 4. Add block_mappings.txt Entries

For each source block, add one mapping:

```text
CubeBlock/ExampleBlock = structure.industrial
```

Use [schematic_groups.md](schematic_groups.md) to pick IDs.

The highest-priority valid mapping wins, including over built-in Working Knowledge assignments. In the normal in-game Active Mods list, the higher entry has higher priority. The optional `override` prefix can document an intentional built-in move:

```text
override BatteryBlock/LargeBlockBatteryBlock = example.power_storage
```

## 5. Optional: Define Custom Groups

The copied `ExampleMod` demonstrates the versioned `schematic_groups.txt` format and its three generated definition files. Keep those files and replace the example IDs, or let `Start.ps1` generate them. Custom group IDs and definition subtypes are published save contracts and should never be renamed.

For a slower walkthrough of these files, read [editing_generated_layers.md](editing_generated_layers.md).

## 6. Validate And Test Priority

Run:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-MyBlockPack"
```

Use this normal in-game Active Mods list, shown top to bottom with highest priority first:

1. Your layer
2. The source block mod
3. Working Knowledge

In game, check that the source blocks appear under the intended Working Knowledge schematic family and that construction, grinding, repair, and salvage behavior follows that family.

Before releasing the result, complete [Publishing A Layer](publishing_layers.md), including a fresh-world test and `/wk admin audit`.
