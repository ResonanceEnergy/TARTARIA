"""
Player Elara Voss — canon protagonist per docs/01 LORE_BIBLE + appendices/G.

- Silent female humanoid, Harmonic Human with latent giant blood
- ~1.72m tall (above average for Harmonic Human, hints at giant blood)
- Mid-poly per R171 Stylized PBR Realism (~800-1200 verts)
- Practical traveling gear: hooded cloak + tunic + leggings + boots
- Aether-Gold pendant at chest (Listeners' Hall fragment)
- Silhouette: hooded figure, neutral pose, ready for Mecanim
"""
import os, sys, math, bpy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)


def main():
    reset_scene()
    print("[Char_Player_ElaraVoss] 1.72m protagonist")
    parts = []

    # Boots
    for sx in [-0.10, 0.10]:
        boot = cube_at(f"Boot_{sx}", (sx, 0.08, 0.05), (0.16, 0.16, 0.30))
        parts.append(boot)

    # Legs
    for sx in [-0.10, 0.10]:
        leg = cylinder_y(f"Leg_{sx}", (sx, 0.50, 0), 0.08, 0.85)
        parts.append(leg)

    # Hips/pelvis
    pelvis = cube_at("Pelvis", (0, 1.00, 0), (0.32, 0.20, 0.22))
    parts.append(pelvis)

    # Torso (tunic body)
    torso = cube_at("Torso", (0, 1.32, 0), (0.40, 0.50, 0.26))
    parts.append(torso)

    # Cloak hanging back
    cloak = cube_at("Cloak", (0, 1.10, -0.18), (0.55, 0.95, 0.05))
    parts.append(cloak)

    # Shoulders (subtle widening)
    shoulders = cube_at("Shoulders", (0, 1.55, 0), (0.50, 0.10, 0.26))
    parts.append(shoulders)

    # Arms
    for sx in [-0.27, 0.27]:
        upper = cylinder_y(f"UpperArm_{sx}", (sx, 1.30, 0), 0.07, 0.40)
        parts.append(upper)
        forearm = cylinder_y(f"Forearm_{sx}", (sx, 0.90, 0.05), 0.06, 0.38)
        parts.append(forearm)
        hand = uv_orb(f"Hand_{sx}", (sx, 0.65, 0.05), 0.06)
        parts.append(hand)

    # Neck
    neck = cylinder_y("Neck", (0, 1.65, 0), 0.05, 0.10)
    parts.append(neck)

    # Head (slightly oval)
    head = uv_orb("Head", (0, 1.75, 0), 0.13)
    head.scale = (1.0, 1.1, 0.95)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(head)

    # Hood (covers back/sides of head)
    hood = uv_orb("Hood", (0, 1.77, -0.04), 0.18)
    hood.scale = (1.05, 0.85, 1.10)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(hood)

    # Hair tuft front (visible under hood)
    bangs = cube_at("Bangs", (0, 1.83, 0.10), (0.18, 0.06, 0.04))
    parts.append(bangs)

    # Eyes
    for sx in [-0.045, 0.045]:
        e = uv_orb(f"Eye_{sx}", (sx, 1.76, 0.11), 0.018)
        parts.append(e)

    # Aether-Gold chest pendant (Listeners' Hall fragment - canon item)
    pendant = uv_orb("ChestPendant", (0, 1.40, 0.13), 0.045)
    parts.append(pendant)
    # Pendant chain
    chain = cube_at("PendantChain", (0, 1.50, 0.13), (0.02, 0.20, 0.01))
    parts.append(chain)

    # Belt
    belt = cube_at("Belt", (0, 1.06, 0), (0.36, 0.05, 0.24))
    parts.append(belt)
    # Belt buckle
    buckle = cube_at("BeltBuckle", (0, 1.06, 0.13), (0.07, 0.06, 0.03))
    parts.append(buckle)

    # MATERIALS — R171 PBR matte spec
    mat_cloak = make_character_mat("Elara_Cloak", (0.28, 0.22, 0.18, 1.0), roughness=0.85)
    mat_tunic = make_character_mat("Elara_Tunic", (0.55, 0.42, 0.30, 1.0), roughness=0.80)
    mat_leather = make_character_mat("Elara_Leather", (0.32, 0.18, 0.12, 1.0), roughness=0.90)
    mat_skin = make_character_mat("Elara_Skin", (0.85, 0.72, 0.62, 1.0), roughness=0.65)
    mat_hair = make_character_mat("Elara_Hair", (0.30, 0.18, 0.12, 1.0), roughness=0.80)
    mat_eye = make_aether_emissive("Elara_Eyes", AETHER_GOLD, 2.5)  # hint of giant blood
    mat_pendant = make_aether_emissive("Elara_Pendant", AETHER_GOLD, 4.5)
    mat_metal = make_character_mat("Elara_Metal", (0.65, 0.55, 0.30, 1.0), roughness=0.40, metallic=0.7)

    for p in parts:
        if p.name == "ChestPendant":
            p.data.materials.append(mat_pendant)
        elif p.name == "PendantChain" or p.name == "BeltBuckle":
            p.data.materials.append(mat_metal)
        elif p.name.startswith("Eye_"):
            p.data.materials.append(mat_eye)
        elif p.name in ("Head", "Neck") or p.name.startswith("Hand_"):
            p.data.materials.append(mat_skin)
        elif p.name == "Hood" or p.name == "Cloak":
            p.data.materials.append(mat_cloak)
        elif p.name == "Bangs":
            p.data.materials.append(mat_hair)
        elif p.name.startswith("Boot_") or p.name == "Belt":
            p.data.materials.append(mat_leather)
        elif p.name == "Torso" or p.name == "Pelvis" or p.name == "Shoulders":
            p.data.materials.append(mat_tunic)
        elif p.name.startswith("Leg_") or p.name.startswith("UpperArm_") or p.name.startswith("Forearm_"):
            p.data.materials.append(mat_leather)
        else:
            p.data.materials.append(mat_tunic)

    o = join_character(parts, "Player_ElaraVoss")
    save_and_export(o, "Player_ElaraVoss")


if __name__ == "__main__":
    main()
