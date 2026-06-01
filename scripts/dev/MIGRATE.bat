@echo off
REM MIGRATE.bat - one-click LFS history migration (DESTRUCTIVE, force-pushes)
REM Calls Tools\lfs-migrate-history.ps1 -Force which:
REM   1. Backs up .git/ to .git.backup-YYYYMMDD-HHMMSS
REM   2. Rewrites every commit to route matching binaries through LFS
REM   3. Force-pushes all branches and tags to origin
REM
REM Output captured to Tools\migrate-last-run.log
REM
REM SAFETY: Solo dev = generally safe. Other clones must re-clone after this.
REM         Backup folder stays around so you can roll back if anything breaks.

cd /d "%~dp0"

set LOGFILE=%~dp0Tools\migrate-last-run.log
echo ===================================================================== > "%LOGFILE%"
echo  MIGRATE.bat run @ %DATE% %TIME% >> "%LOGFILE%"
echo ===================================================================== >> "%LOGFILE%"

echo.
echo Running lfs-migrate-history.ps1 -Force ...
echo (output also captured to Tools\migrate-last-run.log)
echo This can take 5-15 minutes for a 700+ MB repo. Do not interrupt.
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Tools\lfs-migrate-history.ps1" -Force >> "%LOGFILE%" 2>&1
set RC=%ERRORLEVEL%

echo. >> "%LOGFILE%"
echo --- exit code: %RC% --- >> "%LOGFILE%"

type "%LOGFILE%"

echo.
if %RC% NEQ 0 (
    echo *** Script exited with code %RC%
    echo *** Restore .git from the .git.backup-* folder if needed.
) else (
    echo *** Migration complete - history rewritten and force-pushed
)
echo.
echo Log saved to: %LOGFILE%
echo.
echo Press any key to close...
pause >nul
