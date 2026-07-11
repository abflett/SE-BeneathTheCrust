using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using MountedComponentGroupSnapshot = WkKn.WeldBotchComponentRecoveryService.MountedComponentGroupSnapshot;
using MountedComponentLoss = WkKn.WeldBotchComponentRecoveryService.MountedComponentLoss;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const float PreCapWeldBotchPhysicalLossRatio = 0.50f;

        private static readonly MyStringHash WeldBotchDamageType = MyStringHash.GetOrCompute("WkKnWeldBotch");

        private bool ApplyWeldBotchOperation(WeldOperation operation, double workScale, float positiveRawDelta)
        {
            if (operation == null || operation.Block == null || operation.Inventory == null)
                return false;

            var botchChance = weldBotchChancePolicy.GetChance(
                operation.Block,
                operation.Proficiency,
                new WeldBotchChanceTuning(
                    config.ProficiencyBuildCapEnabled,
                    config.WeldBotchBaseChance,
                    config.WeldBotchChanceScale,
                    config.WeldBotchMaxChance,
                    config.WeldBotchPostFunctionalPressure,
                    config.WeldBotchSoftCapPressure,
                    config.WeldBotchPressureScale));
            if (botchChance <= WeldCapTolerance || random.NextDouble() >= botchChance)
                return false;

            WeldBotchResult result;
            if (!TryApplyProductionWeldBotchDamage(operation, positiveRawDelta, out result))
                return false;

            ShowWeldBotchWarning(operation.IdentityId, operation.ResearchId, operation.Block, result);
            return true;
        }

        private bool TryApplyProductionWeldBotchDamage(
            WeldOperation operation,
            float positiveRawDelta,
            out WeldBotchResult result)
        {
            result = new WeldBotchResult();
            if (operation == null || operation.Block == null || operation.Block.ComponentStack == null || positiveRawDelta <= WeldCapTolerance)
                return false;

            var destroyable = operation.Block as IMyDestroyableObject;
            if (destroyable == null)
                return false;

            var beforeIntegrity = operation.Block.ComponentStack.Integrity;
            var minimumRemainingIntegrity = GetMinimumBotchRemainingIntegrity(operation.Block.ComponentStack.MaxIntegrity);
            var maxRawLoss = Math.Max(0f, beforeIntegrity - minimumRemainingIntegrity);
            if (maxRawLoss <= WeldCapTolerance)
                return false;

            var baseRawLoss = Math.Max(0f, positiveRawDelta * (float)config.WeldBotchRawLossRatio);
            if (baseRawLoss <= WeldCapTolerance)
                return false;

            var beforeComponents = WeldBotchComponentRecoveryService.CaptureMountedComponentGroups(operation.Block);
            var steelPlateIntegrity = Math.Max(1f, WeldBotchComponentRecoveryService.GetSteelPlateIntegrity(beforeComponents));
            var debtKey = GetWeldBotchDebtKey(operation);
            var debtBefore = GetWeldBotchRawDamageDebt(debtKey);
            var debtToApply = GetWeldBotchRawDamageDebtToApply(debtBefore, baseRawLoss, steelPlateIntegrity, maxRawLoss);
            var plannedPhysicalRawLoss = Math.Min(maxRawLoss, baseRawLoss + debtToApply);
            var targetRawLoss = GetGovernedWeldBotchPhysicalRawLoss(operation, plannedPhysicalRawLoss, baseRawLoss);

            if (targetRawLoss > WeldCapTolerance)
            {
                var damageRatio = GetWeldDamageRatio(operation.Block);
                if (damageRatio <= 0f)
                    return false;

                var requestedDamage = targetRawLoss / damageRatio;
                if (requestedDamage <= 0f)
                    return false;

                destroyable.DoDamage(requestedDamage, WeldBotchDamageType, true, null, operation.AttackerId, 0, false, null);
                operation.Block.ApplyAccumulatedDamage(true);
                blockIntegrityMonitor.RefreshBlockSnapshot(operation.Block);
            }

            var afterIntegrity = operation.Block.ComponentStack.Integrity;
            var actualRawLoss = Math.Max(0f, beforeIntegrity - afterIntegrity);
            if (actualRawLoss <= WeldCapTolerance && baseRawLoss <= WeldCapTolerance)
                return false;

            result.DamageAmount = actualRawLoss;
            var componentLosses = WeldBotchComponentRecoveryService.BuildMountedComponentLosses(
                beforeComponents,
                WeldBotchComponentRecoveryService.CaptureMountedComponentGroups(operation.Block));
            var debtUpdate = UpdateWeldBotchRawDamageDebt(
                operation,
                debtKey,
                debtBefore,
                baseRawLoss,
                actualRawLoss,
                steelPlateIntegrity,
                componentLosses,
                beforeComponents);
            var reportedLosses = WeldBotchComponentRecoveryService.CombineMountedComponentLosses(
                componentLosses,
                debtUpdate.InventoryLosses);
            result.LostComponentsText = WeldBotchComponentRecoveryService.FormatMountedComponentLosses(reportedLosses);
            result.RecoveredComponentsText = weldBotchComponentRecovery.ApplyMountedComponentForgiveness(
                operation,
                componentLosses,
                actualRawLoss,
                beforeComponents,
                config == null ? 1.0 : config.WeldBotchForgivenessScale);
            return true;
        }

        private static float GetMinimumBotchRemainingIntegrity(float maxIntegrity)
        {
            return Math.Max(1f, Math.Max(0f, maxIntegrity) * 0.01f);
        }

        private float GetGovernedWeldBotchPhysicalRawLoss(WeldOperation operation, float plannedRawLoss, float baseRawLoss)
        {
            if (operation == null || operation.Block == null || operation.Block.ComponentStack == null || plannedRawLoss <= WeldCapTolerance)
                return 0f;

            var maxIntegrity = operation.Block.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= WeldCapTolerance)
                return plannedRawLoss;

            var currentIntegrity = operation.Block.ComponentStack.Integrity;
            var capRatio = weldBotchChancePolicy.GetCapIntegrityRatio(
                BlockCondition.GetFunctionalBuildRatio(operation.Block),
                operation.Proficiency);
            var capIntegrity = (float)(RatioMath.Clamp01(capRatio) * maxIntegrity);
            var minimumRemainingIntegrity = GetMinimumBotchRemainingIntegrity(maxIntegrity);
            var floorIntegrity = Math.Max(minimumRemainingIntegrity, capIntegrity);

            if (currentIntegrity <= floorIntegrity + WeldCapTolerance)
                return Math.Min(plannedRawLoss, Math.Max(0f, baseRawLoss * PreCapWeldBotchPhysicalLossRatio));

            var capRollbackRawLoss = Math.Max(0f, currentIntegrity - floorIntegrity);
            return Math.Min(plannedRawLoss, capRollbackRawLoss);
        }

        private static float GetWeldBotchRawDamageDebtToApply(float debtBefore, float baseRawLoss, float steelPlateIntegrity, float maxRawLoss)
        {
            if (debtBefore <= WeldCapTolerance || steelPlateIntegrity <= WeldCapTolerance)
                return 0f;

            if (debtBefore + baseRawLoss < steelPlateIntegrity - WeldCapTolerance)
                return 0f;

            return Math.Max(0f, Math.Min(debtBefore, maxRawLoss - baseRawLoss));
        }

        private float GetWeldBotchRawDamageDebt(string debtKey)
        {
            if (string.IsNullOrWhiteSpace(debtKey))
                return 0f;

            float debt;
            return blockWorkState.WeldBotchRawDamageDebtByKey.TryGetValue(debtKey, out debt) ? Math.Max(0f, debt) : 0f;
        }

        private sealed class WeldBotchDebtUpdate
        {
            public readonly List<MountedComponentLoss> InventoryLosses = new List<MountedComponentLoss>();
        }

        private WeldBotchDebtUpdate UpdateWeldBotchRawDamageDebt(
            WeldOperation operation,
            string debtKey,
            float debtBefore,
            float baseRawLoss,
            float actualRawLoss,
            float steelPlateIntegrity,
            List<MountedComponentLoss> componentLosses,
            List<MountedComponentGroupSnapshot> beforeGroups)
        {
            var update = new WeldBotchDebtUpdate();
            if (string.IsNullOrWhiteSpace(debtKey))
                return update;

            if (WeldBotchComponentRecoveryService.CountMountedComponentLosses(componentLosses) > 0)
            {
                var componentLossDebtAfter = Math.Max(0f, baseRawLoss - Math.Min(baseRawLoss, actualRawLoss));
                componentLossDebtAfter = TryPayWeldBotchDebtFromInventory(
                    operation,
                    beforeGroups,
                    steelPlateIntegrity,
                    componentLossDebtAfter,
                    update.InventoryLosses);
                if (componentLossDebtAfter > WeldCapTolerance)
                    blockWorkState.WeldBotchRawDamageDebtByKey[debtKey] = componentLossDebtAfter;
                else
                    blockWorkState.WeldBotchRawDamageDebtByKey.Remove(debtKey);

                return update;
            }

            var debtAfter = debtBefore + Math.Max(0f, baseRawLoss);
            debtAfter = TryPayWeldBotchDebtFromInventory(
                operation,
                beforeGroups,
                steelPlateIntegrity,
                debtAfter,
                update.InventoryLosses);
            if (debtAfter <= WeldCapTolerance)
            {
                blockWorkState.WeldBotchRawDamageDebtByKey.Remove(debtKey);
                return update;
            }

            blockWorkState.WeldBotchRawDamageDebtByKey[debtKey] = debtAfter;
            return update;
        }

        private static string GetWeldBotchDebtKey(WeldOperation operation)
        {
            if (operation == null || operation.IdentityId == 0 || string.IsNullOrWhiteSpace(operation.ResearchId))
                return null;

            return operation.IdentityId.ToString(CultureInfo.InvariantCulture) + ":" + operation.ResearchId;
        }

        private static float TryPayWeldBotchDebtFromInventory(
            WeldOperation operation,
            List<MountedComponentGroupSnapshot> beforeGroups,
            float steelPlateIntegrity,
            float debt,
            List<MountedComponentLoss> inventoryLosses)
        {
            if (operation == null || operation.Inventory == null || beforeGroups == null || beforeGroups.Count == 0 || inventoryLosses == null)
                return debt;

            var threshold = Math.Max(1f, steelPlateIntegrity);
            var remainingDebt = Math.Max(0f, debt);
            if (remainingDebt < threshold - WeldCapTolerance)
                return remainingDebt;

            var candidates = GetWeldBotchDebtInventoryCandidates(beforeGroups);
            while (remainingDebt >= threshold - WeldCapTolerance)
            {
                MountedComponentGroupSnapshot paidComponent;
                if (!TryRemoveCheapestWeldBotchDebtComponent(operation, candidates, out paidComponent))
                    break;

                WeldBotchComponentRecoveryService.AddMountedComponentRecovery(
                    inventoryLosses,
                    new MountedComponentLoss
                    {
                        Key = paidComponent.Key,
                        DefinitionId = paidComponent.DefinitionId,
                        DisplayName = paidComponent.DisplayName,
                        LostCount = 1,
                        ComponentMaxIntegrity = paidComponent.ComponentMaxIntegrity,
                        ComponentMass = paidComponent.ComponentMass,
                    },
                    1);
                remainingDebt = Math.Max(0f, remainingDebt - threshold);
            }

            return remainingDebt;
        }

        private static List<MountedComponentGroupSnapshot> GetWeldBotchDebtInventoryCandidates(List<MountedComponentGroupSnapshot> beforeGroups)
        {
            var candidates = new List<MountedComponentGroupSnapshot>();
            if (beforeGroups == null)
                return candidates;

            for (var i = 0; i < beforeGroups.Count; i++)
            {
                var group = beforeGroups[i];
                if (group.MountedCount <= 0 || string.IsNullOrWhiteSpace(group.DefinitionId.SubtypeName))
                    continue;

                candidates.Add(group);
            }

            candidates.Sort(WeldBotchComponentRecoveryService.CompareMountedComponentDebtPriority);
            return candidates;
        }

        private static bool TryRemoveCheapestWeldBotchDebtComponent(
            WeldOperation operation,
            List<MountedComponentGroupSnapshot> candidates,
            out MountedComponentGroupSnapshot paidComponent)
        {
            paidComponent = new MountedComponentGroupSnapshot();
            if (operation == null || operation.Inventory == null || candidates == null || candidates.Count == 0)
                return false;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (TryRemoveInventoryComponent(operation.Inventory, candidate.DefinitionId))
                {
                    paidComponent = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool TryRemoveInventoryComponent(MyInventory inventory, MyDefinitionId definitionId)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(definitionId.SubtypeName))
                return false;

            var before = GetInventoryComponentAmount(inventory, definitionId);
            if (before < (MyFixedPoint)1)
                return false;

            var componentBuilder = MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(definitionId.SubtypeName);
            inventory.RemoveItemsOfType((MyFixedPoint)1, componentBuilder, false);

            var after = GetInventoryComponentAmount(inventory, definitionId);
            return before - after > 0;
        }

        private static MyFixedPoint GetInventoryComponentAmount(MyInventory inventory, MyDefinitionId definitionId)
        {
            if (inventory == null)
                return (MyFixedPoint)0;

            var snapshot = ComponentInventory.Snapshot(inventory);
            MyFixedPoint amount;
            return snapshot.TryGetValue(definitionId, out amount) ? amount : (MyFixedPoint)0;
        }

    }
}
