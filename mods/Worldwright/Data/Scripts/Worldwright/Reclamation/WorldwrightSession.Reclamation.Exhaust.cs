using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        private const int ReclamationExhaustUpdateFrames = 10;
        private const int ReclamationSpawnExhaustTailFrames = 90;

        private readonly Dictionary<long, int> reclamationExhaustUntilFrame =
            new Dictionary<long, int>();

        private readonly Dictionary<long, ReclamationSmokeStyle> reclamationAppliedSmokeStyles =
            new Dictionary<long, ReclamationSmokeStyle>();

        private readonly Dictionary<long, float> reclamationAppliedExhaustIntensities =
            new Dictionary<long, float>();

        private readonly HashSet<long> reclamationActiveExhausts = new HashSet<long>();
        private int nextReclamationExhaustUpdateFrame;

        private void BeginReclamationSpawnExhaust(IMyTerminalBlock block)
        {
            if (!IsReclamationSpawner(block) || MyAPIGateway.Session == null)
                return;

            var untilFrame = MyAPIGateway.Session.GameplayFrameCounter + ReclamationSpawnExhaustTailFrames;
            int currentUntil;
            if (!reclamationExhaustUntilFrame.TryGetValue(block.EntityId, out currentUntil) || currentUntil < untilFrame)
                reclamationExhaustUntilFrame[block.EntityId] = untilFrame;

            ApplyReclamationSpawnerExhaust(block, MyAPIGateway.Session.GameplayFrameCounter);
        }

        private void UpdateReclamationSpawnerExhausts()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Entities == null)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            if (frame < nextReclamationExhaustUpdateFrame)
                return;

            nextReclamationExhaustUpdateFrame = frame + ReclamationExhaustUpdateFrames;
            var entityIds = new List<long>(reclamationEmissiveBlocks);
            for (var i = 0; i < entityIds.Count; i++)
            {
                var block = MyAPIGateway.Entities.GetEntityById(entityIds[i]) as IMyTerminalBlock;
                if (!IsReclamationSpawner(block) || block.Closed)
                {
                    StopAndForgetReclamationExhaust(entityIds[i], block as IMyExhaustBlock);
                    continue;
                }

                ApplyReclamationSpawnerExhaust(block, frame);
            }
        }

        private void ApplyReclamationSpawnerExhaust(IMyTerminalBlock block, int frame)
        {
            var exhaust = block as IMyExhaustBlock;
            if (exhaust == null)
                return;

            var config = ReadReclamationSpawnerConfig(block);
            var styleChanged = !reclamationAppliedSmokeStyles.ContainsKey(block.EntityId) ||
                               reclamationAppliedSmokeStyles[block.EntityId] != config.SmokeStyle;
            if (styleChanged)
            {
                exhaust.SelectEffect(GetReclamationSmokeEffect(config.SmokeStyle));
                reclamationAppliedSmokeStyles[block.EntityId] = config.SmokeStyle;
            }

            float appliedIntensity;
            if (!reclamationAppliedExhaustIntensities.TryGetValue(block.EntityId, out appliedIntensity) ||
                Math.Abs(appliedIntensity - config.ExhaustIntensity) > 0.01f)
            {
                block.SetValueFloat("PowerDependency", config.ExhaustIntensity / 100f);
                reclamationAppliedExhaustIntensities[block.EntityId] = config.ExhaustIntensity;
            }

            int untilFrame;
            var spawning = pendingReclamationSpawns.ContainsKey(block.EntityId) ||
                           (reclamationExhaustUntilFrame.TryGetValue(block.EntityId, out untilFrame) && untilFrame >= frame);
            if (!spawning && reclamationExhaustUntilFrame.ContainsKey(block.EntityId))
                reclamationExhaustUntilFrame.Remove(block.EntityId);

            var shouldRun = block.IsFunctional &&
                            (config.ExhaustMode == ReclamationExhaustMode.Always ||
                             (config.ExhaustMode == ReclamationExhaustMode.WhileSpawning && spawning));

            if (shouldRun)
            {
                if (!reclamationActiveExhausts.Contains(block.EntityId) || styleChanged)
                {
                    exhaust.StartEffects();
                    reclamationActiveExhausts.Add(block.EntityId);
                }
            }
            else if (reclamationActiveExhausts.Remove(block.EntityId))
            {
                exhaust.StopEffects();
            }
        }

        private void StopAndForgetReclamationExhaust(long entityId, IMyExhaustBlock exhaust)
        {
            if (exhaust != null && reclamationActiveExhausts.Contains(entityId))
                exhaust.StopEffects();

            reclamationActiveExhausts.Remove(entityId);
            reclamationAppliedSmokeStyles.Remove(entityId);
            reclamationAppliedExhaustIntensities.Remove(entityId);
            reclamationExhaustUntilFrame.Remove(entityId);
        }

        private void UnloadReclamationSpawnerExhausts()
        {
            var active = new List<long>(reclamationActiveExhausts);
            for (var i = 0; i < active.Count; i++)
            {
                var exhaust = MyAPIGateway.Entities != null
                    ? MyAPIGateway.Entities.GetEntityById(active[i]) as IMyExhaustBlock
                    : null;
                StopAndForgetReclamationExhaust(active[i], exhaust);
            }

            reclamationActiveExhausts.Clear();
            reclamationAppliedSmokeStyles.Clear();
            reclamationAppliedExhaustIntensities.Clear();
            reclamationExhaustUntilFrame.Clear();
        }

        private static string GetReclamationSmokeEffect(ReclamationSmokeStyle style)
        {
            switch (style)
            {
                case ReclamationSmokeStyle.Dark:
                    return "WwDarkSmoke";
                case ReclamationSmokeStyle.Reactor:
                    return "WwReactorSmoke";
                case ReclamationSmokeStyle.White:
                    return "WwWhiteSmoke";
                default:
                    return "WwBlackSmoke";
            }
        }
    }
}
