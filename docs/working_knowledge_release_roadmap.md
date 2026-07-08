# Working Knowledge Roadmap

This roadmap tracks the planned path from the current public-feedback releases to a stable `1.0.0` Working Knowledge release. It combines release targets, feature candidates, compatibility work, and the validation checklist used before publishing.

Working Knowledge should stay standalone until `1.0.0`. Campaign integration, companion mod packs, richer story content, and deeper terminal trade systems are later work.

## Versioning

Working Knowledge uses a `major.feature.fix` version pattern during pre-`1.0.0` development.

- Fix-only releases stay in the current feature line, such as `0.9.6`.
- The next feature release after `0.9.x` should become `0.10.0`.
- `1.0.0` is the planned full release after the main feedback-driven features are added and the standalone mod is stable.

## Current Track: 0.9.x Bugfix And Compatibility Testing

Goal: keep the first public Workshop release healthy while gathering feedback from normal play and common mod stacks.

### Feedback And Message Polish

- Continue testing recent feedback, toast, and progress reporting fixes in normal play.
- Review chat message frequency during discovery, research, and Proficiency updates.
- Confirm that existing config options properly reduce or disable progress and info message spam.
- Improve config documentation where needed.
- Run a light in-game smoke test after each gameplay fix.

### Modded Block Compatibility

- Test how normal modded blocks behave with Working Knowledge.
- Confirm whether modded blocks are currently buildable as already researched and full Proficiency.
- Verify that block mods do not unintentionally become locked, blocked, or partially restricted by the progression system.
- Track compatibility reports from mods that change progression, placement, grinding, welding, or salvage.
- Document current behavior clearly so players know whether configuration is required.

### 0.9.x Known Tradeoffs

- Hidden unlockers may appear in G-menu search for "Schematics".
- Hidden unlockers are not survival-buildable because their requirements are unavailable.
- In creative, hidden unlockers can be placed as small colorable datapad-like blocks.
- The default `medium` balance is considered close enough for public feedback.
- Broad dedicated-server validation remains a known gap until it has been tested directly.

## Next Feature Track: 0.10.0 Feedback-Driven Update

Goal: add the next substantial quality-of-life and compatibility features before the `1.0.0` stabilization push.

### HUD Progress Display

- Investigate replacing or supplementing chat progress messages with a cleaner HUD-style display.
- Prototype recent progress bars for discovery, research, or Proficiency updates.
- Show only the most recent few progress entries if possible.
- Have progress bars fade out after a few seconds of no updates.
- Keep chat messages available as a fallback or optional mode.
- Evaluate whether this can be done without heavy dependencies.
- Investigate frameworks such as Rich HUD, but avoid adding required dependencies unless the benefit is worth it.

### Custom Modded Block Integration

- Add a way for admins or players to assign custom modded blocks into Working Knowledge schematic groups.
- Allow modded blocks to become part of the normal research and Proficiency system instead of always acting as already unlocked.
- Consider config-driven definitions for custom block groups.
- Make the system safe for servers and modpacks where many block mods are installed.

### Automation Mod Attribution

- Investigate compatibility with automation mods such as Nanobot Build and Repair System.
- Determine whether automated welding and grinding actions expose a player owner, builder, controller, or usable attribution source.
- Check whether Working Knowledge can safely grant research and Proficiency from automated actions.
- Add a fallback attribution option if possible.
- If reliable attribution is not possible, document the limitation clearly.

## Target Release: 1.0.0 Stable

Goal: make Working Knowledge safe to recommend as a stable standalone progression mod.

- Complete one real fresh survival playthrough with the intended `medium` config.
- Tune research gain if schematic unlocks feel too fast or too grindy.
- Tune Proficiency gain if early skill growth or long-term mastery feels off.
- Tune botch chance and botch damage if low skill feels toothless or frustrating.
- Tune salvage recovery if scrap pressure feels wrong.
- Validate save/reload behavior across a normal play session.
- Confirm Research Pedestal and LCD apps remain useful during normal play.
- Run a focused multiplayer or hosted-session pass for shared research, faction sync, and nearby-player attribution.
- Recheck public docs for version-specific or maintainer-only wording.
- Bump `mods/WorkingKnowledge/modinfo.sbc` to `1.0.0` when the above gates are satisfied.

## General Design Goals

- Keep Working Knowledge usable without requiring players to edit config.
- Use config mainly for tuning, server balancing, and advanced customization.
- Avoid hard dependencies on companion mods unless absolutely necessary.
- Prioritize compatibility with common modpacks.
- Continue using player feedback to guide quality-of-life improvements.

## Post-1.0 Candidates

Goal: expand carefully after the standalone progression layer is stable.

- Dedicated server validation.
- Multiplayer edge-case attribution testing with several players near the same weld target.
- Richer Research Terminal schematic trade and sync workflows.
- Optional companion mod recommendations after compatibility testing.
- Deeper compatibility testing with the future campaign companion stack.
- Localization and clearer player-facing help text.
- Save/config migration support if public worlds need long-term compatibility.

## Release Validation

Run this before publishing a Workshop update or tagging a release:

```powershell
.\tools\generate-working-knowledge-data.ps1
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\build.ps1 -ModName WkKn
```

Then verify:

- Generated data changes are expected.
- All Working Knowledge `.sbc` files parse as XML.
- The deployed local folder is `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`.
- The deployed `modinfo.sbc` has the intended version.
- `thumb.jpg` exists in the deployed mod and stays under 1 MB.
- `.tmp/` is empty before committing release prep.

## Smoke Test Matrix

Use a fresh survival world with Experimental Mode off unless the test says otherwise.

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
