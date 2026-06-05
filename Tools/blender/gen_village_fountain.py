"""Village center fountain — 4m wide stone basin + 2m central column.

Smaller plaza version (the canonical 8m "Thread of Memory" is HarmonicFountain).
This goes in village plaza as decorative ambient prop.

2026-06-05 — Moon 1 DEEP hammer round.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus
import bpy

reset_scene()
stone = make_material("Fnt_stone", (0.68, 0.64, 0.58), roughness=0.85)
water = make_material("Fnt_water", (0.40, 0.62, 0.78), roughness=0.20, metallic=0.05,
                       emission=(0.30, 0.50, 0.70), emission_strength=0.35)
trim = make_material("Fnt_trim", (0.42, 0.38, 0.30), roughness=0.78)

# Outer basin ring (torus)
torus("ring_outer", 2.0, 0.30, (0, 0, 0.30), stone, mseg=24, miseg=8)
# Inner step
torus("ring_inner", 1.6, 0.18, (0, 0, 0.50), trim, mseg=20, miseg=6)
# Water surface
cyl("water_surf", 1.65, 0.06, (0, 0, 0.60), water, verts=22)
# Central column base
cyl("col_base", 0.45, 0.30, (0, 0, 0.75), stone, verts=14)
# Central column shaft
cyl("col_shaft", 0.28, 1.5, (0, 0, 1.65), stone, verts=14)
# Decorative capital
torus("capital", 0.38, 0.10, (0, 0, 2.45), trim, mseg=18, miseg=6)
# Top fountain bowl
cyl("top_bowl", 0.50, 0.18, (0, 0, 2.60), stone, verts=14)
# Water spout center
sphere("water_top", 0.18, (0, 0, 2.78), water, segs=10, rings=8)
# 4 corner sphere finials
for i in range(4):
    a = i * math.pi / 2
    sphere(f"finial_{i}", 0.15, (math.cos(a) * 2.05, math.sin(a) * 2.05, 0.65),
           stone, segs=8, rings=6)

export_current_as("VillageFountain", "Moon1")
print("[gen_village_fountain] Done")
