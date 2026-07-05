using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;



namespace WkKn
{
    public partial class WkKnSession
    {
        private void BeforeGrindDamage(object target, ref MyDamageInformation damage)
        {
            var slimBlock = target as IMySlimBlock;
            if (slimBlock == null || slimBlock.BlockDefinition == null)
                return;

            ResearchUnlockTarget unlockTarget;
            if (!schematicCatalog.TryGetTargetByBlock(slimBlock.BlockDefinition.Id, out unlockTarget))
                return;

            long researchIdentityId;
            if (TryResolveGrinderOperator(damage.AttackerId, out researchIdentityId) && researchIdentityId != 0)
            {
                TrackResearchDamageStart(damage.AttackerId, slimBlock, unlockTarget, researchIdentityId);
                TrackProficiencyGrindStart(damage.AttackerId, slimBlock, unlockTarget, researchIdentityId);
            }

            MyInventory outputInventory;
            long operatorIdentityId;
            if (!TryResolveGrindContext(damage.AttackerId, out outputInventory, out operatorIdentityId))
                return;

            if (!config.ProficiencyEnabled || !config.ProficiencyGrindingLossEnabled || !config.SalvageScrapEnabled)
                return;

            var intactPercent = GetProficiencyBasedSalvageIntactPercent(operatorIdentityId, unlockTarget.ResearchId);
            if (intactPercent >= 100)
                return;

            var operation = new SalvageOperation
            {
                AttackerId = damage.AttackerId,
                Block = slimBlock,
                Inventory = outputInventory,
                ResearchId = unlockTarget.ResearchId,
                DisplayName = unlockTarget.DisplayName,
                IntactPercent = intactPercent,
                Tick = simulationTick,
                BeforeComponents = ComponentInventory.Snapshot(outputInventory),
            };

            blockWorkState.PendingSalvageOperations.Add(operation);
        }

        private void AfterGrindDamage(object target, MyDamageInformation damage)
        {
            var slimBlock = target as IMySlimBlock;
            if (slimBlock == null)
                return;

            ApplyResearchDamageOperations(damage.AttackerId, slimBlock);
            ApplyProficiencyGrindOperations(damage.AttackerId, slimBlock);

            for (var i = blockWorkState.PendingSalvageOperations.Count - 1; i >= 0; i--)
            {
                var operation = blockWorkState.PendingSalvageOperations[i];
                if (operation.AttackerId != damage.AttackerId || operation.Block != slimBlock)
                    continue;

                blockWorkState.PendingSalvageOperations.RemoveAt(i);
                ApplySalvageOperation(operation);
                return;
            }
        }

        private void ClearStaleProficiencyGrindOperations()
        {
            for (var i = blockWorkState.PendingProficiencyGrindOperations.Count - 1; i >= 0; i--)
            {
                if (simulationTick - blockWorkState.PendingProficiencyGrindOperations[i].Tick > 1)
                    blockWorkState.PendingProficiencyGrindOperations.RemoveAt(i);
            }
        }

        private void OnDestroyed(object target, MyDamageInformation damage)
        {
            if (!MyAPIGateway.Session.IsServer || damage.Type != MyDamageType.Grind)
                return;

            var slimBlock = target as IMySlimBlock;
            if (slimBlock == null || slimBlock.BlockDefinition == null)
                return;

            var grinder = MyEntities.GetEntityById(damage.AttackerId);
            if (!(grinder is IMyAngleGrinder) && !(grinder is IMyShipGrinder))
                return;

            long researchIdentityId;
            if (!TryResolveGrinderOperator(damage.AttackerId, out researchIdentityId) || researchIdentityId == 0)
                return;

            ApplyResearchDamageOperations(damage.AttackerId, slimBlock, true);
            ApplyProficiencyGrindOperations(damage.AttackerId, slimBlock, true);
        }

        private void TrackResearchDamageStart(long attackerId, IMySlimBlock slimBlock, ResearchUnlockTarget target, long identityId)
        {
            if (GetPlayerResearchProgress(identityId, target.ResearchId) >= RequiredResearchProgress)
                return;

            blockWorkState.PendingResearchOperations.Add(new ResearchDamageOperation
            {
                AttackerId = attackerId,
                Block = slimBlock,
                Target = target,
                IdentityId = identityId,
                BeforeBuildRatio = BlockCondition.GetBuildRatio(slimBlock),
                Tick = simulationTick,
            });
        }

        private void TrackProficiencyGrindStart(long attackerId, IMySlimBlock slimBlock, ResearchUnlockTarget target, long identityId)
        {
            if (!config.ProficiencyEnabled)
                return;

            blockWorkState.PendingProficiencyGrindOperations.Add(new ProficiencyDamageOperation
            {
                AttackerId = attackerId,
                Block = slimBlock,
                IdentityId = identityId,
                ResearchId = target.ResearchId,
                DisplayName = target.DisplayName,
                BeforeBuildRatio = BlockCondition.GetBuildRatio(slimBlock),
                Tick = simulationTick,
            });
        }

        private void ApplyResearchDamageOperations(long attackerId, IMySlimBlock slimBlock, bool destroyed = false)
        {
            for (var i = blockWorkState.PendingResearchOperations.Count - 1; i >= 0; i--)
            {
                var operation = blockWorkState.PendingResearchOperations[i];
                if (operation.AttackerId != attackerId || operation.Block != slimBlock)
                    continue;

                blockWorkState.PendingResearchOperations.RemoveAt(i);
                var afterBuildRatio = destroyed ? 0.0 : BlockCondition.GetBuildRatio(slimBlock);
                var dismantledRatio = Clamp01(operation.BeforeBuildRatio - afterBuildRatio);
                AwardResearch(operation.IdentityId, operation.Target, dismantledRatio, GetFullWorkReward(operation.Block, operation.Target));
                return;
            }
        }

        private void ApplyProficiencyGrindOperations(long attackerId, IMySlimBlock slimBlock, bool destroyed = false)
        {
            for (var i = blockWorkState.PendingProficiencyGrindOperations.Count - 1; i >= 0; i--)
            {
                var operation = blockWorkState.PendingProficiencyGrindOperations[i];
                if (operation.AttackerId != attackerId || operation.Block != slimBlock)
                    continue;

                blockWorkState.PendingProficiencyGrindOperations.RemoveAt(i);
                var afterBuildRatio = destroyed ? 0.0 : BlockCondition.GetBuildRatio(slimBlock);
                var dismantledRatio = Clamp01(operation.BeforeBuildRatio - afterBuildRatio);
                if (dismantledRatio > 0.0)
                    AwardProficiencyFromHandsOnWork(operation.IdentityId, operation.ResearchId, dismantledRatio * GetFullWorkReward(operation.Block, operation.ResearchId), "grinding");

                return;
            }
        }

        private void ClearStaleResearchOperations()
        {
            for (var i = blockWorkState.PendingResearchOperations.Count - 1; i >= 0; i--)
            {
                if (simulationTick - blockWorkState.PendingResearchOperations[i].Tick > 1)
                    blockWorkState.PendingResearchOperations.RemoveAt(i);
            }
        }

        private static double GetFullWorkReward(IMySlimBlock slimBlock, ResearchUnlockTarget target)
        {
            return GetFullWorkReward(slimBlock, target.ResearchId);
        }

        private static double GetFullWorkReward(IMySlimBlock slimBlock, string researchId)
        {
            var definition = slimBlock != null ? slimBlock.BlockDefinition as MyCubeBlockDefinition : null;
            var smallGrid = definition != null && definition.CubeSize == MyCubeSize.Small;
            return SchematicWorkRewardTable.GetFullWorkReward(researchId, smallGrid, GetBuildSeconds(definition));
        }

        private static double GetBuildSeconds(MyCubeBlockDefinition definition)
        {
            if (definition == null || definition.MaxIntegrity <= 0f || definition.IntegrityPointsPerSec <= 0f)
                return 0.0;

            return definition.MaxIntegrity / definition.IntegrityPointsPerSec;
        }

    }
}
