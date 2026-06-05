"""Abandoned wooden cart — bed + 4 wheels + side rails + handles.

~2m long, ~1.2m tall. Scattered around village + outskirts as
"the Mud-Flood" environmental storytelling.

2026-06-05 — Moon 1 DEEP hammer.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, torus
import bpy

reset_scene()
wood = make_material("Cart_wood", (0.42, 0.28, 0.16), roughness=0.92)
wood_dark = make_material("Cart_wood_dark", (0.28, 0.18, 0.10), roughness=0.95)
iron = make_material("Cart_iron", (0.25, 0.22, 0.20), roughness=0.65, metallic=0.4)

# Cart bed
cube("bed_floor", (0, 0, 0.50), (1.0, 0.5, 0.05), wood)
# 4 corner posts + side rails
for x_sign in [-1, 1]:
    for y_sign in [-1, 1]:
        cube(f"post_{x_sign}_{y_sign}",
             (0.95 * x_sign, 0.45 * y_sign, 0.65),
             (0.05, 0.05, 0.15),
             wood_dark)
# Side rails
cube("rail_l", (0, -0.45, 0.75), (1.0, 0.04, 0.06), wood)
cube("rail_r", (0,  0.45, 0.75), (1.0, 0.04, 0.06), wood)
cube("rail_back", (-0.95, 0, 0.75), (0.04, 0.45, 0.06), wood)
# Floor planks (3 longitudinal)
for i in range(3):
    cube(f"plank_{i}", (0, -0.3 + i * 0.3, 0.55), (1.0, 0.08, 0.02), wood)

# Front handle/tongue
cube("tongue", (1.4, 0, 0.42), (0.4, 0.06, 0.04), wood_dark)
cube("yoke", (1.75, 0, 0.42), (0.05, 0.30, 0.04), wood_dark)

# 4 wheels — torus + spokes
wheel_y = [-0.55, 0.55]
wheel_x = [-0.65, 0.65]
for wx in wheel_x:
    for wy in wheel_y:
        torus(f"wheel_rim_{wx}_{wy}", 0.32, 0.05, (wx, wy, 0.32), wood_dark,
              rot=(math.pi / 2, 0, 0), mseg=16, miseg=6)
        # Hub
        cyl(f"hub_{wx}_{wy}", 0.06, 0.08, (wx, wy, 0.32), iron,
            rot=(math.pi / 2, 0, 0), verts=10)
        # 4 spokes
        for s in range(4):
            ang = s * math.pi / 2
            cube(f"spk_{wx}_{wy}_{s}",
                 (wx, wy, 0.32),
                 (0.27, 0.02, 0.02),
                 wood,
                 rot=(0, ang, math.pi / 2))

# Axle bar between wheels
cyl("axle_f", 0.04, 1.1, (0.65, 0, 0.32), iron, rot=(math.pi / 2, 0, 0), verts=8)
cyl("axle_r", 0.04, 1.1, (-0.65, 0, 0.32), iron, rot=(math.pi / 2, 0, 0), verts=8)

export_current_as("CartWagon", "Moon1")
print("[gen_cart_wagon] Done")
