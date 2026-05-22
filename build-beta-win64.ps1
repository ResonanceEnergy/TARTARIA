# ══════════════════════════════════════════════════════════════════
# TARTARIA Beta Build — Windows x64
# ══════════════════════════════════════════════════════════════════

param(
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.32f1\Editor\Unity.exe",
    [string]$BuildTarget = "Builds/TARTARIA_Beta_v0.9/TARTARIA.exe"
)

Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host "   TARTARIA — Beta Build v0.9 (Windows x64)"
Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host ""

$projectPath = $PSScriptRoot
$buildPath = Join-Path $projectPath $BuildTarget
$buildDir = Split-Path -Parent $buildPath
$logFile = Join-Path $projectPath "Logs\beta-build-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"

# Ensure build directory exists
if (!(Test-Path $buildDir)) {
    New-Item -ItemType Directory -Path $buildDir -Force | Out-Null
}

Write-Host "Project:  $projectPath"
Write-Host "Build to: $buildPath"
Write-Host "Log:      $logFile"
Write-Host ""
Write-Host "Starting Unity build (this may take 10-20 minutes)..."
Write-Host ""

# Build via Unity batchmode
$args = @(
    "-batchmode",
    "-nographics",
    "-quit",
    "-projectPath", $projectPath,
    "-buildWindows64Player", $buildPath,
    "-logFile", $logFile
)

$process = Start-Process -FilePath $UnityPath -ArgumentList $args -NoNewWindow -PassThru -Wait

Write-Host ""
if ($process.ExitCode -eq 0) {
    Write-Host "✓ BUILD SUCCESS"
    Write-Host ""
    Write-Host "Build location: $buildDir"
    Write-Host "Executable:     $buildPath"
    
    # Check build size
    if (Test-Path $buildPath) {
        $size = (Get-Item $buildPath).Length / 1MB
        Write-Host "Exe size:       $([math]::Round($size, 2)) MB"
    }
    
    # Check total build folder size
    $totalSize = (Get-ChildItem $buildDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "Total size:     $([math]::Round($totalSize, 2)) MB"
    
    if ($totalSize -gt 2048) {
        Write-Host ""
        Write-Host "⚠ WARNING: Build size exceeds 2GB target ($([math]::Round($totalSize, 2)) MB)"
    }
    
    exit 0
} else {
    Write-Host "✗ BUILD FAILED (exit code $($process.ExitCode))"
    Write-Host ""
    Write-Host "Check log: $logFile"
    exit 1
}
