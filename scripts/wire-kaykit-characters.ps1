# KayKit Character Wiring Script
# Wires all companion/enemy characters to KayKit models

Write-Host "=== KAYKIT CHARACTER WIRING ===" -ForegroundColor Cyan
Write-Host ""

$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = $PWD.Path
$logFile = "Logs\kaykit-wiring-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

if (-not (Test-Path $unityPath)) {
    Write-Host "ERROR: Unity not found at $unityPath" -ForegroundColor Red
    exit 1
}

Write-Host "Unity: $unityPath" -ForegroundColor Gray
Write-Host "Project: $projectPath" -ForegroundColor Gray
Write-Host "Log: $logFile" -ForegroundColor Gray
Write-Host ""

Write-Host "Starting Unity batch process..." -ForegroundColor Yellow

$proc = Start-Process -FilePath $unityPath -ArgumentList @(
    "-projectPath", $projectPath,
    "-executeMethod", "Tartaria.Editor.KayKitWiringBatch.RunWiring",
    "-batchmode",
    "-quit",
    "-logFile", $logFile
) -PassThru -NoNewWindow

Write-Host "Unity PID: $($proc.Id)" -ForegroundColor Gray
Write-Host "Waiting for Unity to complete..." -ForegroundColor Yellow
Write-Host ""

# Wait for Unity to finish
$proc.WaitForExit()

$exitCode = $proc.ExitCode

Write-Host ""
Write-Host "Unity exit code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { 'Green' } else { 'Red' })
Write-Host ""

# Show log results
if (Test-Path $logFile) {
    Write-Host "=== WIRING LOG ===" -ForegroundColor Cyan
    $logContent = Get-Content $logFile -Raw
    
    # Extract wiring results
    $wiringLines = $logContent -split "`n" | Where-Object { $_ -match '\[KayKitWiring\]' }
    
    if ($wiringLines.Count -gt 0) {
        $wiringLines | ForEach-Object { Write-Host $_ }
    } else {
        Write-Host "No wiring messages found in log" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Last 50 lines of log:" -ForegroundColor Gray
        Get-Content $logFile -Tail 50
    }
    
    Write-Host ""
    
    # Check for errors
    $errors = $logContent -split "`n" | Where-Object { $_ -match 'error|exception|failed' -and $_ -notmatch 'error CS' }
    if ($errors.Count -gt 0) {
        Write-Host "=== ERRORS ===" -ForegroundColor Red
        $errors | Select-Object -First 20 | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    }
} else {
    Write-Host "ERROR: Log file not created" -ForegroundColor Red
    exit 1
}

Write-Host ""

if ($exitCode -eq 0) {
    Write-Host "✓ Character wiring complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "1. Open Unity Editor" -ForegroundColor Gray
    Write-Host "2. Open Echohaven_VerticalSlice scene" -ForegroundColor Gray
    Write-Host "3. Press Play to verify Milo spawns as Ranger model" -ForegroundColor Gray
    Write-Host "4. Check other characters in Characters/ prefabs folder" -ForegroundColor Gray
} else {
    Write-Host "✗ Wiring failed - check log for details" -ForegroundColor Red
    exit 1
}
