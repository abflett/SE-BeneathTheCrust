# Working Knowledge 0.13.0 Test Plan

Working Knowledge `0.13.0` must not be published to the Steam Workshop until this checklist passes in Space Engineers. Compile, XML validation, offline layer resolution, and local deployment are necessary but do not replace this game test.

## Prepare

From the repository root:

```powershell
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 0.13.0
.\build.ps1 -ModName WkKn
.\tools\WorkingKnowledgeLayerToolkit\Tests\Deploy-ConflictTestLayers.ps1
```

Create a fresh survival test world with progression enabled. Use a disposable world because pre-1.0 development assumes fresh saves.

## Baseline

Load only Working Knowledge.

- `/wk admin audit` reports no unresolved layer warnings.
- Large and small heavy armor blocks belong to Heavy Armor Schematics.
- Grinding unknown heavy armor grants Heavy Armor research and Proficiency.
- Save/reload preserves progress.

## One Override Layer

Load in this order:

1. Working Knowledge
2. WKL Test - Hard Armor

Expected:

- `/wk admin audit` reports two built-in block assignments replaced by `test.armor.hard` as notices, not F11 warnings.
- Large and small heavy armor blocks use Hard Armor Schematics.
- Other heavy armor shapes remain in Heavy Armor Schematics.
- Grinding either reassigned armor block grants Hard Armor progress.
- Hard Armor appears in research, Proficiency, Research Pedestal, LCD apps, HUD progress, and admin schematic commands.

## Conflicting Layers

Load in this order:

1. Working Knowledge
2. WKL Test - Hard Armor
3. WKL Test - Dense Armor

Expected:

- Dense Armor wins both heavy armor block assignments.
- `/wk admin audit` shows both assignment histories, Dense Armor as winner, and two resolved multi-layer conflicts.
- The conflicts appear as F11 warnings, but do not prevent the winning group from working.

Reverse only the two test layers:

1. Working Knowledge
2. WKL Test - Dense Armor
3. WKL Test - Hard Armor

Expected:

- Hard Armor now wins.
- The audit load positions and histories match the world mod order. If they do not, correct the runtime interpretation of `MyAPIGateway.Session.Mods` before release.

## Data Fragments And Exact Schematics

With Hard Armor winning:

- An Uncommon Data Fragment can select incomplete Hard Armor research.
- Hard Armor does not appear in Common, Rare, or Prototech fragment selection.
- `WkKnSchematic_test_armor_hard` completes Hard Armor research and is returned after use.

With Dense Armor winning:

- A Rare Data Fragment can select incomplete Dense Armor research.
- `WkKnSchematic_test_armor_dense` completes Dense Armor research and is returned after use.

- No inactive or empty displaced group is selected by a fragment.
- Consuming an already-completed exact schematic reports that the schematic is known and returns the item.

## Save And Multiplayer

- Earn partial research and Proficiency in the winning group, save, reload, and confirm both values.
- Complete the winning group, save, reload, and confirm its blocks remain unlocked.
- In a hosted multiplayer session, confirm the server and client show the same winner, research name, tier, and progress.
- Confirm faction research sync and player-only Proficiency behave normally for the custom group.

## Existing Layer Compatibility

Load Working Knowledge with the existing ARC Truss mapping-only layer.

- The layer activates without regeneration.
- Its truss mappings still work.
- `/wk admin audit` does not require `override` syntax.

## Publication Gate

After every section passes:

```powershell
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 0.13.0
.\build.ps1 -ModName WkKn
```

Record the completed game-test result in the changelog or release notes before publishing `0.13.0` to the Workshop.
