using System;
using System.Collections.Generic;
using System.Globalization;

namespace WkKn
{
    internal sealed class WkConfigStore
    {
        private static readonly string[] difficultyModifierSettings = new string[]
        {
            "researchScale",
            "dataFragmentRewardScale",
            "dataFragmentLootScale",
            "proficiencyGainScale",
            "salvageScale",
            "weldBotchChanceScale",
            "weldBotchRawLossRatio",
            "weldBotchPressureScale",
            "weldBotchForgivenessScale",
        };

        private static readonly string[] displayCategoryOrder = new string[]
        {
            "Difficulty",
            "Research",
            "Proficiency",
            "Botches",
            "Salvage",
            "Feedback",
            "Defaults",
        };

        private static readonly WkConfigSettingDefinition[] settings = CreateSettings();

        private WkConfig data = WkConfigDifficultyPresets.CreateMedium();

        internal WkConfig Data
        {
            get { return data; }
        }

        internal void Reset()
        {
            data = WkConfigDifficultyPresets.CreateMedium();
            Normalize();
        }

        internal void SetData(WkConfig loaded)
        {
            data = loaded ?? WkConfigDifficultyPresets.CreateMedium();
            Normalize();
        }

        internal void MarkCustom()
        {
            if (data != null)
                data.DifficultyPreset = WkConfigDifficultyPresets.CustomPreset;
        }

        internal bool TryApplyDifficulty(string presetName, out string error)
        {
            if (data == null)
                data = WkConfigDifficultyPresets.CreateMedium();

            if (!WkConfigDifficultyPresets.TryApply(presetName, data, out error))
                return false;

            Normalize();
            return true;
        }

        internal bool IsDifficultyModifierSetting(string setting)
        {
            var definition = FindSetting(setting);
            if (definition == null)
                return false;

            return IsDifficultyModifierSettingDefinition(definition);
        }

        internal bool TrySetDifficultyModifier(string setting, string value, out string error)
        {
            if (!IsDifficultyModifierSetting(setting))
            {
                error = "Difficulty custom accepts: " + string.Join(", ", difficultyModifierSettings) + ".";
                return false;
            }

            if (!TrySetValue(setting, value, out error))
                return false;

            MarkCustom();
            Normalize();
            return true;
        }

        internal IEnumerable<string> GetDifficultyModifierSettingNames()
        {
            for (var i = 0; i < difficultyModifierSettings.Length; i++)
                yield return difficultyModifierSettings[i];
        }

        internal IEnumerable<string> GetDifficultyPresetNames()
        {
            return WkConfigDifficultyPresets.PresetNames;
        }

        internal void Normalize()
        {
            if (data == null)
                data = WkConfigDifficultyPresets.CreateMedium();

            data.DifficultyPreset = string.IsNullOrWhiteSpace(data.DifficultyPreset)
                ? WkConfigDifficultyPresets.CustomPreset
                : WkConfigDifficultyPresets.NormalizePresetName(data.DifficultyPreset);

            data.ResearchScale = RatioMath.Clamp(data.ResearchScale, 0.0, 100.0);
            data.ResearchGrindingGainScale = RatioMath.Clamp(data.ResearchGrindingGainScale, 0.0, 100.0);
            data.DataFragmentRewardScale = RatioMath.Clamp(data.DataFragmentRewardScale, 0.0, 100.0);
            data.DataFragmentLootScale = RatioMath.Clamp(data.DataFragmentLootScale, 0.0, 100.0);
            data.ResearchEfficiencyStart = RatioMath.Clamp(data.ResearchEfficiencyStart, 0.0, 10.0);
            data.ResearchEfficiencyEnd = RatioMath.Clamp(data.ResearchEfficiencyEnd, 0.0, 10.0);
            data.SalvageScale = RatioMath.Clamp(data.SalvageScale, 0.0, 100.0);
            data.SalvageScrapYield = RatioMath.Clamp(data.SalvageScrapYield, 0.0, 100.0);
            data.NotificationDelaySeconds = RatioMath.Clamp(data.NotificationDelaySeconds, 0.1, 30.0);
            data.ResearchChatSuppressionPercent = RatioMath.Clamp(data.ResearchChatSuppressionPercent, 0.0, 100.0);
            data.ProficiencyChatSuppressionPercent = RatioMath.Clamp(data.ProficiencyChatSuppressionPercent, 0.0, 100.0);
            data.ResearchToastSuppressionPercent = RatioMath.Clamp(data.ResearchToastSuppressionPercent, 0.0, 100.0);
            data.ProficiencyToastSuppressionPercent = RatioMath.Clamp(data.ProficiencyToastSuppressionPercent, 0.0, 100.0);
            data.FundamentalsProficiencyProgress = RatioMath.Clamp01(data.FundamentalsProficiencyProgress);
            data.ProficiencyGainScale = RatioMath.Clamp(data.ProficiencyGainScale, 0.0, 100.0);
            data.ProficiencyGrindingGainScale = RatioMath.Clamp(data.ProficiencyGrindingGainScale, 0.0, 100.0);
            data.ProficiencyWeldingGainScale = RatioMath.Clamp(data.ProficiencyWeldingGainScale, 0.0, 100.0);
            data.ProficiencyFirstThreshold = RatioMath.Clamp01(data.ProficiencyFirstThreshold);
            data.ProficiencySecondThreshold = RatioMath.Clamp(data.ProficiencySecondThreshold, data.ProficiencyFirstThreshold, 1.0);
            data.ProficiencyFirstSegmentRate = RatioMath.Clamp(data.ProficiencyFirstSegmentRate, 0.0, 10.0);
            data.ProficiencySecondSegmentRate = RatioMath.Clamp(data.ProficiencySecondSegmentRate, 0.0, 10.0);
            data.ProficiencyFinalSegmentRate = RatioMath.Clamp(data.ProficiencyFinalSegmentRate, 0.0, 10.0);
            data.WeldBotchBaseChance = RatioMath.Clamp01(data.WeldBotchBaseChance);
            data.WeldBotchMaxChance = RatioMath.Clamp01(data.WeldBotchMaxChance);
            data.WeldBotchChanceScale = RatioMath.Clamp(data.WeldBotchChanceScale, 0.0, 100.0);
            data.WeldBotchPostFunctionalPressure = RatioMath.Clamp(data.WeldBotchPostFunctionalPressure, 0.0, 100.0);
            data.WeldBotchSoftCapPressure = RatioMath.Clamp(data.WeldBotchSoftCapPressure, 0.0, 100.0);
            data.WeldBotchPressureScale = RatioMath.Clamp(data.WeldBotchPressureScale, 0.0, 100.0);
            data.WeldBotchRawLossRatio = RatioMath.Clamp(data.WeldBotchRawLossRatio, 0.0, 10.0);
            data.WeldBotchForgivenessScale = RatioMath.Clamp(data.WeldBotchForgivenessScale, 0.0, 100.0);
            data.WeldBotchWarningCooldownSeconds = RatioMath.Clamp(data.WeldBotchWarningCooldownSeconds, 0.0, 30.0);
            data.WeldBotchSoundRange = RatioMath.Clamp(data.WeldBotchSoundRange, 0.0, 1000.0);
        }

        internal IEnumerable<string> GetDisplayCategories()
        {
            var categories = new List<string>();
            for (var i = 0; i < displayCategoryOrder.Length; i++)
            {
                var category = displayCategoryOrder[i];
                if (!HasCategory(category) || categories.Contains(category))
                    continue;

                categories.Add(category);
                yield return category;
            }

            for (var i = 0; i < settings.Length; i++)
            {
                var category = settings[i].Category;
                if (categories.Contains(category))
                    continue;

                categories.Add(category);
                yield return category;
            }
        }

        internal IEnumerable<string> GetDisplayLinesForCategory(string category)
        {
            for (var i = 0; i < settings.Length; i++)
            {
                if (string.Equals(settings[i].Category, category, StringComparison.OrdinalIgnoreCase))
                    yield return settings[i].GetLine(data);
            }
        }

        internal string GetLine(string setting)
        {
            var definition = FindSetting(setting);
            return definition == null ? setting + " = unknown" : definition.GetLine(data);
        }

        internal bool IsSetting(string setting)
        {
            return FindSetting(setting) != null;
        }

        internal bool TrySetValue(string setting, string value, out string error)
        {
            var definition = FindSetting(setting);
            if (definition == null)
            {
                error = "Unknown config setting: " + setting;
                return false;
            }

            return definition.TrySetValue(data, value, out error);
        }

        internal bool TryGetSettingHelp(string setting, out List<string> lines)
        {
            lines = null;
            var definition = FindSetting(setting);
            if (definition == null)
                return false;

            lines = new List<string>();
            lines.Add(definition.Title);
            lines.Add(definition.GetLine(data));
            lines.Add("Category: " + definition.Category);
            lines.Add("Value: " + definition.ValueHint);
            lines.Add(definition.Description);
            if (definition.Aliases != null && definition.Aliases.Length > 0)
                lines.Add("Aliases: " + string.Join(", ", definition.Aliases));

            return true;
        }

        internal static string NormalizeKey(string setting)
        {
            return string.IsNullOrWhiteSpace(setting)
                ? string.Empty
                : setting.Replace("_", string.Empty).Replace("-", string.Empty).Trim().ToLowerInvariant();
        }

        private static WkConfigSettingDefinition FindSetting(string setting)
        {
            var key = NormalizeKey(setting);
            for (var i = 0; i < settings.Length; i++)
            {
                if (settings[i].Matches(key))
                    return settings[i];
            }

            return null;
        }

        private static bool IsDifficultyModifierSettingDefinition(WkConfigSettingDefinition definition)
        {
            if (definition == null)
                return false;

            var key = NormalizeKey(definition.Setting);
            for (var i = 0; i < difficultyModifierSettings.Length; i++)
            {
                if (key == NormalizeKey(difficultyModifierSettings[i]))
                    return true;
            }

            return false;
        }

        private static bool HasCategory(string category)
        {
            for (var i = 0; i < settings.Length; i++)
            {
                if (string.Equals(settings[i].Category, category, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static WkConfigSettingDefinition[] CreateSettings()
        {
            return new WkConfigSettingDefinition[]
            {
                ReadOnly("difficultyPreset", "Difficulty Preset", "Difficulty", "preset name", "Last applied difficulty preset. Manual config edits change this to custom.", delegate(WkConfig c) { return c.DifficultyPreset; }),
                Bool("salvageScrapEnabled", "Salvage Scrap", "Salvage", "Enables low-Proficiency salvage conversion from recovered components into scrap.", delegate(WkConfig c) { return c.SalvageScrapEnabled; }, delegate(WkConfig c, bool v) { c.SalvageScrapEnabled = v; }, "scrap", "salvage"),
                Bool("dataFragmentsEnabled", "Research Data Items", "Research", "Enables consumable data fragments and data schematics. Disabled items are refunded and grant no research.", delegate(WkConfig c) { return c.DataFragmentsEnabled; }, delegate(WkConfig c, bool v) { c.DataFragmentsEnabled = v; }, "fragments", "datafragments", "researchdata"),
                Number("researchScale", "Research Scale", "Research", "0.0 to 100.0", "Global multiplier for all active research gain.", delegate(WkConfig c) { return c.ResearchScale; }, delegate(WkConfig c, double v) { c.ResearchScale = v; }, "researchgain", "researchgainscale"),
                Number("researchGrindingGainScale", "Grinding Research Scale", "Research", "0.0 to 100.0", "Multiplier for schematic research gained by grinding blocks.", delegate(WkConfig c) { return c.ResearchGrindingGainScale; }, delegate(WkConfig c, double v) { c.ResearchGrindingGainScale = v; }, "researchgrinding", "grindingresearchscale"),
                Number("dataFragmentRewardScale", "Data Fragment Reward Scale", "Research", "0.0 to 100.0", "Multiplier for partial research progress granted by consumable data fragments.", delegate(WkConfig c) { return c.DataFragmentRewardScale; }, delegate(WkConfig c, double v) { c.DataFragmentRewardScale = v; }, "fragmentrewardscale", "datafragmentrewards"),
                Number("dataFragmentLootScale", "Data Fragment Loot Scale", "Research", "0.0 to 100.0", "Multiplier for data fragment loot frequency in patched container definitions. Existing spawned containers are not retroactive.", delegate(WkConfig c) { return c.DataFragmentLootScale; }, delegate(WkConfig c, double v) { c.DataFragmentLootScale = v; }, "fragmentlootscale", "datafragmentloot"),
                Number("researchEfficiencyStart", "Research Start Efficiency", "Research", "0.0 to 10.0", "Research efficiency at 0% known. Higher values front-load discovery.", delegate(WkConfig c) { return c.ResearchEfficiencyStart; }, delegate(WkConfig c, double v) { c.ResearchEfficiencyStart = v; }),
                Number("researchEfficiencyEnd", "Research End Efficiency", "Research", "0.0 to 10.0", "Research efficiency near 100% known. Lower values slow the final stretch.", delegate(WkConfig c) { return c.ResearchEfficiencyEnd; }, delegate(WkConfig c, double v) { c.ResearchEfficiencyEnd = v; }),
                Number("salvageScale", "Salvage Scale", "Salvage", "0.0 to 100.0", "Multiplier for intact component recovery before the 100% cap.", delegate(WkConfig c) { return c.SalvageScale; }, delegate(WkConfig c, double v) { c.SalvageScale = v; }, "salvagerecovery", "salvagerecoveryscale"),
                RatioOrPercent("salvageScrapYield", "Salvage Scrap Yield", "Salvage", "Mass ratio returned as scrap ore when low-Proficiency grinding converts components to scrap.", delegate(WkConfig c) { return c.SalvageScrapYield; }, delegate(WkConfig c, double v) { c.SalvageScrapYield = v; }, "scrapyield", "salvagescrapratio", "scrapratio"),
                Number("notificationDelaySeconds", "Notification Delay", "Feedback", "0.1 to 30.0 seconds", "World delay used to combine repeated progress updates before chat/toast feedback.", delegate(WkConfig c) { return c.NotificationDelaySeconds; }, delegate(WkConfig c, double v) { c.NotificationDelaySeconds = v; }, "notificationdelay"),
                Bool("defaultProgressChatEnabled", "Default Progress Chat", "Feedback", "World default for delayed progress chat messages.", delegate(WkConfig c) { return c.ProgressChatEnabled; }, delegate(WkConfig c, bool v) { c.ProgressChatEnabled = v; }, "defaultchat", "defaultchatenabled", "worldprogresschat"),
                Bool("defaultProgressToastEnabled", "Default Progress Toast", "Feedback", "World default for popup progress notifications and botch toasts. The Text HUD progress bars are separate.", delegate(WkConfig c) { return c.ProgressToastEnabled; }, delegate(WkConfig c, bool v) { c.ProgressToastEnabled = v; }, "defaulttoast", "defaulttoastenabled", "worldprogresstoast"),
                Percent("defaultResearchChatSuppressionPercent", "Default Research Chat Threshold", "Feedback", "World default/minimum accumulated research percent before another research chat update is shown.", delegate(WkConfig c) { return c.ResearchChatSuppressionPercent; }, delegate(WkConfig c, double v) { c.ResearchChatSuppressionPercent = v; }, "defaultreschatsuppression", "defaultresearchchatthreshold", "worldresearchchatthreshold"),
                Percent("defaultProficiencyChatSuppressionPercent", "Default Proficiency Chat Threshold", "Feedback", "World default/minimum accumulated Proficiency percent before another Proficiency chat update is shown.", delegate(WkConfig c) { return c.ProficiencyChatSuppressionPercent; }, delegate(WkConfig c, double v) { c.ProficiencyChatSuppressionPercent = v; }, "defaultprofchatsuppression", "defaultproficiencychatthreshold", "worldproficiencychatthreshold"),
                Percent("defaultResearchToastSuppressionPercent", "Default Research Toast Threshold", "Feedback", "World default/minimum accumulated research percent before another research toast appears.", delegate(WkConfig c) { return c.ResearchToastSuppressionPercent; }, delegate(WkConfig c, double v) { c.ResearchToastSuppressionPercent = v; }, "defaultrestoastsuppression", "defaultresearchtoastthreshold", "worldresearchtoastthreshold"),
                Percent("defaultProficiencyToastSuppressionPercent", "Default Proficiency Toast Threshold", "Feedback", "World default/minimum accumulated Proficiency percent before another Proficiency toast appears.", delegate(WkConfig c) { return c.ProficiencyToastSuppressionPercent; }, delegate(WkConfig c, double v) { c.ProficiencyToastSuppressionPercent = v; }, "defaultproftoastsuppression", "defaultproficiencytoastthreshold", "worldproficiencytoastthreshold"),
                Bool("defaultCompletionSoundEnabled", "Default Completion Sound", "Feedback", "World default for completed research and mastered Proficiency sounds.", delegate(WkConfig c) { return c.CompletionSoundEnabled; }, delegate(WkConfig c, bool v) { c.CompletionSoundEnabled = v; }),
                Bool("fundamentalsResearchUnlocked", "Fundamentals Research", "Defaults", "Grants the baseline Fundamentals schematic to new/joining players.", delegate(WkConfig c) { return c.FundamentalsResearchUnlocked; }, delegate(WkConfig c, bool v) { c.FundamentalsResearchUnlocked = v; }, "fundamentalsunlocked", "fundamentalsresearch", "fundamentalresearchunlocked", "fundamentalunlocked", "fundamentalresearch"),
                Progress("fundamentalsProficiencyProgress", "Fundamentals Proficiency", "Defaults", "Minimum baseline Proficiency for Fundamentals.", delegate(WkConfig c) { return c.FundamentalsProficiencyProgress; }, delegate(WkConfig c, double v) { c.FundamentalsProficiencyProgress = v; }, "fundamentalsproficiency", "fundamentalsprof", "fundamentalproficiencyprogress", "fundamentalproficiency", "fundamentalprof"),
                Bool("proficiencyEnabled", "Proficiency", "Proficiency", "Enables the player-only hands-on Proficiency system.", delegate(WkConfig c) { return c.ProficiencyEnabled; }, delegate(WkConfig c, bool v) { c.ProficiencyEnabled = v; }, "proficiency"),
                Number("proficiencyGainScale", "Proficiency Scale", "Proficiency", "0.0 to 100.0", "Global multiplier for hands-on Proficiency gain.", delegate(WkConfig c) { return c.ProficiencyGainScale; }, delegate(WkConfig c, double v) { c.ProficiencyGainScale = v; }, "proficiencyscale", "profgain", "profgainscale"),
                Number("proficiencyGrindingGainScale", "Grinding Proficiency Scale", "Proficiency", "0.0 to 100.0", "Multiplier for Proficiency gained by grinding.", delegate(WkConfig c) { return c.ProficiencyGrindingGainScale; }, delegate(WkConfig c, double v) { c.ProficiencyGrindingGainScale = v; }, "profgrindscale", "grindingproficiencyscale"),
                Number("proficiencyWeldingGainScale", "Welding Proficiency Scale", "Proficiency", "0.0 to 100.0", "Multiplier for Proficiency gained by welding and repairing.", delegate(WkConfig c) { return c.ProficiencyWeldingGainScale; }, delegate(WkConfig c, double v) { c.ProficiencyWeldingGainScale = v; }, "profweldscale", "weldingproficiencyscale"),
                Progress("proficiencyFirstThreshold", "Proficiency First Threshold", "Proficiency", "End of the fast first Proficiency segment.", delegate(WkConfig c) { return c.ProficiencyFirstThreshold; }, delegate(WkConfig c, double v) { c.ProficiencyFirstThreshold = v; }),
                Progress("proficiencySecondThreshold", "Proficiency Second Threshold", "Proficiency", "End of the slow second Proficiency segment. The default 100% value makes Proficiency a two-stage curve.", delegate(WkConfig c) { return c.ProficiencySecondThreshold; }, delegate(WkConfig c, double v) { c.ProficiencySecondThreshold = v; }),
                Number("proficiencyFirstSegmentRate", "Proficiency First Rate", "Proficiency", "0.0 to 10.0", "Base progress earned per table work reward unit in the fast segment before the segment curve is applied.", delegate(WkConfig c) { return c.ProficiencyFirstSegmentRate; }, delegate(WkConfig c, double v) { c.ProficiencyFirstSegmentRate = v; }),
                Number("proficiencySecondSegmentRate", "Proficiency Second Rate", "Proficiency", "0.0 to 10.0", "Base progress earned per table work reward unit in the slow segment before the segment curve is applied.", delegate(WkConfig c) { return c.ProficiencySecondSegmentRate; }, delegate(WkConfig c, double v) { c.ProficiencySecondSegmentRate = v; }),
                Number("proficiencyFinalSegmentRate", "Proficiency Final Rate", "Proficiency", "0.0 to 10.0", "Optional final segment base rate used only when the second threshold is below 100%.", delegate(WkConfig c) { return c.ProficiencyFinalSegmentRate; }, delegate(WkConfig c, double v) { c.ProficiencyFinalSegmentRate = v; }),
                Bool("proficiencyWeldingLossEnabled", "Welding Botches", "Botches", "Enables low-Proficiency construction botches while welding.", delegate(WkConfig c) { return c.ProficiencyWeldingLossEnabled; }, delegate(WkConfig c, bool v) { c.ProficiencyWeldingLossEnabled = v; }, "weldingloss"),
                Bool("proficiencyGrindingLossEnabled", "Grinding Loss", "Salvage", "Enables Proficiency-biased scrap recovery while grinding.", delegate(WkConfig c) { return c.ProficiencyGrindingLossEnabled; }, delegate(WkConfig c, bool v) { c.ProficiencyGrindingLossEnabled = v; }, "grindingloss"),
                Bool("proficiencyBuildCapEnabled", "Build Cap Pressure", "Botches", "Enables guaranteed failure at the current post-functional Proficiency cap.", delegate(WkConfig c) { return c.ProficiencyBuildCapEnabled; }, delegate(WkConfig c, bool v) { c.ProficiencyBuildCapEnabled = v; }, "buildcap"),
                Number("weldBotchBaseChance", "Botch Base Chance", "Botches", "0.0 to 1.0", "Botch chance scale near the functional threshold. The medium value maps to inverse Proficiency.", delegate(WkConfig c) { return c.WeldBotchBaseChance; }, delegate(WkConfig c, double v) { c.WeldBotchBaseChance = v; }),
                Number("weldBotchMaxChance", "Botch Max Chance", "Botches", "0.0 to 1.0", "Maximum normal botch chance before the Proficiency cap forces guaranteed failure.", delegate(WkConfig c) { return c.WeldBotchMaxChance; }, delegate(WkConfig c, double v) { c.WeldBotchMaxChance = v; }),
                Number("weldBotchChanceScale", "Botch Chance Scale", "Botches", "0.0 to 100.0", "Difficulty-facing multiplier for botch chance before the max chance clamp.", delegate(WkConfig c) { return c.WeldBotchChanceScale; }, delegate(WkConfig c, double v) { c.WeldBotchChanceScale = v; }, "botchchancescale", "botchchance"),
                Number("weldBotchPostFunctionalPressure", "Post-Functional Botch Pressure", "Botches", "0.0 to 100.0", "Shapes the post-functional botch-zone climb.", delegate(WkConfig c) { return c.WeldBotchPostFunctionalPressure; }, delegate(WkConfig c, double v) { c.WeldBotchPostFunctionalPressure = v; }),
                Number("weldBotchSoftCapPressure", "Soft-Cap Botch Pressure", "Botches", "0.0 to 100.0", "Shapes the post-functional Proficiency cap pressure.", delegate(WkConfig c) { return c.WeldBotchSoftCapPressure; }, delegate(WkConfig c, double v) { c.WeldBotchSoftCapPressure = v; }),
                Number("weldBotchPressureScale", "Botch Pressure Scale", "Botches", "0.0 to 100.0", "Difficulty-facing multiplier for post-functional botch pressure curves.", delegate(WkConfig c) { return c.WeldBotchPressureScale; }, delegate(WkConfig c, double v) { c.WeldBotchPressureScale = v; }, "botchpressurescale", "botchpressure"),
                Number("weldBotchRawLossRatio", "Botch Damage Ratio", "Botches", "0.0 to 10.0", "Raw integrity loss as a multiple of the triggering positive integrity delta.", delegate(WkConfig c) { return c.WeldBotchRawLossRatio; }, delegate(WkConfig c, double v) { c.WeldBotchRawLossRatio = v; }, "weldbotchdamagescale", "botchdamagescale", "botchdamage"),
                Number("weldBotchForgivenessScale", "Botch Forgiveness Scale", "Botches", "0.0 to 100.0", "Multiplier for returned component forgiveness after botch damage. Higher is gentler.", delegate(WkConfig c) { return c.WeldBotchForgivenessScale; }, delegate(WkConfig c, double v) { c.WeldBotchForgivenessScale = v; }, "botchforgivenessscale", "botchforgiveness"),
                Number("defaultWeldBotchWarningCooldownSeconds", "Default Botch Warning Cooldown", "Feedback", "0.0 to 30.0 seconds", "World default/minimum time before repeating the same botch warning for the same player/block.", delegate(WkConfig c) { return c.WeldBotchWarningCooldownSeconds; }, delegate(WkConfig c, double v) { c.WeldBotchWarningCooldownSeconds = v; }),
                Bool("defaultWeldBotchSoundEnabled", "Default Botch Sound", "Feedback", "World default for botch warning sounds.", delegate(WkConfig c) { return c.WeldBotchSoundEnabled; }, delegate(WkConfig c, bool v) { c.WeldBotchSoundEnabled = v; }),
                Sound("weldBotchSoundSubtype", "Botch Sound Subtype", "Feedback", "Sound subtype played for weld botches.", delegate(WkConfig c) { return c.WeldBotchSoundSubtype; }, delegate(WkConfig c, string v) { c.WeldBotchSoundSubtype = v; }),
                Number("weldBotchSoundRange", "Botch Sound Range", "Feedback", "0.0 to 1000.0 meters", "Range in meters for positional botch sounds.", delegate(WkConfig c) { return c.WeldBotchSoundRange; }, delegate(WkConfig c, double v) { c.WeldBotchSoundRange = v; }),
            };
        }

        private static WkConfigSettingDefinition ReadOnly(string setting, string title, string category, string valueHint, string description, Func<WkConfig, string> getter)
        {
            return new WkConfigSettingDefinition(setting, title, category, valueHint, description, false, getter, null, new string[0]);
        }

        private static WkConfigSettingDefinition Bool(string setting, string title, string category, string description, Func<WkConfig, bool> getter, Action<WkConfig, bool> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                "true/false",
                description,
                true,
                delegate(WkConfig config) { return getter(config).ToString(); },
                delegate(WkConfig config, string value, out string error)
                {
                    bool parsed;
                    if (!TryParseBool(value, out parsed))
                    {
                        error = "Use true/false for " + setting + ".";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkConfigSettingDefinition Number(string setting, string title, string category, string valueHint, string description, Func<WkConfig, double> getter, Action<WkConfig, double> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                valueHint,
                description,
                true,
                delegate(WkConfig config) { return FormatNumber(getter(config)); },
                delegate(WkConfig config, string value, out string error)
                {
                    double parsed;
                    if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                    {
                        error = "Use a numeric value for " + setting + ".";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkConfigSettingDefinition Percent(string setting, string title, string category, string description, Func<WkConfig, double> getter, Action<WkConfig, double> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                "percent, such as 1 or 10%",
                description,
                true,
                delegate(WkConfig config) { return FormatNumber(getter(config)); },
                delegate(WkConfig config, string value, out string error)
                {
                    double parsed;
                    if (!TryParsePercentNumber(value, out parsed))
                    {
                        error = "Use a percent value for " + setting + ", such as 0, 1, or 10%.";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkConfigSettingDefinition Progress(string setting, string title, string category, string description, Func<WkConfig, double> getter, Action<WkConfig, double> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                "progress, such as 0.8, 80, or 80%",
                description,
                true,
                delegate(WkConfig config) { return FormatNumber(getter(config)); },
                delegate(WkConfig config, string value, out string error)
                {
                    double parsed;
                    if (!TryParseProgressValue(value, out parsed))
                    {
                        error = "Use a progress value for " + setting + ", such as 0.8, 80, or 80%.";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkConfigSettingDefinition RatioOrPercent(string setting, string title, string category, string description, Func<WkConfig, double> getter, Action<WkConfig, double> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                "ratio or percent, such as 0.2 or 20%",
                description,
                true,
                delegate(WkConfig config) { return FormatNumber(getter(config)); },
                delegate(WkConfig config, string value, out string error)
                {
                    double parsed;
                    if (!TryParseRatioOrPercentValue(value, out parsed))
                    {
                        error = "Use a ratio or percent value for " + setting + ", such as 0.2 or 20%. Plain 20 means 20x.";
                        return false;
                    }

                    setter(config, parsed);
                    error = null;
                    return true;
                },
                aliases);
        }

        private static WkConfigSettingDefinition Sound(string setting, string title, string category, string description, Func<WkConfig, string> getter, Action<WkConfig, string> setter, params string[] aliases)
        {
            return new WkConfigSettingDefinition(
                setting,
                title,
                category,
                "Space Engineers audio subtype",
                description,
                true,
                delegate(WkConfig config) { return getter(config); },
                delegate(WkConfig config, string value, out string error)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        error = "Use a Space Engineers audio subtype for " + setting + ".";
                        return false;
                    }

                    setter(config, value.Trim());
                    error = null;
                    return true;
                },
                aliases);
        }

        private static bool TryParseBool(string value, out bool parsed)
        {
            if (bool.TryParse(value, out parsed))
                return true;

            if (value.Equals("1", StringComparison.OrdinalIgnoreCase) || value.Equals("on", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
                return true;
            }

            if (value.Equals("0", StringComparison.OrdinalIgnoreCase) || value.Equals("off", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
                return true;
            }

            return false;
        }

        private static bool TryParseProgressValue(string value, out double parsed)
        {
            parsed = 0.0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim();
            var isPercent = normalized.EndsWith("%", StringComparison.Ordinal);
            if (isPercent)
                normalized = normalized.Substring(0, normalized.Length - 1);

            if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return false;

            if (isPercent || parsed > 1.0)
                parsed /= 100.0;

            return true;
        }

        private static bool TryParsePercentNumber(string value, out double parsed)
        {
            parsed = 0.0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim();
            if (normalized.EndsWith("%", StringComparison.Ordinal))
                normalized = normalized.Substring(0, normalized.Length - 1);

            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool TryParseRatioOrPercentValue(string value, out double parsed)
        {
            parsed = 0.0;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var normalized = value.Trim();
            var isPercent = normalized.EndsWith("%", StringComparison.Ordinal);
            if (isPercent)
                normalized = normalized.Substring(0, normalized.Length - 1);

            if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return false;

            if (isPercent)
                parsed /= 100.0;

            return true;
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }
    }
}
