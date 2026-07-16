# Schematic Groups

Use these IDs in `Data/WorkingKnowledge/block_mappings.txt`.

The toolkit script reads the same list from `Data/schematic_groups.json`.

## Custom Groups

`Start.ps1` can define custom groups and generate their required files. Manual authors use `Data/WorkingKnowledge/schematic_groups.txt`:

```text
version = 1
# stable id | display name | tier | research group subtype | unlocker subtype | optional description
example.power_storage | Example Power Storage Schematics | Uncommon | WkKnLayer_Example_power_storage | WkKnUnlocker_Example_power_storage | Schematics for reinforced power storage.
```

Valid tiers are `Common`, `Uncommon`, `Rare`, `Prototech`, and `None`. The ID is stored in research and Proficiency saves. Keep the ID, research group subtype, and unlocker subtype unchanged after publishing.

The fields mean:

1. **Stable ID** - namespaced Working Knowledge identity used by mappings, research, Proficiency, and saves.
2. **Display name** - player-facing group name.
3. **Tier** - the Data Fragment reward pool that may reveal this group. `None` excludes it from tiered fragment selection.
4. **Research group subtype** - globally unique vanilla research-group definition used for runtime binding.
5. **Unlocker subtype** - globally unique hidden block ID; it must begin with `WkKnUnlocker_`.
6. **Description** - optional player-facing explanation.

Use `Common`, `Uncommon`, `Rare`, or `Prototech` according to the intended fragment rarity. Use `None` only when a group should not be selected by random tiered Data Fragments. The toolkit still creates the durable exact Data Schematic for a custom `None` group.

Higher-priority layers may redeclare the same ID to change its name, description, tier, or wiring while preserving the persisted identity. The highest-priority declaration with valid research-group and unlocker wiring wins and is reported by the audit; an incomplete higher-priority declaration is skipped.

Each custom group also needs matching entries in:

- `Data/ResearchUnlockerGroups.sbc`
- `Data/ResearchUnlockers.sbc`
- `Data/PhysicalItems_ResearchSchematics.sbc`

The toolkit generates these definitions. Use namespaced IDs, never reuse a built-in ID, and run `Validate.ps1` after manual edits.

The exact consumable subtype is deterministic: `WkKnSchematic_` plus the stable ID with punctuation converted to underscores. For example, `example.power_storage` produces `WkKnSchematic_example_power_storage`. Changing the stable ID therefore changes the consumable identity too.

| ID | Display Name | Tier | Common Use |
|---|---|---|---|
| `armor.heavy` | Heavy Armor Schematics | Uncommon | Heavy armor and heavy structural armor variants. |
| `armor.light` | Light Armor Schematics | Common | Light armor and basic hull shaping blocks. |
| `automation.ai_control` | AI Control Schematics | Rare | AI flight, recorder, offensive, defensive, and automation control blocks. |
| `automation.logic` | Automation Logic Schematics | Uncommon | Event controllers, timers, sensors, and logic automation blocks. |
| `communications` | Communications Schematics | Common | Antennas, beacons, cameras, broadcast controllers, and decoys. |
| `control.interfaces` | Control Interface Schematics | Uncommon | Button panels, control panels, and interaction surfaces. |
| `control.stations` | Control Station Schematics | Common | Cockpits, seats, flight seats, and remote controls. |
| `decor.decorative_fixtures` | Decorative Fixture Schematics | Uncommon | Decorative props, statues, and visual fixtures. |
| `decor.habitat_fixtures` | Habitat Fixture Schematics | Uncommon | Beds, lockers, kitchens, bathrooms, and crew fixtures. |
| `decor.signage` | Signage Schematics | Common | Signs, posters, labels, and neon signs. |
| `economy.station_services` | Station Services Schematics | Rare | Store, contract, safe zone, and economy station service blocks. |
| `fundamentals` | Fundamental Schematics | None | Baseline blocks that should be available from the start. |
| `gas.processing` | Gas Processing Schematics | Common | O2/H2 generators, oxygen farms, and gas processing machinery. |
| `gas.storage` | Gas Storage Schematics | Common | Oxygen and hydrogen tanks. |
| `life_support` | Life Support Schematics | Uncommon | Medical rooms, survival kits, cryo chambers, vents, and life support. |
| `logistics.cargo_storage` | Cargo Storage Schematics | Common | Cargo containers, lockers, shelves, and storage blocks. |
| `logistics.cargo_transfer` | Cargo Transfer Schematics | Common | Connectors, collectors, ejectors, and transfer endpoints. |
| `logistics.conveyor_network` | Conveyor Network Schematics | Common | Conveyors, tubes, sorters, junctions, and fittings. |
| `mechanical.systems` | Mechanical Systems Schematics | Common | Pistons, rotors, hinges, landing gear, merge blocks, and gyros. |
| `mechanical.wheel_systems` | Wheel Systems Schematics | Common | Wheels, suspensions, and rover mobility components. |
| `power.battery` | Battery Schematics | Common | Battery and energy storage blocks. |
| `power.hydrogen_engine` | Hydrogen Engine Schematics | Uncommon | Hydrogen engine power generation blocks. |
| `power.reactor` | Reactor Schematics | Rare | Small and large reactors. |
| `power.renewable` | Renewable Power Schematics | Common | Solar panels and wind turbines. |
| `production.advanced` | Advanced Production Schematics | Rare | Full assemblers, refineries, upgrade modules, and advanced production. |
| `production.basic` | Basic Production Schematics | Common | Basic assemblers, basic refineries, and simple production. |
| `production.food` | Food Production Schematics | Common | Food or consumable production blocks from compatible mods. |
| `propulsion.atmospheric_thruster` | Atmospheric Thruster Schematics | Uncommon | Atmospheric thrusters and atmospheric propulsion variants. |
| `propulsion.hydrogen_thruster` | Hydrogen Thruster Schematics | Uncommon | Hydrogen thrusters and hydrogen propulsion variants. |
| `propulsion.ion_thruster` | Ion Thruster Schematics | Rare | Ion thrusters and space propulsion variants. |
| `prototech.assembler` | Prototech Assembler Schematics | Prototech | Prototech assembler blocks. |
| `prototech.battery` | Prototech Battery Schematics | Prototech | Prototech battery blocks. |
| `prototech.drill` | Prototech Drill Schematics | Prototech | Prototech drill blocks. |
| `prototech.gyroscope` | Prototech Gyroscope Schematics | Prototech | Prototech gyroscope blocks. |
| `prototech.jump_drive` | Prototech Jump Drive Schematics | Prototech | Prototech jump drive blocks. |
| `prototech.reactor` | Prototech Reactor Schematics | Prototech | Prototech reactor blocks. |
| `prototech.refinery` | Prototech Refinery Schematics | Prototech | Prototech refinery blocks. |
| `prototech.thruster` | Prototech Thruster Schematics | Prototech | Prototech thruster blocks. |
| `structure.bridge` | Bridge Structure Schematics | Uncommon | Bridge, catwalk, and walkway structure blocks. |
| `structure.door` | Door Schematics | Common | Doors, hatches, gates, and access-panel style blocks. |
| `structure.hangar_gate` | Hangar Gate Schematics | Uncommon | Hangar doors and large gate systems. |
| `structure.industrial` | Industrial Structure Schematics | Common | Trusses, beams, frames, platforms, and supports. |
| `structure.interior` | Interior Structure Schematics | Common | Interior walls, floors, stairs, railings, and room structure. |
| `structure.passage` | Passage Schematics | Uncommon | Passage blocks, corridors, and enclosed walkway modules. |
| `structure.window` | Window Schematics | Common | Windows, glass panels, and transparent structural blocks. |
| `tools.drill` | Drill Schematics | Uncommon | Ship drills and drill-like block tools. |
| `tools.grinder` | Grinder Schematics | Uncommon | Ship grinders and grinder-like block tools. |
| `tools.welder` | Welder Schematics | Uncommon | Ship welders and welder-like block tools. |
| `utility.directed_lighting` | Directed Lighting Schematics | Common | Spotlights, searchlights, floodlights, and directional lights. |
| `utility.display_systems` | Display Systems Schematics | Uncommon | LCD panels, text panels, holographic displays, and display blocks. |
| `utility.gravity` | Gravity Schematics | Rare | Gravity generators, artificial mass, and space balls. |
| `utility.interior_lighting` | Interior Lighting Schematics | Common | Interior lights, light panels, neon tubes, and decorative lights. |
| `utility.jump_drive` | Jump Drive Schematics | Rare | Jump drives and jump navigation blocks. |
| `weapons.fixed_weapon` | Fixed Weapon Schematics | Uncommon | Fixed guns, launchers, railguns, warheads, and direct-fire weapons. |
| `weapons.turret` | Turret Schematics | Rare | Gatling, missile, artillery, interior, and other turret weapons. |
