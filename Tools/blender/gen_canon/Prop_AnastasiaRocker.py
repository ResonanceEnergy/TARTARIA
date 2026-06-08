"""
Anastasia's Rocking Chair — canon Day 6+ interactable.

Per docs/15: gold seam on rockers + arm rests, warm walnut wood body.
Silhouette: classic high-back rocking chair, ~1.5m tall.
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
    print("[Prop_AnastasiaRocker] canon rocking chair")

    parts = []

    # 4 legs (vertical wooden posts)
    leg_positions = [(-0.30, 0.40, -0.25), (0.30, 0.40, -0.25), (-0.30, 0.40, 0.25), (0.30, 0.40, 0.25)]
    for i, p in enumerate(leg_positions):
        leg = cylinder_y(f"Leg_{i}", p, 0.04, 0.80)
        parts.append(leg)

    # Seat — horizontal slab
    seat = cube_at("Seat", (0, 0.80, 0), (0.70, 0.06, 0.55))
    parts.append(seat)

    # Back posts
    for sx in [-0.30, 0.30]:
        back = cylinder_y(f"BackPost_{sx}", (sx, 1.25, -0.25), 0.04, 0.90)
        parts.append(back)

    # Backrest slats - 5 horizontal
    for i in range(5):
        y = 0.95 + i * 0.16
        slat = cube_at(f"BackSlat_{i}", (0, y, -0.25), (0.60, 0.04, 0.04))
        parts.append(slat)

    # Curved rockers - 2 long arched cubes at the bottom
    for sx in [-0.30, 0.30]:
        rocker = cube_at(f"Rocker_{sx}", (sx, 0.05, 0), (0.05, 0.05, 0.90))
        rocker.rotation_euler = (math.radians(8), 0, 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        parts.append(rocker)

    # Arm rests
    for sx in [-0.30, 0.30]:
        arm = cube_at(f"Arm_{sx}", (sx, 1.05, -0.05), (0.05, 0.05, 0.40))
        parts.append(arm)

    # Headrest — top crown
    crown = cube_at("Crown", (0, 1.75, -0.25), (0.65, 0.10, 0.08))
    parts.append(crown)

    # Materials
    mat_wood = make_character_mat("Anastasia_Rocker_Wood", (0.42, 0.28, 0.18, 1.0), roughness=0.85)
    mat_gold_seam = make_aether_emissive("Anastasia_Rocker_GoldSeam", AETHER_GOLD, 2.5)

    for p in parts:
        if p.name == "Crown" or p.name.startswith("Rocker_"):
            p.data.materials.append(mat_gold_seam)
        else:
            p.data.materials.append(mat_wood)

    o = join_character(parts, "Prop_AnastasiaRocker")
    save_and_export(o, "Prop_AnastasiaRocker")


if __name__ == "__main__":
    main()
