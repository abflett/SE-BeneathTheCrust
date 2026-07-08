# Space Engineers Prefab Inventory

Generated from a local Space Engineers install on `2026-07-07 01:18:01` local time.

This R&D inventory focuses on reusable vanilla and DLC prefab definitions under `Content\Data\Prefabs`, plus asteroid/voxel structure definitions from `Content\Data\VoxelMapStorages.sbc`. The companion JSON file is the canonical machine-readable data for future conversion scripts.

## Files

- Machine-readable inventory: `docs\BeneathTheCrust\RnD\space_engineers_prefab_inventory.json`
- Human-readable index: `docs\BeneathTheCrust\RnD\space_engineers_prefab_inventory.md`

## Important Caveats

- Prefab files do not directly declare which DLC they belong to. `Source origin` is inferred from folder names such as `Scenarios\Frostbite`; `DLC` columns are inferred from blocks and skins used inside the prefab.
- `.sbcB5` files are prefab cache files generated next to `.sbc` files. They are counted, but only well-formed readable `.sbc` files are XML-parsed.
- `LargeShipRed.sbc` exists with a `.sbc` extension but is binary or malformed in this local install, so it is listed as an unparsed placeholder.
- Scenario world save grids under `Content\Scenarios` and `Content\CustomWorlds` are not expanded in this pass. This inventory is for prefab definitions that can be copied/converted as reusable builds.
- `Content\Data\Blueprints*.sbc` contains production/crafting blueprints, not ship/station blueprint builds.

## Conversion Notes

- Vanilla prefabs live under `Content\Data\Prefabs`. A prefab can be converted manually by copying the `.sbc` into a local blueprint folder under `%APPDATA%\SpaceEngineers\Blueprints\Local` and applying the prefab-to-blueprint XML wrapper/rename steps documented by the Space Engineers Wiki.
- Existing helper tooling: `Vgr255/Prefabs2Blueprints` is an older Python batch converter that can convert one prefab or a folder of prefabs into local blueprints. Treat it as a starting point to test against current prefab XML before relying on it for all 699 files.
- If a prefab `.sbc` is manually edited for testing, delete the adjacent `.sbcB5` cache before expecting the game to regenerate it.

References:

- [Space Engineers Wiki - Prefab Definition](https://spaceengineers.wiki.gg/wiki/Modding/Reference/SBC/Prefab_Definition)
- [Space Engineers Wiki - Convert between Prefab and ShipBlueprint](https://spaceengineers.wiki.gg/wiki/Modding/Tutorials/SBC/Convert_between_Prefab_and_ShipBlueprint)
- [Vgr255/Prefabs2Blueprints](https://github.com/Vgr255/Prefabs2Blueprints)
- [Space Engineers official store DLC list](https://www.spaceengineersgame.com/store/)

## Summary Counts

- Prefab `.sbc` files listed: **699**
- Prefab `.sbc` files XML-parsed: **698**
- Matching `.sbcB5` cache files: **699**
- Voxel map storage definitions: **147**
- Parse errors/placeholders: **1**

### By High-Level Category

| Item | Count |
| --- | ---: |
| Miscellaneous and legacy | 150 |
| Ships | 103 |
| Stations and outposts | 102 |
| Mission prefabs | 91 |
| Scenario prefabs | 52 |
| Drones | 47 |
| Planetary structures | 46 |
| Unknown signals | 41 |
| Global encounters | 33 |
| Space and asteroid encounters | 32 |
| Respawn craft | 2 |

### By Source Kind

| Item | Count |
| --- | ---: |
| Legacy Content | 211 |
| Drone | 47 |
| Planetary Encounter | 46 |
| Economy Sales | 43 |
| Unknown Signal | 41 |
| Legacy Economy Sales | 37 |
| Economy Mission - Salvage | 35 |
| Global Encounter | 33 |
| Random Encounter | 32 |
| Scenario - NeverSurrender | 25 |
| Legacy Economy Stations | 24 |
| Cargo Ship | 23 |
| Economy Mission - HaulingContainers | 22 |
| Economy Stations | 17 |
| Economy Mission - Repair | 14 |
| Economy Mission - Bounty | 11 |
| Scenario - Frostbite | 11 |
| Legacy Economy Missions | 9 |
| Scenario - DeadDrop | 8 |
| Scenario - Scrap | 7 |
| Respawn Ship | 2 |
| Scenario | 1 |

### By Grid Class

| Item | Count |
| --- | ---: |
| Large Grid | 405 |
| Small Grid | 249 |
| Mixed Grid | 44 |
| Unknown Grid | 1 |

### By Mobility

| Item | Count |
| --- | ---: |
| Unknown Mobility | 413 |
| Station/Static | 222 |
| Ship/Mobile | 60 |
| Mixed Static/Mobile | 4 |

### By Inferred DLC Block/Skin Use

| Item | Count |
| --- | ---: |
| HeavyIndustry | 328 |
| Automatons | 301 |
| DecorativeBlocks3 | 298 |
| No inferred DLC block/skin use | 265 |
| DecorativeBlocks | 250 |
| Warfare2 | 244 |
| Signal | 239 |
| DecorativeBlocks2 | 232 |
| SparksOfTheFuture | 213 |
| ScrapRace | 198 |
| Contact | 182 |
| Fieldwork | 157 |
| Warfare1 | 153 |
| Frostbite | 93 |
| CoreSystems | 83 |
| Economy | 74 |
| ApexSurvival | 69 |
| Economy2 | 41 |
| StylePack | 33 |
| DeluxeEdition | 2 |

## Prefab Index

Columns: `DLC/use` is inferred from declared DLC blocks or DLC armor skins found inside the prefab, plus obvious scenario folder origin for Frostbite and Scrap Race. `Referenced by` lists spawn groups, station lists, or respawn definitions that point to the prefab.

### Drones (47)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| DA_Bastion | Bastion Drone | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | DA_Pirate_Bastion | `Data\Prefabs\Drones\DA_Bastion.sbc` |
| DA_BullyBot | DA_BullyBot | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, Warfare2 | DA_BullyBot | `Data\Prefabs\Drones\DA_BullyBot.sbc` |
| DA_TripropAssaultDrone | DA_TripropAssaultDrone | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Warfare2 | DA_TripropAssaultDrone | `Data\Prefabs\Drones\DA_TripropAssaultDrone.sbc` |
| DA_TwinTailedShiv | Twin Tailed Shiv | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | DA_TwinTailedShiv, DA_Pirate_TwinTailedShiv | `Data\Prefabs\Drones\DA_TwinTailedShiv.sbc` |
| DS_Adjudicator | Adjudicator | Large Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks2, SparksOfTheFuture, Warfare2 | DS_Adjudicator | `Data\Prefabs\Drones\DS_Adjudicator.sbc` |
| DS_Assault_Support | DS_Assault_Support | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, Economy, HeavyIndustry, SparksOfTheFuture, Warfare2 | DS_Assault_Support, DS_Assault_Support, DS_HighProfileEscort, DS_M2Barage, +2 more | `Data\Prefabs\Drones\DS_Assault_Support.sbc` |
| DS_AutomatedGunShip | Automated GunShip | Large Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, Signal, Warfare1, Warfare2 | DS_AutomatedGunShip, DS_AutomatedGunShip | `Data\Prefabs\Drones\DS_AutomatedGunShip.sbc` |
| DS_Avenger | Avenger | Small Grid | Unknown Mobility | Drone | Automatons | DS_Avenger | `Data\Prefabs\Drones\DS_Avenger.sbc` |
| DS_Builder_CoilDrone | Coil Drone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, Warfare2 | DS_Builder_CoilDronex2, DS_Builder_CoilDronex2, DS_Builder_CoilDronex3, DS_Builder_CoilDronex3, +3 more | `Data\Prefabs\Drones\DS_Builder_CoilDrone.sbc` |
| DS_Builder_HullBreaker | Hull Breaker | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | DS_Builder_HullBreaker | `Data\Prefabs\Drones\DS_Builder_HullBreaker.sbc` |
| DS_Builder_SentryDrone | Sentry Drone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, ScrapRace | DS_Builder_SentryDroneX1, DS_Builder_SentryDroneX2, DS_Builder_SentryDroneX2, DS_Builder_SentryDroneX3, +4 more | `Data\Prefabs\Drones\DS_Builder_SentryDrone.sbc` |
| DS_BumbleMissile | DS_BumbleMissile | Small Grid | Unknown Mobility | Drone | Automatons, HeavyIndustry | DS_BummbleMissile, DS_BummbleMissiles, DS_BummbleMissiles, DS_BummbleMissiles | `Data\Prefabs\Drones\DS_BumbleMissile.sbc` |
| DS_DemoDrone | DS_DemoDrone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, ScrapRace | DS_DemoDrone, DS_DemoDrone | `Data\Prefabs\Drones\DS_DemoDrone.sbc` |
| DS_Enforcer | Enforcer | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3 | DS_Enforcer | `Data\Prefabs\Drones\DS_Enforcer.sbc` |
| DS_FactorumSpyDrone | Factorum SpyDrone | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | DS_FactorumSpyDrone | `Data\Prefabs\Drones\DS_FactorumSpyDrone.sbc` |
| DS_Factorum_RailDrone | Factorum Rail Drone | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, Economy, HeavyIndustry, Warfare2 | DS_Factorum_RailDrone, DS_Factorum_RailDroneX3, DS_Factorum_RailDroneX3, DS_Factorum_RailDroneX3, +1 more | `Data\Prefabs\Drones\DS_Factorum_RailDrone.sbc` |
| DS_Gunboat | Gunboat | Large Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks2, HeavyIndustry, SparksOfTheFuture, Warfare1 | DS_Gunboat | `Data\Prefabs\Drones\DS_Gunboat.sbc` |
| DS_Gunboat_B | Gunboat | Large Grid | Unknown Mobility | Drone | Automatons, HeavyIndustry, SparksOfTheFuture, Warfare2 | DS_Gunboat_B | `Data\Prefabs\Drones\DS_Gunboat_B.sbc` |
| DS_Harbinger | Harbinger | Large Grid | Unknown Mobility | Drone | Automatons, HeavyIndustry, SparksOfTheFuture, Warfare1, Warfare2 | DS_Harbinger | `Data\Prefabs\Drones\DS_Harbinger.sbc` |
| DS_MilitaryHeavyDrone | Military Heavy Drone | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, StylePack, Warfare2 | DS_MilitaryHeavyDrone, DS_MilitaryHeavyDrone2x, DS_MilitaryHeavyDrone2x | `Data\Prefabs\Drones\DS_MilitaryHeavyDrone.sbc` |
| DS_MilitaryMissile | DS_MilitaryMissile | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, StylePack | DS_MilitaryMissile, DS_MilitaryMissile | `Data\Prefabs\Drones\DS_MilitaryMissile.sbc` |
| DS_Military_GunnerDrone | Military Gunner Drone | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, StylePack, Warfare2 | DS_MilitaryLightDrone | `Data\Prefabs\Drones\DS_Military_GunnerDrone.sbc` |
| DS_Military_Missile2 | Military Missile | Small Grid | Unknown Mobility | Drone | Automatons, HeavyIndustry | DS_MilitaryMissile2, DS_MilitaryMissile2 | `Data\Prefabs\Drones\DS_Military_Missile2.sbc` |
| DS_Miner_DrifterDrone | DS_Miner_DrifterDrone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal | DS_Miner_DrifterDrone, DS_Miner_DrifterDroneX2, DS_Miner_DrifterDroneX2 | `Data\Prefabs\Drones\DS_Miner_DrifterDrone.sbc` |
| DS_MiniMuleDrone | Mini Mule Drone | Small Grid | Unknown Mobility | Drone | Automatons, Signal, Warfare2 | RE27_OldRelayAntenna_C | `Data\Prefabs\Drones\DS_MiniMuleDrone.sbc` |
| DS_Mining_ForemanDrone | DS_Mining_ForemanDrone | Small Grid | Unknown Mobility | Drone | Contact, DecorativeBlocks3, HeavyIndustry, Signal, Warfare2 | DS_Mining_ForemanDrone, DS_Mining_ForemanDroneX2, DS_Mining_ForemanDroneX2, DS_Mining_MiningDroneGroup | `Data\Prefabs\Drones\DS_Mining_ForemanDrone.sbc` |
| DS_Mining_MiningDrone | DS_Mining_MiningDrone | Small Grid | Unknown Mobility | Drone | Contact, DecorativeBlocks3, HeavyIndustry, Warfare2 | DS_Mining_MiningDrone, DS_Mining_MiningDroneGroup | `Data\Prefabs\Drones\DS_Mining_MiningDrone.sbc` |
| DS_OldContainer | Old Container | Small Grid | Unknown Mobility | Drone | Automatons, Contact, HeavyIndustry, Signal | RE27_OldRelayAntenna_A | `Data\Prefabs\Drones\DS_OldContainer.sbc` |
| DS_OldDrone | Old Drone | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, Warfare2 | RE04_Safehouse_Station | `Data\Prefabs\Drones\DS_OldDrone.sbc` |
| DS_OldSupplyModule | Old Supply Module | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal | RE27_OldRelayAntenna_B | `Data\Prefabs\Drones\DS_OldSupplyModule.sbc` |
| DS_PerimeterDrone | Perimeter Drone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture | DS_PerimeterDrone, RE14_StratoFreighterWreck_B, RE16_CorvetteWreck_B | `Data\Prefabs\Drones\DS_PerimeterDrone.sbc` |
| DS_PirateBeaconDrone | Pirate Beacon Drone | Small Grid | Unknown Mobility | Drone | DecorativeBlocks3, HeavyIndustry, Signal | DS_PirateBeaconDrone | `Data\Prefabs\Drones\DS_PirateBeaconDrone.sbc` |
| DS_PirateCorsair | Pirate Corsair | Large Grid | Unknown Mobility | Drone | Contact, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | DS_PirateCorsair | `Data\Prefabs\Drones\DS_PirateCorsair.sbc` |
| DS_Pirate_Bandit | DS_Bandit | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Warfare2 | DS_Pirate_Bandit, DS_Pirate_Bandit2x, DS_Pirate_Bandit2x | `Data\Prefabs\Drones\DS_Pirate_Bandit.sbc` |
| DS_Pirate_Bruiser_A | Bruiser-A | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal | DS_Pirate_Bruiser_A, DS_Pirate_Bruisers | `Data\Prefabs\Drones\DS_Pirate_Bruiser_A.sbc` |
| DS_Pirate_Bruiser_R | Bruiser-R | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 | DS_Pirate_Bruiser_R, DS_Pirate_Bruisers | `Data\Prefabs\Drones\DS_Pirate_Bruiser_R.sbc` |
| DS_Pirate_Scavenger | Scavenger Drone | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, ScrapRace, Warfare2 | DS_ScavengerDrone, DS_ScavengerDrone2x, DS_ScavengerDrone2x | `Data\Prefabs\Drones\DS_Pirate_Scavenger.sbc` |
| DS_Pirate_Scimitar | Scimitar Class Pirate Drone | Small Grid | Unknown Mobility | Drone | HeavyIndustry, SparksOfTheFuture, Warfare2 | Drones_Pirate_Space_Light | `Data\Prefabs\Drones\DS_Pirate_Scimitar.sbc` |
| DS_Pirate_ShakedownDrone | Shakedown Drone | Small Grid | Unknown Mobility | Drone | Automatons, Contact, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture | DS_Pirate_ShakedownDrone | `Data\Prefabs\Drones\DS_Pirate_ShakedownDrone.sbc` |
| DS_PrivateerDrone | Privateer Drone | Large Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare1, Warfare2 | DS_PrivateerDrone, DS_PrivateerDrone2X, DS_PrivateerDrone2X | `Data\Prefabs\Drones\DS_PrivateerDrone.sbc` |
| DS_Sentinel | Sentinel | Small Grid | Unknown Mobility | Drone | HeavyIndustry, SparksOfTheFuture, Warfare2 | DS_Sentinel | `Data\Prefabs\Drones\DS_Sentinel.sbc` |
| DS_SnubShipAutocannon | Snub Ship Autocannon | Small Grid | Unknown Mobility | Drone | Automatons, CoreSystems, Fieldwork, HeavyIndustry | DS_SnubShips | `Data\Prefabs\Drones\DS_SnubShipAutocannon.sbc` |
| DS_SnubShipGatling | Snub Ship Gatling | Small Grid | Unknown Mobility | Drone | Automatons, CoreSystems, Fieldwork, HeavyIndustry | DS_SnubShips | `Data\Prefabs\Drones\DS_SnubShipGatling.sbc` |
| DS_SolarOrbiter_Patrolling | Solar Orbiter | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | RE15_PrivateSailWreckB | `Data\Prefabs\Drones\DS_SolarOrbiter_Patrolling.sbc` |
| DS_Trade_SolarDefender | DS_Trade_SolarDefender | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | DS_Trade_SolarDefender | `Data\Prefabs\Drones\DS_Trade_SolarDefender.sbc` |
| DS_Trade_SolarOrbiter | Solar Orbiter | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | DS_Trade_SolarOrbiter | `Data\Prefabs\Drones\DS_Trade_SolarOrbiter.sbc` |
| DS_TwinrailDrone | Twinrail Drone | Small Grid | Unknown Mobility | Drone | Automatons, DecorativeBlocks3, HeavyIndustry, Warfare2 | DS_TwinrailDrone, RE06_ScienceShuttle_A | `Data\Prefabs\Drones\DS_TwinrailDrone.sbc` |

### Global encounters (33)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GE_Ambush | Factorum Outpost, GE_Outpost_Gate, +1 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_AmbushA | `Data\Prefabs\GlobalEncounters\GE_Ambush.sbc` |
| GE_AmbushB | Factorum Outpost, Prototech Module | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_AmbushB | `Data\Prefabs\GlobalEncounters\GE_AmbushB.sbc` |
| GE_BioResearch | Bio Research Facility, BioStalk, +6 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_BioResearchA | `Data\Prefabs\GlobalEncounters\GE_BioResearch.sbc` |
| GE_BioResearchB | Bio Research Facility, BioStalk, +6 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_BioResearchB | `Data\Prefabs\GlobalEncounters\GE_BioResearchB.sbc` |
| GE_FactorumWarship | Factorum Warship, Prototech Module | Mixed Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_FactorumWarshipA | `Data\Prefabs\GlobalEncounters\GE_FactorumWarship.sbc` |
| GE_FactorumWarshipB | Factorum Warship, Prototech Module | Mixed Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_FactorumWarshipB | `Data\Prefabs\GlobalEncounters\GE_FactorumWarshipB.sbc` |
| GE_FactorumWarshipC | Factorum Warship | Large Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | FactorumWarship | `Data\Prefabs\GlobalEncounters\GE_FactorumWarshipC.sbc` |
| GE_FactorumWarshipD | Factorum Warship | Large Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_FactorumWarshipC | `Data\Prefabs\GlobalEncounters\GE_FactorumWarshipD.sbc` |
| GE_HighProfileTransport | High Profile Transport, Prototech Module | Mixed Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_HighProfileTransportA | `Data\Prefabs\GlobalEncounters\GE_HighProfileTransport.sbc` |
| GE_HighProfileTransportB | High Profile Transport, Prototech Module | Mixed Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_HighProfileTransportB | `Data\Prefabs\GlobalEncounters\GE_HighProfileTransportB.sbc` |
| GE_ListeningPost | Factorum Listening Post, Small Grid 1974 | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | GE_ListeningPostA | `Data\Prefabs\GlobalEncounters\GE_ListeningPost.sbc` |
| GE_ListeningPostB | Factorum Listening Post, Small Grid 1974 | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | GE_ListeningPostB | `Data\Prefabs\GlobalEncounters\GE_ListeningPostB.sbc` |
| GE_ListeningPostC | Factorum Listening Post, Small Grid 1974 | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | GE_ListeningPostC | `Data\Prefabs\GlobalEncounters\GE_ListeningPostC.sbc` |
| GE_LogisticsFacility | Factorum Cargo Container, Factorum Containers, +4 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_LogisticsFacilityA | `Data\Prefabs\GlobalEncounters\GE_LogisticsFacility.sbc` |
| GE_LogisticsFacilityB | Factorum Cargo Container, Factorum Containers, +4 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_LogisticsFacilityB | `Data\Prefabs\GlobalEncounters\GE_LogisticsFacilityB.sbc` |
| GE_StorageFacility | Defence Emplacement, Factorum Guard Drone, +8 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, DeluxeEdition, Economy, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_StorageFacilityA | `Data\Prefabs\GlobalEncounters\GE_StorageFacility.sbc` |
| GE_StorageFacilityB | Defence Emplacement, Factorum Guard Drone, +8 more | Mixed Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, DeluxeEdition, Economy, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_StorageFacilityB | `Data\Prefabs\GlobalEncounters\GE_StorageFacilityB.sbc` |
| DS_M2_Barage | M2-Barage | Large Grid | Unknown Mobility | Global Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | DS_M2Barage, FactorumShips, FactorumShips, M2Barage | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\DS_M2_Barage.sbc` |
| GE_Debris_Ambush | GE_Debris, GE_Debris Lure | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks3, Economy, HeavyIndustry, ScrapRace | GE_Debris_Ambush | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_Debris_Ambush.sbc` |
| GE_Debris_Tube | DS_TwoStageMissile, D_PodDrone, +3 more | Mixed Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | GE_Debris_Tubes | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_Debris_Tube.sbc` |
| GE_FactorumGunPlatform_A | Factorum Gun Platform (Type A) | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, SparksOfTheFuture, Warfare2 | GE_GunPlatform_A | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_FactorumGunPlatform_A.sbc` |
| GE_FactorumGunPlatform_B | Factorum Gun Platform (Type B) | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, SparksOfTheFuture, Warfare2 | GE_GunPlatform_B | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_FactorumGunPlatform_B.sbc` |
| GE_FactorumGunPlatform_C | Factorum Gun Platform (Type C) | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, SparksOfTheFuture, Warfare2 | GE_GunPlatform_C | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_FactorumGunPlatform_C.sbc` |
| GE_FreightPlateA | D_PodDrone, GE_Debris_FreightHull, +1 more | Mixed Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, Warfare2 | GE_FreightPlateA | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_FreightPlateA.sbc` |
| GE_FreightPlateB | GE_Debris_FreightPlate, GE_GunBouy | Large Grid | Unknown Mobility | Global Encounter | Automatons, Economy, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare2 | GE_FreightPlateB | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_FreightPlateB.sbc` |
| GE_Mine | GE_Mine | Large Grid | Station/Static | Global Encounter | HeavyIndustry | GE_Mine | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_Mine.sbc` |
| GE_Reclaimer | Factorum Reclaimer Drone | Large Grid | Unknown Mobility | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | DS_ReclaimerDrone, GE_Reclaimer | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_Reclaimer.sbc` |
| GE_SmallShipyard | Factorum Reclaimer (Damaged), Factorum Shipyard | Large Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_SmallShipyard | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_SmallShipyard.sbc` |
| GE_SmallShipyard_Empty | Factorum Shipyard | Large Grid | Station/Static | Global Encounter | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | GE_SmallShipyardEmpty | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_SmallShipyard_Empty.sbc` |
| GE_ZoneSubStationA | GE_ZoneSubStationA | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | GE_ZoneSubStationA | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_ZoneSubStationA.sbc` |
| GE_ZoneSubStationB | GE_ZoneSubStationB | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | GE_ZoneSubStationB | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_ZoneSubStationB.sbc` |
| GE_ZoneSubStationC | GE_ZoneSubStationC | Large Grid | Station/Static | Global Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare2 | GE_ZoneSubStationC | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\GE_ZoneSubStationC.sbc` |
| HighProfileEscort | High Profile Escort | Large Grid | Unknown Mobility | Global Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | DS_HighProfileEscort, FactorumShips, FactorumShips, HighProfileEscort | `Data\Prefabs\GlobalEncounters\GlobalSubEncounters\HighProfileEscort.sbc` |

### Miscellaneous and legacy (150)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 'Tick'_Ore_Scow_and_'Mite'_Mining_Drone | 'Tick' Ore Scow and 'Mite' Mining Drone, Small Ship 4554 | Mixed Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\'Tick'_Ore_Scow_and_'Mite'_Mining_Drone.sbc` |
| (DL._Inc)_Defender_Drone_MK_I | (DL. Inc) Defender Drone MK I | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Defender_Drone_MK_I.sbc` |
| (DL._Inc)_Defender_Drone_Mk_II | (DL. Inc) Defender Drone Mk II | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Defender_Drone_Mk_II.sbc` |
| (DL._Inc)_Grinder_Drone_Mk_I | (DL. Inc) Grinder Drone Mk I | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Grinder_Drone_Mk_I.sbc` |
| (DL._Inc)_Grinder_Drone_Mk_II | (DL. Inc) Grinder Drone Mk II | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Grinder_Drone_Mk_II.sbc` |
| (DL._Inc)_Mining_Drone_MK_I | (DL. Inc) Mining Drone MK I | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Mining_Drone_MK_I.sbc` |
| (DL._Inc)_Mining_Drone_Mk_II | (DL. Inc) Mining Drone Mk II | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Mining_Drone_Mk_II.sbc` |
| (DL._Inc)_Welder_Drone_Mk_I | (DL. Inc) Welder Drone Mk I | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Welder_Drone_Mk_I.sbc` |
| (DL._Inc)_Welder_Drone_Mk_II | (DL. Inc) Welder Drone Mk II | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\(DL._Inc)_Welder_Drone_Mk_II.sbc` |
| ASM-2 | ASM-2, Малый корабль 304, +1 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\ASM-2.sbc` |
| Ablator | Ablator - Interplanetary Fighter | Small Grid | Unknown Mobility | Legacy Content |  | Ablator | `Data\Prefabs\LegacyContent\Ablator.sbc` |
| AlienLander | Atmospheric Lander mk.1 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\AlienLander.sbc` |
| Arke_Supply_Ship | Arke Supply Ship, Large Ship 5552 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Arke_Supply_Ship.sbc` |
| Armed_Transport_Charon | Armed Transport "Charon", Small Ship 2641, +7 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Armed_Transport_Charon.sbc` |
| Assailant_mk.1 | Assailant mk.1 | Large Grid | Unknown Mobility | Legacy Content |  | Assailant_mk.1 | `Data\Prefabs\LegacyContent\Assailant_mk.1.sbc` |
| AtmosphericLander | Atmospheric Lander mk.1 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\AtmosphericLander.sbc` |
| Atmospheric_Miner_mk.2 | Drill Ship Atmosphere mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Atmospheric_Miner_mk.2.sbc` |
| Barb_mk.1 | Barb mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Barb_mk.1.sbc` |
| Buggy | Buggy, Small Ship 1165, +3 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Buggy.sbc` |
| Bumblebee | Bumblebee | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Bumblebee.sbc` |
| Camera_Creep_mk.1 | Camera Creep mk.1, Small Grid 1651, +1 more | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Camera_Creep_mk.1.sbc` |
| CargoShip |  | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\CargoShip.sbc` |
| Cargo_Buddy_mk.1 | Cargo Buddy mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Cargo Buddy mk.1 | `Data\Prefabs\LegacyContent\Cargo_Buddy_mk.1.sbc` |
| CarrierShipProjector | CarrierShipProjector | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\CarrierShipProjector.sbc` |
| Carrier_[Infinity] | Carrier [Infinity] | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Carrier_[Infinity].sbc` |
| Combatant mk.1_13 Weapons Ready | Combatant mk.1_13 Weapons Ready | Small Grid | Unknown Mobility | Legacy Content |  | Combatant mk.1_13 Weapons Ready | `Data\Prefabs\LegacyContent\Combatant mk.1_13 Weapons Ready.sbc` |
| Commercial_Salvager | Commercial Salvager | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Commercial_Salvager.sbc` |
| Computer_Part_Factory_(No_Mods) | Computer Part Factory (No Mods) | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Computer_Part_Factory_(No_Mods).sbc` |
| ConstructionShip | Small Ship 9137 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\ConstructionShip.sbc` |
| Constructor_mk.1 | Constructor mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Constructor_mk.1.sbc` |
| ContainerSpaceSmall1 | Reward Crate Small mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\ContainerSpaceSmall1.sbc` |
| DSF3_Duchessa | DSF3 Duchessa | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\DSF3_Duchessa.sbc` |
| Deterrent_mk.1 | Deterrent mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Deterrent_mk.1.sbc` |
| DodgeDrone | Dodge Drone | Small Grid | Unknown Mobility | Legacy Content |  | DodgeDrone | `Data\Prefabs\LegacyContent\Small_Grid_1642.sbc` |
| Dread_Hulk_Nefarious_[A2-9604-p] | Dread Hulk Nefarious [A2-9604-p] | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Dread_Hulk_Nefarious_[A2-9604-p].sbc` |
| Drifting_Pursuant_mk.1 | Drifting Pursuant mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | DriftingPursuant | `Data\Prefabs\LegacyContent\Drifting_Pursuant_mk.1.sbc` |
| Drill_Ship_Moon_mk.1 | Drill Ship Moon mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Drill_Ship_Moon_mk.1.sbc` |
| Drillbot_mk.1 | Drillbot mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Drillbot mk.1 | `Data\Prefabs\LegacyContent\Drillbot_mk.1.sbc` |
| Encounter Ghoul Corvette A | Encounter Ghoul Corvette | Large Grid | Unknown Mobility | Legacy Content |  | Encounter Ghoul Corvette A | `Data\Prefabs\LegacyContent\Encounter Ghoul Corvette A.sbc` |
| Encounter Ghoul Corvette B | Encounter Ghoul Corvette | Large Grid | Unknown Mobility | Legacy Content |  | Encounter Ghoul Corvette B | `Data\Prefabs\LegacyContent\Encounter Ghoul Corvette B.sbc` |
| Encounter Ghoul Corvette C | Encounter Ghoul Corvette | Large Grid | Unknown Mobility | Legacy Content |  | Encounter Ghoul Corvette C | `Data\Prefabs\LegacyContent\Encounter Ghoul Corvette C.sbc` |
| Encounter Habitat Pods | Dead Astronaut, Debris, +1 more | Mixed Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Habitat Pods | `Data\Prefabs\LegacyContent\Encounter Habitat Pods.sbc` |
| Encounter Homing beacon | Homing beacon | Small Grid | Unknown Mobility | Legacy Content |  | Encounter Homing beacon | `Data\Prefabs\LegacyContent\Encounter Homing beacon.sbc` |
| Encounter Imp A | Encounter Imp | Large Grid | Unknown Mobility | Legacy Content |  | Encounter Imp A | `Data\Prefabs\LegacyContent\Encounter Imp A.sbc` |
| Encounter Imp B | Encounter Imp | Large Grid | Unknown Mobility | Legacy Content |  | Encounter Imp B | `Data\Prefabs\LegacyContent\Encounter Imp B.sbc` |
| Encounter Minelayer | Encounter Minelayer | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Minelayer.sbc` |
| Encounter MushStation A | Encounter MushStation | Large Grid | Unknown Mobility | Legacy Content |  | Encounter MushStation A | `Data\Prefabs\LegacyContent\Encounter MushStation A.sbc` |
| Encounter MushStation B | Encounter MushStation | Large Grid | Unknown Mobility | Legacy Content |  | Encounter MushStation B | `Data\Prefabs\LegacyContent\Encounter MushStation B.sbc` |
| Encounter Pirate Raider | Encounter Pirate Raider | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Pirate Raider | `Data\Prefabs\LegacyContent\Encounter Pirate Raider.sbc` |
| Encounter Ponos-F1 | Encounter Ponos-F1, Large Grid 8833, +1 more | Mixed Grid | Unknown Mobility | Legacy Content |  | Encounter Ponos-F1 A, Encounter Ponos-F1 B | `Data\Prefabs\LegacyContent\Encounter Ponos-F1.sbc` |
| Encounter RespawnShip | Encounter RespawnShip | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter RespawnShip | `Data\Prefabs\LegacyContent\Encounter RespawnShip.sbc` |
| Encounter Salvage station | Encounter Salvage station | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Salvage station | `Data\Prefabs\LegacyContent\Encounter Salvage station.sbc` |
| Encounter Severed Bow | Encounter Debris, Encounter Severed Bow | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Severed Bow | `Data\Prefabs\LegacyContent\Encounter Severed Bow.sbc` |
| Encounter Shuttle | Dead Engineer, Encounter Shuttle, +1 more | Mixed Grid | Unknown Mobility | Legacy Content |  | Encounter Shuttle | `Data\Prefabs\LegacyContent\Encounter Shuttle.sbc` |
| Encounter Stealth pirate station | Encounter Stealth pirate station | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Stealth pirate station | `Data\Prefabs\LegacyContent\Encounter Stealth pirate station.sbc` |
| Encounter Vulture vessel | Encounter Vulture vessel | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | Encounter Vulture vessel | `Data\Prefabs\LegacyContent\Encounter Vulture vessel.sbc` |
| EncounterCitadelStation | Encounter Mining Station, Large Grid 6810 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Citadel Station.sbc` |
| Enforcer_mk.2 | Enforcer mk.2 | Small Grid | Unknown Mobility | Legacy Content |  | Enforcer mk.2 | `Data\Prefabs\LegacyContent\Enforcer_mk.2.sbc` |
| Explorer | Explorer, Large Ship 1976, +1 more | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Explorer.sbc` |
| Fighter |  | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Fighter.sbc` |
| Fleeting_Rival_mk.1 | Fleeting Rival mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | FleetingRival | `Data\Prefabs\LegacyContent\Fleeting_Rival_mk.1.sbc` |
| GLU_Jaguar_B-1,_light_Interceptor | GLU Jaguar B-1, light Interceptor | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\GLU_Jaguar_B-1,_light_Interceptor.sbc` |
| GLU_Mule_A-1,_light_Freighter | GLU Mule A-1, light Freighter, Small Ship 2875, +3 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\GLU_Mule_A-1,_light_Freighter.sbc` |
| GM2-411_Combat_Destroyer | GM/2-411 Combat Destroyer | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\GM2-411_Combat_Destroyer.sbc` |
| Grindbot_mk.1 | Grindbot mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Grindbot mk.1 | `Data\Prefabs\LegacyContent\Grindbot_mk.1.sbc` |
| Hostile Miner | Hostile Miner | Small Grid | Unknown Mobility | Legacy Content |  | Hostile Miner | `Data\Prefabs\LegacyContent\Hostile Miner.sbc` |
| Hydrogen_Test_Drone | Hydrogen Test Drone | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Hydrogen_Test_Drone.sbc` |
| IMDC_Cerberus_Destroyer_Mk.3 | IMDC Cerberus Destroyer Mk.3 NPC | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\IMDC_Cerberus_Destroyer_Mk.3.sbc` |
| IMDC_Defense_Platform_NPC | IMDC Defense Platform NPC | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\IMDC_Defense_Platform_NPC.sbc` |
| IMDC_Vulture_Fighter_Bomber | IMDC Vulture Fighter Bomber NPC | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\IMDC_Vulture_Fighter_Bomber.sbc` |
| Informant_mk.1 | Informant mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Informant_mk.1 | `Data\Prefabs\LegacyContent\Informant_mk.1.sbc` |
| Kestral-Class_Assault_Fighter | Kestral-Class Assault Fighter | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Kestral-Class_Assault_Fighter.sbc` |
| LargeShipBlue | Large Ship 1553 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipBlue.sbc` |
| LargeShipRed | UNPARSED: not well-formed (invalid token): line 1, column 0 | Unknown Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipRed.sbc` |
| LargeShipRedBuilding |  | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipRedBuilding.sbc` |
| LargeShipRedCrashed | Large Ship 7434 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipRedCrashed.sbc` |
| LargeShipRedQS1 | Large Ship 5332 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipRedQS1.sbc` |
| LargeShipUnderConstruction |  | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\LargeShipUnderConstruction.sbc` |
| Large_Ship_1360 | Large Ship 1360, Large Ship 8748 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Large_Ship_1360.sbc` |
| Light patrol drone | Light patrol drone | Small Grid | Unknown Mobility | Legacy Content |  | Light patrol drone | `Data\Prefabs\LegacyContent\Light patrol drone.sbc` |
| Longsword_Mk._9S | Longsword Mk. 9S | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Longsword_Mk._9S.sbc` |
| MarsLander | Atmospheric Lander mk.1 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\MarsLander.sbc` |
| Medium-Sized_Explorer_Christine | Large Ship 143, Large Ship 3811, +2 more | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Medium-Sized_Explorer_Christine.sbc` |
| Military1_Legacy | CargoShip_Military1 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Military1_Legacy.sbc` |
| Military2 | CargoShip_Military2 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Military2.sbc` |
| Military3 | CargoShip_Military3, Large Ship 6131 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Military3.sbc` |
| Mining1Legacy | CargoShip_Mining1 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Mining1Legacy.sbc` |
| Mining2 | CargoShip_Mining2 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Mining2.sbc` |
| Mining3 | CargoShip_Mining3, Large Ship 1473, +1 more | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Mining3.sbc` |
| NewLargeShipPrefab |  | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewLargeShipPrefab.sbc` |
| NewLargeShipPrefabSurvival |  | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewLargeShipPrefabSurvival.sbc` |
| NewSmallShipPrefab |  | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewSmallShipPrefab.sbc` |
| NewSmallShipPrefabSurvival |  | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewSmallShipPrefabSurvival.sbc` |
| PZK_Coockie_monster_hull | Mały statek 22, Mały statek 2874, +4 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\PZK_Coockie_monster_hull.sbc` |
| PatrolBot | PatrolBot | Small Grid | Unknown Mobility | Legacy Content | SparksOfTheFuture, StylePack |  | `Data\Prefabs\LegacyContent\PatrolBot.sbc` |
| Penguin_mk.1 | Penguin mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Penguin_mk.1.sbc` |
| Pitbull_Bomber | Pitbull Bomber | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Pitbull_Bomber.sbc` |
| Plataforma_1000 | Nave Grande 1345, Nave Grande 2022, +1 more | Large Grid | Mixed Static/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Plataforma_1000.sbc` |
| Platform_5546 | Large Ship 8022, Platform 5546 | Large Grid | Mixed Static/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_5546.sbc` |
| Platform_6572 | Large Ship 2746, Large Ship 754, +2 more | Large Grid | Mixed Static/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_6572.sbc` |
| PractiseShip | PractiseShip | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\PractiseShip.sbc` |
| Projector_large_ship | Projector large ship | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Projector_large_ship.sbc` |
| Projector_small_ship | Projector small ship | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Projector_small_ship.sbc` |
| ProtectoBot | ProtectoBot | Small Grid | Unknown Mobility | Legacy Content |  | ProtectoBot, ProtectoBot | `Data\Prefabs\LegacyContent\ProtectoBot.sbc` |
| R.U.S.T._Freighter | R.U.S.T. Freighter | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks | R.U.S.T. Freighter | `Data\Prefabs\LegacyContent\R.U.S.T._Freighter.sbc` |
| RAVEN | "RAVEN", Large Ship 2561, +4 more | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\RAVEN.sbc` |
| Raefftech_Base_Ship_(outdated) | Raefftech Base Ship (outdated) | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Raefftech_Base_Ship_(outdated).sbc` |
| Raider Drone | Raider Drone | Small Grid | Unknown Mobility | Legacy Content |  | Raider Drone, Encounter Ponos-F1 B, Encounter Ponos-F1 B | `Data\Prefabs\LegacyContent\Raider Drone.sbc` |
| Raiding_Outpost_mk.1 | Raiding Outpost mk.1 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Raiding_Outpost_mk.1.sbc` |
| Raven_MkII | Raven MkII | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Raven_MkII.sbc` |
| Renaissance | Nave grande 498, Nave grande 8673, +1 more | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Renaissance.sbc` |
| Respawn Ship 2 | RespawnShip2 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SpawnShipAssembler.sbc` |
| Respawn Ship 3 | Large Ship 6694 | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SpawnShipRefineryAssembler.sbc` |
| RespawnMoonPod | Moon Drop Pod, Respawn Moon Pod, +3 more | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\RespawnMoonPod.sbc` |
| RespawnPlanetPod | Respawn Planet Pod | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\RespawnPlanetPod.sbc` |
| SF-45 | SF-45, Small Ship 3601, +3 more | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SF-45.sbc` |
| STARTERSHIP | STARTERSHIP | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\STARTERSHIP.sbc` |
| Salvage Drone | Salvage Drone | Small Grid | Unknown Mobility | Legacy Content |  | Salvage Drone, Salvage Drone | `Data\Prefabs\LegacyContent\Salvage Drone.sbc` |
| Seeker Mine | Seeker Mine | Small Grid | Unknown Mobility | Legacy Content |  | Seeker Mine, Seeker Mine, Seeker Mine | `Data\Prefabs\LegacyContent\Seeker Mine.sbc` |
| Small Drill Ship | Small Ship 8740 | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SpawnShipDrill.sbc` |
| Small Escape Pod | Small Ship 8543 | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SpawnShipEscapePod.sbc` |
| Small Ship 5418 | Small Ship 5418 | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Small Ship 5418.sbc` |
| SmallShip_SingleBlock |  | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SmallShip_SingleBlock.sbc` |
| Small_Ship_5727 | Small Ship 5727 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Small_Ship_5727.sbc` |
| Snub Fighter | Snub Fighter | Small Grid | Unknown Mobility | Legacy Content |  | Snub Fighter | `Data\Prefabs\LegacyContent\Snub Fighter.sbc` |
| Space_Mine_mk.1 | Small Grid 1792, Small Grid 3736, +3 more | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Space_Mine_mk.1.sbc` |
| Spaceport_Terminal | Large Ship 1498, Large Ship 2645, +3 more | Mixed Grid | Mixed Static/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Spaceport_Terminal.sbc` |
| Spiteful_Aggressor_mk.1 | Spiteful Aggressor mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | SpitefulAggressor | `Data\Prefabs\LegacyContent\Spiteful_Aggressor_mk.1.sbc` |
| Starfury_MkII | Starfury MkII | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Starfury_MkII.sbc` |
| Stash-satellite | Stash-satellite | Small Grid | Unknown Mobility | Legacy Content |  | Stash-satellite | `Data\Prefabs\LegacyContent\Stash-satellite.sbc` |
| Stinging_Adversary_mk.1 | Stinging Adversary mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | StingingAdversary | `Data\Prefabs\LegacyContent\Stinging_Adversary_mk.1.sbc` |
| SuicideShip |  | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\SuicideShip.sbc` |
| TLF-28a_(LSF_CargoShip_Mod) | TLF-28a (LSF CargoShip Mod) | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\TLF-28a_(LSF_CargoShip_Mod).sbc` |
| The Pilgrims Curiosity | The Pilgrims Curiosity | Large Grid | Unknown Mobility | Legacy Content |  | The Pilgrims Curiosity | `Data\Prefabs\LegacyContent\The Pilgrims Curiosity.sbc` |
| Thief_mk.1 | Thief mk.1 | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Thief_mk.1.sbc` |
| Trade1 | CargoShip_Trade1 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Trade1.sbc` |
| Trade2 | CargoShip_Trade2 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Trade2.sbc` |
| Trade3 | CargoShip_Trade3 | Large Grid | Unknown Mobility | Legacy Content | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Trade3.sbc` |
| Trader1 | Frigate Escort - Type 7A Aegis, Large Ship 4956 | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Trader1.sbc` |
| Trainspark | Trainspark | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Trainspark.sbc` |
| Tusk_mk.1 | Tusk mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Tusk | `Data\Prefabs\LegacyContent\Tusk_mk.1.sbc` |
| Ubumboo_(for_Exploration) | Ubumboo (for Exploration) | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Ubumboo_(for_Exploration).sbc` |
| V2-Gunboat | V2-Gunboat | Large Grid | Unknown Mobility | Legacy Content |  | V2-Gunboat | `Data\Prefabs\LegacyContent\V2-Gunboat.sbc` |
| VehicleProjector | VehicleProjector | Small Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\VehicleProjector.sbc` |
| Volunder_exploration_ship | Volunder exploration ship | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Volunder_exploration_ship.sbc` |
| Vulture Drone | Vulture Drone | Small Grid | Unknown Mobility | Legacy Content |  | Vulture Drone | `Data\Prefabs\LegacyContent\Vulture Drone.sbc` |
| Weapon_Platform_Gatling_mk.1 | Weapon Platform Gatling mk.1 | Small Grid | Unknown Mobility | Legacy Content |  | Weapon Platform Gatling1 | `Data\Prefabs\LegacyContent\Weapon_Platform_Gatling_mk.1.sbc` |
| Xedus_Ophiniam_class_Attack_Frigate | Xedus Ophiniam class Attack Frigate | Large Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Xedus_Ophiniam_class_Attack_Frigate.sbc` |
| YellowDrone | Yellow Drone | Large Grid | Unknown Mobility | Legacy Content |  |  | `Data\Prefabs\LegacyContent\YellowDrone.sbc` |
| [O-S-L-E]_Enforcer | [O-S-L-E] Enforcer | Small Grid | Ship/Mobile | Legacy Content |  |  | `Data\Prefabs\LegacyContent\[O-S-L-E]_Enforcer.sbc` |

### Mission prefabs (91)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Bounty_AutomatedPatrolCraft | Automated Patrol Craft | Large Grid | Unknown Mobility | Economy Mission - Bounty | Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, Signal, Warfare2 | Bounty_AutomatedPatrolCraft | `Data\Prefabs\Economy\Missions\Bounty\Bounty_AutomatedPatrolCraft.sbc` |
| Bounty_D2Hauler | Bounty_D2Hauler | Large Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | Bounty_D2Hauler | `Data\Prefabs\Economy\Missions\Bounty\Bounty_D2Hauler.sbc` |
| Bounty_MilitaryEnforcer | Military Enforcer | Large Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, HeavyIndustry, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | Bounty_MilitaryEnforcer | `Data\Prefabs\Economy\Missions\Bounty\Bounty_MilitaryEnforcer.sbc` |
| Bounty_MilitaryMissileFrigate | Military Missile Frigate | Large Grid | Unknown Mobility | Economy Mission - Bounty | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | Bounty_MilitaryMissileFrigate | `Data\Prefabs\Economy\Missions\Bounty\Bounty_MilitaryMissileFrigate.sbc` |
| Bounty_PirateBulwarkHauler | Pirate Bulwark Hauler | Small Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, Contact, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace | Bounty_PirateBulwarkHauler | `Data\Prefabs\Economy\Missions\Bounty\Bounty_PirateBulwarkHauler.sbc` |
| Bounty_PirateHaulframe | Bounty_PirateHaulframe | Large Grid | Unknown Mobility | Economy Mission - Bounty | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | Bounty_PirateHaulframe | `Data\Prefabs\Economy\Missions\Bounty\Bounty_PirateHaulframe.sbc` |
| Bounty_PirateHeavyTowship | Pirate Heavy Towship | Large Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | Bounty_PirateHeavyTowship | `Data\Prefabs\Economy\Missions\Bounty\Bounty_PirateHeavyTowship.sbc` |
| Bounty_RogueWarden1 | Rogue Warden 1 | Small Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, StylePack, Warfare2 | Bounty_RogueWarden1 | `Data\Prefabs\Economy\Missions\Bounty\Bounty_RogueWarden1.sbc` |
| Bounty_RogueWarden2 | Rogue Warden 2 | Small Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, StylePack, Warfare2 | Bounty_RogueWarden2 | `Data\Prefabs\Economy\Missions\Bounty\Bounty_RogueWarden2.sbc` |
| Bounty_RookGunship | Bounty_RookGunship | Large Grid | Unknown Mobility | Economy Mission - Bounty | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1 | Bounty_RookGunship | `Data\Prefabs\Economy\Missions\Bounty\Bounty_RookGunship.sbc` |
| Bounty_SnubShipLeader | Snub Ship Leader | Small Grid | Unknown Mobility | Economy Mission - Bounty | Automatons, CoreSystems, Fieldwork, HeavyIndustry | Bounty_SnubShipLeader | `Data\Prefabs\Economy\Missions\Bounty\Bounty_SnubShipLeader.sbc` |
| HaulContainer_L_1 | HaulContainer_L_1 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_1.sbc` |
| HaulContainer_L_2 | HaulContainer_L_2 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_2.sbc` |
| HaulContainer_L_3 | HaulContainer_L_3 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks3, Fieldwork, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_3.sbc` |
| HaulContainer_L_4 | HaulContainer_L_4 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_4.sbc` |
| HaulContainer_L_5 | HaulContainer_L_5 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_5.sbc` |
| HaulContainer_L_6 | HaulContainer_L_6 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_6.sbc` |
| HaulContainer_L_7 | HaulContainer_L_7 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_L_7.sbc` |
| HaulContainer_M_1 | HaulContainer_M_1 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Contact, Signal |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_1.sbc` |
| HaulContainer_M_2 | HaulContainer_M_2 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, Contact, Signal, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_2.sbc` |
| HaulContainer_M_3 | HaulContainer_M_3 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Contact, Signal |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_3.sbc` |
| HaulContainer_M_4 | HaulContainer_M_4 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Contact, DecorativeBlocks2, DecorativeBlocks3 |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_4.sbc` |
| HaulContainer_M_5 | HaulContainer_M_5 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Contact, DecorativeBlocks2, DecorativeBlocks3, Fieldwork |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_5.sbc` |
| HaulContainer_M_6 | HaulContainer_M_6 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Contact, Signal |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_M_6.sbc` |
| HaulContainer_S_1 | HaulContainer_S_1 | Small Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, Contact, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_S_1.sbc` |
| HaulContainer_S_2 | HaulContainer_S_2 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Fieldwork |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_S_2.sbc` |
| HaulContainer_S_3 | HaulContainer_S_3 | Small Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, HeavyIndustry, Signal, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_S_3.sbc` |
| HaulContainer_S_4 | HaulContainer_S_4 | Small Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_S_4.sbc` |
| HaulContainer_S_5 | HaulContainer_S_5 | Small Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_S_5.sbc` |
| HaulContainer_XL_1 | HaulContainer_XL_1 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks3, Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_XL_1.sbc` |
| HaulContainer_XL_2 | HaulContainer_XL_2 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks2, DecorativeBlocks3, Economy2, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_XL_2.sbc` |
| HaulContainer_XL_3 | HaulContainer_XL_3 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | Automatons, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_XL_3.sbc` |
| HaulContainer_XL_4 | HaulContainer_XL_4 | Large Grid | Unknown Mobility | Economy Mission - HaulingContainers | DecorativeBlocks3, Economy2, HeavyIndustry |  | `Data\Prefabs\Economy\Missions\HaulingContainers\HaulContainer_XL_4.sbc` |
| Repair_Beacon | Repair_Beacon | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Beacon.sbc` |
| Repair_Beacon2 | Repair_Beacon2 | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Beacon2.sbc` |
| Repair_Comm | Repair_Comm | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Comm.sbc` |
| Repair_EscapePod | Repair_EscapePod | Large Grid | Station/Static | Economy Mission - Repair | DecorativeBlocks |  | `Data\Prefabs\Economy\Missions\Repair\Repair_EscapePod.sbc` |
| Repair_FuelTank | Repair_FuelTank | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_FuelTank.sbc` |
| Repair_Lab | Repair_Lab | Large Grid | Station/Static | Economy Mission - Repair | DecorativeBlocks |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Lab.sbc` |
| Repair_Mine | Repair_Mine | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Mine.sbc` |
| Repair_Observatory | Repair_Observatory | Large Grid | Station/Static | Economy Mission - Repair | DecorativeBlocks |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Observatory.sbc` |
| Repair_OxygenFarm | Repair_OxygenFarm | Large Grid | Station/Static | Economy Mission - Repair | DecorativeBlocks |  | `Data\Prefabs\Economy\Missions\Repair\Repair_OxygenFarm.sbc` |
| Repair_OxygenModule | Repair_OxygenModule | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_OxygenModule.sbc` |
| Repair_PerimeterGun | Repair_PerimeterGun | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_PerimeterGun.sbc` |
| Repair_Satellite | Repair_Satellite | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Satellite.sbc` |
| Repair_Satellite2 | Repair_Satellite2 | Large Grid | Station/Static | Economy Mission - Repair |  |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Satellite2.sbc` |
| Repair_Surveyor | Repair_Surveyor | Large Grid | Station/Static | Economy Mission - Repair | DecorativeBlocks |  | `Data\Prefabs\Economy\Missions\Repair\Repair_Surveyor.sbc` |
| Salvage_Planetary_AbandonedFarm | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_AbandonedFarm | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_AbandonedFarm.sbc` |
| Salvage_Planetary_AeroShuttle | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_AeroShuttle | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_AeroShuttle.sbc` |
| Salvage_Planetary_BombSkiff | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal | Salvage_Planetary_BombSkiff | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_BombSkiff.sbc` |
| Salvage_Planetary_CargoFerry | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture | Salvage_Planetary_CargoFerry | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_CargoFerry.sbc` |
| Salvage_Planetary_EscapePod | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | Salvage_Planetary_EscapePod | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_EscapePod.sbc` |
| Salvage_Planetary_FreighterAft | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, CoreSystems, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | Salvage_Planetary_FreighterAft | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_FreighterAft.sbc` |
| Salvage_Planetary_FreighterBridge | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_FreighterBridge | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_FreighterBridge.sbc` |
| Salvage_Planetary_HydroProbe | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | Salvage_Planetary_HydroProbe | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_HydroProbe.sbc` |
| Salvage_Planetary_IonShuttle | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare1, Warfare2 | Salvage_Planetary_IonShuttle | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_IonShuttle.sbc` |
| Salvage_Planetary_MoonHopper | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, CoreSystems, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_MoonHopper | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_MoonHopper.sbc` |
| Salvage_Planetary_OldProspector | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_OldProspector | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_OldProspector.sbc` |
| Salvage_Planetary_PatrolCraft | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 | Salvage_Planetary_PatrolCraft | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_PatrolCraft.sbc` |
| Salvage_Planetary_RescueRover | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Planetary_RescueRover | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_RescueRover.sbc` |
| Salvage_Planetary_RookGunship | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture | Salvage_Planetary_RookGunship | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_RookGunship.sbc` |
| Salvage_Planetary_Satellite | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, Signal | Salvage_Planetary_Satellite | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_Satellite.sbc` |
| Salvage_Planetary_SmallHomestead | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, HeavyIndustry, Signal, Warfare2 | Salvage_Planetary_SmallHomestead | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_SmallHomestead.sbc` |
| Salvage_Planetary_SmallTransport | Salvage Target | Large Grid | Station/Static | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | Salvage_Planetary_SmallTransport | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Planetary_SmallTransport.sbc` |
| Salvage_Space_AeroShuttle | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Space_AeroShuttle | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_AeroShuttle.sbc` |
| Salvage_Space_Agripost | Salvage_Agripost | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | Salvage_Space_Agripost | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_Agripost.sbc` |
| Salvage_Space_B980Wreck | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | Salvage_Space_B980Wreck | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_B980Wreck.sbc` |
| Salvage_Space_BombSkiff | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal | Salvage_Space_BombSkiff | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_BombSkiff.sbc` |
| Salvage_Space_CargoFerry | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture | Salvage_Space_CargoFerry | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_CargoFerry.sbc` |
| Salvage_Space_EscapePod | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | Salvage_Space_EscapePod | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_EscapePod.sbc` |
| Salvage_Space_FreighterBridge | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Space_FreighterBridge | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_FreighterBridge.sbc` |
| Salvage_Space_HabModule | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Space_HabModule | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_HabModule.sbc` |
| Salvage_Space_HydroProbe | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | Salvage_Space_HydroProbe | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_HydroProbe.sbc` |
| Salvage_Space_IonShuttle | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare1, Warfare2 | Salvage_Space_IonShuttle | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_IonShuttle.sbc` |
| Salvage_Space_JackdawMk2 | Salvage Target | Small Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, HeavyIndustry, ScrapRace | Salvage_Space_JackdawMk2 | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_JackdawMk2.sbc` |
| Salvage_Space_MoonHopper | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, CoreSystems, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Space_MoonHopper | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_MoonHopper.sbc` |
| Salvage_Space_OldProspector | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Salvage_Space_OldProspector | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_OldProspector.sbc` |
| Salvage_Space_PatrolCraft | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 | Salvage_Space_PatrolCraft | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_PatrolCraft.sbc` |
| Salvage_Space_RhinoMiner | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, Fieldwork, HeavyIndustry | Salvage_Space_RhinoMiner | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_RhinoMiner.sbc` |
| Salvage_Space_RookGunship | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture | Salvage_Space_RookGunship | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_RookGunship.sbc` |
| Salvage_Space_Satellite | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, Signal | Salvage_Space_Satellite | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_Satellite.sbc` |
| Salvage_Space_SmallTransport | Salvage Target | Large Grid | Unknown Mobility | Economy Mission - Salvage | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | Salvage_Space_SmallTransport | `Data\Prefabs\Economy\Missions\Salvage\Salvage_Space_SmallTransport.sbc` |
| Escort B-980 | Escort B-980 | Large Grid | Unknown Mobility | Legacy Economy Missions |  |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort B-980.sbc` |
| Escort Freighter | Escort Freighter | Large Grid | Unknown Mobility | Legacy Economy Missions |  |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Freighter.sbc` |
| Escort HydroTanker | Escort HydroTanker | Large Grid | Unknown Mobility | Legacy Economy Missions | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort HydroTanker.sbc` |
| Escort Mining1 | Escort Mining1 | Large Grid | Unknown Mobility | Legacy Economy Missions |  |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Mining1.sbc` |
| Escort Mining2 | Escort Mining2 | Large Grid | Unknown Mobility | Legacy Economy Missions | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Mining2.sbc` |
| Escort TT-420 | Escort TT-420 | Large Grid | Unknown Mobility | Legacy Economy Missions |  |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort TT-420.sbc` |
| Escort Trade1 | Escort Trade1 | Large Grid | Unknown Mobility | Legacy Economy Missions | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Trade1.sbc` |
| Escort Trade2 | Escort Trade2 | Large Grid | Unknown Mobility | Legacy Economy Missions | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Trade2.sbc` |
| Escort Trade3 | Escort Trade3 | Large Grid | Unknown Mobility | Legacy Economy Missions | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Missions\Escort Trade3.sbc` |

### Planetary structures (46)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| P10_MilitaryBarracks | Military Barracks | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P10_MilitaryBarracks, P10_MilitaryBarracks_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P10_MilitaryBarracks.sbc` |
| P10_MilitaryBarracks_Moon | Military Barracks | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P10_MilitaryBarracks_Moon | `Data\Prefabs\PlanetaryEncounters\P10_MilitaryBarracks_Moon.sbc` |
| P11_VanguardOutpost | Vanguard Outpost | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P11_VanguardOutpost, P11_VanguardOutpost_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P11_VanguardOutpost.sbc` |
| P11_VanguardOutpost_Moon | Vanguard Outpost | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P11_VanguardOutpost_Moon | `Data\Prefabs\PlanetaryEncounters\P11_VanguardOutpost_Moon.sbc` |
| P12_MilitaryWatchtower | Military Watchtower | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P12_MilitaryWatchtower, P12_MilitaryWatchtower_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P12_MilitaryWatchtower.sbc` |
| P12_MilitaryWatchtower_Moon | Military Watchtower | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P12_MilitaryWatchtower_Moon | `Data\Prefabs\PlanetaryEncounters\P12_MilitaryWatchtower_Moon.sbc` |
| P13_PirateRelayPost | Relay Post | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P13_PirateRelayPost | `Data\Prefabs\PlanetaryEncounters\P13_PirateRelayPost.sbc` |
| P13_PirateRelayPostFrozen | P13_PirateRelayPost | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P13_PirateRelayPost_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P13_PirateRelayPostFrozen.sbc` |
| P14_FrozenSalvageYard | Salvage Yard | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | P14_SalvageYard_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P14_FrozenSalvageYard.sbc` |
| P14_SalvageYard | Salvage Yard | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | P14_SalvageYard | `Data\Prefabs\PlanetaryEncounters\P14_SalvageYard.sbc` |
| P15_PirateStronghold | Pirate Stronghold | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P15_PirateStronghold | `Data\Prefabs\PlanetaryEncounters\P15_PirateStronghold.sbc` |
| P15_PirateStrongholdFrozen | Pirate Stronghold | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P15_PirateStronghold_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P15_PirateStrongholdFrozen.sbc` |
| P16_FrozenRover | Abandoned Rover | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P16_FrozenRover, P16_FrozenRover_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P16_FrozenRover.sbc` |
| P17_RustedRover | Abandoned Rover | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P17_RustedRover | `Data\Prefabs\PlanetaryEncounters\P17_RustedRover.sbc` |
| P18_FrozenShuttle | Abandoned Shuttle | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P18_FrozenShuttle, P18_FrozenShuttle_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P18_FrozenShuttle.sbc` |
| P19_RustedShuttle | Abandoned Shuttle | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P19_RustedShuttle | `Data\Prefabs\PlanetaryEncounters\P19_RustedShuttle.sbc` |
| P1_SupplyPost | Supply Post | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P1_SupplyPost, P1_SupplyPost_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P1_SupplyPost.sbc` |
| P1_SupplyPost_Moon | Supply Post | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P1_SupplyPost_Moon | `Data\Prefabs\PlanetaryEncounters\P1_SupplyPost_Moon.sbc` |
| P20_FrozenExplorer | Abandoned Explorer | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, Frostbite, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare1, Warfare2 | P20_FrozenExplorer, P20_FrozenExplorer_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P20_FrozenExplorer.sbc` |
| P21_RustedExplorer | Abandoned Explorer | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, Frostbite, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare1, Warfare2 | P21_RustedExplorer | `Data\Prefabs\PlanetaryEncounters\P21_RustedExplorer.sbc` |
| P22_FrozenColonyShip | Abandoned Colony Ship | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P22_FrozenColonyShip, P22_FrozenColonyShip_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P22_FrozenColonyShip.sbc` |
| P23_RustedColonyShip | Abandoned Colony Ship | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P23_RustedColonyShip | `Data\Prefabs\PlanetaryEncounters\P23_RustedColonyShip.sbc` |
| P24_Alien1 | Ancient Structure | Large Grid | Station/Static | Planetary Encounter | ScrapRace, SparksOfTheFuture | P24_Alien1 | `Data\Prefabs\PlanetaryEncounters\P24_Alien1.sbc` |
| P25_Alien2 | Ancient Structure | Large Grid | Station/Static | Planetary Encounter | ScrapRace, SparksOfTheFuture | P25_Alien2 | `Data\Prefabs\PlanetaryEncounters\P25_Alien2.sbc` |
| P26_HaulerWreck | D-2 Hauler Wreck | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare2 | P26_HaulerWreckRusted | `Data\Prefabs\PlanetaryEncounters\P26_HaulerWreck.sbc` |
| P26_HaulerWreckFrozen | D-2 Hauler Wreck | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks3, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | P26_HaulerWreckFrozen, P26_HaulerWreck_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P26_HaulerWreckFrozen.sbc` |
| P27_TradeRunnerWreck | Trade Runner Wreck | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P27_TradeRunnerWreck | `Data\Prefabs\PlanetaryEncounters\P27_TradeRunnerWreck.sbc` |
| P28_LogisticsPost | Logistics Drone, Logistics Post | Mixed Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | P28_LogisticsPost | `Data\Prefabs\PlanetaryEncounters\P28_LogisticsPost.sbc` |
| P29_WreckedHopper | Wrecked Hopper | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | P29_WreckedHopper | `Data\Prefabs\PlanetaryEncounters\P29_WreckedHopper.sbc` |
| P2_StorageDepot | Storage Depot | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P2_StorageDepot, P2_StorageDepot_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P2_StorageDepot.sbc` |
| P2_StorageDepot_Moon | Storage Depot | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P2_StorageDepot_Moon | `Data\Prefabs\PlanetaryEncounters\P2_StorageDepot_Moon.sbc` |
| P30_AbandonedFarmstead | Abandoned Farmstead | Large Grid | Station/Static | Planetary Encounter | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P30_AbandonedFarmstead | `Data\Prefabs\PlanetaryEncounters\P30_AbandonedFarmstead.sbc` |
| P3_ManufacturingFacility | Manufacturing Facility | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P3_ManufacturingFacility, P3_ManufacturingFacility_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P3_ManufacturingFacility.sbc` |
| P3_ManufacturingFacility_Moon | Manufacturing Facility | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P3_ManufacturingFacility_Moon | `Data\Prefabs\PlanetaryEncounters\P3_ManufacturingFacility_Moon.sbc` |
| P4_ProcessingPlant | Processing Plant | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | P4_ProcessingPlant, P4_ProcessingPlant_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P4_ProcessingPlant.sbc` |
| P4_ProcessingPlant_Moon | Processing Plant | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | P4_ProcessingPlant_Moon | `Data\Prefabs\PlanetaryEncounters\P4_ProcessingPlant_Moon.sbc` |
| P5_ProspectingStation | Prospecting Station | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | P5_ProspectingStation, P5_ProspectingStation_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P5_ProspectingStation.sbc` |
| P5_ProspectingStation_Moon | Prospecting Station | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | P5_ProspectingStation_Moon | `Data\Prefabs\PlanetaryEncounters\P5_ProspectingStation_Moon.sbc` |
| P6_OreProcessingFacility | Ore Processing Facility | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P6_OreProcessingFacility, P6_OreProcessingFacility_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P6_OreProcessingFacility.sbc` |
| P6_OreProcessingFacility_Moon | Ore Processing Facility | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P6_OreProcessingFacility_Moon | `Data\Prefabs\PlanetaryEncounters\P6_OreProcessingFacility_Moon.sbc` |
| P7_FieldResearchOutpost | Field Research Outpost | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P7_FieldResearchOutpost, P7_FieldResearchOutpost_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P7_FieldResearchOutpost.sbc` |
| P7_FieldResearchOutpost_Moon | Field Research Outpost | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P7_FieldResearchOutpost_Moon | `Data\Prefabs\PlanetaryEncounters\P7_FieldResearchOutpost_Moon.sbc` |
| P8_ShippingPlatform | Shipping Platform | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | P8_ShippingPlatform, P8_ShippingPlatform_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P8_ShippingPlatform.sbc` |
| P8_ShippingPlatform_Moon | Shipping Platform | Large Grid | Station/Static | Planetary Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | P8_ShippingPlatform_Moon | `Data\Prefabs\PlanetaryEncounters\P8_ShippingPlatform_Moon.sbc` |
| P9_RegionalHeadquarters | Regional Headquarters | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P9_RegionalHeadquarters, P9_RegionalHeadquarters_IcePlanet | `Data\Prefabs\PlanetaryEncounters\P9_RegionalHeadquarters.sbc` |
| P9_RegionalHeadquarters_Moon | Regional Headquarters | Large Grid | Station/Static | Planetary Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | P9_RegionalHeadquarters_Moon | `Data\Prefabs\PlanetaryEncounters\P9_RegionalHeadquarters_Moon.sbc` |

### Respawn craft (2)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| RespawnRover | Respawn Rover, Wheel | Small Grid | Unknown Mobility | Respawn Ship |  | Respawn:RespawnRover, Respawn:RespawnMoonRover | `Data\Prefabs\Respawn\RespawnRover.sbc` |
| RespawnSpacePod | Space Pod, Respawn Pod | Large Grid | Unknown Mobility | Respawn Ship |  | Respawn:RespawnSpacePod | `Data\Prefabs\Respawn\RespawnSpacePod.sbc` |

### Scenario prefabs (52)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Monolith | Monolith | Large Grid | Station/Static | Scenario |  |  | `Data\Prefabs\Scenarios\Monolith.sbc` |
| DoM_Blue01 | Blue01 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Blue 01 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Blue01.sbc` |
| DoM_Blue02 | Blue02 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Blue 02 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Blue02.sbc` |
| DoM_Blue03 | Blue03 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Blue 03 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Blue03.sbc` |
| DoM_Blue04 | Blue04 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Blue 04 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Blue04.sbc` |
| DoM_Red01 | Red01 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Red 01 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Red01.sbc` |
| DoM_Red02 | Red02 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Red 02 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Red02.sbc` |
| DoM_Red03 | Red03 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Red 03 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Red03.sbc` |
| DoM_Red04 | Red04 | Small Grid | Unknown Mobility | Scenario - DeadDrop |  | DoM Red 04 | `Data\Prefabs\Scenarios\DeadDrop\DoM_Red04.sbc` |
| CoolBox | CoolBox | Large Grid | Unknown Mobility | Scenario - Frostbite | DecorativeBlocks, DecorativeBlocks2, Frostbite |  | `Data\Prefabs\Scenarios\Frostbite\CoolBox.sbc` |
| Frostbite Bombing Drone Mk2 | Frostbite Bombing Drone Mk2, Small Grid 2594, +5 more | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\Frostbite Bombing Drone Mk2.sbc` |
| GatlingDrone | GatlingDrone | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\GatlingDrone.sbc` |
| GatlingTurret Drone | GatlingTurret Drone | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\GatlingTurret Drone.sbc` |
| Large Control Drone | Large Control Drone | Large Grid | Unknown Mobility | Scenario - Frostbite | DecorativeBlocks2, Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\Large Control Drone.sbc` |
| Large Suport Drone | Large Suport Drone | Large Grid | Unknown Mobility | Scenario - Frostbite | DecorativeBlocks2, Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\Large Suport Drone.sbc` |
| MULE Drone | MULE Drone | Small Grid | Unknown Mobility | Scenario - Frostbite | DecorativeBlocks2, Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\MULE Drone.sbc` |
| Mining Drone | Mining Drone | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite |  | `Data\Prefabs\Scenarios\Frostbite\Mining Drone.sbc` |
| Scout Drone | Scout Drone | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite, StylePack |  | `Data\Prefabs\Scenarios\Frostbite\Scout Drone.sbc` |
| SmartMissile Drone | SmartMissile Drone | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite |  | `Data\Prefabs\Scenarios\Frostbite\SmartMissile Drone.sbc` |
| SpiderDrone | Small Grid 1266, Small Grid 2448, +11 more | Small Grid | Unknown Mobility | Scenario - Frostbite | Frostbite |  | `Data\Prefabs\Scenarios\Frostbite\SpiderDrone.sbc` |
| Argentavis_mk.1 | Argentavis mk.1_2 no factory | Large Grid | Unknown Mobility | Scenario - NeverSurrender |  | PirateCarrier | `Data\Prefabs\Scenarios\NeverSurrender\Argentavis_mk.1.sbc` |
| Blue Drone MK1 | Blue Drone MK1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Blue Drone MK1 | `Data\Prefabs\Scenarios\NeverSurrender\Blue Drone MK1.sbc` |
| Blue Drone MK2 | Blue Drone MK2 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Blue Drone MK2 | `Data\Prefabs\Scenarios\NeverSurrender\Blue Drone MK2.sbc` |
| BossDroneL 1RL.2GT.1MT.2IT | BossDroneL 1RL.2GT.1MT.2IT | Large Grid | Unknown Mobility | Scenario - NeverSurrender |  | BossDroneL 1RL.2GT.1MT.2IT | `Data\Prefabs\Scenarios\NeverSurrender\BossDroneL 1RL.2GT.1MT.2IT.sbc` |
| DroneL 1RL.1GG.2IT | DroneL 1RL.1GG.2IT | Large Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneL 1RL.1GG.2IT | `Data\Prefabs\Scenarios\NeverSurrender\DroneL 1RL.1GG.2IT.sbc` |
| DroneS 1GG_1 | DroneS 1GG. | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 1GG_1 | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 1GG_1.sbc` |
| DroneS 1GG_2 | DroneS 1GG. | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 1GG_2 | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 1GG_2.sbc` |
| DroneS 1GG_3 | DroneS 1GG | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 1GG_3 | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 1GG_3.sbc` |
| DroneS 1MT.2GG | DroneS 1MT.2GG | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 1MT.2GG | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 1MT.2GG.sbc` |
| DroneS 2GG.1GT | DroneS 2GG.1GT | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 2GG.1GT | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 2GG.1GT.sbc` |
| DroneS 2GG.1GT.1RL | DroneS 2GG.1GT.1RL | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 2GG.1GT.1RL | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 2GG.1GT.1RL.sbc` |
| DroneS 2GG.2GT | DroneS 2GG.2GT | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS 2GG.2GT | `Data\Prefabs\Scenarios\NeverSurrender\DroneS 2GG.2GT.sbc` |
| DroneS Drill.Warhead | DroneS Drill.Warhead | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | DroneS Drill.Warhead | `Data\Prefabs\Scenarios\NeverSurrender\DroneS Drill.Warhead.sbc` |
| Dyad_mk.1 | Dyad mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Dyad_mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Dyad_mk.1.sbc` |
| Eradicator_mk.1 | Eradicator mk.1 | Large Grid | Unknown Mobility | Scenario - NeverSurrender |  | Eradicator_mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Eradicator_mk.1.sbc` |
| Escord Drone | Escord Drone | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Escord Drone | `Data\Prefabs\Scenarios\NeverSurrender\Escord Drone.sbc` |
| Imperator_mk.1 | Imperator mk.1, Small Grid 4000, +1 more | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Imperator mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Imperator_mk.1.sbc` |
| Incisor_mk.1 | Incisor mk.1 | Large Grid | Unknown Mobility | Scenario - NeverSurrender |  | Incisor_mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Incisor_mk.1.sbc` |
| Insinuator_mk.1 | Insinuator mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Insinuator mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Insinuator_mk.1.sbc` |
| Instigator_mk.1 | Instigator mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Instigator mk.1 | `Data\Prefabs\Scenarios\NeverSurrender\Instigator_mk.1.sbc` |
| King_Molar_mk.1 | King Molar mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | KingMolar | `Data\Prefabs\Scenarios\NeverSurrender\King_Molar_mk.1.sbc` |
| Molar_mk.1 | Molar mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Molar | `Data\Prefabs\Scenarios\NeverSurrender\Molar_mk.1.sbc` |
| Red Drone MK1 | Red Drone MK1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Red Drone MK1 | `Data\Prefabs\Scenarios\NeverSurrender\Red Drone MK1.sbc` |
| Spur_mk.1 | Spur mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  | Spur | `Data\Prefabs\Scenarios\NeverSurrender\Spur_mk.1.sbc` |
| Tusk | Tusk mk.1 | Small Grid | Unknown Mobility | Scenario - NeverSurrender |  |  | `Data\Prefabs\Scenarios\NeverSurrender\Tusk.sbc` |
| ScrapRaceGarage | ScrapRaceGarage | Large Grid | Station/Static | Scenario - Scrap | DecorativeBlocks, DecorativeBlocks2, Frostbite, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\Scenarios\Scrap\ScrapRaceGarage.sbc` |
| Scrap_Blank | Scrap_Blank, Small Grid 1399, +3 more | Small Grid | Unknown Mobility | Scenario - Scrap | ScrapRace |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Blank.sbc` |
| Scrap_Buggy | Scrap_Buggy, Small Grid 2680, +3 more | Small Grid | Unknown Mobility | Scenario - Scrap | ScrapRace |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Buggy.sbc` |
| Scrap_Car | Scrap_Car, Small Grid 1744, +3 more | Small Grid | Unknown Mobility | Scenario - Scrap | ScrapRace |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Car.sbc` |
| Scrap_Hatchback | Scrap_Hatchback, Small Grid 4601, +3 more | Small Grid | Unknown Mobility | Scenario - Scrap | ScrapRace |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Hatch.sbc` |
| Scrap_Rod | Scrap_Rod, Small Grid 1310, +3 more | Small Grid | Unknown Mobility | Scenario - Scrap | ScrapRace |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Rod.sbc` |
| Scrap_Truck | Scrap Race Truck, Small Grid 2710, +5 more | Small Grid | Unknown Mobility | Scenario - Scrap | DecorativeBlocks2, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\Scenarios\Scrap\Scrap_Truck.sbc` |

### Ships (103)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| C00_Builder_Hauler | D-2 Hauler | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | Builder2 | `Data\Prefabs\CargoShips\C00_Builder_Hauler.sbc` |
| C01_Builder_MUP | Mobile Utility Platform | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Builder3 | `Data\Prefabs\CargoShips\C01_Builder_MUP.sbc` |
| C02_Builder_MobileShipyard | Mobile Shipyard, Mobile Shipyard Construction | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Builder4 | `Data\Prefabs\CargoShips\C02_Builder_MobileShipyard.sbc` |
| C03_Builder_TugDrone | Tug Drone, Tug Drone Container | Mixed Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks3, HeavyIndustry, Signal, Warfare2 | Builder1 | `Data\Prefabs\CargoShips\C03_Builder_TugDrone.sbc` |
| C10_Mining_Carriage | Mining Carriage | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | Mining2 | `Data\Prefabs\CargoShips\C10_Mining_Carriage.sbc` |
| C11_Mining_Puller | OR3-Puller | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | Mining3 | `Data\Prefabs\CargoShips\C11_Mining_Puller.sbc` |
| C12_Mining_Armed_Tender | Armed Mining Tender | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Mining4 | `Data\Prefabs\CargoShips\C12_Mining_Armed_Tender.sbc` |
| C13_Mining_SurveyDrone | Mining Survey Drone | Small Grid | Unknown Mobility | Cargo Ship | Contact, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | Mining1 | `Data\Prefabs\CargoShips\C13_Mining_SurveyDrone.sbc` |
| C20_Trade_PrivateSail | Private Sail | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, SparksOfTheFuture, Warfare2 | Trade3 | `Data\Prefabs\CargoShips\C20_Trade_PrivateSail.sbc` |
| C21_Trade_ConsignmentDrone | Solar Consignment Drone | Small Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, Signal, Warfare2 | Trade1 | `Data\Prefabs\CargoShips\C21_Trade_ConsignmentDrone.sbc` |
| C22_Trade_Merchant | Twinsail Merchant | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | Trade4 | `Data\Prefabs\CargoShips\C22_Trade_Merchant.sbc` |
| C23_Trade_SolarBarge | Solar Barge | Large Grid | Unknown Mobility | Cargo Ship | Contact, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, Warfare2 | Trade2 | `Data\Prefabs\CargoShips\C23_Trade_SolarBarge.sbc` |
| C30_Military_Escort | Military Escort | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | Military2 | `Data\Prefabs\CargoShips\C30_Military_Escort.sbc` |
| C32_Military_MissileFrigate | Military Missile Frigate | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | Military3 | `Data\Prefabs\CargoShips\C32_Military_MissileFrigate.sbc` |
| C33_Military_Enforcer | Military Enforcer | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 | Military4 | `Data\Prefabs\CargoShips\C33_Military_Enforcer.sbc` |
| C34_Military_Munitions_Drone | Military Munitions Drone | Small Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, Signal, StylePack, Warfare2 | Military1 | `Data\Prefabs\CargoShips\C34_Military_Munitions_Drone.sbc` |
| C40_Pirate_Vulture | Vulture | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 | Pirate2 | `Data\Prefabs\CargoShips\C40_Pirate_Vulture.sbc` |
| C41_Pirate_Carriage | Pirated Mining Carriage | Large Grid | Unknown Mobility | Cargo Ship | Automatons, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare2 | Pirate3 | `Data\Prefabs\CargoShips\C41_Pirate_Carriage.sbc` |
| C42_Pirate_SalvageCarrier | Pirate Salvage Carrier | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Pirate4 | `Data\Prefabs\CargoShips\C42_Pirate_SalvageCarrier.sbc` |
| C43_Pirate_ScoutDrone | Pirate Scout Drone | Small Grid | Unknown Mobility | Cargo Ship | Contact, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | Pirate1 | `Data\Prefabs\CargoShips\C43_Pirate_ScoutDrone.sbc` |
| C51_Derelict_Shuttle | Medical Shuttle | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | Unknown2 | `Data\Prefabs\CargoShips\C51_Derelict_Shuttle.sbc` |
| C52_Derelict_ObserverDrone | Observer Drone | Small Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, Warfare2 | Unknown3 | `Data\Prefabs\CargoShips\C52_Derelict_ObserverDrone.sbc` |
| C55_Derelict_CommercialHopper | Commercial Hopper | Large Grid | Unknown Mobility | Cargo Ship | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Frostbite, HeavyIndustry, Signal, Warfare1, Warfare2 | Unknown1 | `Data\Prefabs\CargoShips\C55_Derelict_CommercialHopper.sbc` |
| Eco_Missile_LM3LateralMissile | LM-3 Lateral Missile, LM-3 Lateral Missile | Small Grid | Unknown Mobility | Economy Sales | Automatons, DecorativeBlocks3, HeavyIndustry, StylePack |  | `Data\Prefabs\Economy\Sales\Eco_Missile_LM3LateralMissile.sbc` |
| Eco_Rover_LunarScoutMk2 | Lunar Scout Mk.2, Lunar Scout Mk.2, Small Grid 2335, +5 more | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems |  | `Data\Prefabs\Economy\Sales\Eco_Rover_LunarScoutMk2.sbc` |
| Eco_Rover_LunarScoutMk6 | Lunar Scout Mk.6, Lunar Scout Mk.6, Small Grid 3655, +5 more | Small Grid | Unknown Mobility | Economy Sales | Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Rover_LunarScoutMk6.sbc` |
| Eco_Rover_OR7Gravler | OR-7 Gravler, OR-7 Gravler, Small Grid 2490, +9 more | Small Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, HeavyIndustry, ScrapRace, Signal |  | `Data\Prefabs\Economy\Sales\Eco_Rover_OR7Gravler.sbc` |
| Eco_Ship_AmbassadorExplorer | Ambassador Explorer, Ambassador Explorer | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_AmbassadorExplorer.sbc` |
| Eco_Ship_B60BulkFreighter | B-60 Bulk Freighter, B-60 Bulk Freighter | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_B60BulkFreighter.sbc` |
| Eco_Ship_B980Hauler | B-980 Hauler, B-980 Hauler | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_B980Hauler.sbc` |
| Eco_Ship_BuddyMiner | Buddy Miner, Buddy Miner | Small Grid | Unknown Mobility | Economy Sales | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_BuddyMiner.sbc` |
| Eco_Ship_CR2Shrike | CR-2 Shrike, CR-2 Shrike | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_CR2Shrike.sbc` |
| Eco_Ship_CivicHauler | Civic Hauler, Civic Hauler | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_CivicHauler.sbc` |
| Eco_Ship_H01Prospector | H-01 Prospector, H-01 Prospector | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_H01Prospector.sbc` |
| Eco_Ship_H01Sapper | H-01 Sapper, H-01 Sapper | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, Fieldwork, HeavyIndustry |  | `Data\Prefabs\Economy\Sales\Eco_Ship_H01Sapper.sbc` |
| Eco_Ship_H04Advanced | H-04 Advanced, H-04 Advanced | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture |  | `Data\Prefabs\Economy\Sales\Eco_Ship_H04Advanced.sbc` |
| Eco_Ship_H1IndustrialCorvette | H-1 Industrial Corvette, H-1 Industrial Corvette | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_H1IndustrialCorvette.sbc` |
| Eco_Ship_H2BullwarkHauler | H2-Bullwark Hauler, H2-Bulwark Hauler | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, SparksOfTheFuture, StylePack, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_H2BullwarkHauler.sbc` |
| Eco_Ship_HF3NarrowframeFreighter | HF-3 Narrowframe Freighter, HF-3 Narrowframe Freighter | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HF3NarrowframeFreighter.sbc` |
| Eco_Ship_HaulFrameC | HaulFrame-C, HaulFrame-C | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HaulFrameC.sbc` |
| Eco_Ship_HaulFrameJ | HaulFrame-J, HaulFrame-J | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HaulFrameJ.sbc` |
| Eco_Ship_HeavyModuleFreighter | Heavy Module Freighter, Heavy Module Freighter | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HeavyModuleFreighter.sbc` |
| Eco_Ship_HorizonForge | Horizon Forge, Horizon Forge | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, StylePack, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HorizonForge.sbc` |
| Eco_Ship_HullBreaker | Hull Breaker, Hull Breaker | Small Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_HullBreaker.sbc` |
| Eco_Ship_JClassCourier | J-Class Courier, J-Class Courier | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_JClassCourier.sbc` |
| Eco_Ship_Jackdaw | Jackdaw, Jackdaw | Small Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks3, HeavyIndustry, ScrapRace |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Jackdaw.sbc` |
| Eco_Ship_KiteMiner | Kite Miner, Kite Miner | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_KiteMiner.sbc` |
| Eco_Ship_LightSupplyCraft | Light Supply Craft, Light Supply Craft | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_LightSupplyCraft.sbc` |
| Eco_Ship_LunarArk | Lunar Ark, Lunar Ark | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, Signal, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_LunarArk.sbc` |
| Eco_Ship_MiniMerchant | MiniMerchant, MiniMerchant | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems, Fieldwork, HeavyIndustry |  | `Data\Prefabs\Economy\Sales\Eco_Ship_MiniMerchant.sbc` |
| Eco_Ship_Pirate_CR2Shrike | CR-2 Shrike, CR-2 Shrike | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_CR2Shrike.sbc` |
| Eco_Ship_Pirate_HaulFrameJ | HaulFrame-J, HaulFrame-J | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_HaulFrameJ.sbc` |
| Eco_Ship_Pirate_HullBreaker | Hull Breaker, Hull Breaker | Small Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_HullBreaker.sbc` |
| Eco_Ship_Pirate_JClassCourier | J-Class Courier, J-Class Courier | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_JClassCourier.sbc` |
| Eco_Ship_Pirate_MiniMerchant | MiniMerchant, MiniMerchant | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_MiniMerchant.sbc` |
| Eco_Ship_Pirate_RookGunship | Rook Gunship, Rook Gunship | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_Pirate_RookGunship.sbc` |
| Eco_Ship_R3StartSkiff | R3-Start Skiff, R3-Start Skiff | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_R3StartSkiff.sbc` |
| Eco_Ship_RST6Salvador | RST-6 Salvador, RST-6 Salvador | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_RST6Salvador.sbc` |
| Eco_Ship_RhinoMiner | Rhino Miner, Rhino Miner | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, Fieldwork, HeavyIndustry |  | `Data\Prefabs\Economy\Sales\Eco_Ship_RhinoMiner.sbc` |
| Eco_Ship_RookGunship | Rook Gunship, Rook Gunship | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, StylePack, Warfare1 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_RookGunship.sbc` |
| Eco_Ship_SnubShip | Snub Ship, Snub Ship | Small Grid | Unknown Mobility | Economy Sales | Automatons, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\Economy\Sales\Eco_Ship_SnubShip.sbc` |
| Eco_Ship_TIonFade | T-Ion Fade, T-Ion Fade | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_TIonFade.sbc` |
| Eco_Ship_TurtleMiner | Turtle Miner, Turtle Miner | Small Grid | Unknown Mobility | Economy Sales | DecorativeBlocks, DecorativeBlocks3, HeavyIndustry, ScrapRace, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_TurtleMiner.sbc` |
| Eco_Ship_TwinRailDrone | TwinRail Drone, TwinRail Drone | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, HeavyIndustry, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_TwinRailDrone.sbc` |
| Eco_Ship_WardenStrikeCraft | Warden Strike Craft, Warden Strike Craft | Small Grid | Unknown Mobility | Economy Sales | Automatons, Contact, CoreSystems, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, StylePack, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_WardenStrikeCraft.sbc` |
| Eco_Ship_WaylineSutler | Wayline Sutler, Wayline Sutler | Large Grid | Unknown Mobility | Economy Sales | ApexSurvival, Automatons, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\Economy\Sales\Eco_Ship_WaylineSutler.sbc` |
| ATV-Survivor | ATV-Survivor, ATV-Survivor, Small Grid 3225, +5 more | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\ATV-Survivor.sbc` |
| Aggressive Miner | Aggressive Miner, Aggressive Miner | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Aggressive Miner.sbc` |
| Atmo Constructor | Atmo Constructor, Atmo Constructor | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Atmo Constructor.sbc` |
| B-60 Bulk Freighter | B-60 Bulk Freighter, B-60 Bulk Freighter | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\B-60 Bulk Freighter.sbc` |
| B-980 Hauler | B-980 Hauler, B-980 Hauler | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\B-980 Hauler.sbc` |
| Blue Ambassador Explorer | Blue Ambassador Explorer, Blue Ambassador Explorer | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Blue Ambassador Explorer.sbc` |
| Buddy Miner | Buddy Miner, Buddy Miner | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Buddy Miner.sbc` |
| Burstfire Bomber | Burstfire Bomber, Burstfire Bomber | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Burstfire Bomber.sbc` |
| Cargo Shuttle | Cargo Shuttle, Cargo Shuttle | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Cargo Shuttle.sbc` |
| Civic Hauler | Civic Hauler, Civic Hauler | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Civic Hauler.sbc` |
| Cursor | Cursor, Cursor | Small Grid | Unknown Mobility | Legacy Economy Sales |  | Cursor | `Data\Prefabs\LegacyContent\Economy\Sales\Cursor.sbc` |
| Fighter2 | Fighter Mk.2, Fighter Mk.2 | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Fighter2.sbc` |
| Gerbil Miner | Gerbil Miner, Gerbil Miner | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Gerbil Miner.sbc` |
| H-01 Prospector | H-01 Prospector, H-01 Prospector | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\H-01 Prospector.sbc` |
| H-01 Sapper | H-01 Sapper, H-01 Sapper | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\H-01 Sapper.sbc` |
| Hydro Scout Rover | Hydro Scout Rover, Hydro Scout Rover, Small Grid 2790, +2 more | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Hydro Scout Rover.sbc` |
| Ion Constructor | Ion Constructor, Ion Constructor | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Ion Constructor.sbc` |
| Ion Light Scout | Ion Light Scout, Ion Light Scout | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Ion Light Scout.sbc` |
| Ion Tug Ship | Ion Tug Ship, Ion Tug Ship | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Ion Tug Ship.sbc` |
| J-Class Courier | J-Class Courier, J-Class Courier | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\J-Class Courier.sbc` |
| Kite Miner | Kite Miner, Kite Miner | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Kite Miner.sbc` |
| LCC-3 Freighter | LCC-3 Freighter, LCC-3 Freighter | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\LCC-3 Freighter.sbc` |
| Lunar Scout mk.2 | Lunar Scout mk.2, Lunar Scout mk.2, Small Grid 2335, +5 more | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Lunar Scout mk.2.sbc` |
| Lunar Scout mk.4 | Lunar Scout mk.4, Lunar Scout mk.4, Small Grid 3132, +5 more | Small Grid | Unknown Mobility | Legacy Economy Sales | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Sales\Lunar Scout mk.4.sbc` |
| MinerSmallShip | Miner mk.1, Miner Mk.1 | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\MinerSmallShip.sbc` |
| MiniMerchant | MiniMerchant, MiniMerchant | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\MiniMerchant.sbc` |
| PV-4 Buggy | PV-4 Buggy, PV-4 Buggy, Small Grid 1267, +3 more | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\PV-4 Buggy.sbc` |
| Red Cruiser | Red Cruiser, Red Cruiser | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Red Cruiser.sbc` |
| Rescue Rover 1 | Rescue Rover 1, Large Grid 2193, Large Grid 6483, +3 more | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Rescue Rover 1.sbc` |
| RespawnShip | Starter Ship mk.1, RespawnShip | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\RespawnShip.sbc` |
| Scout Miner | Scout Miner, Scout Miner | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Scout Miner.sbc` |
| Solar Scout | Solar Scout, Solar Scout | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\Solar Scout.sbc` |
| T-Ion Fade | T-Ion Fade, T-Ion Fade | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\T-Ion Fade.sbc` |
| TT-15 Freighter | TT-15 Freighter, TT-15 Freighter | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\TT-15 Freighter.sbc` |
| TT-420 Freighter | TT-420 Freighter, TT-420 Freighter | Large Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\TT-420 Freighter.sbc` |
| Turtle Miner | Turtle Miner, Turtle Miner | Small Grid | Unknown Mobility | Legacy Economy Sales | DecorativeBlocks |  | `Data\Prefabs\LegacyContent\Economy\Sales\Turtle Miner.sbc` |
| U-92 Patrol Craft | U-92 Patrol Craft, U-92 Patrol Craft | Small Grid | Unknown Mobility | Legacy Economy Sales |  |  | `Data\Prefabs\LegacyContent\Economy\Sales\U-92 Patrol Craft.sbc` |

### Space and asteroid encounters (32)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| RE01_FactorumRelaySatellite | Factorum Relay Satellite | Large Grid | Station/Static | Random Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture | RE01_FactorumRelaySatellite | `Data\Prefabs\RandomEncounters\RE01_FactorumRelaySatellite.sbc` |
| RE02_Hullbreaker | Debris, HullBreaker, +1 more | Mixed Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | RE02_Hullbreaker | `Data\Prefabs\RandomEncounters\RE02_Hullbreaker.sbc` |
| RE03_PirateOutpost | Pirate Missile, Pirate Outpost | Mixed Grid | Station/Static | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE03_PirateOutpost | `Data\Prefabs\RandomEncounters\RE03_PirateOutpost.sbc` |
| RE04_SafehouseStation | Safehouse Station | Large Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare2 | RE04_Safehouse_Station | `Data\Prefabs\RandomEncounters\RE04_SafehouseStation.sbc` |
| RE05_StagingStation | Dead Engineer, Staging Station, +1 more | Large Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | RE05_StagingStation | `Data\Prefabs\RandomEncounters\RE05_StagingStation.sbc` |
| RE06_ScienceShuttle | Debris, Science Shuttle | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE06_ScienceShuttle_A, RE06_ScienceShuttle_B | `Data\Prefabs\RandomEncounters\RE06_ScienceShuttle.sbc` |
| RE07_PirateSmuggler | Pirate Smuggler | Large Grid | Unknown Mobility | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE07_PirateSmuggler | `Data\Prefabs\RandomEncounters\RE07_PirateSmuggler.sbc` |
| RE08_LostTradeRunner | Debris, Lost Trade Runner | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | RE08_LostTradeRunner | `Data\Prefabs\RandomEncounters\RE08_LostTradeRunner.sbc` |
| RE09_PirateCutter | Pirate Cutter | Large Grid | Unknown Mobility | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, Signal, Warfare1, Warfare2 | DS_PirateCutter, RE09_PirateCutter | `Data\Prefabs\RandomEncounters\RE09_PirateCutter.sbc` |
| RE10_Privateer | Pirate Privateer | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE10_Privateer | `Data\Prefabs\RandomEncounters\RE10_Privateer.sbc` |
| RE11_SolarBargeWreck | Debris, Solar Barge Wreck Aft, +1 more | Large Grid | Station/Static | Random Encounter | Contact, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, Signal, Warfare2 | RE11_SolarBargeWreck | `Data\Prefabs\RandomEncounters\RE11_SolarBargeWreck.sbc` |
| RE12_D2HaulerWreck | D-2 Hauler Wreck Aft, D-2 Hauler Wreck Bow, +1 more | Mixed Grid | Unknown Mobility | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 | RE12_D2HaulerWreckA, RE12_D2HaulerWreckB, RE12_D2HaulerWreckC | `Data\Prefabs\RandomEncounters\RE12_D2HaulerWreck.sbc` |
| RE13_WreckedHauler | Wrecked Hauler Aft, Wrecked Hauler Bow, +1 more | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE13_WreckedHauler | `Data\Prefabs\RandomEncounters\RE13_WreckedHauler.sbc` |
| RE14_StratoFreighterWreck | Debris, StratoFreighter Wreck | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | RE14_StratoFreighterWreck_A, RE14_StratoFreighterWreck_B, RE14_StratoFreighterWreck_C | `Data\Prefabs\RandomEncounters\RE14_StratoFreighterWreck.sbc` |
| RE15_PrivateSailWreck | Debris, Private Sail Wreck | Large Grid | Station/Static | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, SparksOfTheFuture, Warfare2 | RE15_PrivateSailWreckA, RE15_PrivateSailWreckB | `Data\Prefabs\RandomEncounters\RE15_PrivateSailWreck.sbc` |
| RE16_CorvetteWreck | Debris, Wrecked Corvette | Large Grid | Unknown Mobility | Random Encounter | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE16_CorvetteWreck_A, RE16_CorvetteWreck_B | `Data\Prefabs\RandomEncounters\RE16_CorvetteWreck.sbc` |
| RE17_FracturedStation | Barrel, Dead Engineer, +4 more | Large Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Fieldwork, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, StylePack, Warfare2 | RE17_FracturedStation | `Data\Prefabs\RandomEncounters\RE17_FracturedStation.sbc` |
| RE18_AutonomousGunShip | Autonomous GunShip | Large Grid | Unknown Mobility | Random Encounter | Automatons, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, Warfare2 | RE18_AutonomousGunShip | `Data\Prefabs\RandomEncounters\RE18_AutonomousGunShip.sbc` |
| RE19_PirateDepot | Debris, Defence Drone, +1 more | Mixed Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE19_PirateDepot | `Data\Prefabs\RandomEncounters\RE19_PirateDepot.sbc` |
| RE20_MoonmothShuttle | Dead Engineer, Moonmoth-7 | Mixed Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE20_MoonmothShuttle_A, RE20_MoonmothShuttle_B | `Data\Prefabs\RandomEncounters\RE20_MoonmothShuttle.sbc` |
| RE21_AssemblyBarge | Damaged Assembly Barge | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE21_AssemblyBarge | `Data\Prefabs\RandomEncounters\RE21_AssemblyBarge.sbc` |
| RE22_Modules_A | Cargo module, Debris, +5 more | Mixed Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | RE22_Modules_A | `Data\Prefabs\RandomEncounters\RE22_Modules_A.sbc` |
| RE22_Modules_B | Ambush Drone, Cargo module, +3 more | Mixed Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | RE22_Modules_B | `Data\Prefabs\RandomEncounters\RE22_Modules_B.sbc` |
| RE22_Modules_C | Cargo module, Debris, +4 more | Large Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare1, Warfare2 | RE22_Modules_C | `Data\Prefabs\RandomEncounters\RE22_Modules_C.sbc` |
| RE23_BioRoid | BioStalk, Damaged Miner, +2 more | Mixed Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, StylePack, Warfare2 | RE23_BioRoid | `Data\Prefabs\RandomEncounters\RE23_BioRoid.sbc` |
| RE24_PirateHideout | Container, Debris, +1 more | Large Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | RE24_PirateHideout | `Data\Prefabs\RandomEncounters\RE24_PirateHideout.sbc` |
| RE25_DistressedVessel | Dead Engineer, Distressed Vessel | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE25_DistressedVessel_A, RE25_DistressedVessel_B | `Data\Prefabs\RandomEncounters\RE25_DistressedVessel.sbc` |
| RE26_Mercenary2 | Barrel, Body, +2 more | Large Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE26_Mercenary2 | `Data\Prefabs\RandomEncounters\RE26_Mercenary2.sbc` |
| RE27_OldRelayAntenna | Old Relay Antenna | Large Grid | Station/Static | Random Encounter | Automatons, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal | RE27_OldRelayAntenna_A, RE27_OldRelayAntenna_B, RE27_OldRelayAntenna_C | `Data\Prefabs\RandomEncounters\RE27_OldRelayAntenna.sbc` |
| RE28_PirateBeacon | Pirate Beacon | Large Grid | Station/Static | Random Encounter | ApexSurvival, Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, Warfare2 | RE28_PirateBeacon | `Data\Prefabs\RandomEncounters\RE28_PirateBeacon.sbc` |
| RE29_OldSalvager | Old Salvage Vessel, Salvage Container | Large Grid | Unknown Mobility | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | RE29_OldSalvager | `Data\Prefabs\RandomEncounters\RE29_OldSalvager.sbc` |
| RE30_CrazedMiner | Crazed Miner, Crazed Miner Station | Mixed Grid | Station/Static | Random Encounter | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | RE30_CrazedMiner | `Data\Prefabs\RandomEncounters\RE30_CrazedMiner.sbc` |

### Stations and outposts (102)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Economy_Outpost_10 | Economy_Outpost_10 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1 | StationList:Outposts, StationList:Outposts, StationList:Outposts, StationList:Outposts, +1 more | `Data\Prefabs\Economy\Stations\Economy_Outpost_10.sbc` |
| Economy_Outpost_11 | Economy_Outpost_11 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts, StationList:Outposts, StationList:Outposts, StationList:Outposts, +1 more | `Data\Prefabs\Economy\Stations\Economy_Outpost_11.sbc` |
| Economy_Outpost_11_Pirate | Economy_Outpost_11_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts | `Data\Prefabs\Economy\Stations\Economy_Outpost_11_Pirate.sbc` |
| Economy_Outpost_12 | Economy_Outpost_12 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts, StationList:Outposts, StationList:Outposts, StationList:Outposts, +1 more | `Data\Prefabs\Economy\Stations\Economy_Outpost_12.sbc` |
| Economy_Outpost_12_Pirate | Economy_Outpost_12_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts, StationList:Outposts | `Data\Prefabs\Economy\Stations\Economy_Outpost_12_Pirate.sbc` |
| Economy_Outpost_13 | Economy_Outpost_13 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts, StationList:Outposts, StationList:Outposts, StationList:Outposts, +1 more | `Data\Prefabs\Economy\Stations\Economy_Outpost_13.sbc` |
| Economy_Outpost_13_Pirate | Economy_Outpost_13_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts | `Data\Prefabs\Economy\Stations\Economy_Outpost_13_Pirate.sbc` |
| Economy_Outpost_14 | Economy_Outpost_14 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:Outposts, StationList:Outposts, StationList:Outposts, StationList:Outposts, +1 more | `Data\Prefabs\Economy\Stations\Economy_Outpost_14.sbc` |
| Economy_SpaceStation_10 | Economy_SpaceStation_10 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_10.sbc` |
| Economy_SpaceStation_11 | Economy_SpaceStation_11 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_11.sbc` |
| Economy_SpaceStation_11_Pirate | Economy_SpaceStation_11_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:SpaceStations | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_11_Pirate.sbc` |
| Economy_SpaceStation_12 | Economy_SpaceStation_12 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_12.sbc` |
| Economy_SpaceStation_13 | Economy_SpaceStation_13 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_13.sbc` |
| Economy_SpaceStation_14 | Economy_SpaceStation_14 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_14.sbc` |
| Economy_SpaceStation_14_Pirate | Economy_SpaceStation_14_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:SpaceStations | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_14_Pirate.sbc` |
| Economy_SpaceStation_15 | Economy_SpaceStation_15 | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, StationList:OrbitalStations, +6 more | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_15.sbc` |
| Economy_SpaceStation_15_Pirate | Economy_SpaceStation_15_Pirate | Large Grid | Station/Static | Economy Stations | ApexSurvival, Automatons, Contact, CoreSystems, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Economy, Economy2, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 | StationList:OrbitalStations, StationList:SpaceStations | `Data\Prefabs\Economy\Stations\Economy_SpaceStation_15_Pirate.sbc` |
| AlienEasyStation | AlienEasyStation | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\AlienEasyStation.sbc` |
| Base1 |  | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Base1.sbc` |
| Base4 | Large Grid 1099, Large Grid 4557, +4 more | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Base4.sbc` |
| BaseEasyStart1 | Platform 2310 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\BaseEasyStart1.sbc` |
| BaseLoneSurvivor | Platform 1005 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\BaseLoneSurvivor.sbc` |
| Civil_Station | Civil Station | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Civil_Station.sbc` |
| EarthEasyStation | EarthEasyStation | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\EarthEasyStation.sbc` |
| EasyStartPirate | Pirate Planetary Base Small mk.1_9 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\EasyStartPirate.sbc` |
| EasyStartSpacePirate | Pirate Planetary Base Small mk.1_9 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\EasyStartSpacePirate.sbc` |
| Encounter Ambasador A | Ambasador Debree, Encounter Ambasador | Large Grid | Station/Static | Legacy Content |  | Encounter Ambasador A | `Data\Prefabs\LegacyContent\Encounter Ambasador A.sbc` |
| Encounter Ambasador B | Ambasador Debree, Encounter Ambasador, +1 more | Mixed Grid | Station/Static | Legacy Content |  | Encounter Ambasador B | `Data\Prefabs\LegacyContent\Encounter Ambasador B.sbc` |
| Encounter Blue frame | Encounter Blue frame, Encounter Constructor mk.2 | Mixed Grid | Station/Static | Legacy Content |  | Encounter Blue frame | `Data\Prefabs\LegacyContent\Encounter Blue frame.sbc` |
| Encounter Corvette A | Corvette Debree, Corvette Miner, +1 more | Mixed Grid | Station/Static | Legacy Content |  | Encounter Corvette A | `Data\Prefabs\LegacyContent\Encounter Corvette A.sbc` |
| Encounter Corvette B | Corvette Debree, Encounter Corvette | Large Grid | Station/Static | Legacy Content |  | Encounter Corvette B | `Data\Prefabs\LegacyContent\Encounter Corvette B.sbc` |
| Encounter Debris A | Encounter Debris, Large Grid 1768, +8 more | Large Grid | Station/Static | Legacy Content |  | Encounter Debris A | `Data\Prefabs\LegacyContent\Encounter Debris A.sbc` |
| Encounter Debris B | Encounter Debris, Large Grid 1768, +8 more | Large Grid | Station/Static | Legacy Content |  | Encounter Debris B | `Data\Prefabs\LegacyContent\Encounter Debris B.sbc` |
| Encounter Droneyard | Encounter Droneyard | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Droneyard | `Data\Prefabs\LegacyContent\Encounter Droneyard.sbc` |
| Encounter HEC Debris A | Encounter HEC Debris, HEC Debris | Large Grid | Station/Static | Legacy Content |  | Encounter HEC Debris A | `Data\Prefabs\LegacyContent\Encounter HEC Debris A.sbc` |
| Encounter HEC Debris B | HEC Debris | Large Grid | Station/Static | Legacy Content |  | Encounter HEC Debris B | `Data\Prefabs\LegacyContent\Encounter HEC Debris B.sbc` |
| Encounter Haunted Section | Debris, Encounter Haunted Section | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Haunted Section | `Data\Prefabs\LegacyContent\Encounter Haunted Section.sbc` |
| Encounter Hermit Station | Encounter Hermit Station, Hermit Station Storage, +1 more | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Hermit Station | `Data\Prefabs\LegacyContent\Encounter Hermit Station.sbc` |
| Encounter Hydro Tanker | Dead pilot, Encounter Hydro Tanker, +1 more | Mixed Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Hydro Tanker | `Data\Prefabs\LegacyContent\Encounter Hydro Tanker.sbc` |
| Encounter Industrial Ship A | Encounter Industrial Ship, Industrial Ship Debree | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Industrial Ship A.sbc` |
| Encounter Industrial Ship B | Encounter Industrial Ship, Industrial Ship Debree | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Industrial Ship B.sbc` |
| Encounter Mercenary | Dead Engineer, Debree, +6 more | Mixed Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Mercenary.sbc` |
| Encounter Mercenary Wreckage | Dead Engineer, Debris, +4 more | Mixed Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Mercenary Wreckage | `Data\Prefabs\LegacyContent\Encounter Mercenary Wreckage.sbc` |
| Encounter Minefield | Debris, Encounter small cargo vessel, +3 more | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Minefield | `Data\Prefabs\LegacyContent\Encounter Minefield.sbc` |
| Encounter Mining Outpost | Encounter Mining Outpost, Turret | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Mining Outpost | `Data\Prefabs\LegacyContent\Encounter Mining Outpost.sbc` |
| Encounter Mining Vessel | Daniel A. Collins, Encounter Mining Vessel, +3 more | Mixed Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Mining Vessel | `Data\Prefabs\LegacyContent\Encounter Mining Vessel.sbc` |
| Encounter Pilgrim A | Encounter Pilgrim Front, Encounter Pilgrim Pod, +1 more | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Pilgrim A.sbc` |
| Encounter Pilgrim B | Encounter Pilgrim Front, Encounter Pilgrim Pod, +1 more | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter Pilgrim B.sbc` |
| Encounter Prometheus A | Encounter Prometheus, Prometheus Debris, +1 more | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Prometheus A | `Data\Prefabs\LegacyContent\Encounter Prometheus A.sbc` |
| Encounter Prometheus B | Encounter Prometheus, Prometheus Debris, +1 more | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Prometheus B | `Data\Prefabs\LegacyContent\Encounter Prometheus B.sbc` |
| Encounter RS-1217 Transporter A | Encounter RS-1217 Transporter, Large Grid 3230, +1 more | Large Grid | Station/Static | Legacy Content |  | Encounter RS-1217 Transporter A | `Data\Prefabs\LegacyContent\Encounter RS-1217 Transporter A.sbc` |
| Encounter RS-1217 Transporter B | Encounter RS-1217 Transporter, Transporter debree, +1 more | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Encounter RS-1217 Transporter B.sbc` |
| Encounter Red A | Dead Engineers (Colorable) Vanilla Small Grid, Debris, +1 more | Mixed Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Red A | `Data\Prefabs\LegacyContent\Encounter Red A.sbc` |
| Encounter Red B | Dead Engineers (Colorable) Vanilla Small Grid, Debris, +1 more | Mixed Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Red B | `Data\Prefabs\LegacyContent\Encounter Red B.sbc` |
| Encounter RoidStation | Encounter RoidStation | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter RoidStation | `Data\Prefabs\LegacyContent\Encounter RoidStation.sbc` |
| Encounter Safehouse station | Encounter Safehouse station | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Safehouse station | `Data\Prefabs\LegacyContent\Encounter Safehouse station.sbc` |
| Encounter Section-F | Encounter Section-F, Section-F Debris | Large Grid | Station/Static | Legacy Content | DecorativeBlocks | Encounter Section-F | `Data\Prefabs\LegacyContent\Encounter Section-F.sbc` |
| Encounter Skyheart A | Encounter Skyheart, Skyheart Debree | Large Grid | Station/Static | Legacy Content |  | Encounter Skyheart A | `Data\Prefabs\LegacyContent\Encounter Skyheart A.sbc` |
| Encounter Skyheart B | Dead Pilot, Encounter Skyheart, +1 more | Mixed Grid | Station/Static | Legacy Content |  | Encounter Skyheart B | `Data\Prefabs\LegacyContent\Encounter Skyheart B.sbc` |
| Encounter Stingray II A | Encounter Stingray II, Stingray Debree | Large Grid | Station/Static | Legacy Content |  | Encounter Stingray II A | `Data\Prefabs\LegacyContent\Encounter Stingray II A.sbc` |
| Encounter Stingray II B | Encounter Stingray II, Stingray Debree | Large Grid | Station/Static | Legacy Content |  | Encounter Stingray II B | `Data\Prefabs\LegacyContent\Encounter Stingray II B.sbc` |
| MarsEasyStation | MarsEasyStation | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\MarsEasyStation.sbc` |
| MoonEasyStation | MoonEasyStation | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\MoonEasyStation.sbc` |
| NewStationPrefab |  | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewStationPrefab.sbc` |
| NewStationPrefabSurvival |  | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\NewStationPrefabSurvival.sbc` |
| Old Mining Drone | Dead Engineers, Old Mining Drone | Mixed Grid | Station/Static | Legacy Content |  | Old Mining Drone, Encounter RoidStation | `Data\Prefabs\LegacyContent\Old Mining Drone.sbc` |
| Ore_Detection_Station_mk.1 | Ore Detection Station mk.1 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Ore_Detection_Station_mk.1.sbc` |
| PirateBase | Pirate Base | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\PirateBase.sbc` |
| Pirate_Base_Large_mk.2 | Pirate Base Large mk.2 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Pirate_Base_Large_mk.2.sbc` |
| Pirate_Base_Medium_mk.1 | Pirate Base Medium mk.1 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Pirate_Base_Medium_mk.1.sbc` |
| Platform4328 | Platform 9870 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform4328.sbc` |
| Platform_417 | Platform 417 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_417.sbc` |
| Platform_6674 | Platform 6674 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_6674.sbc` |
| Platform_7912 | Platform 7912 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_7912.sbc` |
| Platform_8820 | Platform 8820 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_8820.sbc` |
| Platform_9178 | Platform 9178 | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Platform_9178.sbc` |
| Raiding_Station_-_Scourge | Raiding Station - Scourge | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Raiding_Station_-_Scourge.sbc` |
| Station_8315622 |  | Large Grid | Station/Static | Legacy Content |  |  | `Data\Prefabs\LegacyContent\Station_8315622.sbc` |
| Economy_MiningStation_1 | Economy_MiningStation_1 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2 |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_MiningStation_1.sbc` |
| Economy_MiningStation_2 | Economy_MiningStation_2 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_MiningStation_2.sbc` |
| Economy_MiningStation_3 | Economy_MiningStation_3 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_MiningStation_3.sbc` |
| Economy_OrbitalStation_1 | Economy_OrbitalStation_1 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_1.sbc` |
| Economy_OrbitalStation_2 | Economy_OrbitalStation_2 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_2.sbc` |
| Economy_OrbitalStation_3 | Economy_OrbitalStation_3 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_3.sbc` |
| Economy_OrbitalStation_4 | Economy_OrbitalStation_4 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_4.sbc` |
| Economy_OrbitalStation_5 | Economy_OrbitalStation_5 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_5.sbc` |
| Economy_OrbitalStation_6 | Economy_OrbitalStation_6 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2 |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_6.sbc` |
| Economy_OrbitalStation_7 | Economy_OrbitalStation_7 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite, StylePack, Warfare2 |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_OrbitalStation_7.sbc` |
| Economy_Outpost_1 | Economy_Outpost_1 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_1.sbc` |
| Economy_Outpost_2 | Economy_Outpost_2 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_2.sbc` |
| Economy_Outpost_3 | Economy_Outpost_3 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_3.sbc` |
| Economy_Outpost_4 | Economy_Outpost_4 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_4.sbc` |
| Economy_Outpost_5 | Economy_Outpost_5 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_5.sbc` |
| Economy_Outpost_6 | Economy_Outpost_6 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_6.sbc` |
| Economy_Outpost_7 | Economy_Outpost_7 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_7.sbc` |
| Economy_Outpost_8 | Economy_Outpost_8 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_8.sbc` |
| Economy_Outpost_9 | Economy_Outpost_9 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_Outpost_9.sbc` |
| Economy_SpaceStation_1 | Economy_SpaceStation_1 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_SpaceStation_1.sbc` |
| Economy_SpaceStation_2 | Economy_SpaceStation_2 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_SpaceStation_2.sbc` |
| Economy_SpaceStation_3 | Economy_SpaceStation_3 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_SpaceStation_3.sbc` |
| Economy_SpaceStation_4 | Economy_SpaceStation_4 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy, Frostbite |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_SpaceStation_4.sbc` |
| Economy_SpaceStation_5 | Economy_SpaceStation_5 | Large Grid | Station/Static | Legacy Economy Stations | DecorativeBlocks, DecorativeBlocks2, Economy |  | `Data\Prefabs\LegacyContent\Economy\Stations\Economy_SpaceStation_5.sbc` |

### Unknown signals (41)

| Subtype | Display/Grid Names | Grid | Mobility | Source Kind | DLC/use | Referenced by | File |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Container_MK-1 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-1.sbc` |
| Container_MK-10 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace |  | `Data\Prefabs\UnknownSignals\Container_MK-10.sbc` |
| Container_MK-11 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, Signal |  | `Data\Prefabs\UnknownSignals\Container_MK-11.sbc` |
| Container_MK-12 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-12.sbc` |
| Container_MK-13 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace |  | `Data\Prefabs\UnknownSignals\Container_MK-13.sbc` |
| Container_MK-14 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-14.sbc` |
| Container_MK-15 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, Signal, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-15.sbc` |
| Container_MK-16 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks3, Fieldwork, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-16.sbc` |
| Container_MK-17 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, ScrapRace |  | `Data\Prefabs\UnknownSignals\Container_MK-17.sbc` |
| Container_MK-18 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks3, HeavyIndustry, Signal, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-18.sbc` |
| Container_MK-19 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\Container_MK-19.sbc` |
| Container_MK-2 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks3, ScrapRace, Signal |  | `Data\Prefabs\UnknownSignals\Container_MK-2.sbc` |
| Container_MK-20 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks3, HeavyIndustry, ScrapRace |  | `Data\Prefabs\UnknownSignals\Container_MK-20.sbc` |
| Container_MK-21 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-21.sbc` |
| Container_MK-22 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-22.sbc` |
| Container_MK-23 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, HeavyIndustry, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-23.sbc` |
| Container_MK-24 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-24.sbc` |
| Container_MK-25 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-25.sbc` |
| Container_MK-26 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-26.sbc` |
| Container_MK-3 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-3.sbc` |
| Container_MK-4 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-4.sbc` |
| Container_MK-5 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-5.sbc` |
| Container_MK-6 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, ScrapRace, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-6.sbc` |
| Container_MK-7 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\Container_MK-7.sbc` |
| Container_MK-8 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks2, Fieldwork, HeavyIndustry, ScrapRace |  | `Data\Prefabs\UnknownSignals\Container_MK-8.sbc` |
| Container_MK-9 | Unknown Signal | Small Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks3, HeavyIndustry |  | `Data\Prefabs\UnknownSignals\Container_MK-9.sbc` |
| LargeContainer_Mk-1 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, HeavyIndustry, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-1.sbc` |
| LargeContainer_Mk-10 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-10.sbc` |
| LargeContainer_Mk-11 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-11.sbc` |
| LargeContainer_Mk-12 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-12.sbc` |
| LargeContainer_Mk-13 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Fieldwork, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-13.sbc` |
| LargeContainer_Mk-14 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | DecorativeBlocks3, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-14.sbc` |
| LargeContainer_Mk-15 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, Fieldwork, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-15.sbc` |
| LargeContainer_Mk-2 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-2.sbc` |
| LargeContainer_Mk-3 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, Fieldwork, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-3.sbc` |
| LargeContainer_Mk-4 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-4.sbc` |
| LargeContainer_Mk-5 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, DecorativeBlocks3, HeavyIndustry, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-5.sbc` |
| LargeContainer_Mk-6 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-6.sbc` |
| LargeContainer_Mk-7 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, Signal, SparksOfTheFuture, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-7.sbc` |
| LargeContainer_Mk-8 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, Contact, DecorativeBlocks, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, HeavyIndustry, ScrapRace, Signal, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-8.sbc` |
| LargeContainer_Mk-9 | Strong Unknown Signal | Large Grid | Unknown Mobility | Unknown Signal | Automatons, DecorativeBlocks2, DecorativeBlocks3, Fieldwork, Frostbite, HeavyIndustry, ScrapRace, SparksOfTheFuture, Warfare1, Warfare2 |  | `Data\Prefabs\UnknownSignals\LargeContainer_Mk-9.sbc` |

## Voxel Map Storages

These are not grids, but they matter for asteroid bases, encounter asteroids, tunnels, and scenario voxel structures. Some spawn groups reference these by `StorageName`; those references are captured on prefab entries in the JSON under `referenced_by[].voxel_storage_refs`.

| Subtype | Storage File | Procedural Add Shape | Additions | Removals | Spawn Probability |
| --- | --- | --- | --- | --- | ---: |
| Arabian_Border_7 | `VoxelMaps\Arabian_Border_7.vx2` | True | True | True |  |
| Arabian_Border_Arabian | `VoxelMaps\Arabian_Border_Arabian.vx2` | True |  |  |  |
| Asteroid128-001 | `VoxelMaps\Asteroid128-001.vx2` | True |  | True |  |
| Asteroid128-002 | `VoxelMaps\Asteroid128-002.vx2` | True |  | True |  |
| Asteroid128-003 | `VoxelMaps\Asteroid128-003.vx2` | True |  | True |  |
| Asteroid128-004 | `VoxelMaps\Asteroid128-004.vx2` | True |  | True |  |
| Asteroid128-005 | `VoxelMaps\Asteroid128-005.vx2` | True |  | True |  |
| Asteroid128-006 | `VoxelMaps\Asteroid128-006.vx2` | True |  | True |  |
| Asteroid128-007 | `VoxelMaps\Asteroid128-007.vx2` | True |  | True |  |
| Asteroid256-001 | `VoxelMaps\Asteroid256-001.vx2` | True |  |  |  |
| Asteroid256-002 | `VoxelMaps\Asteroid256-002.vx2` | True |  |  |  |
| Asteroid256-003 | `VoxelMaps\Asteroid256-003.vx2` | True |  |  |  |
| Asteroid256-004 | `VoxelMaps\Asteroid256-004.vx2` | True |  |  |  |
| Asteroid256-005 | `VoxelMaps\Asteroid256-005.vx2` | True |  |  |  |
| Asteroid256-006 | `VoxelMaps\Asteroid256-006.vx2` | True |  |  |  |
| Asteroid256-007 | `VoxelMaps\Asteroid256-007.vx2` | True |  |  |  |
| Asteroid256-008 | `VoxelMaps\Asteroid256-008.vx2` | True |  |  |  |
| Asteroid256-009 | `VoxelMaps\Asteroid256-009.vx2` | True |  |  |  |
| Asteroid256-010 | `VoxelMaps\Asteroid256-010.vx2` | True |  |  |  |
| Asteroid256-011 | `VoxelMaps\Asteroid256-011.vx2` | True |  |  |  |
| Asteroid256-012 | `VoxelMaps\Asteroid256-012.vx2` | True |  |  |  |
| Asteroid256-013 | `VoxelMaps\Asteroid256-013.vx2` | True |  |  |  |
| Asteroid256-014 | `VoxelMaps\Asteroid256-014.vx2` | True |  |  |  |
| Asteroid256-015 | `VoxelMaps\Asteroid256-015.vx2` | True |  |  |  |
| Asteroid256-016 | `VoxelMaps\Asteroid256-016.vx2` | True |  |  |  |
| Asteroid256-017 | `VoxelMaps\Asteroid256-017.vx2` | True |  |  |  |
| Asteroid256-018 | `VoxelMaps\Asteroid256-018.vx2` | True |  |  |  |
| Asteroid256-019 | `VoxelMaps\Asteroid256-019.vx2` | True |  |  |  |
| Asteroid256-020 | `VoxelMaps\Asteroid256-020.vx2` | True |  |  |  |
| Asteroid512-001 | `VoxelMaps\Asteroid512-001.vx2` | True |  |  |  |
| Asteroid512-002 | `VoxelMaps\Asteroid512-002.vx2` | True |  |  |  |
| Asteroid512-003 | `VoxelMaps\Asteroid512-003.vx2` | True |  |  |  |
| Asteroid512-004 | `VoxelMaps\Asteroid512-004.vx2` | True |  |  |  |
| Asteroid512-006 | `VoxelMaps\Asteroid512-006.vx2` | True |  |  |  |
| Asteroid512-007 | `VoxelMaps\Asteroid512-007.vx2` | True |  |  |  |
| Asteroid512-008 | `VoxelMaps\Asteroid512-008.vx2` | True |  |  |  |
| Asteroid512-009 | `VoxelMaps\Asteroid512-009.vx2` | True |  |  |  |
| Asteroid512-011 | `VoxelMaps\Asteroid512-011.vx2` | True |  |  |  |
| Asteroid512-012 | `VoxelMaps\Asteroid512-012.vx2` | True |  |  |  |
| Asteroid512-013 | `VoxelMaps\Asteroid512-013.vx2` | True |  |  |  |
| Asteroid512-014 | `VoxelMaps\Asteroid512-014.vx2` | True |  |  |  |
| Asteroid64-001 | `VoxelMaps\Asteroid64-001.vx2` | True | True | True |  |
| Asteroid64-002 | `VoxelMaps\Asteroid64-002.vx2` | True | True | True |  |
| Asteroid64-003 | `VoxelMaps\Asteroid64-003.vx2` | True | True | True |  |
| Asteroid64-004 | `VoxelMaps\Asteroid64-004.vx2` | True | True | True |  |
| Asteroid64-005 | `VoxelMaps\Asteroid64-005.vx2` | True | True | True |  |
| Asteroid64-006 | `VoxelMaps\Asteroid64-006.vx2` | True | True | True |  |
| AsteroidBase2 | `VoxelMaps\AsteroidBase2.vx2` | True |  |  | 0.5 |
| AsteroidDebris | `VoxelMaps\AsteroidDebris.vx2` | True |  |  | 0.5 |
| AsteroidSpaceStation | `VoxelMaps\AsteroidSpaceStation.vx2` |  |  |  |  |
| Asteroid_011 | `VoxelMaps\Asteroid_011.vx2` | True |  |  | 0.5 |
| Barths_moon_base | `VoxelMaps\Barths_moon_base.vx2` | True |  |  | 0.5 |
| Bioresearch | `VoxelMaps\Bioresearch.vx2` | True |  |  | 0.5 |
| ChineseRefinery_Second_128x128x128 | `VoxelMaps\ChineseRefinery_Second_128x128x128.vx2` | True |  |  |  |
| Chinese_Corridor_Tunnel_256x256x256 | `VoxelMaps\Chinese_Corridor_Tunnel_256x256x256.vx2` | True |  |  |  |
| Chinese_Mines_FrontRightAsteroid | `VoxelMaps\Chinese_Mines_FrontRightAsteroid.vx2` | True |  |  | 0.5 |
| Chinese_Mines_Side | `VoxelMaps\Chinese_Mines_Side.vx2` | True |  |  | 0.5 |
| DeformedSphere1_64x64x64 | `VoxelMaps\DeformedSphere1_64x64x64.vx2` | True | True | True |  |
| DeformedSphere2_64x64x64 | `VoxelMaps\DeformedSphere2_64x64x64.vx2` | True | True | True |  |
| DeformedSphereWithCorridor_128x64x64 | `VoxelMaps\DeformedSphereWithCorridor_128x64x64.vx2` | True |  | True |  |
| DeformedSphereWithHoles_64x128x64 | `VoxelMaps\DeformedSphereWithHoles_64x128x64.vx2` | True |  | True |  |
| EacPrisonAsteroid | `VoxelMaps\EacPrisonAsteroid.vx2` | True |  |  |  |
| EngineersOutpost | `VoxelMaps\EngineersOutpost.vx2` | True |  |  | 0.5 |
| Fortress_Sanc_1 | `VoxelMaps\Fortress_Sanc_1.vx2` | True |  |  |  |
| JunkYardToxic_128x128x128 | `VoxelMaps\JunkYardToxic_128x128x128.vx2` | True |  |  | 0.6 |
| Junkyard_RaceAsteroid_256x256x256 | `VoxelMaps\Junkyard_RaceAsteroid_256x256x256.vx2` | True |  |  |  |
| Laika5_128_128_128 | `VoxelMaps\Laika5_128_128_128.vx2` |  |  |  |  |
| Leedah_Asteroid | `VoxelMaps\Leedah_Asteroid.vx2` | True |  |  | 0.5 |
| Mamedh | `VoxelMaps\SuperRock_002.vx2` | True |  |  | 0.5 |
| Mission01_asteroid_mine | `VoxelMaps\Mission01_asteroid_mine.vx2` | True |  |  | 0.5 |
| Nearby_Station_7 | `VoxelMaps\Nearby_Station_7.vx2` | True |  |  |  |
| PirateBaseStaticAsteroid_A_1000m | `VoxelMaps\PirateBaseStaticAsteroid_A_1000m.vx2` | True | True |  |  |
| PirateBaseStaticAsteroid_A_5000m_1 | `VoxelMaps\PirateBaseStaticAsteroid_A_5000m_1.vx2` | True |  |  | 0.5 |
| PirateBaseStaticAsteroid_A_5000m_2 | `VoxelMaps\PirateBaseStaticAsteroid_A_5000m_2.vx2` | True |  |  | 0.5 |
| Quantorea | `VoxelMaps\Quantorea.vx2` | True |  |  | 0.5 |
| RedShipCrashedAsteroid | `VoxelMaps\RedShipCrashedAsteroid.vx2` | True |  |  | 0.4 |
| RiftStationSmaller | `VoxelMaps\RiftStationSmaller.vx2` |  |  |  |  |
| Russian_Transmitter_2 | `VoxelMaps\Russian_Transmitter_2.vx2` | True |  |  |  |
| ScratchedBoulder_128x128x128 | `VoxelMaps\ScratchedBoulder_128x128x128.vx2` |  |  | True |  |
| Small_Pirate_Base_3_1 | `VoxelMaps\Small_Pirate_Base_3_1.vx2` | True |  |  |  |
| Small_Pirate_Base_3_2 | `VoxelMaps\Small_Pirate_Base_3_2.vx2` | True |  |  |  |
| Small_Pirate_Base_Asteroid | `VoxelMaps\Small_Pirate_Base_Asteroid.vx2` | True |  |  |  |
| SnowCoverageIronCore | `VoxelMaps\MediumStoneIronCoreSnowCoverage.vx2` |  |  |  |  |
| StoneCoverageIronCore | `VoxelMaps\MediumStoneIronCoreStoneCoverage.vx2` |  |  |  |  |
| Stones | `VoxelMaps\MediumStoneIronCoreStoneCoverage.vx2` |  |  |  |  |
| SuperRock_001 | `VoxelMaps\SuperRock_001.vx2` | True |  |  | 0.5 |
| TorusWithManyTunnels_256x128x256 | `VoxelMaps\TorusWithManyTunnels_256x128x256.vx2` | True |  |  | 0.3 |
| TorusWithSmallTunnel_256x128x256 | `VoxelMaps\TorusWithSmallTunnel_256x128x256.vx2` | True |  |  | 0.3 |
| VangelisBase | `VoxelMaps\VangelisBase.vx2` | True |  |  | 0.3 |
| VerticalIslandStorySector_128x256x128 | `VoxelMaps\VerticalIslandStorySector_128x256x128.vx2` | True |  |  | 0.4 |
| VerticalIsland_128x128x128 | `VoxelMaps\VerticalIsland_128x128x128.vx2` |  |  | True |  |
| VerticalIsland_128x256x128 | `VoxelMaps\VerticalIsland_128x256x128.vx2` | True |  |  | 0.4 |
| VoxelstoneStoneFe | `VoxelMaps\VoxelstoneStoneFe.vx2` | True |  | True |  |
| VoxelstoneStoneSnowFe | `VoxelMaps\VoxelstoneStoneSnowFe.vx2` |  |  |  |  |
| alienrockymountain_largestone | `VoxelMaps\alienrockymountain_largestone.vx2` |  |  |  |  |
| alienrockymountain_mediumstone | `VoxelMaps\alienrockymountain_mediumstone.vx2` |  |  |  |  |
| alienrockymountain_smallstone | `VoxelMaps\alienrockymountain_smallstone.vx2` |  |  |  |  |
| alienrockymountain_snowtop_largestone | `VoxelMaps\alienrockymountain_snowtop_largestone.vx2` |  |  |  |  |
| alienrockymountain_snowtop_mediumstone | `VoxelMaps\alienrockymountain_snowtop_mediumstone.vx2` |  |  |  |  |
| alienrockymountain_snowtop_smallstone | `VoxelMaps\alienrockymountain_snowtop_smallstone.vx2` |  |  |  |  |
| alienrockyterrain_largestone | `VoxelMaps\alienrockyterrain_largestone.vx2` |  |  |  |  |
| alienrockyterrain_mediumstone | `VoxelMaps\alienrockyterrain_mediumstone.vx2` |  |  |  |  |
| alienrockyterrain_smallstone | `VoxelMaps\alienrockyterrain_smallstone.vx2` |  |  |  |  |
| alienrockyterrain_snowtop_largestone | `VoxelMaps\alienrockyterrain_snowtop_largestone.vx2` |  |  |  |  |
| alienrockyterrain_snowtop_mediumstone | `VoxelMaps\alienrockyterrain_snowtop_mediumstone.vx2` |  |  |  |  |
| alienrockyterrain_snowtop_smallstone | `VoxelMaps\alienrockyterrain_snowtop_smallstone.vx2` |  |  |  |  |
| barths_moon_camp | `VoxelMaps\barths_moon_camp.vx2` | True |  |  | 0.5 |
| desertrocks_largestone | `VoxelMaps\desertrocks_largestone.vx2` |  |  |  |  |
| desertrocks_mediumstone | `VoxelMaps\desertrocks_mediumstone.vx2` |  |  |  |  |
| desertrocks_smallstone | `VoxelMaps\desertrocks_smallstone.vx2` |  |  |  |  |
| hopebase512 | `VoxelMaps\hopebase512.vx2` | True |  |  |  |
| hopefood128 | `VoxelMaps\hopefood128.vx2` | True |  | True |  |
| ice_02_largestone | `VoxelMaps\ice_02_largestone.vx2` |  |  |  |  |
| ice_02_mediumstone | `VoxelMaps\ice_02_mediumstone.vx2` |  |  |  |  |
| ice_02_smallstone | `VoxelMaps\ice_02_smallstone.vx2` |  |  |  |  |
| ice_largestone | `VoxelMaps\ice_largestone.vx2` |  |  |  |  |
| ice_mediumstone | `VoxelMaps\ice_mediumstone.vx2` |  |  |  |  |
| ice_smallstone | `VoxelMaps\ice_smallstone.vx2` |  |  |  |  |
| many2_small_asteroids | `VoxelMaps\many2_small_asteroids.vx2` | True | True | True | 2 |
| many_medium_asteroids | `VoxelMaps\many_medium_asteroids.vx2` | True | True | True | 2 |
| many_small_asteroids | `VoxelMaps\many_small_asteroids.vx2` | True | True | True | 2 |
| marsrock_mediumstone | `VoxelMaps\marsrock_mediumstone.vx2` |  |  |  |  |
| marsrock_smallstone | `VoxelMaps\marsrock_smallstone.vx2` |  |  |  |  |
| marsrocks_largestone | `VoxelMaps\marsrocks_largestone.vx2` |  |  |  |  |
| moonrocks_largestone | `VoxelMaps\moonrocks_largestone.vx2` |  |  |  |  |
| moonrocks_mediumstone | `VoxelMaps\moonrocks_mediumstone.vx2` |  |  |  |  |
| moonrocks_smallstone | `VoxelMaps\moonrocks_smallstone.vx2` |  |  |  |  |
| moonsoil_largestone | `VoxelMaps\moonsoil_largestone.vx2` |  |  |  |  |
| moonsoil_mediumstone | `VoxelMaps\moonsoil_mediumstone.vx2` |  |  |  |  |
| moonsoil_smallstone | `VoxelMaps\moonsoil_smallstone.vx2` |  |  |  |  |
| reef_ast | `VoxelMaps\reef_ast.vx2` |  |  | True |  |
| rift_base_smaller | `VoxelMaps\rift_base_smaller.vx2` | True |  |  |  |
| small2_asteroids | `VoxelMaps\small2_asteroids.vx2` |  | True | True |  |
| small3_asteroids | `VoxelMaps\small3_asteroids.vx2` |  | True | True |  |
| small_horse_overhang | `VoxelMaps\small_horse_overhang.vx2` |  |  |  |  |
| small_largestone | `VoxelMaps\small_largestone.vx2` |  | True | True |  |
| small_mediumstone | `VoxelMaps\small_mediumstone.vx2` |  |  |  |  |
| small_overhang | `VoxelMaps\small_overhang.vx2` |  |  |  |  |
| small_overhang_flat | `VoxelMaps\small_overhang_flat.vx2` |  |  |  |  |
| small_smallstone | `VoxelMaps\small_smallstone.vx2` |  |  |  |  |
| stone_largestone | `VoxelMaps\stone_largestone.vx2` |  |  |  |  |
| stone_mediumstone | `VoxelMaps\stone_mediumstone.vx2` |  | True |  |  |
| stone_smallstone | `VoxelMaps\stone_smallstone.vx2` |  |  |  |  |
| stone_snowtop_largestone | `VoxelMaps\stone_snowtop_largestone.vx2` |  |  |  |  |
| stone_snowtop_mediumstone | `VoxelMaps\stone_snowtop_mediumstone.vx2` |  |  |  |  |
| stone_snowtop_smallstone | `VoxelMaps\stone_snowtop_smallstone.vx2` |  |  |  |  |
| testplacement | `VoxelMaps\testplacement.vx2` |  |  |  |  |
