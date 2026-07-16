# WKL ARC Variant - Broken Redefinition

This intentionally incomplete test layer redeclares the existing `arc.truss_framework` save ID but names research wiring that does not exist.

Place it above `WKL-ARCTrussSystem` in the normal in-game Active Mods list. Expected behavior:

- the invalid higher-priority group declaration is skipped
- the main ARC layer remains the highest valid declaration and retains **Truss Framework Schematics**
- `CubeBlock/TrussPillarT` still resolves through that valid group
- the audit reports an error for the rejected declaration because its group and unlocker definitions are missing, plus warnings for the same-ID and block-mapping conflicts
- the world continues loading instead of losing the valid lower-priority group

This mod is deliberately invalid by itself and is not part of normal release validation.
