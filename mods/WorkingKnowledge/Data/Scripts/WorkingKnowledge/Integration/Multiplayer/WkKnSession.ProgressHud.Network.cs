using System;
using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int MaxProgressHudMessageBytes = 8192;

        private void RegisterProgressHudNetworkHandler()
        {
            if (progressHudNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ProgressHudNetworkMessageId, OnProgressHudNetworkMessage);
            progressHudNetworkRegistered = true;
        }

        private void UnregisterProgressHudNetworkHandler()
        {
            if (!progressHudNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ProgressHudNetworkMessageId, OnProgressHudNetworkMessage);
            progressHudNetworkRegistered = false;
        }

        private void SendProgressHudUpdate(long identityId, string progressId, string displayName)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(progressId))
                return;

            var message = new ProgressHudNetworkMessage
            {
                IdentityId = identityId,
                ProgressId = progressId,
                DisplayName = displayName,
                ResearchProgress = GetPlayerResearchProgress(identityId, progressId),
                ProficiencyProgress = GetPlayerProficiency(identityId, progressId),
            };

            if (IsLocalIdentity(identityId))
            {
                ApplyProgressHudUpdate(message);
                return;
            }

            if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var player = FindPlayerByIdentity(identityId);
            if (player == null || player.SteamUserId == 0)
                return;

            MyAPIGateway.Multiplayer.SendMessageTo(
                ProgressHudNetworkMessageId,
                XmlNetworkSerializer.Serialize(message),
                player.SteamUserId);
        }

        private void OnProgressHudNetworkMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != ProgressHudNetworkMessageId ||
                !isFromServer ||
                MyAPIGateway.Multiplayer == null ||
                MyAPIGateway.Multiplayer.IsServer ||
                messageBytes == null ||
                messageBytes.Length == 0 ||
                messageBytes.Length > MaxProgressHudMessageBytes)
                return;

            ProgressHudNetworkMessage message;
            if (!XmlNetworkSerializer.TryDeserialize(messageBytes, "progress HUD update", out message) || message == null)
                return;

            ApplyProgressHudUpdate(message);
        }

        private void ApplyProgressHudUpdate(ProgressHudNetworkMessage message)
        {
            if (message == null ||
                message.IdentityId == 0 ||
                string.IsNullOrWhiteSpace(message.ProgressId) ||
                !IsLocalIdentity(message.IdentityId))
                return;

            progressHudOverlay.UpdateCombined(
                message.IdentityId,
                message.ProgressId,
                message.DisplayName,
                message.ResearchProgress,
                message.ProficiencyProgress,
                simulationTick);
        }
    }
}
