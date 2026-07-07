using System;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilders;
using VRage.ObjectBuilders;

namespace WkKn
{
    internal sealed class SalvageRecoveryPolicy
    {
        private readonly double requiredProgress;

        internal SalvageRecoveryPolicy(double requiredProgress)
        {
            this.requiredProgress = requiredProgress;
        }

        internal int GetIntactPercent(double proficiencyProgress, double salvageScale)
        {
            if (proficiencyProgress >= requiredProgress)
                return 100;

            var percent = Math.Max(0, Math.Min(99, (int)Math.Floor(RatioMath.Clamp01(proficiencyProgress) * 100.0)));
            var intactPercent = 10;
            if (percent >= 80)
                intactPercent = 80;
            else if (percent >= 60)
                intactPercent = 60;
            else if (percent >= 40)
                intactPercent = 40;
            else if (percent >= 20)
                intactPercent = 25;

            return Math.Max(0, Math.Min(100, (int)Math.Round(intactPercent * salvageScale)));
        }

        internal bool Apply(SalvageOperation operation, MyPhysicalItemDefinition scrapDefinition, double scrapMassYield, Random random, out SalvageRecoveryResult result)
        {
            result = new SalvageRecoveryResult();
            if (operation == null || operation.Inventory == null || operation.BeforeComponents == null || operation.IntactPercent >= 100)
                return false;

            if (scrapDefinition == null || scrapDefinition.Mass <= 0f)
                return false;

            var yield = RatioMath.Clamp01(scrapMassYield);
            var afterComponents = ComponentInventory.Snapshot(operation.Inventory);
            var totalScrapMass = 0f;

            foreach (var item in afterComponents)
            {
                MyFixedPoint beforeAmount;
                operation.BeforeComponents.TryGetValue(item.Key, out beforeAmount);

                var gained = item.Value - beforeAmount;
                if (gained <= 0)
                    continue;

                var gainedCount = gained.ToIntSafe();
                if (gainedCount <= 0)
                    continue;

                var componentDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Key);
                if (componentDefinition == null || componentDefinition.Mass <= 0f)
                    continue;

                var scrapCount = RollScrappedComponentCount(gainedCount, operation.IntactPercent, random);
                if (scrapCount <= 0)
                    continue;

                var componentBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(item.Key.SubtypeName);
                operation.Inventory.RemoveItemsOfType((MyFixedPoint)scrapCount, componentBuilder, false);

                result.RemovedComponents += scrapCount;
                totalScrapMass += componentDefinition.Mass * scrapCount;
            }

            if (totalScrapMass <= 0f)
                return false;

            if (yield <= 0.0)
                return result.RemovedComponents > 0;

            result.ScrapAmount = (MyFixedPoint)((totalScrapMass * yield) / scrapDefinition.Mass);
            operation.Inventory.AddItems(result.ScrapAmount, MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>("Scrap"));
            return true;
        }

        private static int RollScrappedComponentCount(int gainedCount, int intactPercent, Random random)
        {
            if (intactPercent <= 0)
                return gainedCount;

            var scrapCount = 0;
            for (var i = 0; i < gainedCount; i++)
            {
                if (random.Next(0, 100) >= intactPercent)
                    scrapCount++;
            }

            return scrapCount;
        }
    }

    internal struct SalvageRecoveryResult
    {
        public int RemovedComponents;
        public MyFixedPoint ScrapAmount;
    }
}
