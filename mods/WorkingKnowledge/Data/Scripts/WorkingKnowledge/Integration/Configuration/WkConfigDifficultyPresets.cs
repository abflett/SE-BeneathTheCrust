using System;
using System.Collections.Generic;

namespace WkKn
{
    internal static class WkConfigDifficultyPresets
    {
        internal const string CustomPreset = "custom";
        internal const string MediumPreset = "medium";

        private static readonly string[] presetNames = new string[]
        {
            "novice",
            "easy",
            MediumPreset,
            "hard",
            "extreme",
        };

        internal static IEnumerable<string> PresetNames
        {
            get { return presetNames; }
        }

        internal static WkConfig CreateMedium()
        {
            var config = new WkConfig();
            config.DifficultyPreset = MediumPreset;
            return config;
        }

        internal static bool TryApply(string presetName, WkConfig config, out string error)
        {
            error = null;

            if (config == null)
            {
                error = "Config is not available.";
                return false;
            }

            var preset = NormalizePresetName(presetName);
            if (preset == CustomPreset || preset.Length == 0)
            {
                error = "Use a difficulty preset: novice, easy, medium, hard, or extreme.";
                return false;
            }

            var modifiers = GetModifiers(preset);
            if (modifiers == null)
            {
                error = "Unknown difficulty preset: " + presetName;
                return false;
            }

            ApplyModifiers(config, modifiers);
            config.DifficultyPreset = preset;
            return true;
        }

        internal static string NormalizePresetName(string presetName)
        {
            return string.IsNullOrWhiteSpace(presetName)
                ? string.Empty
                : presetName.Trim().ToLowerInvariant();
        }

        private static WkDifficultyModifiers GetModifiers(string preset)
        {
            if (preset == MediumPreset)
                return new WkDifficultyModifiers();

            if (preset == "novice")
            {
                return new WkDifficultyModifiers
                {
                    ResearchGain = 2.00,
                    ProficiencyGain = 2.00,
                    DataFragmentReward = 2.00,
                    DataFragmentLoot = 2.00,
                    SalvageRecovery = 2.00,
                    BotchChance = 0.50,
                    BotchDamage = 0.50,
                    BotchPressure = 0.50,
                    BotchForgiveness = 2.00,
                };
            }

            if (preset == "easy")
            {
                return new WkDifficultyModifiers
                {
                    ResearchGain = 1.50,
                    ProficiencyGain = 1.50,
                    DataFragmentReward = 1.50,
                    DataFragmentLoot = 1.50,
                    SalvageRecovery = 1.50,
                    BotchChance = 0.75,
                    BotchDamage = 0.75,
                    BotchPressure = 0.75,
                    BotchForgiveness = 1.50,
                };
            }

            if (preset == "hard")
            {
                return new WkDifficultyModifiers
                {
                    ResearchGain = 0.50,
                    ProficiencyGain = 0.50,
                    DataFragmentReward = 0.50,
                    DataFragmentLoot = 0.50,
                    SalvageRecovery = 0.75,
                    BotchChance = 1.50,
                    BotchDamage = 1.50,
                    BotchPressure = 1.50,
                    BotchForgiveness = 0.75,
                };
            }

            if (preset == "extreme")
            {
                return new WkDifficultyModifiers
                {
                    ResearchGain = 0.25,
                    ProficiencyGain = 0.25,
                    DataFragmentReward = 0.25,
                    DataFragmentLoot = 0.25,
                    SalvageRecovery = 0.50,
                    BotchChance = 2.00,
                    BotchDamage = 2.00,
                    BotchPressure = 2.00,
                    BotchForgiveness = 0.50,
                };
            }

            return null;
        }

        private static void ApplyModifiers(WkConfig config, WkDifficultyModifiers modifiers)
        {
            config.ResearchScale = modifiers.ResearchGain;
            config.DataFragmentRewardScale = modifiers.DataFragmentReward;
            config.DataFragmentLootScale = modifiers.DataFragmentLoot;
            config.ProficiencyGainScale = modifiers.ProficiencyGain;
            config.SalvageScale = modifiers.SalvageRecovery;
            config.WeldBotchChanceScale = modifiers.BotchChance;
            config.WeldBotchRawLossRatio = modifiers.BotchDamage;
            config.WeldBotchPressureScale = modifiers.BotchPressure;
            config.WeldBotchForgivenessScale = modifiers.BotchForgiveness;
        }

        private sealed class WkDifficultyModifiers
        {
            internal double ResearchGain = 1.0;
            internal double DataFragmentReward = 1.0;
            internal double DataFragmentLoot = 1.0;
            internal double ProficiencyGain = 1.0;
            internal double SalvageRecovery = 1.0;
            internal double BotchChance = 1.0;
            internal double BotchDamage = 1.0;
            internal double BotchPressure = 1.0;
            internal double BotchForgiveness = 1.0;
        }
    }
}
