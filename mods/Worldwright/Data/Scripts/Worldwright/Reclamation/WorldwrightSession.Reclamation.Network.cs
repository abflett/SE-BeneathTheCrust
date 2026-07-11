using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Worldwright
{
    public sealed partial class WorldwrightSession
    {
        private const ushort ReclamationSpawnerNetworkMessageId = 49301;
        private const string ReclamationRequestKind = "request";
        private const string ReclamationResponseKind = "response";
        private const string ReclamationSmokeKind = "smoke";

        private bool reclamationNetworkRegistered;

        private void RegisterReclamationSpawnerNetwork()
        {
            if (reclamationNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(
                ReclamationSpawnerNetworkMessageId,
                OnReclamationSpawnerNetworkMessage);
            reclamationNetworkRegistered = true;
        }

        private void UnregisterReclamationSpawnerNetwork()
        {
            if (!reclamationNetworkRegistered || MyAPIGateway.Multiplayer == null)
                return;

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(
                ReclamationSpawnerNetworkMessageId,
                OnReclamationSpawnerNetworkMessage);
            reclamationNetworkRegistered = false;
        }

        private void RequestReclamationOperation(
            IMyTerminalBlock block,
            string operation,
            string text = null,
            int index = -1,
            float number = 0f)
        {
            if (!IsReclamationSpawner(block))
                return;

            var request = new ReclamationSpawnerNetworkMessage
            {
                Kind = ReclamationRequestKind,
                Operation = operation,
                BlockEntityId = block.EntityId,
                Text = text,
                Index = index,
                Number = number,
            };

            if (MyAPIGateway.Multiplayer == null || MyAPIGateway.Multiplayer.IsServer)
            {
                string response;
                ExecuteReclamationOperation(block, request, true, 0, out response);
                RefreshReclamationSpawnerVisuals(block);
                return;
            }

            MyAPIGateway.Multiplayer.SendMessageToServer(
                ReclamationSpawnerNetworkMessageId,
                SerializeReclamationMessage(request));
        }

        private void OnReclamationSpawnerNetworkMessage(
            ushort handlerId,
            byte[] messageBytes,
            ulong sender,
            bool isFromServer)
        {
            if (handlerId != ReclamationSpawnerNetworkMessageId)
                return;

            ReclamationSpawnerNetworkMessage message;
            if (!TryDeserializeReclamationMessage(messageBytes, out message) ||
                message == null ||
                string.IsNullOrWhiteSpace(message.Kind))
                return;

            if (MyAPIGateway.Multiplayer != null &&
                MyAPIGateway.Multiplayer.IsServer &&
                !isFromServer &&
                message.Kind.Equals(ReclamationRequestKind, StringComparison.OrdinalIgnoreCase))
            {
                HandleReclamationRequest(message, sender);
                return;
            }

            if (isFromServer &&
                message.Kind.Equals(ReclamationSmokeKind, StringComparison.OrdinalIgnoreCase))
            {
                ApplySynchronizedReclamationSmokeState(
                    message.BlockEntityId,
                    message.Success,
                    message.Number);
                return;
            }

            if ((MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer) &&
                isFromServer &&
                message.Kind.Equals(ReclamationResponseKind, StringComparison.OrdinalIgnoreCase))
            {
                var block = MyAPIGateway.Entities.GetEntityById(message.BlockEntityId) as IMyTerminalBlock;
                RefreshReclamationSpawnerVisuals(block);
            }
        }

        private void HandleReclamationRequest(ReclamationSpawnerNetworkMessage request, ulong sender)
        {
            var block = MyAPIGateway.Entities.GetEntityById(request.BlockEntityId) as IMyTerminalBlock;
            var identityId = ResolveReclamationIdentity(sender);
            string responseText;
            var success = ExecuteReclamationOperation(block, request, false, identityId, out responseText);

            var response = new ReclamationSpawnerNetworkMessage
            {
                Kind = ReclamationResponseKind,
                BlockEntityId = request.BlockEntityId,
                Success = success,
                Message = responseText,
            };

            MyAPIGateway.Multiplayer.SendMessageTo(
                ReclamationSpawnerNetworkMessageId,
                SerializeReclamationMessage(response),
                sender);
        }

        private bool ExecuteReclamationOperation(
            IMyTerminalBlock block,
            ReclamationSpawnerNetworkMessage request,
            bool trustedServerAction,
            long identityId,
            out string response)
        {
            response = string.Empty;
            if (!IsReclamationSpawner(block) || block.Closed)
            {
                response = "Block Spawner is not available.";
                return false;
            }

            if (!trustedServerAction &&
                (identityId == 0 || !block.HasPlayerAccess(identityId, MyRelationsBetweenPlayerAndBlock.NoOwnership)))
            {
                response = "You do not have access to this Block Spawner.";
                return false;
            }

            var operation = request.Operation ?? string.Empty;
            if (operation.Equals("spawn", StringComparison.OrdinalIgnoreCase))
                return QueueReclamationSpawn(block, out response);

            var config = ReadReclamationSpawnerConfig(block);
            if (operation.Equals("start", StringComparison.OrdinalIgnoreCase))
            {
                if (config.Entries.Count == 0)
                {
                    response = "The spawn sequence is empty.";
                    return false;
                }

                if (config.Mode == ReclamationSequenceMode.Once && config.Completed)
                {
                    response = "The Once sequence is complete. Reset it before starting again.";
                    return false;
                }

                runningReclamationSpawners[block.EntityId] = new RunningReclamationSpawner
                {
                    BlockEntityId = block.EntityId,
                    NextSpawnFrame = MyAPIGateway.Session.GameplayFrameCounter,
                };

                string spawnResponse;
                var success = QueueReclamationSpawn(block, out spawnResponse);
                response = success ? "Automatic spawning started. " + spawnResponse : spawnResponse;
                if (!success)
                    runningReclamationSpawners.Remove(block.EntityId);
                RefreshReclamationSpawnerVisuals(block);
                return success;
            }
            if (operation.Equals("stop", StringComparison.OrdinalIgnoreCase))
            {
                StopAutomaticReclamationSpawning(block.EntityId);
                response = "Automatic spawning stopped.";
                RefreshReclamationSpawnerVisuals(block);
                return true;
            }
            if (operation.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
                response = "Spawn sequence reset.";
            }
            else if (operation.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                ReclamationBlockCatalogEntry entry;
                if (string.IsNullOrWhiteSpace(request.Text) ||
                    !reclamationBlockCatalogByKey.TryGetValue(request.Text, out entry))
                {
                    response = "Selected block definition is not loaded.";
                    return false;
                }

                config.Entries.Add(entry.Key);
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
            }
            else if (operation.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Index < 0 || request.Index >= config.Entries.Count)
                {
                    response = "Select a sequence entry to remove.";
                    return false;
                }

                config.Entries.RemoveAt(request.Index);
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
            }
            else if (operation.Equals("move-up", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Index <= 0 || request.Index >= config.Entries.Count)
                    return false;

                Swap(config.Entries, request.Index, request.Index - 1);
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
            }
            else if (operation.Equals("move-down", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Index < 0 || request.Index >= config.Entries.Count - 1)
                    return false;

                Swap(config.Entries, request.Index, request.Index + 1);
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
            }
            else if (operation.Equals("mode", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.IsDefined(typeof(ReclamationSequenceMode), request.Index))
                {
                    response = "Unknown sequence mode.";
                    return false;
                }

                config.Mode = (ReclamationSequenceMode)request.Index;
                config.ResetSequence();
                StopAutomaticReclamationSpawning(block.EntityId);
            }
            else if (operation.Equals("velocity", StringComparison.OrdinalIgnoreCase))
            {
                config.OutwardVelocity = Math.Max(0f, Math.Min(MaximumOutwardVelocity, request.Number));
            }
            else if (operation.Equals("interval", StringComparison.OrdinalIgnoreCase))
            {
                config.AutomaticIntervalSeconds = Math.Max(
                    MinimumAutomaticIntervalSeconds,
                    Math.Min(MaximumAutomaticIntervalSeconds, request.Number));
            }
            else if (operation.Equals("rotation", StringComparison.OrdinalIgnoreCase))
            {
                config.RotationVariance = Math.Max(0f, Math.Min(100f, request.Number));
            }
            else if (operation.Equals("minimum-integrity", StringComparison.OrdinalIgnoreCase))
            {
                config.MinimumIntegrity = Math.Max(10f, Math.Min(100f, request.Number));
                if (config.MinimumIntegrity > config.MaximumIntegrity)
                    config.MaximumIntegrity = config.MinimumIntegrity;
            }
            else if (operation.Equals("maximum-integrity", StringComparison.OrdinalIgnoreCase))
            {
                config.MaximumIntegrity = Math.Max(10f, Math.Min(100f, request.Number));
                if (config.MaximumIntegrity < config.MinimumIntegrity)
                    config.MinimumIntegrity = config.MaximumIntegrity;
            }
            else if (operation.Equals("clearance-scale", StringComparison.OrdinalIgnoreCase))
            {
                config.ClearanceScale = Math.Max(0.25f, Math.Min(2f, request.Number));
            }
            else if (operation.Equals("gravity-assist", StringComparison.OrdinalIgnoreCase))
            {
                config.GravityAssistAcceleration = Math.Max(
                    0f,
                    Math.Min(MaximumGravityAssistAcceleration, request.Number));
            }
            else if (operation.Equals("smoke-mode", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.IsDefined(typeof(ReclamationSmokeMode), request.Index))
                {
                    response = "Unknown smoke mode.";
                    return false;
                }

                config.SmokeMode = (ReclamationSmokeMode)request.Index;
                if (config.SmokeMode != ReclamationSmokeMode.Bursts)
                    EndReclamationBurstSmoke(block.EntityId, true);
            }
            else if (operation.Equals("smoke-effect", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.IsDefined(typeof(ReclamationSmokeEffect), request.Index))
                {
                    response = "Unknown smoke effect.";
                    return false;
                }

                config.SmokeEffect = (ReclamationSmokeEffect)request.Index;
            }
            else if (operation.Equals("smoke-red", StringComparison.OrdinalIgnoreCase))
            {
                config.SmokeRed = Math.Max(0f, Math.Min(255f, request.Number));
            }
            else if (operation.Equals("smoke-green", StringComparison.OrdinalIgnoreCase))
            {
                config.SmokeGreen = Math.Max(0f, Math.Min(255f, request.Number));
            }
            else if (operation.Equals("smoke-blue", StringComparison.OrdinalIgnoreCase))
            {
                config.SmokeBlue = Math.Max(0f, Math.Min(255f, request.Number));
            }
            else if (operation.Equals("smoke-intensity", StringComparison.OrdinalIgnoreCase))
            {
                config.SmokeIntensity = Math.Max(10f, Math.Min(100f, request.Number));
            }
            else if (operation.Equals("reset-smoke-tint", StringComparison.OrdinalIgnoreCase))
            {
                config.SmokeRed = 255f;
                config.SmokeGreen = 255f;
                config.SmokeBlue = 255f;
            }
            else if (operation.Equals("add-appearance", StringComparison.OrdinalIgnoreCase))
            {
                config.AppearancePresets.Add(CaptureReclamationAppearance(block));
            }
            else if (operation.Equals("remove-appearance", StringComparison.OrdinalIgnoreCase))
            {
                if (request.Index < 0 || request.Index >= config.AppearancePresets.Count)
                {
                    response = "Select an appearance preset to remove.";
                    return false;
                }

                config.AppearancePresets.RemoveAt(request.Index);
            }
            else
            {
                response = "Unknown Block Spawner operation.";
                return false;
            }

            WriteReclamationSpawnerConfig(block, config);
            RefreshReclamationSpawnerVisuals(block);
            return true;
        }

        private void StopAutomaticReclamationSpawning(long blockEntityId)
        {
            runningReclamationSpawners.Remove(blockEntityId);
            pendingReclamationSpawns.Remove(blockEntityId);
            EndReclamationBurstSmoke(blockEntityId, true);
        }

        private void BroadcastReclamationSmokeState(long blockEntityId, bool active, float seconds)
        {
            if (MyAPIGateway.Multiplayer == null || !MyAPIGateway.Multiplayer.IsServer)
                return;

            var message = new ReclamationSpawnerNetworkMessage
            {
                Kind = ReclamationSmokeKind,
                BlockEntityId = blockEntityId,
                Success = active,
                Number = seconds,
            };

            MyAPIGateway.Multiplayer.SendMessageToOthers(
                ReclamationSpawnerNetworkMessageId,
                SerializeReclamationMessage(message));
        }

        private static void Swap(List<string> entries, int left, int right)
        {
            var value = entries[left];
            entries[left] = entries[right];
            entries[right] = value;
        }

        private static long ResolveReclamationIdentity(ulong sender)
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i] != null && players[i].SteamUserId == sender)
                    return players[i].IdentityId;
            }

            return 0;
        }

        private static byte[] SerializeReclamationMessage(ReclamationSpawnerNetworkMessage message)
        {
            if (message == null || MyAPIGateway.Utilities == null)
                return new byte[0];

            return Encoding.UTF8.GetBytes(MyAPIGateway.Utilities.SerializeToXML(message));
        }

        private static bool TryDeserializeReclamationMessage(
            byte[] messageBytes,
            out ReclamationSpawnerNetworkMessage message)
        {
            message = null;
            if (messageBytes == null || messageBytes.Length == 0 || MyAPIGateway.Utilities == null)
                return false;

            try
            {
                message = MyAPIGateway.Utilities.SerializeFromXML<ReclamationSpawnerNetworkMessage>(
                    Encoding.UTF8.GetString(messageBytes));
                return message != null;
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole("[Worldwright] failed to read spawner network message: " + exception.Message);
                return false;
            }
        }

    }
}
