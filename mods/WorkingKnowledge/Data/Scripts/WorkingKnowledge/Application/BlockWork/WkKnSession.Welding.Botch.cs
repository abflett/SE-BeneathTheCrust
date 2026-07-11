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

namespace WkKn
{
    public partial class WkKnSession
    {
        private const float FallbackSteelPlateIntegrity = 100f;
        private const float PreCapWeldBotchPhysicalLossRatio = 0.50f;
        private const string SteelPlateSubtypeName = "SteelPlate";

        private static readonly MyStringHash WeldBotchDamageType = MyStringHash.GetOrCompute("WkKnWeldBotch");

        private static readonly Dictionary<string, ComponentValue> WeldBotchComponentValues =
            new Dictionary<string, ComponentValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Computer", new ComponentValue(1.0, 0.7, 0.8) },
                { "InteriorPlate", new ComponentValue(15.0, 3.0, 3.0) },
                { "SmallTube", new ComponentValue(15.0, 5.0, 5.0) },
                { "Girder", new ComponentValue(15.0, 6.0, 6.0) },
                { "Construction", new ComponentValue(30.0, 8.0, 8.0) },
                { "Display", new ComponentValue(5.0, 6.0, 8.5) },
                { "RadioCommunication", new ComponentValue(15.0, 9.0, 9.5) },
                { "Explosives", new ComponentValue(5.0, 2.5, 10.75) },
                { "SolarCell", new ComponentValue(1.0, 9.0, 13.5) },
                { "PowerCell", new ComponentValue(50.0, 13.0, 14.5) },
                { "SteelPlate", new ComponentValue(100.0, 21.0, 21.0) },
                { "BulletproofGlass", new ComponentValue(60.0, 15.0, 22.5) },
                { "MetalGrid", new ComponentValue(30.0, 20.0, 25.5) },
                { "Superconductor", new ComponentValue(5.0, 12.0, 26.0) },
                { "Detector", new ComponentValue(4.0, 20.0, 27.5) },
                { "Motor", new ComponentValue(40.0, 25.0, 27.5) },
                { "LargeTube", new ComponentValue(60.0, 30.0, 30.0) },
                { "Reactor", new ComponentValue(20.0, 40.0, 45.0) },
                { "Canvas", new ComponentValue(5.0, 37.0, 54.5) },
                { "Thrust", new ComponentValue(30.0, 41.4, 64.0) },
                { "Medical", new ComponentValue(70.0, 150.0, 265.0) },
                { "GravityGenerator", new ComponentValue(500.0, 835.0, 1145.0) },
                { "PrototechCircuitry", new ComponentValue(100.0, 18.25, 57.25) },
                { "PrototechPanel", new ComponentValue(300.0, 49.0, 71.5) },
                { "PrototechCapacitor", new ComponentValue(80.0, 26.5, 82.5) },
                { "PrototechMachinery", new ComponentValue(250.0, 68.15, 98.65) },
                { "PrototechPropulsionUnit", new ComponentValue(200.0, 94.25, 202.25) },
                { "PrototechCoolingUnit", new ComponentValue(150.0, 97.75, 227.25) },
                { "PrototechFrame", new ComponentValue(1000.0, 1000.0, 5000.0) },
            };

        private bool ApplyWeldBotchOperation(WeldOperation operation, double workScale, float positiveRawDelta)
        {
            if (operation == null || operation.Block == null || operation.Inventory == null)
                return false;

            var botchChance = GetProductionWeldBotchChance(operation);
            if (botchChance <= WeldCapTolerance || random.NextDouble() >= botchChance)
                return false;

            WeldBotchResult result;
            if (!TryApplyProductionWeldBotchDamage(operation, positiveRawDelta, out result))
                return false;

            ShowWeldBotchWarning(operation.IdentityId, operation.ResearchId, operation.Block, result);
            return true;
        }

        private double GetProductionWeldBotchChance(WeldOperation operation)
        {
            if (operation == null || operation.Block == null || operation.Block.ComponentStack == null)
                return 0.0;

            var proficiency = RatioMath.Clamp01(operation.Proficiency);
            if (proficiency >= RequiredResearchProgress - WeldCapTolerance)
                return 0.0;

            var currentIntegrityRatio = BlockCondition.GetIntegrityRatio(operation.Block);
            var functionalRatio = BlockCondition.GetFunctionalBuildRatio(operation.Block);
            if (IsAtWeldBotchCap(currentIntegrityRatio, functionalRatio, proficiency))
                return RequiredResearchProgress;

            var functionalChance = RatioMath.Clamp01((1.0 - proficiency) * GetWeldBotchBaseChanceScale());
            var functional = RatioMath.Clamp01(functionalRatio);
            if (functional >= RequiredResearchProgress - WeldCapTolerance ||
                currentIntegrityRatio < functional - WeldCapTolerance)
                return ClampWeldBotchChance(GetPreFunctionalWeldBotchChance(currentIntegrityRatio, functional, functionalChance));

            return GetBotchZoneWeldBotchChance(currentIntegrityRatio, functional, proficiency, functionalChance);
        }

        private bool IsAtWeldBotchCap(double currentIntegrityRatio, double functionalRatio, double proficiency)
        {
            if (!config.ProficiencyBuildCapEnabled)
                return false;

            var cap = GetWeldBotchCapIntegrityRatio(functionalRatio, proficiency);
            return RatioMath.Clamp01(currentIntegrityRatio) >= cap - WeldCapTolerance;
        }

        private static double GetWeldBotchCapIntegrityRatio(double functionalRatio, double proficiency)
        {
            var functional = RatioMath.Clamp01(functionalRatio);
            var proficiencyCap = RatioMath.Clamp01(proficiency);
            if (functional >= RequiredResearchProgress - WeldCapTolerance)
                return proficiencyCap;

            return functional + ((RequiredResearchProgress - functional) * proficiencyCap);
        }

        private double GetWeldBotchBaseChanceScale()
        {
            return (Math.Max(0.0, config.WeldBotchBaseChance) / 0.5) * Math.Max(0.0, config.WeldBotchChanceScale);
        }

        private double GetPreFunctionalWeldBotchChance(double currentIntegrityRatio, double functionalRatio, double functionalChance)
        {
            var current = RatioMath.Clamp01(currentIntegrityRatio);
            var functional = RatioMath.Clamp01(functionalRatio);
            if (functional <= WeldCapTolerance)
                return current > WeldCapTolerance ? functionalChance : 0.0;

            var preFunctionalProgress = RatioMath.Clamp01(current / functional);
            if (preFunctionalProgress <= 0.5 + WeldCapTolerance)
            {
                var earlyRamp = RatioMath.Clamp01(preFunctionalProgress / 0.5);
                return functionalChance * 0.02 * earlyRamp * earlyRamp;
            }

            var lateRamp = RatioMath.Clamp01((preFunctionalProgress - 0.5) / 0.5);
            return functionalChance * (0.02 + (0.98 * lateRamp));
        }

        private double GetBotchZoneWeldBotchChance(double currentIntegrityRatio, double functionalRatio, double proficiency, double functionalChance)
        {
            var functional = RatioMath.Clamp01(functionalRatio);
            if (functional >= RequiredResearchProgress - WeldCapTolerance)
                return ClampWeldBotchChance(functionalChance);

            var botchZoneProgress = RatioMath.Clamp01((RatioMath.Clamp01(currentIntegrityRatio) - functional) / (RequiredResearchProgress - functional));
            var botchZoneCap = config.ProficiencyBuildCapEnabled ? RatioMath.Clamp01(proficiency) : RequiredResearchProgress;
            if (botchZoneCap <= WeldCapTolerance || botchZoneProgress >= botchZoneCap - WeldCapTolerance)
                return RequiredResearchProgress;

            var capProgress = RatioMath.Clamp01(botchZoneProgress / botchZoneCap);
            var climb = Math.Pow(capProgress, GetBotchZoneCurveExponent());
            var chance = functionalChance + ((RequiredResearchProgress - functionalChance) * climb);
            return ClampWeldBotchChance(chance);
        }

        private double GetBotchZoneCurveExponent()
        {
            var pressure = Math.Max(0.0, (config.WeldBotchPostFunctionalPressure + config.WeldBotchSoftCapPressure) * 0.5 * Math.Max(0.0, config.WeldBotchPressureScale));
            return 1.0 / (1.0 + (pressure * 0.4));
        }

        private double ClampWeldBotchChance(double chance)
        {
            var maxChance = RatioMath.Clamp(config.WeldBotchMaxChance, 0.0, RequiredResearchProgress);
            return RatioMath.Clamp(chance, 0.0, maxChance);
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

            var beforeComponents = CaptureMountedComponentGroups(operation.Block);
            var steelPlateIntegrity = Math.Max(1f, GetSteelPlateIntegrity(beforeComponents));
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
            var componentLosses = BuildMountedComponentLosses(beforeComponents, CaptureMountedComponentGroups(operation.Block));
            var debtUpdate = UpdateWeldBotchRawDamageDebt(
                operation,
                debtKey,
                debtBefore,
                baseRawLoss,
                actualRawLoss,
                steelPlateIntegrity,
                componentLosses,
                beforeComponents);
            result.LostComponentsText = FormatMountedComponentLosses(CombineMountedComponentLosses(componentLosses, debtUpdate.InventoryLosses));
            result.RecoveredComponentsText = ApplyMountedComponentForgiveness(operation, componentLosses, actualRawLoss, beforeComponents);
            return true;
        }

        private static float GetMinimumBotchRemainingIntegrity(float maxIntegrity)
        {
            return Math.Max(1f, Math.Max(0f, maxIntegrity) * 0.01f);
        }

        private static float GetGovernedWeldBotchPhysicalRawLoss(WeldOperation operation, float plannedRawLoss, float baseRawLoss)
        {
            if (operation == null || operation.Block == null || operation.Block.ComponentStack == null || plannedRawLoss <= WeldCapTolerance)
                return 0f;

            var maxIntegrity = operation.Block.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= WeldCapTolerance)
                return plannedRawLoss;

            var currentIntegrity = operation.Block.ComponentStack.Integrity;
            var capRatio = GetWeldBotchCapIntegrityRatio(
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

            if (CountMountedComponentLosses(componentLosses) > 0)
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

                AddMountedComponentRecovery(
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

            candidates.Sort(CompareMountedComponentDebtPriority);
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
