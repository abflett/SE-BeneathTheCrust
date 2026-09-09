# Working Knowledge 1.1.0 Rich HUD Test Plan

Working Knowledge `1.1.0` migrates the progress overlay to Rich HUD Master and adds graphical player and administrator settings. This plan is intentionally marked **not yet validated** until the in-game passes are complete.

## Build Gate

Open settings while holding a grinder and with recent chat messages visible. Tool context help and idle chat should disappear; close with Exit/Escape and verify the original HUD mode returns. Repeat from all three HUD modes and unload with the panel open. Check section accents, wrapped descriptions, and control alignment across pages at minimum and normal window sizes. Rich HUD window focus controls framework stacking; it cannot reorder the separate vanilla GUI renderer above or below arbitrary mod windows.

For typed-value regression testing, pause for 350 ms and confirm the slider follows valid input without applying configuration. Test Apply immediately after typing and after the pause, including multiple fields, a player setting, a world multiplier beyond the slider range, and a percentage. Reopen the panel, then save and reload the world to verify persistence. Exit without Apply must discard changes; invalid input must remain editable and prevent Apply. Check brighter schematic labels over both light and dark scenery.

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\build.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.1.0
```

Load Rich HUD Master Workshop item `1965654081`. Text HUD API should not be required by the test world once the Workshop required-item list is migrated.

## Single Player

- Exit to the main menu with the settings window closed, open, and with a value box focused; then reload the world. Confirm no `Types of ApiModule cannot be instantiated before RichHudClient is initialized` error occurs in `WkSettingsWindow.Dispose`, and the previous HUD mode is restored. Also exercise a framework reset before Working Knowledge cleanup. After any failed unload from an older build, restart the game before testing to clear potentially incomplete definition cleanup.

- With chat closed, click a numeric value box and type, backspace, select all, and paste. Repeat in the personal cooldown and sound subtype text fields. Move focus between fields: only the focused field may receive text, and typing must not activate toolbar slots, movement, or chat. Apply Changes must submit the typed value; Exit/Escape must discard it.
- Reopen after discarding a focused edit and switch pages while editing; verify no hidden field continues receiving keys. Demote an administrator with a focused server field and verify editing stops. No active field should show the obsolete "Open Chat" warning.
- Compare the progress overlay with the vanilla inventory/weight bar and toolbar: research should be pale cyan, Proficiency a distinct darker blue, with a slate translucent track. Change HUD background opacity to zero, half, and full; only the track opacity should follow it. Verify labels, paired-bar layout, and event fading remain legible over bright and dark scenes.

- Confirm the header has no X button, the sidebar has no scrollbar or contrasting well, and the footer remains visible while content scrolls. At minimum size, every navigation button must remain accessible.
- Confirm each row stacks title, wrapped description, and control; numeric sliders and their value boxes must share one horizontal line without overlap or clipping.
- Make edits on multiple pages, then Exit/Escape and reopen: none should have applied. Repeat and use Apply Changes: all valid edits should apply. A malformed numeric field must prevent the batch from submitting; the footer should identify the setting.
- Queue a preset or reset, then Exit: the world must remain unchanged. Repeat, add a setting override after the preset/reset, and Apply Changes: the override must take effect after the preset/reset.
- Test opening and closing from each game HUD mode (hidden, full, no descriptions). The game HUD should hide during editing and return to the exact prior mode on Exit, Escape, opening another game screen, framework reset, and world unload. Check hosted and remote clients affect only their own HUD.

- Run `tools/test-working-knowledge-settings.ps1` for numeric input conversion, rejection, and draft apply/discard checks.
- On first opening, verify every slider value box contains the current value. Enter `25x` for Research Scale and confirm Apply Changes retains it beyond the slider's 10x range, including after page navigation and save/reload. Dragging the slider must return to its practical range only when actually changed.
- Enter `5` and `5%` in a percentage field; both must mean 5%. Check precise values, valid endpoints, invalid numbers, non-finite input, incorrect units, and fractional row counts. Invalid drafts must remain editable and must not change the setting.
- Leave a value partially typed while settings refresh; the draft must remain intact. Apply Changes, switch pages, and verify the acknowledged value. Non-admin value boxes and Apply Changes must not permit world changes.
- Test window opening, dragging, resizing, and closing at 16:9, ultrawide, and a small or narrow viewport. The entire window, including its close button, must stay inside the screen. Verify cursor hit positions after automatic scaling.
- With Text HUD API also loaded, open its player menu (chat plus F2), select Working Knowledge > Open settings, then close chat. Verify the same Rich HUD window opens. Repeat without Text HUD API; normal entry points must still work. Without Rich HUD Master, the optional launcher should report that it is unavailable. Rejoin a world and check for duplicate launchers.

- Confirm the client log reports that Rich HUD Master connected without a Working Knowledge exception.
- Run `/wk settings` and confirm it opens the dedicated Working Knowledge Settings window directly to Progress HUD.
- Confirm the compact sidebar exposes Player Settings, Server Settings, and Help; every page must use full-width stacked rows and vertical scrolling without a horizontal control strip.
- Open Rich HUD Terminal and confirm Working Knowledge contains a functional Settings launcher and Help page.
- Confirm the window closes through its footer Exit button and Escape, can be dragged/resized, and reopens cleanly.
- Exercise every player toggle, slider, dropdown, text value, and reset button; apply changes, reopen the page and confirm values persist.
- Trigger research and Proficiency gains and confirm the compact two-bar rows update, order correctly, move with position/offset settings, and respect row/fade limits.
- Apply each difficulty preset and representative settings from every server category as an administrator.
- Confirm `/wk config` and `/wk difficulty` still report and change the same values.
- Create a fresh world and confirm its initial preset is Easy with `1.5x` gain/reward/salvage/forgiveness modifiers and `0.75x` botch chance/damage/pressure modifiers.
- Load an existing 1.0.2 world with manually tuned values and confirm every effective world setting is unchanged before and after a normal save/reload.
- Confirm `/wk config world reset` explicitly restores the Easy defaults while `/wk difficulty medium` still applies the neutral `1.0x` preset.
- Confirm ratio commands accept canonical values such as `0.05` and reject `5`, `5%`, `NaN`, and infinity without changing the current value.
- Set player and world suppression thresholds to `0.05`, save/reload, and confirm the UI still displays 5% while persistence retains the established percent-point value.
- Exercise a multiplier slider around `0`, `0.01x`, `0.1x`, `1x`, and `10x`; confirm useful low-end control, correct labels, and canonical server values.
- Load an existing multiplier outside the panel's practical range and confirm opening or refreshing the settings window does not rewrite it.

## Hosted Multiplayer

- As host admin, confirm both player and Server Settings controls are editable.
- As a non-admin client, confirm Player Settings work and Server Settings controls remain disabled.
- Promote the client, request `/wk settings` again, and confirm server controls become editable only after the server returns admin permission.
- Change settings from each client and verify only personal values follow that player while world changes synchronize from the authoritative host.

## Dedicated Server

- Deploy the unpublished Workshop-cache overlay with `build-workingknowledge-dedicated.ps1` while the server is stopped.
- Confirm the dedicated log reports that graphical Rich HUD work is delegated to connected clients.
- Confirm a remote player receives current player/world snapshots and cannot edit world settings.
- Confirm a promoted administrator can apply world settings and difficulty presets through the GUI.
- Trigger remote research and Proficiency gains and verify the overlay updates only on the affected client.
- Restart the server and reconnect to verify player and world settings persisted.

## Logs And Release Metadata

- Inspect client, hosted, and dedicated logs for Working Knowledge errors, warnings, exceptions, rejected valid messages, or repeated Rich HUD initialization.
- Confirm core progression and chat configuration remain usable when Rich HUD Master is deliberately omitted.
- Replace the Workshop required item from Text HUD API (`758597413`) to Rich HUD Master (`1965654081`) before publishing.
