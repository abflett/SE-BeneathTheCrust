# Standalone Mods

[Campaign README](../README.md)

Each folder under `mods/` is intended to be a standalone Space Engineers mod root.

Keep each mod independently loadable while this project grows. Campaign/scenario composition should happen later through a separate scenario layer or documented mod pack, not by making the early standalone mods depend on each other unnecessarily.

## Current mods

- [WorkingKnowledge/](WorkingKnowledge/README.md) - schematic research, salvage-based learning, and hands-on Proficiency for welding, grinding, botches, and recovery quality. It deploys locally as `Working Knowledge`.

## Local deploy

Run `.\build.ps1` from the repo root to deploy standalone mod folders to `%APPDATA%\SpaceEngineers\Mods` for in-game testing.
