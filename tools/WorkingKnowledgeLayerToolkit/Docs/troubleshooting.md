# Troubleshooting

## What Is The 244850 Folder?

`244850` is the Steam App ID for Space Engineers.

Steam stores Workshop downloads by game under:

```text
...\Steam\steamapps\workshop\content\<Steam App ID>
```

For Space Engineers, that becomes:

```text
...\Steam\steamapps\workshop\content\244850
```

Each child folder inside `244850` is usually one downloaded Workshop item, named by its Workshop item ID.

## What Is The Local Mods Folder?

The local Space Engineers mods folder is usually:

```text
%APPDATA%\SpaceEngineers\Mods
```

This is where local test mods, manually copied mods, and mods deployed by local build scripts usually appear.

## The Script Finds No Blocks

Confirm you selected the source mod root, not a parent folder with no `.sbc` files.

The toolkit scans public cube block definitions under `.sbc` files. It skips unreadable XML and blocks marked:

```xml
<Public>false</Public>
```

It also skips unplaceable support definitions that are hidden from the G-menu, marked non-standalone, have no physics, and set voxel placement to `None`. Some script/UI mods define fake cube blocks for toolbar or HUD behavior; those should not become Working Knowledge layer entries.

If you select a parent folder, the toolkit only lists child mod folders that contain public cube block definitions. Mods with scripts, research mappings, blueprints, or non-block content but no public block definitions will not appear in the block-set list.

The toolkit skips Working Knowledge itself. It marks block IDs already covered by Working Knowledge and can include them when you choose explicit overrides. Mods that only replace vanilla definitions therefore appear with zero new blocks but can still be selected for remapping.

## The Mod List Uses Numbers Instead Of Names

Steam Workshop folders are often named with Workshop item IDs. When a folder name looks like a Workshop item ID, the toolkit tries to look up the public Workshop title through Steam's Workshop details API.

If Steam is offline, slow, blocked by a firewall, or does not return a title, the toolkit reads `modinfo.sbc` when present. If that is also unavailable, it falls back to the folder name.

## A Generated Layer Does Not Affect Blocks

Check load order:

1. Working Knowledge
2. The source block mod
3. The generated layer

Also confirm every block has both:

- a `Data/ResearchBlocks.sbc` entry
- a `Data/WorkingKnowledge/block_mappings.txt` mapping

In the loaded world, an admin can run:

```text
/wk admin audit
```

The command reports missing blocks, missing research entries, malformed mappings/groups, duplicate IDs, definition collisions, conflicting block claims, missing custom definitions, and invalid built-in remaps. More than twelve issues are available in `SpaceEngineers.log` and the F11 mod-error screen.

Run the offline validator first:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\YourLayer"
```

## A Custom Group Is Inactive

Confirm `schematic_groups.txt` begins with `version = 1`, has five pipe-separated fields, and uses a unique stable ID. Its research group, `WkKnUnlocker_...` block, and exact Data Schematic definitions must exist and match the declared subtypes. Cross-layer collisions are rejected rather than resolved by load order.

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
