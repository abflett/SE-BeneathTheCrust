# Working Knowledge 1.0.2 Persistence Test Plan

Working Knowledge `1.0.2` is a focused hotfix for progression and configuration persistence when Space Engineers saves a world under a new name.

## Status

Implementation and script compilation are complete. Focused in-game validation is pending.

## Automated Checks

From the repository root:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.0.2
.\build.ps1 -ModName WkKn
```

## Focused Save As Test

1. Load the existing `Star System test 1` world, which has partial research and Proficiency in its legacy `WkKn*.xml` files.
2. Confirm colored chat reports: `Loaded legacy Working Knowledge data. It will be migrated to the new save format on the next world save.`
3. Confirm `SpaceEngineers.log` reports that legacy world storage loaded and migration is pending.
4. Save the world normally and exit to the main menu.
5. Confirm the log reports that legacy data migrated to canonical `Sandbox.sbc` persistence version 1.
6. Reload `Star System test 1` and confirm colored chat reports: `Working Knowledge loaded successfully.`
7. Confirm the same partial research and Proficiency remain, and the log reports that canonical persistence version 1 loaded from `Sandbox.sbc`.
8. Exit and save, then select `Star System test 1` on the Load Game screen and use **Save As** with a new unique name.
9. Load the newly named world without copying any files manually.
10. Confirm colored chat reports a successful Working Knowledge load rather than legacy migration or fresh initialization.
11. Confirm personal research, faction research, Proficiency, player preferences, and world settings match the original save.
12. Reload the original save and confirm its state remains unchanged.

The legacy XML files may remain in the original save, but their timestamps and contents should stop changing after migration. The Save As copy does not require a Working Knowledge `Storage` folder because its canonical state is inside `Sandbox.sbc`.

## Regression Checks

- A normal manual save and reload preserves partial progress.
- A configured Space Engineers autosave and reload preserves partial progress.
- `/wk` research, Proficiency, configuration, and administrator mutations remain active immediately in memory before saving.
- A fresh world reports successful initialization, writes canonical persistence on its first save, and reports a successful load afterward.
- `SpaceEngineers.log` contains no Working Knowledge persistence exceptions.
