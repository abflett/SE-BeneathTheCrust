using System;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void RegisterResearchTerminalNetworkHandler()
        {
            if (researchTerminalSyncRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ResearchTerminalSyncNetworkMessageId, OnResearchTerminalSyncMessage);
            researchTerminalSyncRegistered = true;
        }

        private void UnregisterResearchTerminalNetworkHandler()
        {
            if (!researchTerminalSyncRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ResearchTerminalSyncNetworkMessageId, OnResearchTerminalSyncMessage);
            researchTerminalSyncRegistered = false;
        }

        private void SendResearchTerminalSyncRequest(IMyTerminalBlock block)
        {
            if (block == null || MyAPIGateway.Multiplayer == null)
                return;

            var message = new ResearchTerminalSyncMessage
            {
                Kind = ResearchTerminalSyncRequest,
                BlockEntityId = block.EntityId,
            };

            MyAPIGateway.Multiplayer.SendMessageToServer(ResearchTerminalSyncNetworkMessageId, XmlNetworkSerializer.Serialize(message));
        }

        private void OnResearchTerminalSyncMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != ResearchTerminalSyncNetworkMessageId)
                return;

            ResearchTerminalSyncMessage message;
            if (!XmlNetworkSerializer.TryDeserialize(messageBytes, "research terminal sync message", out message) || message == null || string.IsNullOrWhiteSpace(message.Kind))
                return;

            if (MyAPIGateway.Multiplayer != null &&
                MyAPIGateway.Multiplayer.IsServer &&
                !isFromServer &&
                message.Kind.Equals(ResearchTerminalSyncRequest, StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchTerminalSyncRequest(message, sender);
                return;
            }

            if ((MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer) &&
                isFromServer &&
                message.Kind.Equals(ResearchTerminalSyncResponse, StringComparison.OrdinalIgnoreCase))
                ApplyResearchTerminalSyncResponse(message);
        }

        private void HandleResearchTerminalSyncRequest(ResearchTerminalSyncMessage request, ulong sender)
        {
            var identityId = ResolveIdentityId(sender);
            var block = MyEntities.GetEntityById(request.BlockEntityId) as IMyTerminalBlock;

            string responseText;
            var success = ExecuteResearchTerminalSync(block, identityId, false, out responseText);

            var response = new ResearchTerminalSyncMessage
            {
                Kind = ResearchTerminalSyncResponse,
                BlockEntityId = request.BlockEntityId,
                IdentityId = identityId,
                Success = success,
                Message = responseText,
            };

            if (MyAPIGateway.Multiplayer != null)
                MyAPIGateway.Multiplayer.SendMessageTo(ResearchTerminalSyncNetworkMessageId, XmlNetworkSerializer.Serialize(response), sender);
        }

        private void ApplyResearchTerminalSyncResponse(ResearchTerminalSyncMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Message) || MyAPIGateway.Utilities == null)
                return;

            ShowWkChatSection("Research Sync", message.Message);
        }

    }
}
