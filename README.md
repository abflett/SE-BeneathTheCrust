# Space Engineers: Beneath the Crust

<p align="center">
  <img src="docs/assets/beneath-the-crust-hero.png" alt="A hard-sci-fi engineering outpost beside a glowing breach in an alien planet crust." />
</p>

**Beneath the Crust** is a vanilla-friendly Space Engineers campaign and mod workspace focused on exploration, salvage, research progression, POIs, and long-term survival purpose.

The project is being built as standalone mods first so each major system can be tested on its own before being folded into a larger campaign or scenario package.

## Project Status

- **Current playable layer:** `mods/WorkingKnowledge`
- **Current scenario tooling layer:** `mods/Worldwright`
- **Campaign state:** design and content foundation in `docs/`
- **Release target:** Working Knowledge `0.9.0` public feedback before campaign composition
- **Development style:** fresh test worlds, clean defaults, and standalone mod roots until `1.0.0`

## Current Focus: Working Knowledge

**Working Knowledge** is a technology progression overhaul for Space Engineers. It changes progression from a flat unlock tree into a salvage and reverse-engineering loop:

- Unknown blocks are locked behind schematic families.
- Grinding and salvaging unfamiliar technology grants research progress.
- Data fragments can reward partial schematic progress.
- Player Proficiency tracks hands-on skill separately from schematic knowledge.
- Low Proficiency makes welding, repairs, grinding, and salvage recovery rougher until the engineer improves.
- Research Pedestal and LCD apps expose player and faction progress in-game.

Research answers: **Do I know the schematic?**

Proficiency answers: **How well can I build, repair, grind, and salvage it?**

## Campaign Direction

Beneath the Crust keeps the core Space Engineers sandbox intact while adding stronger structure around:

- Discovery-based progression
- Salvage and reverse engineering
- Authored POIs and environmental storytelling
- Escalating machine/nanite threats
- Technology gates that reward exploration
- A larger campaign arc with a real ending

## Repository Map

- `.github/` - public issue templates for bugs and balance feedback.
- `docs/` - project documentation index, shared assets, campaign docs, and mod docs.
- `docs/BeneathTheCrust/` - campaign design, story, mod stack, and R&D notes.
- `docs/WorkingKnowledge/` - Working Knowledge maintainer, release, generated audit, and publishing docs.
- `docs/assets/` - reusable images for GitHub docs and future public documentation.
- `mods/` - standalone Space Engineers mod roots.
- `mods/WorkingKnowledge/` - the current playable Working Knowledge mod.
- `mods/Worldwright/` - standalone scenario-authoring tools for protected stations, tutorial bays, and staged authored spaces.
- `templates/` - copyable starter layouts for generated or hand-authored companion content.
- `tools/` - local generation, compile, and validation helpers.

## Main Docs

- [Campaign overview](docs/BeneathTheCrust/overview.md)
- [Campaign story foundation](docs/BeneathTheCrust/story_foundation.md)
- [Campaign mod shortlist](docs/BeneathTheCrust/mod_shortlist.md)
- [Standalone mods index](mods/README.md)
- [Working Knowledge overview](mods/WorkingKnowledge/README.md)
- [Working Knowledge changelog](docs/WorkingKnowledge/changelog.md)
- [Working Knowledge configuration](docs/WorkingKnowledge/configuration.md)
- [Working Knowledge implementation reference](docs/WorkingKnowledge/implementation.md)
- [Working Knowledge layer authoring](docs/WorkingKnowledge/layer_authoring.md)
- [Working Knowledge inspirations and attribution](docs/WorkingKnowledge/inspirations.md)
- [Working Knowledge release roadmap](docs/WorkingKnowledge/release_roadmap.md)
- [Working Knowledge Workshop description](docs/WorkingKnowledge/workshop_description_bbcode.txt)

## Feedback

Use GitHub issues for public bug reports and balance feedback:

- [Bug report](.github/ISSUE_TEMPLATE/bug_report.md)
- [Balance feedback](.github/ISSUE_TEMPLATE/balance_feedback.md)

## Local Testing

Deploy all standalone mods into the local Space Engineers mods folder:

```powershell
.\build.ps1
```

Deploy one standalone mod:

```powershell
.\build.ps1 -ModName WkKn
.\build.ps1 -ModName Ww
```

Convenience wrappers are also available:

```powershell
.\build-workingknowledge.ps1
.\build-worldwright.ps1
```

The local Working Knowledge test copy deploys as `%APPDATA%\SpaceEngineers\Mods\Working Knowledge`. The local Worldwright test copy deploys as `%APPDATA%\SpaceEngineers\Mods\Worldwright`. Experimental Mode is not required.

After changing mod C# scripts, compile before deploying:

```powershell
.\tools\compile-mod-scripts.ps1 -ModName WkKn
.\tools\compile-mod-scripts.ps1 -ModName Ww
```
