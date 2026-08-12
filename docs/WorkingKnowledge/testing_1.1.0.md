# Working Knowledge 1.1.0 Rich HUD Test Plan

Working Knowledge `1.1.0` migrates the progress overlay to Rich HUD Master and adds graphical player and administrator settings. This plan is intentionally marked **not yet validated** until the in-game passes are complete.

## Build Gate

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\build.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.1.0
```

Load Rich HUD Master Workshop item `1965654081`. Text HUD API should not be required by the test world once the Workshop required-item list is migrated.

## Single Player

- Confirm the client log reports that Rich HUD Master connected without a Working Knowledge exception.
- Run `/wk settings` and confirm it opens the dedicated Working Knowledge Settings window directly to Progress HUD.
- Confirm the compact sidebar exposes Player Settings, Server Settings, and Help; every page must use full-width stacked rows and vertical scrolling without a horizontal control strip.
- Open Rich HUD Terminal and confirm Working Knowledge contains a functional Settings launcher and Help page.
- Confirm the window closes through its X button and Escape, can be dragged/resized, and reopens cleanly.
- Exercise every player toggle, slider, dropdown, text value, and reset button; reopen the page and confirm values persist.
- Trigger research and Proficiency gains and confirm the compact two-bar rows update, order correctly, move with position/offset settings, and respect row/fade limits.
- Apply each difficulty preset and representative settings from every server category as an administrator.
- Confirm `/wk config` and `/wk difficulty` still report and change the same values.

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
