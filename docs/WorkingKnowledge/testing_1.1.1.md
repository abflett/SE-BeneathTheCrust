# Working Knowledge 1.1.1 unload hotfix validation

Release candidate for [issue #2](https://github.com/abflett/SE-BeneathTheCrust/issues/2). Maintainer smoke testing found normal behavior; the maintainer chose to defer the full checklist and monitor reported issues. Publication is a separate maintainer step.

The subsequent startup-message change adds `Type /wk settings to open the settings panel.` to successful initialization/load notifications, including legacy-data loads. Error notifications remain unchanged. This addition stays in the pending 1.1.1 release.

Validation recorded on 2026-09-26: mod compilation, all 34 settings checks, all 9 unload scenarios, and release validation passed (19 XML files, icons, catalog parity, layer generator and priority/fallback fixtures). The local Working Knowledge test copy was deployed. No Workshop upload or release tag was created.

## Automated checks

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\test-working-knowledge-settings.ps1
.\tools\test-working-knowledge-unload.ps1
.\tools\validate-working-knowledge-release.ps1 -ExpectedVersion 1.1.1 -SkipCompile
.\build.ps1 -ModName WkKn
```

The unload harness compiles production disposal and session-unload methods against dependency doubles. It covers a stale registration flag with an unavailable renderer, HUD restoration and detach failures, continued cleanup, discarded edits, cursor release, and repeated disposal/unload. It cannot reproduce the game's actual mod unload ordering.

## Shutdown audit

- Settings disposal no longer calls Hide/DiscardChanges, text setters, focus handlers, or lazy Rich HUD singletons. Visibility and node detachment operate on the bundled client's local state; cursor release is a plain static flag.
- HUD restoration and node detachments are isolated so a failure does not prevent the other cleanup steps. Window/menu references and pending commands are released during reset.
- Session event teardown tolerates missing Utilities, Players, and Entities APIs. External teardown stages log exceptions and continue with remaining network, terminal, HUD, and display cleanup.
- Reviewed progress-overlay detachment (local visibility/node operations), optional Text HUD launcher (external setter calls already guarded), network handlers (existing null checks), grid event removal, and display snapshot cleanup. Display network failures no longer prevent clearing local snapshots/session references.
- Bundled Rich HUD sources and persistence behavior are unchanged. A logged cleanup failure is still a defect to investigate, even if exiting completes.

## In-game checks pending

Start from a fresh game process, using the 1.1.1 local test build and Rich HUD Master. After each exit, load a world again in the same process.

- Exit without ever opening settings; repeat after opening and closing settings.
- Exit with settings open, with a numeric field focused, and with a text field focused. Include unapplied valid and invalid edits. Unapplied changes must not persist.
- Apply changes, save, exit, and reload; applied settings and progression must persist.
- Check normal Exit/Escape still discards edits, releases the cursor, and restores each of the three HUD modes.
- Exit with progress bars visible and after they fade. Repeat with optional Text HUD API installed, and without Rich HUD Master.
- Repeat host/client disconnect and dedicated-server stop/restart. Include an administrator with server settings open.
- If a framework reset occurs during play, verify the old window disappears and settings can reopen after reconnection.
- Inspect logs for `WkSettingsWindow.Dispose`, `TextBuilder`, `ModCrashedException`, `cleanup failed`, and `Failed to cleanly unload session`. Record any failure and its stack trace before release.

These checks reduce known shutdown risks; they do not guarantee every game/mod combination will exit successfully.
