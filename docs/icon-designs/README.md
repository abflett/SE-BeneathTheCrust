# Working Knowledge Icon Designs

This folder holds the exploratory concepts and production sources for Working Knowledge's research-data icons.

## Version 1

- `wk-data-fragment-v1.png` - incomplete salvaged research data, represented by a physically broken data wafer.
- `wk-schematic-family-unlocker-v1.png` - the hidden progression unlocker for an entire schematic family, represented by an intact indexed archive cartridge.
- `wk-exact-data-schematic-v1.png` - one complete durable exact schematic, represented by a sealed blueprint cartridge.

The concepts were generated with the supplied in-game progression-tree screenshot as the primary context reference and the vanilla Datapad, Basic Assembler, Grinder, and Lab Equipment icons as rendering references. The shared direction was a single isolated utilitarian object in a simple three-quarter view, subdued pale blue-gray materials, low saturation, restrained highlights, modest ambient occlusion, and a silhouette that remains readable at small G-menu sizes. Neon color, emissive screens, elaborate frames, cinematic lighting, and detailed holographic interfaces were explicitly excluded.

These PNGs are large RGB design studies with black backgrounds. They are not transparent, downscaled, compressed, or converted to the `.dds` format required for production use. Keep later explorations under versioned names so individual designs can be retained or discarded without overwriting earlier comparisons.

The reproducible generation instructions are preserved in `prompts-v1.md`.

## Exact Data Schematic Version 2

`wk-exact-data-schematic-v2.png` keeps the physical datapad intact and fills its display with dense, orderly technical information. It began as the second Data Fragment study, but its healthy data bars and complete-looking interface read more naturally as an Exact Data Schematic at small icon sizes. Its original generation instructions are preserved in `prompts-v2.md`.

## Data Fragment Version 3

`wk-data-fragment-v3.png` keeps the same intact physical datapad but makes the missing information unmistakable at icon size. Only a small schematic cluster survives; large pixel blocks, missing quadrants, torn scan regions, and coarse monochrome noise dominate the rest of the display. The exact edit instructions are preserved in `prompts-v3.md`.

## Production Assets

The approved Data Fragment v3, Exact Data Schematic v2, and Schematic Family Unlocker v1 concepts were converted into the transparent 128x128 PNG sources under `production/`. Their connected outer black backgrounds were removed without erasing the intentionally dark screen interiors, their footprints and blue-gray grading were matched to the vanilla Datapad icon, and the production PNGs were encoded as 128x128 BC7 sRGB DDS files with full mip chains under `mods/WorkingKnowledge/Textures/GUI/Icons/Items/`.
