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
            InitializeResearchDisplayModule();
            RegisterBlockIntegrityMonitorHandlers();
            InitializeProficiencyModule();
        }

        internal void LoadRuntimeData()
        {
            try
            {
                LoadConfigStore();
                LoadPlayerConfigStore();
                LoadResearchStore();
                LoadProficiencyStore();
                RebuildResearchDefinitions();
                LoadRuntimeDefinitions();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load runtime data: " + exception);
            }
        }

        internal void StartRuntime()
        {
            MyAPIGateway.Session.SessionSettings.EnableResearch = true;
            ApplyFundamentalsDefaultsForOnlinePlayers();
            SyncCompletedResearchForOnlinePlayers();
        }

        internal void SaveRuntimeData()
        {
            SaveDirtyStores();
        }

        internal void UpdateRuntimeBeforeSimulation()
        {
            simulationTick++;
            UpdateBeforeWeldSimulation();
            AutosaveDirtyStores();
            UpdateResearchRuntime();
            UpdateProficiencyModule();
            FlushReadyProgressNotifications();
            FlushReadyWeldBotchWarnings();
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

        private void AutosaveDirtyStores()
        {
            if (simulationTick % ResearchAutosaveTicks != 0)
                return;

            SaveDirtyStores();
        }

        private void SaveDirtyStores()
        {
            SavePlayerConfigStore();
            SaveResearchStore();
            SaveProficiencyStore();
        }

        private void UpdateResearchRuntime()
        {
            ClearStaleResearchOperations();
            ClearStaleSalvageOperations();
            RegisterResearchTerminalNetworkHandler();
            RegisterLocalFeedbackNetworkHandler();
            UpdateResearchDisplayModule();
        }

        private void ClearRuntimeState()
        {
            researchNotificationService.ClearAll();
            progressChatHeaderByIdentity.Clear();
            progressChatLastShownByKey.Clear();
            progressToastLastShownByKey.Clear();
            weldBotchWarningLastShownByKey.Clear();
            UnregisterResearchTerminalNetworkHandler();
            UnregisterLocalFeedbackNetworkHandler();
            blockWorkState.ClearPendingOperations();
            researchPedestalViewsByBlock.Clear();
            ClearResearchDisplayRuntimeState();
            ClearProficiencyRuntimeState();
        }

    }
}
