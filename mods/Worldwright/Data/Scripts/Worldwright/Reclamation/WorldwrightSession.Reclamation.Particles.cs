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
        private const int ReclamationBurstSmokeLeadFrames = 60;
        private const double ReclamationSmokeSurfaceOffset = 0.05;

        private readonly Dictionary<long, MyParticleEffect> reclamationSmokeEffects =
            new Dictionary<long, MyParticleEffect>();

        private readonly Dictionary<long, ReclamationSmokeRenderSettings> reclamationSmokeRenderSettings =
            new Dictionary<long, ReclamationSmokeRenderSettings>();

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
            ReclamationSmokeRenderSettings appliedSettings;
            if (reclamationSmokeEffects.TryGetValue(block.EntityId, out effect) &&
                reclamationSmokeRenderSettings.TryGetValue(block.EntityId, out appliedSettings) &&
                !appliedSettings.Matches(config))
            {
                StopReclamationSmokeEffect(block.EntityId, true);
                effect = null;
            }

            if (!reclamationSmokeEffects.TryGetValue(block.EntityId, out effect) || effect == null)
            {
                var position = effectMatrix.Translation;
                if (!MyParticlesManager.TryCreateParticleEffect(
                        GetReclamationSmokeParticleName(config.SmokeEffect),
                        ref effectMatrix,
                        ref position,
                        uint.MaxValue,
                        out effect))
                    return;

                ApplyReclamationSmokeSettings(effect, config);
                effect.Play();
                reclamationSmokeEffects[block.EntityId] = effect;
                reclamationSmokeRenderSettings[block.EntityId] =
                    ReclamationSmokeRenderSettings.FromConfig(config);
            }

            effect.WorldMatrix = effectMatrix;
        }

        private static void ApplyReclamationSmokeSettings(
            MyParticleEffect effect,
            ReclamationSpawnerConfig config)
        {
            effect.UserColorMultiplier = new Vector4(
                config.SmokeRed / 255f,
                config.SmokeGreen / 255f,
                config.SmokeBlue / 255f,
                1f);
            effect.UserBirthMultiplier = config.SmokeIntensity / 100f;
            effect.SoftParticleDistanceScaleMultiplier = config.SmokeSoftness / 100f;
            effect.UserFadeMultiplier = 1f;
        }

        private static string GetReclamationSmokeParticleName(ReclamationSmokeEffect effect)
        {
            switch (effect)
            {
                case ReclamationSmokeEffect.WhiteExhaust:
                    return "ExhaustSmokeWhite";
                case ReclamationSmokeEffect.VehicleExhaust:
                    return "ExhaustCarSmoke";
                case ReclamationSmokeEffect.ReactorExhaust:
                    return "ExhaustSmokeReactor";
                default:
                    return "ExhaustSmoke";
            }
        }

        private static MatrixD CreateReclamationSmokeMatrix(IMyTerminalBlock block)
        {
            var outputDirection = GetReclamationOutputDirection(block);
            var outputDepth = GetReclamationSpawnerOutputDepth(block, outputDirection);
            var position = block.GetPosition() +
                           outputDirection * (outputDepth + ReclamationSmokeSurfaceOffset);

            // ExhaustSmokeWhite emits along its local Y axis, so Up points out through the grille.
            return MatrixD.CreateWorld(
                position,
                Vector3D.Normalize(block.WorldMatrix.Up),
                outputDirection);
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
            reclamationSmokeRenderSettings.Remove(blockEntityId);
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
            reclamationSmokeRenderSettings.Clear();
            reclamationBurstSmokeUntilFrame.Clear();
        }

        private sealed class ReclamationSmokeRenderSettings
        {
            internal ReclamationSmokeEffect Effect;
            internal float Red;
            internal float Green;
            internal float Blue;
            internal float Intensity;
            internal float Softness;

            internal bool Matches(ReclamationSpawnerConfig config)
            {
                return Effect == config.SmokeEffect &&
                       Math.Abs(Red - config.SmokeRed) < 0.01f &&
                       Math.Abs(Green - config.SmokeGreen) < 0.01f &&
                       Math.Abs(Blue - config.SmokeBlue) < 0.01f &&
                       Math.Abs(Intensity - config.SmokeIntensity) < 0.01f &&
                       Math.Abs(Softness - config.SmokeSoftness) < 0.01f;
            }

            internal static ReclamationSmokeRenderSettings FromConfig(ReclamationSpawnerConfig config)
            {
                return new ReclamationSmokeRenderSettings
                {
                    Effect = config.SmokeEffect,
                    Red = config.SmokeRed,
                    Green = config.SmokeGreen,
                    Blue = config.SmokeBlue,
                    Intensity = config.SmokeIntensity,
                    Softness = config.SmokeSoftness,
                };
            }
        }
    }
}
