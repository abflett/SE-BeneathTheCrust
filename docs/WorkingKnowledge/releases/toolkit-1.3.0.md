Create compatibility layer mods for **Working Knowledge**.

The Working Knowledge Layer Toolkit helps Space Engineers players and mod authors connect custom block mods to the Working Knowledge progression system. It scans installed block mods, lets you organize their blocks into schematic groups, and generates a complete layer mod ready for validation and in-game testing.

## What's New In 1.3.0

- Use all **58 active Working Knowledge schematic families**, including the new **Prototech Microwave Emitter** family shared by large- and small-grid emitters.
- Match the Working Knowledge 1.2.x catalog for **Space Engineers 1.211 ? Warfare 3**, while retaining Industrial Access and all existing stable family IDs.
- Generate custom Data Schematics **without the unintended radiation immunity effect**. Consumable type, use sound, and durable research behavior are preserved.
- Keep generated family unlockers compatible with Working Knowledge 1.2.0's runtime G-menu hiding behavior.
- Get a validated standalone archive with matching family names and tiers, generator self-tests, example validation, and layer-priority checks.

- Includes an explicit **MIT license** for the standalone toolkit.

## What It Does

- Scans local or Steam Workshop Space Engineers mods.
- Finds public block definitions while skipping hidden support blocks.
- Identifies new blocks and blocks already covered by Working Knowledge.
- Looks up friendly Steam Workshop names when possible, with offline fallbacks.
- Lets you assign a default schematic group and change individual outliers.
- Supports the complete built-in group catalog and stable, namespaced custom groups.
- Generates:
  - `modinfo.sbc`
  - `Data/ResearchBlocks.sbc`
  - `Data/WorkingKnowledge/block_mappings.txt`
  - Custom-group metadata and game definitions when required
  - README, changelog, and Workshop-description starter files
- Includes a standalone validator, complete example layer, format references, troubleshooting guide, and publishing checklist.

## Intended Use

Use this toolkit when you want blocks from another mod to participate in:

- Working Knowledge research gates and schematic unlocks
- Proficiency progression
- Data Fragment and exact Data Schematic rewards
- Welding and repair restrictions
- Grinding and salvage behavior
- Working Knowledge HUD, LCD, Pedestal, and administrative audit displays

## Quick Start

Download and unzip `WorkingKnowledgeLayerToolkit-1.3.0.zip`, then run:

```powershell
.\Start.ps1
```

Or double-click:

```text
Start.bat
```

The toolkit does not need to be installed or placed inside the Beneath the Crust repository. Windows PowerShell 5.1 is required. An internet connection is optional and is used only for Steam Workshop name lookups.

## Mod Priority

Use this normal in-game **Active Mods** order, shown top to bottom with highest priority first:

1. The generated Working Knowledge layer
2. The source block mod
3. Working Knowledge

When two layers map the same block, place the layer you want to win higher in the normal Active Mods list.

## Validate And Test

After editing a generated layer, run from the toolkit folder:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\YourLayer"
```

Before publishing a layer, test it in a fresh world and run:

```text
/wk admin audit
```

The offline validator checks layer files, custom-group wiring, and predicted priority winners. The in-game audit confirms the actual loaded blocks, definitions, and mod order.

## Important For Published Layers

Custom schematic group IDs and generated definition subtypes are persistent compatibility contracts. Choose stable, namespaced IDs and do not rename them after publishing.

Workshop layer authors should set both **Working Knowledge** and the supported source block mod as requirements.

