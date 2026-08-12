# Working Knowledge Inspirations

Last updated: 2026-08-13.

This page records external Space Engineers mod ideas and framework integrations that helped shape **Working Knowledge**.

Most entries are acknowledgements of design inspiration only. Direct dependencies or bundled integration wrappers are called out separately.

## Credited Inspirations

| Mod | Public reference | Inspiration | Working Knowledge direction |
| --- | --- | --- | --- |
| Grind To Learn 3 | [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3207654953) | The clear survival fantasy that taking unfamiliar blocks apart can teach the player how to build them. | Working Knowledge kept the salvage and reverse-engineering feel, then diverged into schematic families, fractional research, Data Fragments, Data Schematics, faction sync, vanilla progression integration, and separate hands-on Proficiency. |
| ZControlPanel+ | [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=2908185563) | The idea that the vanilla control panel could become a more useful visible LCD/control surface instead of only a small access port. | Working Knowledge extends that console and LCD direction through selectable LCD apps, the Research Pedestal workflow, and the Research Sci-Fi Terminal with baked-in research and Proficiency interfaces. |

## Framework Integrations

| Mod | Public reference | Use | Working Knowledge direction |
| --- | --- | --- | --- |
| Rich HUD Master / Rich HUD Framework | [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=1965654081), [client source](https://github.com/ZachHembree/RichHudFramework.Client) | Shared high-DPI HUD rendering and graphical settings interface used by the 1.1.0 migration. Working Knowledge bundles the official Full Client `1.3.0.0` source under its MIT license and expects Rich HUD Master at runtime. | Renders compact research and Proficiency progress rows and exposes player/admin configuration pages. Core progression and chat commands remain independent of the framework. |

## Attribution Boundaries

- Credited inspirations are not dependencies.
- Framework integrations may add optional or required companion-mod behavior; document those cases in this file, release notes, and Workshop copy.
- Working Knowledge should keep core progression playable without companion HUD/framework mods where practical.
- Do not copy code, models, textures, icons, namespaced definitions, or other assets from external mods into this repository without explicit permission and a documented license/attribution decision.
- If a future feature becomes more directly derived from an external mod, add that detail here and to the Workshop description before release.
