using System;
using System.Collections.Generic;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void ApplyFundamentalsDefaultsForOnlinePlayers()
        {
            if (!ShouldApplyFundamentalsDefaults())
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);

            var changed = false;
            foreach (var player in players)
                changed = ApplyFundamentalsDefaultsForIdentityCore(player.IdentityId) || changed;

            if (changed)
                SaveDirtyStores();
        }

        private void ApplyFundamentalsDefaultsForIdentity(long identityId)
        {
            if (!ShouldApplyFundamentalsDefaults() || identityId == 0)
                return;

            if (ApplyFundamentalsDefaultsForIdentityCore(identityId))
                SaveDirtyStores();
        }

        private bool ShouldApplyFundamentalsDefaults()
        {
            return MyAPIGateway.Session != null &&
                   MyAPIGateway.Session.IsServer &&
                   config != null &&
                   (config.FundamentalsResearchUnlocked || config.FundamentalsProficiencyProgress > 0.0);
        }

        private bool ApplyFundamentalsDefaultsForIdentityCore(long identityId)
        {
            var changed = false;
            if (config.FundamentalsResearchUnlocked)
                changed = EnsureFundamentalsResearchUnlocked(identityId) || changed;

            if (config.FundamentalsProficiencyProgress > 0.0)
                changed = EnsureFundamentalsProficiency(identityId, config.FundamentalsProficiencyProgress) || changed;

            return changed;
        }

        private bool EnsureFundamentalsResearchUnlocked(long identityId)
        {
            ResearchUnlockTarget target;
            if (!schematicCatalog.TryGetTargetByResearchId(FundamentalsResearchId, out target))
                return false;

            var result = researchService.RecordProgress(
                researchStore,
                researchStore.PlayerScopes,
                identityId.ToString(),
                target.ResearchId,
                target.UnlockerId.SubtypeName,
                RequiredResearchProgress);

            vanillaResearchMirror.UnlockForPlayer(identityId, target.UnlockerId);

            if (!result.Changed)
                return false;

            NotifyResearchDisplayChanged(identityId);
            return true;
        }

        private bool EnsureFundamentalsProficiency(long identityId, double baselineProgress)
        {
            var result = proficiencyService.EnsureMinimumProgress(proficiencyStore, identityId, FundamentalsResearchId, baselineProgress, simulationTick);
            if (!result.Changed)
                return false;

            NotifyProficiencyDisplayChanged(identityId);
            return true;
        }
    }
}
