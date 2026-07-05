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
            new ResearchFragmentLootEntry("PersonalContainerSmall", 0.25f, 0.08f, 0.02f, 0.005f),
            new ResearchFragmentLootEntry("PersonalContainerLarge", 0.50f, 0.20f, 0.06f, 0.015f),
            new ResearchFragmentLootEntry("WeaponsLight", 0.10f, 0.04f, 0.01f, 0.002f),
            new ResearchFragmentLootEntry("WeaponsHeavy", 0.10f, 0.05f, 0.02f, 0.004f),
            new ResearchFragmentLootEntry("PersonalItemsTrader", 0.10f, 0.03f, 0.006f, 0.001f),
            new ResearchFragmentLootEntry("PersonalItemsMiner", 0.10f, 0.03f, 0.006f, 0.001f),
            new ResearchFragmentLootEntry("PersonalItemsBuilder", 0.10f, 0.03f, 0.006f, 0.001f),
            new ResearchFragmentLootEntry("PersonalItemsPirate", 0.10f, 0.04f, 0.015f, 0.003f),
            new ResearchFragmentLootEntry("PersonalItemsMilitary", 0.10f, 0.04f, 0.015f, 0.003f),
            new ResearchFragmentLootEntry("PersonalItemsDerelict", 0.10f, 0.04f, 0.01f, 0.002f),
            new ResearchFragmentLootEntry("SmallStash", 0.10f, 0.05f, 0.02f, 0.006f),
            new ResearchFragmentLootEntry("Consumables", 0.05f, 0.015f, 0.003f, 0.0005f),
            new ResearchFragmentLootEntry("FoodItems", 0.05f, 0.01f, 0.002f, 0.0005f),
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

            var builder = definition.GetObjectBuilder() as MyObjectBuilder_ContainerTypeDefinition;
            if (builder == null)
                return;

            var items = new List<MyObjectBuilder_ContainerTypeDefinition.ContainerTypeItem>();
            if (builder.Items != null)
                items.AddRange(builder.Items);

            var changed = false;
            changed |= AddFragmentLootItem(items, CommonFragmentId, ScaleFragmentLootFrequency(entry.CommonFrequency, lootScale));
            changed |= AddFragmentLootItem(items, UncommonFragmentId, ScaleFragmentLootFrequency(entry.UncommonFrequency, lootScale));
            changed |= AddFragmentLootItem(items, RareFragmentId, ScaleFragmentLootFrequency(entry.RareFrequency, lootScale));
            changed |= AddFragmentLootItem(items, PrototechFragmentId, ScaleFragmentLootFrequency(entry.PrototechFrequency, lootScale));

            if (!changed)
                return;

            builder.Items = items.ToArray();
            definition.Init(builder, definition.Context);
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
