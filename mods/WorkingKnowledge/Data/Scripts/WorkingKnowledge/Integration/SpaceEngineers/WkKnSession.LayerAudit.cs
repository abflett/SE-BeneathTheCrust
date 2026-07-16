using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int LayerAuditWarningEntryLimit = 12;
        private const int LayerAuditMoveEntryLimitPerDestination = 40;

        private void PublishLayerAudit()
        {
            MyLog.Default.WriteLineAndConsole(
                LogPrefix + " layer audit: " + layerAudit.LayerCount + " layer(s), " +
                layerAudit.ActiveGroupCount + "/" + layerAudit.Groups.Count + " layer group winner(s) active, " +
                layerAudit.ActiveMappingCount + "/" + layerAudit.MappingCount + " mapping(s) active, " +
                layerAudit.BuiltInReplacementCount + " built-in assignment(s) replaced, " +
                layerAudit.ConflictingBlockCount + " multi-layer block conflict(s), " +
                layerAudit.Issues.Count + " warning(s), " + layerAudit.Notices.Count + " notice(s).");

            var modContext = ModContext as MyModContext;
            for (var i = 0; i < layerAudit.Issues.Count; i++)
            {
                var message = "Working Knowledge layer audit: " + layerAudit.Issues[i];
                MyLog.Default.WriteLineAndConsole(LogPrefix + " " + layerAudit.Issues[i]);
                if (modContext != null)
                    MyDefinitionErrors.Add(modContext, message, TErrorSeverity.Warning, false);
            }

            for (var i = 0; i < layerAudit.Notices.Count; i++)
                MyLog.Default.WriteLineAndConsole(LogPrefix + " layer audit notice: " + layerAudit.Notices[i]);
        }

        private void ReportRuntimeLoadFailure(Exception exception)
        {
            runtimeLoadIssue = exception == null ? "Unknown runtime loading failure." : exception.Message;
            var message = "Working Knowledge failed to load runtime data: " + runtimeLoadIssue;
            MyLog.Default.WriteLineAndConsole(LogPrefix + " " + message);

            var modContext = ModContext as MyModContext;
            if (modContext != null)
                MyDefinitionErrors.Add(modContext, message, TErrorSeverity.Error, false);
        }

        private void ShowLayerAudit()
        {
            ShowWkChatSection(
                "Layer Audit",
                "Runtime: " + (runtimeLoadIssue == null ? "ready" : "load failed"),
                "Layers found: " + layerAudit.LayerCount,
                "Priority: higher number wins; in-game Mods list: higher entry wins",
                "Warnings: " + layerAudit.Issues.Count + "; information notices: " + layerAudit.Notices.Count);

            ShowLayerAuditGroups();
            ShowLayerAuditMappings();
            ShowLayerAuditMoves();

            if (layerAudit.Issues.Count > 0)
            {
                var warningLines = new List<string>();
                var shown = Math.Min(layerAudit.Issues.Count, LayerAuditWarningEntryLimit);
                for (var i = 0; i < shown; i++)
                    warningLines.Add("- " + layerAudit.Issues[i]);

                var remaining = layerAudit.Issues.Count - shown;
                if (remaining > 0)
                    warningLines.Add(remaining + " more warning" + (remaining == 1 ? " is" : "s are") + " available in SpaceEngineers.log and F11.");

                ShowWkColoredChatSection("Audit Warnings", warningLines, WkChatCautionColor);
            }

            if (runtimeLoadIssue != null)
                ShowWkColoredChatSection("Audit Error", new[] { runtimeLoadIssue }, WkChatWarningColor);
        }

        private void ShowLayerAuditGroups()
        {
            var lines = new List<string>
            {
                "Active layer groups: " + layerAudit.ActiveGroupCount + " of " + layerAudit.Groups.Count,
                "Built-in groups redefined: " + layerAudit.RedefinedGroupCount,
            };

            var groups = new List<WorkingKnowledgeLayerGroup>(layerAudit.Groups.Values);
            groups.Sort((left, right) => string.Compare(left.Entry.DisplayName, right.Entry.DisplayName, StringComparison.OrdinalIgnoreCase));
            for (var i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                var active = layerAudit.ResolvedGroups.ContainsKey(group.Entry.ResearchId);
                lines.Add(
                    "- [" + group.Entry.Tier + "] " + group.Entry.DisplayName +
                    " (" + group.Entry.ResearchId + ") - " + (active ? "active" : "inactive"));
            }

            if (groups.Count == 0)
                lines.Add("- No layer-defined schematic groups.");

            ShowWkColoredChatSection("Schematic Groups", lines, WkChatResearchColor);
        }

        private void ShowLayerAuditMappings()
        {
            ShowWkColoredChatSection(
                "Block Mappings",
                new[]
                {
                    "Active: " + layerAudit.ActiveMappingCount + " of " + layerAudit.MappingCount,
                    "Moved from built-in groups: " + layerAudit.BuiltInReplacementCount,
                    "Multi-layer conflicts: " + layerAudit.ConflictingBlockCount,
                    "Skipped: " + layerAudit.SkippedMappingCount,
                },
                WkChatProficiencyColor);
        }

        private void ShowLayerAuditMoves()
        {
            var movesByDestination = new Dictionary<string, List<WorkingKnowledgeLayerMove>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < layerAudit.Moves.Count; i++)
            {
                var move = layerAudit.Moves[i];
                List<WorkingKnowledgeLayerMove> destinationMoves;
                if (!movesByDestination.TryGetValue(move.ToDisplayName, out destinationMoves))
                {
                    destinationMoves = new List<WorkingKnowledgeLayerMove>();
                    movesByDestination.Add(move.ToDisplayName, destinationMoves);
                }
                destinationMoves.Add(move);
            }

            var destinations = new List<string>(movesByDestination.Keys);
            destinations.Sort(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < destinations.Count; i++)
                ShowLayerAuditMoveDestination(destinations[i], movesByDestination[destinations[i]]);
        }

        private void ShowLayerAuditMoveDestination(string destination, List<WorkingKnowledgeLayerMove> moves)
        {
            var movesBySource = new Dictionary<string, List<WorkingKnowledgeLayerMove>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < moves.Count; i++)
            {
                List<WorkingKnowledgeLayerMove> sourceMoves;
                if (!movesBySource.TryGetValue(moves[i].FromDisplayName, out sourceMoves))
                {
                    sourceMoves = new List<WorkingKnowledgeLayerMove>();
                    movesBySource.Add(moves[i].FromDisplayName, sourceMoves);
                }
                sourceMoves.Add(moves[i]);
            }

            var lines = new List<string>();
            var sources = new List<string>(movesBySource.Keys);
            sources.Sort(StringComparer.OrdinalIgnoreCase);
            var shown = 0;
            for (var i = 0; i < sources.Count && shown < LayerAuditMoveEntryLimitPerDestination; i++)
            {
                var sourceMoves = movesBySource[sources[i]];
                sourceMoves.Sort((left, right) => string.Compare(left.BlockKey, right.BlockKey, StringComparison.OrdinalIgnoreCase));
                lines.Add("From " + sources[i] + " (" + sourceMoves.Count + "):");
                for (var j = 0; j < sourceMoves.Count && shown < LayerAuditMoveEntryLimitPerDestination; j++, shown++)
                    lines.Add("- " + sourceMoves[j].BlockKey);
            }

            var remaining = moves.Count - shown;
            if (remaining > 0)
                lines.Add("- " + remaining + " more moved block" + (remaining == 1 ? "" : "s") + " listed in SpaceEngineers.log.");

            ShowWkColoredChatSection("Moved to " + destination, lines, WkChatProgressHeaderColor);
        }
    }
}
