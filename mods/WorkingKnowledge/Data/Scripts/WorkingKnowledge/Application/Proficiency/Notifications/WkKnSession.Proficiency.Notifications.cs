using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Weapons;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;



namespace WkKn
{
    public partial class WkKnSession
    {
        private void QueueProficiencyNotification(long identityId, string researchId, ProficiencyProgressResult progress, string source)
        {
            var displayName = GetSchematicDisplayName(researchId);
            proficiencyNotificationService.Queue(identityId, researchId, displayName, progress, simulationTick, IsProgressComboEligibleSource(source));
            if (IsPlayerProgressToastEnabled(identityId))
                progressHudOverlay.UpdateCombined(identityId, researchId, displayName, GetPlayerResearchProgress(identityId, researchId), progress.TotalProgress, simulationTick);
        }

    }
}
