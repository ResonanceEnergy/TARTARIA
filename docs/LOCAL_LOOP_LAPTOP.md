# LOCAL LAPTOP LOOP — Windows autonomous TARTARIA build

> Authored 2026-06-05. Read this end-to-end before first run. Single source
> of truth for the Windows-side autonomous loop with Claude supervision at
> 15-minute cadence.

---

## TL;DR

```powershell
# One-time install (see Step 1)
brew install --cask ollama        # or download from ollama.com
ollama pull qwen2.5-coder:14b
cd C:\dev\TARTARIA_new
ollama create qwen-tartaria -f tools\local-llm\win\Modelfile.qwen-tartaria

# Day-to-day
# Drop tickets in tools\local-llm\LOCAL_TASKS\
# Then double-click:
tools\local-llm\win\RUN_LOOP.bat continuous
```

Claude wakes up every 15 minutes via the scheduled task, reviews what
shipped, and queues the next batch of tickets. NATRIX walks away.

---

## Architecture

```
NATRIX drops a few seed tickets, then walks away.
                        |
                        v
        +----------------------------------+
        |  RUN_LOOP.bat (continuous mode)  |
        |  - reads LOCAL_TASKS\*.md        |
        |  - pipes each to ollama API      |
        |  - apply_outputs.py merges code  |
        |  - moves ticket to _done\        |
        |  - optional: Unity batchmode     |
        |  - optional: Blender batch       |
        +----------------------------------+
                        |
                        v
       Logs every step to Logs\local-llm\run_loop.log
                        |
       Every 15 minutes: Claude scheduled task fires
                        |
                        v
        +----------------------------------+
        |  Claude supervisor (this you)    |
        |  - reads run_loop.log            |
        |  - checks compile via MCP        |
        |  - reverts bad outputs           |
        |  - queues next 5-10 tickets      |
        |  - bumps STATUS.md feed line     |
        +----------------------------------+
```

The runner uses `http://127.0.0.1:11434/api/generate` directly (not the
`ollama run` CLI) to avoid Windows console UTF-8 issues that bit the
prior Sprint 11 runs.

---

## Why qwen2.5-coder:14b on RTX 3080/4080/4090

Per the Mac handoff research, on 16-24 GB VRAM dense Q4 14B is the sharpest
**generation-quality / generation-speed** balance for Unity C# tickets:

- ~30-50 tokens/sec on RTX 4080/4090 with 16K context
- ~12-14 GB VRAM resident with this Modelfile
- 9 GB model on disk
- Sharper code than 7b for non-trivial refactors

Cross-reference table (informational):

| Model | VRAM idle | t/s on 4090 | Best for |
|---|---|---|---|
| `qwen2.5-coder:7b` | ~6 GB | 60-90 | Quick stubs, scaffolding |
| **`qwen2.5-coder:14b`** ← us | ~12 GB | **30-50** | **Production C# tickets** |
| `qwen2.5-coder:32b` Q4 | ~22 GB | 12-20 | Hard reasoning, slow |
| `qwen3-coder:30b` MoE | ~20 GB | 50-70 | Best speed/quality if you have 24GB headroom |

**Upgrade path** if you want to swap models later: edit
`tools\local-llm\win\Modelfile.qwen-tartaria` line 1 (`FROM ...`), then re-run
`ollama create qwen-tartaria -f tools\local-llm\win\Modelfile.qwen-tartaria`.
Everything else stays the same.

---

## First-day install — exact commands

### Step 1 — Ollama

```powershell
# Install (one of):
# A) From browser: https://ollama.com/download — pick Windows installer
# B) winget:
winget install --id Ollama.Ollama

# After install, Ollama auto-runs in the background.
# Verify:
ollama --version       # should be 0.19+ for best perf, but 0.10+ works
```

### Step 2 — Pull the model + build the custom Modelfile

```powershell
cd C:\dev\TARTARIA_new

# Pull base (~9 GB, ~3 min on gigabit)
ollama pull qwen2.5-coder:14b

# Build the tuned custom model
ollama create qwen-tartaria -f tools\local-llm\win\Modelfile.qwen-tartaria

# Smoke test
ollama run qwen-tartaria "Write a Unity 6 C# stub that logs Hello on Awake."
# Expected: ~30 tok/s, real MonoBehaviour with using/namespace, ready in 5-8 sec
```

### Step 3 — Verify GPU usage

```powershell
# In one terminal:
ollama run qwen-tartaria "test"

# In another:
ollama ps
# PROCESSOR column should show "100% GPU" or similar — anything other than
# "CPU" means the GPU is doing the work. If it shows "CPU", restart Ollama:
#   Right-click Ollama tray icon → Quit → Open Ollama app again
```

### Step 4 — Verify the apply_outputs path

```powershell
# Python must be on PATH (any 3.8+)
python --version

# Quick test of the apply pipeline:
echo '```csharp' > LOCAL_OUTPUTS\test\response.md
echo '// File: TEST_DELETE_ME.cs' >> LOCAL_OUTPUTS\test\response.md
echo '// hi' >> LOCAL_OUTPUTS\test\response.md
echo '```' >> LOCAL_OUTPUTS\test\response.md
python tools\local-llm\apply_outputs.py LOCAL_OUTPUTS\test\response.md
# Should create TEST_DELETE_ME.cs at repo root with just the // hi line.
# Delete it after smoke test.
```

### Step 5 — First real run

Drop one test ticket and fire the loop manually:

```powershell
@'
# Ticket 001: Add a doc comment to CameraController

**Destination file**: `Assets/_Project/Scripts/Camera/CameraController.cs`
**Change type**: edit class header

## Spec
Add an XML doc comment above the class declaration explaining that this is
the canonical third-person camera controller, F310 right-stick orbits,
R3 click recenters. Cite docs\appendices\D_CONTROLS_F310.md.

## Output format
Output the full file content as a single fenced csharp block prefixed with
`// File: Assets/_Project/Scripts/Camera/CameraController.cs`.
'@ | Set-Content tools\local-llm\LOCAL_TASKS\001_camera_doccomment.md

# Run it
.\tools\local-llm\win\RUN_LOOP.bat
```

After it finishes:
- `tools\local-llm\LOCAL_TASKS\_done\001_camera_doccomment.md` exists
- `Assets\_Project\Scripts\Camera\CameraController.cs` has new XML doc lines
- `Logs\local-llm\run_loop.log` shows the run

If yes — you're operational.

### Step 6 — Continuous mode

Once you've verified Step 5, run continuously:

```powershell
.\tools\local-llm\win\RUN_LOOP.bat continuous
```

This loops forever — every 60 seconds it scans `LOCAL_TASKS\` for new
tickets, runs Ollama on each, applies the output, and (because `all` mode
is implied via the continuous wrapper) optionally fires Unity smoke shots
and Blender batches. Ctrl+C to stop.

Leave this in a PowerShell window all day. Claude supervises every 15
minutes from a scheduled task.

---

## Scheduled Claude supervision

The scheduled task you accepted at setup fires every 15 minutes. When it
fires, Claude reads `tools\local-llm\win\supervisor_briefing.md` to know
what to do, then executes a 5-step check-in protocol:

1. **Inventory** — what tickets shipped, what failed, what compiled
2. **Verify compile** — `mcp__unity-tartaria__read_console`
3. **Look at latest smoke shot** — visible progress?
4. **Queue next batch** — 5-10 new tickets from the punch list
5. **Bump STATUS.md** — one-line supervisor feed entry

Total per check-in: ~2-5 min, under 5K tokens. At 4 check-ins/hour ×
24 hours = ~96 check-ins/day × 5K tokens = ~480K tokens/day. Versus the
current Cowork loop which can burn 100K+ per round of hand-driving the
Editor.

You can change the cadence later with `mcp__scheduled-tasks__update_scheduled_task`.

---

## Ticket conventions

Tickets live at `tools\local-llm\LOCAL_TASKS\NNN_short_name.md`.

The body should follow the template in `tools\local-llm\win\supervisor_briefing.md`
§ Ticket format. Key rules:

- **Pre-grep** symbol citations before writing the ticket — Qwen will
  hallucinate `RaiseFooBar()` if you don't anchor it.
- **One file per ticket** — don't ask the model to modify 3 files at once.
- **Single fenced code block** in the output with `// File:` header so
  `apply_outputs.py` knows where to route it.
- **Spec citation** — cite `docs/15` § or `CLAUDE.md` punch list item.

---

## Day-to-day workflow

1. **NATRIX wakes up, opens repo, types `RUN_LOOP.bat continuous` in
   PowerShell.** Walks away.
2. **Claude supervisor wakes every 15 min**, reviews log + diff, queues
   next tickets, bumps STATUS.md.
3. **NATRIX checks back in periodically**, reads STATUS.md supervisor
   feed bottom-up. If something's off, he Ctrl+Cs the runner, fixes the
   bad ticket, restarts.
4. **End of day**: NATRIX does `git status` to see total session yield,
   commits, optionally pushes.

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| `Ensure-Ollama` keeps starting daemon every loop | Ollama daemon dies on Windows after a while. Add it to startup: `winget install --id Ollama.Ollama` does this automatically. |
| Model produces broken JSON when the runner uses the API | Likely a malformed prompt. Check `Logs\local-llm\<ticket>.err` — re-roll with corrected spec. |
| `apply_outputs.py` can't find the destination file | Ticket forgot the `// File: <path>` header — Qwen probably dropped it. Re-roll. |
| All tickets going to `_failed/` | Ollama daemon isn't responding on 11434. Restart Ollama tray app. |
| Continuous mode CPU pinned at 100% | Ollama is running on CPU not GPU. Check `ollama ps`. |
| Unity smoke shot is solid black | You passed `-nographics` — don't (Mac+Windows both need GPU on for screenshot). Edit `run_loop.ps1` `$UnityArgs`. |
| Tickets pile up faster than Qwen processes | Slow down ticket authoring (smaller batches per supervisor check-in), or upgrade to `qwen3-coder:30b` MoE for ~2× throughput. |
| Compile breaks after Qwen's last write | Revert via `git checkout -- <file>`, queue corrected re-roll ticket with explicit grep citations. |

---

## What "supervised autonomous" means

You (the human) ship spec docs and approve commits. Claude (the supervisor)
authors tickets and verifies compiles. Qwen (the implementer) writes code.
The loop runs while you sleep. STATUS.md tells you the story when you
wake up.

This is not "AI builds your game while you do nothing." It's a sharper
**you + reviewer + implementer** stack where the reviewer pre-empties
the cycles that burned the most expensive tokens before.

---

*LOCAL_LOOP_LAPTOP.md v1.0 · 2026-06-05 · Local loop, scheduled supervisor, fewer tokens.*
