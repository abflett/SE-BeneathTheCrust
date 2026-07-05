using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchService
    {
        private readonly ResearchProgressTuning tuning;

        internal ResearchService(ResearchProgressTuning tuning)
        {
            this.tuning = tuning;
        }

        internal double GetPlayerProgress(ResearchStore store, long identityId, string researchId)
        {
            if (store == null || identityId == 0 || string.IsNullOrWhiteSpace(researchId))
                return 0.0;

            var scope = store.FindScope(store.PlayerScopes, identityId.ToString());
            if (scope == null || scope.Schematics == null)
                return 0.0;

            foreach (var schematic in scope.Schematics)
            {
                if (!string.Equals(schematic.ResearchId, researchId, StringComparison.OrdinalIgnoreCase))
                    continue;

                return schematic.Unlocked ? tuning.RequiredProgress : Clamp01(schematic.Progress);
            }

            return 0.0;
        }

        internal double GenerateGrindProgress(
            double workScale,
            double fullWorkReward,
            double configuredResearchScale,
            double efficiencyStart,
            double efficiencyEnd,
            double currentProgress)
        {
            var progress = Clamp01(workScale) * Math.Max(0.0, fullWorkReward) * GetResearchEfficiency(currentProgress, efficiencyStart, efficiencyEnd) * Math.Max(0.0, configuredResearchScale);
            if (progress <= 0.0)
                return 0.0;

            return Math.Max(tuning.ProgressPrecision, progress);
        }

        internal ResearchProgressResult RecordProgress(ResearchStore store, List<ResearchScopeRecord> scopes, string scopeId, string researchId, string unlockerSubtype, double progress)
        {
            var result = RecordProgressCore(store, scopes, scopeId, researchId, unlockerSubtype, progress);
            if (result.Changed)
                store.MarkDirty();

            return result;
        }

        internal void AddCompletedUnlockers(ResearchStore store, List<ResearchScopeRecord> scopes, string scopeId, HashSet<string> completedUnlockers)
        {
            if (store == null || completedUnlockers == null)
                return;

            var scope = store.FindScope(scopes, scopeId);
            if (scope == null || scope.Schematics == null)
                return;

            foreach (var schematic in scope.Schematics)
            {
                if (!schematic.Unlocked || string.IsNullOrWhiteSpace(schematic.UnlockerSubtype))
                    continue;

                completedUnlockers.Add(schematic.UnlockerSubtype);
            }
        }

        private ResearchProgressResult RecordProgressCore(ResearchStore store, List<ResearchScopeRecord> scopes, string scopeId, string researchId, string unlockerSubtype, double progress)
        {
            var scope = store.GetOrCreateScope(scopes, scopeId);
            var schematic = store.GetOrCreateSchematic(scope, researchId);
            if (schematic.Unlocked && string.Equals(schematic.UnlockerSubtype, unlockerSubtype, StringComparison.OrdinalIgnoreCase))
                return ResearchProgressResult.NoChange;

            var result = new ResearchProgressResult();
            if (string.IsNullOrWhiteSpace(schematic.ActiveToken))
                schematic.ActiveToken = Guid.NewGuid().ToString("N");

            if (progress > 0.0)
            {
                var acceptedProgress = Math.Min(progress, tuning.RequiredProgress - schematic.Progress);
                if (acceptedProgress <= 0.0)
                    acceptedProgress = 0.0;

                schematic.Progress += acceptedProgress;
                schematic.ActiveProgress += acceptedProgress;
                result.AddedProgress = acceptedProgress;

                if (acceptedProgress > 0.0)
                    result.Changed = true;
            }

            if (schematic.Progress >= tuning.RequiredProgress)
            {
                schematic.Progress = tuning.RequiredProgress;
                schematic.Unlocked = true;
                schematic.ActiveToken = null;
                schematic.ActiveProgress = 0.0;
                schematic.Ledger.Clear();
                result.Changed = true;
                result.Unlocked = true;
            }

            if (!string.Equals(schematic.UnlockerSubtype, unlockerSubtype, StringComparison.OrdinalIgnoreCase))
            {
                schematic.UnlockerSubtype = unlockerSubtype;
                result.Changed = true;
            }

            result.TotalProgress = schematic.Progress;
            return result;
        }

        private static double GetResearchEfficiency(double currentProgress, double efficiencyStart, double efficiencyEnd)
        {
            var progress = Clamp01(currentProgress);
            return efficiencyStart - ((efficiencyStart - efficiencyEnd) * progress * progress);
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
