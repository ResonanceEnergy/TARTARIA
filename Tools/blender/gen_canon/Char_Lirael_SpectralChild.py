"""
Lirael — Spectral Child. Day 25 lullaby beat.

Per docs/15:
- Small child silhouette (1.2m tall)
- Ghostly cyan, semi-transparent feel (emissive on body)
- Dress flowing down
- Two pigtails / side hair
- Silhouette: petite haunting figure
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, ico_orb, uv_orb, cylinder_y, cube_at, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_CYAN, AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Lirael_SpectralChild] 1.2m spectral child")

    parts = []

    # Dress — short conical
    dress = cylinder_y("Dress", (0, 0.3, 0), 0.30, 0.55)
    parts.append(dress)

    # Torso narrower
    torso = cylinder_y("Torso", (0, 0.75, 0), 0.18, 0.30)
    parts.append(torso)

    # Head — child proportions (bigger relative to body)
    head = uv_orb("Head", (0, 1.05, 0), 0.16)
    parts.append(head)

    # Two pigtails - on sides
    for sx in [-0.16, 0.16]:
        p1 = uv_orb(f"Pigtail_{sx}_top", (sx, 1.10, -0.05), 0.08)
        p2 = uv_orb(f"Pigtail_{sx}_mid", (sx + (0.05 * (1 if sx > 0 else -1)), 0.95, -0.05), 0.07)
        p3 = uv_orb(f"Pigtail_{sx}_btm", (sx + (0.08 * (1 if sx > 0 else -1)), 0.80, -0.05), 0.06)
        parts.extend([p1, p2, p3])

    # Arms — short
    for sx in [-0.22, 0.22]:
        arm = cylinder_y(f"Arm_{sx}", (sx, 0.65, 0), 0.06, 0.45)
        parts.append(arm)
        hand = uv_orb(f"Hand_{sx}", (sx, 0.42, 0), 0.05)
        parts.append(hand)

    # Eyes — small cyan glowing
    for sx in [-0.06, 0.06]:
        e = uv_orb(f"Eye_{sx}", (sx, 1.08, 0.13), 0.022)
        parts.append(e)

    # Materials — body slight cyan emissive (semi-spectral)
    mat_body = make_aether_emissive("Lirael_Body", AETHER_CYAN, 1.0)
    mat_eyes = make_aether_emissive("Lirael_Eyes", AETHER_CYAN, 5.0)
    mat_skin = make_character_mat("Lirael_Skin", (0.85, 0.9, 1.0, 1.0), roughness=0.7)

    for p in parts:
        if p.name.startswith("Eye_"):
            p.data.materials.append(mat_eyes)
        elif p.name in ("Head",) or p.name.startswith("Hand_"):
            p.data.materials.append(mat_skin)
        else:
            p.data.materials.append(mat_body)

    o = join_character(parts, "Lirael_SpectralChild")
    save_and_export(o, "Lirael_SpectralChild")


if __name__ == "__main__":
    main()
