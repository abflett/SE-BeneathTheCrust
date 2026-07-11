using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        private const string ReclamationSmokeParticleName = "Smoke_LargeGunShot";
        private const float ReclamationMaximumSmokeBirthMultiplier = 20f;
        private const int ReclamationBurstSmokeFrames = 60;
        private const double ReclamationSmokeSurfaceOffset = 0.05;

        private readonly Dictionary<long, MyParticleEffect> reclamationSmokeEffects =
            new Dictionary<long, MyParticleEffect>();

        private readonly Dictionary<long, int> reclamationBurstSmokeUntilFrame =
            new Dictionary<long, int>();

        private void UpdateReclamationSpawnerParticles()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Entities == null ||
                MyAPIGateway.Utilities == null || MyAPIGateway.Utilities.IsDedicated)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            var entityIds = new List<long>(reclamationEmissiveBlocks);
            for (var i = 0; i < entityIds.Count; i++)
            {
                var block = MyAPIGateway.Entities.GetEntityById(entityIds[i]) as IMyTerminalBlock;
                if (!IsReclamationSpawner(block) || block.Closed)
                {
                    StopReclamationSmokeEffect(entityIds[i], true);
                    reclamationBurstSmokeUntilFrame.Remove(entityIds[i]);
                    continue;
                }

                var config = ReadReclamationSpawnerConfig(block);
                int burstUntilFrame;
                var burstActive = reclamationBurstSmokeUntilFrame.TryGetValue(block.EntityId, out burstUntilFrame) &&
                                  burstUntilFrame >= frame;
                if (!burstActive && reclamationBurstSmokeUntilFrame.ContainsKey(block.EntityId))
                    reclamationBurstSmokeUntilFrame.Remove(block.EntityId);

                var shouldEmit = block.IsFunctional &&
                                 (config.SmokeMode == ReclamationSmokeMode.Always ||
                                  (config.SmokeMode == ReclamationSmokeMode.Bursts && burstActive));
                if (shouldEmit)
                    UpdateReclamationSmokeEffect(block, config);
                else
                    StopReclamationSmokeEffect(block.EntityId, false);
            }
        }

        private void UpdateReclamationSmokeEffect(IMyTerminalBlock block, ReclamationSpawnerConfig config)
        {
            var effectMatrix = CreateReclamationSmokeMatrix(block);
            MyParticleEffect effect;
            if (!reclamationSmokeEffects.TryGetValue(block.EntityId, out effect) || effect == null)
            {
                var position = effectMatrix.Translation;
                if (!MyParticlesManager.TryCreateParticleEffect(
                        ReclamationSmokeParticleName,
                        ref effectMatrix,
                        ref position,
                        uint.MaxValue,
                        out effect))
                    return;

                effect.Play();
                reclamationSmokeEffects[block.EntityId] = effect;
            }

            effect.WorldMatrix = effectMatrix;
            effect.UserColorMultiplier = new Vector4(
                config.SmokeRed / 255f,
                config.SmokeGreen / 255f,
                config.SmokeBlue / 255f,
                1f);
            effect.UserBirthMultiplier =
                config.SmokeIntensity / 100f * ReclamationMaximumSmokeBirthMultiplier;
            effect.UserFadeMultiplier = 1f;
        }

        private static MatrixD CreateReclamationSmokeMatrix(IMyTerminalBlock block)
        {
            var outputDirection = GetReclamationOutputDirection(block);
            var outputDepth = GetReclamationSpawnerOutputDepth(block, outputDirection);
            var position = block.GetPosition() +
                           outputDirection * (outputDepth + ReclamationSmokeSurfaceOffset);

            // Smoke_LargeGunShot emits along local -Z, represented by MatrixD.Forward.
            return MatrixD.CreateWorld(
                position,
                outputDirection,
                Vector3D.Normalize(block.WorldMatrix.Up));
        }

        private void BeginReclamationBurstSmoke(IMyTerminalBlock block, int frames, bool synchronize)
        {
            if (!IsReclamationSpawner(block) || MyAPIGateway.Session == null)
                return;

            var untilFrame = MyAPIGateway.Session.GameplayFrameCounter + Math.Max(1, frames);
            int currentUntilFrame;
            if (!reclamationBurstSmokeUntilFrame.TryGetValue(block.EntityId, out currentUntilFrame) ||
                currentUntilFrame < untilFrame)
                reclamationBurstSmokeUntilFrame[block.EntityId] = untilFrame;

            if (synchronize)
                BroadcastReclamationSmokeState(block.EntityId, true, frames / 60f);
        }

        private void EndReclamationBurstSmoke(long blockEntityId, bool synchronize)
        {
            reclamationBurstSmokeUntilFrame.Remove(blockEntityId);
            StopReclamationSmokeEffect(blockEntityId, false);
            if (synchronize)
                BroadcastReclamationSmokeState(blockEntityId, false, 0f);
        }

        private void ApplySynchronizedReclamationSmokeState(long blockEntityId, bool active, float seconds)
        {
            if (MyAPIGateway.Session == null)
                return;

            if (!active)
            {
                reclamationBurstSmokeUntilFrame.Remove(blockEntityId);
                StopReclamationSmokeEffect(blockEntityId, false);
                return;
            }

            reclamationBurstSmokeUntilFrame[blockEntityId] =
                MyAPIGateway.Session.GameplayFrameCounter + Math.Max(1, (int)Math.Ceiling(seconds * 60f));
        }

        private void StopReclamationSmokeEffect(long blockEntityId, bool instant)
        {
            MyParticleEffect effect;
            if (!reclamationSmokeEffects.TryGetValue(blockEntityId, out effect))
                return;

            reclamationSmokeEffects.Remove(blockEntityId);
            if (effect == null)
                return;

            if (instant)
                effect.Stop(true);
            else
                effect.StopEmitting();
        }

        private void UnloadReclamationSpawnerParticles()
        {
            var entityIds = new List<long>(reclamationSmokeEffects.Keys);
            for (var i = 0; i < entityIds.Count; i++)
                StopReclamationSmokeEffect(entityIds[i], true);

            reclamationSmokeEffects.Clear();
            reclamationBurstSmokeUntilFrame.Clear();
        }
    }
}
