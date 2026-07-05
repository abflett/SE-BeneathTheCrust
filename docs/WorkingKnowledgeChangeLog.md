# Working Knowledge Changelog

Public-facing release notes for the standalone **Working Knowledge** Space Engineers mod.

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
