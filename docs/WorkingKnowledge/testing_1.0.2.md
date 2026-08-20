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

1. Create or load a local survival world with Working Knowledge enabled.
2. Gain partial research and partial Proficiency in at least one non-Fundamentals schematic.
3. If faction research is available, sync some progress through a Research Pedestal.
4. Change one player feedback preference and one administrator world setting.
5. Save the world normally, then wait without earning any additional Working Knowledge progress so every store is unchanged.
6. Use **Save As** and give the world a new unique name.
7. Exit to the main menu and load the newly named world.
8. Confirm personal research, faction research, Proficiency, player preferences, and world settings match the original save.
9. Confirm the new save contains `WkKnResearch.xml`, `WkKnProficiency.xml`, `WkKnConfig.xml`, and `WkKnPlayerConfig.xml` in its Working Knowledge `Storage` folder.
10. Reload the original save and confirm its state remains unchanged.

## Regression Checks

- A normal manual save and reload preserves partial progress.
- A configured Space Engineers autosave and reload preserves partial progress.
- `/wk` research, Proficiency, configuration, and administrator mutations remain active immediately in memory before saving.
- `SpaceEngineers.log` contains no Working Knowledge persistence exceptions.

