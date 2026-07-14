using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Utils;

namespace WkKn
{
    public partial class WkKnSession
    {
        private const int LayerAuditChatIssueLimit = 12;

        private void PublishLayerAudit()
        {
            MyLog.Default.WriteLineAndConsole(
                LogPrefix + " layer audit: " + layerAudit.LayerCount + " layer(s), " +
                layerAudit.ActiveGroupCount + "/" + layerAudit.Groups.Count + " custom group(s) active, " +
                layerAudit.ActiveMappingCount + "/" + layerAudit.MappingCount + " mapping(s) active, " +
                layerAudit.ActiveOverrideCount + "/" + layerAudit.OverrideCount + " override(s) active, " +
                layerAudit.Issues.Count + " issue(s).");

            var modContext = ModContext as MyModContext;
            for (var i = 0; i < layerAudit.Issues.Count; i++)
            {
                var message = "Working Knowledge layer audit: " + layerAudit.Issues[i];
                MyLog.Default.WriteLineAndConsole(LogPrefix + " " + layerAudit.Issues[i]);
                if (modContext != null)
                    MyDefinitionErrors.Add(modContext, message, TErrorSeverity.Warning, false);
            }
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
                "Custom groups active: " + layerAudit.ActiveGroupCount + " of " + layerAudit.Groups.Count,
                "Mappings active: " + layerAudit.ActiveMappingCount + " of " + layerAudit.MappingCount,
                "Explicit overrides active: " + layerAudit.ActiveOverrideCount + " of " + layerAudit.OverrideCount,
                "Issues: " + layerAudit.Issues.Count,
            };

            if (runtimeLoadIssue != null)
                lines.Add("Runtime issue: " + runtimeLoadIssue);

            if (layerAudit.Issues.Count == 0)
            {
                lines.Add("No layer compatibility issues were found.");
            }
            else
            {
                var shown = Math.Min(LayerAuditChatIssueLimit, layerAudit.Issues.Count);
                for (var i = 0; i < shown; i++)
                    lines.Add("- " + layerAudit.Issues[i]);

                if (shown < layerAudit.Issues.Count)
                {
                    lines.Add(
                        (layerAudit.Issues.Count - shown) +
                        " more issue(s) are available in the F11 mod-error screen and SpaceEngineers.log.");
                }
            }

            ShowWkChatSection("Layer Audit", lines);
        }
    }
}
