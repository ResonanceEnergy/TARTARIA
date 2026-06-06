# Open TARTARIA Unity Project
# Locates Unity Hub and opens the project directly

param(
    [switch]$BatchMode,
    [switch]$ExecuteMethod
)

$ErrorActionPreference = "Stop"
cd C:\dev\TARTARIA_new

Write-Host "`n=== OPENING TARTARIA IN UNITY ===" -ForegroundColor Cyan

# Locate Unity Hub
$unityHubPaths = @(
    "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
    "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe",
    "C:\Program Files\Unity Hub\Unity Hub.exe"
)

$unityHubExe = $null
foreach ($path in $unityHubPaths) {
    if (Test-Path $path) {
        $unityHubExe = $path
        break
    }
}

if (-not $unityHubExe) {
    Write-Host "❌ Unity Hub not found!" -ForegroundColor Red
    Write-Host "Please install Unity Hub from: https://unity.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Found Unity Hub: $unityHubExe" -ForegroundColor Green

# Open project via Unity Hub protocol
$projectPath = "C:\dev\TARTARIA_new"
Write-Host "Opening project: $projectPath" -ForegroundColor Gray

if ($BatchMode) {
    # Locate Unity Editor executable
    $unityEditorPaths = @(
        "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\6000.0.0f1\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\2023.2.0f1\Editor\Unity.exe"
    )

    $unityExe = $null
    foreach ($path in $unityEditorPaths) {
        if (Test-Path $path) {
            $unityExe = $path
            break
        }
    }

    if ($unityExe) {
        Write-Host "Running Unity in batch mode..." -ForegroundColor Yellow
        & $unityExe -batchmode -projectPath $projectPath -quit
    } else {
        Write-Host "Unity Editor not found for batch mode" -ForegroundColor Red
    }
} else {
    # Open via Unity Hub (normal mode)
    Start-Process $unityHubExe -ArgumentList "unityhub://$(($projectPath -replace '\\', '/'))"

    Write-Host "`n✓ Unity Hub opening TARTARIA project..." -ForegroundColor Green
    Write-Host "`nWhat happens next:" -ForegroundColor Yellow
    Write-Host "  1. Unity Hub window opens" -ForegroundColor White
    Write-Host "  2. Unity Editor launches (~30 seconds)" -ForegroundColor White
    Write-Host "  3. Asset import runs automatically (~2 minutes)" -ForegroundColor White
    Write-Host "     Status bar shows: 'Importing Assets...'" -ForegroundColor Gray
    Write-Host "  4. Dialog appears: 'TARTARIA Asset Import Detected'" -ForegroundColor White
    Write-Host "     Click: 'Yes, Automate Everything!'" -ForegroundColor Cyan
    Write-Host "  5. Automation runs: Prefabs → Test Scene → Report" -ForegroundColor White
    Write-Host "  6. Dialog: 'Would you like to open test scene?'" -ForegroundColor White
    Write-Host "     Click: 'Yes, Open Scene!'" -ForegroundColor Cyan
    Write-Host "  7. Press Play button (▶) to walk through Star Dome!" -ForegroundColor White
    Write-Host "`n⏱️  Total time: ~3 minutes from now to playing" -ForegroundColor Gray
}
