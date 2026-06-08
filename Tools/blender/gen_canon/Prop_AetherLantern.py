"""
Aether Lantern — golden-glow path marker per docs/15.

Wood post + ironwork frame + central gold flame orb. Lights pathways
at golden-hour/night. 1.5m tall, used along plaza edges + paths.
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
    print("[Prop_AetherLantern] 1.5m path marker lantern")

    parts = []

    # Wooden post
    post = cylinder_y("Post", (0, 0.55, 0), 0.04, 1.10)
    parts.append(post)

    # Top crossbar
    cross = cube_at("CrossBar", (0, 1.18, 0), (0.30, 0.04, 0.04))
    parts.append(cross)

    # Hanging cage (4 thin wire columns + top/bottom rings)
    for sx in [-0.10, 0.10]:
        for sz in [-0.10, 0.10]:
            w = cylinder_y(f"Wire_{sx}_{sz}", (sx, 0.95, sz), 0.012, 0.30)
            parts.append(w)

    # Cage top + bottom plates
    top_plate = cube_at("CageTop", (0, 1.12, 0), (0.26, 0.03, 0.26))
    parts.append(top_plate)
    bot_plate = cube_at("CageBottom", (0, 0.80, 0), (0.26, 0.03, 0.26))
    parts.append(bot_plate)

    # Flame orb — gold emissive
    flame = uv_orb("Flame", (0, 0.95, 0), 0.12)
    parts.append(flame)

    # Materials
    mat_wood = make_character_mat("Lantern_Wood", (0.35, 0.22, 0.14, 1.0), roughness=0.9)
    mat_iron = make_character_mat("Lantern_Iron", (0.18, 0.15, 0.12, 1.0), roughness=0.6, metallic=0.5)
    mat_flame = make_aether_emissive("Lantern_Flame", AETHER_GOLD, 5.0)

    for p in parts:
        if p.name == "Flame":
            p.data.materials.append(mat_flame)
        elif p.name == "Post":
            p.data.materials.append(mat_wood)
        else:
            p.data.materials.append(mat_iron)

    o = join_character(parts, "Prop_AetherLantern")
    save_and_export(o, "Prop_AetherLantern")


if __name__ == "__main__":
    main()
