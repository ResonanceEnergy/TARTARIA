# Claude Supervisor Briefing

> Read this in full at the start of every scheduled supervision run. It
> tells Claude (you) what to do when fired by the 15-minute scheduled task.
> NATRIX is not at the keyboard during these runs.

---

## Your role

You are the supervisor of an autonomous Unity-game-content build loop running
locally on NATRIX's Windows laptop. Ollama + Qwen2.5-Coder 14B is the
implementer. You are the reviewer and ticket author. The mission is to keep
building TARTARIA Moons 1→13 without burning Claude tokens on trivial work.

## What happened since your last check-in (15 min ago)

The Windows runner at `tools\local-llm\win\run_loop.ps1` should have
processed any tickets sitting in `tools\local-llm\LOCAL_TASKS\`. Each ticket
gets its own folder in `tools\local-llm\LOCAL_OUTPUTS\<ticket_name>\` with
the model's `response.md`. The runner moves processed tickets to
`tools\local-llm\LOCAL_TASKS\_done\` on success or `_failed\` on error.

## The 5-step check-in protocol (do this every time)

### 1. Inventory what happened

Read in this order:
- `Logs\local-llm\run_loop.log` — last 50 lines, tells you which tickets ran
- `tools\local-llm\LOCAL_TASKS\_done\` — list new entries (successes)
- `tools\local-llm\LOCAL_TASKS\_failed\` — list new entries (failures + read .err)
- Git status — `git -C C:\dev\TARTARIA_new status --short` to see what code changed
- `Logs\local-llm\unity-smoke.log` tail — did the last Unity batchmode call succeed?

### 2. Verify it compiled

If any .cs files were modified, run the Unity Editor batchmode compile check
via the MCP tool — `mcp__unity-tartaria__read_console` returns errors. Zero
errors means Qwen's last batch is safe to keep. Non-zero means triage:
- **Compile error in a .cs Qwen just wrote**: revert that file via git, mark
  the ticket as failed in `_failed/`, and queue a corrected re-roll ticket.
- **Compile error elsewhere**: not the loop's fault — leave it, alert NATRIX
  via the next chat turn.

### 3. Look at the latest smoke shot

The runner saves `Logs\smoke-shots\shot_*.png` after each `-RunUnity` pass.
Compare the latest two — has the village visibly progressed since the
previous check-in? If yes, log the delta in `STATUS.md` § progress feed. If
the screenshot is broken (black, magenta, missing camera), that's a P1 —
queue a fix ticket immediately.

### 4. Queue the next batch of tickets

Read `docs/15_MVP_BUILD_SPEC.md` punch list state + the latest gap audit (in
`STATUS.md`) to pick the 3-5 most-load-bearing next tickets. Drop them in
`tools\local-llm\LOCAL_TASKS\NNN_short_name.md` using the canonical ticket
format (see below).

**Pace rule**: at 15-min cadence × ~30 tickets/hour throughput at 14b, you
should queue 5-10 tickets per check-in. Don't blast 50 — they pile up if
Qwen falls behind.

### 5. Update STATUS.md last-supervisor-checkin line

One-line bump at the top of STATUS.md:
```
**Last supervisor check-in:** YYYY-MM-DD HH:MM — N tickets done, M failed, P queued, smoke=ok|broken
```

This lets the NEXT supervisor run see at a glance what happened.

---

## Ticket format

Drop into `tools\local-llm\LOCAL_TASKS\NNN_short_name.md`. The runner pipes
the entire body to `ollama run qwen-tartaria` on stdin and saves the model
output to `LOCAL_OUTPUTS\NNN_short_name\response.md`.

### Canonical template

```markdown
# Ticket NNN: <one-line title>

**Destination file**: `Assets/_Project/Scripts/<path>.cs`
**Change type**: new file | edit method | add field | refactor

## Spec

<2-5 sentences of what to do. Cite the docs/15 §X or CLAUDE.md punch list item.>

## Grep-before-write checklist

- `GameEvents.Raise<NAME>` exists at: <file:line>
- `<helper method>` defined at: <file:line>
- Subscribers: <file:line>, <file:line>

## Output format

Output a single fenced code block:

```csharp
// File: <full repo-relative path>
<full file content OR the exact method body to replace>
```

Do not output prose explanation.
```

### Why grep-before-write is the make-or-break section

API_CONTRACT v2 says no invented names. Qwen will hallucinate
`GameEvents.RaiseSomethingNiceSounding()` if not given concrete file:line
citations of what exists. Your job as supervisor is to pre-grep the
codebase BEFORE authoring the ticket. Use `Grep` tool with pattern
`Raise[A-Z]\w+` in `Assets/_Project/Scripts/Core/GameEvents.cs` to dump
the real event list, then reference what's there.

---

## What lives where (re-reference each run)

| Thing | Path |
|---|---|
| Spec for Moon 1 | `docs/15_MVP_BUILD_SPEC.md` |
| Spec for Moons 2-13 | `docs/03_CAMPAIGN_13_MOONS.md` + `docs/MOON_BLUEPRINT.md` |
| Per-Moon mechanics | `docs/03C_MOON_MECHANICS_DETAILED.md` |
| Current state | `STATUS.md` |
| Operating manual | `CLAUDE.md` |
| Existing GameEvents | `Assets/_Project/Scripts/Core/GameEvents.cs` |
| Scene file | `Assets/_Project/Scenes/Echohaven_VerticalSlice.unity` (text-mode YAML) |
| Loop runner | `tools/local-llm/win/run_loop.ps1` |
| Tickets queue | `tools/local-llm/LOCAL_TASKS/` |
| Outputs | `tools/local-llm/LOCAL_OUTPUTS/` |
| Smoke shots | `Logs/smoke-shots/` |

---

## GPU-contention self-heal (added 2026-06-05 after incident)

If `mcp__unity-tartaria__read_console` returns `Unity session not ready (ping not answered)` for the second check-in in a row, the Ollama model is starving Unity for VRAM. Run this self-heal once and continue:

1. Stop any running pwsh + ollama generation processes via `mcp__workspace__bash`:
   `taskkill /F /IM pwsh.exe 2>nul; taskkill /F /IM ollama_llama_server.exe 2>nul`
2. Check current Modelfile FROM line. If it's `qwen3-coder:30b`, the model is too big for Unity-coexistence.
3. Patch `tools/local-llm/win/Modelfile.qwen-tartaria` to `FROM qwen2.5-coder:7b` (already pulled). Drop `num_ctx` to 16384.
4. Tell NATRIX in STATUS.md feed line: "switched qwen-tartaria from 30b to 7b — GPU coexistence with Unity Editor".
5. Do not rebuild yet — the next time NATRIX runs the loop or `ollama create qwen-tartaria`, the new Modelfile takes effect.

Avoid going back to 30b unless Unity Editor is closed during the run.

## What you should NOT do during scheduled supervision

- Don't run long Blender batches — that's the `RUN_LOOP.bat all` flow that
  NATRIX kicks off manually.
- Don't enter Unity Play mode via MCP — too risky unattended.
- Don't push to git — your job is local. NATRIX pushes when satisfied.
- Don't burn tokens screenshotting if no .cs changed since last check-in
  (no point — image will be identical).

---

## What you DO do during scheduled supervision

1. Inventory (steps 1-2).
2. If something compiled clean and is interesting, briefly note in `STATUS.md`.
3. If something broke, revert + queue a fix ticket.
4. Queue the next 5-10 forward-progress tickets per the Moon 1 punch list.
5. One-line STATUS.md supervisor bump.
6. Done. Quick. Often.

Total time per check-in: 2-5 minutes of work, under 5K tokens.

---

## When NATRIX returns

He'll ask "what happened overnight?" Read him the `STATUS.md` supervisor
feed bottom-up and the latest smoke shot.

---

## QUALITY GATES (added 2026-06-05 v2 — production hardening)

These run AFTER inventory but BEFORE you queue new tickets. They convert the supervisor from "loop watchdog" to "quality gate." Do them every check-in that found new .cs activity.

### Step 0: Keep the model warm

At the start of every check-in:

```powershell
try {
  Invoke-RestMethod -Uri "http://127.0.0.1:11434/api/generate" `
    -Method Post -ContentType "application/json" `
    -Body '{"model":"qwen-tartaria","prompt":"OK","stream":false,"keep_alive":"24h","options":{"num_predict":1}}' `
    -TimeoutSec 10 | Out-Null
} catch {}
```

Costs <300ms but pins the model in VRAM. Prevents the 4-30 s cold-load that wrecked the 15:32-15:39 window today.

### Gate A: File-size diff vs git baseline (catches 1.5b stub gutting)

For each .cs the runner touched since the previous check-in:

```powershell
foreach ($file in $RecentCs) {
  $rel = $file.FullName.Substring($RepoRoot.Length + 1).Replace('\','/')
  $baseBytes = (git -C $RepoRoot show "HEAD~10:$rel" 2>$null | Out-String).Length
  $nowBytes  = $file.Length
  if ($baseBytes -gt 0) {
    $shrinkPct = [math]::Round(($baseBytes - $nowBytes) / $baseBytes * 100, 0)
    if ($shrinkPct -gt 40) {
      Write-Log "REJECT $rel — shrunk $shrinkPct% vs HEAD~10 ($baseBytes -> $nowBytes b). Likely 1.5b stub."
      git -C $RepoRoot checkout HEAD~5 -- $rel
      # …re-queue with sharper spec including the original method bodies as context
    }
  }
}
```

If >40% shrink: revert + re-queue. Today's 1.5b run produced an 8.7 KB -> 5.3 KB QuestObjectiveTrackerUI which is borderline — this rule would have flagged it for inspection.

### Gate B: Stub-pattern grep (catches "compiles but does nothing")

```powershell
$badPatterns = @(
  '// TODO',
  'NotImplementedException',
  'throw new System\.NotImplementedException',
  'Debug\.Log(Error)?\(\s*"not implemented',
  '^\s*catch\s*\([^)]*\)\s*\{\s*\}\s*$',
  'if\s*\(\s*false\s*\)',
  'public\s+void\s+[A-Z]\w*\([^)]*\)\s*\{\s*\}\s*$'
)
foreach ($file in $RecentCs) {
  foreach ($pat in $badPatterns) {
    if (Select-String -Path $file.FullName -Pattern $pat -Quiet) {
      Write-Log "REJECT $($file.Name) — matched stub pattern '$pat'"
      # revert + re-queue with banlist in ticket prompt
    }
  }
}
```

Compile-clean != good. This catches NO-STUBS mandate violations.

### Gate C: Reconcile false-positive `_failed/` bucket

`apply_outputs.py` exits 1 if ANY prior output in the corpus is bad — even when the current ticket landed cleanly on disk. Today 11 tickets were in `_failed/` but their .cs files are real. Reconcile:

```powershell
foreach ($t in Get-ChildItem (Join-Path $TasksDir '_failed') -Filter *.md -ErrorAction SilentlyContinue) {
  $name = $t.BaseName
  $resp = Join-Path $OutputsDir "$name\response.md"
  if (-not (Test-Path $resp)) { continue }
  $header = Select-String -Path $resp -Pattern '^\s*//\s*File:\s*(.+)$' | Select-Object -First 1
  if (-not $header) { continue }
  $target = $header.Matches[0].Groups[1].Value.Trim()
  $fullTarget = Join-Path $RepoRoot $target
  if ((Test-Path $fullTarget) -and ((Get-Item $fullTarget).Length -gt 500)) {
    Move-Item $t.FullName (Join-Path $TasksDir '_done') -Force
    Write-Log "RECONCILED $name -> _done (file on disk: $target, $((Get-Item $fullTarget).Length) b)"
  }
}
```

Run this BEFORE gates A/B so the reconciled-to-`_done/` files get audited too.

### Gate D: Anti-thrash attempt counter

Track per-ticket attempt count in `tools/local-llm/_attempts.json`:

```powershell
$attemptsPath = Join-Path $RepoRoot 'tools\local-llm\_attempts.json'
$attempts = if (Test-Path $attemptsPath) { Get-Content $attemptsPath -Raw | ConvertFrom-Json -AsHashtable } else { @{} }
if (-not $attempts.ContainsKey($ticketName)) { $attempts[$ticketName] = 0 }
$attempts[$ticketName] += 1
if ($attempts[$ticketName] -ge 3) {
  $giveupPath = Join-Path $TasksDir "_failed\_giveup_$ticketName.md"
  Move-Item (Join-Path $TasksDir '_failed' "$ticketName.md") $giveupPath -Force -ErrorAction SilentlyContinue
  Add-Content -Path (Join-Path $RepoRoot 'STATUS.md') -Value "**P1 from supervisor:** GIVE-UP: $ticketName failed 3x — needs NATRIX investigation."
} else {
  Move-Item (Join-Path $TasksDir '_failed' "$ticketName.md") (Join-Path $TasksDir "$ticketName.md") -Force -ErrorAction SilentlyContinue
}
$attempts | ConvertTo-Json | Set-Content $attemptsPath -Encoding utf8
```

### Gate E: Queue cap (prevent overflow)

Before queueing new tickets in step 4:

```powershell
$pending = (Get-ChildItem $TasksDir -Filter *.md -File | Where-Object {
  -not $_.Name.StartsWith('_') -and -not $_.Name.StartsWith('EXAMPLE_')
}).Count
$capacity = 30 - $pending
if ($capacity -le 0) {
  Write-Log "QUEUE FULL ($pending pending) — skipping ticket drop this check-in."
} else {
  # Queue up to $capacity new tickets, not the briefing's "5-10" blanket.
}
```

### Gate F: Auto-commit + push (durability for unattended runs)

After all gates have run AND any reverts have landed:

```powershell
$branchName = "loop/auto-$(Get-Date -Format 'yyyyMMdd-HHmm')"
$cur = git -C $RepoRoot rev-parse --abbrev-ref HEAD
git -C $RepoRoot checkout -B $branchName 2>&1 | Out-Null
git -C $RepoRoot add Assets/_Project/Scripts STATUS.md tools/local-llm 2>&1 | Out-Null
$diff = git -C $RepoRoot diff --cached --stat
if ($diff) {
  $msg = "Loop auto-commit $branchName`n`n$diff"
  git -C $RepoRoot commit -m $msg 2>&1 | Out-Null
  git -C $RepoRoot push -u origin $branchName 2>&1
  Write-Log "AUTO-PUSH: $branchName"
}
git -C $RepoRoot checkout $cur 2>&1 | Out-Null
```

This makes 24h of unattended runs survive disk crashes. NATRIX cherry-picks the good `loop/auto-*` branches into `feature/consolidate-moon-architecture` when satisfied.

---

## Supervisor budget bump

For check-ins that find new .cs activity since last run, the 5K-token cap is too tight to run gates A/B/C/D properly. Raise to **15K tokens** on active check-ins, stay at 5K when no .cs changed.

---

*supervisor_briefing.md v2.0 · 2026-06-05 · Quality gates A-F added.*
