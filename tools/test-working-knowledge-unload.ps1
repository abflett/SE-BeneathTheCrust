[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$root = Join-Path (Split-Path -Parent $PSScriptRoot) 'mods/WorkingKnowledge/Data/Scripts/WorkingKnowledge'

# Compile the production teardown methods against hostile dependency doubles: a
# registered client whose renderer is gone, and cleanup operations that throw.
function Read-Method([string] $Path, [string] $Signature) {
    $source = Get-Content -LiteralPath (Join-Path $root $Path) -Raw
    $start = $source.IndexOf($Signature, [StringComparison]::Ordinal)
    if ($start -lt 0) { throw "Method not found: $Signature" }
    $opening = $source.IndexOf('{', $start)
    $depth = 1
    $end = $opening + 1
    while ($depth -gt 0 -and $end -lt $source.Length) {
        if ($source[$end] -eq '{') { $depth++ }
        if ($source[$end] -eq '}') { $depth-- }
        $end++
    }
    if ($depth -ne 0) { throw "Unbalanced method: $Signature" }
    return $source.Substring($start, $end - $start)
}

$dispose = Read-Method 'Integration/RichHud/WkSettingsWindow.cs' 'internal void Dispose()'
$unload = Read-Method 'WkKnSession.cs' 'protected override void UnloadData()'
$cleanup = Get-Content (Join-Path $root 'Runtime/WkCleanup.cs') -Raw
$draft = Get-Content (Join-Path $root 'Integration/RichHud/WkSettingsDraft.cs') -Raw
$sources = @($cleanup, $draft) | ForEach-Object { $_ -replace '(?m)^using [^;]+;\r?\n', '' }
$harness = @'
namespace VRage.Utils {
    public class MyLog {
        public static readonly MyLog Default = new MyLog();
        public readonly List<string> Messages = new List<string>();
        public void WriteLineAndConsole(string message) { Messages.Add(message); }
    }
}
namespace WkKn {
    public class SessionBase { protected virtual void UnloadData() {} }
    public class WkKnSession : SessionBase {
        public const string LogPrefix = "WK";
        bool runtimeActive = true;
        public int EventCalls, ModuleCalls;
        void UnregisterGameEventHandlers() { EventCalls++; throw new Exception("event teardown unavailable"); }
        void UnloadRuntimeModules() { ModuleCalls++; }
        public void Exit() { UnloadData(); }
        __UNLOAD__
    }
    static class RichHudClient { public static bool Registered; }
    static class HudMain { public static bool EnableCursor; }
    sealed class Bind { public event EventHandler NewPressed; public void Press() { if (NewPressed != null) NewPressed(this, EventArgs.Empty); } }
    sealed class Root { public int Calls; public bool Unregister() { Calls++; return true; } }
    sealed class Window {
        bool disposed, live = true;
        public bool Visible = true, FailRestore, FailDetach;
        public int RestoreCalls, DetachCalls, EscapeCalls;
        readonly Bind escapeBind = new Bind();
        readonly Root responsiveRoot = new Root();
        readonly WkSettingsDraft draft = new WkSettingsDraft();
        readonly List<int> pages = new List<int> { 1 }, rows = new List<int> { 1 }, worldRows = new List<int> { 1 }, navigationButtons = new List<int> { 1 };
        object selectedPage = new object();
        public Window() {
            escapeBind.NewPressed += OnEscapePressed;
            draft.Stage("edit", "1", delegate { throw new Exception("must not apply on exit"); });
        }
        void OnEscapePressed(object sender, EventArgs args) { EscapeCalls++; }
        void Hide() { throw new Exception("renderer already destroyed"); }
        void RestoreGameHud() { RestoreCalls++; if (FailRestore) throw new Exception("game HUD unavailable"); }
        bool Unregister() { DetachCalls++; if (FailDetach) throw new Exception("detach unavailable"); return true; }
        __DISPOSE__
        public void Verify() {
            escapeBind.Press();
            if (!disposed || live || Visible || HudMain.EnableCursor || draft.Count != 0 ||
                RestoreCalls != 1 || DetachCalls != 1 || responsiveRoot.Calls != 1 || EscapeCalls != 0 ||
                pages.Count + rows.Count + worldRows.Count + navigationButtons.Count != 0 || selectedPage != null)
                throw new Exception("Incomplete or repeated window cleanup");
        }
    }
    public static class UnloadChecks {
        public static int Run() {
            int count = 0;
            foreach (bool registered in new[] { false, true })
                foreach (bool failRestore in new[] { false, true })
                    foreach (bool failDetach in new[] { false, true }) {
                        RichHudClient.Registered = registered;
                        HudMain.EnableCursor = true;
                        int before = VRage.Utils.MyLog.Default.Messages.Count;
                        var window = new Window { FailRestore = failRestore, FailDetach = failDetach };
                        window.Dispose();
                        window.Dispose();
                        window.Verify();
                        if (VRage.Utils.MyLog.Default.Messages.Count - before != (failRestore ? 1 : 0) + (failDetach ? 1 : 0))
                            throw new Exception("Cleanup failures were not logged exactly once");
                        count++;
                    }
            var session = new WkKnSession();
            session.Exit();
            session.Exit();
            if (session.EventCalls != 1 || session.ModuleCalls != 1)
                throw new Exception("Session cleanup stopped early or ran twice");
            return count + 1;
        }
    }
}
'@
$harness = $harness.Replace('__DISPOSE__', $dispose).Replace('__UNLOAD__', $unload)
Add-Type -TypeDefinition ("using System;`nusing System.Collections.Generic;`nusing VRage.Utils;`n" + ($sources -join "`n") + $harness)
Write-Host "Passed $([WkKn.UnloadChecks]::Run()) Working Knowledge unload regression scenarios."
