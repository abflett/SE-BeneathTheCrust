# Standalone Mods

[Campaign README](../README.md)

Each folder under `mods/` is intended to be a standalone Space Engineers mod root.

Keep each mod independently loadable while this project grows. Campaign/scenario composition should happen later through a separate scenario layer or documented mod pack, not by making the early standalone mods depend on each other unnecessarily.

Do not put templates or draft layer starters here. Use the [Working Knowledge Layer Toolkit](../tools/WorkingKnowledgeLayerToolkit/README.md) and [Working Knowledge layer authoring](../docs/WorkingKnowledge/layer_authoring.md) when creating a new compatibility layer, then move or generate the finished mod root under `mods/`.

## Current mods

- [WorkingKnowledge/](WorkingKnowledge/README.md) - schematic research, salvage-based learning, and hands-on Proficiency for welding, grinding, botches, and recovery quality. It deploys locally as `Working Knowledge`.
- [Worldwright/](Worldwright/README.md) - standalone scenario-authoring tools for protected stations, tutorial bays, and staged authored spaces. It deploys locally as `Worldwright`.
- [WKL-ARCTrussSystem/](WKL-ARCTrussSystem/README.md) - Working Knowledge compatibility layer for ARC Truss System blocks.

## Local deploy

Run `.\build.ps1` from the repo root to deploy all standalone mod folders to `%APPDATA%\SpaceEngineers\Mods` for in-game testing.

Use `.\build-workingknowledge.ps1` or `.\build-worldwright.ps1` to deploy one mod directly.
