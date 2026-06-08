"""
Resonance Bell — Aether-Bell shrine per docs/15.

Wooden frame + suspended brass bell. Player rings = activates harmonic zone.
1.8m tall total.
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
    # 2 wooden posts
    for sx in [-0.40, 0.40]:
        post = cylinder_y(f"Post_{sx}", (sx, 0.75, 0), 0.05, 1.50)
        parts.append(post)
    # Top crossbar
    cross = cube_at("CrossBar", (0, 1.50, 0), (0.92, 0.08, 0.10))
    parts.append(cross)
    # Bell rope
    rope = cylinder_y("Rope", (0, 1.25, 0), 0.015, 0.20)
    parts.append(rope)
    # Bell — inverted cone
    bell_top = uv_orb("BellTop", (0, 1.05, 0), 0.14)
    bell_top.scale = (1.0, 0.5, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(bell_top)
    bell_body = cylinder_y("BellBody", (0, 0.90, 0), 0.20, 0.30)
    parts.append(bell_body)
    bell_lip = cylinder_y("BellLip", (0, 0.72, 0), 0.22, 0.06)
    parts.append(bell_lip)
    # Clapper inside
    clapper = uv_orb("Clapper", (0, 0.83, 0), 0.05)
    parts.append(clapper)
    # Materials
    mat_wood = make_character_mat("Bell_Wood", (0.35, 0.22, 0.14, 1.0), roughness=0.9)
    mat_brass = make_character_mat("Bell_Brass", (0.75, 0.55, 0.20, 1.0), roughness=0.3, metallic=0.8)
    mat_rope = make_character_mat("Bell_Rope", (0.45, 0.32, 0.20, 1.0), roughness=0.95)
    mat_clapper = make_aether_emissive("Bell_Clapper", AETHER_GOLD, 4.0)
    for p in parts:
        if p.name == "Clapper":
            p.data.materials.append(mat_clapper)
        elif p.name == "Rope":
            p.data.materials.append(mat_rope)
        elif p.name.startswith("Bell"):
            p.data.materials.append(mat_brass)
        else:
            p.data.materials.append(mat_wood)
    o = join_character(parts, "Prop_ResonanceBell")
    save_and_export(o, "Prop_ResonanceBell")

if __name__ == "__main__":
    main()
