# ══════════════════════════════════════════════════════════════════
# TARTARIA Beta Build — Monitor & Verify
# ══════════════════════════════════════════════════════════════════
# Run this script to monitor the ongoing build and verify completion.

param(
    [switch]$Wait
)

Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host "   TARTARIA Beta Build Monitor"
Write-Host "══════════════════════════════════════════════════════════════════"
Write-Host ""

$buildExe = "Build\Windows\Tartaria.exe"
$logFile = "Logs\beta-build-20260522-110958.log"

if ($Wait) {
    Write-Host "Waiting for Unity build to complete...`n"
    while (Get-Process Unity -ErrorAction SilentlyContinue) {
        $elapsed = [math]::Round(((Get-Date) - (Get-Process Unity).StartTime).TotalMinutes, 1)
        Write-Host "  Unity building... ($elapsed min)" -NoNewline
        Start-Sleep -Seconds 30
        Write-Host " [$(Get-Date -Format 'HH:mm:ss')]"
    }
    Write-Host "`n✓ Unity finished`n"
    Start-Sleep -Seconds 3
}

# Check build status
if (Test-Path $buildExe) {
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host "    ✓✓✓ BUILD SUCCESS! ✓✓✓" -ForegroundColor Green
    Write-Host "════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    
    $exe = Get-Item $buildExe
    Write-Host "Executable:  $($exe.FullName)"
    Write-Host "Exe size:    $([math]::Round($exe.Length / 1MB, 2)) MB"
    Write-Host "Created:     $($exe.LastWriteTime)"
    Write-Host ""
    
    $buildDir = Split-Path -Parent $buildExe
    $totalSize = (Get-ChildItem $buildDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
    $fileCount = (Get-ChildItem $buildDir -Recurse).Count
    
    Write-Host "Total size:  $([math]::Round($totalSize, 2)) MB"
    Write-Host "File count:  $fileCount files"
    Write-Host ""
    
    if (Test-Path $logFile) {
        $warnings = (Get-Content $logFile | Select-String -Pattern "warning CS").Count
        $errors = (Get-Content $logFile | Select-String -Pattern "error CS").Count
        Write-Host "Warnings:    $warnings"
        Write-Host "Errors:      $errors"
    }
    
    Write-Host ""
    if ($totalSize -gt 2048) {
        Write-Host "⚠ Warning: Build size exceeds 2GB target" -ForegroundColor Yellow
    } else {
        Write-Host "✓ Build size within target (<2GB)" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "✓ Beta build ready for distribution!" -ForegroundColor Cyan
    Write-Host ""
    exit 0
    
} elseif (Get-Process Unity -ErrorAction SilentlyContinue) {
    $elapsed = [math]::Round(((Get-Date) - (Get-Process Unity).StartTime).TotalMinutes, 1)
    Write-Host "⏳ Build still in progress ($elapsed minutes elapsed)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Run with -Wait flag to monitor: .\monitor-beta-build.ps1 -Wait"
    Write-Host ""
    exit 2
    
} else {
    Write-Host "✗ BUILD FAILED" -ForegroundColor Red
    Write-Host ""
    
    if (Test-Path $logFile) {
        Write-Host "Checking log file...`n"
        $log = Get-Content $logFile
        
        $buildMsgs = $log | Select-String -Pattern "\[Build|BUILD" | Select-Object -Last 10
        if ($buildMsgs.Count -gt 0) {
            Write-Host "Build messages:" -ForegroundColor Yellow
            $buildMsgs | ForEach-Object { Write-Host "  $($_.Line)" }
            Write-Host ""
        }
        
        $errors = $log | Select-String -Pattern "error|failed" -Context 1 | Select-Object -Last 10
        if ($errors.Count -gt 0) {
            Write-Host "Errors:" -ForegroundColor Red
            $errors | ForEach-Object { Write-Host "  $($_.Line)" }
        }
    } else {
        Write-Host "Log file not found: $logFile"
    }
    
    Write-Host ""
    exit 1
}
