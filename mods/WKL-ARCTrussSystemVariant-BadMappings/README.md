# WKL ARC Variant - Bad Mappings

This deliberately problematic layer tests four independent mapping mistakes without replacing any schematic-group definition.

Load it above `WKL-ARCTrussSystemTest` and `WKL-ARCTrussSystem`. Expected behavior:

- `CubeBlock/TrussPillarCorner` targets an unknown group, so that claim is skipped and a lower valid mapping can win
- the `TrussPillarDiagonal` line uses invalid syntax, so the runtime loader warns and ignores it
- `CubeBlock/Arc_Truss_NotReal` is declared as a research block but does not exist in the loaded public block definitions, so binding warns and skips it
- `CubeBlock/TrussPillarX` is claimed twice; the last claim in this higher-priority layer wins and moves it back to `structure.industrial`

The offline toolkit validator intentionally stops at the malformed line and reports its file and line number. The in-game loader is expected to continue safely and report all applicable issues in `/wk admin audit`, the F11 audit view, and the log.

This mod is deliberately invalid and is not part of normal release validation.
