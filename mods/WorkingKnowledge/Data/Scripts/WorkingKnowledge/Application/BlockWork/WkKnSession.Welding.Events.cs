using System;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void UpdateBeforeWeldSimulation()
        {
            if (ShouldRunBlockIntegrityTracking())
            {
                blockIntegrityMonitor.Update(
                    simulationTick,
                    BlockIntegrityMaxGridScansPerTick,
                    BlockIntegrityScanDiscoveryIntervalTicks,
                    BlockIntegrityScanDiscoveryRadius);
            }
        }

        private void UpdateAfterWeldSimulation()
        {
            if (ShouldRunWeldEvents())
                FlushPendingLocalSoundsAtPositionToNearbyPlayers();
        }

        private bool ShouldRunWeldEvents()
        {
            return MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.IsServer &&
                   config.ProficiencyEnabled;
        }

        private bool ShouldRunBlockIntegrityTracking()
        {
            return MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.IsServer;
        }

        private void RegisterBlockIntegrityMonitorHandlers()
        {
            blockIntegrityMonitor.PositiveIntegrityChanged -= OnPositiveIntegrityChanged;
            blockIntegrityMonitor.PositiveIntegrityChanged += OnPositiveIntegrityChanged;
            blockIntegrityMonitor.BlockAdded -= OnBlockIntegrityMonitorBlockAdded;
            blockIntegrityMonitor.BlockAdded += OnBlockIntegrityMonitorBlockAdded;
        }

        private void UnregisterBlockIntegrityMonitorHandlers()
        {
            blockIntegrityMonitor.PositiveIntegrityChanged -= OnPositiveIntegrityChanged;
            blockIntegrityMonitor.BlockAdded -= OnBlockIntegrityMonitorBlockAdded;
        }

        private void SubscribeExistingBlockIntegrityGrids()
        {
            if (ShouldRunBlockIntegrityTracking())
                blockIntegrityMonitor.TrackExistingGrids();
        }

        private void UnsubscribeTrackedBlockIntegrityGrids()
        {
            blockIntegrityMonitor.UntrackAllGrids();
        }

        private void OnEntityAdded(IMyEntity entity)
        {
            if (ShouldRunBlockIntegrityTracking())
                blockIntegrityMonitor.TrackEntity(entity);
        }

        private void OnEntityRemoved(IMyEntity entity)
        {
            blockIntegrityMonitor.UntrackEntity(entity);
        }

        private void OnBlockIntegrityMonitorBlockAdded(IMySlimBlock slimBlock)
        {
            EnforceResearchPlacement(slimBlock);
        }

        private void OnPositiveIntegrityChanged(BlockIntegrityChange change)
        {
            if (!ShouldRunWeldEvents())
                return;

            var slimBlock = change.Block;
            if (slimBlock == null || slimBlock.BlockDefinition == null)
                return;

            ResearchUnlockTarget unlockTarget;
            if (!schematicCatalog.TryGetTargetByBlock(slimBlock.BlockDefinition.Id, out unlockTarget))
                return;

            MyInventory inventory;
            long identityId;
            long sourceEntityId;
            if (!TryResolveActiveWeldContextForBlock(slimBlock, out inventory, out identityId, out sourceEntityId))
                return;

            ApplyAttributedWeldChange(change, inventory, identityId, sourceEntityId, unlockTarget);
        }

        private void ApplyAttributedWeldChange(
            BlockIntegrityChange change,
            MyInventory inventory,
            long identityId,
            long sourceEntityId,
            ResearchUnlockTarget unlockTarget)
        {
            var slimBlock = change.Block;
            if (GetIntegrityDelta(change) <= WeldCapTolerance ||
                slimBlock == null ||
                slimBlock.BlockDefinition == null ||
                slimBlock.ComponentStack == null ||
                inventory == null ||
                identityId == 0 ||
                string.IsNullOrWhiteSpace(unlockTarget.ResearchId))
            {
                return;
            }

            var before = change.Previous;
            var operation = new WeldOperation
            {
                AttackerId = sourceEntityId,
                Block = slimBlock,
                Inventory = inventory,
                IdentityId = identityId,
                ResearchId = unlockTarget.ResearchId,
                DisplayName = unlockTarget.DisplayName,
                Proficiency = GetPlayerProficiency(identityId, unlockTarget.ResearchId),
                BeforeBuildRatio = GetSnapshotBuildRatio(before),
                BeforeIntegrityRatio = GetSnapshotIntegrityRatio(before),
                BeforeBuildIntegrity = before.BuildIntegrity,
                BeforeIntegrity = before.Integrity,
            };

            ApplyWeldOperation(operation);
        }

        private void ClearWeldRuntimeState()
        {
            UnregisterBlockIntegrityMonitorHandlers();
            blockIntegrityMonitor.UntrackAllGrids();
        }

        private static float GetIntegrityDelta(BlockIntegrityChange change)
        {
            return change.Current.Integrity - change.Previous.Integrity;
        }

        private static double GetSnapshotBuildRatio(BlockSnapshot snapshot)
        {
            if (snapshot.MaxIntegrity <= 0f)
                return 0.0;

            return RatioMath.Clamp01(snapshot.BuildIntegrity / snapshot.MaxIntegrity);
        }

        private static double GetSnapshotIntegrityRatio(BlockSnapshot snapshot)
        {
            if (snapshot.MaxIntegrity <= 0f)
                return 0.0;

            return RatioMath.Clamp01(snapshot.Integrity / snapshot.MaxIntegrity);
        }

        private static float GetWeldDamageRatio(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.ComponentStack == null)
                return 1f;

            var maxIntegrity = slimBlock.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= 0f)
                return 1f;

            var buildRatio = slimBlock.ComponentStack.BuildIntegrity / maxIntegrity;
            return (float)Math.Max(0.01, 2.0 - buildRatio);
        }
    }
}
