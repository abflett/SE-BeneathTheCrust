# Quick Start

Use this path if you want the toolkit to create a Working Knowledge Layer for a block mod.

## 1. Unzip The Toolkit

Unzip `WorkingKnowledgeLayerToolkit` anywhere convenient.

The toolkit does not need to be inside the Beneath the Crust repository.

## 2. Run The Script

From PowerShell:

```powershell
.\Start.ps1
```

Or double-click:

```text
Start.bat
```

If Windows blocks script execution, `Start.bat` runs PowerShell with a process-only bypass for this script.

## 3. Select A Folder To Scan

The script can look in common Space Engineers mod folders:

- Local Space Engineers mods folder: `%APPDATA%\SpaceEngineers\Mods`
- Steam Workshop Space Engineers mods folder, when it can be found: `...\Steam\steamapps\workshop\content\244850`
- A custom folder you type in

`244850` is the Steam App ID for Space Engineers. Steam uses that number as the Workshop content folder name.

All choices are numbered. Pick the custom-folder number if your source mod is somewhere else.

## 4. Select Block Sets

The script scans the selected folder for mods that contain public cube block definitions. For Steam Workshop folders named by Workshop item ID, it tries to look up the public Workshop title through Steam first. If that lookup fails, it reads `modinfo.sbc` when available, then falls back to the folder name.

Working Knowledge itself is skipped automatically. Blocks already covered by Working Knowledge are also ignored. This is expected for mods that only override vanilla block definitions without adding new block IDs.

Example:

```text
[1] Example Truss Pack - contains 24 new public blocks
[2] Example Lights Pack - contains 5 new public blocks
[3] Example Doors Pack - contains 15 new public blocks
[4] Select all block sets and all blocks
```

Select one or more block sets:

```text
1
1 3
4
```

The all option is the default when more than one block set is found.

## 5. Pick A Default Schematic Group

Most block packs are mostly one kind of block. Pick the best default group first.

Examples:

- Trusses, frames, beams, platforms: `structure.industrial`
- Interior lights: `utility.interior_lighting`
- Cargo containers: `logistics.cargo_storage`
- Doors: `structure.door`
- Batteries: `power.battery`

The script assigns every found block to the default group.

## 6. Override Outliers

The script can walk through the found blocks and let you override only the blocks that do not fit the default.

For example, a truss pack might mostly use `structure.industrial`, but a truss light should use `utility.interior_lighting`.

## 7. Choose Output

Choose the output folder and layer folder name, such as:

```text
WKL-ExampleBlockMod
```

The script creates a normal Space Engineers mod root there.

## 8. Review And Test

Open the generated files:

```text
Data/ResearchBlocks.sbc
Data/WorkingKnowledge/block_mappings.txt
```

Use [Docs/mapping_format.md](Docs/mapping_format.md) and [Docs/schematic_groups.md](Docs/schematic_groups.md) if you want to adjust mappings by hand.

Then test with this load order:

1. Working Knowledge
2. The source block mod
3. Your generated layer
