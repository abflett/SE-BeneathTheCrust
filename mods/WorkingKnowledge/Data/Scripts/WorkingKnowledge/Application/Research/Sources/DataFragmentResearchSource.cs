using System;
using System.Collections.Generic;

namespace WkKn
{
    internal enum ResearchFragmentTier
    {
        Common,
        Uncommon,
        Rare,
        Prototech
    }

    internal sealed class DataFragmentResearchSource
    {
        private const double CommonFragmentMinProgress = 0.25;
        private const double CommonFragmentMaxProgress = 0.50;
        private const double UncommonFragmentMinProgress = 0.30;
        private const double UncommonFragmentMaxProgress = 0.55;
        private const double RareFragmentMinProgress = 0.40;
        private const double RareFragmentMaxProgress = 0.70;
        private const double PrototechFragmentMinProgress = 0.25;
        private const double PrototechFragmentMaxProgress = 0.50;

        internal bool TrySelectReward(
            SchematicCatalog schematicCatalog,
            ResearchFragmentTier tier,
            Random random,
            double rewardScale,
            out ResearchFragmentReward reward)
        {
            reward = new ResearchFragmentReward();
            if (schematicCatalog == null || random == null)
                return false;

            SchematicTier schematicTier;
            double minProgress;
            double maxProgress;
            GetTierSettings(tier, out schematicTier, out minProgress, out maxProgress);

            var candidates = new List<ResearchUnlockTarget>();
            foreach (var candidate in schematicCatalog.Targets)
            {
                if (candidate.Tier != schematicTier)
                    continue;

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
                return false;

            reward.Target = candidates[random.Next(candidates.Count)];
            reward.Progress = (minProgress + (random.NextDouble() * (maxProgress - minProgress))) * Math.Max(0.0, rewardScale);
            return true;
        }

        private static void GetTierSettings(ResearchFragmentTier tier, out SchematicTier schematicTier, out double minProgress, out double maxProgress)
        {
            switch (tier)
            {
                case ResearchFragmentTier.Uncommon:
                    schematicTier = SchematicTier.Uncommon;
                    minProgress = UncommonFragmentMinProgress;
                    maxProgress = UncommonFragmentMaxProgress;
                    return;

                case ResearchFragmentTier.Rare:
                    schematicTier = SchematicTier.Rare;
                    minProgress = RareFragmentMinProgress;
                    maxProgress = RareFragmentMaxProgress;
                    return;

                case ResearchFragmentTier.Prototech:
                    schematicTier = SchematicTier.Prototech;
                    minProgress = PrototechFragmentMinProgress;
                    maxProgress = PrototechFragmentMaxProgress;
                    return;

                default:
                    schematicTier = SchematicTier.Common;
                    minProgress = CommonFragmentMinProgress;
                    maxProgress = CommonFragmentMaxProgress;
                    return;
            }
        }
    }
}
