using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace WkKn
{
    internal struct BlockKey : IEquatable<BlockKey>
    {
        public readonly long GridEntityId;
        public readonly Vector3I Min;

        internal BlockKey(long gridEntityId, Vector3I min)
        {
            GridEntityId = gridEntityId;
            Min = min;
        }

        internal static bool TryCreate(IMySlimBlock slimBlock, out BlockKey key)
        {
            key = new BlockKey();
            if (slimBlock == null || slimBlock.CubeGrid == null)
                return false;

            key = new BlockKey(slimBlock.CubeGrid.EntityId, slimBlock.Min);
            return true;
        }

        public bool Equals(BlockKey other)
        {
            return GridEntityId == other.GridEntityId &&
                   Min.Equals(other.Min);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BlockKey))
                return false;

            return Equals((BlockKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 31) + GridEntityId.GetHashCode();
                hash = (hash * 31) + Min.GetHashCode();
                return hash;
            }
        }
    }

    internal struct BlockSnapshot
    {
        public readonly float BuildIntegrity;
        public readonly float Integrity;
        public readonly float MaxIntegrity;

        internal BlockSnapshot(float buildIntegrity, float integrity, float maxIntegrity)
        {
            BuildIntegrity = buildIntegrity;
            Integrity = integrity;
            MaxIntegrity = maxIntegrity;
        }

        internal static bool TryCapture(IMySlimBlock slimBlock, out BlockSnapshot snapshot)
        {
            snapshot = new BlockSnapshot();
            if (slimBlock == null || slimBlock.ComponentStack == null)
                return false;

            var maxIntegrity = slimBlock.ComponentStack.MaxIntegrity;
            if (maxIntegrity <= 0f)
                return false;

            snapshot = new BlockSnapshot(
                slimBlock.ComponentStack.BuildIntegrity,
                slimBlock.ComponentStack.Integrity,
                maxIntegrity);
            return true;
        }
    }

    internal struct BlockIntegrityChange
    {
        public readonly BlockKey Key;
        public readonly IMySlimBlock Block;
        public readonly BlockSnapshot Previous;
        public readonly BlockSnapshot Current;
        public readonly long Tick;

        internal BlockIntegrityChange(
            BlockKey key,
            IMySlimBlock block,
            BlockSnapshot previous,
            BlockSnapshot current,
            long tick)
        {
            Key = key;
            Block = block;
            Previous = previous;
            Current = current;
            Tick = tick;
        }
    }

    internal sealed class BlockIntegrityMonitor
    {
        private const double DefaultTolerance = 0.001;

        private readonly Dictionary<long, GridIntegrityState> grids = new Dictionary<long, GridIntegrityState>();
        private readonly Queue<long> fullScanQueue = new Queue<long>();
        private readonly List<IMySlimBlock> scanBuffer = new List<IMySlimBlock>();
        private readonly double tolerance;
        private long currentTick;
        private long lastScanDiscoveryTick = long.MinValue;

        internal event Action<BlockIntegrityChange> PositiveIntegrityChanged;
        internal event Action<IMySlimBlock> BlockAdded;

        internal BlockIntegrityMonitor(double tolerance = DefaultTolerance)
        {
            this.tolerance = Math.Max(0.0, tolerance);
        }

        internal void Update(
            long simulationTick,
            int maxGridScans,
            long discoveryIntervalTicks,
            double discoveryRadius)
        {
            currentTick = simulationTick;
            ProcessScheduledScans(maxGridScans);

            if (lastScanDiscoveryTick != long.MinValue &&
                simulationTick - lastScanDiscoveryTick < discoveryIntervalTicks)
            {
                return;
            }

            lastScanDiscoveryTick = simulationTick;
            DiscoverNearbyGrids(discoveryRadius);
        }

        internal void TrackExistingGrids()
        {
            if (MyAPIGateway.Entities == null)
                return;

            var entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, entity => entity is IMyCubeGrid);
            foreach (var entity in entities)
                TrackGrid(entity as IMyCubeGrid);
        }

        internal void TrackEntity(IMyEntity entity)
        {
            TrackGrid(entity as IMyCubeGrid);
        }

        internal void UntrackEntity(IMyEntity entity)
        {
            UntrackGrid(entity as IMyCubeGrid);
        }

        internal void UntrackAllGrids()
        {
            var gridIds = new List<long>();
            foreach (var entry in grids)
            {
                if (entry.Value.Tracked)
                    gridIds.Add(entry.Key);
            }

            for (var i = 0; i < gridIds.Count; i++)
            {
                GridIntegrityState state;
                if (grids.TryGetValue(gridIds[i], out state))
                    UntrackGrid(state.Grid);
            }

            Clear();
        }

        private void Clear()
        {
            grids.Clear();
            fullScanQueue.Clear();
            scanBuffer.Clear();
            lastScanDiscoveryTick = long.MinValue;
        }

        private void DiscoverNearbyGrids(double discoveryRadius)
        {
            if (MyAPIGateway.Players == null || MyAPIGateway.Entities == null)
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            var discoveredGridIds = new HashSet<long>();

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.Character == null)
                    continue;

                var sphere = new BoundingSphereD(player.Character.WorldMatrix.Translation, discoveryRadius);
                var entities = MyAPIGateway.Entities.GetEntitiesInSphere(ref sphere);
                if (entities == null)
                    continue;

                foreach (var entity in entities)
                {
                    var grid = entity as IMyCubeGrid;
                    if (grid == null || !discoveredGridIds.Add(grid.EntityId))
                        continue;

                    TrackGrid(grid);
                }
            }
        }

        private void TrackGrid(IMyCubeGrid grid)
        {
            if (grid == null)
                return;

            var state = GetOrCreateGridState(grid.EntityId, grid);
            state.Grid = grid;
            if (state.Tracked)
                return;

            state.Tracked = true;
            grid.OnBlockAdded += OnTrackedBlockAdded;
            grid.OnBlockRemoved += OnTrackedBlockRemoved;
            grid.OnBlockIntegrityChanged += OnTrackedBlockIntegrityChanged;
            grid.OnGridSplit += OnTrackedGridSplit;
            grid.OnGridMerge += OnTrackedGridMerge;
            ScheduleFullScan(grid);
        }

        private void UntrackGrid(IMyCubeGrid grid)
        {
            if (grid == null)
                return;

            GridIntegrityState state;
            if (!grids.TryGetValue(grid.EntityId, out state) || !state.Tracked)
                return;

            grid.OnBlockAdded -= OnTrackedBlockAdded;
            grid.OnBlockRemoved -= OnTrackedBlockRemoved;
            grid.OnBlockIntegrityChanged -= OnTrackedBlockIntegrityChanged;
            grid.OnGridSplit -= OnTrackedGridSplit;
            grid.OnGridMerge -= OnTrackedGridMerge;
            grids.Remove(grid.EntityId);
        }

        private void OnTrackedBlockAdded(IMySlimBlock slimBlock)
        {
            var handler = BlockAdded;
            if (handler != null)
                handler(slimBlock);

            if (slimBlock == null || slimBlock.CubeGrid == null)
                return;

            CaptureOrRemoveRecord(slimBlock);
        }

        private void OnTrackedBlockRemoved(IMySlimBlock slimBlock)
        {
            if (slimBlock == null || slimBlock.CubeGrid == null)
                return;

            RemoveBlockRecord(slimBlock);
        }

        private void OnTrackedGridSplit(IMyCubeGrid oldGrid, IMyCubeGrid newGrid)
        {
            if (oldGrid != null)
            {
                ClearGridRecords(oldGrid);
                ScheduleFullScan(oldGrid);
            }

            if (newGrid != null)
            {
                TrackGrid(newGrid);
                ClearGridRecords(newGrid);
                ScheduleFullScan(newGrid);
            }
        }

        private void OnTrackedGridMerge(IMyCubeGrid gridA, IMyCubeGrid gridB)
        {
            if (gridA != null)
            {
                TrackGrid(gridA);
                ClearGridRecords(gridA);
                ScheduleFullScan(gridA);
            }

            if (gridB != null)
            {
                TrackGrid(gridB);
                ClearGridRecords(gridB);
                ScheduleFullScan(gridB);
            }
        }

        private void OnTrackedBlockIntegrityChanged(IMySlimBlock slimBlock)
        {
            BlockIntegrityChange change;
            if (!TryUpdateBlock(slimBlock, currentTick, out change))
                return;

            var handler = PositiveIntegrityChanged;
            if (handler != null)
                handler(change);
        }

        private bool TryUpdateBlock(IMySlimBlock slimBlock, long simulationTick, out BlockIntegrityChange change)
        {
            change = new BlockIntegrityChange();

            BlockKey key;
            if (!BlockKey.TryCreate(slimBlock, out key))
                return false;

            BlockSnapshot current;
            if (!BlockSnapshot.TryCapture(slimBlock, out current))
                return false;

            var gridState = GetOrCreateGridState(key.GridEntityId, slimBlock.CubeGrid);

            BlockSnapshot previous;
            var hasPrevious = gridState.Blocks.TryGetValue(key.Min, out previous) &&
                              CanUsePrevious(previous, current);
            if (!hasPrevious)
                previous = new BlockSnapshot();

            CaptureOrRemoveRecord(gridState, key.Min, current);

            if (!hasPrevious || current.Integrity - previous.Integrity <= tolerance)
                return false;

            change = new BlockIntegrityChange(key, slimBlock, previous, current, simulationTick);
            return true;
        }

        private void CaptureOrRemoveRecord(IMySlimBlock slimBlock)
        {
            BlockKey key;
            if (!BlockKey.TryCreate(slimBlock, out key))
                return;

            BlockSnapshot snapshot;
            if (!BlockSnapshot.TryCapture(slimBlock, out snapshot))
                return;

            var gridState = GetOrCreateGridState(key.GridEntityId, slimBlock.CubeGrid);
            CaptureOrRemoveRecord(gridState, key.Min, snapshot);
        }

        internal void RefreshBlockSnapshot(IMySlimBlock slimBlock)
        {
            CaptureOrRemoveRecord(slimBlock);
        }

        private void CaptureOrRemoveRecord(GridIntegrityState gridState, Vector3I blockMin, BlockSnapshot snapshot)
        {
            if (ShouldStore(snapshot))
            {
                gridState.Blocks[blockMin] = snapshot;
                return;
            }

            gridState.Blocks.Remove(blockMin);
        }

        private void RemoveBlockRecord(IMySlimBlock slimBlock)
        {
            BlockKey key;
            if (!BlockKey.TryCreate(slimBlock, out key))
                return;

            GridIntegrityState gridState;
            if (!grids.TryGetValue(key.GridEntityId, out gridState))
                return;

            gridState.Blocks.Remove(key.Min);
        }

        private void ClearGridRecords(IMyCubeGrid grid)
        {
            if (grid == null)
                return;

            var gridState = GetOrCreateGridState(grid.EntityId, grid);
            gridState.Blocks.Clear();
        }

        private void ScheduleFullScan(IMyCubeGrid grid)
        {
            if (grid == null)
                return;

            var gridState = GetOrCreateGridState(grid.EntityId, grid);
            gridState.Grid = grid;

            if (gridState.ScanQueued)
                return;

            gridState.ScanQueued = true;
            fullScanQueue.Enqueue(grid.EntityId);
        }

        private int ProcessScheduledScans(int maxGridScans)
        {
            var remaining = Math.Max(0, maxGridScans);
            var processed = 0;

            while (processed < remaining)
            {
                if (TryProcessOneScheduledScan())
                {
                    processed++;
                    continue;
                }

                break;
            }

            return processed;
        }

        private bool TryProcessOneScheduledScan()
        {
            while (fullScanQueue.Count > 0)
            {
                var gridEntityId = fullScanQueue.Dequeue();

                GridIntegrityState gridState;
                if (!grids.TryGetValue(gridEntityId, out gridState))
                    continue;

                if (!gridState.ScanQueued)
                    continue;

                if (gridState.Grid == null)
                {
                    grids.Remove(gridEntityId);
                    continue;
                }

                ScanGrid(gridState);
                return true;
            }

            return false;
        }

        private void ScanGrid(GridIntegrityState gridState)
        {
            if (gridState == null || gridState.Grid == null)
                return;

            gridState.Blocks.Clear();

            scanBuffer.Clear();
            gridState.Grid.GetBlocks(scanBuffer);
            for (var i = 0; i < scanBuffer.Count; i++)
            {
                var block = scanBuffer[i];
                if (block == null)
                    continue;

                BlockSnapshot snapshot;
                if (!BlockSnapshot.TryCapture(block, out snapshot) || !ShouldStore(snapshot))
                    continue;

                gridState.Blocks[block.Min] = snapshot;
            }

            scanBuffer.Clear();
            gridState.ScanQueued = false;
        }

        private GridIntegrityState GetOrCreateGridState(long gridEntityId, IMyCubeGrid grid)
        {
            GridIntegrityState gridState;
            if (!grids.TryGetValue(gridEntityId, out gridState))
            {
                gridState = new GridIntegrityState();
                grids[gridEntityId] = gridState;
            }

            if (grid != null)
                gridState.Grid = grid;

            return gridState;
        }

        private bool ShouldStore(BlockSnapshot snapshot)
        {
            return snapshot.MaxIntegrity > 0f &&
                   snapshot.Integrity < snapshot.MaxIntegrity - tolerance;
        }

        private bool CanUsePrevious(BlockSnapshot previous, BlockSnapshot current)
        {
            if (previous.MaxIntegrity <= 0f || current.MaxIntegrity <= 0f)
                return false;

            return Math.Abs(previous.MaxIntegrity - current.MaxIntegrity) <= tolerance;
        }

        private sealed class GridIntegrityState
        {
            public readonly Dictionary<Vector3I, BlockSnapshot> Blocks = new Dictionary<Vector3I, BlockSnapshot>();
            public IMyCubeGrid Grid;
            public bool ScanQueued;
            public bool Tracked;
        }
    }
}
