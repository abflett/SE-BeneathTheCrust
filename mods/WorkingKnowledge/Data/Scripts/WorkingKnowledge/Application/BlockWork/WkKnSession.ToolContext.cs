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
        private struct WeldAttributionCandidate
        {
            public readonly MyInventory Inventory;
            public readonly long IdentityId;
            public readonly long SourceEntityId;
            public readonly double Distance;

            public WeldAttributionCandidate(MyInventory inventory, long identityId, long sourceEntityId, double distance)
            {
                Inventory = inventory;
                IdentityId = identityId;
                SourceEntityId = sourceEntityId;
                Distance = distance;
            }

            public bool IsValid
            {
                get
                {
                    return Inventory != null &&
                           IdentityId != 0 &&
                           SourceEntityId != 0 &&
                           Distance < double.MaxValue;
                }
            }
        }

        private bool TryResolveActiveWeldContextForBlock(IMySlimBlock slimBlock, out MyInventory inventory, out long identityId, out long sourceEntityId)
        {
            inventory = null;
            identityId = 0;
            sourceEntityId = 0;

            if (slimBlock == null || slimBlock.CubeGrid == null)
                return false;

            VRageMath.Vector3D targetCenter;
            slimBlock.ComputeWorldCenter(out targetCenter);

            VRageMath.BoundingBoxD targetBox;
            slimBlock.GetWorldBoundingBox(out targetBox, true);

            var targetRadius = GetSlimBlockWorldRadius(slimBlock);
            var best = new WeldAttributionCandidate();
            TryConsiderNearestHandWelderContextForBlock(targetBox, ref best);
            TryConsiderNearestShipWelderContextForBlock(slimBlock, targetCenter, targetBox, targetRadius, ref best);

            if (!best.IsValid)
                return false;

            inventory = best.Inventory;
            identityId = best.IdentityId;
            sourceEntityId = best.SourceEntityId;
            return true;
        }

        private void TryConsiderNearestHandWelderContextForBlock(
            VRageMath.BoundingBoxD targetBox,
            ref WeldAttributionCandidate best)
        {
            if (MyAPIGateway.Players == null)
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.Character == null || player.IdentityId == 0)
                    continue;

                var handWelder = player.Character.EquippedTool as IMyWelder;
                if (handWelder == null)
                    continue;

                var candidateInventory = player.Character.GetInventory() as MyInventory;
                if (candidateInventory == null)
                    continue;

                var distance = Math.Sqrt(GetDistanceSquaredToBox(player.Character.WorldMatrix.Translation, targetBox));
                if (distance > ActiveHandWelderRange + ShipWelderAttributionPadding)
                    continue;

                TryAcceptWeldAttributionCandidate(
                    new WeldAttributionCandidate(
                        candidateInventory,
                        player.IdentityId,
                        handWelder.EntityId,
                        distance),
                    ref best);
            }
        }

        private void TryConsiderNearestShipWelderContextForBlock(
            IMySlimBlock slimBlock,
            VRageMath.Vector3D targetCenter,
            VRageMath.BoundingBoxD targetBox,
            double targetRadius,
            ref WeldAttributionCandidate best)
        {
            if (slimBlock == null || slimBlock.CubeGrid == null || MyAPIGateway.Entities == null)
                return;

            var searchSphere = new VRageMath.BoundingSphereD(targetCenter, targetRadius + ShipWelderAttributionSearchRadius);
            var entities = MyAPIGateway.Entities.GetEntitiesInSphere(ref searchSphere);
            if (entities == null || entities.Count == 0)
                return;

            var shipWelderBlocks = new List<IMySlimBlock>();

            foreach (var entity in entities)
            {
                var grid = entity as IMyCubeGrid;
                if (grid == null)
                    continue;

                shipWelderBlocks.Clear();
                grid.GetBlocks(shipWelderBlocks, block => block != null && block.FatBlock is IMyShipWelder);

                for (var i = 0; i < shipWelderBlocks.Count; i++)
                {
                    var shipWelder = shipWelderBlocks[i].FatBlock as IMyShipWelder;
                    var toolBlock = shipWelder as IMyCubeBlock;
                    var shipTool = shipWelder as IMyShipToolBase;
                    if (shipWelder == null || toolBlock == null || shipTool == null)
                        continue;

                    if (!shipTool.IsActivated)
                        continue;

                    var candidateIdentityId = ResolveShipToolOperator(toolBlock);
                    if (candidateIdentityId == 0)
                        continue;

                    var candidateInventory = shipWelder.GetInventory() as MyInventory;
                    if (candidateInventory == null)
                        continue;

                    var sensorRadius = GetShipWelderSensorRadius(toolBlock);
                    var distance = GetDistanceBetweenSlimBlockAndBox(shipWelderBlocks[i], targetBox);
                    if (distance > sensorRadius + ShipWelderAttributionPadding)
                        continue;

                    TryAcceptWeldAttributionCandidate(
                        new WeldAttributionCandidate(
                            candidateInventory,
                            candidateIdentityId,
                            toolBlock.EntityId,
                            distance),
                        ref best);
                }
            }
        }

        private static void TryAcceptWeldAttributionCandidate(WeldAttributionCandidate candidate, ref WeldAttributionCandidate best)
        {
            if (!candidate.IsValid)
                return;

            if (best.IsValid && candidate.Distance >= best.Distance)
                return;

            best = candidate;
        }

        private static double GetDistanceBetweenSlimBlockAndBox(IMySlimBlock toolSlimBlock, VRageMath.BoundingBoxD targetBox)
        {
            if (toolSlimBlock == null)
                return double.MaxValue;

            VRageMath.BoundingBoxD toolBox;
            toolSlimBlock.GetWorldBoundingBox(out toolBox, true);
            return Math.Sqrt(GetDistanceSquaredBetweenBoxes(toolBox, targetBox));
        }

        private static double GetDistanceSquaredToBox(VRageMath.Vector3D point, VRageMath.BoundingBoxD box)
        {
            var dx = GetAxisDistance(point.X, box.Min.X, box.Max.X);
            var dy = GetAxisDistance(point.Y, box.Min.Y, box.Max.Y);
            var dz = GetAxisDistance(point.Z, box.Min.Z, box.Max.Z);
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        private static double GetAxisDistance(double value, double min, double max)
        {
            if (value < min)
                return min - value;

            if (value > max)
                return value - max;

            return 0.0;
        }

        private static double GetDistanceSquaredBetweenBoxes(VRageMath.BoundingBoxD left, VRageMath.BoundingBoxD right)
        {
            var dx = GetBoxAxisDistance(left.Min.X, left.Max.X, right.Min.X, right.Max.X);
            var dy = GetBoxAxisDistance(left.Min.Y, left.Max.Y, right.Min.Y, right.Max.Y);
            var dz = GetBoxAxisDistance(left.Min.Z, left.Max.Z, right.Min.Z, right.Max.Z);
            return (dx * dx) + (dy * dy) + (dz * dz);
        }

        private static double GetBoxAxisDistance(double leftMin, double leftMax, double rightMin, double rightMax)
        {
            if (leftMax < rightMin)
                return rightMin - leftMax;

            if (rightMax < leftMin)
                return leftMin - rightMax;

            return 0.0;
        }

        private static double GetShipWelderSensorRadius(IMyCubeBlock toolBlock)
        {
            if (toolBlock == null || MyDefinitionManager.Static == null)
                return ShipWelderFallbackSensorRadius;

            var definition = MyDefinitionManager.Static.GetCubeBlockDefinition(toolBlock.BlockDefinition) as MyShipWelderDefinition;
            if (definition == null || definition.SensorRadius <= 0f)
                return ShipWelderFallbackSensorRadius;

            return definition.SensorRadius;
        }

        private static double GetSlimBlockWorldRadius(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.CubeGrid == null)
                return 0.0;

            var x = Math.Max(1, slimBlock.Max.X - slimBlock.Min.X + 1);
            var y = Math.Max(1, slimBlock.Max.Y - slimBlock.Min.Y + 1);
            var z = Math.Max(1, slimBlock.Max.Z - slimBlock.Min.Z + 1);
            return slimBlock.CubeGrid.GridSize * Math.Sqrt((x * x) + (y * y) + (z * z)) * 0.5;
        }

        private bool TryResolveGrindContext(long attackerId, out MyInventory outputInventory, out long operatorIdentityId)
        {
            outputInventory = null;
            operatorIdentityId = 0;

            var attacker = MyEntities.GetEntityById(attackerId);
            var handGrinder = attacker as IMyAngleGrinder;
            if (handGrinder != null)
            {
                operatorIdentityId = handGrinder.OwnerIdentityId;
                var player = FindPlayerByIdentity(operatorIdentityId);
                outputInventory = player != null && player.Character != null ? player.Character.GetInventory() as MyInventory : null;
                return outputInventory != null;
            }

            var shipGrinder = attacker as IMyShipGrinder;
            if (shipGrinder != null)
            {
                outputInventory = shipGrinder.GetInventory() as MyInventory;
                var grinderBlock = shipGrinder as IMyCubeBlock;
                operatorIdentityId = ResolveShipToolOperator(grinderBlock);
                return outputInventory != null;
            }

            return false;
        }

        private bool TryResolveGrinderOperator(long attackerId, out long operatorIdentityId)
        {
            operatorIdentityId = 0;

            var attacker = MyEntities.GetEntityById(attackerId);
            var handGrinder = attacker as IMyAngleGrinder;
            if (handGrinder != null)
            {
                operatorIdentityId = handGrinder.OwnerIdentityId;
                return true;
            }

            var shipGrinder = attacker as IMyShipGrinder;
            if (shipGrinder != null)
            {
                var grinderBlock = shipGrinder as IMyCubeBlock;
                operatorIdentityId = ResolveShipToolOperator(grinderBlock);
                return true;
            }

            return false;
        }

    }
}
