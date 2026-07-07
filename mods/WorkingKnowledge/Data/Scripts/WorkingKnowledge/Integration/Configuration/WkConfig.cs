namespace WkKn
{
    // World-storage DTO for WkKnConfig.xml. Early development can break this shape.
    public class WkConfig
    {
        // Label for the last applied difficulty preset. Manual config edits mark this as custom.
        public string DifficultyPreset = "medium";

        // Enables Proficiency-based conversion of low-skill grind recovery into scrap.
        public bool SalvageScrapEnabled = true;

        // Enables consumable research data items. Disabled items are refunded on use.
        public bool DataFragmentsEnabled = true;

        // Global multiplier for research gain from all active research sources.
        public double ResearchScale = 1.0;

        // Multiplier for schematic research gained by grinding blocks.
        public double ResearchGrindingGainScale = 1.0;

        // Multiplier for partial research progress granted by consumable data fragments.
        public double DataFragmentRewardScale = 1.0;

        // Multiplier for data fragment loot frequency in patched container definitions.
        public double DataFragmentLootScale = 1.0;

        // Research efficiency at 0% known. Values above 1 front-load early discovery.
        public double ResearchEfficiencyStart = 1.0;

        // Research efficiency near 100% known. Lower values slow down final confirmation.
        public double ResearchEfficiencyEnd = 0.50;

        // Semicolon-separated block-to-schematic mappings for modded or admin-overridden blocks.
        public string ModBlockSchematicMappings = "";

        // Multiplier for intact component recovery while grinding with incomplete Proficiency.
        public double SalvageScale = 1.0;

        // Mass ratio returned as scrap ore when low-Proficiency grinding converts components to scrap.
        public double SalvageScrapYield = 0.20;

        // Delay used to combine repeated progress updates before chat/toast feedback.
        public double NotificationDelaySeconds = 2.0;

        // Enables delayed progress chat messages.
        public bool ProgressChatEnabled = true;

        // Enables delayed HUD notifications and completion toasts.
        public bool ProgressToastEnabled = true;

        // Minimum accumulated research percent before another research chat update is shown.
        public double ResearchChatSuppressionPercent = 0.0;

        // Minimum accumulated Proficiency percent before another Proficiency chat update is shown.
        public double ProficiencyChatSuppressionPercent = 0.0;

        // Minimum accumulated research percent before another research toast update is shown.
        public double ResearchToastSuppressionPercent = 0.0;

        // Minimum accumulated Proficiency percent before another Proficiency toast update is shown.
        public double ProficiencyToastSuppressionPercent = 0.0;

        // Enables completion sounds for completed research and mastered Proficiency.
        public bool CompletionSoundEnabled = true;

        // Grants the baseline Fundamentals schematic to new/joining players.
        public bool FundamentalsResearchUnlocked = true;

        // Minimum baseline Proficiency for Fundamentals, from 0.0 to 1.0.
        public double FundamentalsProficiencyProgress = 0.80;

        // Enables the player-only hands-on Proficiency system.
        public bool ProficiencyEnabled = true;

        // Global multiplier for hands-on Proficiency gain.
        public double ProficiencyGainScale = 1.0;

        // Multiplier for Proficiency gained by grinding.
        public double ProficiencyGrindingGainScale = 1.0;

        // Multiplier for Proficiency gained by welding and repairing.
        public double ProficiencyWeldingGainScale = 1.0;

        // First Proficiency curve breakpoint. Progress below this uses the fast first-segment rate.
        public double ProficiencyFirstThreshold = 0.70;

        // Second Proficiency curve breakpoint. Defaults to 100% for a two-stage curve.
        public double ProficiencySecondThreshold = 1.0;

        // Progress earned per table work reward unit in the first Proficiency segment.
        public double ProficiencyFirstSegmentRate = 0.56;

        // Progress earned per table work reward unit in the slow Proficiency segment.
        public double ProficiencySecondSegmentRate = 0.018;

        // Progress earned per table work reward unit after the second threshold, if configured below 100%.
        public double ProficiencyFinalSegmentRate = 0.018;

        // Enables low-Proficiency construction botches while welding.
        public bool ProficiencyWeldingLossEnabled = true;

        // Enables Proficiency-biased scrap recovery while grinding.
        public bool ProficiencyGrindingLossEnabled = true;

        // Enables the post-functional Proficiency cap inside the botch zone.
        public bool ProficiencyBuildCapEnabled = true;

        // Botch chance scale near the functional threshold. The medium value maps to inverse Proficiency.
        public double WeldBotchBaseChance = 0.50;

        // Maximum botch chance after all pressure curves.
        public double WeldBotchMaxChance = 0.95;

        // Difficulty-facing multiplier for botch chance before the max chance clamp.
        public double WeldBotchChanceScale = 1.0;

        // Shapes the post-functional botch-zone climb.
        public double WeldBotchPostFunctionalPressure = 4.0;

        // Shapes the post-functional Proficiency cap pressure.
        public double WeldBotchSoftCapPressure = 3.0;

        // Difficulty-facing multiplier for post-functional botch pressure curves.
        public double WeldBotchPressureScale = 1.0;

        // Raw integrity loss as a multiple of the triggering positive integrity delta.
        public double WeldBotchRawLossRatio = 1.0;

        // Multiplier for returned component forgiveness after botch damage. Higher is gentler.
        public double WeldBotchForgivenessScale = 1.0;

        // Minimum seconds before repeating the same botch warning for the same player/block.
        public double WeldBotchWarningCooldownSeconds = 0.0;

        // Enables the botch warning sound.
        public bool WeldBotchSoundEnabled = true;

        // Sound subtype played for weld botches.
        public string WeldBotchSoundSubtype = "ArcPoofExplosionCat1";

        // Range in meters for positional botch sounds.
        public double WeldBotchSoundRange = 75.0;
    }
}
