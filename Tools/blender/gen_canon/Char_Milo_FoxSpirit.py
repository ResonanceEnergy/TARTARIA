"""
Milo — Fox Spirit guide. Day 1 tutorial companion.

Per docs/15 + appendices/G_NPC_INDEX.md:
- Small playful fox spirit (about 0.7m at shoulder, ~1m total length)
- Aether-Gold body with cyan tail-tip glow (telluric anchor)
- 4-legged base + raised torso, big ears, bushy tail
- Silhouette reads as fox at 50m
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, ico_orb, uv_orb, cylinder_y, cube_at, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export, set_pivot_bottom_center, shade_smooth,
    AETHER_GOLD, AETHER_CYAN,
)
import bpy


def main():
    reset_scene()
    print("[Milo_FoxSpirit] small fox guide")

    parts = []

    # Body — elongated ico-sphere
    body = ico_orb("Body", (0, 0.45, 0), 0.3)
    body.scale = (1.0, 0.7, 1.8)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(body)

    # Head — smaller orb forward
    head = uv_orb("Head", (0, 0.55, 0.45), 0.18)
    parts.append(head)

    # Snout cone
    snout = uv_orb("Snout", (0, 0.50, 0.60), 0.12)
    snout.scale = (0.7, 0.7, 1.3)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(snout)

    # Two ears — small triangular
    for sx in [-0.10, 0.10]:
        ear = cube_at(f"Ear_{sx}", (sx, 0.75, 0.40), (0.05, 0.18, 0.08))
        ear.rotation_euler = (math.radians(-15), 0, 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(ear)

    # 4 legs — small cylinders
    for x_off in [-0.15, 0.15]:
        for z_off in [-0.30, 0.30]:
            leg = cylinder_y(f"Leg_{x_off}_{z_off}", (x_off, 0.15, z_off), 0.05, 0.3)
            parts.append(leg)

    # Bushy tail — series of orbs trailing back, getting smaller
    tail_pts = [(0, 0.40, -0.40), (0, 0.45, -0.55), (0, 0.50, -0.70), (0, 0.55, -0.85)]
    tail_orbs = []
    for i, p in enumerate(tail_pts):
        t = uv_orb(f"TailSeg_{i}", p, 0.15 - i * 0.025)
        tail_orbs.append(t)
        parts.append(t)
    # Tail tip glows cyan
    tip = uv_orb("TailTip_Glow", (0, 0.58, -0.95), 0.08)
    parts.append(tip)

    # Eyes — two small gold orbs
    for sx in [-0.07, 0.07]:
        eye = uv_orb(f"Eye_{sx}", (sx, 0.57, 0.55), 0.025)
        parts.append(eye)

    # Materials
    mat_body = make_character_mat("Milo_Body", (1.0, 0.55, 0.2, 1.0), roughness=0.8)
    mat_tail_tip = make_aether_emissive("Milo_TailGlow", AETHER_CYAN, 4.0)
    mat_eyes = make_aether_emissive("Milo_Eyes", AETHER_GOLD, 3.0)

    for p in parts:
        if p.name == "TailTip_Glow":
            p.data.materials.append(mat_tail_tip)
        elif p.name.startswith("Eye_"):
            p.data.materials.append(mat_eyes)
        else:
            p.data.materials.append(mat_body)

    o = join_character(parts, "Milo_FoxSpirit")
    save_and_export(o, "Milo_FoxSpirit")


if __name__ == "__main__":
    main()
