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
            Reward("structure.hangar_gate", 0.21, 0.1035, 18.521), // target 6.4 blocks, complexity 5.03, anchor 18s, small 2.8s x0.25
            Reward("structure.door", 0.21, 0.0887, 46.031), // target 6.7 blocks, complexity 5.30, anchor 40s, small 14s x0.25
            Reward("gas.processing", 0.21, 0.081, 29.597), // target 6.8 blocks, complexity 5.37, anchor 25s, small 10.5s x0.25
            Reward("production.basic", 0.21, 0.08, 50.152), // target 7.0 blocks, complexity 5.52, anchor 40s, small 20s x0.27
            Reward("production.food", 0.21, 0.0857, 35.378), // target 6.8 blocks, complexity 5.36, anchor 30s
            Reward("production.advanced", 0.21, 0.0857, 94.857), // target 6.8 blocks, complexity 5.38, anchor 80s
            Reward("prototech.assembler", 0.21, 0.0857, 273.259), // target 9.6 blocks, complexity 7.37, anchor 120s
            Reward("automation.ai_control", 0.21, 0.0813, 33.069), // target 7.4 blocks, complexity 5.78, anchor 24s, small 10s x0.25
            Reward("power.battery", 0.21, 0.08, 58.355), // target 7.6 blocks, complexity 5.94, anchor 40s, small 20s x0.27
            Reward("prototech.battery", 0.21, 0.08, 178.119), // target 11.0 blocks, complexity 8.38, anchor 60s, small 20s x0.33
            Reward("communications", 0.21, 0.083, 51.171), // target 7.1 blocks, complexity 5.57, anchor 40s, small 16s x0.25
            Reward("control.interfaces", 0.21, 0.0959, 20.368), // target 6.3 blocks, complexity 5.00, anchor 20s, small 6s x0.25
            Reward("decor.decorative_fixtures", 0.21, 0.1069, 19.31), // target 6.1 blocks, complexity 4.87, anchor 20s, small 3.8s x0.25
            Reward("structure.bridge", 0.21, 0.0857, 34.421), // target 6.7 blocks, complexity 5.29, anchor 30s
            Reward("logistics.cargo_storage", 0.21, 0.08, 17.214), // target 6.7 blocks, complexity 5.29, anchor 15s, small 14s x0.37
            Reward("decor.habitat_fixtures", 0.21, 0.0921, 21.144), // target 6.4 blocks, complexity 5.09, anchor 20s, small 6.5s x0.25
            Reward("control.stations", 0.21, 0.08, 42.873), // target 6.5 blocks, complexity 5.13, anchor 40s, small 20s x0.27
            Reward("logistics.cargo_transfer", 0.21, 0.0909, 33.975), // target 6.7 blocks, complexity 5.26, anchor 30s, small 10s x0.25
            Reward("economy.station_services", 0.21, 0.0857, 29.531), // target 6.8 blocks, complexity 5.37, anchor 25s
            Reward("logistics.conveyor_network", 0.21, 0.083, 21.973), // target 6.6 blocks, complexity 5.19, anchor 20s, small 8s x0.25
            Reward("structure.industrial", 0.21, 0.108, 11.338), // target 6.1 blocks, complexity 4.82, anchor 12s, small 2s x0.25
            Reward("life_support", 0.21, 0.08, 25.294), // target 7.1 blocks, complexity 5.54, anchor 20s, small 9s x0.26
            Reward("structure.passage", 0.21, 0.0857, 15.119), // target 6.1 blocks, complexity 4.82, anchor 16s
            Reward("structure.interior", 0.21, 0.0802, 12.967), // target 6.0 blocks, complexity 4.69, anchor 14s, small 6s x0.25
            Reward("structure.window", 0.21, 0.0985, 18.164), // target 6.7 blocks, complexity 5.27, anchor 16s, small 3s x0.25
            Reward("structure.industrial_access", 0.21, 0.0857, 12.967), // target 6.0 blocks, complexity 4.76, anchor 14s
            Reward("armor.light", 0.21, 0.1091, 12.967), // target 6.0 blocks, complexity 4.77, anchor 14s, small 1s x0.25
            Reward("armor.heavy", 0.21, 0.0942, 24.872), // target 7.0 blocks, complexity 5.50, anchor 20s, small 2s x0.25
            Reward("fundamentals", 0.21, 0.0857, 7.41), // target 6.0 blocks, complexity 4.77, anchor 8s, small 3s x0.25
            Reward("decor.signage", 0.21, 0.0909, 5.557), // target 6.0 blocks, complexity 4.77, anchor 6s, small 2s x0.25
            Reward("tools.drill", 0.21, 0.08, 34.254), // target 6.7 blocks, complexity 5.28, anchor 30s, small 20s x0.31
            Reward("prototech.drill", 0.21, 0.0857, 137.89), // target 9.7 blocks, complexity 7.40, anchor 60s
            Reward("utility.interior_lighting", 0.21, 0.1079, 13.265), // target 6.1 blocks, complexity 4.83, anchor 14s, small 3s x0.25
            Reward("utility.display_systems", 0.21, 0.0987, 22.628), // target 6.7 blocks, complexity 5.26, anchor 20s, small 4s x0.25
            Reward("automation.logic", 0.21, 0.0972, 27.998), // target 6.8 blocks, complexity 5.33, anchor 24s, small 5.3s x0.25
            Reward("mechanical.systems", 0.21, 0.08, 18.576), // target 6.8 blocks, complexity 5.32, anchor 16s, small 8s x0.27
            Reward("utility.gravity", 0.21, 0.095, 61.131), // target 6.9 blocks, complexity 5.45, anchor 50s, small 5.8s x0.25
            Reward("prototech.gyroscope", 0.21, 0.08, 204.928), // target 11.9 blocks, complexity 8.97, anchor 60s, small 35s x0.35
            Reward("power.hydrogen_engine", 0.21, 0.08, 73.644), // target 7.0 blocks, complexity 5.46, anchor 60s, small 40s x0.31
            Reward("prototech.reactor", 0.21, 0.0857, 300), // target 12.0 blocks, complexity 9.29, anchor 120s
            Reward("weapons.turret", 0.21, 0.08, 31.493), // target 7.2 blocks, complexity 5.64, anchor 24s, small 21.3s x0.36
            Reward("utility.jump_drive", 0.21, 0.0857, 300), // target 11.9 blocks, complexity 9.03, anchor 120s
            Reward("prototech.jump_drive", 0.21, 0.08, 300), // target 12.0 blocks, complexity 9.49, anchor 120s, small 40s x0.36
            Reward("mechanical.wheel_systems", 0.21, 0.08, 24.249), // target 6.9 blocks, complexity 5.43, anchor 20s, small 10s x0.27
            Reward("prototech.o2_h2_generator", 0.21, 0.0857, 300), // target 11.5 blocks, complexity 8.69, anchor 100s
            Reward("gas.storage", 0.21, 0.084, 36.848), // target 6.7 blocks, complexity 5.30, anchor 32s, small 12.5s x0.25
            Reward("power.reactor", 0.21, 0.08, 96.072), // target 9.9 blocks, complexity 7.56, anchor 40s, small 20s x0.30
            Reward("prototech.refinery", 0.21, 0.08, 300), // target 12.0 blocks, complexity 9.41, anchor 90s, small 40s x0.36
            Reward("utility.directed_lighting", 0.21, 0.0972, 25.809), // target 6.5 blocks, complexity 5.13, anchor 24s, small 7s x0.25
            Reward("tools.grinder", 0.21, 0.08, 27.734), // target 6.8 blocks, complexity 5.31, anchor 24s, small 18s x0.33
            Reward("tools.welder", 0.21, 0.08, 26.971), // target 6.7 blocks, complexity 5.24, anchor 24s, small 18s x0.33
            Reward("weapons.fixed_weapon", 0.21, 0.0848, 45.077), // target 7.7 blocks, complexity 6.03, anchor 30s, small 11.5s x0.25
            Reward("power.renewable", 0.21, 0.08, 44.58), // target 7.7 blocks, complexity 5.99, anchor 30s, small 14s x0.26
            Reward("propulsion.atmospheric_thruster", 0.21, 0.08, 71.608), // target 8.5 blocks, complexity 6.56, anchor 40s, small 10s x0.25
            Reward("propulsion.hydrogen_thruster", 0.21, 0.087, 58.231), // target 7.6 blocks, complexity 5.94, anchor 40s, small 10s x0.25
            Reward("propulsion.ion_thruster", 0.21, 0.08, 113.286), // target 10.8 blocks, complexity 8.19, anchor 40s, small 10s x0.32
            Reward("prototech.thruster", 0.21, 0.08, 239.433), // target 10.4 blocks, complexity 7.95, anchor 90s, small 20s x0.31
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
