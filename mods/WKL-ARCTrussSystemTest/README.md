# Working Knowledge Layer - ARC Truss Load Order Test

Disposable compatibility layer for testing ordered conflicts between two Working Knowledge layers.

For the intentionally invalid and conflicting companion variants, see [EDGE_CASE_MATRIX.md](EDGE_CASE_MATRIX.md).

This layer adds the Uncommon **Reinforced Truss Schematics** group (`test.arc.reinforced_truss`) and maps five ARC blocks plus five vanilla truss pillars into it. Every mapping intentionally overlaps `WKL-ARCTrussSystem`.

## Test Priority A: Reinforced Wins

Normal in-game Active Mods list, top to bottom:

1. WKL-ARCTrussSystemTest
2. WKL-ARCTrussSystem
3. ARC Truss System
4. Working Knowledge

Expected: Reinforced Truss Schematics wins the ten overlapping blocks.

## Test Priority B: Framework Wins

Normal in-game Active Mods list, top to bottom:

1. WKL-ARCTrussSystem
2. WKL-ARCTrussSystemTest
3. ARC Truss System
4. Working Knowledge

Expected: Truss Framework Schematics wins the same ten blocks because the main ARC layer has higher priority.

Space Engineers loads the normal in-game list from bottom to top, so the higher visible entry has higher priority and is applied later. In both arrangements, `/wk admin audit` should report ten multi-layer conflicts, name the higher-priority layer as winner, and group moved vanilla blocks under the winning destination. The exact test consumable is `WkKnSchematic_test_arc_reinforced_truss`, displayed as **Reinforced Truss Data Schematic**.
