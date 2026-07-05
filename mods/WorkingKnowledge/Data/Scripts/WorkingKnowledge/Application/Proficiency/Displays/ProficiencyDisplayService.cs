using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ProficiencyDisplayService
    {
        private readonly double requiredProgress;

        internal ProficiencyDisplayService(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal ProficiencyDisplaySnapshot BuildSnapshot(
            string playerName,
            long updatedTick,
            bool proficiencyEnabled,
            ProficiencyScopeRecord scope,
            Func<string, string> getDisplayName)
        {
            var snapshot = new ProficiencyDisplaySnapshot
            {
                PlayerName = string.IsNullOrWhiteSpace(playerName) ? "Local Player" : playerName,
                UpdatedTick = updatedTick,
                Entries = new List<ProficiencyDisplayEntry>(),
            };

            if (!proficiencyEnabled)
            {
                snapshot.Message = "Proficiency is disabled.";
                return snapshot;
            }

            if (scope == null || scope.Proficiencies == null || scope.Proficiencies.Count == 0)
            {
                snapshot.Message = "No Proficiency recorded yet.";
                return snapshot;
            }

            foreach (var proficiency in scope.Proficiencies)
            {
                if (proficiency == null || string.IsNullOrWhiteSpace(proficiency.ProficiencyId))
                    continue;

                var progress = RatioMath.Clamp01(proficiency.Progress);
                snapshot.TrackedCount++;
                if (progress >= requiredProgress)
                    snapshot.MasteredCount++;

                if (progress <= 0.0 || progress >= requiredProgress)
                    continue;

                snapshot.Entries.Add(new ProficiencyDisplayEntry
                {
                    ProficiencyId = proficiency.ProficiencyId,
                    DisplayName = GetDisplayName(getDisplayName, proficiency.ProficiencyId),
                    Progress = progress,
                });
            }

            snapshot.Entries.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (snapshot.Entries.Count == 0)
                snapshot.Message = snapshot.MasteredCount > 0 ? "All tracked Proficiency is mastered." : "No active Proficiency in progress.";

            return snapshot;
        }

        internal List<ProficiencyRecord> CopyRecords(ProficiencyScopeRecord scope)
        {
            var records = new List<ProficiencyRecord>();
            if (scope == null || scope.Proficiencies == null)
                return records;

            foreach (var proficiency in scope.Proficiencies)
            {
                if (proficiency == null)
                    continue;

                records.Add(new ProficiencyRecord
                {
                    ProficiencyId = proficiency.ProficiencyId,
                    Progress = proficiency.Progress,
                    LastTouchedTick = proficiency.LastTouchedTick,
                });
            }

            return records;
        }

        private static string GetDisplayName(Func<string, string> getDisplayName, string proficiencyId)
        {
            return getDisplayName == null ? proficiencyId : getDisplayName(proficiencyId);
        }
    }
}
