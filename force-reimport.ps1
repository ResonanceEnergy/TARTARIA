#!/usr/bin/env pwsh
# force-reimport.ps1 - Force Unity to refresh asset database and recompile all scripts
# Usage: .\force-reimport.ps1

cd C:\dev\TARTARIA_new

Write-Host "`n═══ UNITY FORCE REIMPORT & RECOMPILE ═══`n" -ForegroundColor Cyan

# 1. Check if Unity is running - MUST close it first
Write-Host "1. Checking for running Unity instances..." -ForegroundColor Yellow
$unity = Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -match "TARTARIA" }
if ($unity) {
    Write-Host "  ✗ Unity is running (PID $($unity.Id))" -ForegroundColor Red
    Write-Host "  Please close Unity and run this script again." -ForegroundColor Yellow
    Write-Host "  Press any key to exit..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    exit 1
}
Write-Host "  √ Unity not running`n" -ForegroundColor Green

# 2. Backup current state
Write-Host "2. Creating backup..." -ForegroundColor Yellow
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "Library\Backup_$timestamp"
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

if (Test-Path "Library\ScriptAssemblies") {
    Copy-Item -Path "Library\ScriptAssemblies" -Destination "$backupDir\ScriptAssemblies" -Recurse -ErrorAction SilentlyContinue
    Write-Host "  √ Backed up ScriptAssemblies to $backupDir" -ForegroundColor Green
}

if (Test-Path "Library\Bee\artifacts\1900b0aE.dag") {
    Copy-Item -Path "Library\Bee\artifacts\1900b0aE.dag" -Destination "$backupDir\Bee_artifacts" -Recurse -ErrorAction SilentlyContinue
    Write-Host "  √ Backed up Bee artifacts to $backupDir`n" -ForegroundColor Green
}

# 3. Delete compiled assemblies
Write-Host "3. Deleting compiled assemblies..." -ForegroundColor Yellow
if (Test-Path "Library\ScriptAssemblies") {
    $dllCount = (Get-ChildItem "Library\ScriptAssemblies\*.dll" -ErrorAction SilentlyContinue).Count
    Remove-Item "Library\ScriptAssemblies\*.dll" -Force -ErrorAction SilentlyContinue
    Remove-Item "Library\ScriptAssemblies\*.pdb" -Force -ErrorAction SilentlyContinue
    Write-Host "  √ Deleted $dllCount DLL files from ScriptAssemblies" -ForegroundColor Green
}

# 4. Delete Bee cache for Editor assembly
Write-Host "`n4. Deleting Bee cache..." -ForegroundColor Yellow
if (Test-Path "Library\Bee\artifacts\1900b0aE.dag") {
    $editorFiles = Get-ChildItem "Library\Bee\artifacts\1900b0aE.dag\Tartaria.Scripts.Editor*" -ErrorAction SilentlyContinue
    if ($editorFiles) {
        $editorFiles | Remove-Item -Force -ErrorAction SilentlyContinue
        Write-Host "  √ Deleted $($editorFiles.Count) Tartaria.Scripts.Editor files from Bee cache" -ForegroundColor Green
    }
}

# 5. Touch all Editor scripts to trigger reimport
Write-Host "`n5. Touching Editor scripts to trigger reimport..." -ForegroundColor Yellow
$editorScripts = Get-ChildItem "Assets\_Project\Scripts\Editor\*.cs" -Recurse -ErrorAction SilentlyContinue
if ($editorScripts) {
    $touchCount = 0
    foreach ($script in $editorScripts) {
        $script.LastWriteTime = Get-Date
        $touchCount++
    }
    Write-Host "  √ Touched $touchCount Editor scripts" -ForegroundColor Green
} else {
    Write-Host "  ? No Editor scripts found" -ForegroundColor Yellow
}

# 6. Delete asset database cache
Write-Host "`n6. Deleting asset database cache..." -ForegroundColor Yellow
if (Test-Path "Library\SourceAssetDB") {
    Remove-Item "Library\SourceAssetDB" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  √ Deleted SourceAssetDB" -ForegroundColor Green
}
if (Test-Path "Library\ArtifactDB") {
    Remove-Item "Library\ArtifactDB" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  √ Deleted ArtifactDB" -ForegroundColor Green
}

# 7. Restart Unity with reimport
Write-Host "`n7. Restarting Unity to reimport and recompile..." -ForegroundColor Yellow
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.0.33f1\Editor\Unity.exe"
if (-not (Test-Path $unityPath)) {
    Write-Host "  ✗ Unity not found at $unityPath" -ForegroundColor Red
    Write-Host "  Please update the path in this script and run again." -ForegroundColor Yellow
    exit 1
}

Write-Host "  Starting Unity with -quit flag to force reimport..." -ForegroundColor Gray
Write-Host "  This will take 2-5 minutes. Please wait...`n" -ForegroundColor Gray

$logFile = "Library\unity_reimport_$timestamp.log"
$process = Start-Process -FilePath $unityPath `
    -ArgumentList "-projectPath `"C:\dev\TARTARIA_new`" -quit -batchmode -logFile `"$logFile`"" `
    -PassThru `
    -NoNewWindow

Write-Host "  Unity PID: $($process.Id)" -ForegroundColor Gray
Write-Host "  Log file: $logFile`n" -ForegroundColor Gray

# Wait for Unity to finish
$process.WaitForExit()
$exitCode = $process.ExitCode

if ($exitCode -eq 0) {
    Write-Host "  √ Unity reimport completed successfully!" -ForegroundColor Green
} else {
    Write-Host "  ✗ Unity exited with code $exitCode" -ForegroundColor Red
    Write-Host "  Check log file for details: $logFile" -ForegroundColor Yellow
}

# 8. Verify recompilation
Write-Host "`n8. Verifying recompilation..." -ForegroundColor Yellow
if (Test-Path "Library\ScriptAssemblies\Tartaria.Scripts.Editor.dll") {
    $dll = Get-Item "Library\ScriptAssemblies\Tartaria.Scripts.Editor.dll"
    $age = (Get-Date) - $dll.LastWriteTime
    Write-Host "  √ Tartaria.Scripts.Editor.dll found" -ForegroundColor Green
    Write-Host "  Last compiled: $($dll.LastWriteTime)" -ForegroundColor Gray
    Write-Host "  Age: $([Math]::Round($age.TotalMinutes, 1)) minutes" -ForegroundColor Gray
    
    if ($age.TotalMinutes -lt 10) {
        Write-Host "  √ Fresh compilation!" -ForegroundColor Green
    }
} else {
    Write-Host "  ✗ Tartaria.Scripts.Editor.dll not found - compilation may have failed" -ForegroundColor Red
    Write-Host "  Check Unity console after opening the project" -ForegroundColor Yellow
}

Write-Host "`n═══ REIMPORT COMPLETE ═══`n" -ForegroundColor Cyan
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Open Unity normally" -ForegroundColor White
Write-Host "  2. Check Console for any remaining errors" -ForegroundColor White
Write-Host "  3. If errors persist, check: $logFile`n" -ForegroundColor White
Write-Host "Backup location: $backupDir`n" -ForegroundColor Gray

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
