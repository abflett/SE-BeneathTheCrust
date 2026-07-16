using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int LayerAuditChatEntryLimit = 12;

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
            var lines = new List<string>
            {
                "Runtime: " + (runtimeLoadIssue == null ? "ready" : "load failed"),
                "Layers found: " + layerAudit.LayerCount,
                "Layer group winners active: " + layerAudit.ActiveGroupCount + " of " + layerAudit.Groups.Count,
                "Built-in groups redefined: " + layerAudit.RedefinedGroupCount,
                "Mappings active: " + layerAudit.ActiveMappingCount + " of " + layerAudit.MappingCount,
                "Built-in block assignments replaced: " + layerAudit.BuiltInReplacementCount,
                "Multi-layer block conflicts: " + layerAudit.ConflictingBlockCount,
                "Mappings skipped: " + layerAudit.SkippedMappingCount,
                "Warnings: " + layerAudit.Issues.Count + "; notices: " + layerAudit.Notices.Count,
            };

            if (runtimeLoadIssue != null)
                lines.Add("Runtime issue: " + runtimeLoadIssue);

            var shown = 0;
            for (var i = 0; i < layerAudit.Issues.Count && shown < LayerAuditChatEntryLimit; i++, shown++)
                lines.Add("- Warning: " + layerAudit.Issues[i]);

            for (var i = 0; i < layerAudit.Notices.Count && shown < LayerAuditChatEntryLimit; i++, shown++)
                lines.Add("- Notice: " + layerAudit.Notices[i]);

            if (layerAudit.Issues.Count == 0)
                lines.Add("No unresolved layer compatibility warnings were found.");

            var remaining = layerAudit.Issues.Count + layerAudit.Notices.Count - shown;
            if (remaining > 0)
            {
                lines.Add(
                    remaining + " more audit entr" + (remaining == 1 ? "y is" : "ies are") +
                    " available in SpaceEngineers.log; warnings also appear in F11.");
            }

            ShowWkChatSection("Layer Audit", lines);
        }
    }
}
