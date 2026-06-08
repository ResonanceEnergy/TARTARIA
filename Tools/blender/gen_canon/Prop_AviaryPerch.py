"""
Aviary Perch — bird/spirit perch for Milo's family per docs/15 + lore.

Tall thin wooden post (2.5m) with horizontal top bar, suggestion of cloth banners.
Each plaza zone gets one. Ambient scene dressing.
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
    # Tall wooden post
    post = cylinder_y("Post", (0, 1.25, 0), 0.05, 2.50)
    parts.append(post)
    # Top crossbar
    cross = cube_at("CrossTop", (0, 2.50, 0), (0.90, 0.05, 0.05))
    parts.append(cross)
    # Hanging cloth banner triangles
    for sx in [-0.30, 0.30]:
        banner = cube_at(f"Banner_{sx}", (sx, 2.25, 0.05), (0.18, 0.30, 0.01))
        parts.append(banner)
    # Top finial — gold ball
    finial = uv_orb("Finial", (0, 2.65, 0), 0.07)
    parts.append(finial)
    mat_wood = make_character_mat("Perch_Wood", (0.35, 0.22, 0.14, 1.0), roughness=0.92)
    mat_cloth = make_character_mat("Perch_Cloth", (0.75, 0.45, 0.25, 1.0), roughness=0.95)
    mat_gold = make_aether_emissive("Perch_Finial", AETHER_GOLD, 3.0)
    for p in parts:
        if p.name == "Finial":
            p.data.materials.append(mat_gold)
        elif p.name.startswith("Banner_"):
            p.data.materials.append(mat_cloth)
        else:
            p.data.materials.append(mat_wood)
    o = join_character(parts, "Prop_AviaryPerch")
    save_and_export(o, "Prop_AviaryPerch")

if __name__ == "__main__":
    main()
