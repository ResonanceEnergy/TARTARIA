# Asset Replacement Pipeline Runner
# Executes Unity Editor menu command to generate production assets
param(
    [switch]$Headless = $false
)

$ErrorActionPreference = "Stop"
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = $PWD.Path

Write-Host "=== TARTARIA Asset Replacement Pipeline ===" -ForegroundColor Cyan
Write-Host "Unity: $unityPath" -ForegroundColor Gray
Write-Host "Project: $projectPath" -ForegroundColor Gray
Write-Host ""

if ($Headless) {
    Write-Host "Running in BATCH MODE (headless)..." -ForegroundColor Yellow
    & "$unityPath" `
        -batchmode `
        -nographics `
        -projectPath "$projectPath" `
        -executeMethod "Tartaria.Editor.AssetGen.AssetReplacementPipeline.RunFullPipeline" `
        -logFile "Logs\asset-replacement.log" `
        -quit

    Write-Host ""
    Write-Host "Batch execution complete. Checking log..." -ForegroundColor Cyan
    
    if (Test-Path "Logs\asset-replacement.log") {
        Write-Host ""
        Write-Host "=== Last 30 lines of log ===" -ForegroundColor Yellow
        Get-Content "Logs\asset-replacement.log" -Tail 30
    }
} else {
    Write-Host "Running in GUI MODE (Unity Editor will open)..." -ForegroundColor Yellow
    Write-Host "After Unity opens, go to: Tartaria > Asset Replacement > RUN FULL PIPELINE" -ForegroundColor Green
    Write-Host ""
    
    Start-Process -FilePath $unityPath -ArgumentList "-projectPath `"$projectPath`""
}

Write-Host ""
Write-Host "✓ Asset replacement pipeline launched" -ForegroundColor Green
