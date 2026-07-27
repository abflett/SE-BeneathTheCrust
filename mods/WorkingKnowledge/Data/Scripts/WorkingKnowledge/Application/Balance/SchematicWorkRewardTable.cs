using System;
using System.Collections.Generic;

namespace WkKn
{
    internal static class SchematicWorkRewardTable
    {
        private const double DefaultLargeGridBaseWorkReward = 0.21;
        private const double DefaultSmallGridBaseWorkReward = 0.0857;
        private const double DefaultReferenceBuildSeconds = 8.0;
        private const double SmallGridFallbackBuildSeconds = 3.0;
        private const double MinimumBuildTimeFactor = 0.5;
        private const double MaximumBuildTimeFactor = 4.0;

        private static readonly SchematicWorkReward[] Rewards = new SchematicWorkReward[]
        {
            Reward("structure.hangar_gate", 0.21, 0.1035, 18.515), // target 6.4 blocks, complexity 5.03, anchor 18s, small 2.8s x0.25
            Reward("structure.door", 0.21, 0.0887, 46.001), // target 6.7 blocks, complexity 5.30, anchor 40s, small 14s x0.25
            Reward("gas.processing", 0.21, 0.081, 29.575), // target 6.8 blocks, complexity 5.37, anchor 25s, small 10.5s x0.25
            Reward("production.basic", 0.21, 0.08, 50.111), // target 7.0 blocks, complexity 5.52, anchor 40s, small 20s x0.27
            Reward("production.food", 0.21, 0.0857, 35.353), // target 6.8 blocks, complexity 5.36, anchor 30s
            Reward("production.advanced", 0.21, 0.0857, 94.787), // target 6.8 blocks, complexity 5.38, anchor 80s
            Reward("prototech.assembler", 0.21, 0.0857, 272.656), // target 9.6 blocks, complexity 7.37, anchor 120s
            Reward("automation.ai_control", 0.21, 0.0813, 33.033), // target 7.4 blocks, complexity 5.78, anchor 24s, small 10s x0.25
            Reward("power.battery", 0.21, 0.08, 58.283), // target 7.6 blocks, complexity 5.94, anchor 40s, small 20s x0.27
            Reward("prototech.battery", 0.21, 0.08, 177.659), // target 11.0 blocks, complexity 8.38, anchor 60s, small 20s x0.33
            Reward("communications", 0.21, 0.083, 51.126), // target 7.1 blocks, complexity 5.57, anchor 40s, small 16s x0.25
            Reward("control.interfaces", 0.21, 0.0959, 20.362), // target 6.3 blocks, complexity 5.00, anchor 20s, small 6s x0.25
            Reward("decor.decorative_fixtures", 0.21, 0.1069, 19.308), // target 6.1 blocks, complexity 4.87, anchor 20s, small 3.8s x0.25
            Reward("structure.bridge", 0.21, 0.0857, 34.399), // target 6.7 blocks, complexity 5.29, anchor 30s
            Reward("logistics.cargo_storage", 0.21, 0.08, 17.203), // target 6.7 blocks, complexity 5.29, anchor 15s, small 14s x0.37
            Reward("decor.habitat_fixtures", 0.21, 0.0921, 21.135), // target 6.4 blocks, complexity 5.09, anchor 20s, small 6.5s x0.25
            Reward("control.stations", 0.21, 0.08, 42.854), // target 6.5 blocks, complexity 5.13, anchor 40s, small 20s x0.27
            Reward("logistics.cargo_transfer", 0.21, 0.0909, 33.954), // target 6.7 blocks, complexity 5.26, anchor 30s, small 10s x0.25
            Reward("economy.station_services", 0.21, 0.0857, 29.509), // target 6.8 blocks, complexity 5.37, anchor 25s
            Reward("logistics.conveyor_network", 0.21, 0.083, 21.962), // target 6.6 blocks, complexity 5.19, anchor 20s, small 8s x0.25
            Reward("structure.industrial", 0.21, 0.1091, 11.115), // target 6.0 blocks, complexity 4.77, anchor 12s, small 2s x0.25
            Reward("life_support", 0.21, 0.08, 25.272), // target 7.1 blocks, complexity 5.54, anchor 20s, small 9s x0.26
            Reward("structure.passage", 0.21, 0.0857, 15.118), // target 6.1 blocks, complexity 4.82, anchor 16s
            Reward("structure.interior", 0.21, 0.0802, 12.967), // target 6.0 blocks, complexity 4.69, anchor 14s, small 6s x0.25
            Reward("structure.window", 0.21, 0.0986, 18.153), // target 6.7 blocks, complexity 5.27, anchor 16s, small 3s x0.25
            Reward("armor.light", 0.21, 0.1091, 12.967), // target 6.0 blocks, complexity 4.77, anchor 14s, small 1s x0.25
            Reward("armor.heavy", 0.21, 0.0942, 24.853), // target 7.0 blocks, complexity 5.50, anchor 20s, small 2s x0.25
            Reward("fundamentals", 0.21, 0.0857, 7.41), // target 6.0 blocks, complexity 4.77, anchor 8s, small 3s x0.25
            Reward("decor.signage", 0.21, 0.0909, 5.557), // target 6.0 blocks, complexity 4.77, anchor 6s, small 2s x0.25
            Reward("tools.drill", 0.21, 0.08, 34.232), // target 6.7 blocks, complexity 5.28, anchor 30s, small 20s x0.31
            Reward("prototech.drill", 0.21, 0.0857, 137.582), // target 9.7 blocks, complexity 7.40, anchor 60s
            Reward("utility.interior_lighting", 0.21, 0.1079, 13.264), // target 6.1 blocks, complexity 4.83, anchor 14s, small 3s x0.25
            Reward("utility.display_systems", 0.21, 0.0987, 22.614), // target 6.7 blocks, complexity 5.26, anchor 20s, small 4s x0.25
            Reward("automation.logic", 0.21, 0.0972, 27.979), // target 6.8 blocks, complexity 5.33, anchor 24s, small 5.3s x0.25
            Reward("mechanical.systems", 0.21, 0.08, 18.563), // target 6.8 blocks, complexity 5.32, anchor 16s, small 8s x0.27
            Reward("utility.gravity", 0.21, 0.095, 61.081), // target 6.9 blocks, complexity 5.45, anchor 50s, small 5.8s x0.25
            Reward("prototech.gyroscope", 0.21, 0.08, 204.324), // target 11.8 blocks, complexity 8.97, anchor 60s, small 35s x0.35
            Reward("power.hydrogen_engine", 0.21, 0.08, 73.583), // target 7.0 blocks, complexity 5.46, anchor 60s, small 40s x0.31
            Reward("prototech.reactor", 0.21, 0.0857, 300), // target 12.0 blocks, complexity 9.29, anchor 120s
            Reward("weapons.turret", 0.21, 0.08, 31.463), // target 7.2 blocks, complexity 5.64, anchor 24s, small 21.3s x0.36
            Reward("utility.jump_drive", 0.21, 0.0857, 300), // target 11.9 blocks, complexity 9.03, anchor 120s
            Reward("prototech.jump_drive", 0.21, 0.08, 300), // target 12.0 blocks, complexity 9.49, anchor 120s, small 40s x0.36
            Reward("mechanical.wheel_systems", 0.21, 0.08, 24.229), // target 6.9 blocks, complexity 5.43, anchor 20s, small 10s x0.27
            Reward("prototech.o2_h2_generator", 0.21, 0.0857, 300), // target 11.4 blocks, complexity 8.69, anchor 100s
            Reward("gas.storage", 0.21, 0.084, 36.824), // target 6.7 blocks, complexity 5.30, anchor 32s, small 12.5s x0.25
            Reward("power.reactor", 0.21, 0.08, 95.847), // target 9.9 blocks, complexity 7.56, anchor 40s, small 20s x0.29
            Reward("prototech.refinery", 0.21, 0.08, 300), // target 12.0 blocks, complexity 9.41, anchor 90s, small 40s x0.36
            Reward("utility.directed_lighting", 0.21, 0.0972, 25.797), // target 6.5 blocks, complexity 5.13, anchor 24s, small 7s x0.25
            Reward("tools.grinder", 0.21, 0.08, 27.716), // target 6.8 blocks, complexity 5.31, anchor 24s, small 18s x0.33
            Reward("tools.welder", 0.21, 0.08, 26.956), // target 6.7 blocks, complexity 5.24, anchor 24s, small 18s x0.33
            Reward("weapons.fixed_weapon", 0.21, 0.0848, 45.017), // target 7.7 blocks, complexity 6.03, anchor 30s, small 11.5s x0.25
            Reward("power.renewable", 0.21, 0.08, 44.522), // target 7.7 blocks, complexity 5.99, anchor 30s, small 14s x0.26
            Reward("propulsion.atmospheric_thruster", 0.21, 0.08, 71.486), // target 8.5 blocks, complexity 6.56, anchor 40s, small 10s x0.25
            Reward("propulsion.hydrogen_thruster", 0.21, 0.0871, 58.159), // target 7.6 blocks, complexity 5.94, anchor 40s, small 10s x0.25
            Reward("propulsion.ion_thruster", 0.21, 0.08, 112.988), // target 10.8 blocks, complexity 8.19, anchor 40s, small 10s x0.32
            Reward("prototech.thruster", 0.21, 0.08, 238.844), // target 10.4 blocks, complexity 7.95, anchor 90s, small 20s x0.31
        };

        private static Dictionary<string, SchematicWorkReward> rewardsByResearchId;

        internal static double GetFullWorkReward(string researchId, bool smallGrid, double actualBuildSeconds)
        {
            EnsureLookup();

            SchematicWorkReward reward;
            if (!string.IsNullOrWhiteSpace(researchId) && rewardsByResearchId.TryGetValue(researchId, out reward))
            {
                return GetBaseWorkReward(reward, smallGrid) * GetBuildTimeFactor(actualBuildSeconds, smallGrid, reward.ReferenceBuildSeconds);
            }

            var defaultReward = smallGrid
                ? DefaultSmallGridBaseWorkReward
                : DefaultLargeGridBaseWorkReward;

            return defaultReward * GetBuildTimeFactor(actualBuildSeconds, smallGrid, DefaultReferenceBuildSeconds);
        }

        private static double GetBaseWorkReward(SchematicWorkReward reward, bool smallGrid)
        {
            return smallGrid
                ? reward.SmallGridBaseWorkReward
                : reward.LargeGridBaseWorkReward;
        }

        private static double GetBuildTimeFactor(double actualBuildSeconds, bool smallGrid, double referenceBuildSeconds)
        {
            var safeReferenceBuildSeconds = referenceBuildSeconds > 0.0
                ? referenceBuildSeconds
                : DefaultReferenceBuildSeconds;

            var safeActualBuildSeconds = actualBuildSeconds > 0.0
                ? actualBuildSeconds
                : (smallGrid ? SmallGridFallbackBuildSeconds : DefaultReferenceBuildSeconds);

            var factor = Math.Sqrt(safeActualBuildSeconds / safeReferenceBuildSeconds);
            if (factor < MinimumBuildTimeFactor)
                return MinimumBuildTimeFactor;

            if (factor > MaximumBuildTimeFactor)
                return MaximumBuildTimeFactor;

            return factor;
        }

        private static SchematicWorkReward Reward(string researchId, double largeGridBaseWorkReward, double smallGridBaseWorkReward, double referenceBuildSeconds)
        {
            return new SchematicWorkReward(researchId, largeGridBaseWorkReward, smallGridBaseWorkReward, referenceBuildSeconds);
        }

        private static void EnsureLookup()
        {
            if (rewardsByResearchId != null)
                return;

            var lookup = new Dictionary<string, SchematicWorkReward>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < Rewards.Length; i++)
            {
                var reward = Rewards[i];
                if (string.IsNullOrWhiteSpace(reward.ResearchId))
                    continue;

                lookup[reward.ResearchId] = reward;
            }

            rewardsByResearchId = lookup;
        }
    }
}
