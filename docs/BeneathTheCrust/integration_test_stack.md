# Campaign Integration Test Stack

This document tracks the first normal-world integration test for the external mod stack intended for **Space Engineers: Beneath the Crust** campaign planning.

The goal of this phase is not to build the campaign yet. The goal is to run Working Knowledge with the likely campaign-support mods in an ordinary sandbox world, find compatibility issues early, and decide which mods should remain candidates before authored campaign composition begins.

## Test Phase

- Phase: `Working Knowledge v0.9.0` external-mod integration test
- Save name: `Working Knowledge v0.9.0 Extra Mods`
- Started: 2026-07-05
- World type: normal Space Engineers save, not an authored campaign scenario
- Source for stack list: save `Sandbox_config.sbc` `Mods` list
- Current status: initial load smoke test passed with warnings to watch

## Initial Smoke Check

The flushed 2026-07-05 game log showed Working Knowledge loading, initializing `WkKnSession`, writing save storage, and unloading normally on return to menu. No Working Knowledge script crash, unhandled exception, or mod compile failure was found in the inspected game log.

Working Knowledge save storage files were created for:

- `WkKnConfig.xml`
- `WkKnResearch.xml`
- `WkKnProficiency.xml`

Warnings seen during the initial smoke test:

- `Textured Build States` reported an unsupported `.DDS` file extension warning for an armor skin icon path.
- `Long Range Searchlight` reported repeated unsupported `.DDS` file extension warnings for `Textures\Lights\flood_light.DDS`.
- The game log reported `ERROR: Camera entity from checkpoint does not exists!` during load. The world continued loading and requested respawn afterward.
- The game log reported one `Could not get planet at position ...` runtime message.
- The render log contained vanilla content missing-texture warnings, but no render crash, fatal error, device removal, or out-of-memory error was found.

Treat these as watch items during longer play. The Long Range Searchlight warnings match the existing shortlist concern and should be retested before making it a firm campaign dependency.

## Mod Stack

| Load Order | Mod | Workshop ID | Service | Campaign Test Role |
| ---: | --- | --- | --- | --- |
| 1 | [Working Knowledge](https://steamcommunity.com/sharedfiles/filedetails/?id=3758066250) | `3758066250` | Steam | Core research and progression layer under test. |
| 2 | [Welder no eye damage (remove/delete flashes/glare/strob)](https://steamcommunity.com/sharedfiles/filedetails/?id=3082535654) | `3082535654` | Steam | Welding-heavy accessibility and visual comfort. |
| 3 | [Textured Build States](https://steamcommunity.com/sharedfiles/filedetails/?id=2867863531) | `2867863531` | Steam | More readable partial-construction visuals. |
| 4 | [Sneaky Sounds - Quieter Tools](https://steamcommunity.com/sharedfiles/filedetails/?id=1662953858) | `1662953858` | Steam | Lower-fatigue tool and ambience audio. |
| 5 | [Powerful Small Rotors](https://steamcommunity.com/sharedfiles/filedetails/?id=3464224701) | `3464224701` | Steam | Small-grid mechanical tuning for salvage rigs and compact builds. |
| 6 | [Paint Gun - realistic painting for survival + special creative features](https://steamcommunity.com/sharedfiles/filedetails/?id=500818376) | `500818376` | Steam | Survival repainting for recovered grids and player bases. |
| 7 | [Long Range Searchlight](https://steamcommunity.com/sharedfiles/filedetails/?id=2741701803) | `2741701803` | Steam | Better night visibility, POI scanning, and field search. |
| 8 | [Improvised Experimentation](https://steamcommunity.com/sharedfiles/filedetails/?id=2891367014) | `2891367014` | Steam | Physical grid pickup, dragging, moving, and field salvage handling. |
| 9 | [Force Dynamic Grids](https://steamcommunity.com/sharedfiles/filedetails/?id=2940745842) | `2940745842` | Steam | Makes broken-off station fragments dynamic for salvage and destruction tests. |
| 10 | [Easy Block Renaming](https://steamcommunity.com/sharedfiles/filedetails/?id=2077166496) | `2077166496` | Steam | Bulk terminal naming quality of life. |
| 11 | [Cooperative NPC Takeover](https://steamcommunity.com/sharedfiles/filedetails/?id=2968719733) | `2968719733` | Steam | Capturing NPC grids through control-station takeover. |
| 12 | [Colorful Icons](https://steamcommunity.com/sharedfiles/filedetails/?id=801185519) | `801185519` | Steam | Inventory and terminal readability. |
| 13 | [Color Hud (Apex)](https://steamcommunity.com/sharedfiles/filedetails/?id=2332245521) | `2332245521` | Steam | HUD readability and visual comfort. |
| 14 | [Clean and Vibrant Post Processing](https://steamcommunity.com/sharedfiles/filedetails/?id=2650287553) | `2650287553` | Steam | Post-processing readability and visual clarity. |
| 15 | [Build Vision 3.0](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408) | `1697184408` | Steam | Faster in-world block controls for repair, salvage, and base operation. |
| 16 | [Build Info](https://steamcommunity.com/sharedfiles/filedetails/?id=514062285) | `514062285` | Steam | Engineering overlays, block stats, conveyor info, and placement feedback. |
| 17 | [AQD - No Armor Edges](https://steamcommunity.com/sharedfiles/filedetails/?id=1825460112) | `1825460112` | Steam | Visual cleanup for armor-heavy grids and authored locations. |
| 18 | [Advanced Welding - detaching and reattaching blocks!](https://steamcommunity.com/sharedfiles/filedetails/?id=510790477) | `510790477` | Steam | Controlled dismantling, detaching, and reattaching during salvage work. |
| 19 | [Leak Finder](https://steamcommunity.com/sharedfiles/filedetails/?id=3170315974) | `3170315974` | Steam | Base and ship airtightness debugging. |
| 20 | [Rich HUD Master](https://steamcommunity.com/sharedfiles/filedetails/?id=1965654081) | `1965654081` | Steam | HUD framework dependency for other interface mods. |
| 21 | [Text HUD API](https://steamcommunity.com/sharedfiles/filedetails/?id=758597413) | `758597413` | Steam | HUD API dependency for other interface mods. |

## Longer Playtest Focus

- Confirm Working Knowledge research and Proficiency rewards still behave correctly while using Advanced Welding detach/reattach flows.
- Check whether Improvised Experimentation moving or carrying grids can bypass intended research, salvage, or ownership gates.
- Test Cooperative NPC Takeover with Working Knowledge research and Proficiency gains from captured or hacked grids.
- Verify Force Dynamic Grids does not create unstable debris behavior around spawned stations, trading posts, or future POIs.
- Watch Long Range Searchlight and Textured Build States warnings for visible missing textures or broken block/icon presentation.
- Confirm HUD mods remain readable and do not hide Working Knowledge chat, LCD, or terminal feedback.
- Track performance during normal survival play, especially after longer salvage sessions with debris and captured grids.
