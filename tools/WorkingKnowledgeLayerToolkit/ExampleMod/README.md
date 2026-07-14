# Working Knowledge Layer - Example Block Mod

This is a complete example layer mod for manual editing.

It is not meant to be published as-is. Copy this folder, rename it, replace the example block IDs, and adjust the schematic groups.

## Load Order

1. Working Knowledge
2. Example Block Mod
3. Working Knowledge Layer - Example Block Mod

## Files To Edit

- `modinfo.sbc` - change the mod name, description, version, and author if needed.
- `Data/ResearchBlocks.sbc` - add one `ResearchBlock` entry for each block from the source mod.
- `Data/WorkingKnowledge/block_mappings.txt` - map each block ID to a Working Knowledge schematic group.
- `Data/WorkingKnowledge/schematic_groups.txt` - define this example's custom schematic group.
- `Data/ResearchUnlockerGroups.sbc`, `Data/ResearchUnlockers.sbc`, and `Data/PhysicalItems_ResearchSchematics.sbc` - provide the custom group's game definitions.

Before copying this folder, read `../Docs/editing_generated_layers.md` for a slower walkthrough of these files.

Use `../Docs/schematic_groups.md` to choose schematic IDs.

The battery mapping demonstrates the explicit `override` prefix required to replace a built-in Working Knowledge mapping. Remove it when copying the example unless that remap is intentional.

After editing, run `..\Validate.ps1 -LayerPath .` from this folder's parent toolkit directory.
