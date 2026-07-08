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

        internal static Dictionary<string, ResearchCatalogEntry> LoadMappings()
        {
            var mappings = new Dictionary<string, ResearchCatalogEntry>(StringComparer.OrdinalIgnoreCase);

            if (MyAPIGateway.Session == null || MyAPIGateway.Utilities == null || MyAPIGateway.Session.Mods == null)
                return mappings;

            for (var i = 0; i < MyAPIGateway.Session.Mods.Count; i++)
            {
                var mod = MyAPIGateway.Session.Mods[i];
                if (!MyAPIGateway.Utilities.FileExistsInModLocation(MappingFilePath, mod))
                    continue;

                LoadMappingsFromMod(mod, mappings);
            }

            return mappings;
        }

        private static void LoadMappingsFromMod(MyObjectBuilder_Checkpoint.ModItem mod, Dictionary<string, ResearchCatalogEntry> mappings)
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
                        TryLoadMappingLine(mod, lineNumber, line, mappings);
                    }
                }
            }
            catch (Exception exception)
            {
                MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " failed to read WK layer mappings from " + GetModName(mod) + ": " + exception.Message);
            }
        }

        private static void TryLoadMappingLine(MyObjectBuilder_Checkpoint.ModItem mod, int lineNumber, string rawLine, Dictionary<string, ResearchCatalogEntry> mappings)
        {
            var line = StripComment(rawLine).Trim();
            if (line.Length == 0)
                return;

            var equalsIndex = line.IndexOf('=');
            if (equalsIndex <= 0 || equalsIndex >= line.Length - 1)
            {
                LogInvalidLine(mod, lineNumber, "expected Type/Subtype = schematic.id");
                return;
            }

            var blockKey = line.Substring(0, equalsIndex).Trim();
            var researchId = line.Substring(equalsIndex + 1).Trim();
            if (blockKey.Length == 0 || researchId.Length == 0 || blockKey.IndexOf('/') <= 0)
            {
                LogInvalidLine(mod, lineNumber, "expected Type/Subtype = schematic.id");
                return;
            }

            ResearchCatalogEntry entry;
            if (!TryCreateCatalogEntry(blockKey, researchId, out entry))
            {
                LogInvalidLine(mod, lineNumber, "unknown Working Knowledge schematic id '" + researchId + "'");
                return;
            }

            mappings[blockKey] = entry;
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

        private static void LogInvalidLine(MyObjectBuilder_Checkpoint.ModItem mod, int lineNumber, string reason)
        {
            MyLog.Default.WriteLineAndConsole(WkKnSession.LogPrefix + " ignored WK layer mapping in " + GetModName(mod) + " line " + lineNumber + ": " + reason);
        }

        private static string GetModName(MyObjectBuilder_Checkpoint.ModItem mod)
        {
            if (!string.IsNullOrWhiteSpace(mod.FriendlyName))
                return mod.FriendlyName;

            return string.IsNullOrWhiteSpace(mod.Name) ? "unknown mod" : mod.Name;
        }
    }
}
