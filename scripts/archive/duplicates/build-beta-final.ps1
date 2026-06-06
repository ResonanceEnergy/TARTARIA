# ══════════════════════════════════════════════════════════════════
# TARTARIA Beta Build — Windows x64 (Mono Backend)
# ══════════════════════════════════════════════════════════════════

param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"
)

Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host "   TARTARIA — Beta Build v0.9 (Windows x64 - Mono)"
Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host ""

$projectPath = $PSScriptRoot
$logFile = Join-Path $projectPath "Logs\beta-build-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
$outputDir = "Build\Windows"
$outputExe = "Build\Windows\Tartaria.exe"

Write-Host "Project:  $projectPath"
Write-Host "Output:   $outputExe"
Write-Host "Log:      $logFile"
Write-Host "Backend:  Mono2x (development settings)"
Write-Host ""
Write-Host "Starting Unity build (10-20 minutes)..."
Write-Host ""

# Build via Unity batchmode calling BetaBuild.BuildMonoStandalone
$args = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $projectPath,
    "-executeMethod", "Tartaria.Build.BetaBuild.BuildMonoStandalone",
    "-logFile", $logFile
)

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$process = Start-Process -FilePath $UnityPath -ArgumentList $args -NoNewWindow -PassThru -Wait
$sw.Stop()

Write-Host ""
Write-Host "Build completed in $([math]::Round($sw.Elapsed.TotalMinutes, 1)) minutes"
Write-Host ""

if ($process.ExitCode -eq 0 -and (Test-Path $outputExe)) {
    Write-Host "════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "    ✓✓✓ BUILD SUCCESS! ✓✓✓" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    
    $exe = Get-Item $outputExe
    Write-Host "Executable:  $($exe.FullName)"
    Write-Host "Exe size:    $([math]::Round($exe.Length / 1MB, 2)) MB"
    Write-Host "Created:     $($exe.LastWriteTime)"
    
    $totalSize = (Get-ChildItem $outputDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Total size:  $([math]::Round($totalSize, 2)) MB"
    
    if ($totalSize -gt 2048) {
        Write-Host ""
        Write-Host "⚠ WARNING: Build exceeds 2GB target" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "✓ Beta build ready for distribution!" -ForegroundColor Cyan
    exit 0
} else {
    Write-Host "✗ BUILD FAILED (exit code $($process.ExitCode))" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check log: $logFile"
    
    # Show errors
    if (Test-Path $logFile) {
        $errors = Get-Content $logFile | Select-String -Pattern "error|Error|failed|Failed|FAILED" | Select-Object -Last 15
        if ($errors.Count -gt 0) {
            Write-Host ""
            Write-Host "Recent errors:" -ForegroundColor Yellow
            $errors | ForEach-Object { Write-Host "  $_" }
        }
    }
    exit 1
}
