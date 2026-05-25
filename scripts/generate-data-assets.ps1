# TARTARIA - Automated Data Asset Generation
# Executes Unity Editor methods via batchmode to create game data

param(
    [switch]$SkipPopulate
)

$ErrorActionPreference = "Stop"

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = "C:\dev\TARTARIA_new"
$logPath = "$projectPath\Logs\tartaria-build.log"  # Use same log as tartaria-play.ps1

Write-Host ""
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " TARTARIA — Data Asset Generation" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Step 1: Generate 26 data assets
Write-Host "[1/2] Generating 26 data assets (1 database + 10 items + 10 equipment + 5 enemies)..." -ForegroundColor Yellow
Write-Host "      Method: Tartaria.Editor.DataAssetGenerator.GenerateAllDataAssets" -ForegroundColor Gray

$args1 = @(
    "-batchmode"
    "-projectPath", $projectPath
    "-executeMethod", "Tartaria.Editor.DataAssetGenerator.GenerateAllDataAssets"
    "-logFile", $logPath
    "-quit"
)

$process1 = Start-Process -FilePath $unityPath -ArgumentList $args1 -Wait -PassThru -NoNewWindow

# Show [Tartaria] or [DataAssetGenerator] lines from log
if (Test-Path $logPath) {
    Write-Host ""
    Write-Host "-- Build Log ([Tartaria] / [DataAssetGenerator] lines) --" -ForegroundColor DarkGray
    Get-Content $logPath -Tail 100 | ForEach-Object {
        if ($_ -match "\[Tartaria\]|\[DataAssetGenerator\]") {
            if ($_ -match "FAIL|ERROR|CRASH") {
                Write-Host "  $_" -ForegroundColor Red
            } elseif ($_ -match "OK|PASSED|complete|SUCCESS|Created") {
                Write-Host "  $_" -ForegroundColor Green
            } else {
                Write-Host "  $_" -ForegroundColor Cyan
            }
        }
    }
    Write-Host "-- End --" -ForegroundColor DarkGray
    Write-Host ""
}

if ($process1.ExitCode -eq 0) {
    Write-Host "      ✓ Generation PASSED" -ForegroundColor Green
} else {
    Write-Host "      ✗ Generation FAILED (exit code $($process1.ExitCode))" -ForegroundColor Red
    Write-Host ""
    Write-Host "Full log: $logPath" -ForegroundColor Yellow
    exit $process1.ExitCode
}

if ($SkipPopulate) {
    Write-Host ""
    Write-Host "Skipping ItemDatabase population" -ForegroundColor Gray
    exit 0
}

# Step 2: Populate ItemDatabase
Write-Host ""
Write-Host "[2/2] Populating ItemDatabase with generated assets..." -ForegroundColor Yellow
Write-Host "      Method: Tartaria.Editor.DataAssetGenerator.PopulateItemDatabase" -ForegroundColor Gray

$args2 = @(
    "-batchmode"
    "-projectPath", $projectPath
    "-executeMethod", "Tartaria.Editor.DataAssetGenerator.PopulateItemDatabase"
    "-logFile", $logPath
    "-quit"
)

$process2 = Start-Process -FilePath $unityPath -ArgumentList $args2 -Wait -PassThru -NoNewWindow

# Show relevant log lines
if (Test-Path $logPath) {
    Write-Host ""
    Write-Host "-- Build Log ([Tartaria] / [DataAssetGenerator] lines) --" -ForegroundColor DarkGray
    Get-Content $logPath -Tail 100 | ForEach-Object {
        if ($_ -match "\[Tartaria\]|\[DataAssetGenerator\]") {
            if ($_ -match "FAIL|ERROR|CRASH") {
                Write-Host "  $_" -ForegroundColor Red
            } elseif ($_ -match "OK|PASSED|complete|SUCCESS|Added") {
                Write-Host "  $_" -ForegroundColor Green
            } else {
                Write-Host "  $_" -ForegroundColor Cyan
            }
        }
    }
    Write-Host "-- End --" -ForegroundColor DarkGray
    Write-Host ""
}

if ($process2.ExitCode -eq 0) {
    Write-Host "      ✓ Population PASSED" -ForegroundColor Green
} else {
    Write-Host "      ✗ Population FAILED (exit code $($process2.ExitCode))" -ForegroundColor Red
    Write-Host ""
    Write-Host "Full log: $logPath" -ForegroundColor Yellow
    exit $process2.ExitCode
}

# Success summary
Write-Host ""
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host " Assets generated successfully" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Created assets in Assets/_Project/Resources/:" -ForegroundColor White
Write-Host "  • ItemDatabase.asset" -ForegroundColor Gray
Write-Host "  • Items/ (10 consumables)" -ForegroundColor Gray
Write-Host "  • Equipment/ (10 pieces)" -ForegroundColor Gray
Write-Host "  • Enemies/ (5 data assets)" -ForegroundColor Gray
Write-Host ""
Write-Host "Vertical slice now unblocked — inventory/equipment systems ready" -ForegroundColor Cyan
Write-Host ""
