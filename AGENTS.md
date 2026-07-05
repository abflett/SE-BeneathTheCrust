# AGENTS.md

Guidance for AI coding assistants and maintainers working in this repository.

## Project Context

**Space Engineers: Beneath the Crust** is a Space Engineers campaign/mod workspace. The project is being built as standalone mods first, then later composed into a larger campaign or scenario package.

The current main mod is `mods/WorkingKnowledge`, a standalone research and progression overhaul that will later be usable as one layer of the campaign.

## Where To Start

- Read `README.md` for the repo map and current focus.
- Read `docs/working_knowledge_implementation.md` before changing Space Engineers `.sbc` files, generated research data, or mod script structure.
- Read `mods/WorkingKnowledge/README.md` before changing the Working Knowledge mod.
- Keep campaign design, planning, and maintainer notes in `docs/`.
- Keep each standalone mod folder focused on loadable Space Engineers content plus a short local README.

## Working Rules

- Preserve each folder under `mods/` as an independently loadable Space Engineers mod root unless a maintainer explicitly asks to compose them.
- Keep `Data/` limited to files Space Engineers should load. Put design notes, generated audits, and planning material under `docs/`.
- Prefer current Space Engineers syntax from local game files or official/current sources over old boilerplate.
- Use existing generators for generated data when possible; if generated outputs are edited directly, document why in the change summary.
- After changing mod C# scripts, run `.\tools\compile-mod-scripts.ps1 -ModName WkKn` from the repo root.
- Use `.\build.ps1 -ModName WkKn` only when a local deploy to `%APPDATA%\SpaceEngineers\Mods\Working Knowledge` is needed for testing.
- During pre-v1 development of `mods/WorkingKnowledge`, do not add legacy save migration, version-merging, or old-config fallback logic unless a maintainer explicitly asks for it. Assume frequent fresh test worlds and prefer current clean defaults until the initial `1.0.0` release is declared.
- Do not commit or push unless a maintainer explicitly asks for that action.
- Do not invent campaign details beyond the current docs unless the task is explicitly design work.

## Public Repo Hygiene

- Do not add local tool installs, scratch output, deployed mod copies, or downloaded archives. `tools/MWMBuilder/` and `.tmp/` should stay untracked.
- Do not add secrets, private paths, personal machine details, or private repository history.
- Keep binary assets covered by `.gitattributes`; models, images, audio, and texture assets should not be treated as text.
