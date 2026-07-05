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
        private void RegisterProficiencyNetworkHandler()
        {
            GetProficiencyDisplaySyncTransport().Register();
        }

        private void UnregisterProficiencyNetworkHandler()
        {
            if (proficiencyDisplaySyncTransport != null)
                proficiencyDisplaySyncTransport.Unregister();
        }

        private void RequestProficiencyDisplaySyncIfReady()
        {
            GetProficiencyDisplaySyncTransport().RequestIfReady(simulationTick, ProficiencyDisplayNetworkRequestTicks, ref lastProficiencyDisplayNetworkRequestTick);
        }

        private void SendProficiencyDisplayResponseToIdentity(long identityId)
        {
            var player = FindPlayerByIdentity(identityId);
            if (player == null || player.SteamUserId == 0)
                return;

            if (MyAPIGateway.Multiplayer != null && player.SteamUserId == MyAPIGateway.Multiplayer.MyId)
                return;

            SendProficiencyDisplayResponse(player.SteamUserId);
        }

        private void SendProficiencyDisplayResponse(ulong recipientSteamId)
        {
            if (recipientSteamId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var identityId = ResolveIdentityId(recipientSteamId);
            var response = new ProficiencyDisplaySyncMessage
            {
                Kind = ProficiencyDisplaySyncResponse,
                IdentityId = identityId,
                Proficiencies = CopyProficiencyRecords(FindProficiencyScope(identityId.ToString())),
            };

            GetProficiencyDisplaySyncTransport().SendTo(recipientSteamId, response);
        }

        private void ApplyProficiencyDisplayResponse(ProficiencyDisplaySyncMessage message)
        {
            if (message == null || message.IdentityId == 0)
                return;

            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null || MyAPIGateway.Session.Player.IdentityId != message.IdentityId)
                return;

            syncedLocalProficiencyScope = new ProficiencyScopeRecord
            {
                Id = message.IdentityId.ToString(),
                Proficiencies = message.Proficiencies == null ? new List<ProficiencyRecord>() : message.Proficiencies,
            };
            NormalizeProficiencyScope(syncedLocalProficiencyScope);
            UpdateLocalProficiencyDisplaySnapshot();
        }

        private DisplaySyncTransport<ProficiencyDisplaySyncMessage> GetProficiencyDisplaySyncTransport()
        {
            if (proficiencyDisplaySyncTransport == null)
                proficiencyDisplaySyncTransport = new DisplaySyncTransport<ProficiencyDisplaySyncMessage>(
                    ProficiencyDisplayNetworkMessageId,
                    "proficiency display",
                    ProficiencyDisplaySyncRequest,
                    ProficiencyDisplaySyncResponse,
                    CreateProficiencyDisplaySyncRequest,
                    message => message == null ? null : message.Kind,
                    SendProficiencyDisplayResponse,
                    ApplyProficiencyDisplayResponse);

            return proficiencyDisplaySyncTransport;
        }

        private ProficiencyDisplaySyncMessage CreateProficiencyDisplaySyncRequest()
        {
            return new ProficiencyDisplaySyncMessage
            {
                Kind = ProficiencyDisplaySyncRequest,
            };
        }

    }
}
