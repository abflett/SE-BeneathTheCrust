# Working Knowledge

[Campaign README](../../README.md) | [Standalone mods index](../README.md)

<p align="center">
  <img src="../../docs/assets/working-knowledge-hero.png" alt="An engineer reviewing holographic rover schematics beside a survival rover and field base on an alien surface." />
</p>

**Working Knowledge** is a standalone Space Engineers progression mod.

It turns technology unlocks into a salvage, research, and hands-on practice loop. The mod can be tested on its own today and can later become one progression layer inside the larger **Space Engineers: Beneath the Crust** campaign.

## What It Changes

- Most vanilla blocks are grouped into schematic families.
- Unknown schematic families are locked through vanilla progression.
- Grinding unfamiliar blocks grants research progress toward their schematic family.
- Data Fragments can appear as rare loot and grant random partial schematic research.
- Data Schematics are durable exact schematic items for POI, mission, scenario, or admin rewards.
- Player Proficiency tracks how skilled an engineer is with each family.
- Low Proficiency can reduce salvage recovery and cause construction botches that damage build progress, destroy components, and recover forgiven components when loss would be too punishing.
- Completed research unlocks the matching block family for normal use.

Research answers: **Do I know the schematic?**

Proficiency answers: **How well can I build, repair, grind, and salvage it?**

## In-Game Tools

- `Working Knowledge Research` LCD app - shows the local viewer's personal and faction schematic progress.
- `Working Knowledge Proficiency` LCD app - shows the local viewer's active hands-on skill progress.
- `Working Knowledge Identity` LCD app - shows player and faction identity data.
- `Working Knowledge Calibrator` LCD app - helps tune display safe areas.
- `Research Pedestal` - terminal block for viewing and syncing player/faction research.

See [LCDHelp.md](LCDHelp.md) for LCD setup and Custom Data options.

## Chat Commands

Public commands:

- `/wk` or `/wk help` - Show public help.
- `/wk res` or `/wk research` - Show personal research progress.
- `/wk research help` - Show research commands and tuning settings.
- `/wk prof` or `/wk proficiency` - Show personal Proficiency progress.
- `/wk proficiency help` - Show Proficiency commands and tuning settings.
- `/wk config` - Show personal feedback settings.
- `/wk config help` - Show player config help.
- `/wk config <setting> help` - Show one player setting's description and aliases.
- `/wk config <setting> <value>` - Update one personal feedback setting.
- `/wk config reset` - Reset personal feedback settings.
- `/wk difficulty` - Show the current difficulty preset and modifiers.
- `/wk difficulty help` - Show difficulty presets and custom settings.
- `/wk admin` or `/wk admin help` - Show admin help categories.
- `/wk admin audit` - Show runtime and compatibility-layer audit results.

Admin-only config and tuning:

- `/wk admin unlockall` - Complete all research for yourself without changing Proficiency.
- `/wk research show <player>` - Show another player's research progress.
- `/wk research reset <server|player>` - Reset server or player research.
- `/wk research unlock <player> <schematic>` - Complete one schematic for a player target.
- `/wk research forget <player> <schematic>` - Revoke one schematic for a player target.
- `/wk research set <player> <schematic> <percent>` - Set exact schematic progress.
- `/wk proficiency show <player>` - Show another player's Proficiency.
- `/wk proficiency reset <server|player>` - Reset server or player Proficiency.
- `/wk proficiency master <player> <schematic>` - Master one Proficiency for a player target.
- `/wk proficiency forget <player> <schematic>` - Remove one Proficiency for a player target.
- `/wk proficiency set <player> <schematic> <percent>` - Set exact Proficiency progress.
- `/wk config <setting> help` - Show one world setting's description and aliases.
- `/wk config <setting> <value>` - Update one world setting.
- `/wk config world` - Show world settings.
- `/wk config world help` - Show world config help.
- `/wk config world reset` - Reset world settings to mod defaults.
- `/wk difficulty <novice|easy|medium|hard|extreme>`
- `/wk difficulty custom <difficulty-setting> help`
- `/wk difficulty custom <difficulty-setting> <value>`

See [Working Knowledge configuration](../../docs/WorkingKnowledge/configuration.md) for current settings and accepted values.

## Current Status

Working Knowledge is in its `0.13.x` public feedback release series.

The current balance targets the default `medium` play experience. Public feedback, smoke testing, and survival playthrough tuning are the remaining focus before treating the mod as stable.

The mod currently includes:

- Vanilla research gates generated for public cube blocks.
- Hidden unlock markers for schematic-family completion.
- Player and faction research stores.
- Player-only Proficiency persistence.
- Grinding research and Proficiency gains.
- Configurable Text HUD API-backed progress overlay for recent research and Proficiency progress.
- Welding and repair quality effects.
- Player feedback settings through `/wk config`.
- Admin-configurable tuning through `/wk config`.
- LCD apps and Research Pedestal workflows.
- External Working Knowledge Layer support for mapping third-party block mods into schematic families.
- Versioned custom schematic groups and priority-controlled remapping of built-in or lower-priority layer assignments.
- Layer diagnostics in admin chat, the Space Engineers log, and the F11 mod-error screen.

## Compatibility

- Best tested in a new world.
- Experimental Mode is not required.
- The mod forces vanilla progression on at runtime so it can own the lock path.
- Creative worlds and admin creative-mode testing are allowed to bypass schematic locks.
- Text HUD API is used for the optional, player-configurable HUD progress overlay; core progression still falls back to chat and notifications if the framework is unavailable.
- Other progression or research-overhaul mods are likely to conflict.
- Public modded blocks without a Working Knowledge mapping remain outside its research and Proficiency systems. Use a compatibility layer when those blocks should participate.

## Project Docs

- [Campaign overview](../../docs/BeneathTheCrust/overview.md)
- [Working Knowledge changelog](../../docs/WorkingKnowledge/changelog.md)
- [Working Knowledge configuration](../../docs/WorkingKnowledge/configuration.md)
- [Working Knowledge implementation reference](../../docs/WorkingKnowledge/implementation.md)
- [Working Knowledge inspirations and attribution](../../docs/WorkingKnowledge/inspirations.md)
- [Working Knowledge release roadmap](../../docs/WorkingKnowledge/release_roadmap.md)
- [Working Knowledge Workshop description](../../docs/WorkingKnowledge/workshop_description_bbcode.txt)

## Local Test Build

From the repository root:

```powershell
.\build.ps1 -ModName WkKn
```

After C# script changes:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
```
