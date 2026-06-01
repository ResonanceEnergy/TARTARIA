@echo off
REM Double-click launcher for the local-LLM Moon 1 ticket queue.
REM Created 2026-05-30 because Claude can't type into PowerShell directly.

setlocal
cd /d "%~dp0..\.."

echo ===============================================================
echo  TARTARIA local-LLM ticket runner
echo ===============================================================
echo.
echo Repo root: %CD%
echo.

REM Check Ollama
where ollama >nul 2>nul
if errorlevel 1 (
    echo [ERROR] ollama is not on PATH.
    echo         Install from https://ollama.com
    pause
    exit /b 1
)

REM Default model — qwen2.5-coder:1.5b is ~1 GB, runs at 30-50 tok/sec on CPU.
REM Quality is adequate for boilerplate C# tickets. Override for bigger model:
REM   RUN_OLLAMA_TICKETS.bat qwen2.5-coder:7b   (~4.5 GB, better quality, 3-5x slower)
REM   RUN_OLLAMA_TICKETS.bat qwen3-coder:30b    (~18 GB, best, needs GPU realistically)
set MODEL=qwen2.5-coder:1.5b
if not "%~1"=="" set MODEL=%~1

echo Model: %MODEL%
echo Tickets directory: tools\local-llm\LOCAL_TASKS\
echo.

REM Check model is pulled; if not, prompt to pull (default Y — just press Enter)
ollama list | findstr /i "%MODEL%" >nul
if errorlevel 1 (
    echo.
    echo [INFO] Model "%MODEL%" not yet pulled.
    echo        ^(qwen2.5-coder:7b is ~4.5 GB — should pull in 2-5 min on a decent connection.^)
    echo.
    set "ANS=Y"
    set /p ANS=Pull it now? [Y/n] (default Y)
    if /i "%ANS%"=="n" (
        echo Aborted by user.
        pause
        exit /b 1
    )
    echo.
    echo Pulling %MODEL% ...
    ollama pull %MODEL%
    if errorlevel 1 (
        echo [ERROR] ollama pull failed.
        pause
        exit /b 1
    )
)

REM Run the launcher
pwsh -ExecutionPolicy Bypass -File "tools\local-llm\Run-LocalLLM.ps1" -Model %MODEL%
if errorlevel 1 (
    echo.
    echo [WARN] Run-LocalLLM.ps1 returned a non-zero exit code.
)

echo.
echo ===============================================================
echo  Outputs are in tools\local-llm\LOCAL_OUTPUTS\
echo  Review each response.md, then copy code blocks to destinations
echo  listed at the top of each ticket .md file.
echo ===============================================================
pause
