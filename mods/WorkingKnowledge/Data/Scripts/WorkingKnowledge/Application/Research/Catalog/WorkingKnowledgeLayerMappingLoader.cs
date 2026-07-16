using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage.Game;

namespace WkKn
{
    internal static class WorkingKnowledgeLayerMappingLoader
    {
        private const string MappingFilePath = "Data/WorkingKnowledge/block_mappings.txt";
        private const string GroupFilePath = "Data/WorkingKnowledge/schematic_groups.txt";
        private const string SupportedGroupFormatVersion = "1";
        private const string UnlockerSubtypePrefix = "WkKnUnlocker_";

        private sealed class GroupClaim
        {
            internal ResearchCatalogEntry Entry;
            internal string ModName;
            internal int LineNumber;
            internal int LoadIndex;
            internal bool IsBuiltIn;

            internal string Source
            {
                get
                {
                    return IsBuiltIn
                        ? "Working Knowledge built-in catalog"
                        : ModName + " (load position " + (LoadIndex + 1) + ") line " + LineNumber;
                }
            }
        }

        private sealed class MappingClaim
        {
            internal string BlockKey;
            internal string ResearchId;
            internal string ModName;
            internal int LineNumber;
            internal int LoadIndex;
            internal bool IsOverride;

            internal string Source
            {
                get { return ModName + " (load position " + (LoadIndex + 1) + ") line " + LineNumber; }
            }
        }

        internal static WorkingKnowledgeLayerAudit LoadMappings()
        {
            var audit = new WorkingKnowledgeLayerAudit();
            if (MyAPIGateway.Session == null || MyAPIGateway.Utilities == null || MyAPIGateway.Session.Mods == null)
                return audit;

            var groupClaims = BuildBuiltInGroupClaims();
            var mappingClaims = new List<MappingClaim>();
            for (var i = 0; i < MyAPIGateway.Session.Mods.Count; i++)
            {
                var mod = MyAPIGateway.Session.Mods[i];
                var hasGroups = MyAPIGateway.Utilities.FileExistsInModLocation(GroupFilePath, mod);
                var hasMappings = MyAPIGateway.Utilities.FileExistsInModLocation(MappingFilePath, mod);
                if (!hasGroups && !hasMappings)
                    continue;

                audit.LayerCount++;
                if (hasGroups)
                    LoadGroupsFromMod(mod, i, groupClaims, audit);
                if (hasMappings)
                    LoadMappingsFromMod(mod, i, mappingClaims, audit);
            }

            ResolveGroups(groupClaims, audit);
            ResolveMappings(mappingClaims, ResearchCatalog.BuildLookupByBlockKey(), audit);
            audit.SortMessages();
            return audit;
        }

        private static List<GroupClaim> BuildBuiltInGroupClaims()
        {
            var claims = new List<GroupClaim>();
            var line = 0;
            foreach (var pair in ResearchCatalog.BuildLookupByResearchId())
            {
                claims.Add(new GroupClaim
                {
                    Entry = pair.Value,
                    ModName = "Working Knowledge",
                    LineNumber = ++line,
                    LoadIndex = -1,
                    IsBuiltIn = true,
                });
            }
            return claims;
        }

        private static void LoadGroupsFromMod(
            MyObjectBuilder_Checkpoint.ModItem mod,
            int loadIndex,
            List<GroupClaim> claims,
            WorkingKnowledgeLayerAudit audit)
        {
            var modName = GetModName(mod);
            try
            {
                var rows = new List<string>();
                using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(GroupFilePath, mod))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        rows.Add(line);
                }

                var versionLine = FindFirstContentLine(rows);
                if (versionLine < 0 || !IsSupportedVersionLine(StripComment(rows[versionLine]).Trim()))
                {
                    audit.AddIssue(
                        "Ignored group declarations from " + modName +
                        " because schematic_groups.txt must begin with 'version = " + SupportedGroupFormatVersion + "'.");
                    return;
                }

                for (var i = versionLine + 1; i < rows.Count; i++)
                    TryLoadGroupLine(modName, loadIndex, i + 1, rows[i], claims, audit);
            }
            catch (Exception exception)
            {
                audit.AddIssue("Could not read group declarations from " + modName + ": " + exception.Message);
            }
        }

        private static void TryLoadGroupLine(
            string modName,
            int loadIndex,
            int lineNumber,
            string rawLine,
            List<GroupClaim> claims,
            WorkingKnowledgeLayerAudit audit)
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                return;

            var fields = line.Split('|');
            if (fields.Length != 5 && fields.Length != 6)
            {
                AddInvalidGroupIssue(
                    audit,
                    modName,
                    loadIndex,
                    lineNumber,
                    "expected id | display name | tier | research group subtype | unlocker subtype [| description]");
                return;
            }

            for (var i = 0; i < fields.Length; i++)
                fields[i] = fields[i].Trim();

            SchematicTier tier;
            if (!IsValidResearchId(fields[0]))
            {
                AddInvalidGroupIssue(audit, modName, loadIndex, lineNumber, "invalid stable schematic id '" + fields[0] + "'");
                return;
            }
            if (fields[1].Length == 0)
            {
                AddInvalidGroupIssue(audit, modName, loadIndex, lineNumber, "display name is required");
                return;
            }
            if (!TryParseTier(fields[2], out tier))
            {
                AddInvalidGroupIssue(audit, modName, loadIndex, lineNumber, "invalid tier '" + fields[2] + "'");
                return;
            }
            if (!IsValidDefinitionSubtype(fields[3]))
            {
                AddInvalidGroupIssue(audit, modName, loadIndex, lineNumber, "invalid research group subtype '" + fields[3] + "'");
                return;
            }
            if (!IsValidDefinitionSubtype(fields[4]) ||
                !fields[4].StartsWith(UnlockerSubtypePrefix, StringComparison.OrdinalIgnoreCase))
            {
                AddInvalidGroupIssue(
                    audit,
                    modName,
                    loadIndex,
                    lineNumber,
                    "unlocker subtype must use the " + UnlockerSubtypePrefix + " prefix and contain only letters, digits, or underscores");
                return;
            }

            claims.Add(new GroupClaim
            {
                Entry = new ResearchCatalogEntry(
                    string.Empty,
                    fields[0],
                    fields[1],
                    fields.Length == 6 ? fields[5] : string.Empty,
                    fields[3],
                    fields[4],
                    tier),
                ModName = modName,
                LineNumber = lineNumber,
                LoadIndex = loadIndex,
                IsBuiltIn = false,
            });
            audit.DeclaredGroupCount++;
        }

        private static void ResolveGroups(List<GroupClaim> claims, WorkingKnowledgeLayerAudit audit)
        {
            claims.Sort(CompareGroupClaims);
            var historyById = new Dictionary<string, List<GroupClaim>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < claims.Count; i++)
            {
                AddClaim(historyById, claims[i].Entry.ResearchId, claims[i]);
            }

            var winners = new List<GroupClaim>();
            foreach (var history in historyById)
            {
                var winner = FindLastValidGroupClaim(history.Key, history.Value, audit);
                if (winner == null)
                    continue;

                winners.Add(winner);
                ReportGroupHistory(history.Key, history.Value, winner, audit);
            }
            winners.Sort(CompareGroupClaims);
            var groupSubtypeOwner = new Dictionary<string, GroupClaim>(StringComparer.OrdinalIgnoreCase);
            var unlockerSubtypeOwner = new Dictionary<string, GroupClaim>(StringComparer.OrdinalIgnoreCase);
            var itemSubtypeOwner = new Dictionary<string, GroupClaim>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < winners.Count; i++)
            {
                var winner = winners[i];
                groupSubtypeOwner[winner.Entry.GroupSubtype] = winner;
                unlockerSubtypeOwner[winner.Entry.UnlockerSubtype] = winner;
                itemSubtypeOwner[GetSafeSubtypeToken(winner.Entry.ResearchId)] = winner;
            }

            for (var i = 0; i < winners.Count; i++)
            {
                var winner = winners[i];
                if (!winner.IsBuiltIn)
                {
                    audit.Groups[winner.Entry.ResearchId] = new WorkingKnowledgeLayerGroup(
                        winner.Entry,
                        winner.ModName,
                        winner.LineNumber,
                        winner.LoadIndex);
                }

                GroupClaim groupOwner;
                GroupClaim unlockerOwner;
                GroupClaim itemOwner;
                groupSubtypeOwner.TryGetValue(winner.Entry.GroupSubtype, out groupOwner);
                unlockerSubtypeOwner.TryGetValue(winner.Entry.UnlockerSubtype, out unlockerOwner);
                itemSubtypeOwner.TryGetValue(GetSafeSubtypeToken(winner.Entry.ResearchId), out itemOwner);
                if (groupOwner != winner || unlockerOwner != winner || itemOwner != winner)
                {
                    audit.AddIssue(
                        "Group '" + winner.Entry.ResearchId + "' from " + winner.Source +
                        " is inactive because a later group owns one of its research group, unlocker, or exact-schematic definition IDs. " +
                        "Winning owners: " + FormatGroupOwners(groupOwner, unlockerOwner, itemOwner) + ".");
                    continue;
                }

                audit.ResolvedGroups[winner.Entry.ResearchId] = winner.Entry;
                if (!winner.IsBuiltIn)
                    audit.ActiveGroupCount++;

                var schematicId = new MyDefinitionId(
                    typeof(MyObjectBuilder_ConsumableItem),
                    "WkKnSchematic_" + GetSafeSubtypeToken(winner.Entry.ResearchId));
                if (MyDefinitionManager.Static.GetPhysicalItemDefinition(schematicId) == null)
                {
                    audit.AddIssue(
                        "Group '" + winner.Entry.ResearchId + "' from " + winner.Source +
                        " has no exact Data Schematic item definition; Data Fragment and block-work rewards still work.");
                }
            }
        }

        private static void ReportGroupHistory(
            string researchId,
            List<GroupClaim> history,
            GroupClaim winner,
            WorkingKnowledgeLayerAudit audit)
        {
            if (history.Count <= 1)
                return;

            var externalCount = 0;
            var wiringChanged = false;
            for (var i = 0; i < history.Count; i++)
            {
                if (!history[i].IsBuiltIn)
                    externalCount++;
                if (i > 0 && !HasSameWiring(history[i - 1].Entry, history[i].Entry))
                    wiringChanged = true;
            }

            if (externalCount == 0)
                return;
            if (history[0].IsBuiltIn)
                audit.RedefinedGroupCount++;

            var message =
                "Group '" + researchId + "' was declared " + history.Count + " times; " +
                winner.Source + " is the last valid declaration and wins. History: " + FormatGroupHistory(history) + ".";
            audit.AddIssue(message + (wiringChanged ? " Vanilla research wiring also changed." : string.Empty));
        }

        private static GroupClaim FindLastValidGroupClaim(
            string researchId,
            List<GroupClaim> history,
            WorkingKnowledgeLayerAudit audit)
        {
            for (var i = history.Count - 1; i >= 0; i--)
            {
                var claim = history[i];
                var unlockerId = new MyDefinitionId(typeof(MyObjectBuilder_CubeBlock), claim.Entry.UnlockerSubtype);
                var unlocker = MyDefinitionManager.Static.GetCubeBlockDefinition(unlockerId);
                var researchGroup = MyDefinitionManager.Static.GetResearchGroup(claim.Entry.GroupSubtype);
                if (unlocker != null && researchGroup != null)
                    return claim;

                audit.AddIssue(
                    "Skipped group '" + researchId + "' from " + claim.Source +
                    " because its unlocker block or research group definition is missing.");
            }
            return null;
        }

        private static void LoadMappingsFromMod(
            MyObjectBuilder_Checkpoint.ModItem mod,
            int loadIndex,
            List<MappingClaim> claims,
            WorkingKnowledgeLayerAudit audit)
        {
            try
            {
                using (var reader = MyAPIGateway.Utilities.ReadFileInModLocation(MappingFilePath, mod))
                {
                    var lineNumber = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        TryLoadMappingLine(GetModName(mod), loadIndex, lineNumber, line, claims, audit);
                    }
                }
            }
            catch (Exception exception)
            {
                audit.AddIssue("Could not read mappings from " + GetModName(mod) + ": " + exception.Message);
            }
        }

        private static void TryLoadMappingLine(
            string modName,
            int loadIndex,
            int lineNumber,
            string rawLine,
            List<MappingClaim> claims,
            WorkingKnowledgeLayerAudit audit)
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                return;

            var isOverride = line.StartsWith("override ", StringComparison.OrdinalIgnoreCase);
            if (isOverride)
                line = line.Substring("override ".Length).Trim();

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || equalsIndex >= line.Length - 1)
            {
                AddInvalidMappingIssue(audit, modName, loadIndex, lineNumber, "expected [override] Type/Subtype = schematic.id");
                return;
            }

            var blockKey = line.Substring(0, equalsIndex).Trim();
            var researchId = line.Substring(equalsIndex + 1).Trim();
            var slashIndex = blockKey.IndexOf('/');
            if (blockKey.Length == 0 || researchId.Length == 0 || slashIndex <= 0 ||
                slashIndex != blockKey.LastIndexOf('/') || slashIndex >= blockKey.Length - 1)
            {
                AddInvalidMappingIssue(audit, modName, loadIndex, lineNumber, "expected one complete Type/Subtype before '='");
                return;
            }

            claims.Add(new MappingClaim
            {
                BlockKey = blockKey,
                ResearchId = researchId,
                ModName = modName,
                LineNumber = lineNumber,
                LoadIndex = loadIndex,
                IsOverride = isOverride,
            });
            audit.MappingCount++;
            if (isOverride)
                audit.OverrideCount++;
        }

        private static void ResolveMappings(
            List<MappingClaim> claims,
            Dictionary<string, ResearchCatalogEntry> builtInBlocks,
            WorkingKnowledgeLayerAudit audit)
        {
            claims.Sort(CompareMappingClaims);
            var claimsByBlock = new Dictionary<string, List<MappingClaim>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < claims.Count; i++)
                AddClaim(claimsByBlock, claims[i].BlockKey, claims[i]);

            foreach (var pair in claimsByBlock)
            {
                var validClaims = new List<MappingClaim>();
                for (var i = 0; i < pair.Value.Count; i++)
                {
                    if (audit.ResolvedGroups.ContainsKey(pair.Value[i].ResearchId))
                    {
                        validClaims.Add(pair.Value[i]);
                    }
                    else
                    {
                        audit.SkippedMappingCount++;
                        audit.AddIssue(
                            "Skipped " + pair.Value[i].Source + " mapping for " + pair.Key +
                            " because schematic group '" + pair.Value[i].ResearchId + "' is unknown or inactive.");
                    }
                }

                if (validClaims.Count == 0)
                    continue;

                var winner = validClaims[validClaims.Count - 1];
                ResearchCatalogEntry metadata;
                audit.ResolvedGroups.TryGetValue(winner.ResearchId, out metadata);
                var entry = new ResearchCatalogEntry(
                    winner.BlockKey,
                    metadata.ResearchId,
                    metadata.DisplayName,
                    metadata.Description,
                    metadata.GroupSubtype,
                    metadata.UnlockerSubtype,
                    metadata.Tier);
                audit.Mappings[winner.BlockKey] = new WorkingKnowledgeLayerMapping(
                    entry,
                    winner.ModName,
                    winner.LineNumber,
                    winner.LoadIndex,
                    winner.IsOverride);

                if (validClaims.Count > 1)
                {
                    audit.ConflictingBlockCount++;
                    audit.AddIssue(
                        winner.BlockKey + " was assigned by " + validClaims.Count + " layer mappings; " +
                        winner.Source + " -> '" + winner.ResearchId + "' wins. History: " +
                        FormatMappingHistory(validClaims) + ".");
                }

                ResearchCatalogEntry builtIn;
                if (builtInBlocks.TryGetValue(winner.BlockKey, out builtIn) &&
                    !string.Equals(builtIn.ResearchId, winner.ResearchId, StringComparison.OrdinalIgnoreCase))
                {
                    audit.BuiltInReplacementCount++;
                    audit.AddNotice(
                        winner.BlockKey + " moved from built-in group '" + builtIn.ResearchId +
                        "' to '" + winner.ResearchId + "' by " + winner.Source + ".");
                }
            }
        }

        private static int CompareGroupClaims(GroupClaim left, GroupClaim right)
        {
            var loadCompare = left.LoadIndex.CompareTo(right.LoadIndex);
            if (loadCompare != 0)
                return loadCompare;
            var lineCompare = left.LineNumber.CompareTo(right.LineNumber);
            if (lineCompare != 0)
                return lineCompare;
            return string.Compare(left.Entry.ResearchId, right.Entry.ResearchId, StringComparison.OrdinalIgnoreCase);
        }

        private static int CompareMappingClaims(MappingClaim left, MappingClaim right)
        {
            var loadCompare = left.LoadIndex.CompareTo(right.LoadIndex);
            if (loadCompare != 0)
                return loadCompare;
            return left.LineNumber.CompareTo(right.LineNumber);
        }

        private static bool HasSameWiring(ResearchCatalogEntry left, ResearchCatalogEntry right)
        {
            return string.Equals(left.GroupSubtype, right.GroupSubtype, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(left.UnlockerSubtype, right.UnlockerSubtype, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatGroupHistory(List<GroupClaim> history)
        {
            var values = new List<string>();
            for (var i = 0; i < history.Count; i++)
                values.Add(history[i].Source + " -> '" + history[i].Entry.DisplayName + "'");
            return string.Join("; ", values.ToArray());
        }

        private static string FormatMappingHistory(List<MappingClaim> history)
        {
            var values = new List<string>();
            for (var i = 0; i < history.Count; i++)
                values.Add(history[i].Source + " -> '" + history[i].ResearchId + "'");
            return string.Join("; ", values.ToArray());
        }

        private static string FormatGroupOwners(GroupClaim groupOwner, GroupClaim unlockerOwner, GroupClaim itemOwner)
        {
            var values = new List<string>();
            AddUniqueOwner(values, groupOwner);
            AddUniqueOwner(values, unlockerOwner);
            AddUniqueOwner(values, itemOwner);
            return string.Join("; ", values.ToArray());
        }

        private static void AddUniqueOwner(List<string> values, GroupClaim owner)
        {
            if (owner == null)
                return;
            var value = "'" + owner.Entry.ResearchId + "' from " + owner.Source;
            if (!values.Contains(value))
                values.Add(value);
        }

        private static string GetSafeSubtypeToken(string value)
        {
            var result = new System.Text.StringBuilder();
            var lastWasSeparator = false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsLetterOrDigit(c))
                {
                    result.Append(c);
                    lastWasSeparator = false;
                }
                else if (result.Length > 0 && !lastWasSeparator)
                {
                    result.Append('_');
                    lastWasSeparator = true;
                }
            }
            while (result.Length > 0 && result[result.Length - 1] == '_')
                result.Length--;
            return result.ToString();
        }

        private static bool IsValidResearchId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !char.IsLetterOrDigit(value[0]) || !char.IsLetterOrDigit(value[value.Length - 1]))
                return false;
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                if (!char.IsLetterOrDigit(c) && c != '.' && c != '_' && c != '-')
                    return false;
            }
            return true;
        }

        private static bool IsValidDefinitionSubtype(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsLetterOrDigit(value[i]) && value[i] != '_')
                    return false;
            }
            return true;
        }

        private static bool TryParseTier(string value, out SchematicTier tier)
        {
            tier = SchematicTier.None;
            if (string.Equals(value, "None", StringComparison.OrdinalIgnoreCase))
                tier = SchematicTier.None;
            else if (string.Equals(value, "Common", StringComparison.OrdinalIgnoreCase))
                tier = SchematicTier.Common;
            else if (string.Equals(value, "Uncommon", StringComparison.OrdinalIgnoreCase))
                tier = SchematicTier.Uncommon;
            else if (string.Equals(value, "Rare", StringComparison.OrdinalIgnoreCase))
                tier = SchematicTier.Rare;
            else if (string.Equals(value, "Prototech", StringComparison.OrdinalIgnoreCase))
                tier = SchematicTier.Prototech;
            else
                return false;
            return true;
        }

        private static int FindFirstContentLine(List<string> lines)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                if (StripComment(lines[i]).Trim().Length > 0)
                    return i;
            }
            return -1;
        }

        private static bool IsSupportedVersionLine(string line)
        {
            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || equalsIndex >= line.Length - 1)
                return false;
            return string.Equals(line.Substring(0, equalsIndex).Trim(), "version", StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(line.Substring(equalsIndex + 1).Trim(), SupportedGroupFormatVersion, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddClaim<T>(Dictionary<string, List<T>> claimsByKey, string key, T claim) where T : class
        {
            List<T> values;
            if (!claimsByKey.TryGetValue(key, out values))
            {
                values = new List<T>();
                claimsByKey.Add(key, values);
            }
            values.Add(claim);
        }

        private static string StripComment(string line)
        {
            if (line == null)
                return string.Empty;
            var commentIndex = line.IndexOf('#');
            return commentIndex < 0 ? line : line.Substring(0, commentIndex);
        }

        private static void AddInvalidGroupIssue(
            WorkingKnowledgeLayerAudit audit,
            string modName,
            int loadIndex,
            int lineNumber,
            string reason)
        {
            audit.AddIssue(
                "Ignored group declaration in " + modName + " (load position " + (loadIndex + 1) + ") line " +
                lineNumber + ": " + reason + ".");
        }

        private static void AddInvalidMappingIssue(
            WorkingKnowledgeLayerAudit audit,
            string modName,
            int loadIndex,
            int lineNumber,
            string reason)
        {
            audit.AddIssue(
                "Ignored mapping in " + modName + " (load position " + (loadIndex + 1) + ") line " +
                lineNumber + ": " + reason + ".");
        }

        private static string GetModName(MyObjectBuilder_Checkpoint.ModItem mod)
        {
            if (!string.IsNullOrWhiteSpace(mod.FriendlyName))
                return mod.FriendlyName;
            return string.IsNullOrWhiteSpace(mod.Name) ? "unknown mod" : mod.Name;
        }
    }
}
