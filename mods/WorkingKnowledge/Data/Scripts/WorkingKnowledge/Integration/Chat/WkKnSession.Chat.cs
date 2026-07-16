using System.Collections.Generic;
using System.Text;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRageMath;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const string WkChatWarningSender = "Warning";
        private const string WkChatResearchFont = "WkChat04";
        private const string WkChatProficiencyFont = "WkChat02";
        private const string WkChatCompletionFont = "WkChat06";
        private const string WkChatWarningFont = "WkChat07";
        private const int WkProgressToastDurationMs = 3500;
        private static readonly Color WkChatInfoColor = new Color(255, 214, 74);
        private static readonly Color WkChatResearchColor = new Color(140, 220, 95);
        private static readonly Color WkChatProficiencyColor = new Color(95, 205, 230);
        private static readonly Color WkChatProgressHeaderColor = new Color(80, 215, 155);
        private static readonly Color WkChatCautionColor = new Color(255, 170, 70);
        private static readonly Color WkChatWarningColor = new Color(235, 92, 80);
        private static readonly string[] ResearchCompletionMessages =
        {
            "Your smarts added up!",
            "That brain work paid off!",
            "Pattern solved. Nice thinking!",
            "Another mystery, neatly untangled.",
            "Clever work, engineer."
        };
        private static readonly string[] ProficiencyCompletionMessages =
        {
            "All the hard work paid off!",
            "Practice made permanent!",
            "Field work forged real skill!",
            "Your hands know this one now!",
            "Repetition became mastery!"
        };

        private void ShowWkChatSection(string heading, params string[] lines)
        {
            ShowWkChatSection(heading, (IEnumerable<string>)lines);
        }

        private void ShowWkChatSection(string heading, IEnumerable<string> lines)
        {
            ShowWkColoredChatSection(heading, lines, WkChatInfoColor);
        }

        private void ShowWkColoredChatSection(string heading, IEnumerable<string> lines, Color color)
        {
            if (string.IsNullOrWhiteSpace(heading) || lines == null)
                return;

            var builder = new StringBuilder();
            foreach (var rawLine in lines)
            {
                var line = rawLine ?? string.Empty;
                if (builder.Length > 0)
                    builder.Append('\n');

                builder.Append(line);
            }

            ShowWkColoredChatMessage(heading, builder.Length == 0 ? " " : builder.ToString(), color);
        }

        private void ShowWkWarningMessage(string message)
        {
            ShowWkChatMessage(WkChatWarningSender, message, WkChatWarningFont);
        }

        private void ShowWkProgressHeaderMessage(long identityId, string displayName)
        {
            ShowWkTargetColoredChatLine(identityId, displayName, " ", WkChatProgressHeaderColor, true);
        }

        private void ShowWkResearchProgressMessage(long identityId, string message)
        {
            ShowWkTargetColoredChatLine(identityId, "Research", message, WkChatResearchColor, false);
        }

        private void ShowWkProficiencyProgressMessage(long identityId, string message)
        {
            ShowWkTargetColoredChatLine(identityId, "Proficiency", message, WkChatProficiencyColor, false);
        }

        private void ShowWkResearchProgressToast(long identityId, string displayName, string message)
        {
            ShowWkProgressToast(identityId, displayName + " Research " + message, WkChatResearchFont);
        }

        private void ShowWkProficiencyProgressToast(long identityId, string displayName, string message)
        {
            ShowWkProgressToast(identityId, displayName + " Proficiency " + message, WkChatProficiencyFont);
        }

        private void ShowWkResearchCompletionFeedback(long identityId, string displayName)
        {
            ShowWkCompletionFeedback(identityId, displayName, "Research Complete", PickRandomMessage(ResearchCompletionMessages), GetEffectiveResearchCompletionSoundSubtype(identityId));
        }

        private void ShowWkProficiencyCompletionFeedback(long identityId, string displayName)
        {
            ShowWkCompletionFeedback(identityId, displayName, "Proficiency Mastered", PickRandomMessage(ProficiencyCompletionMessages), GetEffectiveProficiencyCompletionSoundSubtype(identityId));
        }

        private void ShowWkCompletionFeedback(long identityId, string displayName, string label, string message, string soundSubtype)
        {
            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(message))
                return;

            ShowWkCompletionMessage(identityId, displayName, label, message);
            SendCompletionFeedback(identityId, displayName, label, message, soundSubtype);
        }

        private void ShowWkCompletionMessage(long identityId, string displayName, string label, string message)
        {
            if (string.IsNullOrWhiteSpace(displayName) || string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(message))
                return;

            ShowWkTargetChatLine(identityId, displayName + "\n" + label, message, WkChatCompletionFont);
        }

        private void ShowWkChatMessage(string sender, string message, string font)
        {
            ShowWkTargetChatMessage(GetLocalIdentityId(), sender, message, font);
        }

        private void ShowWkColoredChatMessage(string sender, string message, Color color)
        {
            ShowWkTargetColoredChatMessage(GetLocalIdentityId(), sender, message, color);
        }

        private void ShowWkTargetChatMessage(long identityId, string sender, string message, string font)
        {
            if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(message))
                return;

            var formattedMessage = FormatWkChatMessage(message);

            if (identityId != 0)
            {
                MyVisualScriptLogicProvider.SendChatMessage(formattedMessage, sender, identityId, font);
                return;
            }

            if (MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage(sender, formattedMessage);
        }

        private void ShowWkTargetColoredChatMessage(long identityId, string sender, string message, Color color)
        {
            if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(message))
                return;

            var formattedMessage = FormatWkChatMessage(message);

            if (identityId != 0)
            {
                MyVisualScriptLogicProvider.SendChatMessageColored(formattedMessage, color, sender, identityId);
                return;
            }

            if (MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage(sender, formattedMessage);
        }

        private void ShowWkTargetColoredChatLine(long identityId, string sender, string message, Color color, bool allowWhitespaceMessage)
        {
            if (string.IsNullOrWhiteSpace(sender) || message == null || (!allowWhitespaceMessage && string.IsNullOrWhiteSpace(message)))
                return;

            if (identityId != 0)
            {
                MyVisualScriptLogicProvider.SendChatMessageColored(message, color, sender, identityId);
                return;
            }

            if (MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage(sender, message);
        }

        private void ShowWkTargetChatLine(long identityId, string sender, string message, string font)
        {
            if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(message))
                return;

            if (identityId != 0)
            {
                MyVisualScriptLogicProvider.SendChatMessage(message, sender, identityId, font);
                return;
            }

            if (MyAPIGateway.Utilities != null)
                MyAPIGateway.Utilities.ShowMessage(sender, message);
        }

        private void ShowWkProgressToast(long identityId, string message, string font)
        {
            if (!IsLocalIdentity(identityId) || string.IsNullOrWhiteSpace(message))
                return;

            ShowWkToast(message, font);
        }

        private void ShowWkToast(string message, string font)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            MyVisualScriptLogicProvider.ShowNotification(message, WkProgressToastDurationMs, string.IsNullOrWhiteSpace(font) ? "White" : font);
        }

        private static string FormatWkChatMessage(string message)
        {
            return message.Length > 0 && message[0] == '\n'
                ? message
                : "\n" + message;
        }

        private string PickRandomMessage(string[] messages)
        {
            if (messages == null || messages.Length == 0)
                return string.Empty;

            return messages[random.Next(messages.Length)];
        }

        private long GetLocalIdentityId()
        {
            return MyAPIGateway.Session != null && MyAPIGateway.Session.Player != null
                ? MyAPIGateway.Session.Player.IdentityId
                : 0;
        }
    }
}
