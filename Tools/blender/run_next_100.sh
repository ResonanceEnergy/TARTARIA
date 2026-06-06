#!/usr/bin/env bash
# Master runner for the NEXT 100 ASSETS batch (Logitech F310 + Blender pipeline session).
# Spawns one Blender process per gen script to avoid namespace pollution.
set -u
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BLENDER="${BLENDER_BIN:-/tmp/blender-4.5.4-linux-x64/blender-launcher}"
if [ ! -x "$BLENDER" ]; then
  BLENDER="${BLENDER:-C:/Program Files/Blender Foundation/Blender 4.5/blender.exe}"
fi
export TARTARIA_ROOT="${TARTARIA_ROOT:-$(cd "$SCRIPT_DIR/../.." && pwd)}"

SCRIPTS=(
  gen_characters_humanoid.py
  gen_characters_enemies.py
  gen_buildings_village.py
  gen_buildings_special.py
  gen_props_tools.py
  gen_props_furniture_set2.py
  gen_props_ritual.py
  gen_minigame_props.py
  gen_extras_utility.py
)

ok=0; fail=0; total_assets=0
for s in "${SCRIPTS[@]}"; do
  if [ -f "$SCRIPT_DIR/$s" ]; then
    echo "=== $s ==="
    "$BLENDER" --background --python "$SCRIPT_DIR/$s" 2>&1 | grep -E "done|ERROR|Traceback" | head -3
    if [ ${PIPESTATUS[0]} -eq 0 ]; then ((ok++)); else ((fail++)); fi
  fi
done
echo "Next-100 batch summary: $ok ok, $fail failed"
