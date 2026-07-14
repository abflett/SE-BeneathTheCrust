using System;
using System.Collections.Generic;
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

            internal string Source
            {
                get { return ModName + " line " + LineNumber; }
            }
        }

        private sealed class MappingClaim
        {
            internal string BlockKey;
            internal string ResearchId;
            internal string ModName;
            internal int LineNumber;
            internal bool IsOverride;

            internal string Source
            {
                get { return ModName + " line " + LineNumber; }
            }
        }

        internal static WorkingKnowledgeLayerAudit LoadMappings()
        {
            var audit = new WorkingKnowledgeLayerAudit();
            if (MyAPIGateway.Session == null || MyAPIGateway.Utilities == null || MyAPIGateway.Session.Mods == null)
                return audit;

            var groupClaims = new List<GroupClaim>();
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
                    LoadGroupsFromMod(mod, groupClaims, audit);
                if (hasMappings)
                    LoadMappingsFromMod(mod, mappingClaims, audit);
            }

            var metadataByResearchId = ResearchCatalog.BuildLookupByResearchId();
            ResolveGroups(groupClaims, metadataByResearchId, audit);
            foreach (var group in audit.Groups)
                metadataByResearchId.Add(group.Key, group.Value.Entry);

            ResolveMappings(mappingClaims, metadataByResearchId, ResearchCatalog.BuildLookupByBlockKey(), audit);
            audit.SortIssues();
            return audit;
        }

        private static void LoadGroupsFromMod(
            MyObjectBuilder_Checkpoint.ModItem mod,
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
                        "Ignored custom groups from " + modName +
                        " because schematic_groups.txt must begin with 'version = " + SupportedGroupFormatVersion + "'.");
                    return;
                }

                for (var i = versionLine + 1; i < rows.Count; i++)
                    TryLoadGroupLine(modName, i + 1, rows[i], claims, audit);
            }
            catch (Exception exception)
            {
                audit.AddIssue("Could not read custom groups from " + modName + ": " + exception.Message);
            }
        }

        private static void TryLoadGroupLine(
            string modName,
            int lineNumber,
            string rawLine,
            List<GroupClaim> claims,
            WorkingKnowledgeLayerAudit audit)
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                return;

            var fields = line.Split('|');
            if (fields.Length != 5)
            {
                AddInvalidGroupIssue(audit, modName, lineNumber, "expected id | display name | tier | research group subtype | unlocker subtype");
                return;
            }

            for (var i = 0; i < fields.Length; i++)
                fields[i] = fields[i].Trim();

            SchematicTier tier;
            if (!IsValidResearchId(fields[0]))
            {
                AddInvalidGroupIssue(audit, modName, lineNumber, "invalid stable schematic id '" + fields[0] + "'");
                return;
            }
            if (fields[1].Length == 0)
            {
                AddInvalidGroupIssue(audit, modName, lineNumber, "display name is required");
                return;
            }
            if (!TryParseTier(fields[2], out tier))
            {
                AddInvalidGroupIssue(audit, modName, lineNumber, "invalid tier '" + fields[2] + "'");
                return;
            }
            if (!IsValidDefinitionSubtype(fields[3]))
            {
                AddInvalidGroupIssue(audit, modName, lineNumber, "invalid research group subtype '" + fields[3] + "'");
                return;
            }
            if (!IsValidDefinitionSubtype(fields[4]) ||
                !fields[4].StartsWith(UnlockerSubtypePrefix, StringComparison.OrdinalIgnoreCase))
            {
                AddInvalidGroupIssue(
                    audit,
                    modName,
                    lineNumber,
                    "unlocker subtype must use the " + UnlockerSubtypePrefix + " prefix and contain only letters, digits, or underscores");
                return;
            }

            claims.Add(new GroupClaim
            {
                Entry = new ResearchCatalogEntry(string.Empty, fields[0], fields[1], fields[3], fields[4], tier),
                ModName = modName,
                LineNumber = lineNumber,
            });
        }

        private static void ResolveGroups(
            List<GroupClaim> claims,
            Dictionary<string, ResearchCatalogEntry> builtInGroups,
            WorkingKnowledgeLayerAudit audit)
        {
            var claimsById = new Dictionary<string, List<GroupClaim>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < claims.Count; i++)
                AddClaim(claimsById, claims[i].Entry.ResearchId, claims[i]);

            var candidates = new List<GroupClaim>();
            foreach (var pair in claimsById)
            {
                if (builtInGroups.ContainsKey(pair.Key))
                {
                    audit.AddIssue("Ignored custom group '" + pair.Key + "' because it conflicts with a built-in schematic ID.");
                    continue;
                }
                if (pair.Value.Count != 1)
                {
                    audit.AddIssue("Ignored duplicate custom group ID '" + pair.Key + "' from " + FormatSources(pair.Value) + ".");
                    continue;
                }

                candidates.Add(pair.Value[0]);
            }

            var invalidIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var builtInEntries = new List<ResearchCatalogEntry>(builtInGroups.Values);
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                for (var j = 0; j < builtInEntries.Count; j++)
                {
                    if (HasDefinitionCollision(candidate.Entry, builtInEntries[j]))
                    {
                        invalidIds.Add(candidate.Entry.ResearchId);
                        audit.AddIssue(
                            "Ignored custom group '" + candidate.Entry.ResearchId + "' from " + candidate.Source +
                            " because its generated definition IDs collide with built-in group '" + builtInEntries[j].ResearchId + "'.");
                        break;
                    }
                }

                for (var j = i + 1; j < candidates.Count; j++)
                {
                    if (!HasDefinitionCollision(candidate.Entry, candidates[j].Entry))
                        continue;

                    invalidIds.Add(candidate.Entry.ResearchId);
                    invalidIds.Add(candidates[j].Entry.ResearchId);
                    audit.AddIssue(
                        "Ignored custom groups '" + candidate.Entry.ResearchId + "' and '" + candidates[j].Entry.ResearchId +
                        "' because their generated definition IDs collide (" + candidate.Source + "; " + candidates[j].Source + ").");
                }
            }

            candidates.Sort((left, right) => string.Compare(left.Entry.ResearchId, right.Entry.ResearchId, StringComparison.OrdinalIgnoreCase));
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (invalidIds.Contains(candidate.Entry.ResearchId))
                    continue;

                audit.Groups.Add(
                    candidate.Entry.ResearchId,
                    new WorkingKnowledgeLayerGroup(candidate.Entry, candidate.ModName, candidate.LineNumber));
            }
        }

        private static void LoadMappingsFromMod(
            MyObjectBuilder_Checkpoint.ModItem mod,
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
                        TryLoadMappingLine(GetModName(mod), lineNumber, line, claims, audit);
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
                AddInvalidMappingIssue(audit, modName, lineNumber, "expected [override] Type/Subtype = schematic.id");
                return;
            }

            var blockKey = line.Substring(0, equalsIndex).Trim();
            var researchId = line.Substring(equalsIndex + 1).Trim();
            var slashIndex = blockKey.IndexOf('/');
            if (blockKey.Length == 0 || researchId.Length == 0 || slashIndex <= 0 ||
                slashIndex != blockKey.LastIndexOf('/') || slashIndex >= blockKey.Length - 1)
            {
                AddInvalidMappingIssue(audit, modName, lineNumber, "expected one complete Type/Subtype before '='");
                return;
            }

            claims.Add(new MappingClaim
            {
                BlockKey = blockKey,
                ResearchId = researchId,
                ModName = modName,
                LineNumber = lineNumber,
                IsOverride = isOverride,
            });
            audit.MappingCount++;
            if (isOverride)
                audit.OverrideCount++;
        }

        private static void ResolveMappings(
            List<MappingClaim> claims,
            Dictionary<string, ResearchCatalogEntry> groupsById,
            Dictionary<string, ResearchCatalogEntry> builtInBlocks,
            WorkingKnowledgeLayerAudit audit)
        {
            var claimsByBlock = new Dictionary<string, List<MappingClaim>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < claims.Count; i++)
                AddClaim(claimsByBlock, claims[i].BlockKey, claims[i]);

            foreach (var pair in claimsByBlock)
            {
                var blockClaims = pair.Value;
                if (blockClaims.Count != 1)
                {
                    audit.AddIssue("Ignored conflicting mappings for " + pair.Key + " from " + FormatSources(blockClaims) + ".");
                    continue;
                }

                var claim = blockClaims[0];
                ResearchCatalogEntry metadata;
                if (!groupsById.TryGetValue(claim.ResearchId, out metadata))
                {
                    AddInvalidMappingIssue(audit, claim.ModName, claim.LineNumber, "unknown Working Knowledge schematic id '" + claim.ResearchId + "'");
                    continue;
                }

                if (builtInBlocks.ContainsKey(claim.BlockKey) && !claim.IsOverride)
                {
                    audit.AddIssue(
                        "Ignored " + claim.Source + " because " + claim.BlockKey +
                        " has a built-in mapping; use the explicit 'override' prefix to remap it.");
                    continue;
                }

                var entry = new ResearchCatalogEntry(
                    claim.BlockKey,
                    metadata.ResearchId,
                    metadata.DisplayName,
                    metadata.GroupSubtype,
                    metadata.UnlockerSubtype,
                    metadata.Tier);
                audit.Mappings.Add(claim.BlockKey, new WorkingKnowledgeLayerMapping(entry, claim.ModName, claim.LineNumber, claim.IsOverride));
            }
        }

        private static bool HasDefinitionCollision(ResearchCatalogEntry left, ResearchCatalogEntry right)
        {
            return string.Equals(left.GroupSubtype, right.GroupSubtype, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(left.UnlockerSubtype, right.UnlockerSubtype, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(GetSafeSubtypeToken(left.ResearchId), GetSafeSubtypeToken(right.ResearchId), StringComparison.OrdinalIgnoreCase);
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

        private static string FormatSources<T>(List<T> claims) where T : class
        {
            var sources = new List<string>();
            for (var i = 0; i < claims.Count; i++)
            {
                var group = claims[i] as GroupClaim;
                var mapping = claims[i] as MappingClaim;
                sources.Add(group != null ? group.Source : mapping.Source);
            }
            sources.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(", ", sources.ToArray());
        }

        private static string StripComment(string line)
        {
            if (line == null)
                return string.Empty;
            var commentIndex = line.IndexOf('#');
            return commentIndex < 0 ? line : line.Substring(0, commentIndex);
        }

        private static void AddInvalidGroupIssue(WorkingKnowledgeLayerAudit audit, string modName, int lineNumber, string reason)
        {
            audit.AddIssue("Ignored custom group in " + modName + " line " + lineNumber + ": " + reason + ".");
        }

        private static void AddInvalidMappingIssue(WorkingKnowledgeLayerAudit audit, string modName, int lineNumber, string reason)
        {
            audit.AddIssue("Ignored mapping in " + modName + " line " + lineNumber + ": " + reason + ".");
        }

        private static string GetModName(MyObjectBuilder_Checkpoint.ModItem mod)
        {
            if (!string.IsNullOrWhiteSpace(mod.FriendlyName))
                return mod.FriendlyName;
            return string.IsNullOrWhiteSpace(mod.Name) ? "unknown mod" : mod.Name;
        }
    }
}
