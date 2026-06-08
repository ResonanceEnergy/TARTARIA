"""
Prophecy Obelisk — lore-beat monolith, scattered around Echohaven.

Per docs/15 + lore bible: 3-6-9 prophecy stones carved with sacred geometry.
Tall narrow stone (3m), gold-emissive glyph rings at φ-ratio heights.
Player reads = unlocks prophecy fragment quest beat.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Prop_ProphecyObelisk] 3m carved menhir")

    parts = []

    # Main column — tall narrow tapered hex
    col_base = cube_at("ColBase", (0, 0.10, 0), (0.55, 0.20, 0.55))
    parts.append(col_base)

    col = cylinder_y("Column", (0, 1.55, 0), 0.22, 2.80)
    parts.append(col)

    # 3 horizontal gold rings at φ-derived heights (3-6-9 rhythm)
    ring_heights = [0.40, 1.40, 2.50]
    for i, y in enumerate(ring_heights):
        bpy.ops.mesh.primitive_torus_add(
            major_radius=0.28, minor_radius=0.04, major_segments=20, minor_segments=6,
            location=(0, y, 0),
        )
        r = bpy.context.active_object
        r.name = f"Ring_{i}"
        parts.append(r)

    # Apex — capped pyramid
    apex = cube_at("Apex", (0, 3.05, 0), (0.30, 0.16, 0.30))
    apex.scale = (1.0, 1.0, 1.0)
    parts.append(apex)
    # Apex orb gold
    apex_orb = uv_orb("ApexOrb", (0, 3.20, 0), 0.10)
    parts.append(apex_orb)

    # Materials
    mat_stone = make_character_mat("Obelisk_Stone", (0.45, 0.42, 0.38, 1.0), roughness=0.95)
    mat_ring = make_aether_emissive("Obelisk_Ring", AETHER_GOLD, 3.0)

    for p in parts:
        if p.name.startswith("Ring_") or p.name == "ApexOrb":
            p.data.materials.append(mat_ring)
        else:
            p.data.materials.append(mat_stone)

    o = join_character(parts, "Prop_ProphecyObelisk")
    save_and_export(o, "Prop_ProphecyObelisk")


if __name__ == "__main__":
    main()
