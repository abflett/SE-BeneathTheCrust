# Publishing A Layer

Use this checklist after the generated layer works locally and before making it public.

## 1. Review The Generated Mod

Give the layer a clear name such as `Working Knowledge Layer - Source Mod Name`. Review `modinfo.sbc`, `README.md`, and the starter copy under `Publishing/`.

Do not include the source mod's blocks, models, textures, or recipes. A layer should contain only its own research definitions, Working Knowledge metadata, and publishing notes.

If the layer defines custom groups, treat these values as permanent after release:

- Schematic group ID
- Research group subtype
- Unlocker subtype
- Exact Data Schematic subtype derived from the group ID

Names, descriptions, and tiers may be updated by a higher-priority declaration. Stable IDs should not be renamed because saves and other layers may refer to them.

## 2. Run Offline Validation

From the toolkit folder, run:

```powershell
.\Validate.ps1 -LayerPath "C:\Path\To\YourLayer"
```

If the layer is intended to override another layer, validate the stack from lowest to highest priority:

```powershell
.\Validate.ps1 -LayerPath @("C:\Path\To\LowerLayer", "C:\Path\To\YourHigherLayer")
```

The last path has the highest numeric priority. Fix all errors and understand every warning before continuing.

Offline validation checks the layer's syntax, research entries, custom-group wiring, exact schematic definitions, collisions, and predicted winners. It cannot prove that a separately loaded source mod still defines every referenced block.

## 3. Test A Fresh World

Use this normal Active Mods order, shown top to bottom with highest priority first:

1. Your layer
2. The source block mod
3. Working Knowledge

Test at least the following:

- The world loads without F11 errors.
- Every representative source block appears in the intended schematic group.
- Intended remaps win and unrelated Working Knowledge blocks remain unchanged.
- A custom group appears in research and Proficiency displays.
- Its matching tier can yield a Data Fragment, unless its tier is `None`.
- Its exact Data Schematic consumable exists, unlocks that group, and is returned after use.
- Construction, repair, grinding, and salvage follow the assigned family.
- Saving, reloading, and leaving the world complete normally.

Run `/wk admin audit`. Expected moves can be notices, but unexpected skipped claims, missing definitions, collisions, warnings, or errors should be resolved before release.

## 4. Prepare The Workshop Item

Set Workshop requirements for both:

- Working Knowledge
- The source block mod

Steam should resolve Working Knowledge's own downstream requirements. In the description, still state the manual Active Mods priority clearly for offline users and modpack maintainers.

The generated `Publishing/workshop_description_bbcode.txt` and `Publishing/changelog.md` are starter copy. Replace generic claims with the actual supported block pack, schematic groups, test version, and known limitations. Add a suitable preview image that you created or have permission to redistribute.

Before making the item public, verify:

- Title, author, version, description, preview image, and requirements are correct.
- No local paths, logs, test saves, copied source assets, or private notes are included.
- The published mod folder contains only the layer's loadable files and its short README.
- The source mod's permissions allow a separate compatibility patch if its author requires approval.

## 5. Maintain An Existing Layer

When the source mod or Working Knowledge updates, generate a new comparison layer into a different temporary folder. Compare block IDs and mappings instead of immediately overwriting the published copy.

Add new blocks and correct removed or renamed subtype IDs. Preserve published custom-group IDs and definition subtypes. Re-run offline validation, a fresh-world test, `/wk admin audit`, and save/reload before updating the Workshop item.
