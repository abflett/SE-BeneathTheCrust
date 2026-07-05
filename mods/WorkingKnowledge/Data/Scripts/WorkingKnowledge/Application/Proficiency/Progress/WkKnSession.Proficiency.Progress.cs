namespace WkKn
{
    public partial class WkKnSession
    {
        private double GetPlayerProficiency(long identityId, string researchId)
        {
            return proficiencyService.GetPlayerProgress(proficiencyStore, identityId, researchId);
        }

        private ProficiencyProgressResult AwardProficiencyFromHandsOnWork(long identityId, string researchId, double workScale, string source)
        {
            var result = proficiencyService.AwardHandsOnWork(
                proficiencyStore,
                identityId,
                researchId,
                workScale,
                source,
                config.ProficiencyEnabled,
                GetProficiencyGainScaleForSource(source),
                GetActiveProficiencyProgressTuning(),
                simulationTick);
            if (result.Changed)
            {
                NotifyProficiencyDisplayChanged(identityId);
                QueueProficiencyNotification(identityId, researchId, result, source);

                var previousProgress = result.TotalProgress - result.AddedProgress;
                if (!string.Equals(researchId, FundamentalsResearchId, System.StringComparison.OrdinalIgnoreCase) &&
                    previousProgress < RequiredResearchProgress &&
                    result.TotalProgress >= RequiredResearchProgress)
                    ShowWkProficiencyCompletionFeedback(identityId, GetSchematicDisplayName(researchId));
            }

            return result;
        }

        private ProficiencyProgressTuning GetActiveProficiencyProgressTuning()
        {
            return new ProficiencyProgressTuning(
                config.ProficiencyFirstThreshold,
                config.ProficiencySecondThreshold,
                config.ProficiencyFirstSegmentRate,
                config.ProficiencySecondSegmentRate,
                config.ProficiencyFinalSegmentRate,
                RequiredResearchProgress);
        }

        private double GetProficiencyGainScaleForSource(string source)
        {
            var scale = config.ProficiencyGainScale;
            if (source != null && source.Equals("grinding", System.StringComparison.OrdinalIgnoreCase))
                scale *= config.ProficiencyGrindingGainScale;
            else if (source != null && source.Equals("welding", System.StringComparison.OrdinalIgnoreCase))
                scale *= config.ProficiencyWeldingGainScale;

            return scale;
        }

    }
}
