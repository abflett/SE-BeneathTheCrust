# Working Knowledge 0.14.0 Test Plan

Working Knowledge `0.14.0` must pass this focused Space Engineers 1.210 test before it is published to the Steam Workshop.

## Prepare The Local Build

From the repository root:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 0.14.0
.\build.ps1 -ModName WkKn
```

Use a fresh survival test world with Working Knowledge enabled. Enable the Prosperity Pack when checking its DLC blocks. Keep admin creative tools off while checking placement locks.

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
- Record any failures before publishing `0.14.0` to the Workshop.
