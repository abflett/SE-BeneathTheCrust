# Working Knowledge Layer Toolkit

This toolkit creates small compatibility layer mods for **Working Knowledge**.

A layer maps blocks from another Space Engineers block mod into built-in or layer-defined Working Knowledge schematic groups. Layers may remap built-in blocks, and the highest-priority valid layer wins when assignments overlap. In the normal in-game Active Mods list, entries nearer the top have higher priority because Space Engineers loads that list from bottom to top. Once mapped, those blocks can participate in Working Knowledge research gates, vanilla progression unlocks, Proficiency, welding, repair, grinding, and salvage behavior.

This folder is designed to be zipped and distributed as a standalone toolkit. It should work after being unzipped anywhere on disk.

## Download

Most users should download the ready-to-use zip:

[Download WorkingKnowledgeLayerToolkit-2026-07-09.zip](https://github.com/abflett/SE-BeneathTheCrust/releases/download/v1.0.0/WorkingKnowledgeLayerToolkit-2026-07-09.zip)

If that direct link changes, use the project releases page:

[SE-BeneathTheCrust Releases](https://github.com/abflett/SE-BeneathTheCrust/releases)

After downloading, unzip the file anywhere convenient. The zip contains a `WorkingKnowledgeLayerToolkit` folder with the script, docs, and example mod.

## Start Here

From the unzipped `WorkingKnowledgeLayerToolkit` folder, read [QUICKSTART.md](QUICKSTART.md), then run:

```powershell
.\Start.ps1
```

For users who prefer double-clicking from Explorer, run:

```text
Start.bat
```

## What Is Included

- `Start.ps1` - interactive layer generator.
- `Validate.ps1` - validates generated or manually edited layers.
- `Tests/Test-LayerResolution.ps1` - automated ordered conflict-resolution checks.
- `Tests/Deploy-ConflictTestLayers.ps1` - deploys local Hard Armor and Dense Armor layers for the in-game `0.13.0` test plan.
- `Start.bat` - simple launcher for `Start.ps1`.
- `ExampleMod/` - copyable example layer mod for manual editing.
- `Docs/` - [toolkit documentation](Docs/README.md), mapping format, schematic group, manual authoring, and troubleshooting notes.
- `Data/` - internal toolkit data used by the script.

Most users should only touch `Start.ps1`, `Start.bat`, `QUICKSTART.md`, `Docs/`, `ExampleMod/`, and the generated output mod.

`Data/Template/` is the internal template used by the script. Edit it only if you are intentionally changing how generated layers are shaped.

`Data/working_knowledge_block_keys.txt` is the built-in Working Knowledge block catalog. The script uses it to skip vanilla or already-supported block IDs, including mods that only override existing vanilla definitions.

## Output

Generated layers are normal Space Engineers mod roots. A typical output looks like:

```text
WKL-ExampleBlockMod/
  modinfo.sbc
  README.md
  Data/
    ResearchBlocks.sbc
    WorkingKnowledge/
      block_mappings.txt
      schematic_groups.txt       # when custom groups are defined
    ResearchUnlockerGroups.sbc   # when custom groups are defined
    ResearchUnlockers.sbc        # when custom groups are defined
    PhysicalItems_ResearchSchematics.sbc # when custom groups are defined
  Publishing/
    changelog.md
    workshop_description_bbcode.txt
```

Recommended normal in-game Active Mods list, shown top to bottom with highest priority first:

1. The generated Working Knowledge Layer mod
2. The source block mod
3. Working Knowledge

After generation or editing, validate the layer:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-ExampleBlockMod"
```

Pass multiple paths from **lowest to highest priority** to preview group and block winners. The last path receives the highest numeric priority and wins valid conflicts. This is the reverse of how the normal in-game Active Mods list is read from top to bottom.
