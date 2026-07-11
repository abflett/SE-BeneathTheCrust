# Worldwright

[Campaign README](../../README.md) | [Standalone mods index](../README.md)

**Worldwright** is a standalone Space Engineers scenario-authoring toolkit.

It provides small, reusable tools for builders who want stronger control over tutorial spaces, stations, reclamation bays, and scripted scenario moments without making those tools specific to Beneath the Crust.

## Current Features

- Protected grids can block grinder damage without using a safe zone.
- Separate scrap or tutorial grids remain grindable if they are not protected.
- Protection is server-side and applies to normal grinder damage.
- Reclamation Spawners create ordered one-block dynamic grids for authored bays and scenario machinery.
- Spawner sequences support duplicate entries, search across loaded public blocks, and Once, Loop, or Random selection.
- Timer blocks and other vanilla automation can use the spawner's `Spawn Next` and `Reset Sequence` toolbar actions.

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

## Reclamation Spawner

The **Worldwright Reclamation Spawner** is a large-grid terminal block shaped like a Light Armor Half Block. Its recessed side is the output side. It has no physical control screen, so open it from another terminal on the same construct.

The terminal provides:

- A search field and filtered list of every loaded public cube-block definition.
- An ordered spawn sequence with Add, Remove, Move Up, and Move Down controls.
- `Once`, `Loop`, and `Random` sequence modes.
- A `Spawn Next` button and toolbar action.
- A `Reset Sequence` button and toolbar action.
- An outward velocity slider from `0` to the vanilla grid speed of `100 m/s`.

Each list entry represents one spawn. Add the same block three times when it should appear three times. Duplicate entries also act as weighting in Random mode.

`Spawn Next` requests one grid containing one fully built, unowned block. If the required volume is occupied, the request waits until the area clears. Repeated requests do not build an invisible queue; each spawner holds at most one waiting request.

The new grid inherits the source grid's current linear velocity, then adds the configured velocity away from the recessed face. Worldwright deliberately does not clamp the resulting vector so local testing can show how vanilla world physics handles launches from a moving grid.

Spawner configuration is stored in the block's Custom Data under `[Worldwright.ReclamationSpawner]`. Worldwright preserves other Custom Data sections when updating this configuration.

## Planned Direction

Future Worldwright features may include trigger volumes, staged prefab or blueprint drops, scenario reset helpers, and other reusable tools for authored Space Engineers experiences.
