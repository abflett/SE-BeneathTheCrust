# Space Engineers: Beneath the Crust

**Space Engineers: Beneath the Crust** is a campaign and mod workspace for Space Engineers.

The long-term goal is a vanilla-friendly campaign with stronger exploration, salvage, research, POIs, and endgame purpose. The project is being built as standalone mods first so each major system can be tested on its own before being folded into the larger campaign.

## Current Focus

The active mod is **Working Knowledge**, a technology progression overhaul in `mods/WorkingKnowledge`.

Working Knowledge changes progression from a flat unlock tree into a salvage and reverse-engineering loop:

- Unknown blocks are locked behind schematic families.
- Grinding and salvaging unfamiliar technology grants research progress.
- Data fragments can reward partial schematic progress.
- Player Proficiency tracks hands-on skill separately from schematic knowledge.
- Low Proficiency makes welding, repairs, grinding, and salvage recovery rougher until the engineer improves.
- Research Pedestal and LCD apps expose player and faction progress in-game.

## Repository Map

- `docs/` - durable campaign, mod, admin, and publishing docs.
- `docs/assets/` - reusable images for GitHub docs and future public documentation.
- `.github/` - public issue templates for bugs and balance feedback.
- `mods/` - standalone Space Engineers mod roots.
- `mods/WorkingKnowledge/` - the current playable Working Knowledge mod.
- `tools/` - local generation, compile, and validation helpers.

## Main Docs

- [Campaign overview](docs/campaign_overview.md)
- [Campaign story foundation](docs/campaign_story_foundation.md)
- [Campaign mod shortlist](docs/campaign_mod_shortlist.md)
- [Working Knowledge overview](mods/WorkingKnowledge/README.md)
- [Working Knowledge configuration](docs/working_knowledge_configuration.md)
- [Working Knowledge implementation reference](docs/working_knowledge_implementation.md)
- [Working Knowledge release readiness checklist](docs/working_knowledge_v1_release_checklist.md)
- [Working Knowledge Workshop page draft](docs/working_knowledge_workshop_page.md)

## Feedback

Use GitHub issues for public bug reports and balance feedback:

- [Bug report](.github/ISSUE_TEMPLATE/bug_report.md)
- [Balance feedback](.github/ISSUE_TEMPLATE/balance_feedback.md)

## Local Testing

Use the root build script to deploy the active mod into the local Space Engineers mods folder:

```powershell
.\build.ps1 -ModName WkKn
```

The local Working Knowledge test copy deploys as `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`. Experimental Mode is not required.

After changing Working Knowledge C# scripts, compile before deploying:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
```
