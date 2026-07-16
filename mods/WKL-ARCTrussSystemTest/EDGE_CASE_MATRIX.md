# ARC Truss Layer Edge-Case Matrix

These disposable variants exercise layer priority, invalid declarations, wiring collisions, malformed input, and safe fallback behavior for Working Knowledge 0.13.0.

All lists below use the normal Space Engineers **Active Mods** display order: the top entry has the highest priority and wins valid conflicts. The toolkit validator accepts `-LayerPath` values in the opposite direction, from lowest to highest priority.

Keep **ARC Truss System** below its Working Knowledge layers and **Working Knowledge** at the bottom of these focused test stacks.

## Focused Tests

### 1. Valid Same-ID Rename

Active Mods, top to bottom:

1. WKL ARC Variant - Valid Rename
2. WKL-ARCTrussSystem
3. ARC Truss System
4. Working Knowledge

Expected:

- `arc.truss_framework` remains the stable save ID
- its visible name becomes **Advanced Truss Network Schematics**, its tier becomes Rare, and its exact item is renamed
- the audit reports a same-ID redefinition and names the rename variant as the winner
- reversing the top two entries restores the main layer's Common-tier name and item

### 2. Invalid Same-ID Redefinition

Active Mods, top to bottom:

1. WKL ARC Variant - Broken Redefinition
2. WKL-ARCTrussSystem
3. ARC Truss System
4. Working Knowledge

Expected:

- the higher-priority declaration is rejected because `WkKnLayer_ARCTruss_missing` and `WkKnUnlocker_ARCTruss_missing` do not exist
- the main layer remains the highest valid declaration of `arc.truss_framework`
- `TrussPillarT` resolves through the valid main group
- the audit reports an error for the rejected declaration, plus warnings for the same-ID and block-mapping conflicts; world loading continues

### 3. Bad Mapping Lines and Targets

Active Mods, top to bottom:

1. WKL ARC Variant - Bad Mappings
2. WKL-ARCTrussSystemTest
3. WKL-ARCTrussSystem
4. ARC Truss System
5. Working Knowledge

Expected:

- unknown group `test.arc.does_not_exist` is rejected and a lower valid claim for `TrussPillarCorner` wins
- malformed `TrussPillarDiagonal` input is reported and ignored
- nonexistent `Arc_Truss_NotReal` is reported as not matching a loaded public block
- the final local claim for `TrussPillarX` wins and assigns it to `structure.industrial`
- the offline validator fails deliberately at the malformed line, while the in-game loader continues and audits the remaining recoverable problems

### 4. Shared Wiring Collision Wins

Active Mods, top to bottom:

1. WKL ARC Variant - Wiring Collision
2. WKL-ARCTrussSystem
3. ARC Truss System
4. Working Knowledge

Expected:

- `test.arc.shared_wiring` owns the shared vanilla group and unlocker subtype IDs
- `arc.truss_framework` becomes inactive because a higher-priority group owns its wiring
- `Arc_Truss_I` and `TrussPillar` use **Shared Wiring Truss Schematics**
- other mappings that depended only on the rejected main group fall back to lower valid assignments
- the audit warns that the main group is inactive because the test group owns its shared definition IDs, and reports an error for the missing exact schematic item

### 5. Shared Wiring Collision Loses

Active Mods, top to bottom:

1. WKL-ARCTrussSystem
2. WKL ARC Variant - Wiring Collision
3. ARC Truss System
4. Working Knowledge

Expected:

- `arc.truss_framework` owns its wiring and behaves normally
- `test.arc.shared_wiring` and both of its mapping claims are skipped
- the audit warns that the test group is inactive because the main layer owns its shared definition IDs

## Combined Stress Test

Active Mods, top to bottom:

1. WKL ARC Variant - Bad Mappings
2. WKL ARC Variant - Wiring Collision
3. WKL ARC Variant - Broken Redefinition
4. WKL ARC Variant - Valid Rename
5. WKL-ARCTrussSystemTest
6. WKL-ARCTrussSystem
7. ARC Truss System
8. Working Knowledge

Expected high-level result:

- the bad-mappings layer reports its malformed line, unknown target, nonexistent block, and duplicate claims without crashing the session
- the broken same-ID declaration is skipped; the valid rename is the highest valid `arc.truss_framework` declaration before wiring ownership is considered
- the higher-priority wiring-collision group owns the shared wiring, so `arc.truss_framework` is ultimately inactive
- **Reinforced Truss Schematics** remains active for its valid claims except where a higher-priority valid mapping wins
- `TrussPillar` is owned by **Shared Wiring Truss Schematics**, and `TrussPillarX` returns to `structure.industrial`
- the concise chat audit may truncate the long warning list; use the F11 audit view or log for the full report

This is intentionally a noisy runtime test. Do not publish these variants or add them to the normal release-validation suite.
