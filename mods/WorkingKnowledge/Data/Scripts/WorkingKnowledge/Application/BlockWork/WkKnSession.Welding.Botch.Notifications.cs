using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void ShowWeldBotchWarning(long identityId, string researchId, VRage.Game.ModAPI.IMySlimBlock slimBlock, WeldBotchResult result)
        {
            if (identityId == 0)
                return;

            Vector3D soundPosition;
            slimBlock.ComputeWorldCenter(out soundPosition);
            if (config.WeldBotchSoundEnabled)
                QueueLocalSoundAtPositionToNearbyPlayers(soundPosition, config.WeldBotchSoundSubtype, config.WeldBotchSoundRange);

            var key = GetWeldBotchWarningKey(identityId, researchId, slimBlock);
            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(identityId)) * 60.0);
            long lastShownTick;
            if (!string.IsNullOrWhiteSpace(key) &&
                warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            QueueWeldBotchWarning(identityId, researchId, slimBlock, result);
        }

        private void QueueWeldBotchWarning(long identityId, string researchId, IMySlimBlock slimBlock, WeldBotchResult result)
        {
            if (identityId == 0 || slimBlock == null || slimBlock.ComponentStack == null)
                return;

            var key = GetWeldBotchWarningKey(identityId, researchId, slimBlock);
            if (string.IsNullOrWhiteSpace(key))
                return;

            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(identityId)) * 60.0);
            long lastShownTick;
            if (warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            WeldBotchNotificationAccumulator accumulator;
            if (!blockWorkState.PendingWeldBotchWarningsByKey.TryGetValue(key, out accumulator))
            {
                accumulator = new WeldBotchNotificationAccumulator
                {
                    IdentityId = identityId,
                    ResearchId = researchId,
                    BlockDisplayName = GetWeldBotchBlockDisplayName(slimBlock),
                    MaxIntegrity = Math.Max(0f, slimBlock.ComponentStack.MaxIntegrity),
                };
                blockWorkState.PendingWeldBotchWarningsByKey[key] = accumulator;
            }

            accumulator.DamageAmount += Math.Max(0f, result.DamageAmount);
            if (accumulator.MaxIntegrity <= WeldCapTolerance)
                accumulator.MaxIntegrity = Math.Max(0f, slimBlock.ComponentStack.MaxIntegrity);

            AddFormattedComponentSummary(accumulator.LostComponentsByName, result.LostComponentsText);
            AddFormattedComponentSummary(accumulator.RecoveredComponentsByName, result.RecoveredComponentsText);
            accumulator.LastTick = simulationTick;
        }

        private void FlushReadyWeldBotchWarnings()
        {
            if (blockWorkState.PendingWeldBotchWarningsByKey.Count == 0)
                return;

            var delayTicks = GetNotificationDelayTicks();
            var readyKeys = new List<string>();
            foreach (var entry in blockWorkState.PendingWeldBotchWarningsByKey)
            {
                if (simulationTick - entry.Value.LastTick >= delayTicks)
                    readyKeys.Add(entry.Key);
            }

            for (var i = 0; i < readyKeys.Count; i++)
            {
                var key = readyKeys[i];
                WeldBotchNotificationAccumulator accumulator;
                if (!blockWorkState.PendingWeldBotchWarningsByKey.TryGetValue(key, out accumulator))
                    continue;

                blockWorkState.PendingWeldBotchWarningsByKey.Remove(key);
                ShowReadyWeldBotchWarning(key, accumulator);
            }
        }

        private void ShowReadyWeldBotchWarning(string key, WeldBotchNotificationAccumulator accumulator)
        {
            if (accumulator == null || accumulator.IdentityId == 0)
                return;

            var warningCooldownTicks = (long)Math.Round(Math.Max(0.0, GetEffectiveWeldBotchWarningCooldownSeconds(accumulator.IdentityId)) * 60.0);
            long lastShownTick;
            if (!string.IsNullOrWhiteSpace(key) &&
                warningCooldownTicks > 0 &&
                weldBotchWarningLastShownByKey.TryGetValue(key, out lastShownTick) &&
                simulationTick - lastShownTick < warningCooldownTicks)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(key))
                weldBotchWarningLastShownByKey[key] = simulationTick;

            var blockDisplayName = string.IsNullOrWhiteSpace(accumulator.BlockDisplayName) ? "Block" : accumulator.BlockDisplayName;
            var maxIntegrity = accumulator.MaxIntegrity <= WeldCapTolerance ? 1f : accumulator.MaxIntegrity;
            var knockedBack = FormatProgressDelta(accumulator.DamageAmount / maxIntegrity);
            var chatMessage = "Work lost: -" + knockedBack;

            var lostText = FormatComponentSummary(accumulator.LostComponentsByName);
            if (!string.IsNullOrWhiteSpace(lostText))
                chatMessage += "\nLost: " + lostText;

            var recoveredText = FormatComponentSummary(accumulator.RecoveredComponentsByName);
            if (!string.IsNullOrWhiteSpace(recoveredText))
                chatMessage += "\nRecovered: " + recoveredText;

            ShowWkTargetColoredChatLine(
                accumulator.IdentityId,
                blockDisplayName + " Build Failure",
                chatMessage,
                WkChatWarningColor,
                false);
            SendLocalFeedback(
                accumulator.IdentityId,
                blockDisplayName + " Build Failed -" + knockedBack,
                WkChatWarningFont,
                IsPlayerProgressToastEnabled(accumulator.IdentityId),
                null,
                false);
        }

        private static void AddFormattedComponentSummary(Dictionary<string, int> componentsByName, string formattedText)
        {
            if (componentsByName == null || string.IsNullOrWhiteSpace(formattedText))
                return;

            var parts = formattedText.Split(',');
            for (var i = 0; i < parts.Length; i++)
            {
                var part = parts[i] == null ? string.Empty : parts[i].Trim();
                if (part.Length == 0)
                    continue;

                var marker = part.LastIndexOf(" x", StringComparison.OrdinalIgnoreCase);
                if (marker <= 0 || marker + 2 >= part.Length)
                    continue;

                var name = part.Substring(0, marker).Trim();
                var countText = part.Substring(marker + 2).Trim();
                int count;
                if (string.IsNullOrWhiteSpace(name) || !int.TryParse(countText, NumberStyles.Integer, CultureInfo.InvariantCulture, out count) || count <= 0)
                    continue;

                int existing;
                if (componentsByName.TryGetValue(name, out existing))
                    componentsByName[name] = existing + count;
                else
                    componentsByName.Add(name, count);
            }
        }

        private static string FormatComponentSummary(Dictionary<string, int> componentsByName)
        {
            if (componentsByName == null || componentsByName.Count == 0)
                return null;

            var builder = new StringBuilder();
            foreach (var entry in componentsByName)
            {
                if (entry.Value <= 0)
                    continue;

                if (builder.Length > 0)
                    builder.Append(", ");

                builder.Append(entry.Key);
                builder.Append(" x");
                builder.Append(entry.Value.ToString(CultureInfo.InvariantCulture));
            }

            return builder.Length == 0 ? null : builder.ToString();
        }

    }
}
