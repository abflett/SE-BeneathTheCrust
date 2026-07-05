using System;
using System.Collections.Generic;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void FlushReadyProgressNotifications()
        {
            var delayTicks = GetNotificationDelayTicks();
            var readyResearch = researchNotificationService.CollectReady(simulationTick, delayTicks);
            var readyProficiency = proficiencyNotificationService.CollectReady(simulationTick, delayTicks);
            if (readyResearch.Count == 0 && readyProficiency.Count == 0)
                return;

            var combinableProficiency = new Dictionary<string, ProficiencyNotificationMessage>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < readyProficiency.Count; i++)
            {
                var notification = readyProficiency[i];
                if (!notification.ComboEligible)
                    continue;

                var key = GetProgressNotificationKey(notification.IdentityId, notification.ProficiencyId);
                if (!combinableProficiency.ContainsKey(key))
                    combinableProficiency.Add(key, notification);
            }

            var combinedProficiencyKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < readyResearch.Count; i++)
            {
                var research = readyResearch[i];
                var key = GetProgressNotificationKey(research.IdentityId, research.ResearchId);

                ProficiencyNotificationMessage proficiency;
                if (research.ComboEligible && combinableProficiency.TryGetValue(key, out proficiency))
                {
                    ShowCombinedProgressNotification(research, proficiency);
                    combinedProficiencyKeys.Add(key);
                    continue;
                }

                ShowResearchNotification(research);
            }

            for (var i = 0; i < readyProficiency.Count; i++)
            {
                var proficiency = readyProficiency[i];
                var key = GetProgressNotificationKey(proficiency.IdentityId, proficiency.ProficiencyId);
                if (combinedProficiencyKeys.Contains(key))
                    continue;

                ShowProficiencyNotification(proficiency);
            }
        }

        private void ShowCombinedProgressNotification(ResearchNotificationMessage research, ProficiencyNotificationMessage proficiency)
        {
            var displayName = !string.IsNullOrWhiteSpace(research.DisplayName) ? research.DisplayName : proficiency.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = "Progress";

            var researchKey = GetTypedProgressNotificationKey("research", research.IdentityId, research.ResearchId);
            var proficiencyKey = GetTypedProgressNotificationKey("proficiency", proficiency.IdentityId, proficiency.ProficiencyId);
            double researchChatDelta;
            double proficiencyChatDelta;
            double researchToastDelta;
            double proficiencyToastDelta;
            var showResearchChat = TryGetResearchProgressChatDelta(research.IdentityId, researchKey, research.TotalProgress, out researchChatDelta);
            var showProficiencyChat = TryGetProficiencyProgressChatDelta(proficiency.IdentityId, proficiencyKey, proficiency.TotalProgress, out proficiencyChatDelta);
            var showResearchToast = TryGetResearchProgressToastDelta(research.IdentityId, researchKey, research.TotalProgress, out researchToastDelta);
            var showProficiencyToast = TryGetProficiencyProgressToastDelta(proficiency.IdentityId, proficiencyKey, proficiency.TotalProgress, out proficiencyToastDelta);

            if (showResearchChat || showProficiencyChat)
                ShowProgressHeaderIfChanged(research.IdentityId, research.ResearchId, displayName);

            if (showResearchChat)
                ShowWkResearchProgressMessage(research.IdentityId, FormatProgressNotificationValue(researchChatDelta, research.TotalProgress));

            if (showProficiencyChat)
                ShowWkProficiencyProgressMessage(proficiency.IdentityId, FormatProgressNotificationValue(proficiencyChatDelta, proficiency.TotalProgress));

            if (showResearchToast)
                ShowWkResearchProgressToast(research.IdentityId, displayName, FormatProgressNotificationValue(researchToastDelta, research.TotalProgress));

            if (showProficiencyToast)
                ShowWkProficiencyProgressToast(proficiency.IdentityId, displayName, FormatProgressNotificationValue(proficiencyToastDelta, proficiency.TotalProgress));
        }

        private void ShowResearchNotification(ResearchNotificationMessage notification)
        {
            var key = GetTypedProgressNotificationKey("research", notification.IdentityId, notification.ResearchId);
            double chatDelta;
            if (TryGetResearchProgressChatDelta(notification.IdentityId, key, notification.TotalProgress, out chatDelta))
            {
                ShowProgressHeaderIfChanged(notification.IdentityId, notification.ResearchId, notification.DisplayName);
                ShowWkResearchProgressMessage(notification.IdentityId, FormatProgressNotificationValue(chatDelta, notification.TotalProgress));
            }

            double toastDelta;
            if (TryGetResearchProgressToastDelta(notification.IdentityId, key, notification.TotalProgress, out toastDelta))
                ShowWkResearchProgressToast(notification.IdentityId, notification.DisplayName, FormatProgressNotificationValue(toastDelta, notification.TotalProgress));
        }

        private void ShowProficiencyNotification(ProficiencyNotificationMessage notification)
        {
            var key = GetTypedProgressNotificationKey("proficiency", notification.IdentityId, notification.ProficiencyId);
            double chatDelta;
            if (TryGetProficiencyProgressChatDelta(notification.IdentityId, key, notification.TotalProgress, out chatDelta))
            {
                ShowProgressHeaderIfChanged(notification.IdentityId, notification.ProficiencyId, notification.DisplayName);
                ShowWkProficiencyProgressMessage(notification.IdentityId, FormatProgressNotificationValue(chatDelta, notification.TotalProgress));
            }

            double toastDelta;
            if (TryGetProficiencyProgressToastDelta(notification.IdentityId, key, notification.TotalProgress, out toastDelta))
                ShowWkProficiencyProgressToast(notification.IdentityId, notification.DisplayName, FormatProgressNotificationValue(toastDelta, notification.TotalProgress));
        }

        private static string FormatProgressNotificationValue(double addedProgress, double totalProgress)
        {
            return "+" + FormatProgressDelta(addedProgress) + " (" + FormatProgress(totalProgress) + ")";
        }

        private static string GetProgressNotificationKey(long identityId, string progressId)
        {
            return identityId + ":" + progressId;
        }

        private static string GetTypedProgressNotificationKey(string type, long identityId, string progressId)
        {
            return type + ":" + identityId + ":" + progressId;
        }

        private bool TryGetResearchProgressChatDelta(long identityId, string key, double totalProgress, out double displayDelta)
        {
            displayDelta = 0.0;
            return IsPlayerProgressChatEnabled(identityId) &&
                   TryGetProgressNotificationDelta(progressChatLastShownByKey, key, totalProgress, GetSuppressionThreshold(GetEffectiveResearchChatSuppressionPercent(identityId)), out displayDelta);
        }

        private bool TryGetProficiencyProgressChatDelta(long identityId, string key, double totalProgress, out double displayDelta)
        {
            displayDelta = 0.0;
            return IsPlayerProgressChatEnabled(identityId) &&
                   TryGetProgressNotificationDelta(progressChatLastShownByKey, key, totalProgress, GetSuppressionThreshold(GetEffectiveProficiencyChatSuppressionPercent(identityId)), out displayDelta);
        }

        private bool TryGetResearchProgressToastDelta(long identityId, string key, double totalProgress, out double displayDelta)
        {
            displayDelta = 0.0;
            return IsPlayerProgressToastEnabled(identityId) &&
                   TryGetProgressNotificationDelta(progressToastLastShownByKey, key, totalProgress, GetSuppressionThreshold(GetEffectiveResearchToastSuppressionPercent(identityId)), out displayDelta);
        }

        private bool TryGetProficiencyProgressToastDelta(long identityId, string key, double totalProgress, out double displayDelta)
        {
            displayDelta = 0.0;
            return IsPlayerProgressToastEnabled(identityId) &&
                   TryGetProgressNotificationDelta(progressToastLastShownByKey, key, totalProgress, GetSuppressionThreshold(GetEffectiveProficiencyToastSuppressionPercent(identityId)), out displayDelta);
        }

        private static bool TryGetProgressNotificationDelta(Dictionary<string, double> lastShownByKey, string key, double totalProgress, double threshold, out double displayDelta)
        {
            displayDelta = 0.0;
            if (lastShownByKey == null || string.IsNullOrWhiteSpace(key))
                return false;

            var normalizedProgress = Clamp01(totalProgress);
            double lastShownProgress;
            if (!lastShownByKey.TryGetValue(key, out lastShownProgress))
                lastShownProgress = 0.0;

            displayDelta = normalizedProgress - lastShownProgress;
            if (displayDelta <= threshold)
            {
                displayDelta = 0.0;
                return false;
            }

            lastShownByKey[key] = normalizedProgress;
            return true;
        }

        private static double GetSuppressionThreshold(double suppressionPercent)
        {
            return RatioMath.Clamp(suppressionPercent, 0.0, 100.0) / 100.0;
        }

        private void ShowProgressHeaderIfChanged(long identityId, string progressId, string displayName)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(displayName))
                return;

            var headerKey = string.IsNullOrWhiteSpace(progressId) ? displayName : progressId;
            string previousHeaderKey;
            if (progressChatHeaderByIdentity.TryGetValue(identityId, out previousHeaderKey) &&
                string.Equals(previousHeaderKey, headerKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            progressChatHeaderByIdentity[identityId] = headerKey;
            ShowWkProgressHeaderMessage(identityId, displayName);
        }

        private static bool IsProgressComboEligibleSource(string source)
        {
            return source != null && source.Equals("grinding", StringComparison.OrdinalIgnoreCase);
        }
    }
}
