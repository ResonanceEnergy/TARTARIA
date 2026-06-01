# TARTARIA - Unity Quick Launch
# Opens Unity and provides execution instructions

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║            TARTARIA - UNITY QUICK LAUNCH                       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Find Unity
$unityPath = "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe"
$unityExe = Get-ChildItem $unityPath -ErrorAction SilentlyContinue | Select-Object -First 1

if (!$unityExe) {
    Write-Host "❌ Unity not found at standard path" -ForegroundColor Red
    Write-Host "   Please launch Unity Hub manually and open:" -ForegroundColor Yellow
    Write-Host "   C:\dev\TARTARIA_new" -ForegroundColor White
    Write-Host ""
    Read-Host "Press Enter to exit"
    exit
}

Write-Host "✅ Found Unity: $($unityExe.FullName)" -ForegroundColor Green
Write-Host ""
Write-Host "Opening Unity project..." -ForegroundColor Yellow
Write-Host ""

# Launch Unity
Start-Process $unityExe.FullName -ArgumentList "-projectPath `"$PWD`""

Start-Sleep -Seconds 3

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "⚡ UNITY EXECUTION CHECKLIST ⚡" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "Wait for Unity to open and compile scripts (~2 minutes)" -ForegroundColor White
Write-Host ""
Write-Host "STEP 1: GENERATE PREFABS (5 minutes)" -ForegroundColor Cyan
Write-Host "  1. Menu → Tartaria → Prefab Generator" -ForegroundColor White
Write-Host "  2. Click 'Test: Find KayKit Models' (verify ✅)" -ForegroundColor White
Write-Host "  3. Select mode: 'Moon 1 Only'" -ForegroundColor White
Write-Host "  4. Click '▶ GENERATE PREFABS'" -ForegroundColor White
Write-Host "  5. Wait for completion (watch progress bar)" -ForegroundColor White
Write-Host ""
Write-Host "STEP 2: WIRE PREFABS TO SYSTEMS (10 minutes)" -ForegroundColor Cyan
Write-Host "  6. Menu → Tartaria → Automated Prefab Wiring" -ForegroundColor White
Write-Host "  7. Select 'Wire Moon 1'" -ForegroundColor White
Write-Host "  8. Check 'Create Missing Prefabs' (if needed)" -ForegroundColor White
Write-Host "  9. Click '▶ RUN AUTOMATED WIRING'" -ForegroundColor White
Write-Host "  10. Wait for completion" -ForegroundColor White
Write-Host ""
Write-Host "STEP 3: TEST PLAYABLE MOON 1 (now!)" -ForegroundColor Cyan
Write-Host "  11. Open: Scenes/Echohaven_VerticalSlice.unity" -ForegroundColor White
Write-Host "  12. Press Play (▶)" -ForegroundColor White
Write-Host "  13. WASD to move, Mouse to look, E to interact" -ForegroundColor White
Write-Host "  14. Collect glowing cyan shards" -ForegroundColor White
Write-Host "  15. Fight MudGolems" -ForegroundColor White
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""
Write-Host "This window will stay open for reference." -ForegroundColor Gray
Write-Host "Close it when done." -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to close this window." -ForegroundColor Yellow
Write-Host ""

# Keep window open
while ($true) {
    Start-Sleep -Seconds 60
}
