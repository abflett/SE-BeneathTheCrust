[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Get-Content -Raw (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/Configuration/WkSettingNumericInput.cs')
$draftSource = Get-Content -Raw (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/RichHud/WkSettingsDraft.cs')
$draftSource = $draftSource -replace '(?m)^using [^;]+;\r?\n', ''
$source = "using System.Collections.Generic;" + [Environment]::NewLine + $source + [Environment]::NewLine + $draftSource
$integrationRoot = Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration'
$extraSources = @(
    (Join-Path $integrationRoot 'RichHud/WkSettingsTypingDelay.cs'),
    (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Domain/Common/RatioMath.cs')
)
$extraSources += Get-ChildItem (Join-Path $integrationRoot 'Configuration') -Filter '*.cs' |
    Where-Object { $_.Name -notlike 'WkKnSession*' -and $_.Name -ne 'WkSettingNumericInput.cs' } |
    Select-Object -ExpandProperty FullName
foreach ($sourcePath in $extraSources) {
    $source += [Environment]::NewLine + ((Get-Content -Raw $sourcePath) -replace '(?m)^using [^;]+;\r?\n', '')
}
$source = "using System.Xml.Serialization;" + [Environment]::NewLine + $source
$checks = @'
namespace WkKn
{
    public static class NumericInputChecks
    {
        private static int count;
        private static void Check(string text, double scale, string suffix, double min, double max, bool integer, string expected)
        {
            string value, error;
            bool accepted = WkSettingNumericInput.TryParse(text, scale, suffix, min, max, integer, out value, out error);
            if (accepted != (expected != null) || value != expected || (!accepted && string.IsNullOrEmpty(error)))
                throw new System.Exception("Unexpected numeric input result for: " + text);
            count++;
        }

        public static int Run()
        {
            Check("25x", 1, "x", 0, 100, false, "25"); // Beyond slider's 10x maximum.
            Check("100", 1, "x", 0, 100, false, "100");
            Check("100.01x", 1, "x", 0, 100, false, null);
            Check("1.234567x", 1, "x", 0, 100, false, "1.234567");
            Check("0", 1, "x", 0, 100, false, "0");
            Check("-1", 1, "x", 0, 100, false, null);
            Check("5", 100, "%", 0, 1, false, "0.05");
            Check(" 5% ", 100, "%", 0, 1, false, "0.05");
            Check("0.05%", 100, "%", 0, 1, false, "0.0005");
            Check("100%", 100, "%", 0, 1, false, "1");
            Check("101%", 100, "%", 0, 1, false, null);
            Check("150%", 100, "%", 0, 10, false, "1.5"); // Segment rate, not a bounded probability.
            Check("45 s", 1, " s", 0, 60, false, "45");
            Check("500 m", 1, " m", 0, 1000, false, "500");
            Check("-1.5", 1, "", -2, 2, false, "-1.5");
            Check("10", 1, "", 1, 10, true, "10");
            Check("5.5", 1, "", 1, 10, true, null);
            foreach (var text in new[] { "", "abc", "NaN", "Infinity", "-Infinity", "1e999", "5%", "1,5" })
                Check(text, 1, "x", 0, 100, false, null);
            var draft = new WkSettingsDraft();
            var applied = new System.Collections.Generic.List<string>();
            draft.Stage("rate", "2", () => applied.Add("old"));
            draft.Stage("RATE", "3", () => applied.Add("rate"));
            draft.Stage("delay", "5", () => applied.Add("delay"));
            if (applied.Count != 0 || draft.Count != 2 || draft.GetValue("rate", () => "1") != "3")
                throw new System.Exception("Edits must remain local and repeated edits must replace the prior value.");
            draft.Apply();
            if (string.Join(",", applied) != "rate,delay" || draft.Count != 0 || draft.GetValue("rate", () => "1") != "1")
                throw new System.Exception("Apply must submit the latest values in order and clear drafts.");
            draft.Apply();
            if (applied.Count != 2)
                throw new System.Exception("Applying an empty draft must not resubmit changes.");
            draft.Stage("reset", "", () => applied.Add("reset"));
            draft.Clear();
            draft.Apply();
            if (applied.Count != 2 || draft.Count != 0)
                throw new System.Exception("Discard must never execute pending actions.");
            count += 4;
            var delay = new WkSettingsTypingDelay();
            delay.Reset("1");
            if (delay.IsReady("2", 0) || delay.IsReady("25", 200) || delay.IsReady("25", 549))
                throw new System.Exception("Typing must restart the debounce interval.");
            if (!delay.IsReady("25", 550) || delay.IsReady("25", 551))
                throw new System.Exception("A settled edit must be processed once.");
            delay.Reset("3");
            if (delay.IsReady("3", 1000))
                throw new System.Exception("Programmatic refresh/discard must cancel pending typing.");
            count += 3;

            var world = new WkConfigStore();
            var players = new WkPlayerConfigStore();
            draft.Stage("researchScale", "25", () => {
                string error;
                if (!world.TrySetValue("researchScale", "25", out error)) throw new System.Exception(error);
            });
            draft.Stage("progressHudRows", "9", () => {
                string error;
                if (!players.TrySetValue("test", "progressHudRows", "9", out error)) throw new System.Exception(error);
            });
            draft.Stage("researchChatSuppressionPercent", "0.05", () => {
                string error;
                if (!players.TrySetValue("test", "researchChatSuppressionPercent", "0.05", out error)) throw new System.Exception(error);
            });
            if (world.Data.ResearchScale == 25 || players.GetPlayerOrDefault("test").ProgressHudRows == 9)
                throw new System.Exception("Draft changes must not reach configuration before Apply.");
            draft.Apply();
            var worldXml = new System.Xml.Serialization.XmlSerializer(typeof(WkConfig));
            var playerXml = new System.Xml.Serialization.XmlSerializer(typeof(WkPlayerConfigSaveData));
            var worldText = new System.IO.StringWriter();
            var playerText = new System.IO.StringWriter();
            worldXml.Serialize(worldText, world.Data);
            playerXml.Serialize(playerText, players.Data);
            var restoredWorld = new WkConfigStore();
            var restoredPlayers = new WkPlayerConfigStore();
            restoredWorld.SetData((WkConfig)worldXml.Deserialize(new System.IO.StringReader(worldText.ToString())));
            restoredPlayers.SetData((WkPlayerConfigSaveData)playerXml.Deserialize(new System.IO.StringReader(playerText.ToString())));
            if (restoredWorld.Data.ResearchScale != 25 || restoredPlayers.GetPlayerOrDefault("test").ProgressHudRows != 9 ||
                restoredPlayers.GetPlayerOrDefault("test").ResearchChatSuppressionPercent != 5)
                throw new System.Exception("Applied values must survive configuration serialization and normalization.");
            count += 2;
            return count;
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $checks) -ReferencedAssemblies System.Xml
Write-Host "Passed $([WkKn.NumericInputChecks]::Run()) Working Knowledge numeric input and draft checks."
