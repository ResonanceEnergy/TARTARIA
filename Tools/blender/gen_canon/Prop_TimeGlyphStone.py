"""
Time Glyph Stone — 13 Moons calendar marker per docs/15 §1 + lore bible.

Squat stone block (1m tall) with 13 carved glyph-rings + central gold disc.
Player reads = unlocks calendar fragment quest beat.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD, AETHER_VIOLET,
)
import bpy


def main():
    reset_scene()
    print("[Prop_TimeGlyphStone] 1m calendar marker")

    parts = []

    # Squat base — wide flat stone
    base = cube_at("Base", (0, 0.10, 0), (1.10, 0.20, 1.10))
    parts.append(base)

    # Upper drum
    drum = cylinder_y("Drum", (0, 0.55, 0), 0.45, 0.50)
    parts.append(drum)

    # Central gold disc (the 'now' marker)
    disc = cylinder_y("CenterDisc", (0, 0.85, 0), 0.20, 0.06)
    parts.append(disc)

    # 13 small glyph-orbs around the drum (representing 13 Moons)
    for i in range(13):
        ang = math.radians(i * (360 / 13))
        gx = 0.45 * math.cos(ang)
        gz = 0.45 * math.sin(ang)
        glyph = uv_orb(f"Glyph_{i}", (gx, 0.55, gz), 0.045)
        parts.append(glyph)

    # Materials
    mat_stone = make_character_mat("TimeStone_Body", (0.45, 0.43, 0.40, 1.0), roughness=0.95)
    mat_disc = make_aether_emissive("TimeStone_Disc", AETHER_GOLD, 4.0)
    mat_glyph = make_aether_emissive("TimeStone_Glyph", AETHER_VIOLET, 3.5)

    for p in parts:
        if p.name == "CenterDisc":
            p.data.materials.append(mat_disc)
        elif p.name.startswith("Glyph_"):
            p.data.materials.append(mat_glyph)
        else:
            p.data.materials.append(mat_stone)

    o = join_character(parts, "Prop_TimeGlyphStone")
    save_and_export(o, "Prop_TimeGlyphStone")


if __name__ == "__main__":
    main()
