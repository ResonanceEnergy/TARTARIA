# MAC STUDIO HANDOFF — TARTARIA autonomous build on Apple Silicon

> Authored 2026-06-05. Single source of truth for moving the autonomous
> content-build loop from Cowork-in-Windows to Mac Studio + Ollama + Qwen.
> Read this end to end before clicking anything.

---

## Why this exists

The Cowork-driven loop (write → bake → place → screenshot → verify) works
but each cycle costs real Claude tokens. NATRIX wants the loop to run
locally on Mac Studio against Ollama, with Claude / Cowork supervising
once per session rather than driving every prefab placement.

This doc tells you (a) exactly what to install on the Mac, (b) what files
to copy or sync, (c) what to run first, and (d) how to keep iterating.

---

## Architecture — what changes vs Windows

| Layer | Windows (current) | Mac Studio (new) |
|---|---|---|
| Runner | `tools/local-llm/RUN_OLLAMA_TICKETS.bat` | `tools/local-llm/mac/run_tickets.sh` |
| Default model | `qwen2.5-coder:1.5b` (CPU) | **`qwen3-tartaria`** custom-tuned MoE (Metal/MLX) |
| Ollama backend | CPU + optional AMD GPU | Apple Silicon MLX (auto, Ollama ≥0.19) |
| Apply outputs | `tools/local-llm/apply_outputs.py` | same file, no port needed (Python is cross-platform) |
| Unity validation | manual hit-Play | `Unity -batchmode -executeMethod Tartaria.Editor.AutoLoop.RunSmokeShot -force-metal -quit` |
| Blender batch | Editor menu `Tartaria/4 Generate Art/Blender — Moon 1` | `/Applications/Blender.app/Contents/MacOS/Blender --background --python tools/blender/run_all_moon1.py` |
| Screenshot | Editor-time via Cowork MCP | Editor-time via `Camera.targetTexture → ReadPixels → EncodeToPNG` in `AutoLoop.cs` |
| Tickets | drop into `tools/local-llm/LOCAL_TASKS/` | same dir, both platforms |
| Outputs | `tools/local-llm/LOCAL_OUTPUTS/<ticket>/response.md` | same |

---

## Why Qwen3-Coder 30B instead of Qwen2.5-Coder

`qwen3-coder:30b` is a **Mixture-of-Experts** model: 30B total params, only
3.3B active per token. On Apple Silicon with MLX backend that lands at
~70–100 tokens/sec on M2 Ultra, which is dense-32B quality at dense-7B
speed. The 256K native context fits an entire .cs file + ticket + 3-4
related files in one prompt — no more "model truncated my response"
follow-up cycles like the Windows runs at 2048 ctx.

Disk: ~19 GB. RAM during inference: ~24–28 GB with the custom 32768
context window. Comfortable alongside Unity + Blender on a 64 GB Mac
Studio.

Fallback model kept on disk: `qwen2.5-coder:14b` (~9 GB) — use for tickets
that don't need long context.

---

## Step-by-step: NATRIX day-1 on Mac Studio

### Step 0 — On Windows: commit + push current work

The autonomous content session run 2026-06-05 left ~11 new Blender
scripts, 1 audio gen script, the Mac handoff files, and v11–v22
screenshots. **They are not yet pushed.** From Windows PowerShell:

```powershell
cd C:\dev\TARTARIA_new

# Stage the new content
git add tools/blender/gen_oak_tree.py
git add tools/blender/gen_pine_tree.py
git add tools/blender/gen_bush_clump.py
git add tools/blender/gen_windmill_blade.py
git add tools/blender/gen_village_fountain.py
git add tools/blender/gen_cart_wagon.py
git add tools/blender/gen_market_stall.py
git add tools/blender/gen_wooden_fence.py
git add tools/blender/gen_lantern_post.py
git add tools/blender/gen_village_bell.py
git add tools/blender/run_all_moon1.py
git add tools/audio/gen_seventeenth_hour.py
git add tools/local-llm/mac/
git add Assets/_Project/Scripts/Editor/AutoLoop.cs
git add Assets/_Project/Scripts/Editor/BlenderImportPostprocessor.cs
git add Assets/_Project/Scripts/Integration/EchohavenCombatArena.cs
git add docs/MAC_STUDIO_HANDOFF.md
git add STATUS.md
git add Logs/screenshots/v1*_*.png Logs/screenshots/v2*_*.png

# Optional: also stage the scene if you want it persisted
git add Assets/_Project/Scenes/Echohaven_VerticalSlice.unity

# Commit
git -c user.email="nate@gripandripphdd.com" commit -m "Moon 1 Deep Hammer + Mac handoff"

# Push (LFS hooks will upload FBX bodies if any new ones)
git push origin feature/consolidate-moon-architecture
```

Verify with `git log --oneline -1` that your commit is on top.

### Step 1 — Install prerequisites on Mac

```bash
# Homebrew (if missing)
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Git + LFS
brew install git git-lfs
git lfs install                              # one-time per machine
git config --global core.autocrlf input      # Unity script line-ending sanity

# Ollama (must be ≥0.19 for MLX backend)
brew install --cask ollama
open /Applications/Ollama.app                # starts the background daemon
ollama --version                             # confirm ≥0.19.0
```

### Step 2 — Pull the models

```bash
ollama pull qwen3-coder:30b      # ~19 GB, primary
ollama pull qwen2.5-coder:14b    # ~9 GB, fallback
ollama list                      # both should show
```

### Step 3 — Clone the repo

```bash
mkdir -p ~/dev && cd ~/dev
git clone https://github.com/ResonanceEnergy/TARTARIA.git TARTARIA_new
cd TARTARIA_new
git checkout feature/consolidate-moon-architecture   # or main, whichever has your push
git lfs pull                                          # pull FBX bodies, not the 130-byte stubs

# Sanity check — these should be MB-scale, not 130 bytes:
ls -lh Assets/_Project/Models/Blender/Moon1/*.fbx | head -5
```

### Step 4 — Build the custom Ollama model

```bash
cd ~/dev/TARTARIA_new
ollama create qwen3-tartaria -f tools/local-llm/mac/Modelfile.qwen3-tartaria

# Smoke
ollama run qwen3-tartaria "Write a Unity 6 C# stub that logs Hello on Awake."
# Expected: real MonoBehaviour with using/namespace, generated in <5 sec
```

### Step 5 — Verify GPU acceleration

```bash
# In one terminal:
ollama run qwen3-tartaria "test" &
# In another:
ollama ps
# The PROCESSOR column must show 100% GPU for qwen3-tartaria.
# If it shows CPU: brew upgrade --cask ollama, restart Ollama.app.
```

### Step 6 — Install Unity 6.3.6f1 LTS

Use Unity Hub. Pick the **Apple Silicon (arm64)** installer.
Open the TARTARIA project. Let Library/ regenerate — ~20 min the first
time on a fresh clone with full LFS assets.

Verify after import: open the Echohaven_VerticalSlice scene, console
shows 0 errors.

### Step 7 — Make the run script executable

```bash
cd ~/dev/TARTARIA_new
chmod +x tools/local-llm/mac/run_tickets.sh

# Verify the bash entrypoint syntax
bash -n tools/local-llm/mac/run_tickets.sh && echo "OK"
```

### Step 8 — Drop a smoke ticket and run

```bash
mkdir -p tools/local-llm/LOCAL_TASKS
cat > tools/local-llm/LOCAL_TASKS/000_mac_smoke.md <<'EOF'
# Mac smoke ticket
Add the comment "// hello mac" as the first line of
Assets/_Project/Scripts/Camera/CameraController.cs.
Output the full file content.
EOF

./tools/local-llm/mac/run_tickets.sh
```

Then verify:
- `Assets/_Project/Scripts/Camera/CameraController.cs` has the new comment line.
- `tools/local-llm/LOCAL_TASKS/_done/000_mac_smoke.md` exists.
- `tools/local-llm/LOCAL_OUTPUTS/000_mac_smoke/response.md` contains the model's full output.

### Step 9 — Full loop with Unity validation

```bash
RUN_SMOKE=1 ./tools/local-llm/mac/run_tickets.sh
```

This will fire all pending tickets, then open Unity headless, call
`AutoLoop.RunSmokeShot`, render the scene at 1920×1080, save to
`Logs/smoke-shots/shot_*.png`, and exit. Total time: ~30s + ticket count.

### Step 10 — Full loop with Blender regen

```bash
RUN_SMOKE=1 RUN_BLENDER=1 ./tools/local-llm/mac/run_tickets.sh
```

This adds a Blender headless batch run against
`tools/blender/run_all_moon1.py`, which re-generates every FBX from
its Python source. Useful when a ticket modifies a `gen_*.py` script.

---

## What Claude / Cowork supervises (no longer drives)

After this handoff, Cowork is for:

1. **Spec interpretation** — read docs/15, audit gap list, write new ticket .md files.
2. **Architecture review** — when Qwen produces something that compiles but is wrong shape.
3. **Debugging hard bugs** — when 3 ticket cycles fail to fix the same issue.
4. **Cross-Moon coordination** — Moon 1 → Moon 2 spec port, MOON_BLUEPRINT template work.
5. **Quarterly STATUS.md + CLAUDE.md refreshes.**

Cowork does NOT:
- Generate new C# files (Qwen does).
- Edit Blender gen scripts (Qwen does).
- Run the per-prefab placement loop (run_tickets.sh + Unity does).
- Take screenshots (AutoLoop.cs does).

---

## Ticket authoring conventions

Tickets live at `tools/local-llm/LOCAL_TASKS/NNN_short_name.md`.

The runner pipes the **entire ticket body** to `ollama run qwen3-tartaria`
on stdin. So the ticket body should:

1. Tell Qwen exactly which file to read or write (full path).
2. Describe the change in 2-5 sentences.
3. Cite any related files Qwen should grep first (API_CONTRACT v2).
4. Specify whether to output a code block, a full file, or both.

Example:
```
# Ticket 042: Add VillageFenceTrigger.cs

Create a new MonoBehaviour at
Assets/_Project/Scripts/Integration/VillageFenceTrigger.cs that fires
GameEvents.RaiseQuestObjective("inspect_fences") when player enters
the BoxCollider with isTrigger=true. Subscribe-side already exists at
QuestObjectiveTrackerUI.cs:148 — grep it before writing.

Output the full file content.
```

`apply_outputs.py` scans `response.md` for fenced code blocks with
file-path headers (e.g. `## File: Assets/_Project/Scripts/...`) and
writes them. See its inline docs.

---

## Files to copy if you DON'T want to use git

Some users prefer rsync to seed the first Mac clone, then switch to git
for ongoing sync. The full minimal set is:

```
tools/blender/                            # 30+ gen_*.py + _common.py
tools/audio/gen_seventeenth_hour.py
tools/local-llm/apply_outputs.py
tools/local-llm/mac/                      # new this commit
Assets/_Project/                          # entire project, ~3-6 GB with LFS
ProjectSettings/                          # Quality, Input, URP, etc.
Packages/                                 # manifest.json + lock
docs/                                     # all .md including this file
CLAUDE.md
STATUS.md
ROADMAP.md
```

You do NOT need (let Mac regenerate):
```
Library/      # Unity cache, NOT portable Win↔Mac
Temp/         # transient
Logs/         # logs are per-machine; bring Logs/screenshots/ if you want history
Builds/       # never sync builds; rebuild on Mac
obj/          # Visual Studio cache
```

---

## Troubleshooting

| Symptom | Fix |
|---|---|
| `ollama: command not found` | `brew install --cask ollama` then `open /Applications/Ollama.app` once |
| `ollama ps` shows CPU not GPU | Need Ollama ≥0.19. `brew upgrade --cask ollama`. |
| First Unity import takes >40 min | Spotlight is indexing `~/dev`. Exclude in System Settings → Siri & Spotlight → Privacy. |
| FBX files are 130 bytes | `git lfs pull` wasn't run. Do it now. |
| Smoke shot is solid black | You passed `-nographics`. Remove it. Mac needs `-force-metal` instead. |
| Ticket produces garbage code | Lower `temperature` in Modelfile to 0.1, or switch to `qwen2.5-coder:14b` for that ticket |
| Unity Hub can't find Apple Silicon installer | Choose 6000.3.6f1 specifically — earlier versions weren't arm64. |
| `apply_outputs.py: ModuleNotFoundError` | `python3 -m pip install -r tools/local-llm/requirements.txt` (if a requirements file is added) |
| Library regen takes >40 min on 2nd run | `rm -rf Library/` then reopen — sometimes the cache corrupts. |

---

## Memory budget cheat sheet (64 GB Mac Studio)

| Process | Idle | Active |
|---|---|---|
| Ollama daemon | 1 GB | 1 GB |
| qwen3-tartaria loaded @ 32K ctx | — | 24–28 GB |
| Unity 6.3 Editor (Echohaven scene) | 4 GB | 8–12 GB |
| Blender background batch (burst) | 0 | 2–4 GB |
| macOS + Finder + system | 8 GB | 8 GB |
| **Total under full load** | — | **~50 GB** |
| Headroom on 64 GB | — | ~14 GB |

On 128 GB: bump `num_ctx` to 65536 or 131072 in the Modelfile for even
longer file contexts. Or run two parallel Ollama workers on different ports.

---

## When you sit back down at the Mac

```bash
cd ~/dev/TARTARIA_new
git pull
git lfs pull          # only if new LFS objects landed
ollama ps             # verify daemon is up
ls tools/local-llm/LOCAL_TASKS/   # see pending tickets
RUN_SMOKE=1 ./tools/local-llm/mac/run_tickets.sh
```

That's the entire daily workflow. Tickets in. Code + screenshot out.
Cowork supervisor only when something doesn't compile or makes no sense.

---

*MAC_STUDIO_HANDOFF.md v1.0 · 2026-06-05 · Ship the loop, not the tokens.*
