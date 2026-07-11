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
    internal sealed class WeldBotchComponentRecoveryService
    {
        private const float FallbackSteelPlateIntegrity = 100f;
        private const string SteelPlateSubtypeName = "SteelPlate";

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

        private readonly double tolerance;

        internal WeldBotchComponentRecoveryService(double tolerance)
        {
            this.tolerance = tolerance;
        }

        internal struct MountedComponentGroupSnapshot
        {
            public string Key;
            public MyDefinitionId DefinitionId;
            public string DisplayName;
            public int MountedCount;
            public float ComponentMaxIntegrity;
            public float ComponentMass;
        }

        internal sealed class MountedComponentLoss
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

        internal static List<MountedComponentGroupSnapshot> CaptureMountedComponentGroups(IMySlimBlock slimBlock)
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

        internal static List<MountedComponentLoss> BuildMountedComponentLosses(List<MountedComponentGroupSnapshot> beforeGroups, List<MountedComponentGroupSnapshot> afterGroups)
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

        internal string ApplyMountedComponentForgiveness(
            WeldOperation operation,
            List<MountedComponentLoss> losses,
            float actualRawLoss,
            List<MountedComponentGroupSnapshot> beforeGroups,
            double forgivenessScale)
        {
            if (operation == null || operation.Inventory == null || losses == null || losses.Count == 0 || actualRawLoss <= tolerance)
                return null;

            var totalLostCount = CountMountedComponentLosses(losses);
            if (totalLostCount <= 1)
                return null;

            var steelPlateIntegrity = Math.Max(1f, GetSteelPlateIntegrity(beforeGroups));
            forgivenessScale = Math.Max(0.01, forgivenessScale);
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
            var clearance = Math.Max(gridSize, BlockGeometry.GetWorldRadius(slimBlock) + Math.Max(0.75, gridSize * 0.25));
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

        internal static void AddMountedComponentRecovery(List<MountedComponentLoss> recovered, MountedComponentLoss source, int count)
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

        internal static int CountMountedComponentLosses(List<MountedComponentLoss> losses)
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

        internal static List<MountedComponentLoss> CombineMountedComponentLosses(
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

        internal static int CompareMountedComponentDebtPriority(MountedComponentGroupSnapshot left, MountedComponentGroupSnapshot right)
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

        internal static string FormatMountedComponentLosses(List<MountedComponentLoss> losses)
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

        internal static float GetSteelPlateIntegrity(List<MountedComponentGroupSnapshot> groups)
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

        internal static string HumanizeSubtypeName(string subtypeName)
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

    }
}
