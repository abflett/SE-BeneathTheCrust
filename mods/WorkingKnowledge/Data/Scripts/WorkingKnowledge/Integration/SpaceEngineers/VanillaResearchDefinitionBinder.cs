using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
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
            var validCustomGroups = ValidateCustomGroups(layerAudit);
            AddLayerMappings(catalogByBlockKey, validCustomGroups, layerAudit);
            schematicCatalog.LoadMetadata(ResearchCatalog.Entries);
            foreach (var group in layerAudit.Groups)
                schematicCatalog.LoadMetadata(new[] { group.Value.Entry });

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
                WorkingKnowledgeLayerMapping activeMapping;
                if (layerAudit.Mappings.TryGetValue(GetDefinitionKey(block.Id), out activeMapping) &&
                    layerAudit.IsMappingEligible(GetDefinitionKey(block.Id)))
                {
                    layerAudit.ActiveMappingCount++;
                    if (activeMapping.IsOverride)
                        layerAudit.ActiveOverrideCount++;
                }
            }

            AddMissingBlockIssues(blocks, layerAudit);

            ConfigureResearchTerminalUnlocks(researchPedestalSubtype, researchSciFiTerminalSubtype, controlPanelResearchGroupSubtype);
            layerAudit.SortIssues();
            return layerAudit;
        }

        private static HashSet<string> ValidateCustomGroups(WorkingKnowledgeLayerAudit layerAudit)
        {
            var valid = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in layerAudit.Groups)
            {
                var entry = pair.Value.Entry;
                var unlockerId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), entry.UnlockerSubtype);
                var unlocker = MyDefinitionManager.Static.GetCubeBlockDefinition(unlockerId);
                var researchGroup = MyDefinitionManager.Static.GetResearchGroup(entry.GroupSubtype);
                if (unlocker == null || researchGroup == null)
                {
                    layerAudit.AddIssue(
                        "Custom group '" + entry.ResearchId + "' from " + pair.Value.Source +
                        " is inactive because its unlocker block or research group definition is missing.");
                    continue;
                }

                valid.Add(entry.ResearchId);
                layerAudit.ActiveGroupCount++;

                var schematicId = new MyDefinitionId(
                    typeof(MyObjectBuilder_ConsumableItem),
                    "WkKnSchematic_" + GetSafeSubtypeToken(entry.ResearchId));
                if (MyDefinitionManager.Static.GetPhysicalItemDefinition(schematicId) == null)
                {
                    layerAudit.AddIssue(
                        "Custom group '" + entry.ResearchId + "' from " + pair.Value.Source +
                        " has no exact Data Schematic item definition; fragment rewards still work.");
                }
            }

            return valid;
        }

        private static void AddLayerMappings(
            Dictionary<string, ResearchCatalogEntry> catalogByBlockKey,
            HashSet<string> validCustomGroups,
            WorkingKnowledgeLayerAudit layerAudit)
        {
            foreach (var mapping in layerAudit.Mappings)
            {
                if (layerAudit.Groups.ContainsKey(mapping.Value.Entry.ResearchId) &&
                    !validCustomGroups.Contains(mapping.Value.Entry.ResearchId))
                {
                    layerAudit.IgnoreMapping(mapping.Key);
                    continue;
                }

                catalogByBlockKey[mapping.Key] = mapping.Value.Entry;
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

        private static string GetSafeSubtypeToken(string value)
        {
            var builder = new System.Text.StringBuilder();
            var lastWasSeparator = false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                    lastWasSeparator = false;
                }
                else if (builder.Length > 0 && !lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }

            while (builder.Length > 0 && builder[builder.Length - 1] == '_')
                builder.Length--;
            return builder.ToString();
        }
    }
}
