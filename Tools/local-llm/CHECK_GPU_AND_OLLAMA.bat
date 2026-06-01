@echo off
REM Diagnose what GPU is on this machine and whether Ollama can use it.
REM Reports to ../tools/local-llm/GPU_REPORT.txt for Claude to read via bash.

setlocal
cd /d "%~dp0..\.."
set REPORT=tools\local-llm\GPU_REPORT.txt

echo Writing GPU + Ollama diagnostic report... > %REPORT%
echo. >> %REPORT%

echo === GPU INFO (wmic / dxdiag style) === >> %REPORT%
wmic path win32_VideoController get name, AdapterRAM, DriverVersion /format:list >> %REPORT% 2>&1
echo. >> %REPORT%

echo === NVIDIA-SMI (only succeeds on NVIDIA) === >> %REPORT%
where nvidia-smi >nul 2>nul
if %errorlevel%==0 (
    nvidia-smi >> %REPORT% 2>&1
) else (
    echo nvidia-smi not on PATH. ^(NVIDIA driver not installed, or no NVIDIA GPU.^) >> %REPORT%
)
echo. >> %REPORT%

echo === AMD HIP SDK presence === >> %REPORT%
if exist "%ProgramFiles%\AMD\ROCm" (
    dir "%ProgramFiles%\AMD\ROCm" /b >> %REPORT% 2>&1
) else (
    echo No AMD ROCm folder at Program Files\AMD\ROCm >> %REPORT%
)
if exist "%HIP_PATH%" (
    echo HIP_PATH env var: %HIP_PATH% >> %REPORT%
)
echo. >> %REPORT%

echo === OLLAMA STATUS === >> %REPORT%
where ollama >> %REPORT% 2>&1
ollama --version >> %REPORT% 2>&1
echo. >> %REPORT%

echo === OLLAMA PS ^(active models + processor^) === >> %REPORT%
ollama ps >> %REPORT% 2>&1
echo. >> %REPORT%

echo === OLLAMA SERVER LOG TAIL === >> %REPORT%
if exist "%LOCALAPPDATA%\Ollama\server.log" (
    powershell -NoProfile -Command "Get-Content '$env:LOCALAPPDATA\Ollama\server.log' -Tail 80" >> %REPORT% 2>&1
) else if exist "%USERPROFILE%\.ollama\logs\server.log" (
    powershell -NoProfile -Command "Get-Content '$env:USERPROFILE\.ollama\logs\server.log' -Tail 80" >> %REPORT% 2>&1
) else (
    echo No Ollama server log found at known locations >> %REPORT%
)
echo. >> %REPORT%

echo === DONE === >> %REPORT%
echo Report written to: %CD%\%REPORT%
type %REPORT%
pause
