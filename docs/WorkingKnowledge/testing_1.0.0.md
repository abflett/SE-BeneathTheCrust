# Working Knowledge 1.0.0 Test Plan

Working Knowledge `1.0.0` must pass this final release-candidate test before it is published to the Steam Workshop.

## Prepare The Local Build

From the repository root:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.0.0
.\build.ps1 -ModName WkKn
```

Use a fresh survival test world with Working Knowledge enabled. Enable the Prosperity Pack when checking its DLC blocks. Keep admin creative tools off while checking placement locks.

Also keep one existing `0.13.0` Workshop save available for the upgrade check.

## Existing Save Upgrade

- Back up an existing `0.13.0` save before opening it with the candidate.
- Confirm personal and faction research, player Proficiency, world configuration, and player feedback settings load unchanged.
- Confirm previously completed schematic families remain unlocked.
- Confirm the new Prototech O2/H2 Generator family appears with clean new progress rather than affecting another Prototech family.
- Save, reload, and confirm the upgraded world remains stable.

## Definition And Menu Smoke Test

- Open the G-menu and confirm there are no definition errors.
- Run `/wk admin audit` and confirm the base catalog loads without missing research groups, unlockers, or research-block definitions.
- Confirm the Working Knowledge category, progression tab, Research Pedestal, and research LCD still open normally.
- Search for the new blocks and confirm each appears under its intended schematic family.

## New Block Families

Check at least one locked block from every affected family:

- Control Station Schematics: Sloped Cockpit.
- Battery Schematics: Battery Bank.
- Gas Storage Schematics: Bulky Hydrogen Tank.
- Cargo Transfer Schematics: Flat Collector.
- Industrial Structure Schematics: Structural Platform Conveyor, Short Stairs, Grated Catwalk Triangle, Steep Stairs, Solid Stairs, and Square Railing.
- Passage Schematics: Passage 2 Frame.
- Decorative Fixture Schematics: Server Rack and Cable Run.
- Interior Lighting Schematics: Round Light and Square Light.
- Prototech O2/H2 Generator Schematics: Prototech O2/H2 Generator.

For each sampled family:

- Confirm an unknown block cannot be placed by a normal survival player.
- Grind an existing block and confirm research and Proficiency use the intended family.
- Unlock the family through an admin research command or normal research.
- Confirm its blocks can then be placed, welded, ground, and tracked for Proficiency.

## Variant-Set Checks

- Cycle the Structural Platform variants and confirm the platform, conveyor, and connector remain together.
- Cycle all Square Railing variants, including Square Railing Entry, without crossing schematic families.
- Cycle Server Racks A, B, and C and confirm all three use Decorative Fixture Schematics.
- Cycle all six Steep Stairs variants and confirm all use Industrial Structure Schematics.
- Confirm the Prototech O2/H2 Generator has its own radial item and does not unlock or cycle into other Prototech families.

## Persistence And Completion

- Save and reload with partial Prototech O2/H2 Generator research and Proficiency; confirm both values persist.
- Complete the new schematic, save, reload, and confirm the generator remains unlocked.
- Consume its exact Data Schematic and confirm the durable item is returned after use.
- Confirm common, uncommon, rare, and existing Prototech fragment rewards still select valid active families.

## Core 1.0 Survival Loop

- In a fresh survival world, obtain research by grinding an unknown block.
- Consume a Data Fragment and confirm it grants valid progress or refunds cleanly when no target is available.
- Complete a schematic and confirm its blocks unlock.
- Gain Proficiency through welding and grinding.
- Confirm low-Proficiency salvage and welding botches still behave according to the selected difficulty.
- Confirm the Research Pedestal and Research, Proficiency, Identity, and Calibrator LCD apps render normally.
- Save and reload, then confirm research, Proficiency, configuration, and unlock state persist.

## Multiplayer And Server Check

- Run one hosted multiplayer session and confirm faction research sharing and nearby-player work attribution.
- Start a dedicated server with the candidate if available, then confirm startup, join, save, and reload.
- If dedicated-server testing is unavailable, record that as the remaining known validation gap rather than silently treating it as passed.

## Release Decision

- Run `/wk admin audit` once more and check F11 and `SpaceEngineers.log` for definition or compatibility errors.
- Record any failures before publishing `1.0.0` to the Workshop.
- If the test passes, keep the version at `1.0.0`, finalize the release notes, and upload the deployed local mod through Space Engineers.
