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
        private void LoadProficiencyStore()
        {
            proficiencyStore.Reset();

            try
            {
                if (!MyAPIGateway.Utilities.FileExistsInWorldStorage(ProficiencyStorageFile, typeof(WkKnSession)))
                    return;

                using (var reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(ProficiencyStorageFile, typeof(WkKnSession)))
                {
                    var xml = reader.ReadToEnd();
                    if (!string.IsNullOrWhiteSpace(xml))
                    {
                        var loaded = MyAPIGateway.Utilities.SerializeFromXML<ProficiencySaveData>(xml);
                        if (loaded != null)
                            proficiencyStore.SetData(loaded);
                    }
                }

                NormalizeProficiencyStore();
            }
            catch (Exception exception)
            {
                proficiencyStore.Reset();
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to load proficiency store; starting fresh: " + exception);
            }
        }

        private void SaveProficiencyStore()
        {
            if (!proficiencyStore.IsDirty)
                return;

            try
            {
                NormalizeProficiencyStore();
                using (var writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(ProficiencyStorageFile, typeof(WkKnSession)))
                    writer.Write(MyAPIGateway.Utilities.SerializeToXML(proficiencyStore.Data));

                proficiencyStore.MarkClean();
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(LogPrefix + " failed to save proficiency store: " + exception);
            }
        }

        private void NormalizeProficiencyStore()
        {
            proficiencyStore.Normalize();
        }

        private ProficiencyScopeRecord FindProficiencyScope(string playerId)
        {
            return proficiencyStore.FindScope(playerId);
        }

        private void NormalizeProficiencyScope(ProficiencyScopeRecord scope)
        {
            proficiencyStore.NormalizeScope(scope);
        }

    }
}
