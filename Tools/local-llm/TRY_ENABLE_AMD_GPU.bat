@echo off
REM Attempt to enable AMD Radeon Graphics (integrated APU) for Ollama.
REM Sets HSA_OVERRIDE_GFX_VERSION to a value Ollama can use for iGPUs,
REM tells Ollama to load all model layers on GPU, then restarts the Ollama service.
REM Wait until your CURRENT runner finishes before running this.

setlocal
echo === ATTEMPTING AMD GPU ENABLE FOR OLLAMA ===
echo.
echo Before: confirming current state
ollama ps
echo.

echo --- Setting env vars for current user (persistent) ---
REM 11.0.0 for newer integrated (RDNA3 Phoenix/Hawk Point Radeon Graphics)
REM 10.3.0 for older RDNA2 integrated
REM Start with 11.0.0; if it fails switch to 10.3.0 manually
setx HSA_OVERRIDE_GFX_VERSION "11.0.0"
setx OLLAMA_GPU_LAYERS "999"
setx OLLAMA_NUM_GPU "999"
echo.

echo --- Restarting Ollama (kill + start) ---
echo This will close any running Ollama processes. Make sure the runner cmd window
echo is FINISHED (says "Done. Review outputs in...") before continuing.
echo.
pause

taskkill /F /IM ollama.exe /T 2>nul
taskkill /F /IM "ollama app.exe" /T 2>nul
timeout /t 2 /nobreak >nul

echo Starting Ollama with new env vars...
start "" "%LOCALAPPDATA%\Programs\Ollama\ollama app.exe"
timeout /t 5 /nobreak >nul

echo.
echo === After restart, Ollama PS should show GPU instead of CPU ===
ollama ps
echo.

echo If PROCESSOR still says "100%% CPU", AMD iGPU isn't being picked up.
echo Try:
echo   1. Restart your computer ^(env vars only fully take effect after reboot^)
echo   2. Or change HSA_OVERRIDE_GFX_VERSION to "10.3.0" and retry
echo   3. Or stick with CPU and use a smaller model: ollama pull qwen2.5-coder:1.5b
echo.
pause
