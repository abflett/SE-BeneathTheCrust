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
        private double GetPlayerResearchProgress(long identityId, string researchId)
        {
            return researchService.GetPlayerProgress(researchStore, identityId, researchId);
        }

        private void AwardResearch(long identityId, ResearchUnlockTarget target, double workScale, double fullWorkReward)
        {
            if (identityId == 0 || workScale <= 0.0 || fullWorkReward <= 0.0)
                return;

            var progress = GenerateGrindResearchProgress(workScale, fullWorkReward, GetPlayerResearchProgress(identityId, target.ResearchId));
            if (progress <= 0.0)
                return;

            AwardResearchProgress(identityId, target, progress, "grinding");
        }

        private void AwardResearchProgress(long identityId, ResearchUnlockTarget target, double progressAmount, string source)
        {
            if (identityId == 0 || progressAmount <= 0.0)
                return;

            var progress = RecordResearchContribution(identityId, target, progressAmount);
            if (!progress.Changed)
                return;

            QueueResearchNotification(identityId, target, progress, source);

            if (progress.Unlocked)
            {
                vanillaResearchMirror.UnlockForPlayer(identityId, target.UnlockerId);
                if (!string.Equals(target.ResearchId, FundamentalsResearchId, StringComparison.OrdinalIgnoreCase))
                    ShowWkResearchCompletionFeedback(identityId, target.DisplayName);

                SaveResearchStore();
            }
        }

        private ResearchProgressResult RecordResearchContribution(long identityId, ResearchUnlockTarget target, double progress)
        {
            if (identityId == 0)
                return ResearchProgressResult.NoChange;

            var unlockerSubtype = target.UnlockerId.SubtypeName;
            var researchId = target.ResearchId;
            if (progress <= 0.0)
                return ResearchProgressResult.NoChange;

            var result = RecordResearchProgress(researchStore.PlayerScopes, identityId.ToString(), researchId, unlockerSubtype, progress);

            if (result.Changed)
            {
                NotifyResearchDisplayChanged(identityId);
            }

            return result;
        }

        private double GenerateGrindResearchProgress(double workScale, double fullWorkReward, double currentProgress)
        {
            return researchService.GenerateGrindProgress(
                workScale,
                fullWorkReward,
                config.ResearchScale * config.ResearchGrindingGainScale,
                config.ResearchEfficiencyStart,
                config.ResearchEfficiencyEnd,
                currentProgress);
        }

        private ResearchProgressResult RecordResearchProgress(List<ResearchScopeRecord> scopes, string scopeId, string researchId, string unlockerSubtype, double progress)
        {
            return researchService.RecordProgress(researchStore, scopes, scopeId, researchId, unlockerSubtype, progress);
        }

        private void SyncCompletedResearchForOnlinePlayers()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);

            foreach (var player in players)
                SyncCompletedResearchForIdentity(player.IdentityId);
        }

        private void SyncCompletedResearchForIdentity(long identityId)
        {
            vanillaResearchMirror.SyncCompletedForIdentity(researchStore, researchService, identityId);
        }

    }
}
