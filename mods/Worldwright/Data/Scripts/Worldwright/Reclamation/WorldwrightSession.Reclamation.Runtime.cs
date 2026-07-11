using System;
using System.Collections.Generic;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        internal const float MaximumOutwardVelocity = 100f;
        internal const float MinimumAutomaticIntervalSeconds = 0.1f;
        internal const float MaximumAutomaticIntervalSeconds = 60f;

        private const string BlockSpawnerSubtype = "WwBlockSpawner";
        private const int PendingSpawnPollFrames = 10;
        private const double SpawnSurfaceClearance = 0.5;
        private const double SpawnBoundsPadding = 0.1;

        private readonly List<ReclamationBlockCatalogEntry> reclamationBlockCatalog =
            new List<ReclamationBlockCatalogEntry>();

        private readonly Dictionary<string, ReclamationBlockCatalogEntry> reclamationBlockCatalogByKey =
            new Dictionary<string, ReclamationBlockCatalogEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<long, PendingReclamationSpawn> pendingReclamationSpawns =
            new Dictionary<long, PendingReclamationSpawn>();

        private readonly Dictionary<long, RunningReclamationSpawner> runningReclamationSpawners =
            new Dictionary<long, RunningReclamationSpawner>();

        private readonly Dictionary<long, string> reclamationSearchByBlock =
            new Dictionary<long, string>();

        private readonly Dictionary<long, string> selectedCatalogEntryByBlock =
            new Dictionary<long, string>();

        private readonly Dictionary<long, int> selectedSequenceIndexByBlock =
            new Dictionary<long, int>();

        private readonly Dictionary<long, int> selectedAppearanceIndexByBlock =
            new Dictionary<long, int>();

        private readonly Random reclamationRandom = new Random();
        private int nextPendingSpawnPollFrame;

        private void InitializeReclamationSpawners()
        {
            BuildReclamationBlockCatalog();
            RegisterReclamationSpawnerNetwork();
            RegisterReclamationSpawnerControls();
        }

        private void UnloadReclamationSpawners()
        {
            UnloadReclamationSpawnerParticles();
            UnregisterReclamationSpawnerControls();
            UnregisterReclamationSpawnerNetwork();
            reclamationBlockCatalog.Clear();
            reclamationBlockCatalogByKey.Clear();
            pendingReclamationSpawns.Clear();
            runningReclamationSpawners.Clear();
            reclamationSearchByBlock.Clear();
            selectedCatalogEntryByBlock.Clear();
            selectedSequenceIndexByBlock.Clear();
            selectedAppearanceIndexByBlock.Clear();
            reclamationEmissiveBlocks.Clear();
            reclamationEmissiveEntityBuffer.Clear();
            reclamationEmissiveBlockBuffer.Clear();
        }

        private void BuildReclamationBlockCatalog()
        {
            reclamationBlockCatalog.Clear();
            reclamationBlockCatalogByKey.Clear();

            foreach (var definition in MyDefinitionManager.Static.GetAllDefinitions())
            {
                var block = definition as MyCubeBlockDefinition;
                if (block == null || !block.Public || !block.Enabled || IsReclamationSpawnerDefinition(block.Id))
                    continue;

                var key = GetDefinitionKey(block.Id);
                var displayName = string.IsNullOrWhiteSpace(block.DisplayNameText)
                    ? block.Id.SubtypeName
                    : block.DisplayNameText;

                var entry = new ReclamationBlockCatalogEntry
                {
                    Key = key,
                    DisplayName = displayName + " [" + block.CubeSize + "]",
                    Definition = block,
                };

                reclamationBlockCatalog.Add(entry);
                reclamationBlockCatalogByKey[key] = entry;
            }

            reclamationBlockCatalog.Sort((left, right) =>
            {
                var byName = string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase);
                return byName != 0
                    ? byName
                    : string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static string GetDefinitionKey(MyDefinitionId id)
        {
            var typeName = id.TypeId.ToString();
            const string prefix = "MyObjectBuilder_";
            if (typeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                typeName = typeName.Substring(prefix.Length);

            return typeName + "/" + id.SubtypeName;
        }

        private static bool IsReclamationSpawnerDefinition(MyDefinitionId id)
        {
            return id.TypeId.ToString().Equals("MyObjectBuilder_TerminalBlock", StringComparison.OrdinalIgnoreCase) &&
                   id.SubtypeName.Equals(BlockSpawnerSubtype, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReclamationSpawner(IMyTerminalBlock block)
        {
            if (block == null)
                return false;

            return block.BlockDefinition.TypeIdString.Equals("MyObjectBuilder_TerminalBlock", StringComparison.OrdinalIgnoreCase) &&
                   block.BlockDefinition.SubtypeName.Equals(BlockSpawnerSubtype, StringComparison.OrdinalIgnoreCase);
        }

        private static ReclamationSpawnerConfig ReadReclamationSpawnerConfig(IMyTerminalBlock block)
        {
            return ReclamationSpawnerConfigCodec.Read(block != null ? block.CustomData : null);
        }

        private static void WriteReclamationSpawnerConfig(IMyTerminalBlock block, ReclamationSpawnerConfig config)
        {
            if (block == null)
                return;

            block.CustomData = ReclamationSpawnerConfigCodec.Write(block.CustomData, config);
            block.SetDetailedInfoDirty();
            block.RefreshCustomInfo();
        }

        private void UpdatePendingReclamationSpawns()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            if (frame < nextPendingSpawnPollFrame)
                return;

            nextPendingSpawnPollFrame = frame + PendingSpawnPollFrames;
            if (pendingReclamationSpawns.Count > 0)
            {
                var pending = new List<PendingReclamationSpawn>(pendingReclamationSpawns.Values);
                for (var i = 0; i < pending.Count; i++)
                    TryCompletePendingReclamationSpawn(pending[i]);
            }

            if (runningReclamationSpawners.Count > 0)
                UpdateRunningReclamationSpawners(frame);
        }

        private void UpdateRunningReclamationSpawners(int frame)
        {
            var running = new List<RunningReclamationSpawner>(runningReclamationSpawners.Values);
            for (var i = 0; i < running.Count; i++)
            {
                var state = running[i];
                if (state.NextSpawnFrame > frame || pendingReclamationSpawns.ContainsKey(state.BlockEntityId))
                    continue;

                var block = MyAPIGateway.Entities.GetEntityById(state.BlockEntityId) as IMyTerminalBlock;
                if (!IsReclamationSpawner(block) || block.Closed)
                {
                    runningReclamationSpawners.Remove(state.BlockEntityId);
                    continue;
                }

                var config = ReadReclamationSpawnerConfig(block);
                if (config.Entries.Count == 0 ||
                    (config.Mode == ReclamationSequenceMode.Once && config.Completed))
                {
                    runningReclamationSpawners.Remove(state.BlockEntityId);
                    RefreshReclamationSpawnerVisuals(block);
                    continue;
                }

                string ignored;
                if (!QueueReclamationSpawn(block, out ignored))
                    runningReclamationSpawners.Remove(state.BlockEntityId);
            }
        }

        private bool QueueReclamationSpawn(IMyTerminalBlock block, out string message)
        {
            message = string.Empty;
            if (!IsReclamationSpawner(block) || block.Closed || block.CubeGrid == null)
            {
                message = "Block Spawner is not available.";
                return false;
            }

            if (pendingReclamationSpawns.ContainsKey(block.EntityId))
            {
                message = "A spawn is already waiting for the area to clear.";
                return true;
            }

            var config = ReadReclamationSpawnerConfig(block);
            if (config.Entries.Count == 0)
            {
                message = "The spawn sequence is empty.";
                return false;
            }

            if (config.Mode == ReclamationSequenceMode.Once && config.Completed)
            {
                message = "The Once sequence is complete. Reset it before spawning again.";
                return false;
            }

            var sequenceIndex = SelectSequenceIndex(config);
            if (sequenceIndex < 0 || sequenceIndex >= config.Entries.Count)
            {
                message = "The spawn sequence has no valid next entry.";
                return false;
            }

            ReclamationBlockCatalogEntry catalogEntry;
            if (!reclamationBlockCatalogByKey.TryGetValue(config.Entries[sequenceIndex], out catalogEntry))
            {
                message = "The next block definition is not loaded: " + config.Entries[sequenceIndex];
                return false;
            }

            var request = new PendingReclamationSpawn
            {
                BlockEntityId = block.EntityId,
                DefinitionKey = catalogEntry.Key,
                SequenceIndex = sequenceIndex,
                Appearance = SelectAppearance(block, config),
                IntegrityPercent = SelectIntegrity(config),
            };
            SelectSpawnOrientation(block, config.RotationVariance, out request.Forward, out request.Up);

            pendingReclamationSpawns[block.EntityId] = request;
            if (TryCompletePendingReclamationSpawn(request))
                message = "Spawned " + catalogEntry.DisplayName + ".";
            else if (request.EarliestSpawnFrame > 0)
                message = "Priming smoke before spawning " + catalogEntry.DisplayName + ".";
            else
                message = "Waiting for enough room to spawn " + catalogEntry.DisplayName + ".";

            RefreshReclamationSpawnerVisuals(block);
            return true;
        }

        private int SelectSequenceIndex(ReclamationSpawnerConfig config)
        {
            if (config == null || config.Entries.Count == 0)
                return -1;

            if (config.Mode == ReclamationSequenceMode.Random)
                return reclamationRandom.Next(config.Entries.Count);

            return Math.Max(0, Math.Min(config.Entries.Count - 1, config.Cursor));
        }

        private ReclamationAppearancePreset SelectAppearance(
            IMyTerminalBlock spawner,
            ReclamationSpawnerConfig config)
        {
            if (config.AppearancePresets.Count == 0)
                return CaptureReclamationAppearance(spawner);

            var selected = config.AppearancePresets[reclamationRandom.Next(config.AppearancePresets.Count)];
            return new ReclamationAppearancePreset
            {
                ColorMaskHsv = selected.ColorMaskHsv,
                SkinSubtypeId = selected.SkinSubtypeId,
            };
        }

        private float SelectIntegrity(ReclamationSpawnerConfig config)
        {
            if (config.MaximumIntegrity <= config.MinimumIntegrity)
                return config.MinimumIntegrity;

            return config.MinimumIntegrity +
                   (float)reclamationRandom.NextDouble() * (config.MaximumIntegrity - config.MinimumIntegrity);
        }

        private void SelectSpawnOrientation(
            IMyTerminalBlock spawner,
            float variancePercent,
            out Vector3D forward,
            out Vector3D up)
        {
            var amount = Math.Max(0f, Math.Min(1f, variancePercent / 100f));
            if (amount <= 0f)
            {
                forward = GetReclamationOutputDirection(spawner);
                up = Vector3D.Normalize(spawner.WorldMatrix.Up);
                return;
            }

            var u1 = reclamationRandom.NextDouble();
            var u2 = reclamationRandom.NextDouble();
            var u3 = reclamationRandom.NextDouble();
            var randomRotation = new Quaternion(
                (float)(Math.Sqrt(1d - u1) * Math.Sin(MathHelper.TwoPi * u2)),
                (float)(Math.Sqrt(1d - u1) * Math.Cos(MathHelper.TwoPi * u2)),
                (float)(Math.Sqrt(u1) * Math.Sin(MathHelper.TwoPi * u3)),
                (float)(Math.Sqrt(u1) * Math.Cos(MathHelper.TwoPi * u3)));
            var blendedRotation = Quaternion.Slerp(Quaternion.Identity, randomRotation, amount);
            var localRotation = Matrix.CreateFromQuaternion(blendedRotation);
            var baseRotation = MatrixD.CreateWorld(
                Vector3D.Zero,
                GetReclamationOutputDirection(spawner),
                Vector3D.Normalize(spawner.WorldMatrix.Up));

            forward = Vector3D.Normalize(Vector3D.TransformNormal(localRotation.Forward, baseRotation));
            up = Vector3D.Normalize(Vector3D.TransformNormal(localRotation.Up, baseRotation));
        }

        private static ReclamationAppearancePreset CaptureReclamationAppearance(IMyTerminalBlock block)
        {
            var builder = block != null ? block.GetObjectBuilderCubeBlock(true) : null;
            return new ReclamationAppearancePreset
            {
                ColorMaskHsv = builder != null ? (Vector3)builder.ColorMaskHSV : Vector3.Zero,
                SkinSubtypeId = builder != null ? builder.SkinSubtypeId : string.Empty,
            };
        }

        private bool TryCompletePendingReclamationSpawn(PendingReclamationSpawn pending)
        {
            if (pending == null)
                return false;

            var block = MyAPIGateway.Entities.GetEntityById(pending.BlockEntityId) as IMyTerminalBlock;
            if (!IsReclamationSpawner(block) || block.Closed || block.CubeGrid == null)
            {
                pendingReclamationSpawns.Remove(pending.BlockEntityId);
                EndReclamationBurstSmoke(pending.BlockEntityId, true);
                return false;
            }

            var config = ReadReclamationSpawnerConfig(block);
            if (pending.SequenceIndex < 0 ||
                pending.SequenceIndex >= config.Entries.Count ||
                !config.Entries[pending.SequenceIndex].Equals(pending.DefinitionKey, StringComparison.OrdinalIgnoreCase))
            {
                pendingReclamationSpawns.Remove(pending.BlockEntityId);
                EndReclamationBurstSmoke(pending.BlockEntityId, true);
                RefreshReclamationSpawnerVisuals(block);
                return false;
            }

            ReclamationBlockCatalogEntry catalogEntry;
            if (!reclamationBlockCatalogByKey.TryGetValue(pending.DefinitionKey, out catalogEntry))
            {
                pendingReclamationSpawns.Remove(pending.BlockEntityId);
                EndReclamationBurstSmoke(pending.BlockEntityId, true);
                RefreshReclamationSpawnerVisuals(block);
                return false;
            }

            Vector3D spawnPosition;
            BoundingSphereD spawnClearance;
            CalculateSpawnPlacement(
                block,
                catalogEntry.Definition,
                pending.Forward,
                pending.Up,
                out spawnPosition,
                out spawnClearance);
            if (!IsSpawnVolumeClear(block, ref spawnClearance))
            {
                if (pending.EarliestSpawnFrame > 0)
                {
                    pending.EarliestSpawnFrame = 0;
                    EndReclamationBurstSmoke(pending.BlockEntityId, true);
                }

                return false;
            }

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            if (config.SmokeMode == ReclamationSmokeMode.Bursts)
            {
                if (pending.EarliestSpawnFrame <= 0)
                {
                    pending.EarliestSpawnFrame = frame + ReclamationBurstSmokeFrames;
                    BeginReclamationBurstSmoke(block, ReclamationBurstSmokeFrames, true);
                    return false;
                }

                if (frame < pending.EarliestSpawnFrame)
                    return false;
            }

            if (!SpawnSingleBlockGrid(
                    block,
                    catalogEntry.Definition,
                    spawnPosition,
                    pending.Forward,
                    pending.Up,
                    pending.Appearance,
                    pending.IntegrityPercent,
                    config.OutwardVelocity))
            {
                pendingReclamationSpawns.Remove(pending.BlockEntityId);
                EndReclamationBurstSmoke(pending.BlockEntityId, true);
                RefreshReclamationSpawnerVisuals(block);
                return false;
            }

            pendingReclamationSpawns.Remove(pending.BlockEntityId);
            if (config.SmokeMode == ReclamationSmokeMode.Bursts)
                BeginReclamationBurstSmoke(block, ReclamationBurstSmokeFrames, true);
            AdvanceSequence(config, pending.SequenceIndex);
            WriteReclamationSpawnerConfig(block, config);
            ScheduleNextAutomaticSpawn(block, config);
            RefreshReclamationSpawnerVisuals(block);
            return true;
        }

        private void ScheduleNextAutomaticSpawn(IMyTerminalBlock block, ReclamationSpawnerConfig config)
        {
            RunningReclamationSpawner running;
            if (!runningReclamationSpawners.TryGetValue(block.EntityId, out running))
                return;

            if (config.Mode == ReclamationSequenceMode.Once && config.Completed)
            {
                runningReclamationSpawners.Remove(block.EntityId);
                return;
            }

            var intervalFrames = Math.Max(1, (int)Math.Ceiling(config.AutomaticIntervalSeconds * 60f));
            running.NextSpawnFrame = MyAPIGateway.Session.GameplayFrameCounter + intervalFrames;
        }

        private static void AdvanceSequence(ReclamationSpawnerConfig config, int spawnedIndex)
        {
            if (config.Mode == ReclamationSequenceMode.Random)
                return;

            if (config.Mode == ReclamationSequenceMode.Loop)
            {
                config.Cursor = (spawnedIndex + 1) % config.Entries.Count;
                return;
            }

            config.Cursor = spawnedIndex + 1;
            config.Completed = config.Cursor >= config.Entries.Count;
        }

        private static void CalculateSpawnPlacement(
            IMyTerminalBlock spawner,
            MyCubeBlockDefinition payloadDefinition,
            Vector3D payloadForward,
            Vector3D payloadUp,
            out Vector3D position,
            out BoundingSphereD clearanceSphere)
        {
            var outputDirection = GetReclamationOutputDirection(spawner);
            var forward = Vector3D.Normalize(payloadForward);
            var up = Vector3D.Normalize(payloadUp);
            var payloadGridSize = payloadDefinition.CubeSize == MyCubeSize.Large ? 2.5 : 0.5;
            var payloadHalf = new Vector3D(
                payloadDefinition.Size.X * payloadGridSize * 0.5,
                payloadDefinition.Size.Y * payloadGridSize * 0.5,
                payloadDefinition.Size.Z * payloadGridSize * 0.5);
            var clearanceRadius = payloadHalf.Length() + SpawnBoundsPadding;

            var spawnerOutputDepth = GetReclamationSpawnerOutputDepth(spawner, outputDirection);
            var payloadCenter = spawner.GetPosition() +
                                outputDirection *
                                (spawnerOutputDepth + clearanceRadius + SpawnSurfaceClearance);

            var payloadMin = -payloadDefinition.Center;
            var localCenterCells = new Vector3D(
                payloadMin.X + (payloadDefinition.Size.X - 1) * 0.5,
                payloadMin.Y + (payloadDefinition.Size.Y - 1) * 0.5,
                payloadMin.Z + (payloadDefinition.Size.Z - 1) * 0.5);
            var payloadOrientation = MatrixD.CreateWorld(Vector3D.Zero, forward, up);
            var payloadCenterOffset = Vector3D.TransformNormal(
                localCenterCells * payloadGridSize,
                payloadOrientation);
            position = payloadCenter - payloadCenterOffset;
            clearanceSphere = new BoundingSphereD(payloadCenter, clearanceRadius);
        }

        private static double GetReclamationSpawnerOutputDepth(
            IMyTerminalBlock spawner,
            Vector3D outputDirection)
        {
            var localBounds = spawner.LocalAABB;
            var inverseOrientation = MatrixD.Transpose(spawner.WorldMatrix.GetOrientation());
            var localOutputDirection = Vector3D.Normalize(
                Vector3D.TransformNormal(outputDirection, inverseOrientation));
            var localCenter = (Vector3D)localBounds.Center;
            var localHalf = (Vector3D)localBounds.HalfExtents;

            return Math.Max(
                0.0,
                Vector3D.Dot(localCenter, localOutputDirection) +
                Math.Abs(localOutputDirection.X) * localHalf.X +
                Math.Abs(localOutputDirection.Y) * localHalf.Y +
                Math.Abs(localOutputDirection.Z) * localHalf.Z);
        }

        private static Vector3D GetReclamationOutputDirection(IMyTerminalBlock spawner)
        {
            // The half vent mounts on Back; its visible grille and output face point Forward.
            return Vector3D.Normalize(spawner.WorldMatrix.Forward);
        }

        private static bool IsSpawnVolumeClear(
            IMyTerminalBlock spawner,
            ref BoundingSphereD clearanceSphere)
        {
            var entities = MyAPIGateway.Entities.GetEntitiesInSphere(ref clearanceSphere);
            for (var i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (entity == null || entity.Closed || entity.EntityId == spawner.EntityId)
                    continue;

                var grid = entity as IMyCubeGrid;
                if (grid != null)
                {
                    if (GridHasBlockInSphere(grid, spawner, ref clearanceSphere))
                        return false;

                    continue;
                }

                if (entity is IMyVoxelBase)
                    continue;

                if (entity.WorldAABB.Intersects(clearanceSphere))
                    return false;
            }

            return true;
        }

        private static bool GridHasBlockInSphere(
            IMyCubeGrid grid,
            IMyTerminalBlock spawner,
            ref BoundingSphereD clearanceSphere)
        {
            var blocks = grid.GetBlocksInsideSphere(ref clearanceSphere);
            for (var i = 0; i < blocks.Count; i++)
            {
                var fatBlock = blocks[i].FatBlock;
                if (fatBlock != null && fatBlock.EntityId == spawner.EntityId)
                    continue;

                return true;
            }

            return false;
        }

        private static bool SpawnSingleBlockGrid(
            IMyTerminalBlock spawner,
            MyCubeBlockDefinition definition,
            Vector3D position,
            Vector3D forward,
            Vector3D up,
            ReclamationAppearancePreset appearance,
            float integrityPercent,
            float outwardVelocity)
        {
            try
            {
                var blockBuilder = MyObjectBuilderSerializer.CreateNewObject(definition.Id.TypeId, definition.Id.SubtypeName)
                    as MyObjectBuilder_CubeBlock;
                if (blockBuilder == null)
                    return false;

                blockBuilder.EntityId = 0;
                blockBuilder.Min = -definition.Center;
                blockBuilder.BlockOrientation = new SerializableBlockOrientation(
                    Base6Directions.Direction.Forward,
                    Base6Directions.Direction.Up);
                blockBuilder.BuildPercent = 1f;
                blockBuilder.IntegrityPercent = Math.Max(0.1f, Math.Min(1f, integrityPercent / 100f));
                blockBuilder.Owner = 0;
                blockBuilder.BuiltBy = 0;
                blockBuilder.ShareMode = MyOwnershipShareModeEnum.None;
                blockBuilder.ColorMaskHSV = appearance != null ? appearance.ColorMaskHsv : spawner.SlimBlock.ColorMaskHSV;
                blockBuilder.SkinSubtypeId = appearance != null ? appearance.SkinSubtypeId : string.Empty;

                var gridBuilder = new MyObjectBuilder_CubeGrid
                {
                    EntityId = 0,
                    GridSizeEnum = definition.CubeSize,
                    IsStatic = false,
                    PersistentFlags = MyPersistentEntityFlags2.CastShadows | MyPersistentEntityFlags2.InScene,
                    PositionAndOrientation = new MyPositionAndOrientation(
                        position,
                        (Vector3)forward,
                        (Vector3)up),
                    CubeBlocks = new List<MyObjectBuilder_CubeBlock> { blockBuilder },
                };

                var spawnedGrid = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(gridBuilder) as IMyCubeGrid;
                if (spawnedGrid == null || spawnedGrid.Physics == null)
                    return spawnedGrid != null;

                var inheritedVelocity = spawner.CubeGrid.Physics != null
                    ? spawner.CubeGrid.Physics.LinearVelocity
                    : Vector3.Zero;

                spawnedGrid.Physics.LinearVelocity = inheritedVelocity +
                                                     (Vector3)GetReclamationOutputDirection(spawner) * outwardVelocity;
                return true;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole("[Worldwright] failed to spawn block: " + exception.Message);
                return false;
            }
        }
    }
}
