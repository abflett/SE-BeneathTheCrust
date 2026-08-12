# Working Knowledge 1.0.1 Dedicated Server Test Plan

Working Knowledge `1.0.1` is a focused hotfix for server-authoritative commands and the multiplayer progression issues first reproduced after the stable Workshop release.

## Prepare The Build

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.0.1
.\build.ps1 -ModName WkKn
```

Use a fresh dedicated-server world for the primary pass. Also keep a single-player world and a hosted multiplayer world available for regressions.

For unpublished local testing, stop the dedicated server and run `build-workingknowledge-dedicated.ps1`. Local-name mods are rejected in multiplayer, so the helper keeps Workshop identity `3758066250` while overlaying its client and server caches with the same local build. Text HUD API remains on Workshop. Rerun the helper after each code change and whenever Steam updates or verifies the Workshop cache.

## Command Routing

As a normal connected player:

- Confirm `/wk`, `/wk research`, `/wk proficiency`, `/wk difficulty`, and `/wk config` return private output.
- Confirm the commands do not appear in public chat or the dedicated-server console as player chat.
- Change a personal feedback setting, query it again, reconnect, and confirm it persisted.
- Change a Text HUD position or row setting and confirm the local overlay uses the new value without reconnecting.
- Confirm `/wk admin` and other admin-only mutations are rejected.

As a configured server administrator:

- Confirm `/wk` includes the admin entry.
- Confirm `/wk admin` and `/wk admin audit` return private output.
- Apply a difficulty or world-config change and confirm another admin can read the authoritative value.
- Use research and Proficiency commands against `me`, a named online player, and an identity or Steam ID.
- Save and restart the server, then confirm command mutations persisted.

## Security And Isolation

- Confirm one player's command output is not visible to another player.
- Confirm a non-admin cannot invoke admin research, Proficiency, config, difficulty, reset, or audit operations.
- Confirm malformed and unknown `/wk` commands fail safely without disconnecting the player or producing a server exception.

## Regression Checks

- Repeat the public and admin command checks in single-player.
- Repeat them as the host and as one remote player in hosted multiplayer.
- Check `SpaceEngineers.log` and the dedicated-server log for Working Knowledge exceptions.

## Multiplayer Progression Follow-up

As a player connected to the dedicated server:

- Grind an unknown block and confirm a recent-event HUD row appears with both research and Proficiency bars.
- Continue grinding the same schematic family and confirm both bars update to the server's current values.
- Weld a known block and confirm its Proficiency bar appears and updates without inventing research progress.
- Use `/wk research` and `/wk proficiency` to confirm the displayed bar values agree with the authoritative summaries.
- Confirm another connected player does not receive your progress row.
- Set `progressHudEnabled` to `false`, earn progress, and confirm no row appears; re-enable it and confirm the next progress event appears.
- Test a non-default position, row count, order, offset, and fade time after changing them through `/wk config`.
- Confirm the client log reports `Text HUD API connected; progress bars are ready.` and contains no Working Knowledge HUD exception.
- Repeat the HUD checks as a hosted remote player and in single-player to cover both networked and local delivery paths.
