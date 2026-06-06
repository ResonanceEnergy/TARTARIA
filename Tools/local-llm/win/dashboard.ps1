# dashboard.ps1 — TARTARIA local-loop production dashboard.
#
# Reads Logs/local-llm/_metrics.jsonl (one JSON line per processed ticket)
# and emits:
#   - Logs/local-llm/dashboard.json  — machine-readable rollup
#   - A markdown table embedded at the top of STATUS.md (between BEGIN and END markers)
#
# Designed to be called inline from supervisor.ps1, or standalone.
#
# Usage:
#   pwsh tools\local-llm\win\dashboard.ps1
#   pwsh tools\local-llm\win\dashboard.ps1 -WindowHours 24   # rolling window

param(
  [string]$RepoRoot   = "C:\dev\TARTARIA_new",
  [int]$WindowHours  = 24,
  [switch]$NoStatusUpdate
)

$ErrorActionPreference = "Continue"
Set-Location $RepoRoot

$LogDir       = Join-Path $RepoRoot 'Logs\local-llm'
$MetricsPath  = Join-Path $LogDir   '_metrics.jsonl'
$JsonOut      = Join-Path $LogDir   'dashboard.json'
$StatusPath   = Join-Path $RepoRoot 'STATUS.md'
$Cutoff       = (Get-Date).AddHours(-$WindowHours)

if (-not (Test-Path $MetricsPath)) {
  Write-Host "No metrics file at $MetricsPath — nothing to dashboard."
  return
}

# Load all metric lines (NDJSON)
$all = @()
foreach ($line in Get-Content $MetricsPath) {
  if ($line.Trim().Length -eq 0) { continue }
  try { $all += ($line | ConvertFrom-Json) } catch { continue }
}

# Filter to window
$recent = $all | Where-Object {
  try { ([datetime]$_.start) -gt $Cutoff } catch { $false }
}

# Aggregate stats
$total       = $recent.Count
$applied     = ($recent | Where-Object { $_.applied -eq $true }).Count
$failed      = ($recent | Where-Object { $_.applied -ne $true }).Count
$successRate = if ($total -gt 0) { [math]::Round(100.0 * $applied / $total, 1) } else { 0 }
$avgGenMs    = if ($total -gt 0) { [math]::Round(($recent | Measure-Object gen_ms -Average).Average, 0) } else { 0 }
$avgApplyMs  = if ($total -gt 0) { [math]::Round(($recent | Measure-Object apply_ms -Average).Average, 0) } else { 0 }
$avgOutChars = if ($total -gt 0) { [math]::Round(($recent | Measure-Object out_chars -Average).Average, 0) } else { 0 }
$totalGenSec = [math]::Round(($recent | Measure-Object gen_ms -Sum).Sum / 1000.0, 0)

# Per-model breakdown
$byModel = $recent | Group-Object model | ForEach-Object {
  [PSCustomObject]@{
    model   = $_.Name
    count   = $_.Count
    success = ($_.Group | Where-Object { $_.applied -eq $true }).Count
  }
}

# Errors summary (top 5)
$topErrors = $recent | Where-Object { $_.error } | Group-Object error |
             Sort-Object Count -Descending | Select-Object -First 5 |
             ForEach-Object { [PSCustomObject]@{ count = $_.Count; error = $_.Name } }

# Auto-commit branch count (from git for-each-ref)
$autoBranches = 0
try {
  $autoBranches = (git -C $RepoRoot for-each-ref --format='%(refname:short)' refs/remotes/origin/loop/auto-* 2>$null | Measure-Object).Count
} catch {}

# Build the rollup
$rollup = [ordered]@{
  generated_at      = (Get-Date).ToString('o')
  window_hours      = $WindowHours
  tickets_total     = $total
  tickets_applied   = $applied
  tickets_failed    = $failed
  success_rate_pct  = $successRate
  avg_gen_ms        = $avgGenMs
  avg_apply_ms      = $avgApplyMs
  avg_out_chars     = $avgOutChars
  total_gen_seconds = $totalGenSec
  auto_branches     = $autoBranches
  by_model          = @($byModel)
  top_errors        = @($topErrors)
}

# Write JSON sidecar
$rollup | ConvertTo-Json -Depth 5 | Set-Content $JsonOut -Encoding utf8

# Build the markdown table
$md = @"
<!-- DASHBOARD:BEGIN -->
### Loop dashboard — last $WindowHours h (refreshed $((Get-Date).ToString('HH:mm:ss')))

| Metric | Value |
|---|---|
| Tickets processed | **$total** |
| Applied | $applied |
| Failed | $failed |
| Success rate | **${successRate}%** |
| Avg gen time | $($avgGenMs) ms |
| Avg apply time | $($avgApplyMs) ms |
| Avg output size | $($avgOutChars) chars |
| Total compute | $($totalGenSec)s |
| Auto-branches on origin | $autoBranches |

"@

if ($byModel.Count -gt 0) {
  $md += "**By model:**`n`n"
  $md += "| model | count | applied |`n|---|---|---|`n"
  foreach ($r in $byModel) {
    $md += "| $($r.model) | $($r.count) | $($r.success) |`n"
  }
  $md += "`n"
}

if ($topErrors.Count -gt 0) {
  $md += "**Top errors:**`n`n"
  $md += "| count | error |`n|---|---|`n"
  foreach ($e in $topErrors) {
    $errPreview = $e.error
    if ($errPreview.Length -gt 80) { $errPreview = $errPreview.Substring(0, 77) + "..." }
    $md += "| $($e.count) | $errPreview |`n"
  }
  $md += "`n"
}

$md += "<!-- DASHBOARD:END -->"

# Patch STATUS.md
if (-not $NoStatusUpdate -and (Test-Path $StatusPath)) {
  $status = Get-Content $StatusPath -Raw
  if ($status -match '<!-- DASHBOARD:BEGIN -->[\s\S]*?<!-- DASHBOARD:END -->') {
    $status = [regex]::Replace($status, '<!-- DASHBOARD:BEGIN -->[\s\S]*?<!-- DASHBOARD:END -->', { param($m) $md })
  } else {
    # Insert after the supervisor feed lines
    if ($status -match '(?ms)(\*\*Prior check-in:[^\r\n]*\r?\n)') {
      $status = $status -replace '(?ms)(\*\*Prior check-in:[^\r\n]*\r?\n)', "`$1`n$md`n"
    } else {
      # Fall back: prepend
      $status = "$md`n`n$status"
    }
  }
  $status | Set-Content $StatusPath -Encoding utf8 -NoNewline
}

Write-Host "Dashboard: $total tickets in last ${WindowHours}h, $successRate% success, $autoBranches auto-branches."
