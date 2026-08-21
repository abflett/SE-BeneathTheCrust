using System;
using Sandbox.ModAPI;
using VRage.Game.Components.Session;
using VRage.Utils;

namespace WkKn
{
    public class WorkingKnowledgeSaveData
    {
        public int Version;
        public WkConfig Config;
        public WkPlayerConfigSaveData PlayerConfig;
        public ResearchSaveData Research;
        public ProficiencySaveData Proficiency;
    }

    public partial class WkKnSession
    {
        private const int CheckpointPersistenceVersion = 1;
        private const string CheckpointPersistenceNamespace = "WkKn.Persistence";
        private const string CheckpointPersistenceKey = "State";

        private PersistenceLoadSource persistenceLoadSource;
        private bool checkpointSaveBlocked;
        private bool configWorldStorageLoaded;
        private bool playerConfigWorldStorageLoaded;
        private bool researchWorldStorageLoaded;
        private bool proficiencyWorldStorageLoaded;
        private bool persistenceStatusMessageShown;
        private string persistenceStatusMessage;
        private bool persistenceStatusIsError;

        private enum PersistenceLoadSource
        {
            None,
            Fresh,
            LegacyWorldStorage,
            Checkpoint,
            Failed,
        }

        private void ResetPersistenceStores()
        {
            configStore.Reset();
            playerConfigStore.Reset();
            researchStore.Reset();
            proficiencyStore.Reset();
            persistenceLoadSource = PersistenceLoadSource.None;
            checkpointSaveBlocked = false;
            persistenceStatusMessageShown = false;
            persistenceStatusMessage = null;
            persistenceStatusIsError = false;
        }

        private bool LoadCheckpointOrLegacyPersistence()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer)
                return false;

            var storage = MySessionComponentScriptSharedStorage.Instance;
            if (storage == null)
            {
                FailCheckpointPersistence("shared checkpoint storage is unavailable");
                return false;
            }

            string xml;
            try
            {
                xml = storage.ReadString(CheckpointPersistenceNamespace, CheckpointPersistenceKey);
            }
            catch (Exception exception)
            {
                FailCheckpointPersistence("failed to read the Sandbox.sbc persistence entry", exception);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(xml))
                return LoadCheckpointPersistence(xml);

            LoadConfigStore();
            LoadPlayerConfigStore();
            LoadResearchStore();
            LoadProficiencyStore();

            var loadedLegacyData = configWorldStorageLoaded ||
                                   playerConfigWorldStorageLoaded ||
                                   researchWorldStorageLoaded ||
                                   proficiencyWorldStorageLoaded;

            if (loadedLegacyData)
            {
                persistenceLoadSource = PersistenceLoadSource.LegacyWorldStorage;
                persistenceStatusMessage = "Loaded legacy Working Knowledge data. It will be migrated to the new save format on the next world save.";
                MyLog.Default.WriteLineAndConsole(LogPrefix + " loaded legacy WkKn*.xml world storage; migration to Sandbox.sbc is pending the next world save.");
            }
            else
            {
                persistenceLoadSource = PersistenceLoadSource.Fresh;
                persistenceStatusMessage = "Working Knowledge initialized successfully.";
                MyLog.Default.WriteLineAndConsole(LogPrefix + " no saved Working Knowledge data was found; initialized a fresh state.");
            }

            return true;
        }

        private bool LoadCheckpointPersistence(string xml)
        {
            try
            {
                var loaded = MyAPIGateway.Utilities.SerializeFromXML<WorkingKnowledgeSaveData>(xml);
                if (loaded == null)
                    throw new InvalidOperationException("the persistence entry is empty");
                if (loaded.Version != CheckpointPersistenceVersion)
                    throw new InvalidOperationException("unsupported persistence version " + loaded.Version);
                if (loaded.Config == null || loaded.PlayerConfig == null || loaded.Research == null || loaded.Proficiency == null)
                    throw new InvalidOperationException("the persistence entry is incomplete");

                configStore.SetData(loaded.Config);
                playerConfigStore.SetData(loaded.PlayerConfig);
                researchStore.SetData(loaded.Research);
                proficiencyStore.SetData(loaded.Proficiency);
                NormalizeResearchStore();
                NormalizeProficiencyStore();

                persistenceLoadSource = PersistenceLoadSource.Checkpoint;
                persistenceStatusMessage = "Working Knowledge loaded successfully.";
                MyLog.Default.WriteLineAndConsole(LogPrefix + " loaded canonical persistence version " + CheckpointPersistenceVersion + " from Sandbox.sbc.");
                return true;
            }
            catch (Exception exception)
            {
                FailCheckpointPersistence("failed to load the Sandbox.sbc persistence entry", exception);
                return false;
            }
        }

        private void SaveCheckpointPersistence()
        {
            if (MyAPIGateway.Session == null || !MyAPIGateway.Session.IsServer || checkpointSaveBlocked)
                return;

            var storage = MySessionComponentScriptSharedStorage.Instance;
            if (storage == null)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " shared checkpoint storage is unavailable; Working Knowledge persistence was not saved.");
                return;
            }

            try
            {
                configStore.Normalize();
                playerConfigStore.Normalize();
                NormalizeResearchStore();
                NormalizeProficiencyStore();

                var snapshot = new WorkingKnowledgeSaveData
                {
                    Version = CheckpointPersistenceVersion,
                    Config = config,
                    PlayerConfig = playerConfigStore.Data,
                    Research = researchStore.Data,
                    Proficiency = proficiencyStore.Data,
                };

                var xml = MyAPIGateway.Utilities.SerializeToXML(snapshot);
                if (!storage.Write(CheckpointPersistenceNamespace, CheckpointPersistenceKey, xml))
                    throw new InvalidOperationException("shared checkpoint storage rejected the persistence write");

                playerConfigStore.MarkClean();
                researchStore.MarkClean();
                proficiencyStore.MarkClean();

                if (persistenceLoadSource == PersistenceLoadSource.LegacyWorldStorage)
                {
                    MyLog.Default.WriteLineAndConsole(LogPrefix + " migrated legacy WkKn*.xml data to canonical Sandbox.sbc persistence version " + CheckpointPersistenceVersion + ".");
                    persistenceLoadSource = PersistenceLoadSource.Checkpoint;
                }
                else if (persistenceLoadSource == PersistenceLoadSource.Fresh)
                {
                    MyLog.Default.WriteLineAndConsole(LogPrefix + " created canonical Sandbox.sbc persistence version " + CheckpointPersistenceVersion + ".");
                    persistenceLoadSource = PersistenceLoadSource.Checkpoint;
                }
                else
                {
                    MyLog.Default.WriteLineAndConsole(LogPrefix + " saved canonical persistence version " + CheckpointPersistenceVersion + " to Sandbox.sbc.");
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to save canonical persistence to Sandbox.sbc: " + exception);
            }
        }

        private void FailCheckpointPersistence(string reason)
        {
            FailCheckpointPersistence(reason, null);
        }

        private void FailCheckpointPersistence(string reason, Exception exception)
        {
            persistenceLoadSource = PersistenceLoadSource.Failed;
            checkpointSaveBlocked = true;
            persistenceStatusMessage = "Working Knowledge could not load its saved data. Check the log before saving this world.";
            persistenceStatusIsError = true;

            var detail = exception == null ? reason + "." : reason + ": " + exception;
            MyLog.Default.WriteLineAndConsole(LogPrefix + " " + detail + " Canonical persistence writes are disabled for this session; legacy XML was not loaded.");
        }

        private void TryShowPersistenceStatusMessage()
        {
            if (persistenceStatusMessageShown || string.IsNullOrWhiteSpace(persistenceStatusMessage) || simulationTick < 60)
                return;
            if (MyAPIGateway.Session == null || MyAPIGateway.Session.Player == null)
                return;

            if (persistenceStatusIsError)
                ShowWkTargetColoredChatMessage(GetLocalIdentityId(), "Working Knowledge", persistenceStatusMessage, WkChatWarningColor);
            else
                ShowWkColoredChatMessage("Working Knowledge", persistenceStatusMessage, WkChatInfoColor);

            persistenceStatusMessageShown = true;
        }
    }
}
