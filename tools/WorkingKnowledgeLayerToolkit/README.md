# Working Knowledge Layer Toolkit

Version **1.1.0**

This Windows toolkit creates small compatibility layer mods for **Working Knowledge**. A layer assigns blocks from another Space Engineers mod to built-in or layer-defined schematic groups without copying that mod's blocks, models, or recipes.

Layers can also reorganize blocks already covered by Working Knowledge. When assignments overlap, the highest-priority valid layer wins. In the normal in-game Active Mods list, entries nearer the top have higher priority.

## Download And Requirements

Download [WorkingKnowledgeLayerToolkit-1.1.0.zip](https://github.com/abflett/SE-BeneathTheCrust/releases/download/v1.1.0/WorkingKnowledgeLayerToolkit-1.1.0.zip), then unzip it anywhere convenient.

Use the [SE-BeneathTheCrust releases page](https://github.com/abflett/SE-BeneathTheCrust/releases) for release notes, older versions, and future updates.

You need:

- Windows PowerShell 5.1, included with supported Windows installations.
- A locally installed or downloaded Space Engineers block mod to scan.
- Working Knowledge and the source block mod when testing the generated layer in game.

The toolkit does not need to be installed or placed inside this repository. Scanning is read-only. Files are written only to the output folder you choose. An internet connection is optional; it is used only to look up friendly Steam Workshop names, with local-name fallbacks when unavailable.

## Create A Layer

Read [QUICKSTART.md](QUICKSTART.md), then run from the unzipped toolkit folder:

```powershell
.\Start.ps1
```

You can instead double-click `Start.bat`. It launches the same script with a process-only execution-policy bypass.

The generator scans public block definitions, lets you choose built-in or custom schematic groups, writes a complete Space Engineers mod folder, and validates it before finishing.

## Edit Or Build A Layer Manually

- [Editing Generated Layers](Docs/editing_generated_layers.md) explains the files most authors may change.
- [Mapping Format](Docs/mapping_format.md) defines block mapping syntax and priority.
- [Schematic Groups](Docs/schematic_groups.md) lists built-in IDs and explains custom groups.
- [Manual Layer Authoring](Docs/manual_layer_authoring.md) starts from the included `ExampleMod`.
- [Troubleshooting](Docs/troubleshooting.md) covers scanning, validation, and in-game problems.

`Data/Template/` and the other files under `Data/` are generator internals. Normal layer authors should not edit them.

## Output

A mapping-only generated layer contains:

```text
WKL-ExampleBlockMod/
  modinfo.sbc
  README.md
  Data/
    ResearchBlocks.sbc
    WorkingKnowledge/
      block_mappings.txt
  Publishing/
    changelog.md
    workshop_description_bbcode.txt
```

When custom groups are defined, the generator also creates `schematic_groups.txt`, `ResearchUnlockerGroups.sbc`, `ResearchUnlockers.sbc`, and `PhysicalItems_ResearchSchematics.sbc`.

Use this normal in-game Active Mods list, shown top to bottom with highest priority first:

1. The generated Working Knowledge layer
2. The source block mod
3. Working Knowledge

After any manual edit, validate the layer:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\WKL-ExampleBlockMod"
```

Pass multiple paths from **lowest to highest priority** to preview conflict winners. The last path receives the highest numeric priority. This argument order is the reverse of reading the normal in-game Active Mods list from top to bottom.

## Publish A Layer

Follow [Publishing A Layer](Docs/publishing_layers.md). Generated `Publishing/` files provide starter Workshop copy and release notes, but the author must still set Workshop requirements for Working Knowledge and the source block mod.

The offline validator checks layer syntax, definitions, wiring, and priority resolution. It cannot prove that a separately loaded source mod still contains every block. A fresh in-game test and `/wk admin audit` are required before publishing.

## Toolkit Contents

- `Start.ps1` and `Start.bat` - interactive generator and launcher.
- `Validate.ps1` - standalone layer and conflict-stack validator.
- `ExampleMod/` - complete copyable manual-authoring example; do not publish it unchanged.
- `Docs/` - authoring, format, publishing, and troubleshooting guides.
- `Tests/Test-LayerResolution.ps1` - automated priority and fallback checks.
- `Tests/Deploy-ConflictTestLayers.ps1` - maintainer-only local conflict-fixture deployment.
- `VERSION.txt` and `CHANGELOG.md` - toolkit release identity and history.

Report toolkit problems on the [project issue tracker](https://github.com/abflett/SE-BeneathTheCrust/issues). Include the exact error, what you selected, and whether the source was a local mod or Workshop item. Do not attach another author's complete mod unless its license permits redistribution.
