"""
Memory Tablet — readable lore stone per docs/15 + lore bible.

Angled stone tablet, 0.9m tall, with gold sigil rows on face.
Player presses E = lore fragment + quest beat.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, uv_orb, cylinder_y, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    base = cube_at("Base", (0, 0.10, 0), (0.80, 0.20, 0.40))
    parts.append(base)
    tablet = cube_at("Tablet", (0, 0.65, 0), (0.70, 0.95, 0.10))
    tablet.rotation_euler = (math.radians(-15), 0, 0)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    parts.append(tablet)
    # 4 sigil rows on tablet face
    for i in range(4):
        y = 0.30 + i * 0.20
        s = cube_at(f"Sigil_{i}", (0, y, 0.10), (0.55, 0.04, 0.02))
        parts.append(s)
    glyph_top = uv_orb("GlyphTop", (0, 1.20, 0.10), 0.08)
    parts.append(glyph_top)
    mat_stone = make_character_mat("Tablet_Stone", (0.42, 0.40, 0.38, 1.0), roughness=0.95)
    mat_glyph = make_aether_emissive("Tablet_Glyph", AETHER_GOLD, 3.5)
    for p in parts:
        if p.name.startswith("Sigil_") or p.name == "GlyphTop":
            p.data.materials.append(mat_glyph)
        else:
            p.data.materials.append(mat_stone)
    o = join_character(parts, "Prop_MemoryTablet")
    save_and_export(o, "Prop_MemoryTablet")

if __name__ == "__main__":
    main()
