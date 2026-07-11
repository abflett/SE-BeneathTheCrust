# Worldwright Changelog

Public-facing release notes for the standalone **Worldwright** Space Engineers scenario-authoring toolkit.

## 0.2.0 - Reclamation Spawner

This development release adds the first Worldwright Reclamation Spawner for authored salvage bays, tutorials, traps, and other scenario machinery.

- Adds a large-grid spawner using the vanilla Light Armor Half Block shape. The recessed side marks the output direction.
- Searches all loaded public cube-block definitions, including blocks supplied by other loaded mods.
- Stores an ordered sequence where duplicate entries produce repeated spawns.
- Supports Once, Loop, and weighted Random sequence modes.
- Adds Add, Remove, Move Up, and Move Down sequence controls.
- Adds `Spawn Next` and `Reset Sequence` terminal buttons and toolbar actions for vanilla automation.
- Waits for the complete next-block volume to clear instead of relying on a fixed cooldown.
- Allows one pending spawn per spawner so repeated automation signals cannot create a hidden queue.
- Spawns each entry as a fully built, unowned, dynamic one-block grid.
- Adds a `0-100 m/s` outward velocity setting. Spawned grids inherit the source grid velocity before the configured outward velocity is applied.
- Persists configuration on the spawner block while preserving unrelated Custom Data sections.

Current testing boundaries:

- Only individual block definitions are supported. Prefab and blueprint payloads remain future work.
- Pending spawn requests are runtime state and are cancelled by a session reload or sequence edit.
- Vanilla physics remains responsible for the final world-space grid speed.
- The initial release provides a large-grid spawner only.

## 0.1.0 - Initial Protected Grid Tools

Initial development release for testing Worldwright as a standalone scenario-authoring helper.

This release adds server-side grinder protection for authored grids. A grid can be protected either by adding `G-PROT` to the grid name or by adding this marker to any terminal block's Custom Data:

```ini
[Worldwright]
protected=true
```

Protected grids cancel normal grinder damage before it is applied. This is intended for stations, tutorial rooms, spawn spaces, protected infrastructure, and authored locations that players should be able to use without dismantling.

Separate scrap, tutorial, or reclamation grids remain grindable when they are not marked as protected.

Current scope:

- Blocks normal grinder damage against protected grids.
- Works server-side through the Space Engineers damage system.
- Supports a simple grid-name marker for builders and scenario tooling.
- Supports a Custom Data marker for cleaner authored setups.
- Does not require a safe zone.

Known boundaries:

- Does not block welding, movement, terminal access, ownership changes, collisions, weapon damage, or other damage types.
- Does not yet include reclamation bays, trigger volumes, staged prefab drops, reset helpers, or other planned scenario tools.
