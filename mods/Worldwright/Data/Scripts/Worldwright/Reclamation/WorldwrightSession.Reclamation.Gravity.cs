using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        internal const float MaximumGravityAssistAcceleration = 9.81f;

        private const int GravityAssistDurationFrames = 60 * 60;
        private const float SimulationStepSeconds = 1f / 60f;

        private readonly Dictionary<long, ReclamationGravityAssist> reclamationGravityAssists =
            new Dictionary<long, ReclamationGravityAssist>();

        private readonly List<long> reclamationGravityAssistRemovalBuffer = new List<long>();

        private void RegisterReclamationGravityAssist(
            IMyTerminalBlock spawner,
            IMyCubeGrid spawnedGrid,
            float acceleration)
        {
            if (spawner == null || spawnedGrid == null || MyAPIGateway.Session == null ||
                acceleration <= 0.001f)
                return;

            reclamationGravityAssists[spawnedGrid.EntityId] = new ReclamationGravityAssist
            {
                GridEntityId = spawnedGrid.EntityId,
                Direction = Vector3D.Normalize(spawner.CubeGrid.WorldMatrix.Down),
                Acceleration = Math.Max(0f, Math.Min(MaximumGravityAssistAcceleration, acceleration)),
                ExpiresAtFrame = MyAPIGateway.Session.GameplayFrameCounter + GravityAssistDurationFrames,
            };
        }

        private void UpdateReclamationGravityAssists()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer ||
                MyAPIGateway.Entities == null || reclamationGravityAssists.Count == 0)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            reclamationGravityAssistRemovalBuffer.Clear();
            foreach (var pair in reclamationGravityAssists)
            {
                var assist = pair.Value;
                var grid = MyAPIGateway.Entities.GetEntityById(assist.GridEntityId) as IMyCubeGrid;
                if (grid == null || grid.Closed || grid.Physics == null || grid.IsStatic ||
                    frame >= assist.ExpiresAtFrame)
                {
                    reclamationGravityAssistRemovalBuffer.Add(assist.GridEntityId);
                    continue;
                }

                grid.Physics.LinearVelocity +=
                    (Vector3)(assist.Direction * (assist.Acceleration * SimulationStepSeconds));
            }

            for (var i = 0; i < reclamationGravityAssistRemovalBuffer.Count; i++)
                reclamationGravityAssists.Remove(reclamationGravityAssistRemovalBuffer[i]);
        }

        private void UnloadReclamationGravityAssists()
        {
            reclamationGravityAssists.Clear();
            reclamationGravityAssistRemovalBuffer.Clear();
        }
    }
}
