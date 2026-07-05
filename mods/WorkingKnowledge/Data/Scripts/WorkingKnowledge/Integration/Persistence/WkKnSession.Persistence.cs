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
        private void LoadResearchStore()
        {
            researchStore.Reset();

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ResearchStorageFile, typeof(WkKnSession)))
                    return;

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ResearchStorageFile, typeof(WkKnSession)))
                {
                    var xml = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var loaded = MyAPIGateway.Utilities.SerializeFromXML<ResearchSaveData>(xml);
                        if (loaded != null)
                            researchStore.SetData(loaded);
                    }
                }

                NormalizeResearchStore();
            }
            catch (Exception exception)
            {
                researchStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load research store; starting fresh: " + exception);
            }
        }

        private void LoadConfigStore()
        {
            configStore.Reset();

            try
            {
                var configExists = MyAPIGateway.Utilities.FileExistsInWorldStorage(ConfigStorageFile, typeof(WkKnSession));
                var configLoaded = false;
                if (configExists)
                {
                    using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ConfigStorageFile, typeof(WkKnSession)))
                    {
                        var xml = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(xml))
                        {
                            var loaded = MyAPIGateway.Utilities.SerializeFromXML<WkConfig>(xml);
                            if (loaded != null)
                            {
                                configStore.SetData(loaded);
                                configLoaded = true;
                            }
                        }
                    }

                    if (!configLoaded)
                        SaveConfigStore();
                }
                else
                {
                    SaveConfigStore();
                }

            }
            catch (Exception exception)
            {
                configStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load config; using defaults: " + exception);
                SaveConfigStore();
            }
        }

        private void LoadPlayerConfigStore()
        {
            playerConfigStore.Reset();

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(PlayerConfigStorageFile, typeof(WkKnSession)))
                    return;

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(PlayerConfigStorageFile, typeof(WkKnSession)))
                {
                    var xml = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var loaded = MyAPIGateway.Utilities.SerializeFromXML<WkPlayerConfigSaveData>(xml);
                        if (loaded != null)
                            playerConfigStore.SetData(loaded);
                    }
                }
            }
            catch (Exception exception)
            {
                playerConfigStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load player config; using defaults: " + exception);
            }
        }

        private void SaveConfigStore()
        {
            try
            {
                configStore.Normalize();
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ConfigStorageFile, typeof(WkKnSession)))
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(config));
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to save config: " + exception);
            }
        }

        private void SavePlayerConfigStore()
        {
            if (!playerConfigStore.IsDirty)
                return;

            try
            {
                playerConfigStore.Normalize();
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(PlayerConfigStorageFile, typeof(WkKnSession)))
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(playerConfigStore.Data));

                playerConfigStore.MarkClean();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to save player config: " + exception);
            }
        }

        private void SaveResearchStore()
        {
            if (!researchStore.IsDirty)
                return;

            try
            {
                NormalizeResearchStore();
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ResearchStorageFile, typeof(WkKnSession)))
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(researchStore.Data));

                researchStore.MarkClean();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to save research store: " + exception);
            }
        }

        private void NormalizeResearchStore()
        {
            researchStore.Normalize(RequiredResearchProgress);
        }

    }
}
