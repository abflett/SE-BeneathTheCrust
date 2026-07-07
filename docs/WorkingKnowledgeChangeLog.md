# Working Knowledge Changelog

Public-facing release notes for the standalone **Working Knowledge** Space Engineers mod.

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
