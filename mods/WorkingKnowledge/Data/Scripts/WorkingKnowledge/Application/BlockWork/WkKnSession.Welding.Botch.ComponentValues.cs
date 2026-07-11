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
