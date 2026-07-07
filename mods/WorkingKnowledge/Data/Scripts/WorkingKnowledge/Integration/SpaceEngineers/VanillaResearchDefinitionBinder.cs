using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage.ObjectBuilders;

namespace WkKn
{
    internal static class VanillaResearchDefinitionBinder
    {
        internal static void Rebuild(
            SchematicCatalog schematicCatalog,
            string unlockerBlockPrefix,
            string researchPedestalSubtype,
            string researchSciFiTerminalSubtype,
            string controlPanelResearchGroupSubtype,
            string modBlockSchematicMappings)
        {
            schematicCatalog.Clear();
            ClearVanillaResearch(unlockerBlockPrefix);

            var blocks = GetResearchCandidateBlocks(unlockerBlockPrefix, researchPedestalSubtype, researchSciFiTerminalSubtype);
            var entries = ResearchCatalog.Entries;
            var catalogByBlockKey = ResearchCatalog.BuildLookupByBlockKey();
            var catalogByResearchId = BuildLookupByResearchId(entries);
            var mappedResearchIdByBlockKey = ParseBlockSchematicMappings(modBlockSchematicMappings);
            schematicCatalog.LoadMetadata(entries);

            ResearchCatalogEntry fallbackEntry;
            var hasFallbackEntry = TryGetFundamentalsFallbackEntry(entries, out fallbackEntry);

            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                var catalogEntry = default(ResearchCatalogEntry);
                var blockKey = GetDefinitionKey(block.Id);
                string mappedResearchId;
                var hasCatalogEntry = mappedResearchIdByBlockKey.TryGetValue(blockKey, out mappedResearchId) &&
                                      catalogByResearchId.TryGetValue(mappedResearchId, out catalogEntry);

                if (!hasCatalogEntry)
                    hasCatalogEntry = catalogByBlockKey.TryGetValue(blockKey, out catalogEntry);

                if (!hasCatalogEntry)
                {
                    if (!hasFallbackEntry)
                        continue;

                    catalogEntry = CreateFallbackEntry(blockKey, fallbackEntry);
                }

                var unlockerId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), catalogEntry.UnlockerSubtype);
                var researchBlock = MyDefinitionManager.Static.GetResearchBlock(block.Id);
                var unlockerBlock = MyDefinitionManager.Static.GetCubeBlockDefinition(unlockerId);
                var unlockerResearchGroup = MyDefinitionManager.Static.GetResearchGroup(catalogEntry.GroupSubtype);
                if (unlockerBlock == null || unlockerResearchGroup == null)
                    continue;

                if (researchBlock != null)
                    researchBlock.UnlockedByGroups = new[] { catalogEntry.GroupSubtype };

                unlockerResearchGroup.Members = new SerializableDefinitionId[] { unlockerId };
                schematicCatalog.AddMappedBlock(catalogEntry, block.Id, unlockerId);
            }

            ConfigureResearchTerminalUnlocks(researchPedestalSubtype, researchSciFiTerminalSubtype, controlPanelResearchGroupSubtype);
        }

        private static Dictionary<string, ResearchCatalogEntry> BuildLookupByResearchId(IEnumerable<ResearchCatalogEntry> entries)
        {
            var result = new Dictionary<string, ResearchCatalogEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (!result.ContainsKey(entry.ResearchId))
                    result.Add(entry.ResearchId, entry);
            }

            return result;
        }

        private static Dictionary<string, string> ParseBlockSchematicMappings(string mappings)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(mappings))
                return result;

            var rows = mappings.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i].Trim();
                if (row.Length == 0)
                    continue;

                var equalsIndex = row.IndexOf('=');
                if (equalsIndex <= 0 || equalsIndex >= row.Length - 1)
                    continue;

                var blockKey = NormalizeDefinitionKey(row.Substring(0, equalsIndex));
                var researchId = row.Substring(equalsIndex + 1).Trim();
                if (blockKey.Length == 0 || researchId.Length == 0)
                    continue;

                result[blockKey] = researchId;
            }

            return result;
        }

        private static string NormalizeDefinitionKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var trimmed = value.Trim();
            var slashIndex = trimmed.IndexOf('/');
            if (slashIndex < 0)
                return trimmed;

            return NormalizeObjectBuilderType(trimmed.Substring(0, slashIndex).Trim()) + "/" + trimmed.Substring(slashIndex + 1).Trim();
        }

        private static bool TryGetFundamentalsFallbackEntry(IEnumerable<ResearchCatalogEntry> entries, out ResearchCatalogEntry entry)
        {
            foreach (var candidate in entries)
            {
                if (!string.Equals(candidate.ResearchId, "fundamentals", StringComparison.OrdinalIgnoreCase))
                    continue;

                entry = candidate;
                return true;
            }

            entry = default(ResearchCatalogEntry);
            return false;
        }

        private static ResearchCatalogEntry CreateFallbackEntry(string blockKey, ResearchCatalogEntry fallbackEntry)
        {
            return new ResearchCatalogEntry(
                blockKey,
                fallbackEntry.ResearchId,
                fallbackEntry.DisplayName,
                fallbackEntry.GroupSubtype,
                fallbackEntry.UnlockerSubtype,
                fallbackEntry.Tier);
        }

        private static void ConfigureResearchTerminalUnlocks(
            string researchPedestalSubtype,
            string researchSciFiTerminalSubtype,
            string controlPanelResearchGroupSubtype)
        {
            ConfigureResearchTerminalUnlock(new MyDefinitionId(typeof(MyObjectBuilder_TerminalBlock), researchPedestalSubtype), controlPanelResearchGroupSubtype);
            ConfigureResearchTerminalUnlock(new MyDefinitionId(typeof(MyObjectBuilder_MyProgrammableBlock), researchSciFiTerminalSubtype), controlPanelResearchGroupSubtype);
        }

        private static void ConfigureResearchTerminalUnlock(MyDefinitionId terminalId, string controlPanelResearchGroupSubtype)
        {
            var researchBlock = MyDefinitionManager.Static.GetResearchBlock(terminalId);
            if (researchBlock == null)
                return;

            researchBlock.UnlockedByGroups = new[] { controlPanelResearchGroupSubtype };
        }

        private static void ClearVanillaResearch(string unlockerBlockPrefix)
        {
            foreach (var definition in MyDefinitionManager.Static.GetResearchBlockDefinitions())
                definition.UnlockedByGroups = new string[] { };

            foreach (var definition in MyDefinitionManager.Static.GetResearchGroupDefinitions())
            {
                if (definition.Id.SubtypeName == "0")
                {
                    definition.Members = new SerializableDefinitionId[]
                    {
                        new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), unlockerBlockPrefix + "0"),
                    };
                }
                else
                {
                    definition.Members = new SerializableDefinitionId[] { };
                }
            }
        }

        private static List<MyCubeBlockDefinition> GetResearchCandidateBlocks(string unlockerBlockPrefix, string researchPedestalSubtype, string researchSciFiTerminalSubtype)
        {
            var blocks = new List<MyCubeBlockDefinition>();

            foreach (var definition in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var block = definition as MyCubeBlockDefinition;
                if (block == null)
                    continue;

                if (IsGeneratedUnlocker(block.Id, unlockerBlockPrefix))
                    continue;

                if (IsResearchTerminalDefinition(block.Id, researchPedestalSubtype, researchSciFiTerminalSubtype))
                    continue;

                if (!block.Public)
                    continue;

                blocks.Add(block);
            }

            blocks.Sort((left, right) => string.Compare(GetDefinitionKey(left.Id), GetDefinitionKey(right.Id), StringComparison.OrdinalIgnoreCase));
            return blocks;
        }

        private static bool IsGeneratedUnlocker(MyDefinitionId id, string unlockerBlockPrefix)
        {
            return id.TypeId.ToString() == "MyObjectBuilder_CubeBlock" && id.SubtypeName.StartsWith(unlockerBlockPrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsResearchTerminalDefinition(MyDefinitionId id, string researchPedestalSubtype, string researchSciFiTerminalSubtype)
        {
            if (id.TypeId.ToString() == "MyObjectBuilder_TerminalBlock" &&
                id.SubtypeName.Equals(researchPedestalSubtype, StringComparison.OrdinalIgnoreCase))
                return true;

            return id.TypeId.ToString() == "MyObjectBuilder_MyProgrammableBlock" &&
                   id.SubtypeName.Equals(researchSciFiTerminalSubtype, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDefinitionKey(MyDefinitionId id)
        {
            return NormalizeObjectBuilderType(id.TypeId.ToString()) + "/" + id.SubtypeName;
        }

        private static string NormalizeObjectBuilderType(string typeName)
        {
            const string prefix = "MyObjectBuilder_";
            if (typeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return typeName.Substring(prefix.Length);

            return typeName;
        }
    }
}
