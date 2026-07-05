using System.Collections.Generic;
using Sandbox.Game;
using VRage.Game.ModAPI;

namespace WkKn
{
    internal sealed class BlockWorkRuntimeState
    {
        internal readonly List<ResearchDamageOperation> PendingResearchOperations = new List<ResearchDamageOperation>();
        internal readonly List<ProficiencyDamageOperation> PendingProficiencyGrindOperations = new List<ProficiencyDamageOperation>();
        internal readonly List<SalvageOperation> PendingSalvageOperations = new List<SalvageOperation>();
        internal readonly List<WeldBotchSoundEvent> PendingWeldBotchSounds = new List<WeldBotchSoundEvent>();

        internal readonly Dictionary<string, float> WeldBotchRawDamageDebtByKey = new Dictionary<string, float>();
        internal readonly Dictionary<string, WeldBotchNotificationAccumulator> PendingWeldBotchWarningsByKey = new Dictionary<string, WeldBotchNotificationAccumulator>();

        internal void ClearPendingOperations()
        {
            PendingResearchOperations.Clear();
            PendingProficiencyGrindOperations.Clear();
            PendingSalvageOperations.Clear();
            PendingWeldBotchSounds.Clear();
            WeldBotchRawDamageDebtByKey.Clear();
            PendingWeldBotchWarningsByKey.Clear();
        }

    }
}
