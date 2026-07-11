using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;

namespace Worldwright
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public sealed partial class WorldwrightSession : MySessionComponentBase
    {
        private const int DamageHandlerPriority = -100;
        private const int ProtectionCacheFrames = 120;
        private const string ProtectedGridNameToken = "G-PROT";
        private const string ConfigSection = "Worldwright";

        private readonly Dictionary<long, ProtectionCacheEntry> protectionCache =
            new Dictionary<long, ProtectionCacheEntry>();

        private readonly List<IMySlimBlock> blockBuffer = new List<IMySlimBlock>();

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session != null &&
                MyAPIGateway.Session.IsServer &&
                MyAPIGateway.Session.DamageSystem != null)
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(DamageHandlerPriority, BeforeDamage);
            }
        }

        public override void BeforeStart()
        {
            InitializeReclamationSpawners();
        }

        public override void UpdateAfterSimulation()
        {
            UpdateReclamationSpawnerEmissives();
            UpdateReclamationSpawnerExhausts();
            UpdatePendingReclamationSpawns();
        }

        protected override void UnloadData()
        {
            UnloadReclamationSpawners();
            protectionCache.Clear();
            blockBuffer.Clear();
        }

        private void BeforeDamage(object target, ref MyDamageInformation info)
        {
            if (info.IsDeformation || info.Amount <= 0 || info.Type != MyDamageType.Grind)
                return;

            var block = target as IMySlimBlock;
            if (block == null || block.CubeGrid == null)
                return;

            if (IsProtectedGrid(block.CubeGrid))
                info.Amount = 0;
        }

        private bool IsProtectedGrid(IMyCubeGrid grid)
        {
            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            ProtectionCacheEntry cached;
            if (protectionCache.TryGetValue(grid.EntityId, out cached) && cached.ValidUntilFrame >= frame)
                return cached.IsProtected;

            var isProtected = HasProtectedNameToken(grid.CustomName) || HasProtectionMarker(grid);
            protectionCache[grid.EntityId] = new ProtectionCacheEntry
            {
                IsProtected = isProtected,
                ValidUntilFrame = frame + ProtectionCacheFrames,
            };

            return isProtected;
        }

        private static bool HasProtectedNameToken(string gridName)
        {
            return (gridName ?? string.Empty).IndexOf(ProtectedGridNameToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasProtectionMarker(IMyCubeGrid grid)
        {
            blockBuffer.Clear();

            try
            {
                grid.GetBlocks(blockBuffer, IsTerminalBlock);

                for (var i = 0; i < blockBuffer.Count; i++)
                {
                    var terminalBlock = blockBuffer[i].FatBlock as IMyTerminalBlock;
                    if (terminalBlock == null)
                        continue;

                    if (CustomDataEnablesProtection(terminalBlock.CustomData))
                        return true;
                }
            }
            finally
            {
                blockBuffer.Clear();
            }

            return false;
        }

        private static bool IsTerminalBlock(IMySlimBlock block)
        {
            return block != null && block.FatBlock is IMyTerminalBlock;
        }

        private static bool CustomDataEnablesProtection(string customData)
        {
            if (string.IsNullOrWhiteSpace(customData))
                return false;

            var inWorldwrightSection = false;
            var lines = customData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line.Length == 0 || line.StartsWith(";", StringComparison.Ordinal) || line.StartsWith("#", StringComparison.Ordinal))
                    continue;

                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    var sectionName = line.Substring(1, line.Length - 2).Trim();
                    inWorldwrightSection = string.Equals(sectionName, ConfigSection, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inWorldwrightSection)
                    continue;

                var separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();
                if (string.Equals(key, "protected", StringComparison.OrdinalIgnoreCase) && IsTruthy(value))
                    return true;
            }

            return false;
        }

        private static bool IsTruthy(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "on", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
        }

        private struct ProtectionCacheEntry
        {
            public bool IsProtected;
            public int ValidUntilFrame;
        }
    }
}
