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
        internal static WorkingKnowledgeLayerAudit Rebuild(
            SchematicCatalog schematicCatalog,
            string unlockerBlockPrefix,
            string researchPedestalSubtype,
            string researchSciFiTerminalSubtype,
            string controlPanelResearchGroupSubtype)
        {
            schematicCatalog.Clear();
            ClearVanillaResearch(unlockerBlockPrefix);

            var blocks = GetResearchCandidateBlocks(unlockerBlockPrefix, researchPedestalSubtype, researchSciFiTerminalSubtype);
            var catalogByBlockKey = ResearchCatalog.BuildLookupByBlockKey();
            var layerAudit = WorkingKnowledgeLayerMappingLoader.LoadMappings();
            AddLayerMappings(catalogByBlockKey, layerAudit);
            schematicCatalog.LoadMetadata(ResearchCatalog.Entries);

            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                ResearchCatalogEntry catalogEntry;
                if (!catalogByBlockKey.TryGetValue(GetDefinitionKey(block.Id), out catalogEntry))
                    continue;

                var unlockerId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), catalogEntry.UnlockerSubtype);
                var researchBlock = MyDefinitionManager.Static.GetResearchBlock(block.Id);
                if (researchBlock == null)
                {
                    AddMissingResearchBlockIssue(layerAudit, block.Id);
                    continue;
                }

                var unlockerBlock = MyDefinitionManager.Static.GetCubeBlockDefinition(unlockerId);
                var unlockerResearchGroup = MyDefinitionManager.Static.GetResearchGroup(catalogEntry.GroupSubtype);
                if (unlockerBlock == null || unlockerResearchGroup == null)
                {
                    layerAudit.AddIssue(
                        "Could not bind " + GetDefinitionKey(block.Id) + " because its Working Knowledge unlocker or research group is missing.");
                    continue;
                }

                researchBlock.UnlockedByGroups = new[] { catalogEntry.GroupSubtype };
                unlockerResearchGroup.Members = new SerializableDefinitionId[] { unlockerId };
                schematicCatalog.AddMappedBlock(catalogEntry, block.Id, unlockerId);
                if (layerAudit.Mappings.ContainsKey(GetDefinitionKey(block.Id)) &&
                    layerAudit.IsMappingEligible(GetDefinitionKey(block.Id)))
                    layerAudit.ActiveMappingCount++;
            }

            AddMissingBlockIssues(blocks, layerAudit);

            ConfigureResearchTerminalUnlocks(researchPedestalSubtype, researchSciFiTerminalSubtype, controlPanelResearchGroupSubtype);
            return layerAudit;
        }

        private static void AddLayerMappings(Dictionary<string, ResearchCatalogEntry> catalogByBlockKey, WorkingKnowledgeLayerAudit layerAudit)
        {
            foreach (var mapping in layerAudit.Mappings)
            {
                if (catalogByBlockKey.ContainsKey(mapping.Key))
                {
                    layerAudit.IgnoreMapping(mapping.Key);
                    layerAudit.AddIssue(
                        "Ignored " + mapping.Value.Source + " because " + mapping.Key + " already has a built-in Working Knowledge mapping.");
                    continue;
                }

                catalogByBlockKey.Add(mapping.Key, mapping.Value.Entry);
            }
        }

        private static void AddMissingResearchBlockIssue(WorkingKnowledgeLayerAudit layerAudit, MyDefinitionId blockId)
        {
            WorkingKnowledgeLayerMapping mapping;
            var key = GetDefinitionKey(blockId);
            if (!layerAudit.Mappings.TryGetValue(key, out mapping))
                return;

            layerAudit.AddIssue(
                key + " from " + mapping.Source + " has no ResearchBlocks.sbc entry and was not activated.");
        }

        private static void AddMissingBlockIssues(List<MyCubeBlockDefinition> blocks, WorkingKnowledgeLayerAudit layerAudit)
        {
            var loadedBlockKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < blocks.Count; i++)
                loadedBlockKeys.Add(GetDefinitionKey(blocks[i].Id));

            foreach (var mapping in layerAudit.Mappings)
            {
                if (!loadedBlockKeys.Contains(mapping.Key))
                {
                    layerAudit.AddIssue(
                        mapping.Key + " from " + mapping.Value.Source + " does not match a loaded public block.");
                }
            }
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
