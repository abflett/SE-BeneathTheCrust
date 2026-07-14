# Working Knowledge Layer Toolkit

This toolkit creates small compatibility layer mods for **Working Knowledge**.

A layer maps blocks from another Space Engineers block mod into built-in or layer-defined Working Knowledge schematic groups. It can also explicitly remap built-in blocks. Once mapped, those blocks can participate in Working Knowledge research gates, vanilla progression unlocks, Proficiency, welding, repair, grinding, and salvage behavior.

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

Load order for manual local testing:

1. Working Knowledge
2. The source block mod
3. The generated Working Knowledge Layer mod

After generation or editing, validate the layer:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-ExampleBlockMod"
```
