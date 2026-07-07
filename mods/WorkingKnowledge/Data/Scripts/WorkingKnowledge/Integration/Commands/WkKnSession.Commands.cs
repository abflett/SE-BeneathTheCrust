using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const string WorkingKnowledgeCommand = "/wk";

        private void OnMessageEntered(ulong sender, string messageText, ref bool sendToOthers)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return;

            var message = messageText.Trim();
            var args = TokenizeCommand(message);
            if (args.Length == 0)
                return;

            if (!args[0].Equals(WorkingKnowledgeCommand, StringComparison.OrdinalIgnoreCase))
                return;

            sendToOthers = false;
            if (args.Length == 1 || args[1].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowCommandHelp(sender);
                return;
            }

            if (IsResearchModule(args[1]))
            {
                HandleResearchCommand(sender, args);
                return;
            }

            if (args[1].Equals("config", StringComparison.OrdinalIgnoreCase))
            {
                HandleConfigCommand(sender, args);
                return;
            }

            if (args[1].Equals("difficulty", StringComparison.OrdinalIgnoreCase))
            {
                HandleDifficultyCommand(sender, args);
                return;
            }

            if (args[1].Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                HandleAdminCommand(sender, args);
                return;
            }

            if (IsBotchModule(args[1]))
            {
                ShowCategoryHelpOrCommandHelp(sender, args, ShowBotchHelp);
                return;
            }

            if (args[1].Equals("salvage", StringComparison.OrdinalIgnoreCase))
            {
                ShowCategoryHelpOrCommandHelp(sender, args, ShowSalvageHelp);
                return;
            }

            if (args[1].Equals("compatibility", StringComparison.OrdinalIgnoreCase))
            {
                ShowCategoryHelpOrCommandHelp(sender, args, ShowCompatibilityHelp);
                return;
            }

            if (args[1].Equals("feedback", StringComparison.OrdinalIgnoreCase))
            {
                ShowCategoryHelpOrCommandHelp(sender, args, ShowFeedbackHelp);
                return;
            }

            if (args[1].Equals("defaults", StringComparison.OrdinalIgnoreCase))
            {
                ShowCategoryHelpOrCommandHelp(sender, args, ShowDefaultsHelp);
                return;
            }

            if (args[1].Equals("proficiency", StringComparison.OrdinalIgnoreCase) || args[1].Equals("prof", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencyCommand(sender, args);
                return;
            }

            ShowCommandHelp(sender);
        }

        private void HandleConfigCommand(ulong sender, string[] args)
        {
            var identityId = ResolveIdentityId(sender);
            var playerConfigId = GetPlayerConfigId(identityId);
            if (args.Length == 2)
            {
                ShowPlayerConfig(playerConfigId);
                return;
            }

            if (args.Length == 3 && args[2].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowConfigHelp();
                return;
            }

            if (args.Length == 3 && args[2].Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                ResetPlayerConfig(playerConfigId);
                return;
            }

            if (args.Length == 4 &&
                args[2].Equals("world", StringComparison.OrdinalIgnoreCase) &&
                args[3].Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                if (CanEditConfig(sender))
                    ResetWorldConfig();
                else
                    ShowWkWarningMessage("Admin required for world config edits.");
                return;
            }

            if (args.Length == 4 &&
                args[2].Equals("world", StringComparison.OrdinalIgnoreCase) &&
                args[3].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (CanEditConfig(sender))
                    ShowWorldConfigHelp();
                else
                    ShowWkWarningMessage("Admin required for world config help.");
                return;
            }

            if (args.Length == 4 && args[3].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (playerConfigStore.IsSetting(args[2]))
                {
                    ShowPlayerConfigSettingHelp(args[2], playerConfigId);
                    return;
                }

                if (configStore.IsSetting(args[2]))
                {
                    if (CanEditConfig(sender))
                        ShowAdminConfigSettingHelp(args[2]);
                    else
                        ShowWkWarningMessage("Admin required for world config details.");
                    return;
                }

                ShowWkWarningMessage("Unknown config setting: " + args[2]);
                return;
            }

            if (args.Length == 4)
            {
                var setting = args[2];
                var value = args[3];

                if (playerConfigStore.IsSetting(setting))
                {
                    UpdatePlayerConfigSetting(playerConfigId, setting, value);
                    return;
                }

                if (configStore.IsSetting(setting))
                {
                    if (CanEditConfig(sender))
                        UpdateConfigSetting(setting, value);
                    else
                        ShowWkWarningMessage("Admin required for world config edits.");
                    return;
                }

                ShowWkWarningMessage("Unknown config setting: " + setting);
                return;
            }

            if (args.Length == 3 && args[2].Equals("world", StringComparison.OrdinalIgnoreCase))
            {
                if (CanEditConfig(sender))
                    ShowConfig();
                else
                    ShowWkWarningMessage("Admin required for world config.");
                return;
            }

            if (!CanEditConfig(sender))
            {
                ShowWkWarningMessage("Admin required for config edits.");
                return;
            }

            ShowConfigHelp();
        }

        private void HandleDifficultyCommand(ulong sender, string[] args)
        {
            if (args.Length == 2)
            {
                ShowDifficulty();
                return;
            }

            if (args.Length == 3 && args[2].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (!CanEditConfig(sender))
                {
                    ShowWkWarningMessage("Admin required for difficulty help.");
                    return;
                }

                ShowDifficultyHelp();
                return;
            }

            if (args.Length == 5 &&
                args[2].Equals("custom", StringComparison.OrdinalIgnoreCase) &&
                args[4].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (!CanEditConfig(sender))
                {
                    ShowWkWarningMessage("Admin required for difficulty help.");
                    return;
                }

                ShowDifficultyCustomSettingHelp(args[3]);
                return;
            }

            if (!CanEditConfig(sender))
            {
                ShowWkWarningMessage("Admin required for difficulty edits.");
                return;
            }

            if (args[2].Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length != 5)
                {
                    ShowDifficultyHelp();
                    return;
                }

                UpdateDifficultyCustomSetting(args[3], args[4]);
                return;
            }

            if (args.Length != 3)
            {
                ShowDifficultyHelp();
                return;
            }

            string error;
            if (!configStore.TryApplyDifficulty(args[2], out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            SaveConfigStore();
            ApplyWorldConfigRuntimeEffects();
            ShowWkChatSection("Difficulty Applied", GetDifficultyDisplayLines(false));
        }

        private void UpdateDifficultyCustomSetting(string setting, string value)
        {
            string error;
            if (!configStore.TrySetDifficultyModifier(setting, value, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            SaveConfigStore();
            ApplyWorldConfigRuntimeEffects();
            ShowWkChatSection("Difficulty Updated", GetDifficultyDisplayLines(false));
        }

        private void ShowDifficultyCustomSettingHelp(string setting)
        {
            if (!configStore.IsDifficultyModifierSetting(setting))
            {
                ShowWkWarningMessage("Unknown difficulty setting: " + setting);
                return;
            }

            List<string> lines;
            if (!configStore.TryGetSettingHelp(setting, out lines))
            {
                ShowWkWarningMessage("Unknown difficulty setting: " + setting);
                return;
            }

            ShowSettingHelpSections("Difficulty", lines);
        }

        private void HandleAdminCommand(ulong sender, string[] args)
        {
            if (!CanEditConfig(sender))
            {
                ShowWkWarningMessage("Admin required for admin commands.");
                return;
            }

            if (args.Length == 2 || args[2].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                ShowAdminHelp();
                return;
            }

            if (args.Length == 3 && args[2].Equals("unlockall", StringComparison.OrdinalIgnoreCase))
            {
                UnlockAllResearchForAdmin(sender);
                return;
            }

            ShowAdminHelp();
        }

        private void UpdateConfigSetting(string setting, string value)
        {
            string error;
            if (!configStore.TrySetValue(setting, value, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            configStore.MarkCustom();
            configStore.Normalize();
            SaveConfigStore();
            ApplyWorldConfigRuntimeEffects();
            ShowWkChatSection("Config Updated", configStore.GetLine(setting));
        }

        private void UpdatePlayerConfigSetting(string playerConfigId, string setting, string value)
        {
            string error;
            if (!playerConfigStore.TrySetValue(playerConfigId, setting, value, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            SavePlayerConfigStore();
            ShowWkChatSection("Player Config Updated", playerConfigStore.GetLine(playerConfigId, setting));
        }

        private void ResetPlayerConfig(string playerConfigId)
        {
            playerConfigStore.ResetPlayer(playerConfigId);
            SavePlayerConfigStore();
            ShowWkChatSection("Player Config Reset", "Player settings now follow world settings.");
            ShowPlayerConfig(playerConfigId);
        }

        private void ResetWorldConfig()
        {
            configStore.Reset();
            SaveConfigStore();
            ApplyWorldConfigRuntimeEffects();
            ShowConfig();
        }

        private void ApplyWorldConfigRuntimeEffects()
        {
            RebuildResearchDefinitions();
            ApplyFundamentalsDefaultsForOnlinePlayers();
            SyncCompletedResearchForOnlinePlayers();
            RefreshResearchDataFragmentsInContainerLoot();
        }

        private void ShowCommandHelp(ulong sender)
        {
            var lines = new List<string>
            {
                "/wk research - Display your schematic progress",
                "/wk res - Alias for research",
                "/wk proficiency - Display your Proficiency",
                "/wk prof - Alias for proficiency",
                "/wk config - Display player settings",
                "/wk config help - Display player config commands",
                "/wk difficulty - Display current difficulty",
            };

            if (CanEditConfig(sender))
                lines.Add("/wk admin - Display admin command index");

            lines.Add("\nWith chat entry open, press Page Up or Page Down to scroll chat text.");
            ShowWkChatSection("Working Knowledge Commands", lines);
        }

        private void ShowResearchHelp()
        {
            ShowWkChatSection(
                "Research Help",
                "Schematic discovery, unlock pacing, and research data items.",
                "Player command: /wk research");
            ShowWkChatSection(
                "Research Admin Commands",
                "/wk research show <player>",
                "/wk research reset server",
                "/wk research reset <player>",
                "/wk research unlock <player> <schematic>",
                "/wk research forget <player> <schematic>",
                "/wk research set <player> <schematic> <percent>");
            ShowWkChatSection(
                "Research Targets",
                "Players: me, all, online, \"Player Name\", identity id, or Steam id",
                "Schematics: display name or research id, such as Ion Thruster Schematics or propulsion.ion_thruster");
            ShowWkChatSection(
                "Research Examples",
                "/wk research unlock me Ion Thruster Schematics",
                "/wk research set \"Alex\" Basic Production Schematics 75%",
                "/wk config researchScale help");
            ShowWkChatSection(
                "Research Config Settings",
                SplitConfigSettingList("dataFragmentsEnabled, researchScale, researchGrindingGainScale, dataFragmentRewardScale, dataFragmentLootScale, researchEfficiencyStart, researchEfficiencyEnd"));
        }

        private void ShowProficiencyHelp()
        {
            ShowWkChatSection(
                "Proficiency Help",
                "Hands-on skill gain, build comfort, and source scaling.",
                "Player command: /wk proficiency");
            ShowWkChatSection(
                "Proficiency Admin Commands",
                "/wk proficiency show <player>",
                "/wk proficiency reset server",
                "/wk proficiency reset <player>",
                "/wk proficiency master <player> <schematic>",
                "/wk proficiency forget <player> <schematic>",
                "/wk proficiency set <player> <schematic> <percent>");
            ShowWkChatSection(
                "Proficiency Targets",
                "Players: me, all, online, \"Player Name\", identity id, or Steam id",
                "Schematics: display name or research id, such as Basic Production Schematics or production.basic");
            ShowWkChatSection(
                "Proficiency Examples",
                "/wk proficiency master me Basic Production Schematics",
                "/wk proficiency set \"Alex\" Ion Thruster Schematics 60%",
                "/wk config proficiencyGainScale help");
            ShowWkChatSection(
                "Proficiency Config Settings",
                SplitConfigSettingList("proficiencyEnabled, proficiencyGainScale, proficiencyGrindingGainScale, proficiencyWeldingGainScale, proficiencyFirstThreshold, proficiencySecondThreshold, proficiencyFirstSegmentRate, proficiencySecondSegmentRate, proficiencyFinalSegmentRate, proficiencyBuildCapEnabled"));
        }

        private void ShowConfig()
        {
            foreach (var category in configStore.GetDisplayCategories())
                ShowWkChatSection("Config - " + category, configStore.GetDisplayLinesForCategory(category));
        }

        private void ShowPlayerConfig(string playerConfigId)
        {
            ShowWkChatSection("Player Config", playerConfigStore.GetDisplayLines(playerConfigId));
        }

        private void ShowConfigHelp()
        {
            ShowWkChatSection(
                "Player Config Help",
                "Personal feedback preferences for chat, HUD notifications, and sounds.",
                "Resetting player settings makes them follow current world settings.");
            ShowWkChatSection(
                "Player Config Commands",
                "/wk config - Display current player settings",
                "/wk config <setting> help - Display setting details",
                "/wk config <setting> <value> - Change the setting value",
                "/wk config reset - Reset player settings");
            ShowWkChatSection(
                "Player Config Settings",
                playerConfigStore.GetSettingLines());
        }

        private void ShowWorldConfigHelp()
        {
            ShowWkChatSection(
                "World Config Help",
                "Admin-only settings for progression, difficulty, botches, salvage, feedback defaults, and new-player defaults.");
            ShowWkChatSection(
                "World Config Commands",
                "/wk config world - Display all world settings",
                "/wk config world help - Display world config help",
                "/wk config world reset - Reset world settings to mod defaults",
                "/wk config <setting> help - Display one world setting",
                "/wk config <setting> <value> - Update one world setting");
            ShowWkChatSection(
                "World Config Help Topics",
                "/wk difficulty help - Presets and difficulty modifier settings",
                "/wk research help - Research gain and unlock settings",
                "/wk proficiency help - Proficiency gain and skill settings",
                "/wk botch help - Build failure chance, damage, and sound settings",
                "/wk salvage help - Grinding recovery and scrap settings",
                "/wk feedback help - Chat, toast, sound, and notification defaults",
                "/wk defaults help - New-player Fundamentals defaults");
        }

        private void ShowHelpTopic(string topic, string summary, string[] commands, string configSettings)
        {
            ShowWkChatSection(topic + " Help", summary);

            if (commands != null && commands.Length > 0)
                ShowWkChatSection(topic + " Commands", commands);

            if (!string.IsNullOrWhiteSpace(configSettings))
                ShowWkChatSection(topic + " Config Settings", SplitConfigSettingList(configSettings));
        }

        private void ShowWorldConfigHelpTopic(string topic, string summary, string configSettings)
        {
            ShowHelpTopic(
                topic,
                summary,
                new[]
                {
                    "/wk config <setting> help - Display setting details",
                    "/wk config <setting> <value> - Update listed world settings"
                },
                configSettings);
        }

        private void ShowPlayerConfigSettingHelp(string setting, string playerConfigId)
        {
            List<string> lines;
            if (!playerConfigStore.TryGetSettingHelp(setting, playerConfigId, out lines))
            {
                ShowWkWarningMessage("Unknown player config setting: " + setting);
                return;
            }

            ShowSettingHelpSections("Player Feedback", lines);
        }

        private void ShowAdminConfigSettingHelp(string setting)
        {
            List<string> lines;
            if (!configStore.TryGetSettingHelp(setting, out lines))
            {
                ShowWkWarningMessage("Unknown config setting: " + setting);
                return;
            }

            ShowSettingHelpSections(GetSettingHelpCategory(lines), lines);
        }

        private void ShowSettingHelpSections(string headingPrefix, List<string> lines)
        {
            if (string.IsNullOrWhiteSpace(headingPrefix) || lines == null || lines.Count == 0)
                return;

            var title = lines[0];
            var descriptionLines = new List<string>();
            var settingsLines = new List<string>();
            string settingLine = null;
            string valueLine = null;
            string aliasesLine = null;

            for (var i = 1; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("Category: ", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (line.StartsWith("Value: ", StringComparison.OrdinalIgnoreCase))
                {
                    valueLine = line;
                    continue;
                }

                if (line.StartsWith("Aliases: ", StringComparison.OrdinalIgnoreCase))
                {
                    aliasesLine = line;
                    continue;
                }

                if (settingLine == null && line.IndexOf(" = ", StringComparison.Ordinal) >= 0)
                {
                    settingLine = line;
                    continue;
                }

                descriptionLines.Add(line);
            }

            if (!string.IsNullOrWhiteSpace(aliasesLine))
                descriptionLines.Add(aliasesLine);

            if (!string.IsNullOrWhiteSpace(valueLine))
                settingsLines.Add(valueLine);

            if (!string.IsNullOrWhiteSpace(settingLine))
                settingsLines.Add(settingLine);

            ShowWkChatSection(headingPrefix + " - " + title, descriptionLines);
            ShowWkChatSection(headingPrefix + " - Settings", settingsLines);
        }

        private static string GetSettingHelpCategory(List<string> lines)
        {
            if (lines == null)
                return "Config";

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line) ||
                    !line.StartsWith("Category: ", StringComparison.OrdinalIgnoreCase))
                    continue;

                var category = line.Substring("Category: ".Length).Trim();
                if (!string.IsNullOrWhiteSpace(category))
                    return category;
            }

            return "Config";
        }

        private void ShowDifficulty()
        {
            ShowWkChatSection(
                "Difficulty",
                "Overall Working Knowledge difficulty settings.",
                "Presets: " + GetDifficultyPresetList() + ", custom");
            ShowWkChatSection("Difficulty Settings", GetDifficultyDisplayLines(false));
        }

        private void ShowDifficultyHelp()
        {
            ShowWkChatSection(
                "Difficulty Help",
                "Preset bundles for friendly difficulty modifiers. Medium is neutral.");
            ShowWkChatSection(
                "Difficulty Commands",
                "/wk difficulty - Display current difficulty",
                "/wk difficulty <preset> - Apply a difficulty preset",
                "/wk difficulty custom <setting> help - Display one difficulty setting",
                "/wk difficulty custom <setting> <value> - Set one difficulty modifier");
            ShowWkChatSection(
                "Difficulty Presets",
                "novice - 2x gains/rewards, gentler botches",
                "easy - 1.5x gains/rewards, lighter botches",
                "medium - neutral 1.0 baseline",
                "hard - slower gains/rewards, harsher botches",
                "extreme - 4x slower gains/rewards, heavy but playable botches");
            ShowWkChatSection(
                "Difficulty Custom Settings",
                configStore.GetDifficultyModifierSettingNames());
            ShowWkChatSection(
                "Difficulty Examples",
                "/wk difficulty hard",
                "/wk difficulty custom researchScale 0.75",
                "/wk difficulty custom botchDamageScale help",
                "/wk difficulty custom botchDamageScale 1.25");
        }

        private List<string> GetDifficultyDisplayLines(bool includePresetList)
        {
            var lines = new List<string>();
            lines.Add("difficultyPreset = " + config.DifficultyPreset);
            lines.Add(configStore.GetLine("researchScale"));
            lines.Add(configStore.GetLine("dataFragmentRewardScale"));
            lines.Add(configStore.GetLine("dataFragmentLootScale"));
            lines.Add(configStore.GetLine("proficiencyGainScale"));
            lines.Add(configStore.GetLine("salvageScale"));
            lines.Add(configStore.GetLine("weldBotchChanceScale"));
            lines.Add(configStore.GetLine("weldBotchRawLossRatio"));
            lines.Add(configStore.GetLine("weldBotchPressureScale"));
            lines.Add(configStore.GetLine("weldBotchForgivenessScale"));
            if (includePresetList)
                lines.Add("Presets: " + GetDifficultyPresetList());

            return lines;
        }

        private string GetDifficultyPresetList()
        {
            var names = new List<string>();
            foreach (var name in configStore.GetDifficultyPresetNames())
                names.Add(name);

            return string.Join(", ", names.ToArray());
        }

        private void ShowAdminHelp()
        {
            ShowWkChatSection(
                "Admin Help",
                "Category index for world tuning and admin-only configuration.");
            ShowWkChatSection(
                "Admin Commands",
                "/wk admin unlockall - Complete all research for yourself without changing Proficiency");
            ShowWkChatSection(
                "World Config Commands",
                "/wk config world - Display world settings",
                "/wk config world help - Display world config help",
                "/wk config world reset - Reset world settings to mod defaults",
                "/wk config <setting> help - Display world setting details",
                "/wk config <setting> <value> - Update one world setting");
            ShowWkChatSection(
                "Admin Help Categories",
                "/wk research help - Research gain and unlock tuning",
                "/wk proficiency help - Proficiency gain and skill tuning",
                "/wk botch help - Build failure chance, damage, forgiveness, and sound tuning",
                "/wk salvage help - Grinding recovery and scrap tuning",
                "/wk compatibility help - Mod block schematic mapping",
                "/wk feedback help - Chat, toast, sound, and notification tuning",
                "/wk defaults help - New-player Fundamentals defaults",
                "/wk difficulty help - Difficulty modifier presets");
        }

        private void ShowBotchHelp()
        {
            ShowWorldConfigHelpTopic(
                "Botch",
                "Low-Proficiency construction failures while welding.",
                "proficiencyWeldingLossEnabled, proficiencyBuildCapEnabled, weldBotchBaseChance, weldBotchMaxChance, weldBotchChanceScale, weldBotchPostFunctionalPressure, weldBotchSoftCapPressure, weldBotchPressureScale, weldBotchRawLossRatio, weldBotchForgivenessScale, defaultWeldBotchWarningCooldownSeconds, defaultWeldBotchSoundEnabled, weldBotchSoundSubtype, weldBotchSoundRange");
        }

        private void ShowSalvageHelp()
        {
            ShowWorldConfigHelpTopic(
                "Salvage",
                "Grinding recovery, scrap conversion, and salvage pressure.",
                "salvageScrapEnabled, salvageScale, salvageScrapYield, proficiencyGrindingLossEnabled, proficiencyGrindingGainScale");
        }

        private void ShowCompatibilityHelp()
        {
            ShowWorldConfigHelpTopic(
                "Compatibility",
                "Mappings for loaded mod blocks and uncatalogued public block fallback behavior.",
                "modBlockSchematicMappings");
        }

        private void ShowFeedbackHelp()
        {
            ShowWkChatSection(
                "Feedback Help",
                "Chat, HUD notification, completion, and sound feedback.");
            ShowWkChatSection(
                "Feedback Commands",
                "/wk config - Display player feedback settings",
                "/wk config reset - Reset player settings",
                "/wk config world - Display world settings",
                "/wk config <setting> help - Display setting details",
                "/wk config <setting> <value> - Update player settings or listed world defaults");
            ShowWkChatSection(
                "Feedback World Defaults",
                SplitConfigSettingList("notificationDelaySeconds, defaultProgressChatEnabled, defaultProgressToastEnabled, defaultResearchChatSuppressionPercent, defaultProficiencyChatSuppressionPercent, defaultResearchToastSuppressionPercent, defaultProficiencyToastSuppressionPercent, defaultCompletionSoundEnabled, defaultWeldBotchSoundEnabled, defaultWeldBotchWarningCooldownSeconds, weldBotchSoundSubtype, weldBotchSoundRange"));
            ShowWkChatSection(
                "Feedback Player Settings",
                playerConfigStore.GetSettingLines());
        }

        private void ShowDefaultsHelp()
        {
            ShowWorldConfigHelpTopic(
                "Defaults",
                "Starting schematic and Proficiency defaults for new or joining players.",
                "fundamentalsResearchUnlocked, fundamentalsProficiencyProgress");
        }

        private static IEnumerable<string> SplitConfigSettingList(string settings)
        {
            if (string.IsNullOrWhiteSpace(settings))
                yield break;

            var parts = settings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                var setting = parts[i].Trim();
                if (!string.IsNullOrWhiteSpace(setting))
                    yield return setting;
            }
        }

        private bool CanEditConfig(ulong sender)
        {
            return MyAPIGateway.Session == null || MyAPIGateway.Session.IsUserAdmin(sender);
        }

        private static bool IsResearchModule(string module)
        {
            return module.Equals("research", StringComparison.OrdinalIgnoreCase) ||
                   module.Equals("res", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBotchModule(string module)
        {
            return module.Equals("botch", StringComparison.OrdinalIgnoreCase) ||
                   module.Equals("botches", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowCategoryHelpOrCommandHelp(ulong sender, string[] args, Action showHelp)
        {
            if (args.Length == 2 || (args.Length == 3 && args[2].Equals("help", StringComparison.OrdinalIgnoreCase)))
            {
                showHelp();
                return;
            }

            ShowCommandHelp(sender);
        }

        private void ShowResearchSummary(long identityId)
        {
            var lines = researchSummaryService.BuildPersonalSummary(FindScope(researchStore.PlayerScopes, identityId.ToString()), GetSchematicDisplayName);
            ShowWkChatSection("Research Summary", lines);
        }

        private void ShowProficiencySummary(long identityId)
        {
            var lines = proficiencySummaryService.BuildPersonalSummary(FindProficiencyScope(identityId.ToString()), GetSchematicDisplayName);
            ShowWkChatSection("Proficiency Summary", lines);
        }

        private void UnlockAllResearchForAdmin(ulong sender)
        {
            var identityId = ResolveIdentityId(sender);
            if (identityId == 0)
            {
                ShowWkWarningMessage("Could not resolve admin identity.");
                return;
            }

            var total = 0;
            var changed = 0;
            var newlyCompleted = 0;

            foreach (var target in schematicCatalog.Targets)
            {
                total++;
                var previousProgress = GetPlayerResearchProgress(identityId, target.ResearchId);
                var result = RecordResearchProgress(
                    researchStore.PlayerScopes,
                    identityId.ToString(),
                    target.ResearchId,
                    target.UnlockerId.SubtypeName,
                    RequiredResearchProgress);

                if (result.Changed)
                    changed++;

                if (previousProgress < RequiredResearchProgress)
                    newlyCompleted++;
            }

            if (changed > 0)
            {
                NotifyResearchDisplayChanged(identityId);
                SaveResearchStore();
            }

            SyncCompletedResearchForIdentity(identityId);

            ShowWkChatSection(
                "Admin Research Unlock",
                "Completed research for current admin identity.",
                "Schematics: " + total,
                "Newly completed: " + newlyCompleted,
                "Store updates: " + changed,
                "Proficiency unchanged.");
        }

        private static string FormatProgress(double progress)
        {
            return Math.Round(Clamp01(progress) * 100.0, 2) + "%";
        }

        private static string FormatProgressDelta(double progress)
        {
            return Math.Round(Math.Max(0.0, progress) * 100.0, 2) + "%";
        }

        private string GetSchematicDisplayName(string researchId)
        {
            return schematicCatalog.GetDisplayName(researchId);
        }

    }
}
