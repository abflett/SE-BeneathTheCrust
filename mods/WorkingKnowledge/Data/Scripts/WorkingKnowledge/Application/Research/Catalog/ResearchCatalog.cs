using System;
using System.Collections.Generic;

namespace WkKn
{
    internal static partial class ResearchCatalog
    {
        private static readonly char[] EntryLineSeparators = new[] { '\r', '\n' };
        private static readonly char[] EntryFieldSeparators = new[] { '|' };
        private static ResearchCatalogEntry[] entries;

        private struct ResearchCatalogMetadata
        {
            public readonly string DisplayName;
            public readonly string Description;
            public readonly string GroupSubtype;
            public readonly string UnlockerSubtype;
            public readonly SchematicTier Tier;

            public ResearchCatalogMetadata(string displayName, string description, string groupSubtype, string unlockerSubtype, SchematicTier tier)
            {
                DisplayName = displayName;
                Description = description;
                GroupSubtype = groupSubtype;
                UnlockerSubtype = unlockerSubtype;
                Tier = tier;
            }
        }

        internal static ResearchCatalogEntry[] Entries
        {
            get
            {
                if (entries == null)
                    entries = ParseEntries(EntryData);

                return entries;
            }
        }

        internal static Dictionary<string, ResearchCatalogEntry> BuildLookupByBlockKey()
        {
            var catalogByBlockKey = new Dictionary<string, ResearchCatalogEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in Entries)
            {
                if (!catalogByBlockKey.ContainsKey(entry.BlockKey))
                    catalogByBlockKey.Add(entry.BlockKey, entry);
            }

            return catalogByBlockKey;
        }

        internal static Dictionary<string, ResearchCatalogEntry> BuildLookupByResearchId()
        {
            var catalogByResearchId = new Dictionary<string, ResearchCatalogEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in Entries)
            {
                if (!catalogByResearchId.ContainsKey(entry.ResearchId))
                    catalogByResearchId.Add(entry.ResearchId, entry);
            }

            return catalogByResearchId;
        }

        private static ResearchCatalogEntry[] ParseEntries(string data)
        {
            var metadataByResearchId = ParseMetadata(ResearchMetadataData);
            var lines = data.Split(EntryLineSeparators, StringSplitOptions.RemoveEmptyEntries);
            var result = new ResearchCatalogEntry[lines.Length];

            for (var i = 0; i < lines.Length; i++)
            {
                var fields = lines[i].Split(EntryFieldSeparators);
                if (fields.Length != 2)
                    throw new InvalidOperationException("Invalid Working Knowledge research catalog row: " + lines[i]);

                ResearchCatalogMetadata metadata;
                if (!metadataByResearchId.TryGetValue(fields[1], out metadata))
                    throw new InvalidOperationException("Missing Working Knowledge research catalog metadata for: " + fields[1]);

                result[i] = new ResearchCatalogEntry(
                    fields[0],
                    fields[1],
                    metadata.DisplayName,
                    metadata.Description,
                    metadata.GroupSubtype,
                    metadata.UnlockerSubtype,
                    metadata.Tier);
            }

            return result;
        }

        private static Dictionary<string, ResearchCatalogMetadata> ParseMetadata(string data)
        {
            var result = new Dictionary<string, ResearchCatalogMetadata>(StringComparer.OrdinalIgnoreCase);
            var lines = data.Split(EntryLineSeparators, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < lines.Length; i++)
            {
                var fields = lines[i].Split(EntryFieldSeparators);
                if (fields.Length != 5)
                    throw new InvalidOperationException("Invalid Working Knowledge research catalog metadata row: " + lines[i]);

                result[fields[0]] = new ResearchCatalogMetadata(
                    fields[1],
                    string.Empty,
                    fields[2],
                    fields[3],
                    ParseTier(fields[4]));
            }

            return result;
        }

        private static SchematicTier ParseTier(string value)
        {
            switch (value)
            {
                case "None":
                    return SchematicTier.None;
                case "Common":
                    return SchematicTier.Common;
                case "Uncommon":
                    return SchematicTier.Uncommon;
                case "Rare":
                    return SchematicTier.Rare;
                case "Prototech":
                    return SchematicTier.Prototech;
                default:
                    throw new InvalidOperationException("Unknown Working Knowledge schematic tier: " + value);
            }
        }
    }
}
