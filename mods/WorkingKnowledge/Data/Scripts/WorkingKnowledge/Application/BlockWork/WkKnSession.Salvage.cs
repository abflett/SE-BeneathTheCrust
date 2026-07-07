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
        private void ClearStaleSalvageOperations()
        {
            for (var i = blockWorkState.PendingSalvageOperations.Count - 1; i >= 0; i--)
            {
                if (simulationTick - blockWorkState.PendingSalvageOperations[i].Tick > 1)
                    blockWorkState.PendingSalvageOperations.RemoveAt(i);
            }
        }

        private void ApplySalvageOperation(SalvageOperation operation)
        {
            if (scrapDefinition == null)
                scrapDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(ScrapOreId);

            SalvageRecoveryResult result;
            salvageRecoveryPolicy.Apply(operation, scrapDefinition, config.SalvageScrapYield, random, out result);
        }

        private int GetProficiencyBasedSalvageIntactPercent(long identityId, string researchId)
        {
            return salvageRecoveryPolicy.GetIntactPercent(GetPlayerProficiency(identityId, researchId), config.SalvageScale);
        }

    }
}
