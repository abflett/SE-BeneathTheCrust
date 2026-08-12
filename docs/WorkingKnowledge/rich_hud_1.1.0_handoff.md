# Working Knowledge 1.1.0 Rich HUD Handoff

Use this document to resume the Rich HUD work in a new chat without reconstructing the design history. It records the state after the first successful in-game test of the custom settings window on **2026-08-13**.

## Suggested New-Chat Prompt

> Read `AGENTS.md`, `README.md`, `docs/WorkingKnowledge/implementation.md`, `mods/WorkingKnowledge/README.md`, and `docs/WorkingKnowledge/rich_hud_1.1.0_handoff.md`. Continue on the current Rich HUD 1.1.0 feature branch. The custom settings window has passed its first visual test and is already a major improvement, but it still needs visual refinement. Inspect the current state, then wait for my next screenshot or specific feedback before changing its layout.

## Current Git And Release State

- Branch: `feature/wk-rich-hud-1.1.0`
- Latest implementation commit at this handoff: `659bb3c Add compact Rich HUD settings window`
- Intended mod version: `1.1.0`, currently in development
- Stable release/hotfix base: `1.0.1`
- Rich HUD Master Workshop item: `1965654081`
- Text HUD API is being replaced and should not be required by the final `1.1.0` Workshop configuration.
- The local mod was compiled, validated, and deployed to `%APPDATA%\SpaceEngineers\Mods\Working Knowledge` for testing before this handoff.

Do not merge this feature into the stable line or publish it until the full test plan is complete.

## What Has Been Implemented

### Progress Overlay Migration

The recent research and Proficiency overlay now uses Rich HUD Master. It preserves the compact paired-bar presentation while gaining Rich HUD scaling and positioning support.

The current visual choices were made from in-game feedback:

- The label overlaps the upper progress bar rather than floating above or to its left.
- A stronger three-direction dark outline keeps the label readable over bright green and bright world backgrounds.
- The research and Proficiency bars touch vertically so each row reads as a single unit.
- Player controls cover visibility, row count, fade time, ordering, anchor position, and X/Y offsets.

Primary file: `mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/RichHud/WkProgressHudOverlay.cs`.

### Custom Settings Window

Rich HUD Terminal's normal `ControlPage` was tested first, but its controls are arranged as fixed `300 x 250` horizontal tiles. That produced excessive framing and horizontal scrolling. `RebindPage` has the desired stacked appearance but is specialized for key bindings, and the terminal API does not expose an arbitrary custom-page primitive.

The selected solution is a separate `WkSettingsWindow` built with Rich HUD's public lower-level HUD elements. This keeps Rich HUD's styling, cursor, DPI scaling, input, and common widgets while giving Working Knowledge control of the layout.

The first in-game test was successful. The maintainer described it as a huge improvement over the terminal layouts, although it still needs another visual-refinement pass.

Current window behavior:

- `/wk settings` opens the custom window directly on **Progress HUD**.
- The shared Rich HUD Terminal retains a Working Knowledge **Settings** launcher and **Help** page.
- The window has a compact left sidebar with **Player Settings**, **Server Settings**, and **Information** headings.
- The right side uses full-width stacked rows and vertical scrolling.
- It supports X and Escape closing, dragging, resizing, and screen-bound clamping.
- Player pages include Progress HUD and Feedback.
- Server pages include Difficulty plus the Research, Proficiency, Botches, Salvage, Feedback Defaults, and New Player Defaults categories generated from the existing configuration definitions.
- Non-administrators can see server settings but cannot edit them. The existing server permission response controls when those rows become active.
- All changes use the existing command validation, multiplayer request, persistence, and server-authoritative configuration paths. The GUI is not a second configuration system.
- Existing `/wk config`, `/wk difficulty`, and help commands remain supported as a fallback.

Primary file: `mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/RichHud/WkSettingsWindow.cs`.

## Important Integration Files

- `Integration/RichHud/WkSettingsWindow.cs` — custom window, sidebar, pages, stacked setting rows, and controls.
- `Integration/RichHud/WkRichHudSettingsMenu.cs` — Rich HUD Terminal launcher and Help page.
- `Integration/RichHud/WkKnSession.RichHud.Settings.cs` — opens the window, supplies values, applies changes through the existing request paths, and handles admin state.
- `Integration/RichHud/WkRichHudIntegration.cs` — Rich HUD lifecycle, client connection, and dedicated-server diagnostics.
- `Integration/RichHud/WkProgressHudOverlay.cs` — compact research and Proficiency overlay.
- `Integration/Commands/WkKnSession.Commands.cs` — `/wk settings` command entry and command help.
- `Integration/Configuration/WkPlayerConfigStore.cs` — player setting metadata consumed by the GUI.
- `Integration/Configuration/WkConfigStore.cs` — authoritative world setting metadata consumed by the GUI.
- `Integration/RichHudFramework/` — bundled official Rich HUD Framework Full Client `1.3.0.0` source under its MIT license. This is the client bridge, not Rich HUD Master itself.
- `mods/WorkingKnowledge/ThirdParty/README.md` — bundled-framework attribution and dependency explanation.

All paths in this section are relative to `mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/` unless stated otherwise. The source tree remains authoritative if later refactoring moves a file.

## Design Decisions To Preserve

- Keep the custom window. Do not return to the fixed horizontal `ControlPage` card layout unless the Rich HUD API itself gains a genuinely suitable stacked page.
- Prefer simple top-to-bottom rows, clear labels, restrained framing, and vertical scrolling.
- Keep player-owned and server-authoritative settings visibly separated.
- Do not bypass current multiplayer validation or admin gates for UI convenience.
- Keep chat commands functional. Rich HUD is an optional presentation layer; core progression and configuration must still work if it is unavailable.
- Dedicated servers do not render Rich HUD. They synchronize data and settings; connected clients own all graphical work.
- The bundled Full Client communicates with the separately loaded Rich HUD Master Workshop mod. It does not replace that dependency.

## Next Visual Review

Wait for the maintainer's next screenshot or specific feedback before making speculative layout changes. Likely review points are:

- overall window size and initial placement;
- sidebar width, navigation density, and selected-page styling;
- row height, divider weight, and excess empty space;
- label/description wrapping and control alignment;
- scrollbar appearance and mouse-wheel behavior;
- dropdown expansion or clipping near page edges;
- appearance of disabled server controls for non-admin players;
- behavior at smaller resolutions and after resizing.

Apply feedback iteratively and avoid redesigning unrelated progression or configuration behavior during the visual pass.

## Validation Workflow

After every C# change, run from the repository root:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
```

Deploy when an in-game visual test is needed:

```powershell
.\build.ps1 -ModName WkKn
```

Before treating `1.1.0` as release-ready, run:

```powershell
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.1.0
```

Follow `docs/WorkingKnowledge/testing_1.1.0.md` for single-player, hosted multiplayer, dedicated-server, fallback, persistence, and log checks. The test plan remains **not yet validated**.

## Relevant Commit Trail

- `2a760e2` — began the Rich HUD 1.1.0 migration.
- `c5083e0` — restored compact progress-label overlap.
- `5ad8395` — strengthened the progress-label outline.
- `bea0e55` — joined the paired progress bars into one visual unit.
- `211aaa3` — reorganized the initial terminal settings attempt vertically.
- `659bb3c` — replaced the terminal control tiles with the compact custom settings window.

Consult `docs/WorkingKnowledge/changelog.md` for public-facing release notes rather than copying this engineering handoff into the Workshop description.
