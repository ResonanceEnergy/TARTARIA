"""
Aether Crystal cluster — small collectible per docs/15 §1.

Cyan emissive crystal shards on a small matte stone base.
0.4m tall total, used as RS pickup / Day 1-5 collectibles.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_CYAN, AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Prop_AetherCrystal] collectible cluster")

    parts = []

    # Stone base — small flat hex disc
    base = cube_at("Base", (0, 0.04, 0), (0.30, 0.08, 0.30))
    parts.append(base)

    # 5 crystal shards rising at angles
    shard_specs = [
        (0, 0.20, 0, 0.06, 0.30, 0),
        (0.10, 0.18, 0.08, 0.05, 0.26, 15),
        (-0.10, 0.18, 0.08, 0.05, 0.26, -15),
        (0.08, 0.18, -0.10, 0.04, 0.22, 25),
        (-0.08, 0.18, -0.10, 0.04, 0.22, -25),
    ]
    for i, (x, y, z, w, h, rot) in enumerate(shard_specs):
        shard = cube_at(f"Shard_{i}", (x, y, z), (w, h, w))
        shard.rotation_euler = (math.radians(rot), math.radians(i * 30), math.radians(rot))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(shard)

    # Materials
    mat_stone = make_character_mat("AetherCrystal_Base", (0.45, 0.42, 0.40, 1.0), roughness=0.9)
    mat_cyan = make_aether_emissive("AetherCrystal_Shard", AETHER_CYAN, 4.0)

    for p in parts:
        if p.name == "Base":
            p.data.materials.append(mat_stone)
        else:
            p.data.materials.append(mat_cyan)

    o = join_character(parts, "Prop_AetherCrystal")
    save_and_export(o, "Prop_AetherCrystal")


if __name__ == "__main__":
    main()
