"""
Plaza Stairs — canon architectural stair-step from terrain to Dome plaza level.

7-step descending stair, 3m wide, gold seam at each step nosing.
Player walks down toward buried Dome entrance.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    # 7 steps descending
    for i in range(7):
        y = -0.15 * i + 0.075
        z = 0.30 * i
        step = cube_at(f"Step_{i}", (0, y, z), (3.0, 0.30, 0.30))
        parts.append(step)
        # Gold nosing line at front edge
        nose = cube_at(f"Nose_{i}", (0, y + 0.16, z - 0.12), (2.9, 0.04, 0.02))
        parts.append(nose)
    # Side rails
    for sx in [-1.55, 1.55]:
        rail = cube_at(f"Rail_{sx}", (sx, 0.10, 0.90), (0.10, 0.40, 2.0))
        rail.rotation_euler = (math.radians(-26), 0, 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(rail)
    mat_stone = make_character_mat("Stairs_Stone", (0.50, 0.47, 0.42, 1.0), roughness=0.92)
    mat_nose = make_aether_emissive("Stairs_Nose", AETHER_GOLD, 2.5)
    for p in parts:
        if p.name.startswith("Nose_"):
            p.data.materials.append(mat_nose)
        else:
            p.data.materials.append(mat_stone)
    o = join_character(parts, "Prop_PlazaStairs")
    save_and_export(o, "Prop_PlazaStairs")

if __name__ == "__main__":
    main()
