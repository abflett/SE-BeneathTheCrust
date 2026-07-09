# Troubleshooting

## The Script Finds No Blocks

Confirm you selected the source mod root, not a parent folder with no `.sbc` files.

The toolkit scans public cube block definitions under `.sbc` files. It skips unreadable XML and blocks marked:

```xml
<Public>false</Public>
```

## A Generated Layer Does Not Affect Blocks

Check load order:

1. Working Knowledge
2. The source block mod
3. The generated layer

Also confirm every block has both:

- a `Data/ResearchBlocks.sbc` entry
- a `Data/WorkingKnowledge/block_mappings.txt` mapping

## The Wrong Blocks Are Grouped Together

Edit:

```text
Data/WorkingKnowledge/block_mappings.txt
```

Change the schematic ID after `=`.

Use [schematic_groups.md](schematic_groups.md) for valid IDs.

## The Source Mod Updates

If the source block mod changes subtype IDs, the layer may need to be regenerated or edited.

Run the toolkit again against the updated source mod and compare the new `block_mappings.txt` with your existing layer.

## The Layer Adds No Blocks

That is expected. A Working Knowledge Layer should not include the source mod's models, textures, recipes, or block definitions.

It only adds vanilla research entries and Working Knowledge mappings for blocks provided by another mod.
