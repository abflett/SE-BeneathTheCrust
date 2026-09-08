[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Get-Content -Raw (Join-Path $repoRoot 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge/Integration/Configuration/WkSettingNumericInput.cs')
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
            return count;
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $checks)
Write-Host "Passed $([WkKn.NumericInputChecks]::Run()) Working Knowledge numeric input checks."
