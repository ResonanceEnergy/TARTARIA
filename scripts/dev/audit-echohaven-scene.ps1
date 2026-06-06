# audit-echohaven-scene.ps1
# Runs Unity headlessly to audit Echohaven_VerticalSlice.unity for the blockers
# from STATUS.md (PlayerSpawner, NavMesh, building presence, prefab refs, missing scripts).
#
# Usage:  .\scripts\dev\audit-echohaven-scene.ps1
# Exit:   0 = clean (or warnings only), 1 = blocker present
#
# The audit logic itself lives in:
#   Assets/_Project/Scripts/Editor/EchohavenSceneAudit.cs
#
# This wrapper just launches Unity in batchmode, invokes the audit, and surfaces results.

param(
    [string]$UnityVersion = "6000.3.6f1",
    [int]$TimeoutSeconds = 240,
    [switch]$NoExit  # If set, don't exit Unity after audit — leave it open
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path "$PSScriptRoot\..\..").Path
$projectPath = $repoRoot
$reportPath = Join-Path $repoRoot "Logs\echohaven_audit_report.txt"
$unityLog = Join-Path $repoRoot "Logs\audit_unity.log"

# Find Unity executable
$candidates = @(
    "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe",
    "C:\Program Files\Unity\Editor\Unity.exe",
    "${env:ProgramFiles}\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
)
$unityExe = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $unityExe) {
    Write-Host "ERROR: Unity $UnityVersion not found. Tried:" -ForegroundColor Red
    $candidates | ForEach-Object { Write-Host "  $_" }
    Write-Host "Set -UnityVersion to your installed Unity 6 version, or edit the candidates list."
    exit 2
}

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ECHOHAVEN SCENE AUDIT (batchmode)" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Unity:    $unityExe"
Write-Host "Project:  $projectPath"
Write-Host "Report:   $reportPath"
Write-Host "Log:      $unityLog"
Write-Host ""

# Stale-report cleanup so we know the audit actually ran
if (Test-Path $reportPath) { Remove-Item $reportPath -Force }
if (Test-Path $unityLog)   { Remove-Item $unityLog -Force }

$args = @(
    "-batchmode",
    "-nographics",
    "-projectPath", "`"$projectPath`"",
    "-executeMethod", "Tartaria.Editor.EchohavenSceneAudit.AuditFromBatchmode",
    "-logFile", "`"$unityLog`""
)
if (-not $NoExit) { $args += "-quit" }

Write-Host "Launching Unity..." -ForegroundColor Yellow
$startTime = Get-Date
$proc = Start-Process -FilePath $unityExe -ArgumentList $args -PassThru -NoNewWindow

if (-not $proc.WaitForExit($TimeoutSeconds * 1000)) {
    Write-Host "TIMEOUT: Unity did not exit within $TimeoutSeconds seconds." -ForegroundColor Red
    try { $proc.Kill() } catch {}
    exit 124
}
$elapsed = [int]((Get-Date) - $startTime).TotalSeconds

Write-Host "Unity exited (code $($proc.ExitCode)) after ${elapsed}s." -ForegroundColor Yellow
Write-Host ""

# Surface the report
if (Test-Path $reportPath) {
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  AUDIT REPORT" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Get-Content $reportPath | ForEach-Object {
        if ($_ -match "\[PASS\]")      { Write-Host $_ -ForegroundColor Green }
        elseif ($_ -match "\[WARN\]")  { Write-Host $_ -ForegroundColor Yellow }
        elseif ($_ -match "\[FAIL\]")  { Write-Host $_ -ForegroundColor Red }
        elseif ($_ -match "BLOCKER")   { Write-Host $_ -ForegroundColor Red }
        elseif ($_ -match "PLAYABLE")  { Write-Host $_ -ForegroundColor Yellow }
        elseif ($_ -match "CLEAN")     { Write-Host $_ -ForegroundColor Green }
        else                           { Write-Host $_ }
    }
} else {
    Write-Host "WARN: Audit did not produce a report file." -ForegroundColor Yellow
    Write-Host "      This usually means the project failed to compile (see Unity log)." -ForegroundColor Yellow
    Write-Host ""
    if (Test-Path $unityLog) {
        Write-Host "Last 30 lines of Unity log:" -ForegroundColor Yellow
        Get-Content $unityLog -Tail 30
    }
    exit 3
}

Write-Host ""
Write-Host "Full report:   $reportPath"
Write-Host "Full Unity log: $unityLog"
Write-Host ""

# Unity batchmode exit codes from AuditFromBatchmode: 0 = pass/warn, 1 = fail (blocker)
exit $proc.ExitCode
