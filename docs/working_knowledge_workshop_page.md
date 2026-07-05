# Working Knowledge Workshop Page

## Title

Working Knowledge

## Short Description

Salvage unknown blocks, decode research data, and unlock schematic families through a custom progression system mirrored into vanilla research.

## Description

Working Knowledge turns Space Engineers progression into a salvage-first research loop.

Instead of unlocking most blocks by building one item in a flat chain, you learn technology by taking unfamiliar machines apart, recovering research data, and practicing the work until your engineer actually knows what they are doing.

This is the first public feedback release. It is playable as a standalone progression mod, but balance and multiplayer edge cases are still being tested.

## Core Loop

- Find unfamiliar blocks in wrecks, signals, stations, POIs, or captured grids.
- Grind unknown technology to recover schematic progress.
- Decode Data Fragments for random partial research.
- Use exact Data Schematics as POI, mission, scenario, or admin rewards.
- Complete schematic families to unlock their blocks for normal survival use.
- Improve Proficiency by building, repairing, grinding, and salvaging the same families.

Research answers: **Do I know the schematic?**

Proficiency answers: **How well can I build, repair, grind, and salvage it?**

## Features

- Most vanilla public cube blocks are grouped into schematic families.
- Unknown blocks are locked through vanilla research until their schematic family is completed.
- Grinding unknown blocks grants partial research based on how much of the block was dismantled.
- Data Fragments can appear as rare loot in unknown signals and selected vanilla POI containers.
- Exact Data Schematics are durable shareable items for authored rewards and admin use.
- Low Proficiency converts more recovered component value into scrap.
- Low Proficiency can make welding less reliable through construction botches.
- Botches can knock build progress back, destroy some components, and recover forgiven components when material loss would be too punishing.
- A Research Pedestal shows filtered player/faction schematic progress and can manually sync partial or completed progress.
- Selectable LCD apps show research, Proficiency, player/faction identity, and display calibration.
- Difficulty presets and admin tuning are available through `/wk` commands.

## LCD Apps

Working Knowledge adds selectable LCD scripts for normal LCD panels, transparent LCDs, cockpit displays, multi-screen blocks, and the custom Research Sci-Fi Terminal.

- `Working Knowledge Research`
- `Working Knowledge Proficiency`
- `Working Knowledge Identity`
- `Working Knowledge Calibrator`

## Commands

Start with:

- `/wk`
- `/wk help`

The in-game help lists research and Proficiency summaries, player feedback settings, the current difficulty display, and admin categories when run by an admin. With chat entry open, press Page Up or Page Down to scroll long help messages.

## Recommended Setup

- Best tested in a new survival world.
- Experimental Mode is not required.
- The mod enables vanilla progression at runtime so it can own the schematic lock path.
- You can create the world with vanilla progression disabled and let Working Knowledge handle progression.
- Creative worlds and admin creative-mode testing can bypass schematic locks.
- No hard companion mod dependencies.

## Compatibility Notes

- Other progression or research-overhaul mods are likely to conflict.
- Mods that bypass block placement restrictions or alter grinding/welding behavior should be tested carefully.
- Player-built locked blocks are removed server-side unless the player knows the schematic.
- Creative mode bypasses locked placement checks.
- Promoted Space Master/admin full-built placements are allowed.
- Broad dedicated-server validation is still pending for this feedback release.

## Project And Feedback

Working Knowledge is a standalone mod and the first playable layer of the larger **Space Engineers: Beneath the Crust** campaign project.

Bug reports and balance feedback are welcome:

https://github.com/abflett/SE-BeneathTheCrust/issues
