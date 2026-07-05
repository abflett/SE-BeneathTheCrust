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

        private void ShowWeldBotchWarning(long identityId, string researchId, VRage.Game.ModAPI.IMySlimBlock slimBlock, WeldBotchResult result)
        {
            if (identityId == 0)
                return;

            Vector3D soundPosition;
            slimBlock.ComputeWorldCenter(out soundPosition);
            if (config.WeldBotchSoundEnabled)
                QueueLocalSoundAtPositionToNearbyPlayers(soundPosition, config.WeldBotchSoundSubtype, config.WeldBotchSoundRange);

            var key = GetWeldBotchWarningKey(identityId, researchId, slimBlock);
            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(identityId)) * 60.0);
            long lastShownTick;
            if (!string.IsNullOrWhiteSpace(key) &&
                warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            QueueWeldBotchWarning(identityId, researchId, slimBlock, result);
        }

        private void QueueWeldBotchWarning(long identityId, string researchId, IMySlimBlock slimBlock, WeldBotchResult result)
        {
            if (identityId == 0 || slimBlock == null || slimBlock.ComponentStack == null)
                return;

            var key = GetWeldBotchWarningKey(identityId, researchId, slimBlock);
            if (string.IsNullOrWhiteSpace(key))
                return;

            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(identityId)) * 60.0);
            long lastShownTick;
            if (warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            WeldBotchNotificationAccumulator accumulator;
            if (!blockWorkState.PendingWeldBotchWarningsByKey.TryGetValue(key, out accumulator))
            {
                accumulator = new WeldBotchNotificationAccumulator
                {
                    IdentityId = identityId,
                    ResearchId = researchId,
                    BlockDisplayName = GetWeldBotchBlockDisplayName(slimBlock),
                    MaxIntegrity = Math.Max(0f, slimBlock.ComponentStack.MaxIntegrity),
                };
                blockWorkState.PendingWeldBotchWarningsByKey[key] = accumulator;
            }

            accumulator.DamageAmount += Math.Max(0f, result.DamageAmount);
            if (accumulator.MaxIntegrity <= WeldCapTolerance)
                accumulator.MaxIntegrity = Math.Max(0f, slimBlock.ComponentStack.MaxIntegrity);

            AddFormattedComponentSummary(accumulator.LostComponentsByName, result.LostComponentsText);
            AddFormattedComponentSummary(accumulator.RecoveredComponentsByName, result.RecoveredComponentsText);
            accumulator.LastTick = simulationTick;
        }

        private void FlushReadyWeldBotchWarnings()
        {
            if (blockWorkState.PendingWeldBotchWarningsByKey.Count == 0)
                return;

            var delayTicks = GetNotificationDelayTicks();
            var readyKeys = new List<string>();
            foreach (var entry in blockWorkState.PendingWeldBotchWarningsByKey)
            {
                if (simulationTick - entry.Value.LastTick >= delayTicks)
                    readyKeys.Add(entry.Key);
            }

            for (var i = 0; i < readyKeys.Count; i++)
            {
                var key = readyKeys[i];
                WeldBotchNotificationAccumulator accumulator;
                if (!blockWorkState.PendingWeldBotchWarningsByKey.TryGetValue(key, out accumulator))
                    continue;

                blockWorkState.PendingWeldBotchWarningsByKey.Remove(key);
                ShowReadyWeldBotchWarning(key, accumulator);
            }
        }

        private void ShowReadyWeldBotchWarning(string key, WeldBotchNotificationAccumulator accumulator)
        {
            if (accumulator == null || accumulator.IdentityId == 0)
                return;

            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(accumulator.IdentityId)) * 60.0);
            long lastShownTick;
            if (!string.IsNullOrWhiteSpace(key) &&
                warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(key))
                weldBotchWarningLastShownByKey[key] = simulationTick;

            var blockDisplayName = string.IsNullOrWhiteSpace(accumulator.BlockDisplayName) ? "Block" : accumulator.BlockDisplayName;
            var maxIntegrity = accumulator.MaxIntegrity <= WeldCapTolerance ? 1f : accumulator.MaxIntegrity;
            var knockedBack = FormatProgressDelta(accumulator.DamageAmount / maxIntegrity);
            var chatMessage = "Work lost: -" + knockedBack;

            var lostText = FormatComponentSummary(accumulator.LostComponentsByName);
            if (!string.IsNullOrWhiteSpace(lostText))
                chatMessage += "\nLost: " + lostText;

            var recoveredText = FormatComponentSummary(accumulator.RecoveredComponentsByName);
            if (!string.IsNullOrWhiteSpace(recoveredText))
                chatMessage += "\nRecovered: " + recoveredText;

            ShowWkTargetColoredChatLine(
                accumulator.IdentityId,
                blockDisplayName + " Build Failure",
                chatMessage,
                WkChatWarningColor,
                false);
            SendLocalFeedback(
                accumulator.IdentityId,
                blockDisplayName + " Build Failed -" + knockedBack,
                WkChatWarningFont,
                IsPlayerProgressToastEnabled(accumulator.IdentityId),
                null,
                false);
        }

        private static void AddFormattedComponentSummary(Dictionary<string, int> componentsByName, string formattedText)
        {
            if (componentsByName == null || string.IsNullOrWhiteSpace(formattedText))
                return;

            var parts = formattedText.Split(',');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i] == null ? string.Empty : parts[i].Trim();
                if (part.Length == 0)
                    continue;

                var marker = part.LastIndexOf(" x", StringComparison.OrdinalIgnoreCase);
                if (marker <= 0 || marker + 2 >= part.Length)
                    continue;

                var name = part.Substring(0, marker).Trim();
                var countText = part.Substring(marker + 2).Trim();
                int count;
                if (string.IsNullOrWhiteSpace(name) || !int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) || count <= 0)
                    continue;

                int existing;
                if (componentsByName.TryGetValue(name, out existing))
                    componentsByName[name] = existing + count;
                else
                    componentsByName.Add(name, count);
            }
        }

        private static string FormatComponentSummary(Dictionary<string, int> componentsByName)
        {
            if (componentsByName == null || componentsByName.Count == 0)
                return null;

            var builder = new StringBuilder();
            foreach (var entry in componentsByName)
            {
                if (entry.Value <= 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(entry.Key);
                builder.Append(" x");
                builder.Append(entry.Value.ToString(CultureInfo.InvariantCulture));
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        private struct MountedComponentGroupSnapshot
        {
            public string Key;
            public MyDefinitionId DefinitionId;
            public string DisplayName;
            public int MountedCount;
            public float ComponentMaxIntegrity;
            public float ComponentMass;
        }

        private sealed class MountedComponentLoss
        {
            public string Key;
            public MyDefinitionId DefinitionId;
            public string DisplayName;
            public int LostCount;
            public float ComponentMaxIntegrity;
            public float ComponentMass;
        }

        private sealed class MountedComponentAggregate
        {
            public string Key;
            public MyDefinitionId DefinitionId;
            public string DisplayName;
            public int MountedCount;
            public float ComponentMaxIntegrity;
            public float ComponentMass;
        }

        private sealed class ComponentValue
        {
            public readonly double MaxIntegrity;
            public readonly double RawIngotCost;
            public readonly double WeightedMaterialValue;
            public readonly double RecoveryValueDensity;

            public ComponentValue(double maxIntegrity, double rawIngotCost, double weightedMaterialValue)
            {
                MaxIntegrity = Math.Max(0.0, maxIntegrity);
                RawIngotCost = Math.Max(0.0, rawIngotCost);
                WeightedMaterialValue = Math.Max(0.0, weightedMaterialValue);
                RecoveryValueDensity = MaxIntegrity > 0.0 ? WeightedMaterialValue / MaxIntegrity : WeightedMaterialValue;
            }
        }

        private static List<MountedComponentGroupSnapshot> CaptureMountedComponentGroups(IMySlimBlock slimBlock)
        {
            var groups = new List<MountedComponentGroupSnapshot>();
            var componentStack = slimBlock == null ? null : slimBlock.ComponentStack;
            if (componentStack == null || componentStack.GroupCount <= 0)
                return groups;

            for (var i = 0; i < componentStack.GroupCount; i++)
            {
                var info = componentStack.GetComponentStackInfo(i);
                groups.Add(new MountedComponentGroupSnapshot
                {
                    Key = info.DefinitionId.ToString(),
                    DefinitionId = info.DefinitionId,
                    DisplayName = GetComponentDisplayName(info),
                    MountedCount = Math.Max(0, info.MountedCount),
                    ComponentMaxIntegrity = GetComponentMaxIntegrity(slimBlock, info.DefinitionId, info),
                    ComponentMass = GetComponentMass(info.DefinitionId),
                });
            }

            return groups;
        }

        private static List<MountedComponentLoss> BuildMountedComponentLosses(List<MountedComponentGroupSnapshot> beforeGroups, List<MountedComponentGroupSnapshot> afterGroups)
        {
            var losses = new List<MountedComponentLoss>();
            if (beforeGroups == null || beforeGroups.Count == 0)
                return losses;

            var keys = new List<string>();
            var beforeByKey = new Dictionary<string, MountedComponentAggregate>(StringComparer.OrdinalIgnoreCase);
            var afterByKey = new Dictionary<string, MountedComponentAggregate>(StringComparer.OrdinalIgnoreCase);

            AddMountedComponentGroups(beforeGroups, keys, beforeByKey);
            AddMountedComponentGroups(afterGroups, null, afterByKey);

            for (var i = 0; i < keys.Count; i++)
            {
                var key = keys[i];
                MountedComponentAggregate before;
                MountedComponentAggregate after;
                beforeByKey.TryGetValue(key, out before);
                afterByKey.TryGetValue(key, out after);

                var beforeCount = before == null ? 0 : before.MountedCount;
                var afterCount = after == null ? 0 : after.MountedCount;
                var lostCount = beforeCount - afterCount;
                if (lostCount <= 0)
                    continue;

                losses.Add(new MountedComponentLoss
                {
                    Key = before.Key,
                    DefinitionId = before.DefinitionId,
                    DisplayName = before.DisplayName,
                    LostCount = lostCount,
                    ComponentMaxIntegrity = before.ComponentMaxIntegrity,
                    ComponentMass = before.ComponentMass,
                });
            }

            return losses;
        }

        private static void AddMountedComponentGroups(
            List<MountedComponentGroupSnapshot> groups,
            List<string> orderedKeys,
            Dictionary<string, MountedComponentAggregate> groupsByKey)
        {
            if (groups == null || groupsByKey == null)
                return;

            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                if (string.IsNullOrWhiteSpace(group.Key) || group.MountedCount <= 0)
                    continue;

                MountedComponentAggregate existing;
                if (groupsByKey.TryGetValue(group.Key, out existing))
                {
                    existing.MountedCount += group.MountedCount;
                    if (existing.ComponentMaxIntegrity <= 0f && group.ComponentMaxIntegrity > 0f)
                        existing.ComponentMaxIntegrity = group.ComponentMaxIntegrity;

                    if (existing.ComponentMass <= 0f && group.ComponentMass > 0f)
                        existing.ComponentMass = group.ComponentMass;

                    continue;
                }

                groupsByKey.Add(group.Key, new MountedComponentAggregate
                {
                    Key = group.Key,
                    DefinitionId = group.DefinitionId,
                    DisplayName = group.DisplayName,
                    MountedCount = group.MountedCount,
                    ComponentMaxIntegrity = group.ComponentMaxIntegrity,
                    ComponentMass = group.ComponentMass,
                });

                if (orderedKeys != null)
                    orderedKeys.Add(group.Key);
            }
        }

        private string ApplyMountedComponentForgiveness(
            WeldOperation operation,
            List<MountedComponentLoss> losses,
            float actualRawLoss,
            List<MountedComponentGroupSnapshot> beforeGroups)
        {
            if (operation == null || operation.Inventory == null || losses == null || losses.Count == 0 || actualRawLoss <= WeldCapTolerance)
                return null;

            var totalLostCount = CountMountedComponentLosses(losses);
            if (totalLostCount <= 1)
                return null;

            var steelPlateIntegrity = Math.Max(1f, GetSteelPlateIntegrity(beforeGroups));
            var forgivenessScale = Math.Max(0.01, config == null ? 1.0 : config.WeldBotchForgivenessScale);
            var permanentLossCount = Math.Max(1, (int)Math.Ceiling(actualRawLoss / (steelPlateIntegrity * forgivenessScale)));
            var recoveryBudget = totalLostCount - permanentLossCount;
            if (recoveryBudget <= 0)
                return null;

            var sortedLosses = new List<MountedComponentLoss>(losses);
            sortedLosses.Sort(CompareMountedComponentForgivenessPriority);

            var recovered = new List<MountedComponentLoss>();
            for (var i = 0; i < sortedLosses.Count && recoveryBudget > 0; i++)
            {
                var loss = sortedLosses[i];
                if (loss == null || loss.LostCount <= 0)
                    continue;

                var recoverCount = Math.Min(loss.LostCount, recoveryBudget);
                if (recoverCount <= 0)
                    continue;

                var subtypeName = loss.DefinitionId.SubtypeName;
                if (string.IsNullOrWhiteSpace(subtypeName))
                    continue;

                ReturnForgivenComponents(operation, loss.DefinitionId, recoverCount);
                AddMountedComponentRecovery(recovered, loss, recoverCount);
                recoveryBudget -= recoverCount;
            }

            return FormatMountedComponentLosses(recovered);
        }

        private static void ReturnForgivenComponents(WeldOperation operation, MyDefinitionId definitionId, int count)
        {
            if (operation == null || count <= 0)
                return;

            var amount = (MyFixedPoint)count;
            if (TryReturnComponentsToInventory(operation, definitionId, amount))
                return;

            if (operation.Block != null && operation.Inventory != null)
                operation.Block.MoveItemsToConstructionStockpile(operation.Inventory);

            if (TryReturnComponentsToInventory(operation, definitionId, amount))
                return;

            SpawnReturnedComponentsNearBlock(operation, definitionId, amount);
        }

        private static bool TryReturnComponentsToInventory(WeldOperation operation, MyDefinitionId definitionId, MyFixedPoint amount)
        {
            if (operation == null || operation.Inventory == null || amount <= 0)
                return false;

            var subtypeName = definitionId.SubtypeName;
            if (string.IsNullOrWhiteSpace(subtypeName))
                return false;

            return operation.Inventory.AddItems(
                amount,
                MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(subtypeName));
        }

        private static void SpawnReturnedComponentsNearBlock(WeldOperation operation, MyDefinitionId definitionId, MyFixedPoint amount)
        {
            if (amount <= 0)
                return;

            var subtypeName = definitionId.SubtypeName;
            if (string.IsNullOrWhiteSpace(subtypeName))
                return;

            var position = GetReturnedComponentWorldFallbackPosition(operation);

            var item = new MyPhysicalInventoryItem(
                amount,
                MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Component>(subtypeName),
                1f);

            MyFloatingObjects.Spawn(item, position, Vector3D.Forward, Vector3D.Up, null, null);
        }

        private static Vector3D GetReturnedComponentWorldFallbackPosition(WeldOperation operation)
        {
            var slimBlock = operation == null ? null : operation.Block;

            var blockCenter = Vector3D.Zero;
            if (slimBlock != null)
                slimBlock.ComputeWorldCenter(out blockCenter);

            Vector3D sourcePosition;
            var direction = TryGetWeldSourcePosition(operation, out sourcePosition)
                ? sourcePosition - blockCenter
                : GetReturnedComponentFallbackDirection(slimBlock);

            if (direction.LengthSquared() < 0.0001)
                direction = GetReturnedComponentFallbackDirection(slimBlock);

            if (direction.LengthSquared() < 0.0001)
                direction = Vector3D.Up;

            direction.Normalize();

            var gridSize = slimBlock != null && slimBlock.CubeGrid != null ? slimBlock.CubeGrid.GridSize : 1.0f;
            var clearance = Math.Max(gridSize, GetSlimBlockWorldRadius(slimBlock) + Math.Max(0.75, gridSize * 0.25));
            return blockCenter + (direction * clearance);
        }

        private static bool TryGetWeldSourcePosition(WeldOperation operation, out Vector3D position)
        {
            position = Vector3D.Zero;
            if (operation == null)
                return false;

            if (operation.AttackerId != 0 && MyAPIGateway.Entities != null)
            {
                IMyEntity sourceEntity;
                if (MyAPIGateway.Entities.TryGetEntityById(operation.AttackerId, out sourceEntity) && sourceEntity != null)
                {
                    position = sourceEntity.WorldMatrix.Translation;
                    return true;
                }
            }

            if (operation.IdentityId != 0 && MyAPIGateway.Players != null)
            {
                var players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId == operation.IdentityId);
                if (players.Count > 0 && players[0] != null && players[0].Character != null)
                {
                    position = players[0].Character.WorldMatrix.Translation;
                    return true;
                }
            }

            return false;
        }

        private static Vector3D GetReturnedComponentFallbackDirection(IMySlimBlock slimBlock)
        {
            if (slimBlock != null && slimBlock.CubeGrid != null)
                return slimBlock.CubeGrid.WorldMatrix.Up;

            return Vector3D.Up;
        }

        private static void AddMountedComponentRecovery(List<MountedComponentLoss> recovered, MountedComponentLoss source, int count)
        {
            if (recovered == null || source == null || count <= 0)
                return;

            for (var i = 0; i < recovered.Count; i++)
            {
                var existing = recovered[i];
                if (existing == null || !string.Equals(existing.Key, source.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                existing.LostCount += count;
                return;
            }

            recovered.Add(new MountedComponentLoss
            {
                Key = source.Key,
                DefinitionId = source.DefinitionId,
                DisplayName = source.DisplayName,
                LostCount = count,
                ComponentMaxIntegrity = source.ComponentMaxIntegrity,
                ComponentMass = source.ComponentMass,
            });
        }

        private static int CountMountedComponentLosses(List<MountedComponentLoss> losses)
        {
            var total = 0;
            if (losses == null)
                return total;

            for (var i = 0; i < losses.Count; i++)
            {
                var loss = losses[i];
                if (loss != null && loss.LostCount > 0)
                    total += loss.LostCount;
            }

            return total;
        }

        private static List<MountedComponentLoss> CombineMountedComponentLosses(
            List<MountedComponentLoss> first,
            List<MountedComponentLoss> second)
        {
            if ((first == null || first.Count == 0) && (second == null || second.Count == 0))
                return null;

            var combined = new List<MountedComponentLoss>();
            AddMountedComponentLossRange(combined, first);
            AddMountedComponentLossRange(combined, second);
            return combined;
        }

        private static void AddMountedComponentLossRange(List<MountedComponentLoss> target, List<MountedComponentLoss> source)
        {
            if (target == null || source == null)
                return;

            for (var i = 0; i < source.Count; i++)
            {
                var loss = source[i];
                if (loss == null || loss.LostCount <= 0)
                    continue;

                AddMountedComponentRecovery(target, loss, loss.LostCount);
            }
        }

        private static int CompareMountedComponentForgivenessPriority(MountedComponentLoss left, MountedComponentLoss right)
        {
            if (ReferenceEquals(left, right))
                return 0;

            if (left == null)
                return 1;

            if (right == null)
                return -1;

            var recoveryValueCompare = GetComponentRecoveryValue(right).CompareTo(GetComponentRecoveryValue(left));
            if (recoveryValueCompare != 0)
                return recoveryValueCompare;

            var weightedValueCompare = GetComponentWeightedMaterialValue(right).CompareTo(GetComponentWeightedMaterialValue(left));
            if (weightedValueCompare != 0)
                return weightedValueCompare;

            var rawValueCompare = GetComponentRawIngotCost(right).CompareTo(GetComponentRawIngotCost(left));
            if (rawValueCompare != 0)
                return rawValueCompare;

            var massCompare = right.ComponentMass.CompareTo(left.ComponentMass);
            if (massCompare != 0)
                return massCompare;

            var integrityCompare = GetForgivenessIntegritySortValue(left).CompareTo(GetForgivenessIntegritySortValue(right));
            if (integrityCompare != 0)
                return integrityCompare;

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareMountedComponentDebtPriority(MountedComponentGroupSnapshot left, MountedComponentGroupSnapshot right)
        {
            var weightedValueCompare = GetComponentWeightedMaterialValue(left).CompareTo(GetComponentWeightedMaterialValue(right));
            if (weightedValueCompare != 0)
                return weightedValueCompare;

            var rawValueCompare = GetComponentRawIngotCost(left).CompareTo(GetComponentRawIngotCost(right));
            if (rawValueCompare != 0)
                return rawValueCompare;

            var massCompare = left.ComponentMass.CompareTo(right.ComponentMass);
            if (massCompare != 0)
                return massCompare;

            var integrityCompare = GetDebtIntegritySortValue(left).CompareTo(GetDebtIntegritySortValue(right));
            if (integrityCompare != 0)
                return integrityCompare;

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
        }

        private static float GetForgivenessIntegritySortValue(MountedComponentLoss loss)
        {
            if (loss == null || loss.ComponentMaxIntegrity <= 0f)
                return float.MaxValue;

            return loss.ComponentMaxIntegrity;
        }

        private static float GetDebtIntegritySortValue(MountedComponentGroupSnapshot group)
        {
            return group.ComponentMaxIntegrity <= 0f ? float.MaxValue : group.ComponentMaxIntegrity;
        }

        private static double GetComponentRecoveryValue(MountedComponentLoss loss)
        {
            if (loss == null)
                return 0.0;

            ComponentValue value;
            if (TryGetComponentValue(loss, out value))
                return value.RecoveryValueDensity;

            if (loss.ComponentMaxIntegrity <= 0f)
                return Math.Max(0f, loss.ComponentMass);

            return Math.Max(0f, loss.ComponentMass) / loss.ComponentMaxIntegrity;
        }

        private static double GetComponentWeightedMaterialValue(MountedComponentLoss loss)
        {
            if (loss == null)
                return 0.0;

            ComponentValue value;
            if (TryGetComponentValue(loss, out value))
                return value.WeightedMaterialValue;

            return Math.Max(0f, loss.ComponentMass);
        }

        private static double GetComponentWeightedMaterialValue(MountedComponentGroupSnapshot group)
        {
            ComponentValue value;
            if (TryGetComponentValue(group.DefinitionId, out value))
                return value.WeightedMaterialValue;

            return Math.Max(0f, group.ComponentMass);
        }

        private static double GetComponentRawIngotCost(MountedComponentLoss loss)
        {
            if (loss == null)
                return 0.0;

            ComponentValue value;
            if (TryGetComponentValue(loss, out value))
                return value.RawIngotCost;

            return Math.Max(0f, loss.ComponentMass);
        }

        private static double GetComponentRawIngotCost(MountedComponentGroupSnapshot group)
        {
            ComponentValue value;
            if (TryGetComponentValue(group.DefinitionId, out value))
                return value.RawIngotCost;

            return Math.Max(0f, group.ComponentMass);
        }

        private static bool TryGetComponentValue(MountedComponentLoss loss, out ComponentValue value)
        {
            value = null;
            if (loss == null || string.IsNullOrWhiteSpace(loss.DefinitionId.SubtypeName))
                return false;

            return TryGetComponentValue(loss.DefinitionId, out value);
        }

        private static bool TryGetComponentValue(MyDefinitionId definitionId, out ComponentValue value)
        {
            value = null;
            if (string.IsNullOrWhiteSpace(definitionId.SubtypeName))
                return false;

            return WeldBotchComponentValues.TryGetValue(definitionId.SubtypeName, out value);
        }

        private static string FormatMountedComponentLosses(List<MountedComponentLoss> losses)
        {
            if (losses == null || losses.Count == 0)
                return null;

            var builder = new StringBuilder();
            for (var i = 0; i < losses.Count; i++)
            {
                var loss = losses[i];
                if (loss == null || loss.LostCount <= 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                var name = string.IsNullOrWhiteSpace(loss.DisplayName) ? "Component" : loss.DisplayName;
                builder.Append(name);
                builder.Append(" x");
                builder.Append(loss.LostCount.ToString(CultureInfo.InvariantCulture));
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

        private static float GetSteelPlateIntegrity(List<MountedComponentGroupSnapshot> groups)
        {
            if (groups != null)
            {
                for (var i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
                    if (string.Equals(group.DefinitionId.SubtypeName, SteelPlateSubtypeName, StringComparison.OrdinalIgnoreCase) &&
                        group.ComponentMaxIntegrity > 0f)
                    {
                        return group.ComponentMaxIntegrity;
                    }
                }
            }

            return FallbackSteelPlateIntegrity;
        }

        private static float GetComponentMaxIntegrity(IMySlimBlock slimBlock, MyDefinitionId definitionId, MyComponentStackInfo stackInfo)
        {
            var blockDefinition = slimBlock == null ? null : slimBlock.BlockDefinition as MyCubeBlockDefinition;
            if (blockDefinition != null && blockDefinition.Components != null)
            {
                for (var i = 0; i < blockDefinition.Components.Length; i++)
                {
                    var component = blockDefinition.Components[i];
                    if (component.Definition == null)
                        continue;

                    if (component.Definition.Id.Equals(definitionId))
                        return Math.Max(0f, component.Definition.MaxIntegrity);
                }
            }

            var fallback = Math.Max(0f, stackInfo.MaxIntegrity);
            if (fallback <= 0f)
                return 0f;

            var totalCount = Math.Max(1, stackInfo.TotalCount);
            return fallback / totalCount;
        }

        private static float GetComponentMass(MyDefinitionId definitionId)
        {
            var definition = MyDefinitionManager.Static.GetPhysicalItemDefinition(definitionId);
            return definition == null ? 0f : Math.Max(0f, definition.Mass);
        }

        private static string GetComponentDisplayName(MyComponentStackInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.ComponentName))
                return info.ComponentName;

            var subtypeName = info.DefinitionId.SubtypeName;
            if (!string.IsNullOrWhiteSpace(subtypeName))
                return HumanizeSubtypeName(subtypeName);

            return "Component";
        }

        private static string GetWeldBotchBlockDisplayName(VRage.Game.ModAPI.IMySlimBlock slimBlock)
        {
            var definition = slimBlock == null ? null : slimBlock.BlockDefinition as MyCubeBlockDefinition;
            if (definition == null)
                return "Block";

            if (!string.IsNullOrWhiteSpace(definition.DisplayNameText))
                return definition.DisplayNameText;

            return HumanizeSubtypeName(definition.Id.SubtypeName);
        }

        private static string HumanizeSubtypeName(string subtypeName)
        {
            if (string.IsNullOrWhiteSpace(subtypeName))
                return "Block";

            var builder = new StringBuilder();
            for (var i = 0; i < subtypeName.Length; i++)
            {
                var c = subtypeName[i];
                if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(subtypeName[i - 1]) && !char.IsUpper(subtypeName[i - 1]))
                    builder.Append(' ');

                builder.Append(c == '_' ? ' ' : c);
            }

            return builder.ToString().Trim();
        }

        private static string GetWeldBotchWarningKey(long identityId, string researchId, VRage.Game.ModAPI.IMySlimBlock slimBlock)
        {
            return identityId.ToString(CultureInfo.InvariantCulture) + ":" +
                   (string.IsNullOrWhiteSpace(researchId) ? "unknown" : researchId) + ":" +
                   (GetWeldBlockKey(slimBlock) ?? "unknown");
        }
    }
}
