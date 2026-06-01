# ════════════════════════════════════════════════════════════════
# VEX AURELIAN — AUTOMATED TARTARIA BUILD VALIDATION
# ════════════════════════════════════════════════════════════════

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                              ║" -ForegroundColor Cyan
Write-Host "║  VEX AURELIAN — 2100 Engine Architect                       ║" -ForegroundColor Cyan
Write-Host "║  TARTARIA Build Validation & Launch                         ║" -ForegroundColor Cyan
Write-Host "║                                                              ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$projectPath = "C:\dev\TARTARIA_new"
$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"

# Step 1: Clean lockfiles
Write-Host "PHASE 1: Cleanup" -ForegroundColor Yellow
Write-Host "  Removing stale Unity lockfiles..." -ForegroundColor Gray

if (Test-Path "$projectPath\Temp\UnityLockfile") {
    Remove-Item "$projectPath\Temp\UnityLockfile" -Force -ErrorAction SilentlyContinue
    Write-Host "  ✅ Removed UnityLockfile" -ForegroundColor Green
}

# Kill any existing Unity instances for this project
$unityProcs = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -and $_.CommandLine -like "*TARTARIA_new*" }

if ($unityProcs) {
    Write-Host "  ⚡ Killing existing Unity instances..." -ForegroundColor Yellow
    $unityProcs | ForEach-Object {
        Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 3
    Write-Host "  ✅ Cleanup complete" -ForegroundColor Green
}

Write-Host ""

# Step 2: Launch Unity
Write-Host "PHASE 2: Launch Unity Editor" -ForegroundColor Yellow
Write-Host "  Starting Unity 6000.3.6f1..." -ForegroundColor Gray
Write-Host ""

Start-Process -FilePath $unityExe -ArgumentList "-projectPath","$projectPath"

Write-Host "  ✅ Unity launched" -ForegroundColor Green
Write-Host ""

# Step 3: Instructions
Write-Host "PHASE 3: Manual Validation Steps" -ForegroundColor Yellow
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""
Write-Host "  Unity is now opening. When it finishes loading:" -ForegroundColor White
Write-Host ""
Write-Host "  1️⃣  If prompted about scene recovery, choose:" -ForegroundColor Cyan
Write-Host "      → YES to copy backups to Assets/_Recovery" -ForegroundColor White
Write-Host "      (or NO if you don't need them)" -ForegroundColor Gray
Write-Host ""
Write-Host "  2️⃣  Wait for compilation to finish (bottom-right progress bar)" -ForegroundColor Cyan
Write-Host ""
Write-Host "  3️⃣  Menu → Tartaria → Vex → Full Validation" -ForegroundColor Cyan
Write-Host "      This validates:" -ForegroundColor White
Write-Host "        • Script compilation" -ForegroundColor Gray
Write-Host "        • Assembly loading" -ForegroundColor Gray
Write-Host "        • Core managers" -ForegroundColor Gray
Write-Host "        • Essential scenes/prefabs" -ForegroundColor Gray
Write-Host ""
Write-Host "  4️⃣  If validation passes:" -ForegroundColor Cyan
Write-Host "      Menu → Tartaria → 🚀 ONE-CLICK: Load & Setup Echohaven" -ForegroundColor White
Write-Host ""
Write-Host "  5️⃣  Press Ctrl+P to enter Play Mode" -ForegroundColor Cyan
Write-Host ""
Write-Host "  6️⃣  Menu → Tartaria → EMERGENCY: Make Game Playable NOW" -ForegroundColor Cyan
Write-Host ""
Write-Host "  7️⃣  Use WASD or gamepad left stick to move" -ForegroundColor Cyan
Write-Host ""
Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Gray
Write-Host ""
Write-Host "✅ Vex setup complete. Unity is loading..." -ForegroundColor Green
Write-Host ""
Write-Host "Report results when ready. If validation fails, check Console for errors." -ForegroundColor Yellow
Write-Host ""
