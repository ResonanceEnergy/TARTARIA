"""
Anastasia — Archive Echo. Post-Dome-reveal NPC.

Per docs/15 + lore:
- Feminine humanoid, ethereal violet
- 1.7m tall, robed silhouette (skirts down to feet, hidden legs)
- Gold seam on chest (Archive heart)
- Hair flowing past shoulders
- Silhouette: ghostly princess
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, ico_orb, uv_orb, cylinder_y, cube_at, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD, AETHER_VIOLET,
)
import bpy


def main():
    reset_scene()
    print("[Anastasia_ArchiveEcho] 1.7m ethereal NPC")

    parts = []

    # Skirt/robe base — wide cone bottom (cylinder narrowing)
    skirt = cylinder_y("Skirt", (0, 0.4, 0), 0.45, 0.8)
    skirt.scale = (1.0, 1.0, 1.0)
    parts.append(skirt)

    # Torso — vertical narrower cylinder above skirt
    torso = cylinder_y("Torso", (0, 1.05, 0), 0.25, 0.5)
    parts.append(torso)

    # Head — sphere
    head = uv_orb("Head", (0, 1.45, 0), 0.18)
    parts.append(head)

    # Hair — back orbs sweeping down
    for i, y in enumerate([1.35, 1.25, 1.15, 1.05]):
        h = uv_orb(f"Hair_{i}", (0, y, -0.18), 0.13 - i * 0.01)
        parts.append(h)

    # Arms — two angled cylinders down from shoulders
    for sx in [-0.30, 0.30]:
        arm = cylinder_y(f"Arm_{sx}", (sx, 0.85, 0), 0.08, 0.7)
        arm.rotation_euler = (0, 0, math.radians(8 * (1 if sx > 0 else -1)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(arm)
        # Hand
        hand = uv_orb(f"Hand_{sx}", (sx + 0.05 * (1 if sx > 0 else -1), 0.5, 0), 0.07)
        parts.append(hand)

    # Aether seam — gold orb at chest
    chest_orb = uv_orb("ChestGlow", (0, 1.05, 0.20), 0.10)
    parts.append(chest_orb)

    # Crown — small disc
    crown = cylinder_y("Crown", (0, 1.62, 0), 0.18, 0.06)
    parts.append(crown)

    # Materials
    mat_body = make_character_mat("Anastasia_Robe", AETHER_VIOLET, roughness=0.8)
    mat_skin = make_character_mat("Anastasia_Skin", (0.92, 0.82, 0.78, 1.0), roughness=0.6)
    mat_glow = make_aether_emissive("Anastasia_ChestGlow", AETHER_GOLD, 4.5)
    mat_crown = make_aether_emissive("Anastasia_Crown", AETHER_GOLD, 3.0)

    for p in parts:
        if p.name == "ChestGlow":
            p.data.materials.append(mat_glow)
        elif p.name == "Crown":
            p.data.materials.append(mat_crown)
        elif p.name in ("Head",) or p.name.startswith("Hand_"):
            p.data.materials.append(mat_skin)
        elif p.name.startswith("Hair_"):
            mat_hair = make_character_mat("Anastasia_Hair", (0.65, 0.40, 0.65, 1.0))
            p.data.materials.append(mat_hair)
        else:
            p.data.materials.append(mat_body)

    o = join_character(parts, "Anastasia_ArchiveEcho")
    save_and_export(o, "Anastasia_ArchiveEcho")


if __name__ == "__main__":
    main()
