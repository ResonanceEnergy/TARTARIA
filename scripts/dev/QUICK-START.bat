@echo off
cd /d C:\dev\TARTARIA_new

echo.
echo ================================================================
echo            TARTARIA - QUICK START
echo ================================================================
echo.
echo This will:
echo   1. Run pre-flight checks
echo   2. Launch Unity with instructions
echo.
pause

echo.
echo Running pre-flight check...
echo.
powershell -ExecutionPolicy Bypass -File "Preflight-Check.ps1"

echo.
echo.
echo Press any key to launch Unity...
pause > nul

echo.
echo Launching Unity...
echo.
powershell -ExecutionPolicy Bypass -File "Launch-Unity.ps1"
