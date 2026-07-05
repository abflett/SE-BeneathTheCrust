using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void HandleResearchCommand(ulong sender, string[] args)
        {
            if (args.Length == 2)
            {
                ShowResearchSummary(ResolveIdentityId(sender));
                return;
            }

            if (args.Length == 3 && args[2].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (!CanEditConfig(sender))
                {
                    ShowWkWarningMessage("Admin required for research help.");
                    return;
                }

                ShowResearchHelp();
                return;
            }

            if (!CanEditConfig(sender))
            {
                ShowWkWarningMessage("Admin required for research commands.");
                return;
            }

            var action = args[2];
            if (action.Equals("show", StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchShowCommand(sender, args);
                return;
            }

            if (action.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchResetCommand(sender, args);
                return;
            }

            if (action.Equals("unlock", StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchUnlockCommand(sender, args);
                return;
            }

            if (action.Equals("forget", StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchForgetCommand(sender, args);
                return;
            }

            if (action.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                HandleResearchSetCommand(sender, args);
                return;
            }

            ShowResearchHelp();
        }

        private void HandleResearchShowCommand(ulong sender, string[] args)
        {
            if (args.Length != 4)
            {
                ShowResearchHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Research, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            foreach (var identityId in identityIds)
                ShowResearchSummary(identityId, "Research - " + GetIdentityDisplayName(identityId));
        }

        private void HandleResearchResetCommand(ulong sender, string[] args)
        {
            if (args.Length != 4)
            {
                ShowResearchHelp();
                return;
            }

            if (args[3].Equals("server", StringComparison.OrdinalIgnoreCase))
            {
                ResetServerResearchForAdmin();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Research, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            var changed = 0;
            foreach (var identityId in identityIds)
            {
                ResetResearchForIdentity(identityId);
                changed++;
            }

            SaveResearchStore();
            ShowWkChatSection(
                "Research Reset",
                "Target: " + label,
                "Players reset: " + changed);
        }

        private void HandleResearchUnlockCommand(ulong sender, string[] args)
        {
            if (args.Length < 5)
            {
                ShowResearchHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Research, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            ResearchUnlockTarget target;
            if (!TryResolveSchematicTarget(JoinCommandTokens(args, 4), out target, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            var changed = 0;
            foreach (var identityId in identityIds)
            {
                if (SetResearchProgressForIdentity(identityId, target, RequiredResearchProgress))
                    changed++;

                vanillaResearchMirror.UnlockForPlayer(identityId, target.UnlockerId);
                NotifyResearchDisplayChanged(identityId);
            }

            SaveResearchStore();
            ShowWkChatSection(
                "Research Unlocked",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Players changed: " + changed);
        }

        private void HandleResearchForgetCommand(ulong sender, string[] args)
        {
            if (args.Length < 5)
            {
                ShowResearchHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Research, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            ResearchUnlockTarget target;
            if (!TryResolveSchematicTarget(JoinCommandTokens(args, 4), out target, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            var changed = 0;
            foreach (var identityId in identityIds)
            {
                if (RemoveResearchSchematic(identityId, target.ResearchId))
                    changed++;

                vanillaResearchMirror.LockForPlayer(identityId, target.UnlockerId);
                NotifyResearchDisplayChanged(identityId);
            }

            SaveResearchStore();
            ShowWkChatSection(
                "Research Revoked",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Players changed: " + changed);
        }

        private void HandleResearchSetCommand(ulong sender, string[] args)
        {
            if (args.Length < 6)
            {
                ShowResearchHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Research, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            double progress;
            if (!TryParseCommandPercent(args[args.Length - 1], out progress, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            ResearchUnlockTarget target;
            if (!TryResolveSchematicTarget(JoinCommandTokens(args, 4, args.Length - 5), out target, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            var changed = 0;
            foreach (var identityId in identityIds)
            {
                if (SetResearchProgressForIdentity(identityId, target, progress))
                    changed++;

                if (progress >= RequiredResearchProgress)
                    vanillaResearchMirror.UnlockForPlayer(identityId, target.UnlockerId);
                else
                    vanillaResearchMirror.LockForPlayer(identityId, target.UnlockerId);

                NotifyResearchDisplayChanged(identityId);
            }

            SaveResearchStore();
            ShowWkChatSection(
                "Research Set",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Progress: " + FormatProgress(progress),
                "Players changed: " + changed);
        }

        private void ResetServerResearchForAdmin()
        {
            researchStore.Reset();
            researchStore.MarkDirty();
            vanillaResearchMirror.ClearForAllPlayers();

            var defaultsApplied = ApplyFundamentalsResearchDefaultForOnlinePlayers();
            NotifyResearchDisplaysForOnlinePlayers();
            SaveResearchStore();

            ShowWkChatSection(
                "Research Server Reset",
                "Cleared all Working Knowledge research.",
                "Cleared vanilla research for all players.",
                "Fundamentals reapplied to online players: " + defaultsApplied);
        }

        private void ResetResearchForIdentity(long identityId)
        {
            RemoveResearchScope(identityId);
            vanillaResearchMirror.ClearForPlayer(identityId);

            if (config.FundamentalsResearchUnlocked)
                EnsureFundamentalsResearchUnlocked(identityId);

            NotifyResearchDisplayChanged(identityId);
        }

        private int ApplyFundamentalsResearchDefaultForOnlinePlayers()
        {
            if (!config.FundamentalsResearchUnlocked || MyAPIGateway.Players == null)
                return 0;

            var players = new List<VRage.Game.ModAPI.IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);

            var applied = 0;
            foreach (var player in players)
            {
                if (EnsureFundamentalsResearchUnlocked(player.IdentityId))
                    applied++;
            }

            return applied;
        }

        private void NotifyResearchDisplaysForOnlinePlayers()
        {
            if (MyAPIGateway.Players == null)
                return;

            var players = new List<VRage.Game.ModAPI.IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);
            foreach (var player in players)
                NotifyResearchDisplayChanged(player.IdentityId);
        }

        private bool SetResearchProgressForIdentity(long identityId, ResearchUnlockTarget target, double progress)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(target.ResearchId))
                return false;

            progress = Clamp01(progress);
            if (progress <= 0.0)
                return RemoveResearchSchematic(identityId, target.ResearchId);

            var scope = researchStore.GetOrCreateScope(researchStore.PlayerScopes, identityId.ToString());
            var schematic = researchStore.GetOrCreateSchematic(scope, target.ResearchId);
            var unlocked = progress >= RequiredResearchProgress;
            var changed = !string.Equals(schematic.UnlockerSubtype, target.UnlockerId.SubtypeName, StringComparison.OrdinalIgnoreCase) ||
                          Math.Abs(schematic.Progress - progress) > ResearchProgressPrecision ||
                          schematic.Unlocked != unlocked ||
                          schematic.ActiveProgress > 0.0 ||
                          !string.IsNullOrWhiteSpace(schematic.ActiveToken) ||
                          (schematic.Ledger != null && schematic.Ledger.Count > 0);

            schematic.UnlockerSubtype = target.UnlockerId.SubtypeName;
            schematic.Progress = unlocked ? RequiredResearchProgress : progress;
            schematic.Unlocked = unlocked;
            schematic.ActiveToken = null;
            schematic.ActiveProgress = 0.0;
            if (schematic.Ledger == null)
                schematic.Ledger = new List<ResearchLedgerEntry>();
            else
                schematic.Ledger.Clear();

            if (changed)
                researchStore.MarkDirty();

            return changed;
        }

        private bool RemoveResearchScope(long identityId)
        {
            var scopes = researchStore.PlayerScopes;
            for (var i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i] != null && string.Equals(scopes[i].Id, identityId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    scopes.RemoveAt(i);
                    researchStore.MarkDirty();
                    return true;
                }
            }

            researchStore.MarkDirty();
            return false;
        }

        private bool RemoveResearchSchematic(long identityId, string researchId)
        {
            var scope = researchStore.FindScope(researchStore.PlayerScopes, identityId.ToString());
            if (scope == null || scope.Schematics == null)
                return false;

            for (var i = scope.Schematics.Count - 1; i >= 0; i--)
            {
                var schematic = scope.Schematics[i];
                if (schematic != null && string.Equals(schematic.ResearchId, researchId, StringComparison.OrdinalIgnoreCase))
                {
                    scope.Schematics.RemoveAt(i);
                    researchStore.MarkDirty();
                    return true;
                }
            }

            return false;
        }

        private void ShowResearchSummary(long identityId, string heading)
        {
            var lines = researchSummaryService.BuildPersonalSummary(FindScope(researchStore.PlayerScopes, identityId.ToString()), GetSchematicDisplayName);
            ShowWkChatSection(heading, lines);
        }
    }
}
