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
            new ResearchFragmentLootEntry("PersonalContainerSmall", 3.25f, 0.75f, 0.20f, 0.04f),
            new ResearchFragmentLootEntry("PersonalContainerLarge", 4.50f, 1.10f, 0.34f, 0.09f),
            new ResearchFragmentLootEntry("StationContainerType", 4.25f, 1.00f, 0.24f, 0.06f),
            new ResearchFragmentLootEntry("Tools", 2.00f, 0.45f, 0.10f, 0.02f),
            new ResearchFragmentLootEntry("ToolsMiner", 0.90f, 0.25f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("ToolsBuilder", 0.85f, 0.24f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("AdvancedTools", 1.60f, 0.45f, 0.12f, 0.03f),
            new ResearchFragmentLootEntry("WeaponsLight", 2.20f, 0.55f, 0.14f, 0.03f),
            new ResearchFragmentLootEntry("WeaponsHeavy", 2.15f, 0.60f, 0.18f, 0.04f),
            new ResearchFragmentLootEntry("PersonalItemsTrader", 1.00f, 0.28f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("PersonalItemsMiner", 1.00f, 0.28f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("PersonalItemsBuilder", 1.00f, 0.28f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("PersonalItemsPirate", 1.25f, 0.40f, 0.12f, 0.025f),
            new ResearchFragmentLootEntry("PersonalItemsMilitary", 1.25f, 0.40f, 0.12f, 0.025f),
            new ResearchFragmentLootEntry("PersonalItemsDerelict", 0.80f, 0.24f, 0.06f, 0.012f),
            new ResearchFragmentLootEntry("SmallStash", 2.30f, 0.60f, 0.18f, 0.04f),
            new ResearchFragmentLootEntry("Consumables", 0.60f, 0.15f, 0.03f, 0.006f),
            new ResearchFragmentLootEntry("FoodItems", 0.60f, 0.12f, 0.024f, 0.006f),
            new ResearchFragmentLootEntry("SpoiledFoodItems", 0.40f, 0.08f, 0.016f, 0.004f),
            new ResearchFragmentLootEntry("BasicComponents", 1.60f, 0.38f, 0.09f, 0.018f),
            new ResearchFragmentLootEntry("BasicComponentsAndIngots", 2.10f, 0.50f, 0.12f, 0.024f),
            new ResearchFragmentLootEntry("AdvancedComponents", 2.40f, 0.65f, 0.18f, 0.04f),
            new ResearchFragmentLootEntry("AdvancedComponentsAndIngots", 2.50f, 0.68f, 0.20f, 0.045f),
            new ResearchFragmentLootEntry("BasicOres", 0.80f, 0.18f, 0.04f, 0.008f),
            new ResearchFragmentLootEntry("BasicOresAndIngots", 1.00f, 0.22f, 0.05f, 0.01f),
            new ResearchFragmentLootEntry("BasicOresBulk", 1.60f, 0.35f, 0.08f, 0.015f),
            new ResearchFragmentLootEntry("BasicOresIngotsAndComponents", 1.80f, 0.42f, 0.10f, 0.02f),
            new ResearchFragmentLootEntry("BasicIngots", 0.85f, 0.20f, 0.045f, 0.009f),
            new ResearchFragmentLootEntry("AdvancedIngots", 1.20f, 0.32f, 0.08f, 0.02f),
            new ResearchFragmentLootEntry("AdvancedOres", 1.05f, 0.30f, 0.08f, 0.02f),
            new ResearchFragmentLootEntry("AdvancedOresBulk", 2.45f, 0.60f, 0.16f, 0.035f),
            new ResearchFragmentLootEntry("AdvancedOresAndIngots", 1.50f, 0.40f, 0.10f, 0.025f),
            new ResearchFragmentLootEntry("AdvancedOresIngotsAndComponents", 2.40f, 0.65f, 0.18f, 0.04f),
            new ResearchFragmentLootEntry("AmmoStockpile", 0.75f, 0.20f, 0.06f, 0.014f),
            new ResearchFragmentLootEntry("AmmoStockpileMedium", 0.95f, 0.25f, 0.07f, 0.016f),
            new ResearchFragmentLootEntry("AmmoStockpileLarge", 1.25f, 0.32f, 0.09f, 0.02f),
            new ResearchFragmentLootEntry("AmmoStockpileSingle", 1.55f, 0.35f, 0.09f, 0.018f),
            new ResearchFragmentLootEntry("AmmoStockpileSingleMedium", 1.55f, 0.35f, 0.09f, 0.018f),
            new ResearchFragmentLootEntry("AmmoStockpileSingleLarge", 1.55f, 0.35f, 0.09f, 0.018f),
            new ResearchFragmentLootEntry("TreasureBox", 1.10f, 0.35f, 0.12f, 0.035f),
            new ResearchFragmentLootEntry("FactorumComponents", 1.90f, 0.58f, 0.19f, 0.055f),
            new ResearchFragmentLootEntry("FactorumComponentsAndIngots", 2.25f, 0.65f, 0.20f, 0.06f),
            new ResearchFragmentLootEntry("FactorumComponentsOresAndIngots", 2.35f, 0.68f, 0.22f, 0.065f),
            new ResearchFragmentLootEntry("PrototechComponents", 1.50f, 0.60f, 0.25f, 0.10f),
            new ResearchFragmentLootEntry("SalvageContainerAll", 2.25f, 0.55f, 0.14f, 0.03f),
            new ResearchFragmentLootEntry("SalvageContainerComponents", 1.85f, 0.48f, 0.12f, 0.025f),
            new ResearchFragmentLootEntry("SalvageContainerOre", 0.90f, 0.20f, 0.045f, 0.009f),
            new ResearchFragmentLootEntry("SalvageContainerIngotsOre", 1.00f, 0.22f, 0.05f, 0.01f),
            new ResearchFragmentLootEntry("HaulingContainer_S", 0.09f, 0.025f, 0.006f, 0.001f),
            new ResearchFragmentLootEntry("HaulingContainer_M", 0.11f, 0.03f, 0.007f, 0.0015f),
            new ResearchFragmentLootEntry("HaulingContainer_L", 0.13f, 0.035f, 0.008f, 0.002f),
            new ResearchFragmentLootEntry("HaulingContainer_XL", 0.15f, 0.04f, 0.01f, 0.002f),
            new ResearchFragmentLootEntry("CargoLargeMining1A", 0.27f, 0.07f, 0.016f, 0.003f),
            new ResearchFragmentLootEntry("CargoLargeMining1B", 0.40f, 0.10f, 0.024f, 0.005f),
            new ResearchFragmentLootEntry("CargoLargeMining1C", 0.34f, 0.085f, 0.02f, 0.004f),
            new ResearchFragmentLootEntry("CargoSmallMining1A", 0.21f, 0.055f, 0.013f, 0.003f),
            new ResearchFragmentLootEntry("CargoSmallMining3A", 0.27f, 0.07f, 0.018f, 0.004f),
            new ResearchFragmentLootEntry("CargoSmallMining3B", 0.27f, 0.07f, 0.018f, 0.004f),
            new ResearchFragmentLootEntry("CargoSmallMilitary1A", 0.21f, 0.065f, 0.02f, 0.005f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2A", 0.35f, 0.11f, 0.035f, 0.008f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2B", 0.31f, 0.10f, 0.032f, 0.007f),
            new ResearchFragmentLootEntry("CargoLargeMilitary2C", 0.35f, 0.11f, 0.035f, 0.008f),
            new ResearchFragmentLootEntry("CargoLargeMilitary3A", 0.40f, 0.13f, 0.045f, 0.01f),
            new ResearchFragmentLootEntry("CargoSmallMilitary3A", 0.40f, 0.13f, 0.045f, 0.01f),
            new ResearchFragmentLootEntry("CargoLargeTrade1A", 0.20f, 0.05f, 0.012f, 0.0025f),
            new ResearchFragmentLootEntry("CargoSmallTrade2A", 0.20f, 0.05f, 0.012f, 0.0025f),
            new ResearchFragmentLootEntry("CargoSmallTrade2B", 0.22f, 0.055f, 0.014f, 0.003f),
            new ResearchFragmentLootEntry("CargoLargeTrade3A", 0.31f, 0.08f, 0.02f, 0.004f),
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
