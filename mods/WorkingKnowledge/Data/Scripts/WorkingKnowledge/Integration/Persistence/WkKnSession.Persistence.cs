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
            researchWorldStorageLoaded = false;
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
                        {
                            researchStore.SetData(loaded);
                            researchWorldStorageLoaded = true;
                        }
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
            configWorldStorageLoaded = false;
            configStore.Reset();

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ConfigStorageFile, typeof(WkKnSession)))
                    return;

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ConfigStorageFile, typeof(WkKnSession)))
                {
                    var xml = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var loaded = MyAPIGateway.Utilities.SerializeFromXML<WkConfig>(xml);
                        if (loaded != null)
                        {
                            configStore.SetData(loaded);
                            configWorldStorageLoaded = true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                configStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load config; using defaults: " + exception);
            }
        }

        private void LoadPlayerConfigStore()
        {
            playerConfigWorldStorageLoaded = false;
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
                        {
                            playerConfigStore.SetData(loaded);
                            playerConfigWorldStorageLoaded = true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                playerConfigStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load player config; using defaults: " + exception);
            }
        }

        private void NormalizeResearchStore()
        {
            researchStore.Normalize(RequiredResearchProgress);
        }

    }
}
