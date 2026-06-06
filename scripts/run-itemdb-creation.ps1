# AGENT 2: ItemDatabase Asset Creation Runner
# Executes ItemDatabasePopulator in Unity to create 40 item/equipment assets

param(
    [switch]$OpenEditor = $false
)

cd C:\dev\TARTARIA_new

$unityExe = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
$projectPath = "C:\dev\TARTARIA_new"
$logFile = "Logs\ItemDatabasePopulator.log"

Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  AGENT 2: ItemDatabase Asset Creation" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Target: 40 assets (20 consumables, 10 equipment, 10 materials)" -ForegroundColor White
Write-Host "Mode: " -NoNewline
if ($OpenEditor) {
    Write-Host "Open Unity Editor (manual execution)" -ForegroundColor Green
} else {
    Write-Host "Batchmode (automated execution)" -ForegroundColor Cyan
}
Write-Host ""

if (!(Test-Path $unityExe)) {
    Write-Host "✗ Unity not found at: $unityExe" -ForegroundColor Red
    exit 1
}

# Create Logs folder if missing
if (!(Test-Path "Logs")) {
    New-Item -ItemType Directory -Path "Logs" | Out-Null
}

if ($OpenEditor) {
    # Open Unity Editor for manual execution
    Write-Host "Opening Unity Editor..." -ForegroundColor Cyan
    Write-Host "Manual steps:" -ForegroundColor Yellow
    Write-Host "  1. Wait for Unity to load" -ForegroundColor White
    Write-Host "  2. Go to: Tartaria > Build Assets > Item Database (Complete)" -ForegroundColor White
    Write-Host "  3. Click menu item to generate assets" -ForegroundColor White
    Write-Host "  4. Wait for completion dialog" -ForegroundColor White
    Write-Host ""

    Start-Process $unityExe -ArgumentList "-projectPath", $projectPath
    Write-Host "✓ Unity Editor launched" -ForegroundColor Green
} else {
    # Batchmode execution
    Write-Host "Running Unity in batchmode..." -ForegroundColor Cyan
    Write-Host "This will:" -ForegroundColor White
    Write-Host "  - Create 20 consumable items" -ForegroundColor White
    Write-Host "  - Create 10 equipment items" -ForegroundColor White
    Write-Host "  - Create 10 material items" -ForegroundColor White
    Write-Host "  - Skip existing assets" -ForegroundColor White
    Write-Host ""
    Write-Host "Log file: $logFile" -ForegroundColor DarkGray
    Write-Host ""

    $args = @(
        "-batchmode",
        "-nographics",
        "-projectPath", $projectPath,
        "-executeMethod", "Tartaria.Editor.ItemDatabasePopulator.ExecuteBatchMode",
        "-logFile", $logFile,
        "-quit"
    )

    Write-Host "Executing..." -ForegroundColor Yellow
    $process = Start-Process -FilePath $unityExe -ArgumentList $args -Wait -PassThru

    Write-Host ""
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ Asset creation complete!" -ForegroundColor Green
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host "  NEXT STEPS" -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "1. Open Unity Editor: .\run-itemdb-creation.ps1 -OpenEditor" -ForegroundColor White
        Write-Host "2. Select: Assets/_Project/Resources/ItemDatabase.asset" -ForegroundColor White
        Write-Host "3. In Inspector: Click 'Auto-Populate from Assets' button" -ForegroundColor White
        Write-Host "4. Verify item count in Console window" -ForegroundColor White
        Write-Host ""
        Write-Host "Or run auto-populate in batchmode:" -ForegroundColor White
        Write-Host "  (TODO: Add separate auto-populate batchmode command)" -ForegroundColor DarkGray
        Write-Host ""

        # Show last 20 lines of log
        if (Test-Path $logFile) {
            Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
            Write-Host "  LOG TAIL (last 20 lines)" -ForegroundColor Yellow
            Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
            Get-Content $logFile -Tail 20
        }
    } else {
        Write-Host "✗ Asset creation failed (exit code: $($process.ExitCode))" -ForegroundColor Red
        Write-Host ""
        Write-Host "Check log file: $logFile" -ForegroundColor Yellow
        if (Test-Path $logFile) {
            Write-Host ""
            Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
            Write-Host "  ERROR LOG (last 30 lines)" -ForegroundColor Red
            Write-Host "═══════════════════════════════════════════════" -ForegroundColor Cyan
            Get-Content $logFile -Tail 30
        }
    }
}

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor DarkGray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
