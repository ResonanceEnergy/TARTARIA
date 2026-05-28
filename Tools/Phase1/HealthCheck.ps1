# TARTARIA Phase 1 Project Health Check
# Validates entire Phase 1 setup: folders, scripts, assets, measurements

Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🏥 TARTARIA PHASE 1 — PROJECT HEALTH CHECK" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

$checks = @()
$pass = 0
$fail = 0

# Check 1: Unity Editor scripts
Write-Host "📝 Unity Editor Scripts:" -ForegroundColor Yellow
$scripts = @(
    "Assets\_Project\Scripts\Editor\MoonSceneBuilder.cs",
    "Assets\_Project\Scripts\Editor\AddressablesConfigurator.cs",
    "Assets\_Project\Scripts\Editor\AssetInventoryTool.cs",
    "Assets\_Project\Scripts\Editor\AssetWiringTool.cs",
    "Assets\_Project\Scripts\Editor\AutoSceneSetup.cs"
)
foreach ($s in $scripts) {
    if (Test-Path $s) {
        Write-Host "  ✅ $(Split-Path $s -Leaf)" -ForegroundColor Green
        $pass++
    } else {
        Write-Host "  ❌ $(Split-Path $s -Leaf) MISSING" -ForegroundColor Red
        $fail++
    }
}

# Check 2: Phase 1 automation tools
Write-Host "`n⚙️  Phase 1 Automation Tools:" -ForegroundColor Yellow
$tools = @(
    "Tools\Phase1\GoldenRatioCalculator.ps1",
    "Tools\Phase1\Cathedral_Measurements.csv",
    "Tools\Phase1\PREFAB_CREATION_CHECKLIST.md",
    "Tools\Phase1\MaterialPresets.json",
    "Tools\Phase1\ReopenUnity.ps1",
    "Tools\Phase1\QuickStart.ps1",
    "Tools\Phase1\ShaderGraphQuickRef.md",
    "Tools\Phase1\ValidatePrefabs.ps1",
    "Tools\Phase1\DownloadHDRI.ps1"
)
foreach ($t in $tools) {
    if (Test-Path $t) {
        Write-Host "  ✅ $(Split-Path $t -Leaf)" -ForegroundColor Green
        $pass++
    } else {
        Write-Host "  ❌ $(Split-Path $t -Leaf) MISSING" -ForegroundColor Red
        $fail++
    }
}

# Check 3: Directory structure
Write-Host "`n📁 Directory Structure:" -ForegroundColor Yellow
$dirs = @(
    "Assets\_Project\Prefabs\Moon1\Cathedral",
    "Assets\_Project\Materials\Master",
    "Assets\_Project\Materials\Moon1",
    "Assets\_Project\Textures\Skyboxes"
)
foreach ($d in $dirs) {
    if (Test-Path $d) {
        Write-Host "  ✅ $d" -ForegroundColor Green
        $pass++
    } else {
        Write-Host "  ❌ $d MISSING" -ForegroundColor Red
        $fail++
    }
}

# Check 4: Golden Ratio calculations
Write-Host "`n📐 Golden Ratio Measurements:" -ForegroundColor Yellow
$csvPath = "Tools\Phase1\Cathedral_Measurements.csv"
if (Test-Path $csvPath) {
    $csv = Import-Csv $csvPath
    if ($csv.Count -eq 11) {
        Write-Host "  ✅ 11 component measurements loaded" -ForegroundColor Green
        $baseHeight = [float]($csv | Where-Object Component -eq "Wall_4x4m" | Select-Object -ExpandProperty Height)
        $spireBase = [float]($csv | Where-Object Component -eq "Spire_Base" | Select-Object -ExpandProperty Height); $spireMid = [float]($csv | Where-Object Component -eq "Spire_Mid" | Select-Object -ExpandProperty Height); $spireTop = [float]($csv | Where-Object Component -eq "Spire_Top_MercuryBall" | Select-Object -ExpandProperty Height); $spireTotal = $spireBase + $spireMid + $spireTop
        if ([math]::Abs($baseHeight - 6.472) -lt 0.01 -and [math]::Abs($spireTotal - 20.944) -lt 0.01) {
            Write-Host "  ✅ Golden ratio verified (φ = 1.618)" -ForegroundColor Green
            $pass += 2
        } else {
            Write-Host "  ⚠️  Measurements don't match φ formula" -ForegroundColor Yellow
            $fail++
        }
    } else {
        Write-Host "  ❌ Invalid CSV (expected 11 rows, got $($csv.Count))" -ForegroundColor Red
        $fail++
    }
} else {
    Write-Host "  ❌ Cathedral_Measurements.csv NOT FOUND" -ForegroundColor Red
    $fail++
}

# Check 5: HDRI skybox
Write-Host "`n🌅 HDRI Skybox:" -ForegroundColor Yellow
$hdri = "Assets\_Project\Textures\Skyboxes\Moon1_GoldenHour_HDRI.exr"
if (Test-Path $hdri) {
    $size = (Get-Item $hdri).Length / 1MB
    Write-Host "  ✅ Moon1_GoldenHour_HDRI.exr ($([math]::Round($size, 1)) MB)" -ForegroundColor Green
    $pass++
} else {
    Write-Host "  ⚠️  HDRI not downloaded yet (run .\Tools\Phase1\DownloadHDRI.ps1)" -ForegroundColor Yellow
    $fail++
}

# Check 6: Documentation
Write-Host "`n📚 Documentation:" -ForegroundColor Yellow
$docs = @(
    "PHASE_1_MOON_1_EXECUTION.md",
    "PHASE_0_UNITY_TASKS.md",
    "docs\PRODUCTION_TRACKER.md"
)
foreach ($doc in $docs) {
    if (Test-Path $doc) {
        Write-Host "  ✅ $(Split-Path $doc -Leaf)" -ForegroundColor Green
        $pass++
    } else {
        Write-Host "  ❌ $(Split-Path $doc -Leaf) MISSING" -ForegroundColor Red
        $fail++
    }
}

# Check 7: Unity project files
Write-Host "`n🎮 Unity Project:" -ForegroundColor Yellow
if (Test-Path "ProjectSettings\ProjectVersion.txt") {
    $version = Get-Content "ProjectSettings\ProjectVersion.txt" | Select-String "m_EditorVersion:"
    Write-Host "  ✅ Unity version: $($version -replace 'm_EditorVersion: ','')" -ForegroundColor Green
    $pass++
} else {
    Write-Host "  ❌ ProjectVersion.txt NOT FOUND (not a Unity project?)" -ForegroundColor Red
    $fail++
}

if (Test-Path "Packages\manifest.json") {
    Write-Host "  ✅ Package manifest present" -ForegroundColor Green
    $pass++
} else {
    Write-Host "  ❌ Package manifest MISSING" -ForegroundColor Red
    $fail++
}

# Summary
Write-Host "`n═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
$total = $pass + $fail
$percent = [math]::Round(($pass / $total) * 100, 1)

if ($fail -eq 0) {
    Write-Host "  🎉 ALL CHECKS PASSED ($pass/$total - 100%) 🎉" -ForegroundColor Green
    Write-Host "  Phase 1 setup is COMPLETE and HEALTHY!" -ForegroundColor Green
} elseif ($percent -ge 80) {
    Write-Host "  ✅ PHASE 1 SETUP: $percent% COMPLETE ($pass/$total checks)" -ForegroundColor Yellow
    Write-Host "  $fail minor issues detected (see above)" -ForegroundColor Yellow
} else {
    Write-Host "  ⚠️  PHASE 1 INCOMPLETE: $percent% ($pass/$total checks)" -ForegroundColor Red
    Write-Host "  $fail critical items missing!" -ForegroundColor Red
}

Write-Host "═══════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

if ($fail -eq 0) {
    Write-Host "🚀 READY FOR NEXT STEPS:" -ForegroundColor Magenta
    Write-Host "  1. Launch Unity: .\Tools\Phase1\ReopenUnity.ps1" -ForegroundColor White
    Write-Host "  2. Auto-setup scene: Tools → TARTARIA → Auto Setup Moon 1 Scene" -ForegroundColor White
    Write-Host "  3. Validate progress: .\Tools\Phase1\ValidatePrefabs.ps1 -Detailed`n" -ForegroundColor White
}