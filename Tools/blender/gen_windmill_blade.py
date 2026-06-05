"""Standalone windmill — stone base + 4 angled blades + central axle.

~6m tall, real silhouette beyond the existing VillageMill which has
4-blade procedural cluster.

2026-06-05 — Autonomous Moon 1 content loop proof round.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, cone, sphere
import bpy

reset_scene()
stone = make_material("Wm_stone", (0.62, 0.58, 0.52), roughness=0.88)
roof = make_material("Wm_roof", (0.42, 0.20, 0.10), roughness=0.78)
blade = make_material("Wm_blade", (0.88, 0.82, 0.68), roughness=0.72)
trim = make_material("Wm_trim", (0.30, 0.18, 0.08), roughness=0.85)

# Stone tower base 2m radius, 5m tall, slight taper
cyl("tower_lower", 2.0, 2.5, (0, 0, 1.25), stone, verts=18)
cyl("tower_upper", 1.7, 2.5, (0, 0, 3.75), stone, verts=18)
# Conical roof
cone("roof", 1.9, 0.05, 1.4, (0, 0, 5.70), roof, verts=18)
# Door
cube("door", (0, -1.95, 1.10), (0.45, 0.05, 1.0), trim)
# Door frame trim
cube("frame_l", (-0.55, -1.96, 1.10), (0.06, 0.04, 1.10), trim)
cube("frame_r", ( 0.55, -1.96, 1.10), (0.06, 0.04, 1.10), trim)
cube("frame_t", (0, -1.96, 2.22), (0.55, 0.04, 0.06), trim)
# 4 window slits (cardinal)
for i in range(4):
    a = i * (math.pi / 2)
    cube(f"win_{i}",
         (math.cos(a) * 1.78, math.sin(a) * 1.78, 3.5),
         (0.08, 0.08, 0.30),
         trim,
         rot=(0, 0, a))
# Central axle pointing -Y
cyl("axle", 0.22, 0.60, (0, -2.0, 5.0), trim, rot=(1.5708, 0, 0), verts=10)
sphere("axle_cap", 0.30, (0, -2.30, 5.0), trim, segs=10, rings=6)
# 4 blades spaced 90 deg, 2.4m long, angled 15 deg
for i in range(4):
    a = i * (math.pi / 2)
    # Blade base + tip points outward radially around axle (at Y=-2.0)
    bx = math.cos(a) * 1.5
    bz = math.sin(a) * 1.5
    cube(f"blade_{i}",
         (bx, -2.30, 5.0 + bz),
         (0.35, 0.06, 1.7),
         blade,
         rot=(0, 0, a))
# Window-cross brace on each blade
for i in range(4):
    a = i * (math.pi / 2)
    bx = math.cos(a) * 1.5
    bz = math.sin(a) * 1.5
    cube(f"brace_{i}",
         (bx, -2.32, 5.0 + bz),
         (0.40, 0.02, 0.04),
         trim,
         rot=(0, 0, a + 1.5708))

export_current_as("WindmillTower", "Moon1")
print("[gen_windmill_blade] Done")
