"""
Mud Golem — Moon 1's sole enemy type. Harmonic combat encounters.

Per docs/15 §1:
- 2.5m tall humanoid mud creature
- Lumpy, asymmetric (no clean silhouette like sculpted character)
- Wet mud brown body
- Gold cyst on chest (the harmonic target — destroying it = kill)
- 2 glowing crimson eye sockets (corruption)
- Silhouette: lumbering humanoid sludge
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
import random


def main():
    reset_scene()
    random.seed(42)  # deterministic
    print("[Enemy_MudGolem] 2.5m harmonic combat target")

    parts = []

    # Lumpy body — multiple overlapping orbs of varying size
    body_pts = [
        (0, 0.60, 0, 0.55),
        (0, 1.20, 0, 0.50),
        (0.10, 1.60, -0.05, 0.45),  # asymmetric shoulder
        (-0.20, 1.50, 0.05, 0.30),  # different shoulder
        (0.05, 0.80, 0.08, 0.45),  # gut lump
    ]
    for i, (x, y, z, r) in enumerate(body_pts):
        b = ico_orb(f"Body_{i}", (x, y, z), r)
        parts.append(b)

    # Head — lumpy
    head_a = ico_orb("Head_A", (0, 2.00, 0), 0.28)
    parts.append(head_a)
    head_b = ico_orb("Head_B", (0.08, 2.05, -0.05), 0.18)
    parts.append(head_b)

    # 2 crimson eye sockets
    for sx in [-0.10, 0.10]:
        e = uv_orb(f"Eye_{sx}", (sx, 2.05, 0.20), 0.035)
        parts.append(e)

    # Massive arms — chunky asymmetric
    arm_l_upper = cylinder_y("Arm_L_Upper", (-0.45, 1.40, 0), 0.18, 0.65)
    arm_l_upper.rotation_euler = (0, 0, math.radians(-15))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    parts.append(arm_l_upper)
    arm_l_hand = ico_orb("Arm_L_Hand", (-0.60, 0.85, 0), 0.25)
    parts.append(arm_l_hand)

    arm_r_upper = cylinder_y("Arm_R_Upper", (0.50, 1.35, 0), 0.20, 0.70)
    arm_r_upper.rotation_euler = (0, 0, math.radians(20))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    parts.append(arm_r_upper)
    arm_r_hand = ico_orb("Arm_R_Hand", (0.68, 0.75, 0.05), 0.28)
    parts.append(arm_r_hand)

    # Stumpy legs
    for sx in [-0.18, 0.18]:
        leg = cylinder_y(f"Leg_{sx}", (sx, 0.30, 0), 0.22, 0.60)
        parts.append(leg)

    # Gold cyst on chest (harmonic combat target)
    cyst = uv_orb("HarmonicCyst", (0, 1.20, 0.40), 0.16)
    parts.append(cyst)

    # Smaller cyst growths
    cyst2 = uv_orb("HarmonicCyst_Small", (-0.30, 1.30, 0.35), 0.08)
    parts.append(cyst2)

    # Materials
    mat_mud = make_character_mat("MudGolem_Body", (0.31, 0.22, 0.16, 1.0), roughness=0.7)  # wet mud
    mat_eyes = make_aether_emissive("MudGolem_EyesCrimson", (0.75, 0.12, 0.12, 1.0), 4.0)
    mat_cyst = make_aether_emissive("MudGolem_HarmonicCyst", AETHER_GOLD, 5.0)
    mat_cyst_small = make_aether_emissive("MudGolem_CystSmall", AETHER_GOLD, 3.0)

    for p in parts:
        if p.name == "HarmonicCyst":
            p.data.materials.append(mat_cyst)
        elif p.name == "HarmonicCyst_Small":
            p.data.materials.append(mat_cyst_small)
        elif p.name.startswith("Eye_"):
            p.data.materials.append(mat_eyes)
        else:
            p.data.materials.append(mat_mud)

    o = join_character(parts, "Enemy_MudGolem")
    save_and_export(o, "Enemy_MudGolem")


if __name__ == "__main__":
    main()
