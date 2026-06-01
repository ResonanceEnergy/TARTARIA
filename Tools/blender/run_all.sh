#!/usr/bin/env bash
# Master runner — spawns one Blender process per script to avoid namespace pollution
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BLENDER="${BLENDER_BIN:-/tmp/blender-4.5.4-linux-x64/blender-launcher}"
if [ ! -x "$BLENDER" ]; then
  # Fall back to Windows blender if running from there
  BLENDER="${BLENDER:-C:/Program Files/Blender Foundation/Blender 4.5/blender.exe}"
fi
export TARTARIA_ROOT="${TARTARIA_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

SCRIPTS=(
  gen_anastasia_chair.py gen_brazier.py gen_aether_crystals.py
  gen_bobs_inn.py gen_tuning_pedestal.py gen_mud_pool_basin.py
  gen_lore_artifact_scroll.py gen_giant_skeleton_key.py
  gen_skeleton_remains.py gen_pipe_organ.py
  gen_moon1_polish.py gen_moon1_furniture.py gen_moon1_misc.py
  gen_moon2_set.py gen_moon3_set.py gen_moon4_set.py
  gen_moon5_set.py gen_moon6_set.py gen_moon7_set.py
  gen_moon8_set.py gen_moon9_set.py gen_moon10_set.py
  gen_moon11_set.py gen_moon12_set.py gen_moon13_set.py
  gen_shared_props.py
)

ok=0; fail=0
for s in "${SCRIPTS[@]}"; do
  if [ -f "$SCRIPT_DIR/$s" ]; then
    echo "=== $s ==="
    "$BLENDER" --background --python "$SCRIPT_DIR/$s" 2>&1 | grep -E "done|ERROR|Traceback" | head -3
    if [ ${PIPESTATUS[0]} -eq 0 ]; then ((ok++)); else ((fail++)); fi
  fi
done
echo "Batch summary: $ok ok, $fail failed"
