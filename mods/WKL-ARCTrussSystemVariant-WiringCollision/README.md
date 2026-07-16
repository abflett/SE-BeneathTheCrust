# WKL ARC Variant - Wiring Collision

This test layer declares a different save ID, `test.arc.shared_wiring`, while deliberately reusing the main ARC layer's vanilla group and unlocker subtype IDs. It also deliberately omits its exact data-schematic item.

Load it with `WKL-ARCTrussSystem`; it is not valid as a standalone layer.

When this layer has higher priority:

- `test.arc.shared_wiring` owns the shared wiring and **Shared Wiring Truss Schematics** becomes active
- the main `arc.truss_framework` group is rejected because its wiring is already owned
- this layer's two blocks use the new group, while other main-layer-only mappings fall back to lower valid mappings
- the audit warns that the main group is inactive because this group owns its shared definition IDs, and reports an error for the missing exact data-schematic item

When `WKL-ARCTrussSystem` has higher priority:

- the main group owns the shared wiring
- this group and both of its mappings are skipped
- the main ARC behavior remains active
- the audit warns that this group is inactive because the main layer owns its shared definition IDs

This mod is deliberately invalid by itself and is not part of normal release validation.
