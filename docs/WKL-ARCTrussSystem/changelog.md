# Working Knowledge Layer - ARC Truss System Changelog

Public-facing release notes for **Working Knowledge Layer - ARC Truss System**.

## 1.0.1 - Workshop Thumbnail Update

Updated the Workshop thumbnail and publishing metadata.

No block mappings, research definitions, progression behavior, or balance values changed in this release.

## 1.0.0 - Initial Compatibility Layer

Initial stable release for using **ARC Truss System** blocks with **Working Knowledge** progression.

This layer adds vanilla research block definitions for ARC Truss System blocks, then maps those blocks into Working Knowledge schematic families:

- ARC truss structure blocks use `Industrial Structure Schematics`.
- The ARC truss light uses `Interior Lighting Schematics`.

When loaded with Working Knowledge and ARC Truss System, these blocks participate in normal Working Knowledge research, unlock, Proficiency, construction, repair, grinding, and salvage behavior.

This layer does not add new blocks, models, recipes, or balance rules. It only connects ARC Truss System definitions to Working Knowledge progression.
