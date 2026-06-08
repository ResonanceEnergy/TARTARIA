"""
Giant Tartarian Skeleton — canon lore environment piece per docs/15 + lore bible.

Lore: Tartarian giants were the original Listeners' Hall builders.
Their fallen remains decorate Echohaven as buried lore beats.
Skeleton: ~6m long laid out on ground, weathered pale bone, partial skull + ribcage.
Player can find "Giant Skeleton Key #1" near this — Day 1-5 collectible.
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
    print("[Prop_GiantSkeleton] 6m fallen giant lore piece")

    parts = []

    # SKULL — large rounded cube
    skull = uv_orb("Skull", (0, 0.5, 2.5), 0.6)
    skull.scale = (1.0, 0.9, 1.2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(skull)

    # Jaw beneath skull
    jaw = cube_at("Jaw", (0, 0.25, 2.5), (0.55, 0.15, 0.7))
    parts.append(jaw)

    # SPINE — chain of vertebrae cubes
    for i in range(8):
        z = 1.8 - i * 0.4
        vert = cube_at(f"Vert_{i}", (0, 0.35, z), (0.20, 0.20, 0.18))
        parts.append(vert)

    # RIBCAGE — 4 ribs each side curving up from spine
    for side, sx_sign in enumerate([1, -1]):
        for i in range(4):
            z = 1.5 - i * 0.30
            # rib curving outward + up
            rib = cylinder_y(f"Rib_{side}_{i}", (sx_sign * 0.5, 0.45 - i * 0.05, z), 0.05, 0.55)
            rib.rotation_euler = (0, 0, math.radians(sx_sign * 35))
            bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
            parts.append(rib)

    # PELVIS — wide flat slab
    pelvis = cube_at("Pelvis", (0, 0.35, -1.8), (0.85, 0.20, 0.45))
    parts.append(pelvis)

    # LEG BONES — only 1 visible (other buried)
    femur = cylinder_y("Femur", (0.20, 0.30, -2.5), 0.13, 1.20)
    femur.rotation_euler = (0, 0, math.radians(-25))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    parts.append(femur)

    # SKELETON KEY collectible glow — gold orb beside skull
    key = uv_orb("GiantSkeletonKey_Marker", (0.8, 0.25, 2.3), 0.12)
    parts.append(key)

    # Materials
    mat_bone = make_character_mat("Giant_Bone", (0.85, 0.78, 0.65, 1.0), roughness=0.95)
    mat_key = make_aether_emissive("Giant_KeyGlow", AETHER_GOLD, 4.5)

    for p in parts:
        if p.name == "GiantSkeletonKey_Marker":
            p.data.materials.append(mat_key)
        else:
            p.data.materials.append(mat_bone)

    o = join_character(parts, "Prop_GiantSkeleton")
    save_and_export(o, "Prop_GiantSkeleton")


if __name__ == "__main__":
    main()
