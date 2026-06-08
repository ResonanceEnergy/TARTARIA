"""
Save Shrine — canon save point per docs/15 §1 + tech spec.

Small 1.2m statue: ornate stone pillar with central gold orb + ring base.
Player interact = save game.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    # Wide stone disc base
    base = cylinder_y("Base", (0, 0.10, 0), 0.55, 0.20)
    parts.append(base)
    # Mid drum
    drum = cylinder_y("Drum", (0, 0.40, 0), 0.30, 0.40)
    parts.append(drum)
    # Ornate column tapered
    column = cylinder_y("Column", (0, 0.80, 0), 0.18, 0.40)
    parts.append(column)
    # Floating gold orb (the save anchor)
    orb = uv_orb("Orb", (0, 1.10, 0), 0.18)
    parts.append(orb)
    # Ring around orb (hovering)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.25, minor_radius=0.03, major_segments=24, minor_segments=6, location=(0, 1.10, 0))
    ring = bpy.context.active_object
    ring.name = "Ring"
    parts.append(ring)
    mat_stone = make_character_mat("Shrine_Stone", (0.48, 0.45, 0.40, 1.0), roughness=0.9)
    mat_gold = make_aether_emissive("Shrine_Gold", AETHER_GOLD, 5.0)
    for p in parts:
        if p.name == "Orb" or p.name == "Ring":
            p.data.materials.append(mat_gold)
        else:
            p.data.materials.append(mat_stone)
    o = join_character(parts, "Prop_SaveShrine")
    save_and_export(o, "Prop_SaveShrine")

if __name__ == "__main__":
    main()
