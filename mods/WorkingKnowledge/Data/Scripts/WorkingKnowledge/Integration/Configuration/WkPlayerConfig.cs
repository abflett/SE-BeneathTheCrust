using System.Collections.Generic;

namespace WkKn
{
    // World-storage DTO for per-player feedback preferences. Balance stays in WkKnConfig.xml.
    public class WkPlayerConfigSaveData
    {
        public List<WkPlayerConfigRecord> Players = new List<WkPlayerConfigRecord>();
    }

    public class WkPlayerConfigRecord
    {
        public string Id = "";
        public bool ProgressChatEnabled = false;
        public bool ProgressToastEnabled = false;
        public double ResearchChatSuppressionPercent = 0.0;
        public double ProficiencyChatSuppressionPercent = 0.0;
        public double ResearchToastSuppressionPercent = 0.0;
        public double ProficiencyToastSuppressionPercent = 0.0;
        public bool CompletionSoundEnabled = true;
        public bool WeldBotchSoundEnabled = true;
        public double WeldBotchWarningCooldownSeconds = -1.0;
    }
}
