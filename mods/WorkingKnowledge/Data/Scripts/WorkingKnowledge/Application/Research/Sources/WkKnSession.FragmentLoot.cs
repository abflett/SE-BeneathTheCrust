using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private static readonly ResearchFragmentLootEntry[] ResearchFragmentLootTable = new ResearchFragmentLootEntry[]
        {
            new ResearchFragmentLootEntry("PersonalContainerSmall", 3.627f, 1.907f, 1.093f, 0.776f),
            new ResearchFragmentLootEntry("PersonalContainerLarge", 4.598f, 2.593f, 1.557f, 0.960f),
            new ResearchFragmentLootEntry("StationContainerType", 4.479f, 2.650f, 1.532f, 0.871f),
            new ResearchFragmentLootEntry("Tools", 2.293f, 1.376f, 0.803f, 0.459f),
            new ResearchFragmentLootEntry("ToolsMiner", 0.995f, 0.574f, 0.326f, 0.203f),
            new ResearchFragmentLootEntry("ToolsBuilder", 0.966f, 0.564f, 0.322f, 0.201f),
            new ResearchFragmentLootEntry("AdvancedTools", 2.000f, 1.250f, 0.833f, 0.583f),
            new ResearchFragmentLootEntry("WeaponsLight", 2.581f, 1.548f, 1.032f, 0.645f),
            new ResearchFragmentLootEntry("WeaponsHeavy", 2.438f, 1.508f, 0.999f, 0.697f),
            new ResearchFragmentLootEntry("PersonalItemsTrader", 1.208f, 0.705f, 0.403f, 0.252f),
            new ResearchFragmentLootEntry("PersonalItemsMiner", 1.208f, 0.705f, 0.403f, 0.252f),
            new ResearchFragmentLootEntry("PersonalItemsBuilder", 1.208f, 0.705f, 0.403f, 0.252f),
            new ResearchFragmentLootEntry("PersonalItemsPirate", 1.544f, 0.938f, 0.607f, 0.441f),
            new ResearchFragmentLootEntry("PersonalItemsMilitary", 1.544f, 0.938f, 0.607f, 0.441f),
            new ResearchFragmentLootEntry("PersonalItemsDerelict", 0.986f, 0.563f, 0.317f, 0.176f),
            new ResearchFragmentLootEntry("SmallStash", 2.710f, 1.626f, 1.084f, 0.677f),
            new ResearchFragmentLootEntry("Consumables", 0.647f, 0.323f, 0.144f, 0.072f),
            new ResearchFragmentLootEntry("FoodItems", 0.688f, 0.342f, 0.170f, 0.085f),
            new ResearchFragmentLootEntry("SpoiledFoodItems", 0.367f, 0.164f, 0.073f, 0.036f),
            new ResearchFragmentLootEntry("BasicComponents", 1.695f, 0.990f, 0.540f, 0.321f),
            new ResearchFragmentLootEntry("BasicComponentsAndIngots", 2.201f, 1.285f, 0.702f, 0.417f),
            new ResearchFragmentLootEntry("AdvancedComponents", 2.564f, 1.671f, 1.069f, 0.734f),
            new ResearchFragmentLootEntry("AdvancedComponentsAndIngots", 2.665f, 1.732f, 1.106f, 0.758f),
            new ResearchFragmentLootEntry("BasicOres", 0.847f, 0.404f, 0.198f, 0.118f),
            new ResearchFragmentLootEntry("BasicOresAndIngots", 1.031f, 0.562f, 0.307f, 0.183f),
            new ResearchFragmentLootEntry("BasicOresBulk", 1.933f, 1.128f, 0.644f, 0.403f),
            new ResearchFragmentLootEntry("BasicOresIngotsAndComponents", 1.882f, 1.154f, 0.684f, 0.453f),
            new ResearchFragmentLootEntry("BasicIngots", 0.922f, 0.494f, 0.267f, 0.168f),
            new ResearchFragmentLootEntry("AdvancedIngots", 1.281f, 0.659f, 0.402f, 0.264f),
            new ResearchFragmentLootEntry("AdvancedOres", 1.108f, 0.573f, 0.350f, 0.230f),
            new ResearchFragmentLootEntry("AdvancedOresBulk", 3.000f, 1.875f, 1.250f, 0.875f),
            new ResearchFragmentLootEntry("AdvancedOresAndIngots", 1.541f, 0.866f, 0.526f, 0.361f),
            new ResearchFragmentLootEntry("AdvancedOresIngotsAndComponents", 2.466f, 1.578f, 1.007f, 0.727f),
            new ResearchFragmentLootEntry("AmmoStockpile", 0.794f, 0.453f, 0.288f, 0.191f),
            new ResearchFragmentLootEntry("AmmoStockpileMedium", 1.024f, 0.629f, 0.416f, 0.290f),
            new ResearchFragmentLootEntry("AmmoStockpileLarge", 1.441f, 0.951f, 0.650f, 0.471f),
            new ResearchFragmentLootEntry("AmmoStockpileSingle", 1.806f, 1.084f, 0.723f, 0.452f),
            new ResearchFragmentLootEntry("AmmoStockpileSingleMedium", 1.806f, 1.084f, 0.723f, 0.452f),
            new ResearchFragmentLootEntry("AmmoStockpileSingleLarge", 1.806f, 1.084f, 0.723f, 0.452f),
            new ResearchFragmentLootEntry("TreasureBox", 1.188f, 0.773f, 0.484f, 0.347f),
            new ResearchFragmentLootEntry("FactorumComponents", 1.979f, 1.327f, 0.890f, 0.647f),
            new ResearchFragmentLootEntry("FactorumComponentsAndIngots", 2.352f, 1.648f, 1.137f, 0.805f),
            new ResearchFragmentLootEntry("FactorumComponentsOresAndIngots", 2.470f, 1.826f, 1.303f, 0.996f),
            new ResearchFragmentLootEntry("PrototechComponents", 2.119f, 1.616f, 1.127f, 0.839f),
            new ResearchFragmentLootEntry("SalvageContainerAll", 2.354f, 1.442f, 0.854f, 0.566f),
            new ResearchFragmentLootEntry("SalvageContainerComponents", 1.968f, 1.211f, 0.719f, 0.477f),
            new ResearchFragmentLootEntry("SalvageContainerOre", 0.925f, 0.501f, 0.271f, 0.162f),
            new ResearchFragmentLootEntry("SalvageContainerIngotsOre", 1.050f, 0.619f, 0.357f, 0.203f),
            new ResearchFragmentLootEntry("HaulingContainer_S", 0.098f, 0.053f, 0.028f, 0.016f),
            new ResearchFragmentLootEntry("HaulingContainer_M", 0.119f, 0.063f, 0.033f, 0.021f),
            new ResearchFragmentLootEntry("HaulingContainer_L", 0.143f, 0.079f, 0.044f, 0.026f),
            new ResearchFragmentLootEntry("HaulingContainer_XL", 0.169f, 0.092f, 0.055f, 0.036f),
            new ResearchFragmentLootEntry("CargoLargeMining1A", 0.316f, 0.171f, 0.092f, 0.053f),
            new ResearchFragmentLootEntry("CargoLargeMining1B", 0.433f, 0.225f, 0.125f, 0.069f),
            new ResearchFragmentLootEntry("CargoLargeMining1C", 0.369f, 0.193f, 0.108f, 0.059f),
            new ResearchFragmentLootEntry("CargoSmallMining1A", 0.216f, 0.112f, 0.062f, 0.034f),
            new ResearchFragmentLootEntry("CargoSmallMining3A", 0.295f, 0.159f, 0.094f, 0.054f),
            new ResearchFragmentLootEntry("CargoSmallMining3B", 0.295f, 0.159f, 0.094f, 0.054f),
            new ResearchFragmentLootEntry("CargoSmallMilitary1A", 0.237f, 0.136f, 0.083f, 0.057f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2A", 0.413f, 0.242f, 0.160f, 0.099f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2B", 0.340f, 0.198f, 0.130f, 0.081f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2C", 0.413f, 0.242f, 0.160f, 0.099f),
            new ResearchFragmentLootEntry("CargoLargeMilitary3A", 0.463f, 0.286f, 0.193f, 0.121f),
            new ResearchFragmentLootEntry("CargoSmallMilitary3A", 0.463f, 0.286f, 0.193f, 0.121f),
            new ResearchFragmentLootEntry("CargoLargeTrade1A", 0.212f, 0.113f, 0.060f, 0.034f),
            new ResearchFragmentLootEntry("CargoSmallTrade2A", 0.212f, 0.113f, 0.060f, 0.034f),
            new ResearchFragmentLootEntry("CargoSmallTrade2B", 0.235f, 0.122f, 0.068f, 0.038f),
            new ResearchFragmentLootEntry("CargoLargeTrade3A", 0.352f, 0.191f, 0.113f, 0.066f),
        };

        private void InjectResearchDataFragmentsIntoContainerLoot()
        {
            InjectResearchDataFragmentsIntoContainerLoot(false);
        }

        private void RefreshResearchDataFragmentsInContainerLoot()
        {
            InjectResearchDataFragmentsIntoContainerLoot(true);
        }

        private void InjectResearchDataFragmentsIntoContainerLoot(bool refreshExisting)
        {
            if ((!refreshExisting && researchDataFragmentLootInjected) || MyDefinitionManager.Static == null)
                return;

            researchDataFragmentLootInjected = true;
            var lootScale = config == null ? 1.0 : config.DataFragmentLootScale;

            for (var i = 0; i < ResearchFragmentLootTable.Length; i++)
            {
                var entry = ResearchFragmentLootTable[i];
                try
                {
                    InjectResearchDataFragmentsIntoContainerLoot(entry, lootScale);
                }
                catch (Exception exception)
                {
                    MyLog.Default.WriteLineAndConsole(
                        LogPrefix + " failed to patch research data fragment loot for " + entry.ContainerSubtype + ": " + exception);
                }
            }
        }

        private static void InjectResearchDataFragmentsIntoContainerLoot(ResearchFragmentLootEntry entry, double lootScale)
        {
            var definition = MyDefinitionManager.Static.GetContainerTypeDefinition(entry.ContainerSubtype);
            if (definition == null)
                return;

            var countMin = definition.CountMin;
            var countMax = definition.CountMax;
            var builder = definition.GetObjectBuilder() as MyObjectBuilder_ContainerTypeDefinition;
            if (builder == null)
                return;

            var items = GetExistingContainerLootItems(definition, builder);

            var changed = false;
            changed |= AddFragmentLootItem(items, CommonFragmentId, ScaleFragmentLootFrequency(entry.CommonFrequency, lootScale));
            changed |= AddFragmentLootItem(items, UncommonFragmentId, ScaleFragmentLootFrequency(entry.UncommonFrequency, lootScale));
            changed |= AddFragmentLootItem(items, RareFragmentId, ScaleFragmentLootFrequency(entry.RareFrequency, lootScale));
            changed |= AddFragmentLootItem(items, PrototechFragmentId, ScaleFragmentLootFrequency(entry.PrototechFrequency, lootScale));

            if (!changed)
                return;

            // MyContainerTypeDefinition does not serialize its own CountMin/CountMax
            // through GetObjectBuilder(), but Init() needs them to keep containers
            // from generating zero loot rolls after the runtime patch.
            builder.CountMin = countMin;
            builder.CountMax = countMax;
            builder.Items = items.ToArray();
            if (definition.Sets != null)
                definition.Sets.Clear();

            definition.Init(builder, definition.Context);
        }

        private static List<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem> GetExistingContainerLootItems(
            MyContainerTypeDefinition definition,
            MyObjectBuilder_ContainerTypeDefinition builder)
        {
            var items = new List<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem>();

            if (definition.Items != null && definition.Items.Length > 0)
            {
                for (var i = 0; i < definition.Items.Length; i++)
                    items.Add(ToContainerTypeItemBuilder(definition.Items[i]));

                return items;
            }

            if (builder.Items != null)
                items.AddRange(builder.Items);

            return items;
        }

        private static MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem ToContainerTypeItemBuilder(
            MyContainerTypeDefinition.ContainerTypeItem item)
        {
            var storyCategory = item.StoryCategory.SubtypeName;
            if (string.IsNullOrEmpty(storyCategory))
                storyCategory = null;

            return new MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem
            {
                AmountMin = item.AmountMin.ToString(),
                AmountMax = item.AmountMax.ToString(),
                Frequency = item.Frequency,
                Id = item.DefinitionId,
                Set = item.Set,
                StoryCategory = storyCategory,
            };
        }

        private static float ScaleFragmentLootFrequency(float frequency, double lootScale)
        {
            return Math.Max(0f, frequency * (float)Math.Max(0.0, lootScale));
        }

        private static bool AddFragmentLootItem(
            List<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem> items,
            MyDefinitionId definitionId,
            float frequency)
        {
            frequency = Math.Max(0f, frequency);

            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].Id != definitionId)
                    continue;

                if (Math.Abs(items[i].Frequency - frequency) <= 0.00001f)
                    return false;

                items[i].Frequency = frequency;
                return true;
            }

            if (frequency <= 0f)
                return false;

            items.Add(new MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem
            {
                AmountMin = "1",
                AmountMax = "1",
                Frequency = frequency,
                Id = definitionId,
            });

            return true;
        }

        private struct ResearchFragmentLootEntry
        {
            public readonly string ContainerSubtype;
            public readonly float CommonFrequency;
            public readonly float UncommonFrequency;
            public readonly float RareFrequency;
            public readonly float PrototechFrequency;

            public ResearchFragmentLootEntry(
                string containerSubtype,
                float commonFrequency,
                float uncommonFrequency,
                float rareFrequency,
                float prototechFrequency)
            {
                ContainerSubtype = containerSubtype;
                CommonFrequency = commonFrequency;
                UncommonFrequency = uncommonFrequency;
                RareFrequency = rareFrequency;
                PrototechFrequency = prototechFrequency;
            }
        }
    }
}
