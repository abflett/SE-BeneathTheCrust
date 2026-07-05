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
            string controlPanelResearchGroupSubtype)
        {
            schematicCatalog.Clear();
            ClearVanillaResearch(unlockerBlockPrefix);

            var blocks = GetResearchCandidateBlocks(unlockerBlockPrefix, researchPedestalSubtype, researchSciFiTerminalSubtype);
            var catalogByBlockKey = ResearchCatalog.BuildLookupByBlockKey();
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
                    continue;

                var unlockerBlock = MyDefinitionManager.Static.GetCubeBlockDefinition(unlockerId);
                var unlockerResearchGroup = MyDefinitionManager.Static.GetResearchGroup(catalogEntry.GroupSubtype);
                if (unlockerBlock == null || unlockerResearchGroup == null)
                    continue;

                researchBlock.UnlockedByGroups = new[] { catalogEntry.GroupSubtype };
                unlockerResearchGroup.Members = new SerializableDefinitionId[] { unlockerId };
                schematicCatalog.AddMappedBlock(catalogEntry, block.Id, unlockerId);
            }

            ConfigureResearchTerminalUnlocks(researchPedestalSubtype, researchSciFiTerminalSubtype, controlPanelResearchGroupSubtype);
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
