"""
Aether Tree — canon Echohaven flora (large stylized tree with gold-glowing leaves).

3m tall, trunk + spherical leaf canopy. Subtle gold glow.
Scattered around plaza perimeter.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    # Trunk
    trunk = cylinder_y("Trunk", (0, 1.0, 0), 0.12, 2.0)
    parts.append(trunk)
    # Canopy — large ico sphere of leaves
    canopy_a = ico_orb("CanopyA", (0, 2.4, 0), 0.85)
    parts.append(canopy_a)
    canopy_b = ico_orb("CanopyB", (0.40, 2.20, 0), 0.55)
    parts.append(canopy_b)
    canopy_c = ico_orb("CanopyC", (-0.30, 2.30, 0.20), 0.60)
    parts.append(canopy_c)
    # 4 visible gold leaves (small orbs) glinting
    for i in range(4):
        ang = math.radians(i * 90)
        gx = 0.70 * math.cos(ang)
        gz = 0.70 * math.sin(ang)
        gleaf = uv_orb(f"GoldLeaf_{i}", (gx, 2.7, gz), 0.10)
        parts.append(gleaf)
    mat_trunk = make_character_mat("Tree_Trunk", (0.32, 0.22, 0.16, 1.0), roughness=0.92)
    mat_leaves = make_character_mat("Tree_Leaves", (0.28, 0.42, 0.22, 1.0), roughness=0.85)
    mat_gold = make_aether_emissive("Tree_GoldLeaf", AETHER_GOLD, 3.5)
    for p in parts:
        if p.name.startswith("GoldLeaf_"):
            p.data.materials.append(mat_gold)
        elif p.name.startswith("Canopy"):
            p.data.materials.append(mat_leaves)
        else:
            p.data.materials.append(mat_trunk)
    o = join_character(parts, "Prop_AetherTree")
    save_and_export(o, "Prop_AetherTree")

if __name__ == "__main__":
    main()
