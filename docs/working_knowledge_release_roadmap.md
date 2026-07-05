# Working Knowledge Release Roadmap

This roadmap tracks what needs to happen before each Working Knowledge release tier. It is intentionally short: completed launch-prep items belong in git history, not in a permanent checklist.

Working Knowledge should stay standalone until `1.0.0`. Campaign integration, companion mod packs, richer story content, and deeper terminal trade systems are later work.

## Current Release: 0.9.x Feedback

Goal: publish and iterate on the first public Workshop release.

- Gather bug reports and balance feedback from fresh survival worlds.
- Run a light in-game smoke test after each gameplay fix.
- Confirm admin-only command gating and targeted command feedback are clear.
- Watch for confusion around hidden schematic unlockers appearing in G-menu search.
- Track compatibility reports from mods that change progression, placement, grinding, welding, or salvage.
- Keep broad dedicated-server validation as a known gap until it has been tested directly.

Accepted 0.9.x tradeoffs:

- Hidden unlockers may appear in G-menu search for "Schematics".
- Hidden unlockers are not survival-buildable because their requirements are unavailable.
- In creative, hidden unlockers can be placed as small colorable datapad-like blocks.
- The default `medium` balance is considered close enough for public feedback.

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
