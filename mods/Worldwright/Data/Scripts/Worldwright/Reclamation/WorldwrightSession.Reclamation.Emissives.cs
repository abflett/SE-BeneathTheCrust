using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        private const int ReclamationEmissiveUpdateFrames = 10;
        private const int ReclamationEmissiveDiscoveryFrames = 120;

        private static readonly Color ReclamationReadyColor = new Color(40, 180, 255);
        private static readonly Color ReclamationRunningColor = new Color(70, 255, 100);
        private static readonly Color ReclamationBlockedColor = new Color(255, 170, 30);
        private static readonly Color ReclamationErrorColor = new Color(255, 45, 35);
        private static readonly Color ReclamationCompletedColor = new Color(185, 70, 255);

        private readonly HashSet<long> reclamationEmissiveBlocks = new HashSet<long>();
        private readonly HashSet<IMyEntity> reclamationEmissiveEntityBuffer = new HashSet<IMyEntity>();
        private readonly List<IMySlimBlock> reclamationEmissiveBlockBuffer = new List<IMySlimBlock>();

        private int nextReclamationEmissiveUpdateFrame;
        private int nextReclamationEmissiveDiscoveryFrame;

        private void TrackReclamationSpawnerEmissives(IMyTerminalBlock block)
        {
            if (IsReclamationSpawner(block) && block.EntityId != 0)
                reclamationEmissiveBlocks.Add(block.EntityId);
        }

        private void UpdateReclamationSpawnerEmissives()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Entities == null)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            if (frame < nextReclamationEmissiveUpdateFrame)
                return;

            nextReclamationEmissiveUpdateFrame = frame + ReclamationEmissiveUpdateFrames;
            if (frame >= nextReclamationEmissiveDiscoveryFrame)
            {
                DiscoverReclamationSpawnerEmissives();
                nextReclamationEmissiveDiscoveryFrame = frame + ReclamationEmissiveDiscoveryFrames;
            }

            var entityIds = new List<long>(reclamationEmissiveBlocks);
            for (var i = 0; i < entityIds.Count; i++)
            {
                var block = MyAPIGateway.Entities.GetEntityById(entityIds[i]) as IMyTerminalBlock;
                if (!IsReclamationSpawner(block) || block.Closed)
                {
                    reclamationEmissiveBlocks.Remove(entityIds[i]);
                    continue;
                }

                ApplyReclamationSpawnerEmissives(block, frame);
            }
        }

        private void DiscoverReclamationSpawnerEmissives()
        {
            reclamationEmissiveEntityBuffer.Clear();
            try
            {
                MyAPIGateway.Entities.GetEntities(
                    reclamationEmissiveEntityBuffer,
                    entity => entity is IMyCubeGrid);

                foreach (var entity in reclamationEmissiveEntityBuffer)
                {
                    var grid = entity as IMyCubeGrid;
                    if (grid == null)
                        continue;

                    reclamationEmissiveBlockBuffer.Clear();
                    grid.GetBlocks(
                        reclamationEmissiveBlockBuffer,
                        slimBlock => slimBlock != null && IsReclamationSpawner(slimBlock.FatBlock as IMyTerminalBlock));

                    for (var i = 0; i < reclamationEmissiveBlockBuffer.Count; i++)
                        TrackReclamationSpawnerEmissives(reclamationEmissiveBlockBuffer[i].FatBlock as IMyTerminalBlock);
                }
            }
            finally
            {
                reclamationEmissiveBlockBuffer.Clear();
                reclamationEmissiveEntityBuffer.Clear();
            }
        }

        private void ApplyReclamationSpawnerEmissives(IMyTerminalBlock block, int frame)
        {
            var config = ReadReclamationSpawnerConfig(block);
            if (!block.IsFunctional || HasMissingReclamationDefinition(config))
            {
                SetAllReclamationEmissives(block, ReclamationErrorColor, 1f);
                return;
            }

            if (pendingReclamationSpawns.ContainsKey(block.EntityId))
            {
                SetAllReclamationEmissives(block, ReclamationBlockedColor, 1f);
                return;
            }

            if (config.Mode == ReclamationSequenceMode.Once && config.Completed)
            {
                SetAllReclamationEmissives(block, ReclamationCompletedColor, 1f);
                return;
            }

            RunningReclamationSpawner running;
            if (runningReclamationSpawners.TryGetValue(block.EntityId, out running))
            {
                var intervalFrames = Math.Max(1, (int)Math.Round(config.AutomaticIntervalSeconds * 60f));
                var remainingFrames = Math.Max(0, running.NextSpawnFrame - frame);
                var progress = 1f - Math.Min(1f, remainingFrames / (float)intervalFrames);
                var illuminated = Math.Max(1, Math.Min(4, (int)Math.Ceiling(progress * 4f)));
                SetProgressReclamationEmissives(block, illuminated);
                return;
            }

            if (config.Entries.Count == 0)
            {
                SetAllReclamationEmissives(block, Color.Black, 0f);
                return;
            }

            SetAllReclamationEmissives(block, ReclamationReadyColor, 0.25f);
        }

        private bool HasMissingReclamationDefinition(ReclamationSpawnerConfig config)
        {
            for (var i = 0; i < config.Entries.Count; i++)
            {
                if (!reclamationBlockCatalogByKey.ContainsKey(config.Entries[i]))
                    return true;
            }

            return false;
        }

        private static void SetProgressReclamationEmissives(IMyTerminalBlock block, int illuminated)
        {
            for (var i = 0; i < 4; i++)
            {
                var active = i < illuminated;
                block.SetEmissiveParts(
                    "Emissive" + i,
                    ReclamationRunningColor,
                    active ? 1f : 0.05f);
            }
        }

        private static void SetAllReclamationEmissives(IMyTerminalBlock block, Color color, float intensity)
        {
            for (var i = 0; i < 4; i++)
                block.SetEmissiveParts("Emissive" + i, color, intensity);
        }
    }
}
