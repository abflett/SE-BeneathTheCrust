using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;



namespace WkKn
{
    public partial class WkKnSession
    {
        internal static ProficiencyDisplaySnapshot GetProficiencyDisplaySnapshot()
        {
            lock (ProficiencyDisplaySnapshotLock)
                return currentProficiencyDisplaySnapshot == null ? ProficiencyDisplaySnapshot.CreateMessage("No Proficiency data available.") : currentProficiencyDisplaySnapshot.Clone();
        }

        internal static long GetProficiencyDisplayCycleTick()
        {
            var session = activeSession;
            if (session != null)
                return session.simulationTick;

            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond * 60L;
        }

        private static void SetProficiencyDisplaySnapshot(ProficiencyDisplaySnapshot snapshot)
        {
            lock (ProficiencyDisplaySnapshotLock)
                currentProficiencyDisplaySnapshot = snapshot ?? ProficiencyDisplaySnapshot.CreateMessage("No Proficiency data available.");
        }

        private void RefreshLocalProficiencyDisplayIfReady()
        {
            if (simulationTick - lastProficiencyDisplayRefreshTick < ProficiencyDisplayRefreshTicks)
                return;

            lastProficiencyDisplayRefreshTick = simulationTick;
            UpdateLocalProficiencyDisplaySnapshot();
        }

        private void UpdateLocalProficiencyDisplaySnapshot()
        {
            SetProficiencyDisplaySnapshot(BuildLocalProficiencyDisplaySnapshot());
        }

        private ProficiencyDisplaySnapshot BuildLocalProficiencyDisplaySnapshot()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                return ProficiencyDisplaySnapshot.CreateMessage("No local player detected.");

            var player = MyAPIGateway.Session.Player;
            return proficiencyDisplayService.BuildSnapshot(player.DisplayName, simulationTick, config.ProficiencyEnabled, GetLocalProficiencyDisplayScope(player.IdentityId), GetSchematicDisplayName);
        }

        private ProficiencyScopeRecord GetLocalProficiencyDisplayScope(long identityId)
        {
            if (identityId == 0)
                return null;

            if (MyAPIGateway.Multiplayer != null &&
                !MyAPIGateway.Multiplayer.IsServer &&
                syncedLocalProficiencyScope != null &&
                string.Equals(syncedLocalProficiencyScope.Id, identityId.ToString(), StringComparison.OrdinalIgnoreCase))
                return syncedLocalProficiencyScope;

            return FindProficiencyScope(identityId.ToString());
        }

        private void NotifyProficiencyDisplayChanged(long identityId)
        {
            if (identityId == 0)
                return;

            if (IsLocalPlayerIdentity(identityId))
                UpdateLocalProficiencyDisplaySnapshot();

            if (MyAPIGateway.Multiplayer != null && MyAPIGateway.Multiplayer.IsServer)
                SendProficiencyDisplayResponseToIdentity(identityId);
        }

        private bool IsLocalPlayerIdentity(long identityId)
        {
            return MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.Player != null &&
                   MyAPIGateway.Session.Player.IdentityId == identityId;
        }

        private List<ProficiencyRecord> CopyProficiencyRecords(ProficiencyScopeRecord scope)
        {
            return proficiencyDisplayService.CopyRecords(scope);
        }

    }
}
