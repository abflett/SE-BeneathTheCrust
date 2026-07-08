using System.Globalization;
using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const string WkFallbackCompletionSoundSubtype = "ArcHudGPSNotification2";
        private const string WkResearchCompletionSoundSubtype = "WkKnResearchComplete";
        private const string WkProficiencyCompletionSoundSubtype = "WkKnProficiencyMastered";

        private string GetPlayerConfigId(long identityId)
        {
            return identityId.ToString(CultureInfo.InvariantCulture);
        }

        private WkPlayerConfigRecord GetPlayerConfig(long identityId)
        {
            return playerConfigStore.GetPlayerOrDefault(GetPlayerConfigId(identityId));
        }

        private WkProgressHudSettings GetLocalProgressHudSettings()
        {
            if (MyAPIGateway.Session == null ||
                MyAPIGateway.Session.Player == null ||
                MyAPIGateway.Session.Player.IdentityId == 0)
                return WkProgressHudSettings.Default;

            return GetProgressHudSettings(MyAPIGateway.Session.Player.IdentityId);
        }

        private WkProgressHudSettings GetProgressHudSettings(long identityId)
        {
            var player = GetPlayerConfig(identityId);
            return new WkProgressHudSettings
            {
                Enabled = player.ProgressHudEnabled,
                RowCount = player.ProgressHudRows,
                Order = player.ProgressHudOrder,
                Position = player.ProgressHudPosition,
                OffsetX = player.ProgressHudOffsetX,
                OffsetY = player.ProgressHudOffsetY,
                FadeSeconds = player.ProgressHudFadeSeconds,
            };
        }

        private bool IsPlayerProgressChatEnabled(long identityId)
        {
            return config != null && config.ProgressChatEnabled && GetPlayerConfig(identityId).ProgressChatEnabled;
        }

        private bool IsPlayerProgressToastEnabled(long identityId)
        {
            return config != null && config.ProgressToastEnabled && GetPlayerConfig(identityId).ProgressToastEnabled;
        }

        private double GetEffectiveResearchChatSuppressionPercent(long identityId)
        {
            return Max(config == null ? 0.0 : config.ResearchChatSuppressionPercent, GetPlayerConfig(identityId).ResearchChatSuppressionPercent);
        }

        private double GetEffectiveProficiencyChatSuppressionPercent(long identityId)
        {
            return Max(config == null ? 0.0 : config.ProficiencyChatSuppressionPercent, GetPlayerConfig(identityId).ProficiencyChatSuppressionPercent);
        }

        private double GetEffectiveResearchToastSuppressionPercent(long identityId)
        {
            return Max(config == null ? 0.0 : config.ResearchToastSuppressionPercent, GetPlayerConfig(identityId).ResearchToastSuppressionPercent);
        }

        private double GetEffectiveProficiencyToastSuppressionPercent(long identityId)
        {
            return Max(config == null ? 0.0 : config.ProficiencyToastSuppressionPercent, GetPlayerConfig(identityId).ProficiencyToastSuppressionPercent);
        }

        private bool IsPlayerCompletionSoundEnabled(long identityId)
        {
            return config != null && config.CompletionSoundEnabled && GetPlayerConfig(identityId).CompletionSoundEnabled;
        }

        private string GetEffectiveCompletionSoundSubtype(long identityId)
        {
            return WkFallbackCompletionSoundSubtype;
        }

        private string GetEffectiveResearchCompletionSoundSubtype(long identityId)
        {
            return WkResearchCompletionSoundSubtype;
        }

        private string GetEffectiveProficiencyCompletionSoundSubtype(long identityId)
        {
            return WkProficiencyCompletionSoundSubtype;
        }

        private bool IsPlayerWeldBotchSoundEnabled(long identityId)
        {
            return config != null && config.WeldBotchSoundEnabled && GetPlayerConfig(identityId).WeldBotchSoundEnabled;
        }

        private double GetEffectiveWeldBotchWarningCooldownSeconds(long identityId)
        {
            var playerCooldown = GetPlayerConfig(identityId).WeldBotchWarningCooldownSeconds;
            if (playerCooldown < 0.0)
                return config == null ? 0.0 : config.WeldBotchWarningCooldownSeconds;

            return Max(config == null ? 0.0 : config.WeldBotchWarningCooldownSeconds, playerCooldown);
        }

        private static double Max(double left, double right)
        {
            return left > right ? left : right;
        }
    }
}
