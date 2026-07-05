using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void InitializeResearchDisplayModule()
        {
            activeSession = this;
            SetResearchDisplaySnapshot(ResearchDisplaySnapshot.CreateMessage("Working Knowledge is loading."));
            RegisterResearchDisplayNetworkHandler();
        }

        private void UpdateResearchDisplayModule()
        {
            RegisterResearchDisplayNetworkHandler();
            RefreshLocalResearchDisplayIfReady();
            RequestResearchDisplaySyncIfReady();
        }

        private void ClearResearchDisplayRuntimeState()
        {
            UnregisterResearchDisplayNetworkHandler();
            syncedLocalResearchDisplay = null;
            SetResearchDisplaySnapshot(ResearchDisplaySnapshot.CreateMessage("Working Knowledge is offline."));
        }

        internal static ResearchDisplaySnapshot GetResearchDisplaySnapshot()
        {
            lock (ResearchDisplaySnapshotLock)
                return currentResearchDisplaySnapshot == null ? ResearchDisplaySnapshot.CreateMessage("No research data available.") : currentResearchDisplaySnapshot.Clone();
        }

        internal static long GetResearchDisplayCycleTick()
        {
            var session = activeSession;
            if (session != null)
                return session.simulationTick;

            return DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond * 60L;
        }

        internal static IdentityDisplaySnapshot GetIdentityDisplaySnapshot()
        {
            var session = activeSession;
            if (session == null)
            {
                return new IdentityDisplaySnapshot
                {
                    Message = "Working Knowledge is offline.",
                    PlayerName = "Local Player",
                    FactionName = "No Faction",
                    FactionTag = string.Empty,
                    FactionIconColor = new Color(180, 255, 200),
                    FactionBackgroundColor = Color.Black,
                };
            }

            return session.BuildLocalIdentityDisplaySnapshot();
        }

        private IdentityDisplaySnapshot BuildLocalIdentityDisplaySnapshot()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
            {
                return new IdentityDisplaySnapshot
                {
                    Message = "No local player detected.",
                    PlayerName = "Local Player",
                    FactionName = "No Faction",
                    FactionTag = string.Empty,
                    FactionIconColor = new Color(180, 255, 200),
                    FactionBackgroundColor = Color.Black,
                };
            }

            var player = MyAPIGateway.Session.Player;
            var snapshot = new IdentityDisplaySnapshot
            {
                PlayerName = string.IsNullOrWhiteSpace(player.DisplayName) ? "Local Player" : player.DisplayName,
                FactionName = "No Faction",
                FactionTag = string.Empty,
                HasFaction = false,
                FactionIconColor = new Color(180, 255, 200),
                FactionBackgroundColor = Color.Black,
            };

            IMyFaction faction;
            if (!TryGetPlayerFaction(player.IdentityId, out faction) || faction == null)
                return snapshot;

            snapshot.HasFaction = true;
            snapshot.FactionName = string.IsNullOrWhiteSpace(faction.Name) ? faction.Tag : faction.Name;
            snapshot.FactionTag = faction.Tag;
            snapshot.FactionIconName = faction.FactionIcon.HasValue ? faction.FactionIcon.Value.String : null;
            snapshot.FactionIconColor = GetFactionDisplayColor(faction.IconColor, new Color(180, 255, 200));
            snapshot.FactionBackgroundColor = GetFactionDisplayColor(faction.CustomColor, new Color(22, 35, 28));
            return snapshot;
        }

        private static Color GetFactionDisplayColor(Vector3 color, Color fallback)
        {
            if (float.IsNaN(color.X) || float.IsNaN(color.Y) || float.IsNaN(color.Z) ||
                float.IsInfinity(color.X) || float.IsInfinity(color.Y) || float.IsInfinity(color.Z))
                return fallback;

            return ColorExtensions.HSVtoColor(VRage.Game.MyColorPickerConstants.HSVOffsetToHSV(color));
        }

        private static void SetResearchDisplaySnapshot(ResearchDisplaySnapshot snapshot)
        {
            lock (ResearchDisplaySnapshotLock)
                currentResearchDisplaySnapshot = snapshot ?? ResearchDisplaySnapshot.CreateMessage("No research data available.");
        }

        private void RefreshLocalResearchDisplayIfReady()
        {
            if (simulationTick - lastResearchDisplayRefreshTick < ResearchDisplayRefreshTicks)
                return;

            lastResearchDisplayRefreshTick = simulationTick;
            UpdateLocalResearchDisplaySnapshot();
        }

        private void UpdateLocalResearchDisplaySnapshot()
        {
            SetResearchDisplaySnapshot(BuildLocalResearchDisplaySnapshot());
        }

        private ResearchDisplaySnapshot BuildLocalResearchDisplaySnapshot()
        {
            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                return ResearchDisplaySnapshot.CreateMessage("No local player detected.");

            var player = MyAPIGateway.Session.Player;
            var identityId = player.IdentityId;
            var syncData = GetLocalResearchDisplayData(identityId);

            IMyFaction localFaction;
            ResearchDisplayScopeRecord factionScope = null;
            if (TryGetPlayerFaction(identityId, out localFaction))
                factionScope = researchDisplayService.CreateFactionScope(localFaction.Name, localFaction.Tag, FindScope(researchStore.FactionScopes, localFaction.FactionId.ToString()));

            return researchDisplayService.BuildSnapshot(
                simulationTick,
                syncData,
                researchDisplayService.CreatePlayerScope(player.DisplayName, FindScope(researchStore.PlayerScopes, identityId.ToString())),
                factionScope,
                IsResearchVisibleToLocalPlayer,
                GetSchematicDisplayName);
        }

        private ResearchDisplaySyncMessage GetLocalResearchDisplayData(long identityId)
        {
            if (identityId == 0)
                return null;

            if (MyAPIGateway.Multiplayer != null &&
                !MyAPIGateway.Multiplayer.IsServer &&
                syncedLocalResearchDisplay != null &&
                syncedLocalResearchDisplay.IdentityId == identityId)
                return syncedLocalResearchDisplay;

            return CreateResearchDisplaySyncMessage(identityId);
        }

        private ResearchDisplaySyncMessage CreateResearchDisplaySyncMessage(long identityId)
        {
            var player = FindPlayerByIdentity(identityId);
            var playerName = player != null && !string.IsNullOrWhiteSpace(player.DisplayName) ? player.DisplayName : "Local Player";

            IMyFaction faction;
            var factionScope = default(ResearchScopeRecord);
            var factionName = default(string);
            var factionTag = default(string);
            if (TryGetPlayerFaction(identityId, out faction))
            {
                factionScope = FindScope(researchStore.FactionScopes, faction.FactionId.ToString());
                factionName = faction.Name;
                factionTag = faction.Tag;
            }

            return researchDisplayService.CreateSyncMessage(
                ResearchDisplaySyncResponse,
                identityId,
                playerName,
                FindScope(researchStore.PlayerScopes, identityId.ToString()),
                factionName,
                factionTag,
                factionScope);
        }

        private void NotifyResearchDisplayChanged(long identityId)
        {
            if (identityId == 0)
                return;

            if (IsLocalPlayerIdentity(identityId))
                UpdateLocalResearchDisplaySnapshot();

            if (MyAPIGateway.Multiplayer != null && MyAPIGateway.Multiplayer.IsServer)
                SendResearchDisplayResponseToIdentity(identityId);
        }

        private void NotifyFactionResearchDisplayChanged(long factionId)
        {
            if (factionId == 0 || MyAPIGateway.Players == null)
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);
            foreach (var player in players)
            {
                IMyFaction faction;
                if (player != null && TryGetPlayerFaction(player.IdentityId, out faction) && faction.FactionId == factionId)
                    NotifyResearchDisplayChanged(player.IdentityId);
            }
        }

        private void RegisterResearchDisplayNetworkHandler()
        {
            GetResearchDisplaySyncTransport().Register();
        }

        private void UnregisterResearchDisplayNetworkHandler()
        {
            if (researchDisplaySyncTransport != null)
                researchDisplaySyncTransport.Unregister();
        }

        private void RequestResearchDisplaySyncIfReady()
        {
            GetResearchDisplaySyncTransport().RequestIfReady(simulationTick, ResearchDisplayNetworkRequestTicks, ref lastResearchDisplayNetworkRequestTick);
        }

        private void SendResearchDisplayResponseToIdentity(long identityId)
        {
            var player = FindPlayerByIdentity(identityId);
            if (player == null || player.SteamUserId == 0)
                return;

            if (MyAPIGateway.Multiplayer != null && player.SteamUserId == MyAPIGateway.Multiplayer.MyId)
                return;

            SendResearchDisplayResponse(player.SteamUserId);
        }

        private void SendResearchDisplayResponse(ulong recipientSteamId)
        {
            if (recipientSteamId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var identityId = ResolveIdentityId(recipientSteamId);
            GetResearchDisplaySyncTransport().SendTo(recipientSteamId, CreateResearchDisplaySyncMessage(identityId));
        }

        private void ApplyResearchDisplayResponse(ResearchDisplaySyncMessage message)
        {
            if (message == null || message.IdentityId == 0)
                return;

            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null || MyAPIGateway.Session.Player.IdentityId != message.IdentityId)
                return;

            syncedLocalResearchDisplay = message;
            researchDisplayService.NormalizeSyncMessage(syncedLocalResearchDisplay);
            UpdateLocalResearchDisplaySnapshot();
        }

        private DisplaySyncTransport<ResearchDisplaySyncMessage> GetResearchDisplaySyncTransport()
        {
            if (researchDisplaySyncTransport == null)
                researchDisplaySyncTransport = new DisplaySyncTransport<ResearchDisplaySyncMessage>(
                    ResearchDisplayNetworkMessageId,
                    "research display",
                    ResearchDisplaySyncRequest,
                    ResearchDisplaySyncResponse,
                    CreateResearchDisplaySyncRequest,
                    message => message == null ? null : message.Kind,
                    SendResearchDisplayResponse,
                    ApplyResearchDisplayResponse);

            return researchDisplaySyncTransport;
        }

        private ResearchDisplaySyncMessage CreateResearchDisplaySyncRequest()
        {
            return new ResearchDisplaySyncMessage
            {
                Kind = ResearchDisplaySyncRequest,
            };
        }
    }
}
