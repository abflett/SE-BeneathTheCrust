using System;
using Sandbox.ModAPI;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int MaxCommandRequestBytes = 8192;
        private const int MaxCommandTextLength = 2048;
        private const string CommandRequestKind = "Request";
        private const string CommandStateKind = "State";

        private void RegisterCommandRequestNetworkHandler()
        {
            if (commandRequestNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(CommandRequestNetworkMessageId, OnCommandRequestNetworkMessage);
            commandRequestNetworkRegistered = true;
        }

        private void UnregisterCommandRequestNetworkHandler()
        {
            if (!commandRequestNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(CommandRequestNetworkMessageId, OnCommandRequestNetworkMessage);
            commandRequestNetworkRegistered = false;
        }

        private bool SendCommandRequestToServer(string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText) ||
                commandText.Length > MaxCommandTextLength ||
                MyAPIGateway.Multiplayer == null)
                return false;

            var request = new CommandNetworkMessage
            {
                Kind = CommandRequestKind,
                CommandText = commandText,
            };

            return MyAPIGateway.Multiplayer.SendMessageToServer(
                CommandRequestNetworkMessageId,
                XmlNetworkSerializer.Serialize(request));
        }

        private void OnCommandRequestNetworkMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != CommandRequestNetworkMessageId ||
                MyAPIGateway.Multiplayer == null ||
                messageBytes == null ||
                messageBytes.Length == 0 ||
                messageBytes.Length > MaxCommandRequestBytes)
                return;

            CommandNetworkMessage message;
            if (!XmlNetworkSerializer.TryDeserialize(messageBytes, "command network message", out message) ||
                message == null ||
                string.IsNullOrWhiteSpace(message.Kind))
                return;

            if (!MyAPIGateway.Multiplayer.IsServer)
            {
                if (isFromServer && message.Kind.Equals(CommandStateKind, StringComparison.OrdinalIgnoreCase))
                    playerConfigStore.ApplySyncedPlayer(message.PlayerConfig);
                return;
            }

            if (isFromServer ||
                sender == 0 ||
                !message.Kind.Equals(CommandRequestKind, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(message.CommandText))
                return;

            var commandText = message.CommandText.Trim();
            if (commandText.Length > MaxCommandTextLength)
                return;

            var identityId = MyAPIGateway.Players != null
                ? MyAPIGateway.Players.TryGetIdentityId(sender)
                : 0;
            var player = identityId != 0 ? FindPlayerByIdentity(identityId) : null;
            if (player == null || player.SteamUserId != sender)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " rejected command request from an unresolved sender.");
                return;
            }

            try
            {
                ExecuteWorkingKnowledgeCommand(sender, commandText, identityId);
                SendCommandStateToPlayer(sender, identityId);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to execute command request: " + exception);
                ShowWkTargetChatMessage(identityId, "Warning", "Working Knowledge could not execute that command. Check the server log for details.", "Red");
            }
        }

        private void SendCommandStateToPlayer(ulong recipientSteamId, long identityId)
        {
            if (recipientSteamId == 0 || identityId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var response = new CommandNetworkMessage
            {
                Kind = CommandStateKind,
                PlayerConfig = GetPlayerConfig(identityId),
            };

            MyAPIGateway.Multiplayer.SendMessageTo(
                CommandRequestNetworkMessageId,
                XmlNetworkSerializer.Serialize(response),
                recipientSteamId);
        }
    }
}
