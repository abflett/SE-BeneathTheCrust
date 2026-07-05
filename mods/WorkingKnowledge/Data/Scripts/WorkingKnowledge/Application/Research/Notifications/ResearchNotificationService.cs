using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ResearchNotificationService
    {
        private readonly Dictionary<string, ResearchNotificationAccumulator> pendingNotifications = new Dictionary<string, ResearchNotificationAccumulator>(System.StringComparer.OrdinalIgnoreCase);

        internal void Queue(long identityId, ResearchUnlockTarget target, ResearchProgressResult progress, long currentTick, bool comboEligible)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(target.ResearchId) || progress.AddedProgress <= 0.0)
                return;

            var key = GetKey(identityId, target.ResearchId);
            ResearchNotificationAccumulator notification;
            if (pendingNotifications.TryGetValue(key, out notification))
            {
                notification.AddedProgress += progress.AddedProgress;
                notification.TotalProgress = progress.TotalProgress;
                notification.LastTick = currentTick;
                notification.ComboEligible = notification.ComboEligible && comboEligible;
                pendingNotifications[key] = notification;
                return;
            }

            pendingNotifications.Add(key, new ResearchNotificationAccumulator
            {
                IdentityId = identityId,
                ResearchId = target.ResearchId,
                DisplayName = target.DisplayName,
                AddedProgress = progress.AddedProgress,
                TotalProgress = progress.TotalProgress,
                ComboEligible = comboEligible,
                LastTick = currentTick,
            });
        }

        internal List<ResearchNotificationMessage> CollectReady(long currentTick, long delayTicks)
        {
            var readyMessages = new List<ResearchNotificationMessage>();
            if (pendingNotifications.Count == 0)
                return readyMessages;

            var readyKeys = new List<string>();
            foreach (var entry in pendingNotifications)
            {
                if (currentTick - entry.Value.LastTick >= delayTicks)
                    readyKeys.Add(entry.Key);
            }

            foreach (var key in readyKeys)
            {
                ResearchNotificationAccumulator notification;
                if (!pendingNotifications.TryGetValue(key, out notification))
                    continue;

                pendingNotifications.Remove(key);
                readyMessages.Add(new ResearchNotificationMessage
                {
                    IdentityId = notification.IdentityId,
                    ResearchId = notification.ResearchId,
                    DisplayName = notification.DisplayName,
                    AddedProgress = notification.AddedProgress,
                    TotalProgress = notification.TotalProgress,
                    ComboEligible = notification.ComboEligible,
                });
            }

            return readyMessages;
        }

        internal void ClearAll()
        {
            pendingNotifications.Clear();
        }

        private static string GetKey(long identityId, string researchId)
        {
            return identityId + ":" + researchId;
        }
    }
}
