using System;
using System.Collections.Generic;
using Sandbox.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private void HandleProficiencyCommand(ulong sender, string[] args)
        {
            if (args.Length == 2)
            {
                ShowProficiencySummary(ResolveIdentityId(sender));
                return;
            }

            if (args.Length == 3 && args[2].Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                if (!CanEditConfig(sender))
                {
                    ShowWkWarningMessage("Admin required for Proficiency help.");
                    return;
                }

                ShowProficiencyHelp();
                return;
            }

            if (!CanEditConfig(sender))
            {
                ShowWkWarningMessage("Admin required for Proficiency commands.");
                return;
            }

            var action = args[2];
            if (action.Equals("show", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencyShowCommand(sender, args);
                return;
            }

            if (action.Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencyResetCommand(sender, args);
                return;
            }

            if (action.Equals("master", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencyMasterCommand(sender, args);
                return;
            }

            if (action.Equals("forget", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencyForgetCommand(sender, args);
                return;
            }

            if (action.Equals("set", StringComparison.OrdinalIgnoreCase))
            {
                HandleProficiencySetCommand(sender, args);
                return;
            }

            ShowProficiencyHelp();
        }

        private void HandleProficiencyShowCommand(ulong sender, string[] args)
        {
            if (args.Length != 4)
            {
                ShowProficiencyHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Proficiency, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            foreach (var identityId in identityIds)
                ShowProficiencySummary(identityId, "Proficiency - " + GetIdentityDisplayName(identityId));
        }

        private void HandleProficiencyResetCommand(ulong sender, string[] args)
        {
            if (args.Length != 4)
            {
                ShowProficiencyHelp();
                return;
            }

            if (args[3].Equals("server", StringComparison.OrdinalIgnoreCase))
            {
                ResetServerProficiencyForAdmin();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Proficiency, out identityIds, out label, out error))
            {
                ShowWkWarningMessage(error);
                return;
            }

            var changed = 0;
            foreach (var identityId in identityIds)
            {
                ResetProficiencyForIdentity(identityId);
                changed++;
            }

            SaveProficiencyStore();
            ShowWkChatSection(
                "Proficiency Reset",
                "Target: " + label,
                "Players reset: " + changed);
        }

        private void HandleProficiencyMasterCommand(ulong sender, string[] args)
        {
            if (args.Length < 5)
            {
                ShowProficiencyHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Proficiency, out identityIds, out label, out error))
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
                if (SetProficiencyProgressForIdentity(identityId, target.ResearchId, RequiredResearchProgress))
                    changed++;

                NotifyProficiencyDisplayChanged(identityId);
            }

            SaveProficiencyStore();
            ShowWkChatSection(
                "Proficiency Mastered",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Players changed: " + changed);
        }

        private void HandleProficiencyForgetCommand(ulong sender, string[] args)
        {
            if (args.Length < 5)
            {
                ShowProficiencyHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Proficiency, out identityIds, out label, out error))
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
                if (RemoveProficiencyRecord(identityId, target.ResearchId))
                    changed++;

                NotifyProficiencyDisplayChanged(identityId);
            }

            SaveProficiencyStore();
            ShowWkChatSection(
                "Proficiency Cleared",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Players changed: " + changed);
        }

        private void HandleProficiencySetCommand(ulong sender, string[] args)
        {
            if (args.Length < 6)
            {
                ShowProficiencyHelp();
                return;
            }

            List<long> identityIds;
            string label;
            string error;
            if (!TryResolveCommandTargets(sender, args[3], CommandTargetStore.Proficiency, out identityIds, out label, out error))
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
                if (SetProficiencyProgressForIdentity(identityId, target.ResearchId, progress))
                    changed++;

                NotifyProficiencyDisplayChanged(identityId);
            }

            SaveProficiencyStore();
            ShowWkChatSection(
                "Proficiency Set",
                "Target: " + label,
                "Schematic: " + target.DisplayName,
                "Progress: " + FormatProgress(progress),
                "Players changed: " + changed);
        }

        private void ResetServerProficiencyForAdmin()
        {
            proficiencyStore.Reset();
            proficiencyStore.MarkDirty();

            var defaultsApplied = ApplyFundamentalsProficiencyDefaultForOnlinePlayers();
            NotifyProficiencyDisplaysForOnlinePlayers();
            SaveProficiencyStore();

            ShowWkChatSection(
                "Proficiency Server Reset",
                "Cleared all Working Knowledge Proficiency.",
                "Fundamentals reapplied to online players: " + defaultsApplied);
        }

        private void ResetProficiencyForIdentity(long identityId)
        {
            RemoveProficiencyScope(identityId);

            if (config.FundamentalsProficiencyProgress > 0.0)
                EnsureFundamentalsProficiency(identityId, config.FundamentalsProficiencyProgress);

            NotifyProficiencyDisplayChanged(identityId);
        }

        private int ApplyFundamentalsProficiencyDefaultForOnlinePlayers()
        {
            if (config.FundamentalsProficiencyProgress <= 0.0 || MyAPIGateway.Players == null)
                return 0;

            var players = new List<VRage.Game.ModAPI.IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);

            var applied = 0;
            foreach (var player in players)
            {
                if (EnsureFundamentalsProficiency(player.IdentityId, config.FundamentalsProficiencyProgress))
                    applied++;
            }

            return applied;
        }

        private void NotifyProficiencyDisplaysForOnlinePlayers()
        {
            if (MyAPIGateway.Players == null)
                return;

            var players = new List<VRage.Game.ModAPI.IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);
            foreach (var player in players)
                NotifyProficiencyDisplayChanged(player.IdentityId);
        }

        private bool SetProficiencyProgressForIdentity(long identityId, string researchId, double progress)
        {
            if (identityId == 0 || string.IsNullOrWhiteSpace(researchId))
                return false;

            progress = Clamp01(progress);
            if (progress <= 0.0)
                return RemoveProficiencyRecord(identityId, researchId);

            var scope = proficiencyStore.GetOrCreateScope(identityId.ToString());
            var proficiency = proficiencyStore.GetOrCreateProficiency(scope, ProficiencyService.GetProficiencyIdForResearchId(researchId), simulationTick);
            var changed = Math.Abs(proficiency.Progress - progress) > ResearchProgressPrecision;
            proficiency.Progress = progress;
            proficiency.LastTouchedTick = simulationTick;

            if (changed)
                proficiencyStore.MarkDirty();

            return changed;
        }

        private bool RemoveProficiencyScope(long identityId)
        {
            var scopes = proficiencyStore.PlayerScopes;
            for (var i = scopes.Count - 1; i >= 0; i--)
            {
                if (scopes[i] != null && string.Equals(scopes[i].Id, identityId.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    scopes.RemoveAt(i);
                    proficiencyStore.MarkDirty();
                    return true;
                }
            }

            proficiencyStore.MarkDirty();
            return false;
        }

        private bool RemoveProficiencyRecord(long identityId, string researchId)
        {
            var scope = proficiencyStore.FindScope(identityId.ToString());
            if (scope == null || scope.Proficiencies == null)
                return false;

            var proficiencyId = ProficiencyService.GetProficiencyIdForResearchId(researchId);
            for (var i = scope.Proficiencies.Count - 1; i >= 0; i--)
            {
                var proficiency = scope.Proficiencies[i];
                if (proficiency != null && string.Equals(proficiency.ProficiencyId, proficiencyId, StringComparison.OrdinalIgnoreCase))
                {
                    scope.Proficiencies.RemoveAt(i);
                    proficiencyStore.MarkDirty();
                    return true;
                }
            }

            return false;
        }

        private void ShowProficiencySummary(long identityId, string heading)
        {
            var lines = proficiencySummaryService.BuildPersonalSummary(FindProficiencyScope(identityId.ToString()), GetSchematicDisplayName);
            ShowWkChatSection(heading, lines);
        }
    }
}
