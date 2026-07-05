using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchSummaryService
    {
        internal List<string> BuildPersonalSummary(ResearchScopeRecord playerScope, Func<string, string> getDisplayName)
        {
            var lines = new List<string>();
            if (playerScope == null || playerScope.Schematics == null || playerScope.Schematics.Count == 0)
            {
                lines.Add("No personal research recorded yet.");
                return lines;
            }

            var entries = new List<ResearchSchematicRecord>(playerScope.Schematics);
            entries.Sort((left, right) => string.Compare(left.ResearchId, right.ResearchId, StringComparison.OrdinalIgnoreCase));

            var unlocked = 0;
            foreach (var schematic in entries)
            {
                if (schematic.Unlocked)
                    unlocked++;
            }

            lines.Add("Personal schematics: " + entries.Count + " tracked, " + unlocked + " unlocked.");

            foreach (var schematic in entries)
                lines.Add(FormatLine(schematic, getDisplayName));

            return lines;
        }

        private static string FormatLine(ResearchSchematicRecord schematic, Func<string, string> getDisplayName)
        {
            return GetDisplayName(getDisplayName, schematic.ResearchId) + ": " + FormatProgress(schematic.Progress) + (schematic.Unlocked ? " unlocked" : " locked");
        }

        private static string FormatProgress(double progress)
        {
            return Math.Round(RatioMath.Clamp01(progress) * 100.0, 2) + "%";
        }

        private static string GetDisplayName(Func<string, string> getDisplayName, string researchId)
        {
            return getDisplayName == null ? researchId : getDisplayName(researchId);
        }
    }
}
