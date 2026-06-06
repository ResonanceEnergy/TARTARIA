@echo off
REM Better GPU diagnostic using Get-CimInstance (wmic is gone in Win 11 24H2+).
REM Also probes Vulkan support which Ollama 0.24+ can use as GPU fallback.
setlocal
cd /d "%~dp0..\.."
set REPORT=tools\local-llm\GPU_REPORT.txt

> %REPORT% echo === GPU MODELS (Get-CimInstance) ===
powershell -NoProfile -Command "Get-CimInstance Win32_VideoController | Select-Object Name, AdapterRAM, DriverVersion, DriverDate | Format-List" >> %REPORT% 2>&1

>> %REPORT% echo.
>> %REPORT% echo === VULKAN SUPPORT (vulkaninfo if present) ===
where vulkaninfo >nul 2>nul
if %errorlevel%==0 (
    vulkaninfo --summary >> %REPORT% 2>&1
) else (
    echo vulkaninfo not on PATH. Vulkan SDK may not be installed. Ollama can still use Vulkan if the driver supports it. >> %REPORT%
)

>> %REPORT% echo.
>> %REPORT% echo === DIRECTX FEATURE LEVEL (dxdiag summary) ===
powershell -NoProfile -Command "$tmp = [System.IO.Path]::GetTempFileName(); Start-Process -FilePath dxdiag -ArgumentList '/t', $tmp -Wait; Get-Content $tmp | Select-String -Pattern 'Card name|Chip type|DAC type|Dedicated Memory|Feature Levels|Driver Version|Driver Date'" >> %REPORT% 2>&1

>> %REPORT% echo.
>> %REPORT% echo === OLLAMA STATUS ===
ollama --version >> %REPORT% 2>&1
>> %REPORT% echo.
ollama ps >> %REPORT% 2>&1

>> %REPORT% echo.
>> %REPORT% echo === OLLAMA SERVER LOG TAIL (looking for 'Initializing CUDA' / 'Initializing GPU' / 'using GPU') ===
powershell -NoProfile -Command "$log = $env:LOCALAPPDATA + '\Ollama\server.log'; if (Test-Path $log) { Get-Content $log -Tail 200 | Select-String -Pattern 'GPU|CUDA|HIP|ROCm|Vulkan|metal|compute|inference|library' } else { Write-Output 'No server.log at ' + $log }" >> %REPORT% 2>&1

>> %REPORT% echo.
>> %REPORT% echo === DONE ===
type %REPORT%
echo.
echo Press any key to close.
pause >nul
