using System;

namespace WkKn
{
    internal sealed class ProficiencyService
    {
        private const double FirstSegmentEfficiencyStart = 1.20;
        private const double FirstSegmentEfficiencyEnd = 0.80;
        private const double SecondSegmentEfficiencyStart = 1.15;
        private const double SecondSegmentEfficiencyEnd = 0.50;
        private const double FinalSegmentEfficiencyStart = 0.50;
        private const double FinalSegmentEfficiencyEnd = 0.25;

        private readonly double requiredProgress;

        internal ProficiencyService(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal double GetPlayerProgress(ProficiencyStore store, long identityId, string researchId)
        {
            if (store == null || identityId == 0 || string.IsNullOrWhiteSpace(researchId))
                return 0.0;

            var scope = store.FindScope(identityId.ToString());
            if (scope == null || scope.Proficiencies == null)
                return 0.0;

            var proficiencyId = GetProficiencyIdForResearchId(researchId);
            foreach (var proficiency in scope.Proficiencies)
            {
                if (string.Equals(proficiency.ProficiencyId, proficiencyId, StringComparison.OrdinalIgnoreCase))
                    return Clamp01(proficiency.Progress);
            }

            return 0.0;
        }

        internal ProficiencyProgressResult RecordProgress(ProficiencyStore store, long identityId, string researchId, double progress, long currentTick)
        {
            if (store == null || identityId == 0 || string.IsNullOrWhiteSpace(researchId) || progress <= 0.0)
                return ProficiencyProgressResult.NoChange;

            var scope = store.GetOrCreateScope(identityId.ToString());
            var proficiency = store.GetOrCreateProficiency(scope, GetProficiencyIdForResearchId(researchId), currentTick);
            var acceptedProgress = Math.Min(progress, requiredProgress - proficiency.Progress);
            if (acceptedProgress <= 0.0)
                return ProficiencyProgressResult.NoChange;

            proficiency.Progress += acceptedProgress;
            proficiency.LastTouchedTick = currentTick;
            store.MarkDirty();

            return new ProficiencyProgressResult
            {
                Changed = true,
                AddedProgress = acceptedProgress,
                TotalProgress = proficiency.Progress,
            };
        }

        internal ProficiencyProgressResult EnsureMinimumProgress(ProficiencyStore store, long identityId, string researchId, double minimumProgress, long currentTick)
        {
            if (store == null || identityId == 0 || string.IsNullOrWhiteSpace(researchId))
                return ProficiencyProgressResult.NoChange;

            var targetProgress = Clamp01(minimumProgress);
            if (targetProgress <= 0.0)
                return ProficiencyProgressResult.NoChange;

            var scope = store.GetOrCreateScope(identityId.ToString());
            var proficiency = store.GetOrCreateProficiency(scope, GetProficiencyIdForResearchId(researchId), currentTick);
            if (proficiency.Progress >= targetProgress)
                return ProficiencyProgressResult.NoChange;

            var addedProgress = targetProgress - Math.Max(0.0, proficiency.Progress);
            proficiency.Progress = targetProgress;
            proficiency.LastTouchedTick = currentTick;
            store.MarkDirty();

            return new ProficiencyProgressResult
            {
                Changed = true,
                AddedProgress = addedProgress,
                TotalProgress = proficiency.Progress,
            };
        }

        internal ProficiencyProgressResult AwardHandsOnWork(
            ProficiencyStore store,
            long identityId,
            string researchId,
            double workScale,
            string source,
            bool proficiencyEnabled,
            double gainScale,
            ProficiencyProgressTuning activeTuning,
            long currentTick)
        {
            if (!ShouldAwardForSource(proficiencyEnabled, source))
                return ProficiencyProgressResult.NoChange;

            var currentProgress = GetPlayerProgress(store, identityId, researchId);
            var progress = GenerateProgress(workScale, gainScale, currentProgress, activeTuning);
            return RecordProgress(store, identityId, researchId, progress, currentTick);
        }

        internal double GenerateProgress(double workScale, double gainScale, double currentProgress, ProficiencyProgressTuning activeTuning)
        {
            var remainingWork = Math.Max(0.0, workScale) * gainScale;
            if (remainingWork <= 0.0)
                return 0.0;

            var progress = Clamp01(currentProgress);
            while (remainingWork > 0.0 && progress < activeTuning.RequiredProgress)
            {
                double segmentStart;
                double segmentLimit;
                double segmentRate;
                double efficiencyStart;
                double efficiencyEnd;
                if (progress < activeTuning.FirstThreshold)
                {
                    segmentStart = 0.0;
                    segmentLimit = activeTuning.FirstThreshold;
                    segmentRate = activeTuning.FirstSegmentRate;
                    efficiencyStart = FirstSegmentEfficiencyStart;
                    efficiencyEnd = FirstSegmentEfficiencyEnd;
                }
                else if (progress < activeTuning.SecondThreshold)
                {
                    segmentStart = activeTuning.FirstThreshold;
                    segmentLimit = activeTuning.SecondThreshold;
                    segmentRate = activeTuning.SecondSegmentRate;
                    efficiencyStart = SecondSegmentEfficiencyStart;
                    efficiencyEnd = SecondSegmentEfficiencyEnd;
                }
                else
                {
                    segmentStart = activeTuning.SecondThreshold;
                    segmentLimit = activeTuning.RequiredProgress;
                    segmentRate = activeTuning.FinalSegmentRate;
                    efficiencyStart = FinalSegmentEfficiencyStart;
                    efficiencyEnd = FinalSegmentEfficiencyEnd;
                }

                var progressRemainingInSegment = segmentLimit - progress;
                var effectiveSegmentRate = segmentRate * GetSegmentEfficiency(progress, segmentStart, segmentLimit, efficiencyStart, efficiencyEnd);
                if (progressRemainingInSegment <= 0.0 || effectiveSegmentRate <= 0.0)
                    break;

                var workToFinishSegment = progressRemainingInSegment / effectiveSegmentRate;
                var workUsed = Math.Min(remainingWork, workToFinishSegment);
                progress += workUsed * effectiveSegmentRate;
                remainingWork -= workUsed;
            }

            return Math.Max(0.0, Clamp01(progress) - Clamp01(currentProgress));
        }

        internal static string GetProficiencyIdForResearchId(string researchId)
        {
            return string.IsNullOrWhiteSpace(researchId) ? string.Empty : researchId;
        }

        private static bool ShouldAwardForSource(bool proficiencyEnabled, string source)
        {
            return proficiencyEnabled &&
                   source != null &&
                   (source.Equals("grinding", StringComparison.OrdinalIgnoreCase) ||
                    source.Equals("welding", StringComparison.OrdinalIgnoreCase));
        }

        private static double GetSegmentEfficiency(double progress, double segmentStart, double segmentLimit, double efficiencyStart, double efficiencyEnd)
        {
            if (segmentLimit <= segmentStart)
                return Math.Max(0.0, efficiencyEnd);

            var localProgress = Clamp01((progress - segmentStart) / (segmentLimit - segmentStart));
            return Math.Max(0.0, efficiencyStart - ((efficiencyStart - efficiencyEnd) * localProgress));
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0)
                return 0.0;
            if (value > 1.0)
                return 1.0;
            return value;
        }
    }
}
