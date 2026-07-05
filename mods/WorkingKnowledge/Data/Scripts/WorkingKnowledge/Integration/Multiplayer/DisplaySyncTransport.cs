using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;

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
            MyAPIGateway.Multiplayer.SendMessageToServer(messageId, Serialize(createRequest()));
        }

        internal void SendTo(ulong recipientSteamId, TMessage message)
        {
            if (recipientSteamId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            MyAPIGateway.Multiplayer.SendMessageTo(messageId, Serialize(message), recipientSteamId);
        }

        private void OnNetworkMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != messageId)
                return;

            TMessage message;
            if (!TryDeserialize(messageBytes, out message) || message == null)
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

        private byte[] Serialize(TMessage message)
        {
            if (message == null || MyAPIGateway.Utilities == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(MyAPIGateway.Utilities.SerializeToXML(message));
        }

        private bool TryDeserialize(byte[] messageBytes, out TMessage message)
        {
            message = null;
            if (messageBytes == null || messageBytes.Length == 0 || MyAPIGateway.Utilities == null)
                return false;

            try
            {
                var xml = Encoding.UTF8.GetString(messageBytes);
                message = MyAPIGateway.Utilities.SerializeFromXML<TMessage>(xml);
                return message != null;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " failed to read " + logName + " sync message: " + exception.Message);
                return false;
            }
        }
    }
}
