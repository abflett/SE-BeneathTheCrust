# Quick Start

Use this guide to create a Working Knowledge compatibility layer for a Space Engineers block mod.

## 1. Unzip And Run

Unzip the toolkit anywhere convenient. Run `Start.bat`, or open Windows PowerShell in the toolkit folder and run:

```powershell
.\Start.ps1
```

`Start.bat` uses a process-only execution-policy bypass; it does not change the computer's saved policy.

## 2. Select A Folder To Scan

The numbered menu offers common locations:

- `%APPDATA%\SpaceEngineers\Mods` for local mods.
- `...\Steam\steamapps\workshop\content\244850` for downloaded Workshop mods.
- A custom folder you type in.

`244850` is the Steam App ID for Space Engineers. You may select a single mod root or a parent containing multiple mod folders.

The scan output remains visible so you can see which folders were checked. Unreadable or non-block files are skipped.

## 3. Select Block Sets

The initial numbered list contains sets with at least one new public block. Sets containing only blocks already covered by Working Knowledge are hidden from this first list.

The last menu option shows all sets, including covered-only sets. Use that advanced view when you intentionally want to reorganize existing Working Knowledge blocks. Covered mappings are written with the optional `override` label for readability; the label does not change priority.

Select one or more numbers separated by spaces, such as `1 3`. The displayed default is used when you press Enter.

## 4. Choose Groups

The toolkit asks whether to define custom schematic groups. Most layers should first reuse the built-in list. Add a custom group when the source blocks represent a distinct technology that should have its own research, Proficiency, fragment, and exact Data Schematic identity.

For every custom group, choose:

- A stable namespaced ID, such as `author.arc.reinforced_truss`.
- A player-facing display name and description.
- A fragment tier: `Common`, `Uncommon`, `Rare`, `Prototech`, or `None`.

Do not rename a published group ID or generated definition subtype. They are save and compatibility contracts.

Next, choose the default group for the selected blocks. Common examples are:

- Trusses, frames, beams, platforms: `structure.industrial`
- Interior lights: `utility.interior_lighting`
- Cargo containers: `logistics.cargo_storage`
- Doors: `structure.door`
- Batteries: `power.battery`

## 5. Review Outliers

If requested, the toolkit walks through blocks one at a time. Each block uses the same numbered style:

1. Choose another group
2. Stop reviewing overrides
3. Keep the current group (default)

Press Enter to keep the current assignment.

## 6. Choose Output

Provide a display name, author, folder name, and output parent. The default output parent is normally `%APPDATA%\SpaceEngineers\Mods`, which makes the layer immediately available to local test worlds.

Recommended names:

```text
Display: Working Knowledge Layer - Name Of Source Mod
Folder:  WKL-NameOfSourceMod
```

If the folder already exists, the toolkit asks before replacing generated files. It preserves unrelated files. If the new layer has no custom groups, obsolete custom-group files from an earlier generation are removed.

## 7. Review And Validate

The generator validates the output automatically. The most commonly edited files are:

```text
Data/ResearchBlocks.sbc
Data/WorkingKnowledge/block_mappings.txt
Data/WorkingKnowledge/schematic_groups.txt   (custom groups only)
```

After manual edits, run from the toolkit folder:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-ExampleBlockMod"
```

To preview conflicts, pass layers from lowest to highest priority:

```powershell
.\Validate.ps1 -LayerPath @("C:\Path\To\WKL-Lower", "C:\Path\To\WKL-Higher")
```

The last validator path wins valid conflicts. In the normal in-game Active Mods list, put that desired winner above the other layer.

## 8. Test In Game

Use this top-to-bottom Active Mods order:

1. Your generated layer
2. The source block mod
3. Working Knowledge

Use a fresh test world. Confirm the blocks are in the intended schematic groups, custom fragment and exact Data Schematic items appear when applicable, and save/reload works. Then run:

```text
/wk admin audit
```

Resolve unexpected warnings or errors before publishing. See [Publishing A Layer](Docs/publishing_layers.md) for the complete release checklist.
