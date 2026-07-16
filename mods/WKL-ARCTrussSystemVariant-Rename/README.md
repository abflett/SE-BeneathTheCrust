# WKL ARC Variant - Valid Rename

This test layer validly redeclares the existing `arc.truss_framework` save ID while retaining its vanilla research wiring.

When this layer has higher priority than `WKL-ARCTrussSystem`, expect:

- the group to display as **Advanced Truss Network Schematics** at Rare tier
- a same-ID group-redefinition warning naming this layer as winner
- one overlapping block-mapping warning for `CubeBlock/Truss`
- the exact item to display as **Advanced Truss Network Data Schematic**
- existing research and Proficiency under `arc.truss_framework` to retain the same identity

When the main ARC layer has higher priority, expect its original Common-tier name and item display instead.
