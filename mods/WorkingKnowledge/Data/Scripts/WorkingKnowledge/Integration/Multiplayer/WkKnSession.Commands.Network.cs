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
        private const string CommandSettingsRequestKind = "SettingsRequest";

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

        private bool SendCommandRequestToServer(string commandText, bool silent = false)
        {
            if (string.IsNullOrWhiteSpace(commandText) ||
                commandText.Length > MaxCommandTextLength ||
                MyAPIGateway.Multiplayer == null)
                return false;

            var request = new CommandNetworkMessage
            {
                Kind = CommandRequestKind,
                CommandText = commandText,
                Silent = silent,
            };

            return MyAPIGateway.Multiplayer.SendMessageToServer(
                CommandRequestNetworkMessageId,
                XmlNetworkSerializer.Serialize(request));
        }

        private bool SendSettingsRequestToServer()
        {
            if (MyAPIGateway.Multiplayer == null)
                return false;

            return MyAPIGateway.Multiplayer.SendMessageToServer(
                CommandRequestNetworkMessageId,
                XmlNetworkSerializer.Serialize(new CommandNetworkMessage { Kind = CommandSettingsRequestKind }));
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
                    ApplyRichHudSettingsState(message);
                return;
            }

            if (isFromServer ||
                sender == 0 ||
                (!message.Kind.Equals(CommandRequestKind, StringComparison.OrdinalIgnoreCase) &&
                 !message.Kind.Equals(CommandSettingsRequestKind, StringComparison.OrdinalIgnoreCase)))
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
                if (message.Kind.Equals(CommandRequestKind, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(message.CommandText))
                        return;

                    var commandText = message.CommandText.Trim();
                    if (commandText.Length > MaxCommandTextLength)
                        return;

                    var previousSuppressFeedback = suppressWkCommandFeedback;
                    suppressWkCommandFeedback = message.Silent;
                    try
                    {
                        ExecuteWorkingKnowledgeCommand(sender, commandText, identityId);
                    }
                    finally
                    {
                        suppressWkCommandFeedback = previousSuppressFeedback;
                    }
                }

                SendCommandStateToPlayer(sender, identityId, message.CommandText);
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to execute command request: " + exception);
                ShowWkTargetChatMessage(identityId, "Warning", "Working Knowledge could not execute that command. Check the server log for details.", "Red");
            }
        }

        private void SendCommandStateToPlayer(ulong recipientSteamId, long identityId, string responseCommandText = null)
        {
            if (recipientSteamId == 0 || identityId == 0 || MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var response = new CommandNetworkMessage
            {
                Kind = CommandStateKind,
                CommandText = responseCommandText,
                PlayerConfig = GetPlayerConfig(identityId),
                WorldConfig = config,
                CanEditWorldConfig = CanEditConfig(recipientSteamId),
            };

            MyAPIGateway.Multiplayer.SendMessageTo(
                CommandRequestNetworkMessageId,
                XmlNetworkSerializer.Serialize(response),
                recipientSteamId);
        }

        private void ApplyRichHudSettingsState(CommandNetworkMessage message)
        {
            if (message == null)
                return;

            playerConfigStore.ApplySyncedPlayer(message.PlayerConfig);
            richHudWorldConfigSnapshot = message.WorldConfig;
            richHudCanEditWorldConfig = message.CanEditWorldConfig;
            richHudSettingsStateRequested = false;
            if (richHudSettingsMenu != null)
                richHudSettingsMenu.AcknowledgeCommand(message.CommandText);
        }
    }
}
