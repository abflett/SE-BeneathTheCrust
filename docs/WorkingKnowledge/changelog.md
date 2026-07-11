# Working Knowledge Changelog

Public-facing release notes for the standalone **Working Knowledge** Space Engineers mod.

## 0.12.0 - Layer Diagnostics

Added diagnostics for Working Knowledge compatibility layers without changing the layer format or existing layer behavior.

Admins can use `/wk admin audit` to see whether the runtime loaded correctly, how many layer mods were found, how many mappings became active, and any compatibility issues. The chat report shows the first twelve issues. Full details are also written to `SpaceEngineers.log` and registered as warnings in the F11 mod-error screen.

The audit now identifies:

- Malformed mapping lines and unknown schematic IDs.
- Mappings for missing or non-public blocks.
- Mappings missing their required `ResearchBlocks.sbc` entry.
- Duplicate mappings supplied by multiple layers.
- Layer mappings that conflict with a built-in Working Knowledge mapping.
- Missing Working Knowledge unlocker definitions or research groups.
- Unexpected runtime loading failures.

Existing `block_mappings.txt` files remain compatible. The Working Knowledge Layer Toolkit output does not need to be regenerated.

Added a release validation helper for script compilation, XML parsing, version documentation, and thumbnail size.

Split the large welding-botch implementation into focused partial files for chance and damage settlement, notifications, component recovery, and component valuation. This is a code-organization change only; the gameplay formulas and operation order are unchanged.

## 0.11.0 - External Layer Support

Added support for small external **Working Knowledge Layer** mods that map third-party block definitions into Working Knowledge schematic families.

Layer mods can now include a simple `Data/WorkingKnowledge/block_mappings.txt` file. Working Knowledge scans loaded mods for those mapping files at startup, then uses them to bind supported modded blocks into the normal research catalog, vanilla progression unlocks, Proficiency, construction, repair, grinding, and salvage systems.

This keeps the main Working Knowledge catalog focused while allowing compatibility support to live in small companion layers. Players, server owners, or mod authors can create focused layers for specific block packs instead of editing the large generated Working Knowledge catalog directly.

The first published layer is [Working Knowledge Layer - ARC Truss System](https://steamcommunity.com/sharedfiles/filedetails/?id=3760442026). It maps ARC Truss System structure blocks to Industrial Structure Schematics and its truss light to Interior Lighting Schematics.

Released the [Working Knowledge Layer Toolkit](https://github.com/abflett/SE-BeneathTheCrust/tree/main/tools/WorkingKnowledgeLayerToolkit) to make layer creation easier for players, server owners, and mod authors. The toolkit can scan local or Steam Workshop block mods, find new public block definitions, help assign those blocks to schematic families, and generate a ready-to-test layer mod with `ResearchBlocks.sbc`, `Data/WorkingKnowledge/block_mappings.txt`, README, and publishing starter files.

The ready-to-use toolkit zip is available from the [SE-BeneathTheCrust releases page](https://github.com/abflett/SE-BeneathTheCrust/releases).

## 0.10.0 - HUD Progress Overlay

Added a Text HUD API-backed progress overlay for recent Working Knowledge progress.

Research and Proficiency progress now update in a compact HUD display with up to five recent schematic rows by default. Each row shows the schematic name plus research and Proficiency bars using the same green and blue feedback colors used by the existing chat output. Rows update as progress changes, then fade out after a short idle period.

Players can configure the progress bar overlay through `/wk config`: enable or disable it, choose one to ten visible rows, switch row order, choose top-left/top-right/bottom-left/bottom-right/center presets, add X/Y offsets, and set the fade timing. Setting `progressHudFadeSeconds` to `0` keeps the recent progress history visible until replaced by newer entries.

Delayed research and Proficiency chat progress and popup progress toasts are now disabled by default for players so the HUD overlay carries routine progress feedback without spam. Players can re-enable chat progress with `/wk config progressChatEnabled true` and popup progress toasts with `/wk config progressToastEnabled true` when world settings allow them. The Text HUD progress bars update separately from the popup toast setting.

## 0.9.6 - Completion Toast Fix

Fixed completion HUD toasts so they are no longer muted by `progressToastEnabled` or `defaultProgressToastEnabled`.

Those settings now only control repeated progress HUD notifications and botch toasts. Research completion and Proficiency mastery toasts are immediate milestone feedback and still appear even when a player disables the spammy progress toast stream.

## 0.9.5 - Advanced Welding Compatibility

Added targeted compatibility for the Weld Pad blocks from the recommended companion mod **Advanced Welding**.

Because Advanced Welding is likely to be used alongside Working Knowledge in many campaign-style saves, this became both a useful feature and a good proof of concept for future companion-mod compatibility. Working Knowledge now includes research definitions and catalog mappings for Advanced Welding's large and small weld pads:

- `TerminalBlock/LargeWeldPad`
- `TerminalBlock/SmallWeldPad`

When Advanced Welding is loaded, both weld pad variants are treated as Fundamental Schematics and participate in the normal Working Knowledge unlock, placement, welding, grinding, salvage, and Proficiency paths. When Advanced Welding is not loaded, the compatibility entries are inert because the matching cube block definitions do not exist.

## 0.9.4 - Salvage Scrap Tuning

Reduced the default low-Proficiency grinding scrap payout from a full 1:1 component-mass replacement to 20% of the degraded component mass.

The previous behavior made failed salvage feel too close to a material swap instead of a meaningful loss, which made the mechanic more annoying than useful during longer survival play. Grinding still uses Proficiency to decide how many recovered components remain intact, but components converted to scrap now produce much less scrap ore by default.

This scrap payout is now configurable through the world setting `salvageScrapYield`. The default is `0.20`; admins can use ratio or percent values such as `0.2` or `20%` with `/wk config salvageScrapYield <value>`. A plain value such as `20` is treated as a literal ratio, or 20:1, not as 20%.

## 0.9.3 - Loot Restoration and Data Fragment Tuning

Fixed a remaining loot injection issue that could make unknown signal containers generate no loot, then retuned Data Fragment loot so common fragments are easier to find during normal salvage play.

Working Knowledge patches selected container loot tables at runtime so Data Fragments can appear alongside normal loot. The previous fix preserved the loaded item entries before adding Data Fragments, but the runtime rebuild did not also preserve the container's item roll counts. For unknown signals, those counts decide how many loot entries the container should generate.

As a result, the item table could still exist internally, but the container could be left with zero loot rolls after the patch. In normal play this could make unknown signal cargo appear empty, which was especially painful because those containers are an early and visible part of the salvage loop.

The patch now preserves the loaded container roll counts before rebuilding the definition and clears the temporary set map before Space Engineers rebuilds its private selection cache. Data Fragments are still added idempotently, and vanilla loot entries remain intact. Working Knowledge adds fragments as extra weighted loot possibilities; it does not replace the vanilla entries in those containers.

Data Fragment coverage was also expanded and rebalanced across all tiers. Common Data Fragments are now much more visible in the intended loot loop: unknown signal containers should be roughly a one-in-five source, most normal cargo-style containers sit closer to the one-in-ten to one-in-six range, and higher-value military, treasure, prototech, and advanced salvage pools can be better than that. Uncommon and rare fragments now follow the same container-value curve instead of staying at the old very-low rates. Prototech fragments remain the rarest tier, landing around one-in-twenty in unknown signal and strong loot pools, higher in special rare pools, and lower in weak general loot.

What this means in normal play:

- Unknown signal containers should generate normal loot again.
- Common Data Fragments should be noticeably easier to find.
- Uncommon, rare, and prototech fragment chances should better match the new common-fragment balance.
- Data Fragments can appear in a broader set of cargo-style loot pools.
- Working Knowledge no longer drops the container's own item-count settings while patching loot.
- Vanilla loot entries should remain present alongside Working Knowledge fragments.

Existing already-spawned containers are not retroactively refilled; the fixed definitions affect containers generated after the corrected definitions are loaded.

## 0.9.2 - Internal Component Price Fix

Fixed an internal definition issue reported by BuildInfo's mod checker.

Working Knowledge uses an internal data-fragment component and circular validation blueprints for hidden schematic unlockers. That component did not have a fixed minimum price, so diagnostic tools could warn that Space Engineers might try to calculate the component price through a blueprint that requires and produces the same item.

The internal component now has a fixed minimal price value, which prevents that recursive price lookup path. The component remains unavailable for normal economy trading, player orders, and player offers.

## 0.9.1 - Loot Preservation Fix

Fixed an issue where Working Knowledge could accidentally replace normal container loot while adding Data Fragments to loot tables.

Working Knowledge adds Data Fragments to selected vanilla container types at runtime so players can occasionally find research data in normal loot sources. In `0.9.0`, the patch could rebuild those container definitions from an incomplete copy of the loot data. As a result, some vanilla loot entries, and possibly loot entries added by other mods, could be dropped when Working Knowledge inserted its Data Fragment entries.

The fix changes the patching logic to use the container's already-loaded loot list as the source of truth before adding Working Knowledge fragments. Existing loot entries are now preserved, including their item IDs, amounts, frequencies, set data, and story categories. Working Knowledge then adds or updates only its own Data Fragment entries.

What this means in normal play:

- Vanilla container loot should no longer be overwritten by Working Knowledge's Data Fragment injection.
- Other mods that add loot before Working Knowledge patches the container definitions are less likely to be stomped.
- Data Fragments can still appear in the intended rare loot pools.
- Existing generic loot remains intact alongside Working Knowledge research data.
- Exact Data Schematics remain intended for POIs, missions, scenario rewards, or admin use, not generic random loot.

This is a compatibility and preservation fix, not a progression balance change. Existing spawned containers are not retroactively rebuilt; the fix affects container loot generated after the corrected definitions are loaded.

## 0.9.0 - Initial Public Feedback Release

Working Knowledge is now available as an initial public feedback release.

This first publish introduces the core salvage, research, and Proficiency loop for Space Engineers. Most vanilla public cube blocks are grouped into schematic families, and those families are unlocked through research progress instead of the normal flat progression chain.

Players can learn unknown technology by grinding unfamiliar blocks, recovering Data Fragments from selected loot containers, and completing schematic progress over time. Completed research unlocks the matching block family through Space Engineers' vanilla progression system, so the normal block menu and survival build flow still work as expected.

This release also introduces player-only Proficiency. Research controls whether you know the schematic; Proficiency controls how skilled your engineer is at working with that schematic family. Building, repairing, grinding, and salvaging related blocks can improve Proficiency. Low Proficiency can make salvage less efficient and can cause welding botches that roll back some build progress, damage components, and recover forgiven materials when the loss would otherwise be too punishing.

Included in this release:

- Schematic-family research for most vanilla public cube blocks.
- Vanilla progression integration for locked and unlocked block families.
- Grinding-based research progress from unknown technology.
- Data Fragments as rare research loot.
- Exact Data Schematics for POI, mission, scenario, or admin rewards.
- Separate player research, faction research, and player-only Proficiency tracking.
- Salvage recovery effects based on Proficiency.
- Welding and repair botches for low-Proficiency work.
- Research Pedestal terminal block for viewing and syncing research.
- Selectable LCD apps for Research, Proficiency, Identity, and Calibration.
- Chat commands for player summaries, feedback settings, difficulty display, and admin tuning.
- Difficulty presets and configurable world settings for research, Proficiency, salvage, botches, and feedback.

Recommended setup:

- Best tested in a fresh survival world.
- Experimental Mode is not required.
- The mod enables vanilla progression at runtime so Working Knowledge can manage schematic locks.
- Creative worlds and admin creative-mode testing can bypass schematic placement restrictions.
- Other progression or research-overhaul mods are likely to conflict and should be tested carefully.

This is a playable standalone release, but it is still a `0.9.x` feedback build. Balance, multiplayer edge cases, dedicated-server behavior, and compatibility with larger mod stacks are still being tested. Bug reports and balance feedback from fresh survival playthroughs are especially useful.
