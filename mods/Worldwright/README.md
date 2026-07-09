# Worldwright

[Campaign README](../../README.md) | [Standalone mods index](../README.md)

**Worldwright** is a standalone Space Engineers scenario-authoring toolkit.

It provides small, reusable tools for builders who want stronger control over tutorial spaces, stations, reclamation bays, and scripted scenario moments without making those tools specific to Beneath the Crust.

## Current Features

- Protected grids can block grinder damage without using a safe zone.
- Separate scrap or tutorial grids remain grindable if they are not protected.
- Protection is server-side and applies to normal grinder damage.

Public docs:

- [Workshop description](../../docs/Worldwright/workshop_description_bbcode.txt)
- [Changelog](../../docs/Worldwright/changelog.md)

## Grind Protection

Worldwright protects a grid when either condition is true:

- The grid name contains `G-PROT`, such as `Special Station G-PROT`.
- Any terminal block on the grid has this in Custom Data:

```ini
[Worldwright]
protected=true
```

Protected grids cancel grinder damage before it is applied. This is intended for stations, tutorial rooms, spawn areas, and scenario infrastructure that players should be able to use but not dismantle.

The name token is intentionally simple so scenario scripts, admin tools, or builders can add or remove protection by renaming the grid.

This is not a safe zone replacement. It does not block movement, welding, terminal access, weapon damage, collisions, or ownership changes.

## Planned Direction

Future Worldwright features may include reclamation-bay scrap spawning, trigger volumes, staged prefab drops, scenario reset helpers, and other reusable tools for authored Space Engineers experiences.
