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

    }
}
