using System;
using System.Collections.Generic;
using Sandbox.Game;
using VRage.Game;
using VRage.Game.ObjectBuilders;

namespace WkKn
{
    internal sealed class VanillaResearchMirror
    {
        internal void UnlockForPlayer(long identityId, MyDefinitionId unlockerId)
        {
            if (identityId == 0)
                return;

            MyVisualScriptLogicProvider.PlayerResearchUnlock(identityId, unlockerId);
        }

        internal void LockForPlayer(long identityId, MyDefinitionId unlockerId)
        {
            if (identityId == 0)
                return;

            MyVisualScriptLogicProvider.PlayerResearchLock(identityId, unlockerId);
        }

        internal void ClearForPlayer(long identityId)
        {
            if (identityId == 0)
                return;

            MyVisualScriptLogicProvider.PlayerResearchClear(identityId);
        }

        internal void ClearForAllPlayers()
        {
            MyVisualScriptLogicProvider.PlayerResearchClearAll();
        }

        internal int SyncCompletedForIdentity(ResearchStore store, ResearchService researchService, long identityId)
        {
            if (store == null || researchService == null || identityId == 0)
                return 0;

            var completedUnlockers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            researchService.AddCompletedUnlockers(store, store.PlayerScopes, identityId.ToString(), completedUnlockers);

            foreach (var unlockerSubtype in completedUnlockers)
            {
                var unlockerId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), unlockerSubtype);
                UnlockForPlayer(identityId, unlockerId);
            }

            return completedUnlockers.Count;
        }
    }
}
