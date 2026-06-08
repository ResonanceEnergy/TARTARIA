"""
Reset Scout — Cabal patrol drone per docs/15 + 03 campaign.

1.6m tall hovering humanoid: dark torso + cylindrical head + crimson eye visor.
Floats above ground (Y offset 0.5m) signaled by lifted geometry.
Distinct silhouette from Mud Golem — clean machine angles.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD, AETHER_VIOLET,
)
import bpy

def main():
    reset_scene()
    parts = []
    # Hover base (floating wedge instead of legs)
    base = cube_at("HoverBase", (0, 0.5, 0), (0.45, 0.15, 0.45))
    parts.append(base)
    # Torso (octagonal)
    torso = cylinder_y("Torso", (0, 1.0, 0), 0.28, 0.65)
    parts.append(torso)
    # Shoulders
    sh = cube_at("Shoulders", (0, 1.30, 0), (0.65, 0.12, 0.18))
    parts.append(sh)
    # Arms (mechanical thin)
    for sx in [-0.40, 0.40]:
        arm = cylinder_y(f"Arm_{sx}", (sx, 1.0, 0), 0.06, 0.55)
        parts.append(arm)
        forearm = cube_at(f"Forearm_{sx}", (sx, 0.65, 0), (0.08, 0.30, 0.08))
        parts.append(forearm)
    # Head — cylinder with visor
    head = cylinder_y("Head", (0, 1.55, 0), 0.20, 0.30)
    parts.append(head)
    # Visor band (crimson)
    visor = cube_at("Visor", (0, 1.55, 0.18), (0.35, 0.08, 0.04))
    parts.append(visor)
    # Antenna nub
    antenna = cylinder_y("Antenna", (0, 1.80, 0), 0.02, 0.18)
    parts.append(antenna)
    # Materials
    mat_body = make_character_mat("Scout_Body", (0.18, 0.20, 0.24, 1.0), roughness=0.4, metallic=0.6)
    mat_accent = make_character_mat("Scout_Accent", (0.10, 0.10, 0.12, 1.0), roughness=0.3, metallic=0.7)
    mat_visor = make_aether_emissive("Scout_Visor", (0.85, 0.20, 0.20, 1.0), 5.0)  # crimson
    mat_antenna = make_aether_emissive("Scout_Antenna", AETHER_VIOLET, 3.0)
    for p in parts:
        if p.name == "Visor":
            p.data.materials.append(mat_visor)
        elif p.name == "Antenna":
            p.data.materials.append(mat_antenna)
        elif p.name == "HoverBase" or p.name.startswith("Forearm_"):
            p.data.materials.append(mat_accent)
        else:
            p.data.materials.append(mat_body)
    o = join_character(parts, "Enemy_ResetScout")
    save_and_export(o, "Enemy_ResetScout")

if __name__ == "__main__":
    main()
