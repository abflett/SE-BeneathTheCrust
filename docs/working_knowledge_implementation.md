# Working Knowledge Implementation Notes

This is the practical maintainer reference for `mods/WorkingKnowledge`. It keeps the file map, runtime shape, and the few Space Engineers edge cases that are easy to break accidentally.

For admin-facing settings, use [Working Knowledge configuration](working_knowledge_configuration.md). For release status, use [Working Knowledge release roadmap](working_knowledge_release_roadmap.md). For public Workshop copy, use [Working Knowledge Workshop page](working_knowledge_workshop_page.md).

## Load Rules

- The source mod root is `mods/WorkingKnowledge`.
- The local test deploy folder is `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`.
- Space Engineers loads definitions and scripts from `Data/`.
- C# scripts live under `Data/Scripts/WorkingKnowledge/`.
- The runtime entry point is a `MySessionComponentBase` session component with `MySessionComponentDescriptor`.
- `.sbc` files use XML under a `<Definitions>` root. Asset paths such as `Models\...` and `Textures\...` resolve from the mod root.
- Experimental Mode is not required.

After C# changes, run:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
```

Use the root build script for local testing:

```powershell
.\build.ps1 -ModName WkKn
```

## Publishing Metadata

Keep these root-level files tracked in `mods/WorkingKnowledge`:

- `metadata.mod`
- `modinfo.sbmi`

Space Engineers uses them to associate local publishes with the existing Workshop item. Without them, publishing can create a new Workshop item instead of updating the current one.

`build.ps1` copies those files back from `%APPDATA%\SpaceEngineers\Mods\Working Knowledge` into `mods/WorkingKnowledge` before cleaning and redeploying. This preserves any metadata changes Space Engineers writes during publishing.

## Progression Strategy

Working Knowledge uses generated vanilla research definitions plus a session component.

- Vanilla progression owns G-menu lock state.
- The session forces vanilla progression on at runtime so it can own schematic locks even if the world was created with progression disabled.
- Public cube blocks map to generated schematic-family hidden unlockers.
- Completed custom research mirrors into vanilla research.
- Player research, faction research, and player-only Proficiency are stored separately.

Retired approaches:

- Do not return to direct `Public=false` block visibility gating; it caused G-menu and cube-builder instability.
- Do not return to oversized static lock groups; they caused progression-enabled test-world crashes.
- Keep `Blueprints.sbc` limited to validation needs for the internal data-fragment component unless research data items intentionally become craftable.

Accepted tradeoff:

- Hidden unlockers can appear in G-menu search for "Schematics". They are not normally buildable in survival because their requirements are unavailable. In creative they are small colorable datapad-like blocks. Leave this alone unless a safe visibility flag is proven not to break vanilla progression.

## Data Files

Generated progression data:

- `ResearchBlocks.sbc` - generated research block entries for vanilla cube blocks.
- `ResearchGroups.sbc` - generated vanilla research group overrides, with group `0` kept as a crash guard.
- `ResearchUnlockerGroups.sbc` - hidden unlocker research groups, one per schematic family.
- `ResearchUnlockers.sbc` - hidden synthetic datapad-style blocks used as vanilla research unlock triggers.
- `ResearchBlocks_ResearchTerminals.sbc` - progression gate for the Research Pedestal and Research Sci-Fi Terminal.
- `Scripts/WorkingKnowledge/Application/Research/Catalog/ResearchCatalog.generated.cs` - generated block-to-schematic catalog shared by runtime code and generated `.sbc` files.

Generated support data:

- `BlockVariantGroups_Research.sbc` - generated variant-cluster overrides that keep vanilla `+` cycling aligned with schematic families.
- `RadialMenu_Research.sbc` - radial-menu override that keeps controller/radial pages aligned with split variant clusters.
- `BlockCategories_Research.sbc` - Working Knowledge G-menu category plus small vanilla tool-category ordering overrides.

Hand-authored gameplay data:

- `Components.sbc` - internal data-fragment component used by schematic unlock marker blocks.
- `Blueprints.sbc` - internal circular data-fragment component blueprint used for Space Engineers validation.
- `PhysicalItems.sbc` - common, uncommon, rare, and prototech data fragments.
- `PhysicalItems_ResearchSchematics.sbc` - generated durable exact data schematics, one per schematic family.
- `Fonts.sbc` - numbered chat font palette used by Working Knowledge feedback.
- `Audio.sbc` - HUD audio definitions for research completion and Proficiency mastery.
- `CubeBlocks_ResearchPedestal.sbc` - large-grid Research Pedestal terminal block.
- `CubeBlocks_ResearchSciFiTerminal.sbc` - Research Sci-Fi Terminal block using the copied terminal model and LCD support.

Balance source:

- `Scripts/WorkingKnowledge/Application/Balance/SchematicWorkRewardTable.cs` is the live source-code reward table.
- `tools/generate-working-knowledge-balance.ps1 -UpdateSource` refreshes that table from vanilla blueprint, component, cube-block, and generated catalog data.
- Generated balance audits live under `docs/generated/`; treat them as reports, not hand-maintained design docs.

## Runtime Shape

The session partials are split by responsibility:

- Lifecycle, save/load, multiplayer messages, and local feedback routing.
- Research progress, faction/player archives, research data consumables, and vanilla research mirroring.
- Proficiency persistence, gain, notifications, and display sync.
- Grinding, salvage, scrap recovery, and direct grinder attribution.
- Welding, repair, hand/ship welder attribution, botches, component loss, and forgiveness.
- Locked block placement enforcement.
- Research Pedestal controls, lists, and sync.
- LCD app registration, Custom Data parsing, settings, and rendering.
- `/wk` commands, difficulty presets, and config access.

The internal script namespace, session class, world-storage filenames, LCD Custom Data sections, and generated game definition IDs use the short `WkKn` prefix.

## Research Data Loot

Working Knowledge appends tiered data fragments to selected vanilla container type definitions at runtime. This replaces the retired static `ContainerTypes.sbc` override so vanilla and other mod loot definitions keep their own entries.

Current behavior:

- Only data fragments are added to generic random loot.
- Exact data schematics are definition items for POI, mission, scenario, or admin use, not generic loot.
- Fragment entries are added idempotently during `LoadData`; if another source already added the same fragment item to a container type, Working Knowledge does not duplicate it.
- Loot generation still happens through Space Engineers' normal container type flow after the definitions are patched.

## Integrity Monitor

`IMyCubeGrid.OnBlockIntegrityChanged` only provides the changed `IMySlimBlock`; it does not provide the old state, cause, or player attribution. `BlockIntegrityMonitor` owns the raw bookkeeping needed to turn that event into one gameplay signal.

Monitor responsibilities:

- Track existing grids and newly discovered grids.
- Subscribe to `OnBlockIntegrityChanged`, `OnBlockAdded`, `OnBlockRemoved`, `OnGridSplit`, and `OnGridMerge`.
- Use full-grid scans only for newly tracked grids and topology changes.
- Keep one latest known snapshot per incomplete or damaged block, grouped by grid.
- Emit `PositiveIntegrityChanged` only when `Current.Integrity - Previous.Integrity` is positive.

Important details:

- Many slim blocks, especially armor, have no stable entity id. Block identity is grid entity id plus local block coordinate.
- After the initial grid scan, block-level events are treated as authoritative. Do not reintroduce interval full-grid scans unless events prove insufficient.
- Negative integrity movement, including botch damage, is bookkeeping only and must not trigger welding/proficiency/botch work.
- Consumers receive immutable previous/current snapshots. Do not expose mutable monitor records to gameplay code.
- The live `IMySlimBlock` stays in the event payload because botch damage, inventory settlement, catalog lookup, and sound position need it.

## Welding And Botches

Welding and repair both use the same positive-integrity path:

```text
BlockIntegrityMonitor.PositiveIntegrityChanged
  -> resolve schematic target
  -> resolve active hand welder or ship welder context
  -> award Proficiency
  -> maybe apply botch damage
```

Botch rules to preserve:

- Botches use the triggering positive `Integrity` delta, not `BuildIntegrity` delta.
- Build and repair can both botch.
- Pre-functional work uses the ramp below the block's functional threshold.
- Post-functional work uses the botch-zone curve between the functional threshold and the current Proficiency cap.
- Build-cap pressure can force `100%` botch chance at or beyond the computed Proficiency cap.

Damage settlement is the fragile part:

- Target loss is raw integrity loss.
- `DoDamage` input is back-solved through Space Engineers' build-state damage ratio:

```text
requestedDamage = targetRawLoss / (2 - BuildIntegrity / MaxIntegrity)
```

- Call `ApplyAccumulatedDamage(true)` after `DoDamage` so botch damage resolves inside the botch handler instead of lagging behind the next weld event.
- After same-handler botch damage, call `BlockIntegrityMonitor.RefreshBlockSnapshot` for that block. Space Engineers does not reliably emit another integrity callback for this internal damage.
- If future code mutates block integrity internally, refresh the monitor snapshot the same way.

Component loss and forgiveness:

- Snapshot mounted component counts before and after botch damage using `ComponentStack.GetComponentStackInfo(index).MountedCount`.
- Report actual whole component losses from the before/after stack difference.
- Forgiveness runs after game settlement. The block keeps the resolved damage, but excess material loss above a steel-plate-equivalent permanent-loss budget is returned.
- Recovery first tries the attributed inventory, then lets the slim block pull once from that inventory to construction stockpile to free space, retries inventory return, and finally drops remaining items near the block.
- Recovery priority uses component raw ingot cost, weighted material value, and component integrity so fragile expensive components are returned before low-value structural components.

## Storage

- Research state saves to `WkKnResearch.xml`.
- Proficiency state saves to `WkKnProficiency.xml`.
- World config saves to `WkKnConfig.xml`.
- Player feedback preferences save to `WkKnPlayerConfig.xml`.
- `modinfo.sbc` is metadata only.

## Validation Checklist

Use this small local check before committing gameplay changes:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\build.ps1 -ModName WkKn
```

For generated data or release prep, use the longer validation list in [Working Knowledge release roadmap](working_knowledge_release_roadmap.md).
