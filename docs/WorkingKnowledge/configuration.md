# Working Knowledge Configuration

### Graphical numeric controls

Each setting places its title and description above its control. Numeric sliders have a value box to their right. The window footer has **Apply Changes** and **Exit** buttons. Type in the displayed units: `5` or `5%` means 5% in a percentage box, `25` or `25x` means a 25x multiplier. The unit suffix is optional. Chat commands continue to use canonical ratios, so their equivalent of 5% is `0.05`.

Valid typed values update the local draft and slider after a 350 ms pause. Edits stay local to the panel until **Apply Changes** submits them through the usual authoritative command path. Apply reads the latest text even if the typing delay has not elapsed. **Exit** and Escape discard unapplied edits. Presets and resets are also queued for Apply Changes; selecting either replaces earlier unapplied edits, and other fields show their current values until the preset/reset is applied. Normal world saving persists applied settings.

While the window is open, the local game HUD and Working Knowledge progress overlay are hidden. Closing the window restores the previous game HUD mode.

Sliders cover practical tuning ranges; the value box accepts the full valid setting range, shown in its tooltip. Opening or refreshing the panel does not clamp a saved value to the slider range. Typed values are not snapped to slider steps; command precision remains six decimal places. Invalid input leaves the setting unchanged.

| Controls | Slider range | Value-box range |
| --- | --- | --- |
| Gain, reward, loot, salvage, efficiency, botch multipliers | 0–10x, logarithmic with zero | Existing limit: 10x or 100x, depending on setting |
| Probabilities, thresholds, starting Proficiency, scrap yield | 0–100% | 0–100% |
| First Proficiency segment rate | 0–100% | 0–1000% |
| Second and final Proficiency segment rates | 0–10% | 0–1000% |
| Botch pressure shape values | 0–10 | 0–100 |
| HUD rows | 1–10 | 1–10, whole numbers |
| HUD X/Y offsets | −1 to 1 | −2 to 2 |
| HUD fade | 0–30 seconds | 0–60 seconds |
| Notification delay | 0.1–10 seconds | 0.1–30 seconds |
| World botch warning cooldown | 0–10 seconds | 0–30 seconds |
| Botch sound range | 0–100 metres | 0–1000 metres |

The personal botch warning cooldown retains its text box so it can also accept `default`.

Click a value box to type directly; chat does not need to be open. Keyboard focus is released when leaving the field or closing the window. Research progress uses pale cyan and Proficiency uses muted blue; the translucent track follows the game's HUD background-opacity setting.

This guide explains the in-game Working Knowledge commands, player feedback settings, admin tuning controls, and persisted configuration fields used by the mod.

Start in-game with:

```text
/wk
/wk help
```

The `/wk` help screen is the main command index. It shows the player commands, adds the admin entry point when the sender is a Space Engineers admin, and includes this reminder: with chat entry open, press Page Up or Page Down to scroll long chat output.

## Quick Start For Players

Most players only need these commands:

```text
/wk
/wk research
/wk res
/wk proficiency
/wk prof
/wk difficulty
/wk config
/wk config help
```

- `/wk research` and `/wk res` show your schematic research progress.
- `/wk proficiency` and `/wk prof` show your hands-on Proficiency.
- `/wk difficulty` shows the current world difficulty preset and modifier values.
- `/wk config` shows your personal feedback settings.
- `/wk config help` shows the player feedback settings you are allowed to change.

Useful player feedback examples:

```text
/wk config progressChatEnabled false
/wk config progressToastEnabled true
/wk config progressHudPosition topRight
/wk config progressHudRows 5
/wk config progressHudFadeSeconds 0
/wk config researchChatSuppressionPercent 0.05
/wk config proficiencyToastSuppressionPercent 0.1
/wk config completionSoundEnabled false
/wk config weldBotchSoundEnabled false
/wk config weldBotchWarningCooldownSeconds default
/wk config reset
```

Player settings can only mute feedback or raise thresholds for that player. They cannot override world settings that disable feedback globally.

## Who Can Change What

Everyone can:

- View their own research and Proficiency summaries.
- View the current difficulty.
- View and change their own feedback settings.

Admins can also:

- View and change world config.
- Apply difficulty presets and custom difficulty modifiers.
- Reset world config to mod defaults.
- Show, reset, unlock, forget, or set research and Proficiency for player targets.
- Run `/wk admin unlockall` for local admin testing.

Commands that target other players, reset server progress, or change world values require a promoted Space Engineers admin.

## Feedback Players Will See

Working Knowledge feedback is split into chat messages, HUD progress overlay rows, HUD notifications, completion sounds, and botch sounds.

Progress feedback is delayed and combined by `notificationDelaySeconds`. Research and Proficiency gained from the same grinding action can be displayed together. Chat progress uses a schematic header and then lines like:

```text
Research +2.5% (42.5%)
Proficiency +1.1% (73.2%)
```

Popup HUD progress toasts use the schematic display name and are disabled by default for players:

```text
Ion Thruster Schematics Research +2.5% (42.5%)
Ion Thruster Schematics Proficiency +1.1% (73.2%)
```

The HUD progress overlay uses Rich HUD Master when it is loaded. By default it shows up to five recent schematic rows near the top right of the screen, with a green research bar and a blue Proficiency bar. The rows update as progress changes and fade after a short idle period. Use `/wk settings` for the graphical controls or `/wk config` for the equivalent chat commands.

Completion feedback uses chat/toast labels:

```text
Research Complete
Proficiency Mastered
```

Research and Proficiency completion sound subtypes are owned by the mod. They are not world-configurable. Players can enable or mute their own completion sounds only when world settings allow them.

If a player tries to place a locked block, the HUD warning is:

```text
Schematic required: <schematic display name>
```

Welding botches use chat and optional HUD/sound feedback. Chat is headed with the block name, such as `<block> Build Failure`, and can include:

```text
Work lost: -<percent>
Lost: <components>
Recovered: <components>
```

The matching HUD toast is:

```text
<block> Build Failed -<percent>
```

Research data item feedback can include:

```text
Data fragments are disabled by world settings.
Research data items are disabled by world settings.
No compatible schematic data found.
Duplicate schematic data recovered.
Unknown schematic data.
<schematic display name> already known.
```

## Player Feedback Settings

Use `/wk settings` to open the graphical Rich HUD menu. The Player Settings page is available to every player; authenticated administrators also receive editable Server Settings controls. Both interfaces use the same authoritative values and validation as the commands below.

Use:

```text
/wk config
/wk config help
/wk config <setting> help
/wk config <setting> <value>
/wk config reset
```

Boolean values accept `true`, `false`, `on`, `off`, `yes`, `no`, `1`, or `0`.

Ratio settings use one canonical form from `0.0` to `1.0`. For example, `0.05` means 5% and waits until five percentage points of new progress have accumulated before showing another message for that schematic. Percent-suffixed and whole-percent command forms are intentionally not accepted.

Player settings:

- `progressChatEnabled` - Shows delayed research and Proficiency chat messages when world settings allow them. Aliases: `chatenabled`, `chatnotifications`.
- `progressToastEnabled` - Shows popup progress notifications and botch toasts when world settings allow them. The Rich HUD progress bars are separate. Completion toasts are not suppressed by this setting. Aliases: `toastenabled`, `toastnotifications`.
- `progressHudEnabled` - Shows the Rich HUD progress bar overlay. Aliases: `barsenabled`, `hudbars`, `progressbars`.
- `progressHudRows` - Maximum recent schematic rows shown in the progress bar overlay. Range: `1` to `10`. Aliases: `hudrows`, `progressrows`.
- `progressHudOrder` - Row ordering for the progress bar overlay. Values: `ascending` or `descending`. The default `descending` keeps the newest row at the end of the stack. Aliases: `hudorder`, `progressorder`.
- `progressHudPosition` - Position preset for the progress bar overlay. Values: `topLeft`, `topRight`, `bottomLeft`, `bottomRight`, or `center`. Aliases: `hudposition`, `progressposition`, `hudpreset`.
- `progressHudOffsetX` - Horizontal offset added to the progress bar position preset. Positive values move right. Range: `-2.0` to `2.0`. Aliases: `hudx`, `hudoffsetx`, `progressx`.
- `progressHudOffsetY` - Vertical offset added to the progress bar position preset. Positive values move up. Range: `-2.0` to `2.0`. Aliases: `hudy`, `hudoffsety`, `progressy`.
- `progressHudFadeSeconds` - Seconds a progress bar row stays active before fading. Use `0` to keep recent history visible until replaced. Range: `0.0` to `60.0`. Aliases: `hudfade`, `progressfade`, `barfade`.
- `researchChatSuppressionPercent` - Minimum accumulated research percent before another research chat update appears. Aliases: `reschatsuppression`, `researchchatthreshold`, `reschatthreshold`.
- `proficiencyChatSuppressionPercent` - Minimum accumulated Proficiency percent before another Proficiency chat update appears. Aliases: `profchatsuppression`, `proficiencychatthreshold`, `profchatthreshold`.
- `researchToastSuppressionPercent` - Minimum accumulated research percent before another research toast appears. Aliases: `restoastsuppression`, `researchtoastthreshold`, `restoastthreshold`.
- `proficiencyToastSuppressionPercent` - Minimum accumulated Proficiency percent before another Proficiency toast appears. Aliases: `proftoastsuppression`, `proficiencytoastthreshold`, `proftoastthreshold`.
- `completionSoundEnabled` - Plays research and Proficiency completion sounds when world settings allow them.
- `weldBotchSoundEnabled` - Plays positional botch sounds for this player when world settings allow them.
- `weldBotchWarningCooldownSeconds` - Minimum time before repeating the same botch warning for this player and block. Use `default` to follow the world setting, or a number from `0.0` to `30.0`.

Personal defaults keep progress chat and popup progress toasts off, Rich HUD progress bars on with five top-right rows, `descending` row order, zero overlay offset, six-second fade timing, sounds on, zero suppression thresholds, and botch warning cooldown set to `default`. Players can enable chat with `/wk config progressChatEnabled true` and popup toasts with `/wk config progressToastEnabled true` when world settings allow them.

## Admin Command Flow

Start with:

```text
/wk admin
/wk admin audit
/wk config world
/wk config world help
/wk difficulty
```

`/wk admin` is the admin command index. It points to the world config categories and the admin-only research, Proficiency, difficulty, and layer-audit tools.

`/wk admin audit` reports whether the runtime loaded, how many Working Knowledge layers were found, how many mappings became active, and any compatibility issues. Detailed issues also appear in `SpaceEngineers.log` and the F11 mod-error screen.

World config commands:

```text
/wk config world
/wk config world help
/wk config world reset
/wk config <setting> help
/wk config <setting> <value>
```

`/wk config world reset` resets all world settings to the built-in `easy` defaults. Existing worlds keep their saved settings until an administrator applies a preset, changes a value, or explicitly resets them.

When a world setting is changed manually through `/wk config <setting> <value>`, `difficultyPreset` is marked as `custom`.

## Difficulty Presets

Use:

```text
/wk difficulty
/wk difficulty help
/wk difficulty novice
/wk difficulty easy
/wk difficulty medium
/wk difficulty hard
/wk difficulty extreme
/wk difficulty custom <setting> help
/wk difficulty custom <setting> <value>
```

Only admins can apply presets or custom difficulty modifiers.

Presets only change the difficulty-facing modifier fields. They do not change feature flags, research curve values, Proficiency curve values, feedback settings, detailed botch curve shape values, or new-player defaults.

- `novice` - `2.0` research, Proficiency, data rewards, data loot, salvage, and forgiveness; `0.5` botch chance, damage, and pressure.
- `easy` - `1.5` research, Proficiency, data rewards, data loot, salvage, and forgiveness; `0.75` botch chance, damage, and pressure.
- `medium` - neutral `1.0` baseline.
- `hard` - `0.5` research, Proficiency, data rewards, and data loot; `0.75` salvage and forgiveness; `1.5` botch chance, damage, and pressure.
- `extreme` - `0.25` research, Proficiency, data rewards, and data loot; `0.5` salvage and forgiveness; `2.0` botch chance, damage, and pressure.

Custom difficulty accepts only these settings:

- `researchScale`
- `dataFragmentRewardScale`
- `dataFragmentLootScale`
- `proficiencyGainScale`
- `salvageScale`
- `weldBotchChanceScale`
- `weldBotchRawLossRatio`
- `weldBotchPressureScale`
- `weldBotchForgivenessScale`

Setting aliases work for custom difficulty too. For example, the in-game help may show `botchDamageScale`, which is an alias for `weldBotchRawLossRatio`.

Examples:

```text
/wk difficulty hard
/wk difficulty custom researchScale 0.75
/wk difficulty custom proficiencyGainScale 1.25
/wk difficulty custom botchDamageScale help
/wk difficulty custom botchDamageScale 1.25
```

## Admin Progress Commands

Normal players can run `/wk research` and `/wk proficiency` for their own summaries. The detailed `/wk research help` and `/wk proficiency help` screens are currently admin-only because they document admin target commands.

Research admin commands:

```text
/wk research show <player>
/wk research reset server
/wk research reset <player>
/wk research unlock <player> <schematic>
/wk research forget <player> <schematic>
/wk research set <player> <schematic> <progress>
```

Proficiency admin commands:

```text
/wk proficiency show <player>
/wk proficiency reset server
/wk proficiency reset <player>
/wk proficiency master <player> <schematic>
/wk proficiency forget <player> <schematic>
/wk proficiency set <player> <schematic> <progress>
```

Targets can be:

- `me`
- `online`
- `all`
- An online player display name, quoted when it contains spaces.
- An identity id.
- A Steam id that resolves to an identity.

Schematic names can be display names, display names without the `Schematics` suffix, research ids, or unique partial matches. Examples:

```text
Ion Thruster Schematics
Ion Thruster
propulsion.ion_thruster
Basic Production Schematics
production.basic
```

Progress uses a ratio from `0.0` to `1.0`; for example, `0.75` means 75%.

Admin examples:

```text
/wk research unlock me Ion Thruster Schematics
/wk research set "Bob Smith" Basic Production Schematics 0.75
/wk proficiency master me Basic Production Schematics
/wk proficiency set online production.basic 0.8
```

`/wk admin unlockall` completes every schematic family for the admin identity running the command. It does not change Proficiency. It is intended for creative/admin testing of welding, botches, recovery, and fully unlocked research behavior.

## World Setting Reference

Setting names are case-insensitive in chat commands. Underscores and hyphens are ignored, so `researchScale`, `research-scale`, and `research_scale` resolve the same way.

Use `/wk config <setting> help` in game for the current value, accepted value type, category, description, and aliases.

Rich HUD presents multipliers on a logarithmic-with-zero slider covering the practical `0.01x` to `10x` tuning range, with `0` retained as an explicit endpoint. The wider command/persistence limits listed below remain available where applicable so established outlier configurations are not clamped or rewritten merely by opening the panel.

### Difficulty

- `difficultyPreset` - Read-only label for the last applied difficulty preset. Manual world config edits change this to `custom`.

### Research

- `dataFragmentsEnabled` - Enables consumable data fragments and exact data schematics. Disabled items are refunded and grant no research. Aliases: `fragments`, `datafragments`, `researchdata`.
- `researchScale` - Global multiplier for all active research gain. Range: `0.0` to `100.0`. Aliases: `researchgain`, `researchgainscale`.
- `researchGrindingGainScale` - Multiplier for schematic research gained by grinding blocks. Range: `0.0` to `100.0`. Aliases: `researchgrinding`, `grindingresearchscale`.
- `dataFragmentRewardScale` - Multiplier for partial research progress from consumable data fragments. Range: `0.0` to `100.0`. Aliases: `fragmentrewardscale`, `datafragmentrewards`.
- `dataFragmentLootScale` - Multiplier for data fragment loot frequency in patched container definitions. Existing spawned containers are not retroactive. Range: `0.0` to `100.0`. Aliases: `fragmentlootscale`, `datafragmentloot`.
- `researchEfficiencyStart` - Research efficiency at 0% known. Higher values front-load discovery. Range: `0.0` to `10.0`.
- `researchEfficiencyEnd` - Research efficiency near 100% known. Lower values slow the final stretch. Range: `0.0` to `10.0`.

Schematic-specific work rewards are source-code balance data in `Application/Balance/SchematicWorkRewardTable.cs`. Each row stores large-grid base work reward, small-grid base work reward, and reference build seconds. The fallback defaults are `0.21`, `0.0857`, and `8.0`. Vanilla block build time then applies a clamped square-root factor before research or Proficiency curve and config multipliers. Generated audit reports, when refreshed, live under `docs/WorkingKnowledge/generated/`.

### Proficiency

- `proficiencyEnabled` - Enables the player-only hands-on Proficiency system. Alias: `proficiency`.
- `proficiencyGainScale` - Global multiplier for hands-on Proficiency gain. Range: `0.0` to `100.0`. Aliases: `proficiencyscale`, `profgain`, `profgainscale`.
- `proficiencyGrindingGainScale` - Multiplier for Proficiency gained by grinding. Range: `0.0` to `100.0`. Aliases: `profgrindscale`, `grindingproficiencyscale`.
- `proficiencyWeldingGainScale` - Multiplier for Proficiency gained by welding and repairing. Range: `0.0` to `100.0`. Aliases: `profweldscale`, `weldingproficiencyscale`.
- `proficiencyFirstThreshold` - End of the fast first Proficiency segment. Progress value from `0.0` to `1.0`.
- `proficiencySecondThreshold` - End of the slow second Proficiency segment. Progress value from `proficiencyFirstThreshold` to `1.0`. The default `1.0` makes Proficiency a two-stage curve.
- `proficiencyFirstSegmentRate` - Base progress earned per table work reward unit in the fast segment before the segment curve is applied. Range: `0.0` to `10.0`.
- `proficiencySecondSegmentRate` - Base progress earned per table work reward unit in the slow segment before the segment curve is applied. Range: `0.0` to `10.0`.
- `proficiencyFinalSegmentRate` - Optional final segment base rate used only when the second threshold is below 100%. Range: `0.0` to `10.0`.

Progress, probability, and ratio commands use values from `0.0` to `1.0`; for example, `0.8` means 80%. Rich HUD displays these values as percentages while sending the canonical ratio to the server.

### Salvage

- `salvageScrapEnabled` - Enables low-Proficiency salvage conversion from recovered components into scrap. Aliases: `scrap`, `salvage`.
- `salvageScale` - Multiplier for intact component recovery before the 100% cap. Range: `0.0` to `100.0`. Aliases: `salvagerecovery`, `salvagerecoveryscale`.
- `salvageScrapYield` - Component mass ratio returned as scrap ore when low-Proficiency grinding converts components to scrap. Default `0.20`, or 20%. Command range: `0.0` to `1.0`. Aliases: `scrapyield`, `salvagescrapratio`, `scrapratio`.
- `proficiencyGrindingLossEnabled` - Enables Proficiency-biased scrap recovery while grinding. Alias: `grindingloss`.

By default, components converted to scrap return `20%` of their component mass as scrap ore.

### Botches

- `proficiencyWeldingLossEnabled` - Enables low-Proficiency construction botches while welding. Alias: `weldingloss`.
- `proficiencyBuildCapEnabled` - Enables guaranteed failure at the current post-functional Proficiency cap. Alias: `buildcap`.
- `weldBotchBaseChance` - Botch chance scale near the functional threshold. The medium value maps to inverse Proficiency. Range: `0.0` to `1.0`.
- `weldBotchMaxChance` - Maximum normal botch chance before the Proficiency cap forces guaranteed failure. Range: `0.0` to `1.0`.
- `weldBotchChanceScale` - Difficulty-facing multiplier for botch chance before the max chance clamp. Range: `0.0` to `100.0`. Aliases: `botchchancescale`, `botchchance`.
- `weldBotchPostFunctionalPressure` - Shapes the post-functional botch-zone climb. Range: `0.0` to `100.0`.
- `weldBotchSoftCapPressure` - Shapes the post-functional Proficiency cap pressure. Range: `0.0` to `100.0`.
- `weldBotchPressureScale` - Difficulty-facing multiplier for post-functional botch pressure curves. Range: `0.0` to `100.0`. Aliases: `botchpressurescale`, `botchpressure`.
- `weldBotchRawLossRatio` - Raw integrity loss as a multiple of the triggering positive integrity delta. Range: `0.0` to `10.0`. Aliases: `weldbotchdamagescale`, `botchdamagescale`, `botchdamage`.
- `weldBotchForgivenessScale` - Multiplier for returned component forgiveness after botch damage. Higher is gentler. Range: `0.0` to `100.0`. Aliases: `botchforgivenessscale`, `botchforgiveness`.

### Feedback Defaults

World feedback settings act as defaults and minimums for player feedback settings.

For overlapping player/world settings, chat commands use `default...` world names so they are distinct from personal settings. For example:

```text
/wk config defaultProgressChatEnabled false
/wk config progressChatEnabled false
```

The first command changes the world default. The second command changes only the sender's personal setting.

World feedback settings:

- `notificationDelaySeconds` - World delay used to combine repeated progress updates before chat/toast feedback. Range: `0.1` to `30.0` seconds. Alias: `notificationdelay`.
- `defaultProgressChatEnabled` - World default for delayed progress chat messages. Aliases: `defaultchat`, `defaultchatenabled`, `worldprogresschat`.
- `defaultProgressToastEnabled` - World default for popup progress notifications and botch toasts. The Rich HUD progress bars are separate. Completion toasts are not suppressed by this setting. Aliases: `defaulttoast`, `defaulttoastenabled`, `worldprogresstoast`.
- `defaultResearchChatSuppressionPercent` - World default/minimum accumulated research percent before another research chat update appears. Aliases: `defaultreschatsuppression`, `defaultresearchchatthreshold`, `worldresearchchatthreshold`.
- `defaultProficiencyChatSuppressionPercent` - World default/minimum accumulated Proficiency percent before another Proficiency chat update appears. Aliases: `defaultprofchatsuppression`, `defaultproficiencychatthreshold`, `worldproficiencychatthreshold`.
- `defaultResearchToastSuppressionPercent` - World default/minimum accumulated research percent before another research toast appears. Aliases: `defaultrestoastsuppression`, `defaultresearchtoastthreshold`, `worldresearchtoastthreshold`.
- `defaultProficiencyToastSuppressionPercent` - World default/minimum accumulated Proficiency percent before another Proficiency toast appears. Aliases: `defaultproftoastsuppression`, `defaultproficiencytoastthreshold`, `worldproficiencytoastthreshold`.
- `defaultCompletionSoundEnabled` - World default for completed research and mastered Proficiency sounds.
- `defaultWeldBotchWarningCooldownSeconds` - World default/minimum time before repeating the same botch warning for the same player and block. Range: `0.0` to `30.0` seconds.
- `defaultWeldBotchSoundEnabled` - World default for botch warning sounds.
- `weldBotchSoundSubtype` - Space Engineers audio subtype played for weld botches.
- `weldBotchSoundRange` - Range in meters for positional botch sounds. Range: `0.0` to `1000.0`.

### New-Player Defaults

- `fundamentalsResearchUnlocked` - Grants the baseline Fundamentals schematic to new or joining players. Aliases: `fundamentalsunlocked`, `fundamentalsresearch`, `fundamentalresearchunlocked`, `fundamentalunlocked`, `fundamentalresearch`.
- `fundamentalsProficiencyProgress` - Minimum baseline Proficiency for Fundamentals. Progress value from `0.0` to `1.0`. Aliases: `fundamentalsproficiency`, `fundamentalsprof`, `fundamentalproficiencyprogress`, `fundamentalproficiency`, `fundamentalprof`.

## Help Topics In Game

Admins can use these help topics from the `/wk admin` index:

```text
/wk research help
/wk proficiency help
/wk botch help
/wk salvage help
/wk feedback help
/wk defaults help
/wk difficulty help
```

The world config display is grouped by chat senders such as `Config - Research`, `Config - Proficiency`, `Config - Botches`, `Config - Salvage`, `Config - Feedback`, and `Config - Defaults`.

## Persistence

World configuration and player feedback preferences are saved with the rest of Working Knowledge's versioned state inside the world's `Sandbox.sbc` checkpoint. Players with no explicit preference record use the built-in player defaults.

Worlds created before `1.0.2` may contain `WkKnConfig.xml` and `WkKnPlayerConfig.xml`. When no current checkpoint entry exists, those files are loaded once and migrated during the next normal world save. They are not updated after migration.

The persisted field names are not always chat command names. In particular, overlapping world feedback fields are edited with `default...` command names:

- `ProgressChatEnabled` is chat command `defaultProgressChatEnabled`.
- `ProgressToastEnabled` is chat command `defaultProgressToastEnabled`.
- `ResearchChatSuppressionPercent` is chat command `defaultResearchChatSuppressionPercent`.
- `ProficiencyChatSuppressionPercent` is chat command `defaultProficiencyChatSuppressionPercent`.
- `ResearchToastSuppressionPercent` is chat command `defaultResearchToastSuppressionPercent`.
- `ProficiencyToastSuppressionPercent` is chat command `defaultProficiencyToastSuppressionPercent`.
- `CompletionSoundEnabled` is chat command `defaultCompletionSoundEnabled`.
- `WeldBotchWarningCooldownSeconds` is chat command `defaultWeldBotchWarningCooldownSeconds`.
- `WeldBotchSoundEnabled` is chat command `defaultWeldBotchSoundEnabled`.

The four persisted `...SuppressionPercent` fields retain their established `0` to `100` percent-point representation for save compatibility. Commands and Rich HUD translate at the boundary, so `0.05` is displayed as 5% and remains serialized as `5`. No existing saved value is reinterpreted or migrated in 1.1.0.

## Default World Configuration Shape

The canonical checkpoint snapshot serializes this default world configuration shape:

```xml
<WkConfig>
  <DifficultyPreset>easy</DifficultyPreset>
  <SalvageScrapEnabled>true</SalvageScrapEnabled>
  <DataFragmentsEnabled>true</DataFragmentsEnabled>
  <ResearchScale>1.5</ResearchScale>
  <ResearchGrindingGainScale>1</ResearchGrindingGainScale>
  <DataFragmentRewardScale>1.5</DataFragmentRewardScale>
  <DataFragmentLootScale>1.5</DataFragmentLootScale>
  <ResearchEfficiencyStart>1</ResearchEfficiencyStart>
  <ResearchEfficiencyEnd>0.5</ResearchEfficiencyEnd>
  <SalvageScale>1.5</SalvageScale>
  <SalvageScrapYield>0.2</SalvageScrapYield>
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
  <ProficiencyGainScale>1.5</ProficiencyGainScale>
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
  <WeldBotchChanceScale>0.75</WeldBotchChanceScale>
  <WeldBotchPostFunctionalPressure>4</WeldBotchPostFunctionalPressure>
  <WeldBotchSoftCapPressure>3</WeldBotchSoftCapPressure>
  <WeldBotchPressureScale>0.75</WeldBotchPressureScale>
  <WeldBotchRawLossRatio>0.75</WeldBotchRawLossRatio>
  <WeldBotchForgivenessScale>1.5</WeldBotchForgivenessScale>
  <WeldBotchWarningCooldownSeconds>0</WeldBotchWarningCooldownSeconds>
  <WeldBotchSoundEnabled>true</WeldBotchSoundEnabled>
  <WeldBotchSoundSubtype>ArcPoofExplosionCat1</WeldBotchSoundSubtype>
  <WeldBotchSoundRange>75</WeldBotchSoundRange>
</WkConfig>
```
