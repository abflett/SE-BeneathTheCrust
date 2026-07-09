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

## 3. Select A Source Mod

The script can look in common Space Engineers mod folders:

- `%APPDATA%\SpaceEngineers\Mods`
- Steam Workshop content folder, when it can be found
- A custom folder you type in

Choose the block mod you want to support.

## 4. Pick A Default Schematic Group

Most block packs are mostly one kind of block. Pick the best default group first.

Examples:

- Trusses, frames, beams, platforms: `structure.industrial`
- Interior lights: `utility.interior_lighting`
- Cargo containers: `logistics.cargo_storage`
- Doors: `structure.door`
- Batteries: `power.battery`

The script assigns every found block to the default group.

## 5. Override Outliers

The script can walk through the found blocks and let you override only the blocks that do not fit the default.

For example, a truss pack might mostly use `structure.industrial`, but a truss light should use `utility.interior_lighting`.

## 6. Choose Output

Choose the output folder and layer folder name, such as:

```text
WKL-ExampleBlockMod
```

The script creates a normal Space Engineers mod root there.

## 7. Review And Test

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
