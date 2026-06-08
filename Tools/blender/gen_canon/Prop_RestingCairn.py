"""
Resting Cairn — stacked stone pile per docs/15 (pilgrim marker).

Small 0.8m cairn (stacked stones), gold capstone.
Scattered along paths.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, ico_orb, uv_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    # 5 stacked stones decreasing
    sizes = [0.30, 0.25, 0.22, 0.18, 0.13]
    y = 0
    for i, r in enumerate(sizes):
        y += r
        stone = ico_orb(f"Stone_{i}", (0, y, 0), r)
        stone.scale = (1.1, 0.7, 1.0)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        parts.append(stone)
        y += r * 0.7
    # Capstone — small gold orb
    cap = uv_orb("Capstone", (0, y + 0.08, 0), 0.08)
    parts.append(cap)
    mat_stone = make_character_mat("Cairn_Stone", (0.42, 0.38, 0.34, 1.0), roughness=0.95)
    mat_cap = make_aether_emissive("Cairn_Cap", AETHER_GOLD, 3.5)
    for p in parts:
        if p.name == "Capstone":
            p.data.materials.append(mat_cap)
        else:
            p.data.materials.append(mat_stone)
    o = join_character(parts, "Prop_RestingCairn")
    save_and_export(o, "Prop_RestingCairn")

if __name__ == "__main__":
    main()
