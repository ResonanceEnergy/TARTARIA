"""
Tutorial Glyph — Day 1 spawn point marker per docs/15 §1.

Flat circular glyph (Aether-Gold pattern) on the ground at player spawn.
Triggers Milo tutorial chain when player walks over.
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
    # Flat stone disc (slight raise)
    disc = cylinder_y("Disc", (0, 0.05, 0), 1.50, 0.10)
    parts.append(disc)
    # Inner gold ring
    bpy.ops.mesh.primitive_torus_add(major_radius=1.20, minor_radius=0.04, major_segments=36, minor_segments=6, location=(0, 0.12, 0))
    outer_ring = bpy.context.active_object
    outer_ring.name = "OuterRing"
    parts.append(outer_ring)
    bpy.ops.mesh.primitive_torus_add(major_radius=0.80, minor_radius=0.04, major_segments=36, minor_segments=6, location=(0, 0.12, 0))
    inner_ring = bpy.context.active_object
    inner_ring.name = "InnerRing"
    parts.append(inner_ring)
    # 6 radial spokes
    for i in range(6):
        ang = math.radians(i * 60)
        cx = 1.0 * math.cos(ang)
        cz = 1.0 * math.sin(ang)
        spoke = cube_at(f"Spoke_{i}", (cx, 0.12, cz), (0.04, 0.03, 0.45))
        spoke.rotation_euler = (0, -ang, 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(spoke)
    # Central orb
    orb = uv_orb("CenterOrb", (0, 0.18, 0), 0.12)
    parts.append(orb)
    mat_stone = make_character_mat("Glyph_Stone", (0.42, 0.40, 0.38, 1.0), roughness=0.92)
    mat_glyph = make_aether_emissive("Glyph_Gold", AETHER_GOLD, 4.5)
    for p in parts:
        if p.name == "Disc":
            p.data.materials.append(mat_stone)
        else:
            p.data.materials.append(mat_glyph)
    o = join_character(parts, "Prop_TutorialGlyph")
    save_and_export(o, "Prop_TutorialGlyph")

if __name__ == "__main__":
    main()
