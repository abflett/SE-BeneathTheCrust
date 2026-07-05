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
        private void InitializeProficiencyModule()
        {
            activeSession = this;
            SetProficiencyDisplaySnapshot(ProficiencyDisplaySnapshot.CreateMessage("Working Knowledge is loading."));
            RegisterProficiencyNetworkHandler();
        }

        private void UpdateProficiencyModule()
        {
            RegisterProficiencyNetworkHandler();
            ClearStaleProficiencyGrindOperations();
            RefreshLocalProficiencyDisplayIfReady();
            RequestProficiencyDisplaySyncIfReady();
        }

        private void ClearProficiencyRuntimeState()
        {
            UnregisterProficiencyNetworkHandler();
            proficiencyNotificationService.ClearAll();
            ClearWeldRuntimeState();
            syncedLocalProficiencyScope = null;
            if (activeSession == this)
                activeSession = null;

            SetProficiencyDisplaySnapshot(ProficiencyDisplaySnapshot.CreateMessage("Working Knowledge is offline."));
        }

    }
}
