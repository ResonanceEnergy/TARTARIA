# Add-Unity-Defender-Exclusion.ps1
# RUN AS ADMINISTRATOR. Right-click → Run with PowerShell, accept UAC.
#
# Adds Windows Defender exclusions so Unity Package Manager (UPM) can start its
# IPC stream without being blocked by real-time scanning. This is the canonical
# fix for the "[Package Manager] Could not connect to IPC stream" error in
# Editor.log.

param(
    [string]$UnityInstall = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor",
    [string]$ProjectPath  = "C:\dev\TARTARIA_new"
)

# --- Sanity: confirm elevation ---
$currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator))
{
    Write-Host "ERROR: This script must be run as Administrator." -ForegroundColor Red
    Write-Host "Right-click the file → Run with PowerShell (and accept UAC), or open PowerShell as Admin and re-run." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host "=== Adding Windows Defender exclusions for Unity ===" -ForegroundColor Cyan

# --- Process exclusions ---
$processExclusions = @(
    "UnityPackageManager.exe",
    "Unity.exe",
    "UnityHelper.exe",
    "UnityShaderCompiler.exe"
)
foreach ($p in $processExclusions) {
    try {
        Add-MpPreference -ExclusionProcess $p -ErrorAction Stop
        Write-Host "  [OK] process: $p" -ForegroundColor Green
    } catch {
        Write-Host "  [WARN] process $p — $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

# --- Path exclusions ---
$pathExclusions = @(
    $UnityInstall,
    "$ProjectPath\Library",
    "$ProjectPath\Temp",
    "$ProjectPath\obj",
    "$env:LOCALAPPDATA\Unity\cache",
    "$env:LOCALAPPDATA\Unity\Editor\Cache"
)
foreach ($p in $pathExclusions) {
    try {
        Add-MpPreference -ExclusionPath $p -ErrorAction Stop
        Write-Host "  [OK] path: $p" -ForegroundColor Green
    } catch {
        Write-Host "  [WARN] path $p — $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "=== Verification ===" -ForegroundColor Cyan
$pref = Get-MpPreference
Write-Host "Process exclusions ($($pref.ExclusionProcess.Count) total):"
$pref.ExclusionProcess | Where-Object { $_ -match "Unity" } | ForEach-Object { Write-Host "  $_" }
Write-Host "Path exclusions matching 'Unity' or 'TARTARIA':"
$pref.ExclusionPath | Where-Object { $_ -match "Unity|TARTARIA" } | ForEach-Object { Write-Host "  $_" }

Write-Host ""
Write-Host "=== Next steps ===" -ForegroundColor Cyan
Write-Host "1. Kill any running Unity processes (this script does NOT auto-kill them)."
Write-Host "2. Relaunch Unity from Unity Hub or via Tartaria launcher."
Write-Host "3. Wait for compile to finish (cold-cache Library rebuild ~3-8 min)."
Write-Host "4. Once Editor finishes loading, the MCP bridge should start on port 8080."
Write-Host ""
Write-Host "If UPM still hangs, check Editor.log at:"
Write-Host "  $env:LOCALAPPDATA\Unity\Editor\Editor.log"
Read-Host "Done. Press Enter to close"
