using System;
using System.Collections.Generic;
using VRage.Game;

namespace WkKn
{
    internal sealed class SchematicCatalog
    {
        private readonly Dictionary<MyDefinitionId, ResearchUnlockTarget> targetByBlock = new Dictionary<MyDefinitionId, ResearchUnlockTarget>(MyDefinitionId.Comparer);
        private readonly Dictionary<string, ResearchUnlockTarget> targetByResearchId = new Dictionary<string, ResearchUnlockTarget>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<MyDefinitionId>> blockIdsByResearchId = new Dictionary<string, List<MyDefinitionId>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> displayNameByResearchId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        internal IEnumerable<ResearchUnlockTarget> Targets
        {
            get { return targetByResearchId.Values; }
        }

        internal void Clear()
        {
            targetByBlock.Clear();
            targetByResearchId.Clear();
            blockIdsByResearchId.Clear();
            displayNameByResearchId.Clear();
        }

        internal void LoadMetadata(IEnumerable<ResearchCatalogEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (!displayNameByResearchId.ContainsKey(entry.ResearchId))
                    displayNameByResearchId.Add(entry.ResearchId, entry.DisplayName);
            }
        }

        internal void AddMappedBlock(ResearchCatalogEntry entry, MyDefinitionId blockId, MyDefinitionId unlockerId)
        {
            var target = new ResearchUnlockTarget(entry.ResearchId, entry.DisplayName, unlockerId, entry.Tier);
            targetByBlock[blockId] = target;
            AddBlockId(entry.ResearchId, blockId);

            if (!targetByResearchId.ContainsKey(entry.ResearchId))
                targetByResearchId.Add(entry.ResearchId, target);
        }

        internal bool TryGetTargetByBlock(MyDefinitionId blockId, out ResearchUnlockTarget target)
        {
            return targetByBlock.TryGetValue(blockId, out target);
        }

        internal bool TryGetTargetByResearchId(string researchId, out ResearchUnlockTarget target)
        {
            return targetByResearchId.TryGetValue(researchId, out target);
        }

        internal bool TryGetBlockIds(string researchId, out List<MyDefinitionId> blockIds)
        {
            return blockIdsByResearchId.TryGetValue(researchId, out blockIds);
        }

        internal string GetDisplayName(string researchId)
        {
            if (string.IsNullOrWhiteSpace(researchId))
                return "Unknown Schematics";

            string displayName;
            if (displayNameByResearchId.TryGetValue(researchId, out displayName))
                return displayName;

            return researchId;
        }

        private void AddBlockId(string researchId, MyDefinitionId blockId)
        {
            if (string.IsNullOrWhiteSpace(researchId))
                return;

            List<MyDefinitionId> blockIds;
            if (!blockIdsByResearchId.TryGetValue(researchId, out blockIds))
            {
                blockIds = new List<MyDefinitionId>();
                blockIdsByResearchId.Add(researchId, blockIds);
            }

            if (!blockIds.Contains(blockId))
                blockIds.Add(blockId);
        }
    }
}
