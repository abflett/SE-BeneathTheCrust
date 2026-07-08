# Working Knowledge Roadmap

This roadmap is the planning source for Working Knowledge on the path to a stable `1.0.0` release. Working Knowledge should remain a standalone mod until `1.0.0`; campaign composition, companion mod packs, richer story content, and deeper terminal trade systems are later work.

## Versioning

Working Knowledge uses a `major.feature.fix` version pattern during pre-`1.0.0` development.

- Fix-only releases stay in the current feature line, such as `0.9.6`.
- New feature releases advance the middle number, such as `0.10.0` and `0.11.0`.
- `1.0.0` is the planned full release after the main feedback-driven features are in place and the standalone mod is stable.

## Pre-1.0.0 Track: Features, Fixes, And Tests

The pre-`1.0.0` feedback track is about making Working Knowledge comfortable in normal survival play and predictable in modded saves.

Feature backlog:

- Improve progress feedback so research and Proficiency updates do not rely on chat spam.
- Add a safer way to map custom block mods into Working Knowledge schematic groups.
- Keep player-facing config simple enough that ordinary players do not need to edit config to enjoy the mod.
- Keep advanced config useful for server owners, modpacks, and balance tuning.

Fix and polish backlog:

- Continue watching for feedback, toast, and progress reporting issues.
- Confirm player config can reduce or disable progress and info message spam.
- Improve config documentation where needed.
- Keep targeted compatibility fixes small and documented.

Compatibility and test backlog:

- Verify how uncataloged modded blocks behave today.
- Confirm modded blocks do not unintentionally become locked, blocked, or partially restricted.
- Document modded block behavior clearly so players know whether configuration is required.
- Keep the current hidden-unlocker tradeoff documented: hidden unlockers may appear in G-menu search for "Schematics", but are not survival-buildable.
- Run focused smoke tests after gameplay changes and broader validation before version bumps.

## Plans For Each Version

The planned sequence before `1.0.0` is:

- `0.10.0` - better progress display.
- `0.11.0` - custom block mod integration.
- `1.0.0` - stable standalone release after the main feature and validation gates are satisfied.

## 0.10.0 Progress Display

Goal: make research and Proficiency progress readable without relying on spammy chat updates.

Direction:

- Replace or supplement chat progress messages with a cleaner HUD-style display.
- Prototype recent progress bars for discovery, research, or Proficiency updates.
- Show only the most recent few progress entries.
- Fade progress bars out after a few seconds with no new updates.
- Keep chat messages available as a fallback or optional mode.
- Confirm existing config still lets players reduce or disable progress and info message spam.
- Improve config documentation where needed.

Technical questions:

- Can this be done cleanly with vanilla HUD notification tools?
- Is a lightweight custom draw approach viable inside the mod script environment?
- Would Rich HUD or a similar framework provide enough value to justify an optional or required dependency?
- Can the HUD display remain readable with common HUD mods and modpack UI changes?

## 0.11.0 Custom Block Mod Integration

Goal: make Working Knowledge easier to use with block mods instead of depending mostly on hard-coded catalog and generator mappings.

Current behavior to verify and document:

- Test how normal modded blocks behave with Working Knowledge.
- Confirm whether uncataloged modded blocks are currently buildable as already researched and full Proficiency.
- Verify that block mods do not unintentionally become locked, blocked, or partially restricted by the progression system.
- Document current behavior clearly so players know whether configuration is required.

Feature direction:

- Add a way for admins or modpack authors to assign custom modded blocks into Working Knowledge schematic groups.
- Allow modded blocks to become part of the normal research and Proficiency system instead of always acting as already unlocked.
- Consider config-driven definitions for custom block groups.
- Make the system safe for servers and modpacks where many block mods are installed.
- Avoid requiring ordinary players to edit config just to use common block mods.

## 1.0.0 Release Checklist

Goal: make Working Knowledge safe to recommend as a stable standalone progression mod.

Feature readiness:

- `0.10.0` progress display work is complete or intentionally deferred.
- `0.11.0` custom block mod integration work is complete or intentionally deferred.
- Player-facing feedback defaults are comfortable for normal survival play.
- Modded block behavior is documented clearly.

Gameplay validation:

- Complete one real fresh survival playthrough with the intended `medium` config.
- Tune research gain if schematic unlocks feel too fast or too grindy.
- Tune Proficiency gain if early skill growth or long-term mastery feels off.
- Tune botch chance and botch damage if low skill feels toothless or frustrating.
- Tune salvage recovery if scrap pressure feels wrong.
- Confirm Research Pedestal and LCD apps remain useful during normal play.

Technical validation:

- Validate save/reload behavior across a normal play session.
- Run a focused multiplayer or hosted-session pass for shared research, faction sync, and nearby-player attribution.
- Run a dedicated-server validation pass or document any remaining known gap.
- Recheck public docs for version-specific or maintainer-only wording.
- Bump `mods/WorkingKnowledge/modinfo.sbc` to `1.0.0` when the above gates are satisfied.

Release validation commands:

```powershell
.\tools\generate-working-knowledge-data.ps1
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\build.ps1 -ModName WkKn
```

Release verification:

- Generated data changes are expected.
- All Working Knowledge `.sbc` files parse as XML.
- The deployed local folder is `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`.
- The deployed `modinfo.sbc` has the intended version.
- `thumb.jpg` exists in the deployed mod and stays under 1 MB.
- `.tmp/` is empty before committing release prep.

Smoke test matrix:

- G-menu opens and locked block families are locked.
- Grinding unknown blocks grants research and Proficiency.
- Completed schematic families unlock their blocks.
- Data fragments grant compatible research or refund cleanly when disabled.
- Proficiency affects salvage recovery.
- Low-Proficiency welding can botch, rolls back expected integrity, and reports lost/recovered components.
- Botch sound plays once per botch and does not loop.
- Ship welder attribution works in common active-welder cases.
- Locked player-built blocks are removed unless the player knows the schematic.
- Creative/admin bypasses behave as documented.
- Research Pedestal player/faction views and sync work.
- Research, Proficiency, Identity, and Calibrator LCD apps render correctly.
- Save/reload preserves research, Proficiency, world config, and player feedback config.
- A shorter smoke test also works when vanilla progression is already enabled before load.

## Post-1.0.0 Features And Ideas

Goal: expand carefully after the standalone progression layer is stable.

Fallback attribution for other build and repair paths:

- Investigate compatibility with automation mods such as Nanobot Build and Repair System.
- Determine whether automated welding, grinding, repairing, or building actions expose a player owner, builder, controller, or usable attribution source.
- Check whether Working Knowledge can safely grant research and Proficiency from automated actions.
- Add a fallback attribution option if possible.
- If reliable attribution is not possible, document the limitation clearly.

Other ideas:

- Multiplayer edge-case attribution testing with several players near the same weld target.
- Richer Research Terminal schematic trade and sync workflows.
- Optional companion mod recommendations after compatibility testing.
- Deeper compatibility testing with the future campaign companion stack.
- Localization and clearer player-facing help text.
- Save/config migration support if public worlds need long-term compatibility.
