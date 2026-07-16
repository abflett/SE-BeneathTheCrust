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

The toolkit skips Working Knowledge itself. It marks block IDs already covered by Working Knowledge and hides covered-only block sets from the initial selection list. Choose **Show all block sets, including already-covered blocks for explicit remapping** at the bottom of that menu to reveal those sets and include their covered blocks with the optional `override` marker.

## The Mod List Uses Numbers Instead Of Names

Steam Workshop folders are often named with Workshop item IDs. When a folder name looks like a Workshop item ID, the toolkit tries to look up the public Workshop title through Steam's Workshop details API.

If Steam is offline, slow, blocked by a firewall, or does not return a title, the toolkit reads `modinfo.sbc` when present. If that is also unavailable, it falls back to the folder name.

## A Generated Layer Does Not Affect Blocks

Check the normal in-game Active Mods list. It is shown highest priority first:

1. The generated layer
2. The source block mod
3. Working Knowledge

Space Engineers loads this visible list from bottom to top, so the higher entry wins conflicts.

Also confirm every block has:

- a `Data/WorkingKnowledge/block_mappings.txt` mapping
- a `Data/ResearchBlocks.sbc` entry if Working Knowledge does not already register that block

In the loaded world, an admin can run:

```text
/wk admin audit
```

The command reports missing blocks, missing research entries, malformed mappings/groups, priority winners, definition collisions, skipped claims, and missing custom definitions. Expected built-in replacements are log notices; resolved multi-layer conflicts and invalid claims also appear as warnings in F11.

Run the offline validator first:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\YourLayer"
```

## A Custom Group Is Inactive

Confirm `schematic_groups.txt` begins with `version = 1`, has five pipe-separated fields plus an optional description, and uses a stable ID. Its research group, `WkKnUnlocker_...` block, and exact Data Schematic definitions must exist and match the declared subtypes. When different IDs collide on those definitions, the highest-priority valid group owns them and lower-priority groups become inactive.

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
