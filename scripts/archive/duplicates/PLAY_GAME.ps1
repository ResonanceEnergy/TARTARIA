#!/usr/bin/env pwsh
# TARTARIA - Force Play Mode Entry
# Kills current Unity, reopens, and enters Play mode via -executeMethod

$ProjectPath = "C:\dev\TARTARIA_new"
$UnityPath = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe"

Write-Host "`n═══════════════════════════════════" -ForegroundColor Cyan
Write-Host "  TARTARIA FORCE PLAY MODE" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "Killing existing Unity..." -ForegroundColor Yellow
Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 3

Write-Host "Launching Unity with Play mode command...`n" -ForegroundColor Yellow
& $UnityPath -projectPath $ProjectPath -executeMethod "Tartaria.Editor.CommandLinePlayMode.EnterPlayModeFromCommandLine"

Write-Host "`nUnity launching. Wait ~60s for Play mode." -ForegroundColor Green
Write-Host "Watch Unity Console for [PlayerSpawner], [GameState] logs`n" -ForegroundColor White
