# Campaign Mod Shortlist

This is the optional external Space Engineers mod shortlist for the larger **Space Engineers: Beneath the Crust** campaign.

These are not current dependencies. Treat this as a planning reference for campaign feel, compatibility testing, and future server/world-pack decisions. Last manual link/summary check: 2026-06-28. Recheck candidates before adding any of them to a published stack.

## Selection Goals

- Preserve the vanilla Space Engineers feel.
- Make salvage, repair, capture, and field engineering more tactile.
- Reduce UI friction for players and admins.
- Support richer NPC encounters without baking those systems directly into Working Knowledge.
- Avoid turning the campaign into a kitchen-sink modpack.

## Core Campaign Systems

| Mod | Summary | Campaign Use | Notes |
| --- | --- | --- | --- |
| [Modular Encounters Systems](https://steamcommunity.com/sharedfiles/filedetails/?id=1521905890) | Framework for spawning and despawning NPC grids, with behavior systems for NPC encounters. | Core candidate for ambient traffic, hostile patrols, faction ruins, and encounter packs. | Framework only; it does not add encounters by itself. Choose encounter content separately. |
| [Cooperative NPC Takeover](https://steamcommunity.com/workshop/filedetails/?id=2968719733) | Lets players capture NPC grids after boarding and hacking all control stations. | Makes ship/station capture fit the campaign loop without grinding every terminal block by hand. | Good match for salvage and recovery gameplay. Test with Working Knowledge research/proficiency rewards. |

## Salvage And Field Engineering

| Mod | Summary | Campaign Use | Notes |
| --- | --- | --- | --- |
| [Improvised Experimentation](https://steamcommunity.com/sharedfiles/filedetails/?id=2891367014) | Networked grid pickup, carrying, dragging, rotating, throwing, and reference alignment for small grids. | Lets players physically move wreckage, cargo chunks, small weapons, and scavenged components. | Strong fit for crash-site and scrapyard-style early game. Check limits and balance in gravity. |
| [Advanced Welding](https://steamcommunity.com/sharedfiles/filedetails/?id=510790477) | Adds weld pads, handheld grinder detach mode, and precision grinding. | Supports controlled dismantling, repair work, and block recovery as a campaign mechanic. | High synergy with Working Knowledge, but test carefully with research/proficiency awards. |
| [Force Dynamic Grids](https://steamcommunity.com/sharedfiles/filedetails/?id=2940745842) | Converts split-off static-grid fragments that can be dynamic after explosions or breaks. | Prevents floating unsupported wreck fragments and makes destruction/salvage feel more physical. | Requires `Unsupported Stations` off. Important for ruins and damaged POIs. |

## Building And Admin Quality Of Life

| Mod | Summary | Campaign Use | Notes |
| --- | --- | --- | --- |
| [Build Vision 3.0](https://steamcommunity.com/sharedfiles/filedetails/?id=1697184408) | Context menu for aiming at blocks and adjusting terminal settings without opening the full terminal. | Reduces terminal friction during field repairs, base setup, and ship tuning. | Requires UI/dependency validation in the final mod stack. |
| [Build Info](https://steamcommunity.com/sharedfiles/filedetails/?id=514062285) | Adds block stats, overlays, terminal extra info, toolbar info, and conveyor network visualization. | Helps players understand mechanics, ports, airtightness, conveyors, tool ranges, and block placement. | Very useful for less technical players. Check whether any overlays spoil intended mystery. |
| [Easy Block Renaming](https://steamcommunity.com/sharedfiles/filedetails/?id=2077166496) | Adds terminal controls for bulk replacing, prefixing, suffixing, and resetting block names. | Good admin/player hygiene for large bases, factions, and authored grids. | Mostly QoL; low campaign risk. |
| [Automatic Subgrid Renaming](https://steamcommunity.com/sharedfiles/filedetails/?id=3508737118) | Automatically renames rotor, hinge, and piston subgrids from the base grid name. | Keeps complex campaign grids readable after combat, salvage, or crashes. | Useful for debugging and player clarity. |
| [Paint Gun](https://steamcommunity.com/sharedfiles/filedetails/?id=500818376) | Adds a survival paint tool with block targeting, color/skin switching, picking, HUD info, and paint chemicals. | Makes repainting captured or repaired ships feel like an in-world tool rather than an admin action. | Good immersion fit. Validate recipe/balance and dependency behavior. |

## Audio, Visual Comfort, And Accessibility

| Mod | Summary | Campaign Use | Notes |
| --- | --- | --- | --- |
| [Sneaky Sounds - Quieter Tools](https://steamcommunity.com/workshop/filedetails/?id=1662953858) | Reduces loud tool, weather, door, placement, and similar sounds while keeping combat/explosions prominent. | Keeps long salvage and mining sessions easier to listen to. | Works for Arcade sound profile according to the Workshop page. |
| [Welder no eye damage](https://steamcommunity.com/workshop/filedetails/?id=3082535654) | Removes or reduces welder flashes, glare, and strobe effects. | Strong accessibility/QoL candidate for welding-heavy progression. | Important if Working Knowledge makes welding practice frequent. |
| [Long Range Searchlight](https://steamcommunity.com/sharedfiles/filedetails/?id=2741701803) | Increases vanilla searchlight, spotlight, floodlight, and offset spotlight ranges. | Better night exploration, POI scanning, and combat visibility. | Workshop comments mention recent definition warnings; verify before final stack. |

## Small Mechanical Tuning

| Mod | Summary | Campaign Use | Notes |
| --- | --- | --- | --- |
| [Powerful Small Rotors](https://steamcommunity.com/sharedfiles/filedetails/?id=3464224701) | Raises small-grid rotor torque to match other small-grid rotational blocks. | Helps small vehicles, salvage arms, crane rigs, and compact mechanisms feel less underpowered. | NoScripts block tweak. Balance impact should be modest but still test vehicle builds. |

## Watchlist

- `Advanced Welding` and `Improvised Experimentation` may need explicit compatibility testing with Working Knowledge research, Proficiency, salvage recovery, and block placement enforcement.
- `Long Range Searchlight` has useful campaign value, but recent comments mention definition warnings, so it should be tested before becoming a recommended dependency.
- MES should remain a framework choice; specific encounter packs should be selected later based on story tone and performance.
