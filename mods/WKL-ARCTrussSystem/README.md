# Working Knowledge Layer - ARC Truss System

Compatibility layer for using ARC Truss System blocks with Working Knowledge progression.

Version: `1.1.0`

Load order:

1. Working Knowledge
2. ARC Truss System
3. Working Knowledge Layer - ARC Truss System

This mod provides vanilla research definitions for ARC Truss System blocks and adds a Common-tier **Truss Framework Schematics** group (`arc.truss_framework`).

The group contains:

- all ARC Truss System blocks, including its integrated light
- the vanilla truss block family, including truss lights, decoy, and ladder

The vanilla truss blocks are intentionally moved out of Working Knowledge's broader Industrial Structure Schematics group when this layer loads after Working Knowledge. Catwalks, stairs, platforms, railings, and generic beam blocks remain Industrial Structure Schematics.

The matching durable consumable is `WkKnSchematic_arc_truss_framework`, displayed in game as **Truss Framework Data Schematic**. Common Data Fragments can also select incomplete Truss Framework research.

Public docs:

- [Workshop description](../../docs/WKL-ARCTrussSystem/workshop_description_bbcode.txt)
- [Changelog](../../docs/WKL-ARCTrussSystem/changelog.md)
