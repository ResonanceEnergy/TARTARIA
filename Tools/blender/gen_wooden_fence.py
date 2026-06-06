"""Wooden fence segment — 3m long, 1m tall, 2 horizontal rails + 5 vertical pickets.

Place around garden / paddock borders. Tile-able.

2026-06-05 — Moon 1 DEEP hammer.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube
import bpy

reset_scene()
wood = make_material("Fnc_wood", (0.50, 0.36, 0.22), roughness=0.92)
wood_dark = make_material("Fnc_wood_dark", (0.32, 0.22, 0.14), roughness=0.95)

# Top + bottom rails
cube("rail_top", (0, 0, 0.85), (1.5, 0.06, 0.06), wood_dark)
cube("rail_bot", (0, 0, 0.35), (1.5, 0.06, 0.06), wood_dark)

# 5 evenly spaced vertical pickets, pointed tops
for i, x in enumerate([-1.3, -0.65, 0.0, 0.65, 1.3]):
    cube(f"picket_{i}", (x, 0, 0.55), (0.04, 0.04, 0.50), wood)
    # Pointed cap (small cube tilted)
    cube(f"cap_{i}", (x, 0, 1.05), (0.05, 0.05, 0.05), wood)

# End posts (thicker)
cube("post_l", (-1.45, 0, 0.55), (0.07, 0.07, 0.60), wood_dark)
cube("post_r", ( 1.45, 0, 0.55), (0.07, 0.07, 0.60), wood_dark)

export_current_as("WoodenFence", "Moon1")
print("[gen_wooden_fence] Done")
