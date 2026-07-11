using System;
using Sandbox.ModAPI;

namespace WkKn
{
    internal sealed class DisplaySyncTransport<TMessage>
        where TMessage : class
    {
        private readonly ushort messageId;
        private readonly string logName;
        private readonly string requestKind;
        private readonly string responseKind;
        private readonly Func<TMessage> createRequest;
        private readonly Func<TMessage, string> getKind;
        private readonly Action<ulong> sendResponse;
        private readonly Action<TMessage> applyResponse;
        private bool registered;

        internal DisplaySyncTransport(
            ushort messageId,
            string logName,
            string requestKind,
            string responseKind,
            Func<TMessage> createRequest,
            Func<TMessage, string> getKind,
            Action<ulong> sendResponse,
            Action<TMessage> applyResponse)
        {
            this.messageId = messageId;
            this.logName = logName;
            this.requestKind = requestKind;
            this.responseKind = responseKind;
            this.createRequest = createRequest;
            this.getKind = getKind;
            this.sendResponse = sendResponse;
            this.applyResponse = applyResponse;
        }

        internal void Register()
        {
            if (registered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(messageId, OnNetworkMessage);
            registered = true;
        }

        internal void Unregister()
        {
            if (!registered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(messageId, OnNetworkMessage);
            registered = false;
        }

        internal void RequestIfReady(long simulationTick, long requestIntervalTicks, ref long lastRequestTick)
        {
            if (MyAPIGateway.Multiplayer == null ||
                MyAPIGateway.Multiplayer.IsServer ||
                simulationTick - lastRequestTick < requestIntervalTicks)
                return;

            lastRequestTick = simulationTick;
            MyAPIGateway.Multiplayer.SendMessageToServer(messageId, XmlNetworkSerializer.Serialize(createRequest()));
        }

        internal void SendTo(ulong recipientSteamId, TMessage message)
        {
            if (recipientSteamId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            MyAPIGateway.Multiplayer.SendMessageTo(messageId, XmlNetworkSerializer.Serialize(message), recipientSteamId);
        }

        private void OnNetworkMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != messageId)
                return;

            TMessage message;
            if (!XmlNetworkSerializer.TryDeserialize(messageBytes, logName + " sync message", out message) || message == null)
                return;

            var kind = getKind(message);
            if (string.IsNullOrWhiteSpace(kind))
                return;

            if (MyAPIGateway.Multiplayer != null &&
                MyAPIGateway.Multiplayer.IsServer &&
                !isFromServer &&
                kind.Equals(requestKind, StringComparison.OrdinalIgnoreCase))
            {
                sendResponse(sender);
                return;
            }

            if ((MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer) &&
                isFromServer &&
                kind.Equals(responseKind, StringComparison.OrdinalIgnoreCase))
                applyResponse(message);
        }

    }
}
