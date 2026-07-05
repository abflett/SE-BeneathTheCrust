# Working Knowledge Release Readiness Checklist

This is the maintainer checklist for getting Working Knowledge from the current pre-release build to a public feedback release, then toward a stable `1.0.0`. It is separate from [Working Knowledge Workshop page draft](working_knowledge_workshop_page.md) so tuning notes and release decisions do not bloat the public Workshop copy.

## Release Position

Use staged releases instead of treating the first public Workshop upload as final.

- Current checkpoint: `0.8.7`, close to the intended public test shape.
- Public feedback target: `0.9.0`, published once command polish and light smoke testing are done.
- Stable target: `1.0.0`, after public feedback, a real survival playthrough, and any lingering bug fixes.
- Keep Working Knowledge standalone; campaign integration, external mod packs, richer story content, and deeper terminal trade systems are later work.
- Prefer seeding this public repo from the current working tree as one clean initial commit instead of importing old development history.

## Finish Before 0.9.0 Public Feedback

- [x] Bump `mods/WorkingKnowledge/modinfo.sbc` to `0.8.7` as the current checkpoint.
- [x] Polish player/admin commands and command feedback.
- [x] Add admin command support to set a player's research progress for a schematic family.
- [x] Add admin command support to complete or revoke a player's research for a schematic family.
- [x] Add admin command support to set a player's Proficiency progress for a schematic family.
- [x] Add clear help text and examples for those admin commands.
- [ ] Confirm admin-only command gating and feedback messages are clean.
- [x] Add any final config hooks needed by command polish.
- [ ] Run light smoke tests around research, Proficiency, botches, salvage, placement enforcement, save/reload, and local feedback.
- [ ] Bump `mods/WorkingKnowledge/modinfo.sbc` to `0.9.0` for the public feedback release.
- [ ] Run the technical validation, in-game smoke tests, and public release prep sections below.

Implemented command shape to test:

```text
/wk research show <player>
/wk research reset <server|player>
/wk research unlock <player> <schematic>
/wk research forget <player> <schematic>
/wk research set <player> <schematic> <percent>
/wk proficiency show <player>
/wk proficiency reset <server|player>
/wk proficiency master <player> <schematic>
/wk proficiency forget <player> <schematic>
/wk proficiency set <player> <schematic> <percent>
```

## Accepted For 0.9.0

These are known tradeoffs that do not need to block the public feedback release unless testing shows a real gameplay problem.

- Hidden unlockers can appear in G-menu search for "Schematics"; there may not be a safe way to hide them without breaking vanilla progression unlock logic.
- Hidden unlockers are not buildable in survival because their requirements are unavailable.
- In creative, hidden unlockers can be placed as small colorable datapad-like blocks. This is acceptable unless it causes confusion or instability.
- The neutral `medium` defaults are considered close enough for public feedback; command polish may still expose a few more config settings.
- Broad multiplayer and dedicated-server validation can happen after the Workshop feedback release.

## Later 1.0.0 Work

- [ ] Do one real fresh survival playthrough with the intended `medium` config.
- [ ] Tune research gain if schematic unlocks feel too fast or too grindy during that playthrough.
- [ ] Tune Proficiency gain if early skill growth or long-term mastery feels off.
- [ ] Tune botch chance and botch damage if low skill feels either toothless or frustrating.
- [ ] Tune salvage recovery if scrap pressure feels wrong.
- [ ] Remove or revise remaining version-specific maintainer wording from user-facing docs.
- [x] Prepare a clean public GitHub repo and issue templates for bug reports and balance feedback.
- [ ] Bump `mods/WorkingKnowledge/modinfo.sbc` to `1.0.0` after feedback and fixes.

## Personal Gameplay Test

Run this after command polish and the `0.9.0` feedback release work is ready, not before the remaining planned features are done.

- [ ] Start a fresh survival world with Experimental Mode off.
- [ ] Disable vanilla progression in world settings and let Working Knowledge own the lock path.
- [ ] Use the intended `medium` config.
- [ ] Play long enough to unlock several early schematic families through salvage and data fragments.
- [ ] Build and repair with low, mid, and high Proficiency.
- [ ] Use hand grinders, hand welders, ship grinders, and ship welders.
- [ ] Confirm botch sounds, chat messages, and HUD notifications feel understandable.
- [ ] Confirm Research Pedestal and LCD apps remain useful during normal play, not just tests.
- [ ] Save, reload, and continue the same world.

## Companion Mod Test

Working Knowledge can release standalone, but the intended play experience may be stronger with a small companion set.

- [ ] Choose the two key companion mods for your preferred playthrough.
- [ ] Add those choices to [Campaign mod shortlist](campaign_mod_shortlist.md) notes if they are not already there.
- [ ] Test Working Knowledge with only those companion mods enabled.
- [ ] Confirm they do not bypass schematic locks, placement enforcement, grinding rewards, salvage recovery, or Proficiency gain.
- [ ] Document any recommended load order or compatibility note in [Working Knowledge Workshop page draft](working_knowledge_workshop_page.md).

Do not make companion mods hard dependencies unless the standalone experience is no longer the goal.

## Technical Validation

- [ ] Run `.\tools\generate-working-knowledge-data.ps1` and confirm generated outputs are expected.
- [ ] Parse all Working Knowledge `.sbc` XML files.
- [ ] Run `.\tools\compile-mod-scripts.ps1 -ModName WkKn`.
- [ ] Run `.\build.ps1 -ModName WkKn`.
- [ ] Confirm the deployed local folder is `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`.
- [ ] Confirm `thumb.jpg` exists in the deployed mod and stays under 1 MB.
- [ ] Confirm `.tmp/` is empty before committing release prep.

## In-Game Smoke Tests

- [ ] Fresh survival world, Experimental Mode off, vanilla progression disabled.
- [ ] G-menu opens and locked block families are locked.
- [ ] Grinding unknown blocks grants research and Proficiency.
- [ ] Completed schematic families unlock their blocks.
- [ ] Data fragments grant compatible research or refund cleanly when disabled.
- [ ] Proficiency affects salvage recovery.
- [ ] Low-Proficiency welding can botch, rolls back expected integrity, and reports lost/recovered components.
- [ ] Botch sound plays once per botch and does not loop.
- [ ] Ship welder attribution works in the common active-welder cases.
- [ ] Locked player-built blocks are removed unless the player knows the schematic.
- [ ] Creative/admin bypasses behave as documented.
- [ ] Research Pedestal player/faction views and sync work.
- [ ] Research, Proficiency, Identity, and Calibrator LCD apps render correctly.
- [ ] Save/reload preserves research, Proficiency, world config, and player feedback config.
- [ ] Repeat a shorter smoke test with vanilla progression already enabled before load.

## Public Release Prep

- [ ] Finalize Workshop title, short description, and description.
- [ ] Remove maintainer-only wording from public docs.
- [ ] Note that multiplayer has not had broad dedicated-server validation yet.
- [x] Create public GitHub issue templates for bug reports and balance feedback.
- [ ] Tag the release commit.
- [ ] Publish from the in-game Mods screen.
- [ ] After publishing, monitor Workshop comments and GitHub issues for first-wave bugs.

## Known Post-1.0 Follow-Up

- Multiplayer edge-case attribution testing with several players near the same weld target.
- Dedicated server validation.
- Richer Research Terminal schematic trade and sync workflows.
- Deeper compatibility testing with the future campaign companion mod stack.
- Localization and richer public-facing descriptions.
- Save/config migration support if public worlds need long-term compatibility.
