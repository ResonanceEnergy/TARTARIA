"""
Mud Spire — corruption beat (Mud Flood evidence) per docs/15 §1.

Rising mud column with corruption crimson cracks. Placed near Mud Pools.
2.5m tall, signals Mud Flood damage to a wary player.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD,
)
import bpy


def main():
    reset_scene()
    print("[Prop_MudSpire] 2.5m corruption beat")

    parts = []

    # Tall lumpy mud column (3 stacked irregular orbs)
    layers = [(0, 0.40, 0, 0.70), (0.05, 1.30, 0, 0.55), (-0.04, 2.00, 0, 0.40)]
    for i, (x, y, z, r) in enumerate(layers):
        lump = ico_orb(f"Lump_{i}", (x, y, z), r)
        parts.append(lump)

    # Top peak
    peak = ico_orb("Peak", (0, 2.40, 0), 0.20)
    parts.append(peak)

    # Crimson corruption cracks (3 thin gold-line emissive)
    for i in range(3):
        ang = math.radians(i * 120)
        rx = 0.45 * math.cos(ang)
        rz = 0.45 * math.sin(ang)
        crack = cube_at(f"Crack_{i}", (rx, 1.20, rz), (0.05, 1.50, 0.05))
        crack.rotation_euler = (math.radians(10 * (1 if i % 2 == 0 else -1)), math.radians(i * 60), 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(crack)

    # Materials
    mat_mud = make_character_mat("MudSpire_Body", (0.28, 0.20, 0.14, 1.0), roughness=0.8)
    mat_crack = make_aether_emissive("MudSpire_Crack", (0.75, 0.20, 0.20, 1.0), 4.0)  # crimson corruption

    for p in parts:
        if p.name.startswith("Crack_"):
            p.data.materials.append(mat_crack)
        else:
            p.data.materials.append(mat_mud)

    o = join_character(parts, "Prop_MudSpire")
    save_and_export(o, "Prop_MudSpire")


if __name__ == "__main__":
    main()
