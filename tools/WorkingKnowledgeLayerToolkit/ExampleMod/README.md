# Working Knowledge Layer - Example Block Mod

This is a complete example layer mod for manual editing.

It is not meant to be published as-is. Copy this folder, rename it, replace the example block IDs, and adjust the schematic groups.

## In-Game Priority

In the normal in-game Active Mods list, use this top-to-bottom order. Higher entries have higher priority and win conflicts.

1. Working Knowledge Layer - Example Block Mod
2. Example Block Mod
3. Working Knowledge

## Files To Edit

- `modinfo.sbc` - change the mod name, description, version, and author if needed.
- `Data/ResearchBlocks.sbc` - add one `ResearchBlock` entry for each block from the source mod.
- `Data/WorkingKnowledge/block_mappings.txt` - map each block ID to a Working Knowledge schematic group.
- `Data/WorkingKnowledge/schematic_groups.txt` - define this example's custom schematic group.
- `Data/ResearchUnlockerGroups.sbc`, `Data/ResearchUnlockers.sbc`, and `Data/PhysicalItems_ResearchSchematics.sbc` - provide the custom group's game definitions.

Before copying this folder, read `../Docs/editing_generated_layers.md` for a slower walkthrough of these files.

Use `../Docs/schematic_groups.md` to choose schematic IDs.

The battery mapping demonstrates the optional `override` prefix used to document an intentional replacement of a built-in Working Knowledge mapping. A normal mapping has the same priority.

After editing, run this from the toolkit folder:

```powershell
.\Validate.ps1 -LayerPath .\ExampleMod
```

Before releasing a copied layer, complete [Publishing A Layer](../Docs/publishing_layers.md). Do not publish this example unchanged.
