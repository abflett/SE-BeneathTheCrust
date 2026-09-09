[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Get-Content -Raw (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/Configuration/WkSettingNumericInput.cs')
$draftSource = Get-Content -Raw (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/RichHud/WkSettingsDraft.cs')
$draftSource = $draftSource -replace '(?m)^using [^;]+;\r?\n', ''
$source = "using System.Collections.Generic;" + [Environment]::NewLine + $source + [Environment]::NewLine + $draftSource
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
            return count;
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $checks)
Write-Host "Passed $([WkKn.NumericInputChecks]::Run()) Working Knowledge numeric input and draft checks."
