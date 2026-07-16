# Working Knowledge Layer - ARC Truss Load Order Test

Disposable compatibility layer for testing ordered conflicts between two Working Knowledge layers.

This layer adds the Uncommon **Reinforced Truss Schematics** group (`test.arc.reinforced_truss`) and maps five ARC blocks plus five vanilla truss pillars into it. Every mapping intentionally overlaps `WKL-ARCTrussSystem`.

## Test Order A

1. Working Knowledge
2. ARC Truss System
3. WKL-ARCTrussSystem
4. WKL-ARCTrussSystemTest

Expected: Reinforced Truss Schematics wins the ten overlapping blocks.

## Test Order B

1. Working Knowledge
2. ARC Truss System
3. WKL-ARCTrussSystemTest
4. WKL-ARCTrussSystem

Expected: Truss Framework Schematics wins the same ten blocks because the main ARC layer is later.

In both orders, `/wk admin audit` should report ten multi-layer conflicts, name the later layer as winner, and group moved vanilla blocks under the winning destination. The exact test consumable is `WkKnSchematic_test_arc_reinforced_truss`, displayed as **Reinforced Truss Data Schematic**.
