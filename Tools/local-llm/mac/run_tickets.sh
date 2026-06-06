#!/bin/bash
# TARTARIA Mac autonomous ticket runner.
#
# Replaces tools/local-llm/RUN_OLLAMA_TICKETS.bat (Windows).
# Uses Ollama 0.19+ MLX backend on Apple Silicon for ~70-100 tok/s on
# qwen3-coder:30b. See docs/MAC_STUDIO_HANDOFF.md for full setup.
#
# Usage:
#   ./tools/local-llm/mac/run_tickets.sh                # just LLM apply
#   RUN_SMOKE=1   ./tools/local-llm/mac/run_tickets.sh  # + Unity smoke shot
#   RUN_BLENDER=1 ./tools/local-llm/mac/run_tickets.sh  # + Blender batch
#   RUN_SMOKE=1 RUN_BLENDER=1 ./tools/local-llm/mac/run_tickets.sh  # full loop
#
# Created 2026-06-05 for Mac Studio handoff.

set -euo pipefail

# --- config ---
REPO_ROOT="${TARTARIA_ROOT:-$HOME/dev/TARTARIA_new}"
MODEL="${MODEL:-qwen3-tartaria}"
UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity}"
BLENDER_BIN="${BLENDER_BIN:-/Applications/Blender.app/Contents/MacOS/Blender}"
TASKS_DIR="$REPO_ROOT/tools/local-llm/LOCAL_TASKS"
OUTPUTS_DIR="$REPO_ROOT/tools/local-llm/LOCAL_OUTPUTS"
LOG_DIR="$REPO_ROOT/Logs/local-llm"
SLEEP_BETWEEN="${SLEEP_BETWEEN:-5}"
MAX_TICKETS_PER_RUN="${MAX_TICKETS_PER_RUN:-20}"

# --- pre-flight ---
mkdir -p "$OUTPUTS_DIR" "$LOG_DIR" "$TASKS_DIR/_done" "$TASKS_DIR/_failed"
cd "$REPO_ROOT"

# Ollama daemon
if ! pgrep -x ollama >/dev/null; then
  echo "[run_tickets] Starting Ollama daemon..."
  ollama serve >"$LOG_DIR/ollama.log" 2>&1 &
  sleep 3
fi

# Model presence
if ! ollama list | grep -q "^${MODEL}"; then
  echo "[run_tickets] ERROR: model '$MODEL' not found."
  echo "  Build it with: ollama create $MODEL -f tools/local-llm/mac/Modelfile.qwen3-tartaria"
  exit 1
fi

# --- ticket loop ---
COUNT=0
for ticket in "$TASKS_DIR"/*.md; do
  [ -e "$ticket" ] || { echo "[run_tickets] No tickets pending."; break; }

  # Skip subfolders pattern matches
  case "$ticket" in *_done* | *_failed*) continue;; esac

  COUNT=$((COUNT + 1))
  if [ $COUNT -gt $MAX_TICKETS_PER_RUN ]; then
    echo "[run_tickets] Hit MAX_TICKETS_PER_RUN cap ($MAX_TICKETS_PER_RUN). Stopping."
    break
  fi

  TICKET_NAME=$(basename "$ticket" .md)
  OUT_DIR="$OUTPUTS_DIR/$TICKET_NAME"
  mkdir -p "$OUT_DIR"

  echo "[run_tickets] ($COUNT) $TICKET_NAME — generating..."

  if ollama run "$MODEL" < "$ticket" > "$OUT_DIR/response.md" 2> "$LOG_DIR/$TICKET_NAME.err"; then
    echo "[run_tickets] ($COUNT) $TICKET_NAME — applying..."
    if python3 tools/local-llm/apply_outputs.py "$OUT_DIR/response.md"; then
      mv "$ticket" "$TASKS_DIR/_done/"
    else
      echo "[run_tickets] apply_outputs.py failed for $TICKET_NAME"
      mv "$ticket" "$TASKS_DIR/_failed/"
    fi
  else
    echo "[run_tickets] Ollama error on $TICKET_NAME (see $LOG_DIR/$TICKET_NAME.err)"
    mv "$ticket" "$TASKS_DIR/_failed/"
  fi

  sleep "$SLEEP_BETWEEN"
done

# --- optional: Unity smoke shot ---
if [ "${RUN_SMOKE:-0}" = "1" ]; then
  if [ ! -x "$UNITY_BIN" ]; then
    echo "[run_tickets] WARN: Unity not at $UNITY_BIN — skipping smoke."
  else
    echo "[run_tickets] Running Unity smoke shot..."
    "$UNITY_BIN" \
      -batchmode \
      -projectPath "$REPO_ROOT" \
      -executeMethod Tartaria.Editor.AutoLoop.RunSmokeShot \
      -force-metal \
      -logFile "$LOG_DIR/unity-smoke.log" \
      -quit \
      || echo "[run_tickets] WARN: Unity smoke exited non-zero (see $LOG_DIR/unity-smoke.log)"
  fi
fi

# --- optional: Blender batch ---
if [ "${RUN_BLENDER:-0}" = "1" ]; then
  if [ ! -x "$BLENDER_BIN" ]; then
    echo "[run_tickets] WARN: Blender not at $BLENDER_BIN — skipping batch."
  else
    echo "[run_tickets] Running Blender batch..."
    "$BLENDER_BIN" --background --python tools/blender/run_all_moon1.py \
      >> "$LOG_DIR/blender.log" 2>&1 \
      || echo "[run_tickets] WARN: Blender batch exited non-zero"
  fi
fi

echo "[run_tickets] Done. Processed $((COUNT > 0 ? COUNT : 0)) tickets."
