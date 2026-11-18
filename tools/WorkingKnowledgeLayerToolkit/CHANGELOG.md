# Working Knowledge Layer Toolkit Changelog

## 1.3.0 - Warfare 3 Catalog And Stat-Free Schematics

- Added an explicit MIT license for the standalone toolkit.
- Updated the built-in catalog to all 58 active Working Knowledge families, including the new `prototech.microwave_emitter` family shared by large- and small-grid emitters.
- Kept Industrial Access and the existing stable family IDs, display names, and tiers synchronized with Working Knowledge 1.2.x for Space Engineers 1.211 Warfare 3.
- Removed the unintended radiation immunity effect from generated custom Data Schematics and the included examples. Items remain stat-free `ConsumableItem` definitions with the existing use sound and durable research behavior.
- Custom hidden unlockers remain public and registered for the progression hierarchy; Working Knowledge 1.2.0 hides their loaded build-list entries at runtime.
- Validated catalog parity, generator self-tests, example definitions, maintained layers, priority resolution, and standalone archive extraction.

## 1.2.0 - Industrial Access Group Catalog

- Added the built-in `structure.industrial_access` group for catwalks, stairs, ladders, ramps, railings, and other open industrial access structures.
- Updated group-selection guidance so framework blocks remain under Industrial Structure while personnel-access blocks use Industrial Access.
- Added release validation that requires the Toolkit to expose every active built-in Working Knowledge group with matching names and tiers, including all Prototech families.
- Suppressed expected warning and error diagnostics from isolated negative-test fixtures so release output reports the fixture phase clearly without resembling production failures.
- Excluded the repository-only `Tests` directory from downloadable Toolkit archives while retaining those checks in the source release workflow.

## 1.1.0 - Custom Groups And Priority Validation

[Download Working Knowledge Layer Toolkit 1.1.0](https://github.com/abflett/SE-BeneathTheCrust/releases/download/v1.1.0/WorkingKnowledgeLayerToolkit-1.1.0.zip) or read the [published release notes](https://github.com/abflett/SE-BeneathTheCrust/releases/tag/v1.1.0).

- Added complete custom schematic-group generation, including fragment-tier metadata, hidden unlockers, and exact Data Schematic consumables.
- Added built-in remapping and ordered layer-conflict validation with clear priority wording.
- Documented the Working Knowledge in-game audit output for moved blocks, winners, skipped claims, warnings, and errors.
- Improved scanning and selection for covered-only block sets and made outlier actions consistently numbered.
- Added generator self-tests and corrected single-group and zero-group validation edge cases.
- Added publishing guidance and repeatable validation of the standalone release archive.

## 1.0.0 - Initial Release

- Added interactive block-mod scanning, mapping generation, offline validation, an example layer, and authoring documentation.
