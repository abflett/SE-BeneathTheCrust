# Working Knowledge Workshop Page Draft

This is the public-facing Workshop page draft for `Working Knowledge`.

Official publishing flow: https://www.spaceengineersgame.com/modding-guides/space-engineers-workshop-guide/

Use [Working Knowledge release readiness checklist](working_knowledge_v1_release_checklist.md) for maintainer readiness, tuning, and validation tasks before publishing.

## Workshop title

Working Knowledge

## Short description

Salvage unknown blocks, decode research data, and unlock schematic families through a custom progression system mirrored into vanilla research.

## Workshop description draft

Working Knowledge is a standalone technology progression mod for Space Engineers.

It replaces the default block-unlock path with a salvage-first research loop:

- Most vanilla public cube blocks are grouped into schematic families.
- Unknown blocks are locked through vanilla research until their schematic family is completed.
- Grinding unknown blocks grants partial research based on how much of the block was dismantled.
- Low Proficiency converts much of the recovered component value into scrap; high Proficiency restores normal intact recovery.
- Low Proficiency also makes welding less reliable: construction botches can knock build progress back, destroy some components, and recover forgiven components when material loss would be too punishing.
- A selectable `Working Knowledge Research` LCD app shows each viewer's personal and faction schematic progress.
- A selectable `Working Knowledge Proficiency` LCD app shows each viewer's active hands-on skill progress.
- Data Fragments can appear as rare loot in unknown signals and selected vanilla POI containers, with uncommon, rare, and prototech fragments weighted below common fragments.
- Exact Data Schematics are durable shareable items for POI, mission, scenario, or admin rewards.
- A Research Pedestal shows filtered player/faction schematic progress and can manually sync partial or completed progress.
- `/wk` shows command help, `/wk research` or `/wk res` shows schematic progress, `/wk proficiency` shows hands-on skill, `/wk config` shows player feedback settings, `/wk config help` shows player config help, `/wk difficulty` shows the active preset and modifiers, and `/wk admin` indexes admin help categories.

Current balance targets the default `medium` play experience.

Proficiency keeps welding and grinding quality in a player-only hands-on skill layer. Research answers whether a player or faction knows a schematic family; Proficiency answers how well an individual player can build, repair, grind, and recover that family.

This mod is designed as a standalone technology progression layer for the larger Space Engineers: Beneath the Crust project, but it can be loaded and tested on its own.

## Commands

Public commands:

- `/wk`
- `/wk help`
- `/wk research`
- `/wk res`
- `/wk research help`
- `/wk res help`
- `/wk proficiency`
- `/wk prof`
- `/wk proficiency help`
- `/wk config`
- `/wk config help`
- `/wk config <setting> help`
- `/wk config <setting> <value>`
- `/wk config reset`
- `/wk difficulty`
- `/wk difficulty help`
- `/wk admin`
- `/wk admin help`

Admin commands:

- `/wk admin unlockall`
- `/wk research show <player>`
- `/wk research reset <server|player>`
- `/wk research unlock <player> <schematic>`
- `/wk research forget <player> <schematic>`
- `/wk research set <player> <schematic> <percent>`
- `/wk proficiency show <player>`
- `/wk proficiency reset <server|player>`
- `/wk proficiency master <player> <schematic>`
- `/wk proficiency forget <player> <schematic>`
- `/wk proficiency set <player> <schematic> <percent>`
- `/wk config <setting> help`
- `/wk config <setting> <value>`
- `/wk config world`
- `/wk config world help`
- `/wk config world reset`
- `/wk difficulty <novice|easy|medium|hard|extreme>`
- `/wk difficulty custom <difficulty-setting> help`
- `/wk difficulty custom <difficulty-setting> <value>`

Admin help categories:

- `/wk research help`
- `/wk proficiency help`
- `/wk botch help`
- `/wk salvage help`
- `/wk feedback help`
- `/wk defaults help`
- `/wk difficulty help`

Only admins can change world config values, use targeted research/Proficiency commands, or use `/wk admin unlockall`. Players can change their own feedback settings.

## Compatibility notes

- Best tested in a new world.
- Experimental Mode is not required.
- The mod forces vanilla progression on at runtime so it can own the lock path.
- Other progression or research-overhaul mods are likely to conflict.
- Player-built locked blocks are removed server-side unless the player knows the schematic. Creative mode bypasses this check, and promoted Space Master/admin full-built placements are allowed.
- The custom research store is saved in world storage as `WkKnResearch.xml`.
- The custom Proficiency store is saved in world storage as `WkKnProficiency.xml`.
- The world config is saved as `WkKnConfig.xml`.
- Player feedback settings are saved in world storage as `WkKnPlayerConfig.xml`.
- Internal script, storage, LCD Custom Data, and generated game definition identifiers use the `WkKn` prefix.
