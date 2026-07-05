# Working Knowledge Configuration

The mod creates a world-storage config file on first load:

```xml
<WkConfig>
  <DifficultyPreset>medium</DifficultyPreset>
  <SalvageScrapEnabled>true</SalvageScrapEnabled>
  <DataFragmentsEnabled>true</DataFragmentsEnabled>
  <ResearchScale>1</ResearchScale>
  <ResearchGrindingGainScale>1</ResearchGrindingGainScale>
  <DataFragmentRewardScale>1</DataFragmentRewardScale>
  <DataFragmentLootScale>1</DataFragmentLootScale>
  <ResearchEfficiencyStart>1</ResearchEfficiencyStart>
  <ResearchEfficiencyEnd>0.5</ResearchEfficiencyEnd>
  <SalvageScale>1</SalvageScale>
  <NotificationDelaySeconds>2</NotificationDelaySeconds>
  <ProgressChatEnabled>true</ProgressChatEnabled>
  <ProgressToastEnabled>true</ProgressToastEnabled>
  <ResearchChatSuppressionPercent>0</ResearchChatSuppressionPercent>
  <ProficiencyChatSuppressionPercent>0</ProficiencyChatSuppressionPercent>
  <ResearchToastSuppressionPercent>0</ResearchToastSuppressionPercent>
  <ProficiencyToastSuppressionPercent>0</ProficiencyToastSuppressionPercent>
  <CompletionSoundEnabled>true</CompletionSoundEnabled>
  <FundamentalsResearchUnlocked>true</FundamentalsResearchUnlocked>
  <FundamentalsProficiencyProgress>0.8</FundamentalsProficiencyProgress>
  <ProficiencyEnabled>true</ProficiencyEnabled>
  <ProficiencyGainScale>1</ProficiencyGainScale>
  <ProficiencyGrindingGainScale>1</ProficiencyGrindingGainScale>
  <ProficiencyWeldingGainScale>1</ProficiencyWeldingGainScale>
  <ProficiencyFirstThreshold>0.7</ProficiencyFirstThreshold>
  <ProficiencySecondThreshold>1</ProficiencySecondThreshold>
  <ProficiencyFirstSegmentRate>0.56</ProficiencyFirstSegmentRate>
  <ProficiencySecondSegmentRate>0.018</ProficiencySecondSegmentRate>
  <ProficiencyFinalSegmentRate>0.018</ProficiencyFinalSegmentRate>
  <ProficiencyWeldingLossEnabled>true</ProficiencyWeldingLossEnabled>
  <ProficiencyGrindingLossEnabled>true</ProficiencyGrindingLossEnabled>
  <ProficiencyBuildCapEnabled>true</ProficiencyBuildCapEnabled>
  <WeldBotchBaseChance>0.5</WeldBotchBaseChance>
  <WeldBotchMaxChance>0.95</WeldBotchMaxChance>
  <WeldBotchChanceScale>1</WeldBotchChanceScale>
  <WeldBotchPostFunctionalPressure>4</WeldBotchPostFunctionalPressure>
  <WeldBotchSoftCapPressure>3</WeldBotchSoftCapPressure>
  <WeldBotchPressureScale>1</WeldBotchPressureScale>
  <WeldBotchRawLossRatio>1</WeldBotchRawLossRatio>
  <WeldBotchForgivenessScale>1</WeldBotchForgivenessScale>
  <WeldBotchWarningCooldownSeconds>0</WeldBotchWarningCooldownSeconds>
  <WeldBotchSoundEnabled>true</WeldBotchSoundEnabled>
  <WeldBotchSoundSubtype>ArcPoofExplosionCat1</WeldBotchSoundSubtype>
  <WeldBotchSoundRange>75</WeldBotchSoundRange>
</WkConfig>
```

Player feedback preferences are saved separately in world storage as `WkKnPlayerConfig.xml`.

`medium` is the authored baseline and keeps difficulty-facing modifiers at `1.0`. `/wk difficulty` displays the active preset and difficulty modifiers. Admins can use `/wk config world reset` to return all world settings to the built-in `medium` values or `/wk difficulty <preset>` to apply a concrete difficulty modifier bundle into `WkKnConfig.xml`. Manual world config edits mark `DifficultyPreset` as `custom`.

The XML element names above are storage names. In chat commands, overlapping world feedback settings use `default...` names so they are distinct from player settings. For example, the world XML field `ProgressChatEnabled` is edited with `/wk config defaultProgressChatEnabled <true|false>`, while `/wk config progressChatEnabled <true|false>` changes only the local player's preference.

## Difficulty

```text
/wk difficulty
/wk difficulty help
/wk difficulty novice
/wk difficulty easy
/wk difficulty medium
/wk difficulty hard
/wk difficulty extreme
/wk difficulty custom <setting> help
/wk difficulty custom researchScale 0.75
/wk difficulty custom botchDamageScale help
/wk difficulty custom weldBotchChanceScale 1.25
/wk difficulty custom proficiencyGainScale 1.25
```

Only admins can apply a difficulty preset or custom difficulty modifier. Presets only change difficulty-facing modifiers; detailed botch curve values, feedback, defaults, and enabled/disabled feature flags keep their current values.

- `novice` - `2.0` research/Proficiency/data rewards, `2.0` salvage and forgiveness, `0.5` botch chance/damage/pressure.
- `easy` - `1.5` research/Proficiency/data rewards, `1.5` salvage and forgiveness, `0.75` botch chance/damage/pressure.
- `medium` - neutral `1.0` modifiers.
- `hard` - `0.5` research/Proficiency/data rewards, `0.75` salvage and forgiveness, `1.5` botch chance/damage/pressure.
- `extreme` - `0.25` research/Proficiency/data rewards, `0.5` salvage and forgiveness, `2.0` botch chance/damage/pressure.

Difficulty custom accepts these modifier settings:

- `researchScale`
- `dataFragmentRewardScale`
- `dataFragmentLootScale`
- `proficiencyGainScale`
- `salvageScale`
- `weldBotchChanceScale`
- `weldBotchRawLossRatio`
- `weldBotchPressureScale`
- `weldBotchForgivenessScale`

## Config Commands

Anyone can view and update their own player feedback settings:

```text
/wk config
/wk config help
/wk config <setting> help
/wk config <setting> <value>
/wk config reset
```

`/wk config help` shows player settings such as chat, toast, and sound output. Player settings can suppress or mute feedback allowed by world settings.

Admins can view category help and update world config:

```text
/wk admin
/wk admin unlockall
/wk research show <player>
/wk research reset server
/wk research reset <player>
/wk research unlock <player> <schematic>
/wk research forget <player> <schematic>
/wk research set <player> <schematic> <percent>
/wk proficiency show <player>
/wk proficiency reset server
/wk proficiency reset <player>
/wk proficiency master <player> <schematic>
/wk proficiency forget <player> <schematic>
/wk proficiency set <player> <schematic> <percent>
/wk config <setting> help
/wk config <setting> <value>
/wk config world
/wk config world help
/wk config world reset
/wk difficulty help
/wk difficulty <novice|easy|medium|hard|extreme>
/wk difficulty custom <setting> help
/wk difficulty custom <setting> <value>
```

`/wk admin unlockall` completes every schematic family for the admin who runs the command and does not change Proficiency. It is intended for creative/admin testing of welding, botches, and recovery behavior against fully unlocked research.

Research and Proficiency target commands accept `me`, `all`, `online`, a quoted online player name, an identity id, or a Steam id. Schematic names can be display names or research ids, for example `Ion Thruster Schematics` or `propulsion.ion_thruster`.

Admin examples:

```text
/wk research unlock me Ion Thruster Schematics
/wk research set "Bob Smith" Basic Production Schematics 75
/wk proficiency master me Basic Production Schematics
/wk proficiency set online production.basic 80%
/wk difficulty hard
/wk difficulty custom botchDamageScale help
/wk difficulty custom botchDamageScale 1.25
/wk config researchScale 0.5
/wk config proficiencyGainScale 1.25
/wk config weldBotchBaseChance 0.35
/wk config weldBotchRawLossRatio 0.75
/wk config defaultProgressToastEnabled true
/wk config defaultWeldBotchSoundEnabled true
/wk config weldBotchSoundSubtype ArcPoofExplosionCat1
/wk config proficiencySecondSegmentRate help
```

Player settings:

- `progressChatEnabled`, `progressToastEnabled`
- `researchChatSuppressionPercent`, `proficiencyChatSuppressionPercent`
- `researchToastSuppressionPercent`, `proficiencyToastSuppressionPercent`
- `completionSoundEnabled`
- `weldBotchSoundEnabled`
- `weldBotchWarningCooldownSeconds`

World feedback command names:

- `notificationDelaySeconds`
- `defaultProgressChatEnabled`, `defaultProgressToastEnabled`
- `defaultResearchChatSuppressionPercent`, `defaultProficiencyChatSuppressionPercent`
- `defaultResearchToastSuppressionPercent`, `defaultProficiencyToastSuppressionPercent`
- `defaultCompletionSoundEnabled`
- `defaultWeldBotchSoundEnabled`, `defaultWeldBotchWarningCooldownSeconds`
- `weldBotchSoundSubtype`, `weldBotchSoundRange`

Research and Proficiency completion sound subtypes are owned by the mod and are not configurable. Players can only enable or mute their own completion sound feedback when world settings allow it.

## Main Settings

- `DataFragmentsEnabled` - enables consumable research data items. Fragments are random lossy partial rewards; exact Data Schematics are durable shareable unlock items. Disabled items are refunded and grant no research.

Schematic-specific work rewards are source-code balance data in `Application/Balance/SchematicWorkRewardTable.cs`. Each row stores large-grid base work reward, small-grid base work reward, and reference build seconds. The fallback defaults are `0.21`, `0.0857`, and `8.0`. Vanilla block build time then applies a clamped square-root factor before research or Proficiency curve and config multipliers. Generated audit reports, when refreshed, live under `docs/generated/`.

- `ResearchScale` - global multiplier for active research gain.
- `ResearchGrindingGainScale` - multiplier for schematic research from grinding.
- `DataFragmentRewardScale` - multiplier for partial research progress from consumable data fragments.
- `DataFragmentLootScale` - multiplier for Working Knowledge data-fragment frequencies in patched container definitions. Existing spawned containers are not retroactive.
- `ResearchEfficiencyStart` / `ResearchEfficiencyEnd` - shape the research curve from early discovery to final confirmation.
- `ProficiencyGainScale` - global multiplier for hands-on Proficiency gain.
- `ProficiencyGrindingGainScale` / `ProficiencyWeldingGainScale` - source-specific Proficiency gain multipliers.
- `ProficiencyFirstThreshold` / `ProficiencySecondThreshold` - breakpoints for the fast, slow, and optional final Proficiency curve segments. The default `1.0` second threshold makes the baseline a two-stage curve.
- `ProficiencyFirstSegmentRate` / `ProficiencySecondSegmentRate` / `ProficiencyFinalSegmentRate` - base gain rate per table work reward unit in each Proficiency segment before the segment curve is applied.
- `SalvageScrapEnabled` - enables Proficiency-biased conversion of low-skill grind recovery into scrap.
- `SalvageScale` - multiplier for intact component recovery while grinding before the 100% cap.
- `ProficiencyGrindingLossEnabled` - enables the Proficiency check that can turn grinding recovery into scrap.
- `WeldBotchBaseChance` - botch chance scale near the functional threshold. The medium value `0.5` maps to inverse Proficiency, so 40% Proficiency reaches about 60% botch chance at the functional line.
- `ProficiencyBuildCapEnabled` - enables guaranteed failure at the current post-functional Proficiency cap.
- `WeldBotchMaxChance` - maximum normal botch chance before the Proficiency cap forces guaranteed failure.
- `WeldBotchChanceScale` - difficulty-facing botch chance multiplier before the max chance clamp.
- `WeldBotchPostFunctionalPressure` - shapes how sharply botch chance climbs through the post-functional botch zone.
- `WeldBotchSoftCapPressure` - shapes the Proficiency cap pressure inside the post-functional botch zone.
- `WeldBotchPressureScale` - difficulty-facing multiplier for the post-functional botch pressure curves.
- `WeldBotchRawLossRatio` - raw integrity loss as a multiple of the triggering positive integrity delta.
- `WeldBotchForgivenessScale` - returned component forgiveness multiplier after botch damage; higher is gentler.
- `WeldBotchSoundEnabled`, `WeldBotchSoundSubtype`, `WeldBotchSoundRange` - world botch sound feedback and range.

Use `/wk config <setting> help` in game for setting details. `/wk admin` lists category help pages such as research, Proficiency, botches, salvage, feedback, defaults, and difficulty. In-game world config output is grouped under chat senders such as `Config - Research` and `Config - Botches` rather than using heading text inside the message body.
