using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ProficiencySummaryService
    {
        private readonly double requiredProgress;

        internal ProficiencySummaryService(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal List<string> BuildPersonalSummary(ProficiencyScopeRecord playerScope, Func<string, string> getDisplayName)
        {
            var lines = new List<string>();
            if (playerScope == null || playerScope.Proficiencies == null || playerScope.Proficiencies.Count == 0)
            {
                lines.Add("No personal Proficiency recorded yet.");
                return lines;
            }

            var entries = new List<ProficiencyRecord>(playerScope.Proficiencies);
            entries.Sort((left, right) => string.Compare(left.ProficiencyId, right.ProficiencyId, StringComparison.OrdinalIgnoreCase));

            var mastered = 0;
            foreach (var proficiency in entries)
            {
                if (proficiency.Progress >= requiredProgress)
                    mastered++;
            }

            lines.Add("Personal Proficiency: " + entries.Count + " tracked, " + mastered + " mastered.");

            foreach (var proficiency in entries)
                lines.Add(GetDisplayName(getDisplayName, proficiency.ProficiencyId) + ": " + FormatProgress(proficiency.Progress) + " proficiency");

            return lines;
        }

        private static string FormatProgress(double progress)
        {
            return Math.Round(RatioMath.Clamp01(progress) * 100.0, 2) + "%";
        }

        private static string GetDisplayName(Func<string, string> getDisplayName, string proficiencyId)
        {
            return getDisplayName == null ? proficiencyId : getDisplayName(proficiencyId);
        }
    }
}
