# Supervisor.ps1 — executable form of supervisor_briefing.md v2.0.
#
# Runs the 6 quality gates against the live loop state and reports.
# Safe to run any time; idempotent. Designed to be triggered every 15 min
# by the scheduled task `tartaria-loop-supervisor` OR fired manually.
#
# Usage:
#   pwsh tools\local-llm\win\supervisor.ps1
#   pwsh tools\local-llm\win\supervisor.ps1 -DryRun    # report only, no reverts/moves
#   pwsh tools\local-llm\win\supervisor.ps1 -SkipPush  # don't push to git
#
# Created 2026-06-05 to operationalize supervisor_briefing.md v2.

param(
  [string]$RepoRoot = "C:\dev\TARTARIA_new",
  [string]$Model    = "qwen-tartaria",
  [switch]$DryRun,
  [switch]$SkipPush,
  [datetime]$Since  = ([datetime]'2026-06-05 15:40:00')   # files newer than this are "this batch"
)

$ErrorActionPreference = "Continue"
Set-Location $RepoRoot

$TasksDir    = Join-Path $RepoRoot 'tools\local-llm\LOCAL_TASKS'
$OutputsDir  = Join-Path $RepoRoot 'tools\local-llm\LOCAL_OUTPUTS'
$DoneDir     = Join-Path $TasksDir '_done'
$FailedDir   = Join-Path $TasksDir '_failed'
$AttemptsPath = Join-Path $RepoRoot 'tools\local-llm\_attempts.json'
$LogDir      = Join-Path $RepoRoot 'Logs\local-llm'
if (-not (Test-Path $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }

$now = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
Write-Host "============================================================="
Write-Host " Supervisor run at $now (DryRun=$DryRun)"
Write-Host "============================================================="

# --- Step 0: Keep model warm ---
Write-Host "`n[Step 0] Pinging model to refresh keep_alive=24h..."
$startWarm = Get-Date
try {
  $r = Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" `
    -Method Post -ContentType "application/json" `
    -Body "{`"model`":`"$Model`",`"prompt`":`"OK`",`"stream`":false,`"keep_alive`":`"24h`",`"options`":{`"num_predict`":1}}" `
    -TimeoutSec 30
  $secs = [math]::Round(((Get-Date) - $startWarm).TotalSeconds, 2)
  Write-Host "  OK — model warm in ${secs}s ($($r.eval_count) tokens)"
} catch {
  Write-Host "  WARN — ping failed: $_"
}

# --- Step 1: Inventory ---
Write-Host "`n[Step 1] Inventory:"
$doneCount   = (Get-ChildItem $DoneDir   -Filter *.md -ErrorAction SilentlyContinue).Count
$failedCount = (Get-ChildItem $FailedDir -Filter *.md -ErrorAction SilentlyContinue).Count
$queueCount  = (Get-ChildItem $TasksDir  -Filter *.md -File | Where-Object {
                  -not $_.Name.StartsWith('_') -and -not $_.Name.StartsWith('EXAMPLE_')
                }).Count
Write-Host "  done=$doneCount  failed=$failedCount  queue=$queueCount"
$logTail = Get-Content (Join-Path $RepoRoot 'Logs\local-llm\run_loop.log') -Tail 3 -ErrorAction SilentlyContinue
if ($logTail) { Write-Host "  run_loop.log tail:"; $logTail | ForEach-Object { Write-Host "    $_" } }

# --- Recent .cs (this batch) ---
$RecentCs = Get-ChildItem (Join-Path $RepoRoot 'Assets\_Project\Scripts') -Recurse -Filter *.cs -ErrorAction SilentlyContinue `
            | Where-Object { $_.LastWriteTime -gt $Since }
Write-Host "  recent .cs (since $($Since.ToString('HH:mm'))): $($RecentCs.Count) files"

# --- Gate C FIRST: Reconcile false-positive _failed bucket ---
Write-Host "`n[Gate C] Reconcile _failed -> _done where .cs is actually on disk..."
$reconciled = 0
foreach ($t in Get-ChildItem $FailedDir -Filter *.md -ErrorAction SilentlyContinue) {
  $name = $t.BaseName
  $resp = Join-Path $OutputsDir "$name\response.md"
  if (-not (Test-Path $resp)) { continue }
  $hdr  = Select-String -Path $resp -Pattern '^\s*//\s*File:\s*(.+)$' -ErrorAction SilentlyContinue | Select-Object -First 1
  if (-not $hdr) { continue }
  $target = $hdr.Matches[0].Groups[1].Value.Trim()
  $fullTarget = Join-Path $RepoRoot $target
  if ((Test-Path $fullTarget) -and ((Get-Item $fullTarget).Length -gt 500)) {
    if (-not $DryRun) { Move-Item $t.FullName $DoneDir -Force }
    Write-Host "  RECONCILED $name -> _done ($target, $((Get-Item $fullTarget).Length)b)"
    $reconciled++
  }
}
Write-Host "  $reconciled tickets reconciled."

# --- Gate A: File-size diff vs git baseline ---
Write-Host "`n[Gate A] File-size diff vs HEAD~10..."
$reverted = @()
foreach ($f in $RecentCs) {
  $rel = $f.FullName.Substring($RepoRoot.Length + 1).Replace('\','/')
  $base = (git show "HEAD~10:$rel" 2>$null | Out-String).Length
  if ($base -le 0) { continue }  # new file, can't compare
  $now = $f.Length
  $pct = [math]::Round(($base - $now) / $base * 100, 0)
  if ($pct -gt 40) {
    Write-Host "  REVERT $rel — shrunk $pct% ($base -> $now b)"
    if (-not $DryRun) {
      git checkout HEAD~10 -- $rel 2>$null
      $reverted += $rel
    }
  }
}
Write-Host "  $($reverted.Count) files reverted."

# --- Gate B: Stub-pattern grep on remaining recent files ---
Write-Host "`n[Gate B] Stub-pattern grep..."
$badPatterns = @(
  '// TODO',
  'NotImplementedException',
  'throw new System\.NotImplementedException',
  '^\s*catch\s*\([^)]*\)\s*\{\s*\}\s*$',
  'if\s*\(\s*false\s*\)\s*\{'
)
$flagged = @()
$RecentCs2 = Get-ChildItem (Join-Path $RepoRoot 'Assets\_Project\Scripts') -Recurse -Filter *.cs -ErrorAction SilentlyContinue `
             | Where-Object { $_.LastWriteTime -gt $Since }
foreach ($f in $RecentCs2) {
  $rel = $f.FullName.Substring($RepoRoot.Length + 1).Replace('\','/')
  if ($reverted -contains $rel) { continue }  # already handled by Gate A
  foreach ($pat in $badPatterns) {
    $hits = Select-String -Path $f.FullName -Pattern $pat -ErrorAction SilentlyContinue
    if ($hits) {
      Write-Host "  FLAG $rel — matched '$pat' on line(s) $(($hits | Select -ExpandProperty LineNumber) -join ',')"
      $flagged += $rel
      break
    }
  }
}
Write-Host "  $($flagged.Count | Select-Object -Unique) flagged. (Review manually; no auto-revert from Gate B)"

# --- Gate G: Unity Editor.log compile sweep ---
Write-Host "`n[Gate G] Unity Editor.log compile sweep..."
$editorLog = Join-Path $env:LOCALAPPDATA 'Unity\Editor\Editor.log'
$compileErrors = @()
if (Test-Path $editorLog) {
  $logLwt = (Get-Item $editorLog).LastWriteTime
  $logTail = Get-Content $editorLog -Tail 500 -ErrorAction SilentlyContinue
  # Grep for CSnnnn errors
  $csHits = $logTail | Select-String -Pattern '(Assets[/\\][^:(]+\.cs)\(\d+,\d+\):\s*error\s+(CS\d{4})' -ErrorAction SilentlyContinue
  foreach ($h in $csHits) {
    $compileErrors += [PSCustomObject]@{
      file = $h.Matches[0].Groups[1].Value -replace '\\','/'
      code = $h.Matches[0].Groups[2].Value
      line = $h.Line
    }
  }
  $unique = $compileErrors | Sort-Object file -Unique
  if ($unique.Count -eq 0) {
    Write-Host "  CLEAN — 0 CS errors in Editor.log tail (log mtime: $($logLwt.ToString('HH:mm:ss')))"
  } else {
    foreach ($u in $unique) {
      Write-Host "  CS ERROR $($u.code) in $($u.file)"
    }
    Write-Host "  Found $($unique.Count) files with CS errors. Manual revert may be needed."
  }
} else {
  Write-Host "  Editor.log not at $editorLog — Unity may never have run here. Skipping."
}

# --- Gate H: Smoke shot tracker (passive — reports latest shot) ---
Write-Host "`n[Gate H] Smoke shot tracker..."
$shotDir = Join-Path $RepoRoot 'Logs\smoke-shots'
if (Test-Path $shotDir) {
  $shots = Get-ChildItem $shotDir -Filter *.png -ErrorAction SilentlyContinue | Sort-Object LastWriteTime
  if ($shots.Count -ge 1) {
    $newest = $shots[-1]
    $age = (Get-Date) - $newest.LastWriteTime
    Write-Host "  latest: $($newest.Name) ($($newest.Length)b, $([math]::Round($age.TotalMinutes,0))m ago)"
    if ($shots.Count -ge 2) {
      $prev = $shots[-2]
      $delta = $newest.Length - $prev.Length
      Write-Host "  delta vs prev ($($prev.Name)): $(if ($delta -ge 0) {'+'} else {''})$delta bytes"
      if ([math]::Abs($delta) -lt 100 -and $age.TotalHours -lt 2) {
        Write-Host "  WARN — shot size nearly identical to prev. Render may be broken."
      }
    }
    if ($age.TotalHours -gt 4) {
      Write-Host "  STALE — last shot is $([math]::Round($age.TotalHours,1)) h old. Trigger via Tartaria/Take Smoke Shot menu."
    }
  } else {
    Write-Host "  no shots yet. Fire AutoLoop.RunSmokeShot via Unity or Tartaria menu."
  }
} else {
  Write-Host "  smoke-shots dir doesn't exist. Will be created by AutoLoop.RunSmokeShot."
}

# --- Gate I: Blender batch on .py changes ---
Write-Host "`n[Gate I] Blender on .py changes since last check-in..."
$blenderBin   = "C:\Program Files\Blender Foundation\Blender 5.0\blender.exe"
$blenderDir   = Join-Path $RepoRoot 'tools\blender'
$blenderLogD  = Join-Path $LogDir   'blender'
if (-not (Test-Path $blenderLogD)) { New-Item -ItemType Directory -Path $blenderLogD | Out-Null }
$blenderRanFlag = Join-Path $LogDir '_last_blender_check.txt'
$lastCheck = if (Test-Path $blenderRanFlag) { [datetime](Get-Content $blenderRanFlag) } else { (Get-Date).AddHours(-24) }

if ((Test-Path $blenderBin) -and (Test-Path $blenderDir)) {
  $changedPy = Get-ChildItem $blenderDir -Filter gen_*.py -File | Where-Object { $_.LastWriteTime -gt $lastCheck }
  if ($changedPy.Count -gt 0) {
    foreach ($py in $changedPy) {
      Write-Host "  firing $($py.Name)..."
      if (-not $DryRun) {
        $blog = Join-Path $blenderLogD ($py.BaseName + '.log')
        $procArgs = @('--background', '--python', $py.FullName)
        $proc = Start-Process -FilePath $blenderBin -ArgumentList $procArgs -Wait -PassThru -NoNewWindow `
                  -RedirectStandardOutput $blog -RedirectStandardError "$blog.err"
        Write-Host "    -> exit $($proc.ExitCode) log=$blog"
      }
    }
    (Get-Date).ToString('o') | Set-Content $blenderRanFlag -Encoding utf8
  } else {
    Write-Host "  no gen_*.py changed since $($lastCheck.ToString('HH:mm'))"
  }
} else {
  Write-Host "  Blender not at $blenderBin or no tools/blender dir. Skipping."
}

# --- Gate E: Queue cap ---
Write-Host "`n[Gate E] Queue cap check (cap=30)..."
$cap = 30 - $queueCount
if ($cap -le 0) { Write-Host "  QUEUE FULL ($queueCount pending) — would skip new ticket drop." }
else { Write-Host "  capacity=$cap — OK to queue more." }

# --- Gate F: Auto-commit + push (if anything changed) ---
Write-Host "`n[Gate F] Auto-commit + push..."
if ($DryRun) {
  Write-Host "  DryRun: skipping commit/push."
} elseif ($SkipPush) {
  Write-Host "  -SkipPush set: skipping."
} else {
  Remove-Item (Join-Path $RepoRoot '.git\index.lock') -Force -ErrorAction SilentlyContinue
  $branchCur = (git rev-parse --abbrev-ref HEAD 2>$null).Trim()
  git add Assets/_Project/Scripts STATUS.md tools/local-llm 2>$null
  $diff = git diff --cached --stat 2>$null
  if ($diff) {
    $branch = "loop/auto-$(Get-Date -Format 'yyyyMMdd-HHmm')"
    Write-Host "  changes detected — creating branch $branch"
    git checkout -B $branch 2>$null | Out-Null
    # Write diff to tempfile — avoids Windows cmdline length limit on -m
    $msgFile = New-TemporaryFile
    "Supervisor auto-commit $branch`n`n$diff" | Set-Content $msgFile -Encoding utf8
    git commit -F $msgFile.FullName 2>$null | Out-Null
    Remove-Item $msgFile -Force -ErrorAction SilentlyContinue
    if ($LASTEXITCODE -eq 0) {
      git push -u origin $branch 2>&1 | Select-Object -Last 2 | ForEach-Object { Write-Host "    $_" }
    }
    git checkout $branchCur 2>$null | Out-Null
  } else {
    Write-Host "  nothing staged — no commit needed."
  }
}

# --- Dashboard rollup (after all gates) ---
Write-Host "`n[Dashboard] Rolling up 24h metrics..."
$dashScript = Join-Path $RepoRoot 'tools\local-llm\win\dashboard.ps1'
if (Test-Path $dashScript) {
  & $dashScript -WindowHours 24 2>&1 | ForEach-Object { Write-Host "  $_" }
} else {
  Write-Host "  dashboard.ps1 not found — skipping"
}

# --- Final STATUS.md feed bump ---
Write-Host "`n[Final] Updating STATUS.md feed line..."
if (-not $DryRun) {
  $feedTime = (Get-Date).ToString('yyyy-MM-dd HH:mm')
  $flaggedUnique = ($flagged | Sort-Object -Unique).Count
  $line = "**Last supervisor check-in:** $feedTime — v2.0 gates run. reconciled=$reconciled reverted=$($reverted.Count) flagged=$flaggedUnique queue=$queueCount done=$doneCount failed=$failedCount"
  $sf = Join-Path $RepoRoot 'STATUS.md'
  if (Test-Path $sf) {
    $s = Get-Content $sf -Raw
    $s = $s -replace '(?m)^\*\*Last supervisor check-in:.*$', $line
    $s | Set-Content $sf -Encoding utf8 -NoNewline
  }
  Write-Host "  $line"
}

Write-Host "`n============================================================="
Write-Host " Supervisor run complete."
Write-Host "============================================================="
