# P5.L1 — Run the Moon 1 NPC prefab variant builder via Unity headless.
# Requires that no other Unity instance is open on a project that holds the
# Package Manager named pipe (Upm-*). If the main TARTARIA_new editor is
# running, close it first, OR invoke this script's menu item from inside
# Unity: Tartaria -> 5 Phase 5 -> Build Moon 1 NPC Prefab Variants
param(
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe",
    [string]$ProjectPath = "C:\dev\_wt_p5_l1_npc_humanoid"
)
$ErrorActionPreference = "Stop"
if (!(Test-Path $UnityExe)) { throw "Unity exe not found: $UnityExe" }
if (!(Test-Path $ProjectPath)) { throw "Project path not found: $ProjectPath" }
$log = Join-Path $ProjectPath "unity_p5_l1_build.log"
if (Test-Path $log) { Remove-Item $log -Force }
$args = @(
    "-batchmode","-nographics","-quit",
    "-projectPath", $ProjectPath,
    "-executeMethod", "Tartaria.Editor.Moon1NpcPrefabVariantBuilder.BuildAll",
    "-logFile", $log
)
Write-Host "[P5.L1] Launching Unity headless..."
$p = Start-Process -FilePath $UnityExe -ArgumentList $args -PassThru -WindowStyle Hidden
Write-Host "[P5.L1] PID=$($p.Id) — tailing log..."
while (!$p.HasExited) {
    Start-Sleep -Seconds 5
    if (Test-Path $log) {
        Get-Content $log -Tail 5
        Write-Host "----"
    }
}
Write-Host "[P5.L1] Unity exited with code $($p.ExitCode)"
if ($p.ExitCode -ne 0) {
    Write-Host "[P5.L1] FAIL — see $log"
    exit $p.ExitCode
}
Write-Host "[P5.L1] OK"