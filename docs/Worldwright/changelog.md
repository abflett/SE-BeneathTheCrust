# Worldwright Changelog

Public-facing release notes for the standalone **Worldwright** Space Engineers scenario-authoring toolkit.

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
