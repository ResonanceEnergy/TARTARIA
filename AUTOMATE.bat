@echo off
REM AUTOMATE.bat — one-click runner for Tools\automate-lfs-and-push.ps1
REM Captures ALL output (stdout + stderr) to Tools\automate-last-run.log
REM so the result is readable even if the cmd window closes fast.

cd /d "%~dp0"

set LOGFILE=%~dp0Tools\automate-last-run.log
echo ===================================================================== > "%LOGFILE%"
echo  AUTOMATE.bat run @ %DATE% %TIME% >> "%LOGFILE%"
echo ===================================================================== >> "%LOGFILE%"

echo.
echo Running automate-lfs-and-push.ps1 ...
echo (output also captured to Tools\automate-last-run.log)
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\automate-lfs-and-push.ps1" >> "%LOGFILE%" 2>&1
set RC=%ERRORLEVEL%

echo. >> "%LOGFILE%"
echo --- exit code: %RC% --- >> "%LOGFILE%"

type "%LOGFILE%"

echo.
if %RC% NEQ 0 (
    echo *** Script exited with code %RC%
) else (
    echo *** Script succeeded
)
echo.
echo Log saved to: %LOGFILE%
echo.
echo Press any key to close...
pause >nul
