using System;
using System.Collections.Generic;

namespace WkKn
{
    internal sealed class ProficiencyNotificationService
    {
        private readonly Dictionary<string, ProficiencyNotificationAccumulator> pendingNotifications = new Dictionary<string, ProficiencyNotificationAccumulator>(StringComparer.OrdinalIgnoreCase);

        internal void Queue(long identityId, string proficiencyId, string displayName, ProficiencyProgressResult progress, long currentTick, bool comboEligible)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(proficiencyId) || progress.AddedProgress <= 0.0)
                return;

            var key = GetKey(identityId, proficiencyId);
            ProficiencyNotificationAccumulator notification;
            if (pendingNotifications.TryGetValue(key, out notification))
            {
                notification.AddedProgress += progress.AddedProgress;
                notification.TotalProgress = progress.TotalProgress;
                notification.LastTick = currentTick;
                notification.ComboEligible = notification.ComboEligible && comboEligible;
                pendingNotifications[key] = notification;
                return;
            }

            pendingNotifications.Add(key, new ProficiencyNotificationAccumulator
            {
                IdentityId = identityId,
                ProficiencyId = proficiencyId,
                DisplayName = displayName,
                AddedProgress = progress.AddedProgress,
                TotalProgress = progress.TotalProgress,
                ComboEligible = comboEligible,
                LastTick = currentTick,
            });
        }

        internal List<ProficiencyNotificationMessage> CollectReady(long currentTick, long delayTicks)
        {
            var readyMessages = new List<ProficiencyNotificationMessage>();
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
                ProficiencyNotificationAccumulator notification;
                if (!pendingNotifications.TryGetValue(key, out notification))
                    continue;

                pendingNotifications.Remove(key);
                readyMessages.Add(new ProficiencyNotificationMessage
                {
                    IdentityId = notification.IdentityId,
                    ProficiencyId = notification.ProficiencyId,
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

        private static string GetKey(long identityId, string proficiencyId)
        {
            return identityId + ":" + proficiencyId;
        }
    }
}
