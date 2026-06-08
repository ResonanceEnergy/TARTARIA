"""
Carved Wall Ruin — Tartarian wall fragment per docs/15 environment beats.

Broken stone wall ~3m wide × 2m tall, gold-emissive carved sigils on face.
Scattered as ambient ruin around plaza edges.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Prop_CarvedWall] 3m wide ruin fragment")

    parts = []

    # Main wall slab (broken irregularly)
    wall = cube_at("WallMain", (0, 1.0, 0), (3.0, 2.0, 0.4))
    parts.append(wall)

    # Broken top notch
    notch_a = cube_at("NotchA", (1.2, 2.0, 0), (0.45, 0.40, 0.42))
    notch_b = cube_at("NotchB", (-0.5, 2.0, 0), (0.40, 0.35, 0.42))
    parts.append(notch_a)
    parts.append(notch_b)

    # Carved sigil rings — 3 horizontal gold lines
    for i, y in enumerate([0.6, 1.1, 1.6]):
        sigil = cube_at(f"Sigil_{i}", (0, y, 0.21), (2.5, 0.05, 0.02))
        parts.append(sigil)

    # 3 sigil orbs centered
    for i, y in enumerate([0.6, 1.1, 1.6]):
        orb = uv_orb(f"SigilOrb_{i}", (0, y, 0.25), 0.06)
        parts.append(orb)

    # Materials
    mat_stone = make_character_mat("CarvedWall_Stone", (0.42, 0.38, 0.34, 1.0), roughness=0.95)
    mat_sigil = make_aether_emissive("CarvedWall_Sigil", AETHER_GOLD, 3.0)

    for p in parts:
        if p.name.startswith("Sigil") or p.name.startswith("SigilOrb"):
            p.data.materials.append(mat_sigil)
        else:
            p.data.materials.append(mat_stone)

    o = join_character(parts, "Prop_CarvedWall")
    save_and_export(o, "Prop_CarvedWall")


if __name__ == "__main__":
    main()
