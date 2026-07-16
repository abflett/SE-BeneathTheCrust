using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class WorkingKnowledgeLayerMove
    {
        internal readonly string BlockKey;
        internal readonly string FromResearchId;
        internal readonly string FromDisplayName;
        internal readonly string ToResearchId;
        internal readonly string ToDisplayName;
        internal readonly string Source;

        internal WorkingKnowledgeLayerMove(
            string blockKey,
            string fromResearchId,
            string fromDisplayName,
            string toResearchId,
            string toDisplayName,
            string source)
        {
            BlockKey = blockKey;
            FromResearchId = fromResearchId;
            FromDisplayName = fromDisplayName;
            ToResearchId = toResearchId;
            ToDisplayName = toDisplayName;
            Source = source;
        }
    }

    internal sealed class WorkingKnowledgeLayerMapping
    {
        internal readonly ResearchCatalogEntry Entry;
        internal readonly string ModName;
        internal readonly int LineNumber;
        internal readonly int LoadIndex;
        internal readonly bool IsOverride;

        internal WorkingKnowledgeLayerMapping(
            ResearchCatalogEntry entry,
            string modName,
            int lineNumber,
            int loadIndex,
            bool isOverride)
        {
            Entry = entry;
            ModName = modName;
            LineNumber = lineNumber;
            LoadIndex = loadIndex;
            IsOverride = isOverride;
        }

        internal string Source
        {
            get { return ModName + " (priority " + (LoadIndex + 1) + ") line " + LineNumber; }
        }
    }

    internal sealed class WorkingKnowledgeLayerAudit
    {
        private readonly Dictionary<string, WorkingKnowledgeLayerMapping> mappings =
            new Dictionary<string, WorkingKnowledgeLayerMapping>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, WorkingKnowledgeLayerGroup> groups =
            new Dictionary<string, WorkingKnowledgeLayerGroup>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ResearchCatalogEntry> resolvedGroups =
            new Dictionary<string, ResearchCatalogEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> issues = new List<string>();
        private readonly List<string> notices = new List<string>();
        private readonly List<WorkingKnowledgeLayerMove> moves = new List<WorkingKnowledgeLayerMove>();

        internal IDictionary<string, WorkingKnowledgeLayerMapping> Mappings
        {
            get { return mappings; }
        }

        internal IDictionary<string, WorkingKnowledgeLayerGroup> Groups
        {
            get { return groups; }
        }

        internal IDictionary<string, ResearchCatalogEntry> ResolvedGroups
        {
            get { return resolvedGroups; }
        }

        internal IList<string> Issues
        {
            get { return issues; }
        }

        internal IList<string> Notices
        {
            get { return notices; }
        }

        internal IList<WorkingKnowledgeLayerMove> Moves
        {
            get { return moves; }
        }

        internal int LayerCount { get; set; }
        internal int DeclaredGroupCount { get; set; }
        internal int ActiveGroupCount { get; set; }
        internal int RedefinedGroupCount { get; set; }
        internal int MappingCount { get; set; }
        internal int ActiveMappingCount { get; set; }
        internal int BuiltInReplacementCount { get; set; }
        internal int ConflictingBlockCount { get; set; }
        internal int SkippedMappingCount { get; set; }
        internal int OverrideCount { get; set; }
        internal int ActiveOverrideCount { get; set; }

        internal void AddIssue(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                issues.Add(message);
        }

        internal void AddNotice(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                notices.Add(message);
        }

        internal void AddMove(WorkingKnowledgeLayerMove move)
        {
            if (move != null)
                moves.Add(move);
        }

        internal void SortMessages()
        {
            issues.Sort(StringComparer.OrdinalIgnoreCase);
            notices.Sort(StringComparer.OrdinalIgnoreCase);
            moves.Sort((left, right) =>
            {
                var destinationCompare = string.Compare(left.ToDisplayName, right.ToDisplayName, StringComparison.OrdinalIgnoreCase);
                if (destinationCompare != 0)
                    return destinationCompare;
                var sourceCompare = string.Compare(left.FromDisplayName, right.FromDisplayName, StringComparison.OrdinalIgnoreCase);
                if (sourceCompare != 0)
                    return sourceCompare;
                return string.Compare(left.BlockKey, right.BlockKey, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}
