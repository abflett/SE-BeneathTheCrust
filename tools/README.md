# Tools

Utility scripts for maintaining the Space Engineers mod assets live here.

- `compile-mod-scripts.ps1` compiles a mod's `Data/Scripts/**/*.cs` files against the local Space Engineers `Bin64` assemblies for a fast syntax/API check.
- `generate-working-knowledge-balance.ps1` derives Working Knowledge schematic work-reward balance from vanilla blueprint/component/block data, writes generated audit reports under `docs/WorkingKnowledge/generated/`, and updates `SchematicWorkRewardTable.cs` when run with `-UpdateSource`.
- `generate-working-knowledge-data.ps1` rebuilds generated Working Knowledge research data from game definitions.
- `WorkingKnowledgeLayerToolkit/` is the standalone package for creating Working Knowledge Layer compatibility mods. It is intended to be zip-distributed for users outside this repository.
- `package-working-knowledge-layer-toolkit.ps1` builds and validates the versioned standalone toolkit archive. Follow the [toolkit release checklist](../docs/WorkingKnowledge/toolkit_release.md) before uploading it.
- `inspect-space-engineers-block-mod.ps1` scans a local block mod folder and lists public `Type/Subtype` block IDs for Working Knowledge layer authoring.
- `new-working-knowledge-layer.ps1` is the older repository-maintenance generator for creating a layer from a block ID list. Prefer the toolkit for user-facing workflows.
- `install-mwmbuilder.ps1` installs the local Stollie MWMBuilder runtime into `tools/MWMBuilder/`.
- `rebuild-research-scifi-terminal.ps1` rebuilds the Research Sci-Fi Terminal model from the Space Engineers ModSDK source FBX/XML/HKT and cleans its staging folder on success. Use `-KeepScratch` when inspecting generated FBX/MWMBuilder output.
- `inspect-scifi-terminal-model.ps1` prints LCD material, mesh, UV, and MWM marker diagnostics for the Sci-Fi Terminal model.

`tools/MWMBuilder/` is intentionally ignored because it is a local tool install. `.tmp/` is disposable build scratch space and should normally be empty between tool runs.
