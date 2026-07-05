using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchTerminalService
    {
        private readonly double requiredProgress;

        internal ResearchTerminalService(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal List<ResearchListEntry> BuildListEntries(
            ResearchScopeRecord scope,
            ResearchPedestalView view,
            IEnumerable<ResearchUnlockTarget> catalogTargets,
            Func<string, bool> isVisible,
            Func<string, string> getDisplayName)
        {
            var recordsByResearchId = new Dictionary<string, ResearchSchematicRecord>(StringComparer.OrdinalIgnoreCase);
            if (scope != null && scope.Schematics != null)
            {
                foreach (var schematic in scope.Schematics)
                {
                    if (schematic == null || string.IsNullOrWhiteSpace(schematic.ResearchId))
                        continue;

                    if (!recordsByResearchId.ContainsKey(schematic.ResearchId))
                        recordsByResearchId.Add(schematic.ResearchId, schematic);
                }
            }

            var visibleSchematics = new List<ResearchListEntry>();
            var addedResearchIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (view == ResearchPedestalView.All && catalogTargets != null)
            {
                foreach (var target in catalogTargets)
                {
                    if (!IsVisible(isVisible, target.ResearchId))
                        continue;

                    ResearchSchematicRecord schematic;
                    recordsByResearchId.TryGetValue(target.ResearchId, out schematic);
                    AddListEntry(visibleSchematics, addedResearchIds, target.ResearchId, target.DisplayName, schematic, view);
                }
            }

            foreach (var entry in recordsByResearchId)
            {
                if (addedResearchIds.Contains(entry.Key))
                    continue;

                if (!IsVisible(isVisible, entry.Key))
                    continue;

                AddListEntry(visibleSchematics, addedResearchIds, entry.Key, GetDisplayName(getDisplayName, entry.Key), entry.Value, view);
            }

            visibleSchematics.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
            return visibleSchematics;
        }

        private void AddListEntry(List<ResearchListEntry> entries, HashSet<string> addedResearchIds, string researchId, string displayName, ResearchSchematicRecord schematic, ResearchPedestalView view)
        {
            var progress = GetSchematicProgress(schematic);
            var unlocked = schematic != null && schematic.Unlocked;
            if (!ShouldShowListEntry(progress, unlocked, view))
                return;

            entries.Add(new ResearchListEntry
            {
                ResearchId = researchId,
                DisplayName = displayName,
                Progress = progress,
                Unlocked = unlocked,
            });
            addedResearchIds.Add(researchId);
        }

        private double GetSchematicProgress(ResearchSchematicRecord schematic)
        {
            if (schematic == null)
                return 0.0;

            return schematic.Unlocked ? requiredProgress : RatioMath.Clamp01(schematic.Progress);
        }

        private bool ShouldShowListEntry(double progress, bool unlocked, ResearchPedestalView view)
        {
            progress = unlocked ? requiredProgress : RatioMath.Clamp01(progress);
            switch (view)
            {
                case ResearchPedestalView.All:
                    return true;

                case ResearchPedestalView.Completed:
                    return unlocked || progress >= requiredProgress;

                case ResearchPedestalView.Researching:
                    return progress > 0.0 && progress < requiredProgress;

                default:
                    return progress > 0.0;
            }
        }

        private static bool IsVisible(Func<string, bool> isVisible, string researchId)
        {
            return isVisible == null || isVisible(researchId);
        }

        private static string GetDisplayName(Func<string, string> getDisplayName, string researchId)
        {
            return getDisplayName == null ? researchId : getDisplayName(researchId);
        }
    }
}
