using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace WkKn
{
    public partial class WkKnSession
    {
        private enum CommandTargetStore
        {
            Research,
            Proficiency,
        }

        private static string[] TokenizeCommand(string message)
        {
            var tokens = new List<string>();
            if (string.IsNullOrWhiteSpace(message))
                return tokens.ToArray();

            var token = new StringBuilder();
            var inQuotes = false;
            for (var i = 0; i < message.Length; i++)
            {
                var c = message[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    AddCommandToken(tokens, token);
                    continue;
                }

                token.Append(c);
            }

            AddCommandToken(tokens, token);
            return tokens.ToArray();
        }

        private static void AddCommandToken(List<string> tokens, StringBuilder token)
        {
            if (token == null || token.Length == 0)
                return;

            tokens.Add(token.ToString());
            token.Length = 0;
        }

        private bool TryResolveCommandTargets(
            ulong sender,
            string token,
            CommandTargetStore storeKind,
            out List<long> identityIds,
            out string label,
            out string error)
        {
            identityIds = new List<long>();
            label = token;
            error = null;

            if (string.IsNullOrWhiteSpace(token))
            {
                error = "Missing player target.";
                return false;
            }

            if (token.Equals("me", StringComparison.OrdinalIgnoreCase))
            {
                var identityId = ResolveIdentityId(sender);
                if (identityId == 0)
                {
                    error = "Could not resolve your player identity.";
                    return false;
                }

                identityIds.Add(identityId);
                label = GetIdentityDisplayName(identityId);
                return true;
            }

            if (token.Equals("online", StringComparison.OrdinalIgnoreCase))
            {
                AddOnlineIdentityIds(identityIds);
                label = "online players";
                return identityIds.Count > 0 || FailNoPlayers(out error);
            }

            if (token.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                AddKnownIdentityIds(identityIds, storeKind);
                AddOnlineIdentityIds(identityIds);
                label = "known players";
                return identityIds.Count > 0 || FailNoPlayers(out error);
            }

            long parsedId;
            if (long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedId))
            {
                var identityId = ResolveIdentityId(parsedId);
                if (identityId == 0)
                {
                    error = "Could not resolve player id: " + token;
                    return false;
                }

                identityIds.Add(identityId);
                label = GetIdentityDisplayName(identityId);
                return true;
            }

            long namedIdentityId;
            if (!TryResolveOnlinePlayerName(token, out namedIdentityId, out error))
                return false;

            identityIds.Add(namedIdentityId);
            label = GetIdentityDisplayName(namedIdentityId);
            return true;
        }

        private static bool FailNoPlayers(out string error)
        {
            error = "No matching player identities found.";
            return false;
        }

        private void AddKnownIdentityIds(List<long> identityIds, CommandTargetStore storeKind)
        {
            if (storeKind == CommandTargetStore.Research)
            {
                if (researchStore.Data != null && researchStore.Data.Players != null)
                    AddScopeIdentityIds(identityIds, researchStore.Data.Players);
                return;
            }

            if (proficiencyStore.Data != null && proficiencyStore.Data.Players != null)
                AddProficiencyScopeIdentityIds(identityIds, proficiencyStore.Data.Players);
        }

        private static void AddScopeIdentityIds(List<long> identityIds, List<ResearchScopeRecord> scopes)
        {
            if (scopes == null)
                return;

            foreach (var scope in scopes)
            {
                long identityId;
                if (scope != null &&
                    long.TryParse(scope.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out identityId))
                    AddUniqueIdentityId(identityIds, identityId);
            }
        }

        private static void AddProficiencyScopeIdentityIds(List<long> identityIds, List<ProficiencyScopeRecord> scopes)
        {
            if (scopes == null)
                return;

            foreach (var scope in scopes)
            {
                long identityId;
                if (scope != null &&
                    long.TryParse(scope.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out identityId))
                    AddUniqueIdentityId(identityIds, identityId);
            }
        }

        private static void AddUniqueIdentityId(List<long> identityIds, long identityId)
        {
            if (identityId == 0 || identityIds.Contains(identityId))
                return;

            identityIds.Add(identityId);
        }

        private static void AddOnlineIdentityIds(List<long> identityIds)
        {
            if (MyAPIGateway.Players == null)
                return;

            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);
            foreach (var player in players)
                AddUniqueIdentityId(identityIds, player.IdentityId);
        }

        private bool TryResolveOnlinePlayerName(string name, out long identityId, out string error)
        {
            identityId = 0;
            error = null;

            if (MyAPIGateway.Players == null)
            {
                error = "Player list is not available.";
                return false;
            }

            var matches = new List<IMyPlayer>();
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players, player => player != null && player.IdentityId != 0);

            foreach (var player in players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.DisplayName))
                    continue;

                if (player.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    identityId = player.IdentityId;
                    return true;
                }

                if (player.DisplayName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches.Add(player);
            }

            if (matches.Count == 1)
            {
                identityId = matches[0].IdentityId;
                return true;
            }

            if (matches.Count > 1)
            {
                error = "Player name is ambiguous: " + name;
                return false;
            }

            error = "Online player not found: " + name;
            return false;
        }

        private string GetIdentityDisplayName(long identityId)
        {
            var player = FindPlayerByIdentity(identityId);
            if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                return player.DisplayName;

            return identityId.ToString(CultureInfo.InvariantCulture);
        }

        private bool TryResolveSchematicTarget(string text, out ResearchUnlockTarget target, out string error)
        {
            target = default(ResearchUnlockTarget);
            error = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Missing schematic name.";
                return false;
            }

            if (schematicCatalog.TryGetTargetByResearchId(text.Trim(), out target))
                return true;

            var normalized = NormalizeCommandLookup(text);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                error = "Missing schematic name.";
                return false;
            }

            var matches = new List<ResearchUnlockTarget>();
            foreach (var candidate in schematicCatalog.Targets)
            {
                if (IsExactSchematicMatch(candidate, text, normalized))
                {
                    target = candidate;
                    return true;
                }

                if (NormalizeCommandLookup(candidate.ResearchId).IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    NormalizeCommandLookup(candidate.DisplayName).IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    NormalizeCommandLookup(TrimSchematicsSuffix(candidate.DisplayName)).IndexOf(normalized, StringComparison.OrdinalIgnoreCase) >= 0)
                    matches.Add(candidate);
            }

            if (matches.Count == 1)
            {
                target = matches[0];
                return true;
            }

            if (matches.Count > 1)
            {
                error = "Schematic name is ambiguous: " + text + "\nMatches: " + FormatSchematicMatches(matches);
                return false;
            }

            error = "Unknown schematic: " + text;
            return false;
        }

        private static bool IsExactSchematicMatch(ResearchUnlockTarget candidate, string text, string normalized)
        {
            return candidate.DisplayName.Equals(text, StringComparison.OrdinalIgnoreCase) ||
                   TrimSchematicsSuffix(candidate.DisplayName).Equals(text, StringComparison.OrdinalIgnoreCase) ||
                   NormalizeCommandLookup(candidate.ResearchId).Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                   NormalizeCommandLookup(candidate.DisplayName).Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
                   NormalizeCommandLookup(TrimSchematicsSuffix(candidate.DisplayName)).Equals(normalized, StringComparison.OrdinalIgnoreCase);
        }

        private static string TrimSchematicsSuffix(string displayName)
        {
            const string suffix = " Schematics";
            if (string.IsNullOrWhiteSpace(displayName))
                return string.Empty;

            return displayName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? displayName.Substring(0, displayName.Length - suffix.Length)
                : displayName;
        }

        private static string NormalizeCommandLookup(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c))
                    builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }

        private static string FormatSchematicMatches(List<ResearchUnlockTarget> matches)
        {
            var names = new List<string>();
            foreach (var match in matches)
                names.Add(match.DisplayName);

            names.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(", ", names.ToArray());
        }

        private static string JoinCommandTokens(string[] args, int startIndex)
        {
            return JoinCommandTokens(args, startIndex, args == null ? 0 : args.Length - startIndex);
        }

        private static string JoinCommandTokens(string[] args, int startIndex, int count)
        {
            if (args == null || count <= 0 || startIndex < 0 || startIndex >= args.Length)
                return string.Empty;

            var end = Math.Min(args.Length, startIndex + count);
            var builder = new StringBuilder();
            for (var i = startIndex; i < end; i++)
            {
                if (builder.Length > 0)
                    builder.Append(' ');

                builder.Append(args[i]);
            }

            return builder.ToString();
        }

        private static bool TryParseCommandPercent(string text, out double progress, out string error)
        {
            progress = 0.0;
            error = null;

            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Missing percent.";
                return false;
            }

            var trimmed = text.Trim();
            var hasPercent = trimmed.EndsWith("%", StringComparison.Ordinal);
            if (hasPercent)
                trimmed = trimmed.Substring(0, trimmed.Length - 1);

            double value;
            if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                error = "Invalid percent: " + text;
                return false;
            }

            if (value < 0.0)
            {
                error = "Percent must be 0 or higher.";
                return false;
            }

            if (hasPercent || value > 1.0)
                value /= 100.0;

            if (value > 1.0)
            {
                error = "Percent must be 100 or lower.";
                return false;
            }

            progress = value;
            return true;
        }
    }
}
