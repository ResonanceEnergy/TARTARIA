# TARTARIA Phase 1 Quick Start
# Run this after Unity Editor opens and recompiles

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  ⚡ TARTARIA PHASE 1 — QUICK START SEQUENCE" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📋 PRE-FLIGHT CHECKLIST`n" -ForegroundColor Yellow

# Check Unity process
$unity = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unity) {
    Write-Host "✅ Unity Editor running (PID: $($unity.Id))" -ForegroundColor Green
} else {
    Write-Host "⚠️  Unity Editor NOT running - Launch Unity first!" -ForegroundColor Red
    Write-Host "   Run: .\Tools\Phase1\ReopenUnity.ps1`n" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Phase 1 tools ready" -ForegroundColor Green
Write-Host "✅ Cathedral measurements calculated" -ForegroundColor Green
Write-Host "✅ Material presets loaded`n" -ForegroundColor Green

Write-Host "🎬 IN UNITY EDITOR — DO THESE NOW:`n" -ForegroundColor Magenta

Write-Host "STEP 1: Check Console (30 seconds)" -ForegroundColor Yellow
Write-Host "  Window → General → Console" -ForegroundColor Gray
Write-Host "  → Wait for 'Compiling scripts...' to finish" -ForegroundColor Gray
Write-Host "  → Verify 0 errors ✅`n" -ForegroundColor Gray

Write-Host "STEP 2: Auto Setup Moon 1 Scene (1 minute)" -ForegroundColor Yellow
Write-Host "  Tools → TARTARIA → Auto Setup Moon 1 Scene" -ForegroundColor Gray
Write-Host "  → Click 'Yes' when prompted" -ForegroundColor Gray
Write-Host "  → Scene creates automatically with placement markers`n" -ForegroundColor Gray

Write-Host "STEP 3: Build Cathedral Prefabs (4-5 hours) ⚠️ CORE WORK" -ForegroundColor Yellow
Write-Host "  Open checklist:" -ForegroundColor Gray
Write-Host "    code Tools\Phase1\PREFAB_CREATION_CHECKLIST.md" -ForegroundColor Cyan
Write-Host "  Reference CSV:" -ForegroundColor Gray
Write-Host "    .\Tools\Phase1\Cathedral_Measurements.csv" -ForegroundColor Cyan
Write-Host "  Create 18 prefabs in Unity:" -ForegroundColor Gray
Write-Host "    Assets/_Project/Prefabs/Moon1/Cathedral/`n" -ForegroundColor Cyan

Write-Host "STEP 4: Create Shader Graphs (2-3 hours)" -ForegroundColor Yellow
Write-Host "  Reference config:" -ForegroundColor Gray
Write-Host "    code Tools\Phase1\MaterialPresets.json" -ForegroundColor Cyan
Write-Host "  Create in Unity:" -ForegroundColor Gray
Write-Host "    Assets → Create → Shader Graph → URP → Lit Shader Graph" -ForegroundColor Cyan
Write-Host "    Make 3: Stone_Tartarian, Metal_Ornate, Crystal_Aether`n" -ForegroundColor Cyan

Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📊 PHASE 1 TIMELINE`n" -ForegroundColor Yellow
Write-Host "  Day 1 (Today): Scene setup + start prefabs" -ForegroundColor White
Write-Host "  Day 2: Complete 18 prefabs + materials" -ForegroundColor White
Write-Host "  Day 3: Assembly + lighting + profiling`n" -ForegroundColor White

Write-Host "🚀 Unity is running — Begin Step 1 in Unity Editor now!`n" -ForegroundColor Green