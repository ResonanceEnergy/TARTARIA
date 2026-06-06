# One-shot recovery script for the local loop.
#
# Run when:
#  - Unity Editor became unresponsive while loop was running
#  - A ticket has been "generating" for >10 min
#  - You changed the Modelfile and want to rebuild qwen-tartaria
#
# Usage:
#   pwsh tools\local-llm\win\recover.ps1

$ErrorActionPreference = "Continue"
Set-Location $PSScriptRoot\..\..\..

Write-Host "[recover] Killing stuck runner + ollama generation processes..."
Get-Process pwsh        -ErrorAction SilentlyContinue | Where-Object { $_.Id -ne $PID } | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process powershell  -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process ollama_llama_server -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

Write-Host "[recover] Restarting Ollama daemon..."
if (-not (Get-Process ollama -ErrorAction SilentlyContinue)) {
    Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
    Start-Sleep -Seconds 4
}

Write-Host "[recover] Rebuilding qwen-tartaria from current Modelfile..."
& ollama create qwen-tartaria -f tools\local-llm\win\Modelfile.qwen-tartaria
if ($LASTEXITCODE -ne 0) {
    Write-Host "[recover] ERROR: ollama create failed. Check Modelfile FROM line."
    exit 1
}

Write-Host "[recover] Listing models — qwen-tartaria should be present:"
& ollama list

Write-Host ""
Write-Host "[recover] Done. Re-run the loop with:"
Write-Host "  .\tools\local-llm\win\RUN_LOOP.bat"
Write-Host "  or:"
Write-Host "  pwsh tools\local-llm\win\run_loop.ps1 -MaxTicketsPerRun 1"
