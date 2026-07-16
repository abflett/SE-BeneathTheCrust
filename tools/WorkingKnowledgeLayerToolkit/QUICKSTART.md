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

Working Knowledge itself is skipped automatically. The initial selection list shows block sets containing new blocks and hides covered-only sets. A final menu option redraws the list with every set and includes already-covered blocks as explicit overrides. Use that advanced view only when the layer intentionally reorganizes existing Working Knowledge technology.

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

## 5. Choose Built-In Remaps And Custom Groups

The block-set menu includes an option to reveal already-mapped blocks. In that advanced view, covered-only sets are selectable and covered output lines use the optional `override` prefix to make the intent clear. Normal mappings have the same priority.

It also asks whether the layer should define custom schematic groups. For each custom group, choose a stable ID, display name, and tier. The toolkit generates collision-resistant definition subtypes and all required `.sbc` files.

Do not rename a custom group ID after publishing; research and Proficiency saves use it as their stable key.

## 6. Pick A Default Schematic Group

Most block packs are mostly one kind of block. Pick the best default group first.

Examples:

- Trusses, frames, beams, platforms: `structure.industrial`
- Interior lights: `utility.interior_lighting`
- Cargo containers: `logistics.cargo_storage`
- Doors: `structure.door`
- Batteries: `power.battery`

The script assigns every found block to the default group.

## 7. Change Outliers

The script can walk through the found blocks and let you override only the blocks that do not fit the default.

Each block uses a numbered action menu: choose another group, stop reviewing, or keep the current assignment. Keeping the current assignment is option `3` and the Enter-key default.

For example, a truss pack might mostly use `structure.industrial`, but a truss light should use `utility.interior_lighting`.

## 8. Choose Output

Choose the output folder and layer folder name, such as:

```text
WKL-ExampleBlockMod
```

The script also asks for the layer display name and author/maker name. The recommended display name is:

```text
Working Knowledge Layer - Name Of Source Mod
```

The author/maker name is written to `modinfo.sbc`.

The script creates a normal Space Engineers mod root in the output folder.

## 9. Review, Validate, And Test

Open the generated files:

```text
Data/ResearchBlocks.sbc
Data/WorkingKnowledge/block_mappings.txt
Data/WorkingKnowledge/schematic_groups.txt   (when custom groups were added)
```

Use [Docs/mapping_format.md](Docs/mapping_format.md) and [Docs/schematic_groups.md](Docs/schematic_groups.md) if you want to adjust mappings by hand.

For a plain-language explanation of the editable files, read [Docs/editing_generated_layers.md](Docs/editing_generated_layers.md).

Validate the generated or edited folder:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-ExampleBlockMod"
```

To preview two layers together, pass paths from lowest to highest priority:

```powershell
.\Validate.ps1 -LayerPath @("C:\Path\To\WKL-HardArmor", "C:\Path\To\WKL-DenseArmor")
```

The last path has the highest numeric priority, so its valid group declarations and mappings win. In the normal in-game Active Mods list, put that desired winner above the conflicting layer.

Then use this normal in-game Active Mods list, shown top to bottom:

1. Your generated layer
2. The source block mod
3. Working Knowledge
