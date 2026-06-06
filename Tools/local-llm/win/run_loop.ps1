# TARTARIA Windows autonomous ticket runner.
#
# Successor to RUN_OLLAMA_TICKETS.bat — handles full loop + optional Unity +
# Blender validation. Idempotent. Tickets move to _done/ or _failed/.
#
# Usage:
#   pwsh -ExecutionPolicy Bypass -File tools\local-llm\win\run_loop.ps1
#   pwsh -ExecutionPolicy Bypass -File tools\local-llm\win\run_loop.ps1 -RunUnity
#   pwsh -ExecutionPolicy Bypass -File tools\local-llm\win\run_loop.ps1 -RunUnity -RunBlender
#   pwsh -ExecutionPolicy Bypass -File tools\local-llm\win\run_loop.ps1 -Continuous   # loop forever
#
# Created 2026-06-05 for local-laptop autonomous build with Claude supervision.

param(
  [string]$Model = "qwen-tartaria",
  [string]$RepoRoot = $null,
  [string]$UnityBin = "C:\Program Files\Unity\Hub\Editor\6000.3.6f1\Editor\Unity.exe",
  [string]$BlenderBin = "C:\Program Files\Blender Foundation\Blender 5.0\blender.exe",
  [int]$SleepBetween = 5,
  [int]$MaxTicketsPerRun = 20,
  [int]$TicketTimeoutSeconds = 300,   # 5 min cap per ticket — prevents GPU lockup
  [switch]$RunUnity,
  [switch]$RunBlender,
  [switch]$Continuous,
  [int]$ContinuousIdleSeconds = 60
)

$ErrorActionPreference = "Stop"

# --- resolve repo root ---
if (-not $RepoRoot) {
  $RepoRoot = (Resolve-Path "$PSScriptRoot\..\..\..").Path
}
Set-Location $RepoRoot

$TasksDir   = Join-Path $RepoRoot "tools\local-llm\LOCAL_TASKS"
$OutputsDir = Join-Path $RepoRoot "tools\local-llm\LOCAL_OUTPUTS"
$LogDir     = Join-Path $RepoRoot "Logs\local-llm"
$DoneDir    = Join-Path $TasksDir "_done"
$FailedDir  = Join-Path $TasksDir "_failed"

foreach ($d in @($TasksDir, $OutputsDir, $LogDir, $DoneDir, $FailedDir)) {
  if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d | Out-Null }
}

function Write-Log($msg) {
  $stamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
  $line  = "[$stamp] $msg"
  Write-Host $line
  Add-Content -Path (Join-Path $LogDir "run_loop.log") -Value $line
}

# --- Ollama daemon ---
function Ensure-Ollama {
  $proc = Get-Process -Name "ollama" -ErrorAction SilentlyContinue
  if (-not $proc) {
    Write-Log "Starting Ollama daemon..."
    Start-Process -FilePath "ollama" -ArgumentList "serve" -WindowStyle Hidden
    Start-Sleep -Seconds 3
  }
}

function Check-Model {
  $list = & ollama list 2>&1 | Out-String
  if ($list -notmatch [regex]::Escape($Model)) {
    Write-Log "ERROR: model '$Model' not found in 'ollama list'."
    Write-Log "Build it with: ollama create $Model -f tools\local-llm\win\Modelfile.qwen-tartaria"
    exit 1
  }
}

# --- per-ticket metric emitter (NDJSON for dashboard.ps1) ---
function Emit-Metric($payload) {
  $metricsPath = Join-Path $LogDir "_metrics.jsonl"
  ($payload | ConvertTo-Json -Compress -Depth 5) | Add-Content -Path $metricsPath -Encoding utf8
}

# --- single ticket processing ---
function Process-Ticket($ticketPath) {
  $ticketName = [IO.Path]::GetFileNameWithoutExtension($ticketPath)
  $outDir     = Join-Path $OutputsDir $ticketName
  if (-not (Test-Path $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }
  $responsePath = Join-Path $outDir "response.md"
  $errPath      = Join-Path $LogDir "$ticketName.err"

  Write-Log "  -> $ticketName : generating via $Model"

  $ticketBody = Get-Content -Raw $ticketPath
  $tStart = Get-Date
  $metric = [ordered]@{
    ticket  = $ticketName
    model   = $Model
    start   = $tStart.ToString('o')
    in_chars = $ticketBody.Length
    out_chars = 0
    gen_ms  = 0
    apply_ms = 0
    target  = $null
    target_bytes = 0
    applied = $false
    error   = $null
  }
  try {
    $body = @{
      model       = $Model
      prompt      = $ticketBody
      stream      = $false
      keep_alive  = "24h"   # pin model in VRAM — avoids 4-30s cold-load per ticket
      options     = @{ temperature = 0.2; num_predict = 4096 }
    } | ConvertTo-Json -Depth 5

    $genStart = Get-Date
    $resp = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" `
              -Method Post -ContentType "application/json" -Body $body -TimeoutSec $TicketTimeoutSeconds
    $metric.gen_ms = [int]((Get-Date) - $genStart).TotalMilliseconds
    $resp.response | Set-Content -Path $responsePath -Encoding utf8
    $metric.out_chars = $resp.response.Length

    # Extract target path from `// File:` header for dashboard
    $hdr = Select-String -Path $responsePath -Pattern '^\s*//\s*File:\s*(.+)$' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($hdr) { $metric.target = $hdr.Matches[0].Groups[1].Value.Trim() }

    # Apply outputs via Python
    Write-Log "  -> $ticketName : applying outputs..."
    $py = Get-Command python -ErrorAction SilentlyContinue
    if (-not $py) { $py = Get-Command python3 -ErrorAction SilentlyContinue }
    if (-not $py) { Write-Log "  -> $ticketName : ERROR python not on PATH"; throw "python not found" }

    $applyStart = Get-Date
    & $py.Source "tools\local-llm\apply_outputs.py" $responsePath 2>&1 | Tee-Object -FilePath $errPath -Append | ForEach-Object { Write-Log "     $_" }
    $metric.apply_ms = [int]((Get-Date) - $applyStart).TotalMilliseconds
    if ($LASTEXITCODE -ne 0) { throw "apply_outputs.py exit $LASTEXITCODE" }

    # Capture final target size if it exists on disk
    if ($metric.target -and (Test-Path (Join-Path $RepoRoot $metric.target))) {
      $metric.target_bytes = (Get-Item (Join-Path $RepoRoot $metric.target)).Length
      $metric.applied = $true
    }

    Move-Item -Path $ticketPath -Destination $DoneDir -Force
    Write-Log "  -> $ticketName : DONE"
    $metric.applied = $true
    Emit-Metric $metric
    return $true
  }
  catch {
    Write-Log "  -> $ticketName : FAILED — $_"
    Add-Content -Path $errPath -Value $_
    Move-Item -Path $ticketPath -Destination $FailedDir -Force
    $metric.error = "$_"
    Emit-Metric $metric
    return $false
  }
}

# --- batch run ---
function Run-Batch {
  $tickets = Get-ChildItem -Path $TasksDir -Filter "*.md" -File | Where-Object {
    $_.DirectoryName -eq $TasksDir -and
    -not $_.Name.StartsWith("_") -and
    -not $_.Name.StartsWith("EXAMPLE_")
  } | Sort-Object Name

  if ($tickets.Count -eq 0) { Write-Log "No pending tickets."; return 0 }

  $count = 0
  foreach ($t in $tickets) {
    if ($count -ge $MaxTicketsPerRun) {
      Write-Log "Hit MaxTicketsPerRun cap ($MaxTicketsPerRun). Stopping batch."
      break
    }
    $count++
    Write-Log "($count/$($tickets.Count)) $($t.Name)"
    [void](Process-Ticket $t.FullName)
    Start-Sleep -Seconds $SleepBetween
  }
  return $count
}

# --- Unity smoke shot (optional) ---
function Run-UnitySmoke {
  if (-not (Test-Path $UnityBin)) {
    Write-Log "WARN: Unity not at $UnityBin — skipping smoke."
    return
  }
  Write-Log "Running Unity smoke shot..."
  $unityLog = Join-Path $LogDir "unity-smoke.log"
  & $UnityBin -batchmode -projectPath $RepoRoot `
    -executeMethod "Tartaria.Editor.AutoLoop.RunSmokeShot" `
    -logFile $unityLog -quit
  if ($LASTEXITCODE -ne 0) { Write-Log "WARN: Unity smoke exit $LASTEXITCODE (see $unityLog)" }
  else { Write-Log "Unity smoke shot OK." }
}

# --- Blender batch (optional) ---
function Run-BlenderBatch {
  if (-not (Test-Path $BlenderBin)) {
    Write-Log "WARN: Blender not at $BlenderBin — skipping batch."
    return
  }
  Write-Log "Running Blender batch..."
  $blenderLog = Join-Path $LogDir "blender.log"
  & $BlenderBin --background --python "tools\blender\run_all_moon1.py" *>> $blenderLog
  if ($LASTEXITCODE -ne 0) { Write-Log "WARN: Blender batch exit $LASTEXITCODE" }
  else { Write-Log "Blender batch OK." }
}

# --- main ---
Ensure-Ollama
Check-Model

if ($Continuous) {
  Write-Log "Entering continuous mode (Ctrl+C to stop)."
  while ($true) {
    $n = Run-Batch
    if ($n -gt 0) {
      if ($RunUnity)   { Run-UnitySmoke }
      if ($RunBlender) { Run-BlenderBatch }
    }
    Write-Log "Idle for $ContinuousIdleSeconds s..."
    Start-Sleep -Seconds $ContinuousIdleSeconds
  }
}
else {
  $n = Run-Batch
  if ($RunUnity)   { Run-UnitySmoke }
  if ($RunBlender) { Run-BlenderBatch }
  Write-Log "Run complete. Processed $n tickets."
}
