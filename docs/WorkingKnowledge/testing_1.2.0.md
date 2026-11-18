# Working Knowledge 1.2.0 Playtest

This build adds Space Engineers 1.211 Warfare 3 support and hides internal family unlockers from the G-menu build list. Automated validation does not replace these in-game checks.

## Visibility and progression

- Load a survival world with progression enabled and Working Knowledge plus Rich HUD Master. Open G with no search, then search for `Schematics` and `Fundamental`. Internal family unlocker blocks should not be offered in the build list.
- Open the Progression tab. The family hierarchy should still render without a crash; its family nodes remain intentional hierarchy placeholders.
- Repeat the build-list and search checks with admin creative tools enabled, then in a creative world.
- Research an incomplete family through salvage and an exact schematic. Confirm its normal blocks unlock in G; confirm incomplete families remain locked. Save, unload, and reload to confirm the unlock survives.
- If testing a custom layer, confirm its `WkKnUnlocker_` placeholders also stay out of build search while its family still unlocks.

## New blocks

- Check the new Prototech Microwave Emitter family in the research terminal. Large and small emitters must share that family and use the Prototech tier.
- Check jump seats under Habitat Fixtures, decorative intakes under Decorative Fixtures, and separation charges under Mechanical Systems.
- Check Type II weapons, torpedo launchers, inset flare launchers, laser antennas, cockpit/chair, brig doors, round atmospheric thrusters, long conveyors, and barred windows under their existing functional families.
- Try `+` cycling with merge blocks and separation charges. They must remain separate groups, and an unlocked merge block must not expose a locked separation charge. Repeat with an existing Prototech family and the emitter family.
- If using a controller, check the Mechanical and Hi-tech radial pages for separation charges and emitters.
- Run `/wk admin audit`, inspect F11 and the game log, and check for definition, missing icon, runtime compilation, and research errors.

## Save compatibility and consumables

- In an existing world, confirm previously completed families retain their progress. The emitter family should be new; other existing blocks should keep their families.
- Consume a data fragment and exact schematic again. Research should work without radiation immunity or another stat effect. Exact schematics should still be returned after use.

## Maintainer notes

- The installed 1.211 data adds 73 catalog entries compared with the prior catalog. Sixteen old off-road wheel-part definitions are now non-public and leave the generated public-block catalog; the exposed wheel/suspension families retain their IDs.
- The visibility change sets `Enabled=false` only on already-loaded unlocker definitions. Never disable or remove these definitions in SBC, or change their `Public` flag: the vanilla graph needs their registered public definitions.
- Bundled vanilla icons were refreshed from the current game, which changed its icon artwork. Custom Working Knowledge item icons are unchanged.
