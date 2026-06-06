@echo off
REM One-click fix for the Unity Package Manager server being blocked by Defender.
REM Self-elevates, adds the Unity editor folder + UPM exe to Defender exclusions,
REM then drops a flag file so the build agent knows it's done.
net session >nul 2>&1
if %errorlevel% neq 0 (
  echo Requesting administrator approval...
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
powershell -NoProfile -Command "Add-MpPreference -ExclusionPath 'C:\Program Files\Unity\Hub\Editor\6000.3.6f1'; Add-MpPreference -ExclusionProcess 'UnityPackageManager.exe'; Add-MpPreference -ExclusionProcess 'Unity.exe'; New-Item -ItemType File -Force -Path 'C:\dev\TARTARIA_new\Logs\_excl_ok.flag' | Out-Null"
echo.
echo Unity UPM exclusion added. UPM will start now. You can close this window.
timeout /t 4 >nul
