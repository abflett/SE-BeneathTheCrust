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

        private const float SimulationStepSeconds = 1f / 60f;
        private const int GravityAssistPlayerRangeCheckFrames = 60;
        private const double GravityAssistPlayerRange = 250.0;
        private const double GravityAssistPlayerRangeSquared =
            GravityAssistPlayerRange * GravityAssistPlayerRange;

        private readonly Dictionary<long, ReclamationGravityAssist> reclamationGravityAssists =
            new Dictionary<long, ReclamationGravityAssist>();

        private readonly List<long> reclamationGravityAssistRemovalBuffer = new List<long>();
        private readonly List<IMyPlayer> reclamationGravityAssistPlayerBuffer = new List<IMyPlayer>();

        private int nextReclamationGravityAssistPlayerRangeCheckFrame;

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
                HasNearbyPlayer = true,
            };
        }

        private void UpdateReclamationGravityAssists()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer ||
                MyAPIGateway.Entities == null || reclamationGravityAssists.Count == 0)
                return;

            var frame = MyAPIGateway.Session.GameplayFrameCounter;
            var checkPlayerRange = frame >= nextReclamationGravityAssistPlayerRangeCheckFrame;
            if (checkPlayerRange)
            {
                nextReclamationGravityAssistPlayerRangeCheckFrame =
                    frame + GravityAssistPlayerRangeCheckFrames;
                reclamationGravityAssistPlayerBuffer.Clear();
                if (MyAPIGateway.Players != null)
                    MyAPIGateway.Players.GetPlayers(reclamationGravityAssistPlayerBuffer);
            }

            reclamationGravityAssistRemovalBuffer.Clear();
            foreach (var pair in reclamationGravityAssists)
            {
                var assist = pair.Value;
                var grid = MyAPIGateway.Entities.GetEntityById(assist.GridEntityId) as IMyCubeGrid;
                if (grid == null || grid.Closed || grid.Physics == null || grid.IsStatic)
                {
                    reclamationGravityAssistRemovalBuffer.Add(assist.GridEntityId);
                    continue;
                }

                if (checkPlayerRange)
                    assist.HasNearbyPlayer = IsAnyPlayerNearReclamationGrid(grid);

                if (!assist.HasNearbyPlayer)
                    continue;

                grid.Physics.LinearVelocity +=
                    (Vector3)(assist.Direction * (assist.Acceleration * SimulationStepSeconds));
            }

            for (var i = 0; i < reclamationGravityAssistRemovalBuffer.Count; i++)
                reclamationGravityAssists.Remove(reclamationGravityAssistRemovalBuffer[i]);
        }

        private bool IsAnyPlayerNearReclamationGrid(IMyCubeGrid grid)
        {
            var gridPosition = grid.GetPosition();
            for (var i = 0; i < reclamationGravityAssistPlayerBuffer.Count; i++)
            {
                var player = reclamationGravityAssistPlayerBuffer[i];
                if (player != null &&
                    Vector3D.DistanceSquared(player.GetPosition(), gridPosition) <=
                    GravityAssistPlayerRangeSquared)
                    return true;
            }

            return false;
        }

        private void UnloadReclamationGravityAssists()
        {
            reclamationGravityAssists.Clear();
            reclamationGravityAssistRemovalBuffer.Clear();
            reclamationGravityAssistPlayerBuffer.Clear();
        }
    }
}
