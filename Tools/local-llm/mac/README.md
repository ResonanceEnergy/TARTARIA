# tools/local-llm/mac/

Mac Studio (Apple Silicon) entrypoint for the autonomous TARTARIA build loop.

## Files

- `run_tickets.sh` — bash entrypoint replacing the Windows .bat
- `Modelfile.qwen3-tartaria` — custom Ollama model tuned for Unity C# tickets

## Quick start

See `docs/MAC_STUDIO_HANDOFF.md` for full setup.

Three-line summary once installed:

```bash
ollama create qwen3-tartaria -f tools/local-llm/mac/Modelfile.qwen3-tartaria
chmod +x tools/local-llm/mac/run_tickets.sh
RUN_SMOKE=1 ./tools/local-llm/mac/run_tickets.sh
```

## Why a separate entrypoint instead of porting the .bat

- Bash `set -euo pipefail` gives proper failure modes that .bat can't match.
- launchd/cron scheduling is trivial from a bash script.
- The `_done/` and `_failed/` ticket housekeeping pattern is filesystem-native here.
- Shared `apply_outputs.py` is platform-agnostic, so no Python is duplicated.

## Environment variables you can override

| Var | Default | Purpose |
|---|---|---|
| `TARTARIA_ROOT` | `$HOME/dev/TARTARIA_new` | repo root |
| `MODEL` | `qwen3-tartaria` | which Ollama model to call |
| `UNITY_BIN` | `/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity` | Unity binary path |
| `BLENDER_BIN` | `/Applications/Blender.app/Contents/MacOS/Blender` | Blender binary path |
| `SLEEP_BETWEEN` | 5 | seconds between tickets (rate limit) |
| `MAX_TICKETS_PER_RUN` | 20 | thermal SLA cap |
| `RUN_SMOKE` | 0 | set to 1 to fire Unity AutoLoop.RunSmokeShot after tickets |
| `RUN_BLENDER` | 0 | set to 1 to fire Blender batch after tickets |
