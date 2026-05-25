# TARTARIA M3 Gates — Performance Validation + Standalone Build
# Run after closing Unity Editor
# Dr. Vex Aurelian — Session 5 automation

$ErrorActionPreference = "Stop"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe"
$ProjectPath = "C:\dev\TARTARIA_new"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " TARTARIA M3 Gates — Perf + Build" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Check Unity not running
$unityProc = Get-Process Unity -ErrorAction SilentlyContinue
if ($unityProc) {
    Write-Host "ERROR: Unity Editor is running (PID $($unityProc.Id))" -ForegroundColor Red
    Write-Host "Close Unity Editor first, then re-run this script." -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity Editor not running -- proceeding with M3 gates" -ForegroundColor Green
Write-Host ""

# M3.1: Performance Gates
Write-Host ">> M3.1: Running PerformanceGateRunner (batchmode)" -ForegroundColor Yellow
$logPath = "$ProjectPath\Logs\perf-gate-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$perfArgs = @(
    "-projectPath", $ProjectPath,
    "-executeMethod", "Tartaria.Editor.Perf.PerformanceGateRunner.RunCIGates",
    "-batchmode",
    "-quit",
    "-logFile", $logPath
)

Write-Host "Unity command: $UnityPath $($perfArgs -join ' ')" -ForegroundColor DarkGray
Write-Host "Log: $logPath" -ForegroundColor DarkGray
Write-Host ""

$perfStart = Get-Date
& $UnityPath @perfArgs

$perfDuration = (Get-Date) - $perfStart
$perfExitCode = $LASTEXITCODE

if ($perfExitCode -eq 0) {
    Write-Host "   OK  Performance gates PASSED ($([math]::Round($perfDuration.TotalSeconds, 1))s)" -ForegroundColor Green
} else {
    Write-Host "   FAIL  Performance gates exit code: $perfExitCode" -ForegroundColor Red
    Write-Host "Check log: $logPath" -ForegroundColor Yellow
    exit $perfExitCode
}

# Check for perf results JSON
$resultFiles = Get-ChildItem "$ProjectPath\Assets\_Project\Generated\CI_Results\R6_PerfGates_*.json" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($resultFiles) {
    Write-Host "   Results: $($resultFiles.FullName)" -ForegroundColor Cyan
    $json = Get-Content $resultFiles.FullName -Raw | ConvertFrom-Json
    Write-Host "   Tier: $($json.tier), Avg FPS: $($json.avgFps), 1%Low: $($json.onePercentLow), Peak RAM: $($json.peakMemoryMB) MB" -ForegroundColor Cyan
} else {
    Write-Host "   WARNING: No perf results JSON found" -ForegroundColor Yellow
}

Write-Host ""

# M3.2: Standalone Build
Write-Host ">> M3.2: Building standalone Windows .exe" -ForegroundColor Yellow
$buildLogPath = "$ProjectPath\Logs\build-standalone-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$buildArgs = @(
    "-projectPath", $ProjectPath,
    "-executeMethod", "Tartaria.EditorTools.BuildPlayerPipeline.BuildWindows",
    "-batchmode",
    "-quit",
    "-logFile", $buildLogPath
)

Write-Host "Unity command: $UnityPath $($buildArgs -join ' ')" -ForegroundColor DarkGray
Write-Host "Log: $buildLogPath" -ForegroundColor DarkGray
Write-Host ""

$buildStart = Get-Date
& $UnityPath @buildArgs

$buildDuration = (Get-Date) - $buildStart
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -eq 0) {
    Write-Host "   OK  Standalone build COMPLETED ($([math]::Round($buildDuration.TotalSeconds, 1))s)" -ForegroundColor Green
} else {
    Write-Host "   FAIL  Build exit code: $buildExitCode" -ForegroundColor Red
    Write-Host "Check log: $buildLogPath" -ForegroundColor Yellow
    exit $buildExitCode
}

# Check build output
$exePath = "$ProjectPath\Build\Windows\Tartaria.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    Write-Host "   Build output: $exePath ($([math]::Round($exeSize, 1)) MB)" -ForegroundColor Cyan
} else {
    Write-Host "   WARNING: Build .exe not found at $exePath" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " M3 GATES COMPLETE" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next: Run the standalone build to verify launch" -ForegroundColor Yellow
Write-Host "   .\Build\Windows\Tartaria.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "Then: Proceed to M4 beta package preparation" -ForegroundColor Yellow
