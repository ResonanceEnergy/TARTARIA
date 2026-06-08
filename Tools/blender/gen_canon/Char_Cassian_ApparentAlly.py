"""
Cassian — Apparent Ally. Cabal infiltrator (revealed later).

Per docs/15 + lore:
- Tall masculine humanoid (1.85m)
- Dark slate-blue cloak + hood
- Subtle gold seam at belt (false Aether attunement)
- Hidden corruption crimson eye when scanned
- Silhouette: hooded traveler
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, ico_orb, uv_orb, cylinder_y, cube_at, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Cassian_ApparentAlly] 1.85m hooded traveler")

    parts = []

    # Cloak base — wide cylinder bottom
    cloak = cylinder_y("Cloak", (0, 0.55, 0), 0.42, 1.10)
    parts.append(cloak)

    # Torso (under cloak)
    torso = cylinder_y("Torso", (0, 1.30, 0), 0.30, 0.40)
    parts.append(torso)

    # Shoulders (slightly wider)
    shoulders = cube_at("Shoulders", (0, 1.45, 0), (0.65, 0.18, 0.30))
    parts.append(shoulders)

    # Head
    head = uv_orb("Head", (0, 1.65, 0), 0.18)
    parts.append(head)

    # Hood — semi-sphere covering head from above
    hood = uv_orb("Hood", (0, 1.70, -0.05), 0.25)
    hood.scale = (1.0, 0.7, 1.1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(hood)

    # Arms
    for sx in [-0.36, 0.36]:
        arm = cylinder_y(f"Arm_{sx}", (sx, 1.15, 0), 0.09, 0.65)
        parts.append(arm)
        hand = uv_orb(f"Hand_{sx}", (sx, 0.80, 0), 0.07)
        parts.append(hand)

    # Belt with gold buckle
    belt = cube_at("Belt", (0, 1.06, 0), (0.65, 0.07, 0.46))
    parts.append(belt)
    buckle = cube_at("BeltBuckle", (0, 1.06, 0.25), (0.12, 0.10, 0.05))
    parts.append(buckle)

    # Eyes — slightly crimson tint (canon ally reveal)
    for sx in [-0.07, 0.07]:
        e = uv_orb(f"Eye_{sx}", (sx, 1.67, 0.13), 0.025)
        parts.append(e)

    # Materials
    mat_cloak = make_character_mat("Cassian_Cloak", (0.20, 0.27, 0.34, 1.0), roughness=0.9)
    mat_belt = make_character_mat("Cassian_Belt", (0.15, 0.10, 0.10, 1.0), roughness=0.9)
    mat_buckle = make_aether_emissive("Cassian_Buckle", AETHER_GOLD, 3.0)
    mat_skin = make_character_mat("Cassian_Skin", (0.78, 0.62, 0.52, 1.0), roughness=0.7)
    mat_eyes = make_aether_emissive("Cassian_EyesCrimson", (0.75, 0.18, 0.18, 1.0), 4.0)

    for p in parts:
        if p.name == "BeltBuckle":
            p.data.materials.append(mat_buckle)
        elif p.name == "Belt":
            p.data.materials.append(mat_belt)
        elif p.name.startswith("Eye_"):
            p.data.materials.append(mat_eyes)
        elif p.name in ("Head",) or p.name.startswith("Hand_"):
            p.data.materials.append(mat_skin)
        else:
            p.data.materials.append(mat_cloak)

    o = join_character(parts, "Cassian_ApparentAlly")
    save_and_export(o, "Cassian_ApparentAlly")


if __name__ == "__main__":
    main()
