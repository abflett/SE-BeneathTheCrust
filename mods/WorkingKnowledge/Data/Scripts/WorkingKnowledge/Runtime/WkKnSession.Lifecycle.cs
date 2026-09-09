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
        internal void RegisterGameEventHandlers()
        {
            RegisterSessionEvents();
        }

        internal void UnregisterGameEventHandlers()
        {
            UnregisterSessionEvents();
        }

        internal void InitializeRuntimeModules(MyObjectBuilder_SessionComponent sessionComponent)
        {
            RegisterResearchPedestalControls();
            RegisterResearchTerminalNetworkHandler();
            RegisterLocalFeedbackNetworkHandler();
            RegisterCommandRequestNetworkHandler();
            RegisterProgressHudNetworkHandler();
            InitializeResearchDisplayModule();
            RegisterBlockIntegrityMonitorHandlers();
            InitializeProficiencyModule();
            richHudIntegration.Initialize(OnRichHudReady, OnRichHudReset);
        }

        internal void LoadRuntimeData()
        {
            runtimeLoadIssue = null;
            try
            {
                ResetPersistenceStores();
                RebuildResearchDefinitions();
                LoadRuntimeDefinitions();
            }
            catch (Exception exception)
            {
                ReportRuntimeLoadFailure(exception);
            }
        }

        internal void StartRuntime()
        {
            var persistenceLoaded = LoadCheckpointOrLegacyPersistence();
            MyAPIGateway.Session.SessionSettings.EnableResearch = true;
            ApplyFundamentalsDefaultsForOnlinePlayers();
            if (persistenceLoaded)
                RefreshResearchDataFragmentsInContainerLoot();
            SyncCompletedResearchForOnlinePlayers();
        }

        internal void SaveRuntimeData()
        {
            SaveCheckpointPersistence();
        }

        internal void UpdateRuntimeBeforeSimulation()
        {
            simulationTick++;
            TryShowPersistenceStatusMessage();
            UpdateBeforeWeldSimulation();
            UpdateResearchRuntime();
            UpdateProficiencyModule();
            FlushReadyProgressNotifications();
            FlushReadyWeldBotchWarnings();
            richHudIntegration.Update(simulationTick, GetLocalProgressHudSettings(), richHudSettingsMenu != null && richHudSettingsMenu.IsOpen);
            UpdateRichHudSettings();
        }

        internal void UpdateRuntimeAfterSimulation()
        {
            UpdateAfterWeldSimulation();
        }

        internal void UnloadRuntimeModules()
        {
            UnregisterResearchPedestalControls();
            ClearRuntimeState();
        }

        private void RegisterSessionEvents()
        {
            MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, BeforeDamage);
            MyAPIGateway.Session.DamageSystem.RegisterAfterDamageHandler(0, AfterDamage);
            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, OnDestroyed);
            MyVisualScriptLogicProvider.PlayerConnected += OnPlayerConnected;
            MyAPIGateway.Utilities.MessageEnteredSender += OnMessageEntered;
            MyAPIGateway.Players.ItemConsumed += OnItemConsumed;
            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
        }

        private void UnregisterSessionEvents()
        {
            MyVisualScriptLogicProvider.PlayerConnected -= OnPlayerConnected;
            MyAPIGateway.Utilities.MessageEnteredSender -= OnMessageEntered;
            MyAPIGateway.Players.ItemConsumed -= OnItemConsumed;
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
            UnsubscribeTrackedBlockIntegrityGrids();
        }

        private void LoadRuntimeDefinitions()
        {
            scrapDefinition = MyDefinitionManager.Static.GetPhysicalItemDefinition(ScrapOreId);
            InjectResearchDataFragmentsIntoContainerLoot();
            SubscribeExistingBlockIntegrityGrids();
        }

        private void UpdateResearchRuntime()
        {
            ClearStaleResearchOperations();
            ClearStaleSalvageOperations();
            RegisterResearchTerminalNetworkHandler();
            RegisterLocalFeedbackNetworkHandler();
            RegisterCommandRequestNetworkHandler();
            RegisterProgressHudNetworkHandler();
            UpdateResearchDisplayModule();
        }

        private void ClearRuntimeState()
        {
            researchNotificationService.ClearAll();
            if (textHudSettingsLauncher != null)
                textHudSettingsLauncher.Close();
            textHudSettingsLauncher = null;
            // Release settings controls and restore the HUD before resetting framework modules.
            OnRichHudReset();
            richHudIntegration.Close();
            progressChatHeaderByIdentity.Clear();
            progressChatLastShownByKey.Clear();
            progressToastLastShownByKey.Clear();
            weldBotchWarningLastShownByKey.Clear();
            UnregisterResearchTerminalNetworkHandler();
            UnregisterLocalFeedbackNetworkHandler();
            UnregisterCommandRequestNetworkHandler();
            UnregisterProgressHudNetworkHandler();
            blockWorkState.ClearPendingOperations();
            researchPedestalViewsByBlock.Clear();
            ClearResearchDisplayRuntimeState();
            ClearProficiencyRuntimeState();
        }

    }
}
