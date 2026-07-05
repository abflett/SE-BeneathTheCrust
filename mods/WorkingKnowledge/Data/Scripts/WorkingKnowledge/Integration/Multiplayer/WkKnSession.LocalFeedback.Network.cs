using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void RegisterLocalFeedbackNetworkHandler()
        {
            if (localFeedbackNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(LocalFeedbackNetworkMessageId, OnLocalFeedbackNetworkMessage);
            localFeedbackNetworkRegistered = true;
        }

        private void UnregisterLocalFeedbackNetworkHandler()
        {
            if (!localFeedbackNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(LocalFeedbackNetworkMessageId, OnLocalFeedbackNetworkMessage);
            localFeedbackNetworkRegistered = false;
        }

        private void SendCompletionFeedback(long identityId, string displayName, string label, string messageText, string soundSubtype)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(messageText))
                return;

            if (string.IsNullOrWhiteSpace(soundSubtype))
                soundSubtype = GetEffectiveCompletionSoundSubtype(identityId);

            SendLocalFeedback(
                identityId,
                displayName + " " + label + ": " + messageText,
                WkChatCompletionFont,
                IsPlayerProgressToastEnabled(identityId),
                soundSubtype,
                IsPlayerCompletionSoundEnabled(identityId) && !string.IsNullOrWhiteSpace(soundSubtype));
        }

        private void SendLocalFeedback(long identityId, string toastMessage, string toastFont, bool showToast, string soundSubtype, bool playSound)
        {
            SendLocalFeedback(identityId, toastMessage, toastFont, showToast, soundSubtype, playSound, false, Vector3D.Zero);
        }

        private void SendLocalFeedbackAtPosition(long identityId, string toastMessage, string toastFont, bool showToast, string soundSubtype, bool playSound, Vector3D soundPosition)
        {
            SendLocalFeedback(identityId, toastMessage, toastFont, showToast, soundSubtype, playSound, true, soundPosition);
        }

        private void SendLocalSoundAtPositionToNearbyPlayers(Vector3D soundPosition, string soundSubtype, double range)
        {
            if (string.IsNullOrWhiteSpace(soundSubtype) || range <= 0.0 || MyAPIGateway.Players == null)
                return;

            var rangeSquared = range * range;
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player =>
            {
                if (player == null || player.IdentityId == 0 || player.Character == null)
                    return false;

                return (player.Character.WorldMatrix.Translation - soundPosition).LengthSquared() <= rangeSquared;
            });

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                if (player == null || player.IdentityId == 0)
                    continue;

                if (!IsPlayerWeldBotchSoundEnabled(player.IdentityId))
                    continue;

                SendLocalFeedbackAtPosition(player.IdentityId, null, null, false, soundSubtype, true, soundPosition);
            }
        }

        private void QueueLocalSoundAtPositionToNearbyPlayers(Vector3D soundPosition, string soundSubtype, double range)
        {
            if (string.IsNullOrWhiteSpace(soundSubtype) || range <= 0.0)
                return;

            blockWorkState.PendingWeldBotchSounds.Add(new WeldBotchSoundEvent
            {
                Position = soundPosition,
                SoundSubtype = soundSubtype,
                Range = range,
            });
        }

        private void FlushPendingLocalSoundsAtPositionToNearbyPlayers()
        {
            if (blockWorkState.PendingWeldBotchSounds.Count == 0)
                return;

            for (var i = 0; i < blockWorkState.PendingWeldBotchSounds.Count; i++)
            {
                var soundEvent = blockWorkState.PendingWeldBotchSounds[i];
                SendLocalSoundAtPositionToNearbyPlayers(soundEvent.Position, soundEvent.SoundSubtype, soundEvent.Range);
            }

            blockWorkState.PendingWeldBotchSounds.Clear();
        }

        private void SendLocalFeedback(long identityId, string toastMessage, string toastFont, bool showToast, string soundSubtype, bool playSound, bool playSoundAtPosition, Vector3D soundPosition)
        {
            if (identityId == 0)
                return;

            var shouldPlaySound = playSound && !string.IsNullOrWhiteSpace(soundSubtype);
            var message = new LocalFeedbackMessage
            {
                IdentityId = identityId,
                ToastMessage = toastMessage,
                ToastFont = toastFont,
                SoundSubtype = soundSubtype,
                PlaySound = shouldPlaySound,
                PlaySoundAtPosition = shouldPlaySound && playSoundAtPosition,
                SoundX = soundPosition.X,
                SoundY = soundPosition.Y,
                SoundZ = soundPosition.Z,
                ShowToast = showToast && !string.IsNullOrWhiteSpace(toastMessage),
            };

            if (IsLocalIdentity(identityId))
            {
                ApplyLocalFeedback(message);
                return;
            }

            if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var player = FindPlayerByIdentity(identityId);
            if (player == null || player.SteamUserId == 0)
                return;

            MyAPIGateway.Multiplayer.SendMessageTo(LocalFeedbackNetworkMessageId, SerializeLocalFeedbackMessage(message), player.SteamUserId);
        }

        private void OnLocalFeedbackNetworkMessage(ushort handlerId, byte[] messageBytes, ulong sender, bool isFromServer)
        {
            if (handlerId != LocalFeedbackNetworkMessageId || !isFromServer)
                return;

            LocalFeedbackMessage message;
            if (!TryDeserializeLocalFeedbackMessage(messageBytes, out message) || message == null)
                return;

            if (message.IdentityId != 0 && !IsLocalIdentity(message.IdentityId))
                return;

            ApplyLocalFeedback(message);
        }

        private void ApplyLocalFeedback(LocalFeedbackMessage message)
        {
            if (message == null)
                return;

            if (message.PlaySound)
            {
                if (message.PlaySoundAtPosition)
                    PlaySoundAtPositionLocal(message.SoundSubtype, new Vector3D(message.SoundX, message.SoundY, message.SoundZ));
                else
                    PlaySoundLocal(message.SoundSubtype);
            }

            if (message.ShowToast)
                ShowWkToast(message.ToastMessage, message.ToastFont);
        }

        private static void PlaySoundLocal(string soundSubtype)
        {
            if (string.IsNullOrWhiteSpace(soundSubtype))
                return;

            MyVisualScriptLogicProvider.PlaySoundAmbientLocal(soundSubtype);
        }

        private static void PlaySoundAtPositionLocal(string soundSubtype, Vector3D position)
        {
            if (string.IsNullOrWhiteSpace(soundSubtype))
                return;

            MyVisualScriptLogicProvider.PlaySingleSoundAtPositionLocal(soundSubtype, position);
        }

        private static byte[] SerializeLocalFeedbackMessage(LocalFeedbackMessage message)
        {
            if (message == null || MyAPIGateway.Utilities == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(MyAPIGateway.Utilities.SerializeToXML(message));
        }

        private static bool TryDeserializeLocalFeedbackMessage(byte[] messageBytes, out LocalFeedbackMessage message)
        {
            message = null;
            if (messageBytes == null || messageBytes.Length == 0 || MyAPIGateway.Utilities == null)
                return false;

            try
            {
                var xml = Encoding.UTF8.GetString(messageBytes);
                message = MyAPIGateway.Utilities.SerializeFromXML<LocalFeedbackMessage>(xml);
                return message != null;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to read local feedback message: " + exception.Message);
                return false;
            }
        }
    }
}
