using System;
using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;

namespace WkKn
{
    internal static class WorkingKnowledgeLayerMappingLoader
    {
        private const string MappingFilePath = "Data/WorkingKnowledge/block_mappings.txt";

        internal static WorkingKnowledgeLayerAudit LoadMappings()
        {
            var audit = new WorkingKnowledgeLayerAudit();

            if (MyAPIGateway.Session == null || MyAPIGateway.Utilities == null || MyAPIGateway.Session.Mods == null)
                return audit;

            for (var i = 0; i < MyAPIGateway.Session.Mods.Count; i++)
            {
                var mod = MyAPIGateway.Session.Mods[i];
                if (!MyAPIGateway.Utilities.FileExistsInModLocation(MappingFilePath, mod))
                    continue;

                audit.LayerCount++;
                LoadMappingsFromMod(mod, audit);
            }

            return audit;
        }

        private static void LoadMappingsFromMod(MyObjectBuilder_Checkpoint.ModItem mod, WorkingKnowledgeLayerAudit audit)
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
                        TryLoadMappingLine(mod, lineNumber, line, audit);
                    }
                }
            }
            catch (Exception exception)
            {
                audit.AddIssue("Could not read mappings from " + GetModName(mod) + ": " + exception.Message);
            }
        }

        private static void TryLoadMappingLine(MyObjectBuilder_Checkpoint.ModItem mod, int lineNumber, string rawLine, WorkingKnowledgeLayerAudit audit)
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                return;

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || equalsIndex >= line.Length - 1)
            {
                AddInvalidLineIssue(audit, mod, lineNumber, "expected Type/Subtype = schematic.id");
                return;
            }

            var blockKey = line.Substring(0, equalsIndex).Trim();
            var researchId = line.Substring(equalsIndex + 1).Trim();
            var slashIndex = blockKey.IndexOf('/');
            if (blockKey.Length == 0 || researchId.Length == 0 || slashIndex <= 0 || slashIndex != blockKey.LastIndexOf('/') || slashIndex >= blockKey.Length - 1)
            {
                AddInvalidLineIssue(audit, mod, lineNumber, "expected one complete Type/Subtype before '='");
                return;
            }

            ResearchCatalogEntry entry;
            if (!TryCreateCatalogEntry(blockKey, researchId, out entry))
            {
                AddInvalidLineIssue(audit, mod, lineNumber, "unknown Working Knowledge schematic id '" + researchId + "'");
                return;
            }

            var modName = GetModName(mod);
            WorkingKnowledgeLayerMapping previous;
            if (audit.Mappings.TryGetValue(blockKey, out previous))
            {
                audit.AddIssue(
                    "Duplicate mapping for " + blockKey + " in " + modName + " line " + lineNumber +
                    "; it replaces " + previous.Entry.ResearchId + " from " + previous.Source + ".");
            }

            audit.Mappings[blockKey] = new WorkingKnowledgeLayerMapping(entry, modName, lineNumber);
        }

        private static bool TryCreateCatalogEntry(string blockKey, string researchId, out ResearchCatalogEntry entry)
        {
            foreach (var candidate in ResearchCatalog.Entries)
            {
                if (!string.Equals(candidate.ResearchId, researchId, StringComparison.OrdinalIgnoreCase))
                    continue;

                entry = new ResearchCatalogEntry(
                    blockKey,
                    candidate.ResearchId,
                    candidate.DisplayName,
                    candidate.GroupSubtype,
                    candidate.UnlockerSubtype,
                    candidate.Tier);
                return true;
            }

            entry = default(ResearchCatalogEntry);
            return false;
        }

        private static string StripComment(string line)
        {
            if (line == null)
                return string.Empty;

            var commentIndex = line.IndexOf('#');
            return commentIndex < 0 ? line : line.Substring(0, commentIndex);
        }

        private static void AddInvalidLineIssue(WorkingKnowledgeLayerAudit audit, MyObjectBuilder_Checkpoint.ModItem mod, int lineNumber, string reason)
        {
            audit.AddIssue("Ignored mapping in " + GetModName(mod) + " line " + lineNumber + ": " + reason + ".");
        }

        private static string GetModName(MyObjectBuilder_Checkpoint.ModItem mod)
        {
            if (!string.IsNullOrWhiteSpace(mod.FriendlyName))
                return mod.FriendlyName;

            return string.IsNullOrWhiteSpace(mod.Name) ? "unknown mod" : mod.Name;
        }
    }
}
