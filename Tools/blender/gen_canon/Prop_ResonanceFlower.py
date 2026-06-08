"""
Resonance Flower — canon Aether-bloom vegetation per docs/15 §7.

Glowing cyan flower atop a tall slim stalk. Scattered through Echohaven
as ambient flora that hums when player walks near. ~0.8m tall.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_CYAN, AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Prop_ResonanceFlower] 0.8m glowing flower")

    parts = []

    # Stalk - thin tall cylinder
    stalk = cylinder_y("Stalk", (0, 0.30, 0), 0.025, 0.60)
    parts.append(stalk)

    # 2 leaves at mid-stalk
    for sx in [-0.05, 0.05]:
        leaf = cube_at(f"Leaf_{sx}", (sx, 0.28, 0), (0.10, 0.02, 0.06))
        leaf.rotation_euler = (0, math.radians(45 if sx > 0 else -45), math.radians(20 if sx > 0 else -20))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(leaf)

    # Bulb base
    bulb = uv_orb("Bulb", (0, 0.65, 0), 0.10)
    parts.append(bulb)

    # 6 petals — radial spheres
    for i in range(6):
        ang = math.radians(i * 60)
        px = 0.13 * math.cos(ang)
        pz = 0.13 * math.sin(ang)
        petal = uv_orb(f"Petal_{i}", (px, 0.75, pz), 0.08)
        parts.append(petal)

    # Center pollen orb (gold glow)
    pollen = uv_orb("Pollen", (0, 0.78, 0), 0.05)
    parts.append(pollen)

    # Materials
    mat_stem = make_character_mat("Flower_Stem", (0.20, 0.45, 0.20, 1.0), roughness=0.7)
    mat_petal = make_aether_emissive("Flower_Petal", AETHER_CYAN, 3.0)
    mat_pollen = make_aether_emissive("Flower_Pollen", AETHER_GOLD, 5.0)

    for p in parts:
        if p.name == "Pollen":
            p.data.materials.append(mat_pollen)
        elif p.name.startswith("Petal_") or p.name == "Bulb":
            p.data.materials.append(mat_petal)
        else:
            p.data.materials.append(mat_stem)

    o = join_character(parts, "Prop_ResonanceFlower")
    save_and_export(o, "Prop_ResonanceFlower")


if __name__ == "__main__":
    main()
