using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class WorkingKnowledgeLayerMapping
    {
        internal readonly ResearchCatalogEntry Entry;
        internal readonly string ModName;
        internal readonly int LineNumber;

        internal WorkingKnowledgeLayerMapping(ResearchCatalogEntry entry, string modName, int lineNumber)
        {
            Entry = entry;
            ModName = modName;
            LineNumber = lineNumber;
        }

        internal string Source
        {
            get { return ModName + " line " + LineNumber; }
        }
    }

    internal sealed class WorkingKnowledgeLayerAudit
    {
        private readonly Dictionary<string, WorkingKnowledgeLayerMapping> mappings =
            new Dictionary<string, WorkingKnowledgeLayerMapping>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> issues = new List<string>();
        private readonly HashSet<string> ignoredMappingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal IDictionary<string, WorkingKnowledgeLayerMapping> Mappings
        {
            get { return mappings; }
        }

        internal IList<string> Issues
        {
            get { return issues; }
        }

        internal int LayerCount { get; set; }

        internal int ActiveMappingCount { get; set; }

        internal void IgnoreMapping(string blockKey)
        {
            ignoredMappingKeys.Add(blockKey);
        }

        internal bool IsMappingEligible(string blockKey)
        {
            return !ignoredMappingKeys.Contains(blockKey);
        }

        internal void AddIssue(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                issues.Add(message);
        }
    }
}
