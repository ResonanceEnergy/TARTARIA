@echo off
REM Double-click launcher for the TARTARIA Windows autonomous loop.
REM
REM Usage:
REM   RUN_LOOP.bat                 — one batch of tickets
REM   RUN_LOOP.bat unity           — + Unity smoke shot after batch
REM   RUN_LOOP.bat all             — + Unity + Blender batch
REM   RUN_LOOP.bat continuous      — loop forever (Ctrl+C to stop)
REM
REM Created 2026-06-05.

setlocal
cd /d "%~dp0..\..\.."

echo ================================================================
echo  TARTARIA local-laptop autonomous loop
echo ================================================================
echo.

REM Detect arg
set MODE=%~1
if "%MODE%"=="" goto :basic
if /i "%MODE%"=="unity" goto :unity
if /i "%MODE%"=="all" goto :all
if /i "%MODE%"=="continuous" goto :continuous

:basic
echo Mode: tickets-only
pwsh -ExecutionPolicy Bypass -File "tools\local-llm\win\run_loop.ps1"
goto :end

:unity
echo Mode: tickets + Unity smoke shot
pwsh -ExecutionPolicy Bypass -File "tools\local-llm\win\run_loop.ps1" -RunUnity
goto :end

:all
echo Mode: tickets + Unity + Blender
pwsh -ExecutionPolicy Bypass -File "tools\local-llm\win\run_loop.ps1" -RunUnity -RunBlender
goto :end

:continuous
echo Mode: continuous loop ^(Ctrl+C to stop^)
pwsh -ExecutionPolicy Bypass -File "tools\local-llm\win\run_loop.ps1" -RunUnity -RunBlender -Continuous
goto :end

:end
echo.
echo Done. Outputs in tools\local-llm\LOCAL_OUTPUTS\
echo Logs   in Logs\local-llm\
pause
