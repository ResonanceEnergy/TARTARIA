"""Standing lantern on post — 2.5m tall, glowing emissive lens.

Place along village paths for ambient lighting + golden-hour readability.

2026-06-05 — Moon 1 DEEP hammer.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, cone, sphere
import bpy

reset_scene()
iron = make_material("Lnt_iron", (0.20, 0.18, 0.16), roughness=0.65, metallic=0.4)
glass = make_material("Lnt_glass", (0.95, 0.85, 0.55), roughness=0.10,
                       emission=(1.0, 0.85, 0.50), emission_strength=2.5)
post = make_material("Lnt_post", (0.30, 0.24, 0.18), roughness=0.85)

# Base block
cube("base", (0, 0, 0.10), (0.20, 0.20, 0.10), iron)
# Post shaft
cyl("post", 0.05, 2.0, (0, 0, 1.10), post, verts=10)
# Lantern housing — 4 corner iron struts + glass body
housing_h = 0.35
top_z = 2.30

# 4 corner struts (rotation)
for i in range(4):
    import math
    a = i * math.pi / 2
    cube(f"strut_{i}",
         (0.07 * math.cos(a), 0.07 * math.sin(a), top_z),
         (0.02, 0.02, housing_h),
         iron,
         rot=(0, 0, a))

# Glass body (slightly smaller)
cube("glass", (0, 0, top_z), (0.07, 0.07, housing_h - 0.02), glass)

# Pyramidal cap
cone("cap", 0.12, 0.02, 0.15, (0, 0, top_z + housing_h + 0.05), iron, verts=4)
sphere("finial", 0.04, (0, 0, top_z + housing_h + 0.18), iron, segs=6, rings=5)

# Roof bottom rim
cube("rim", (0, 0, top_z + housing_h - 0.02), (0.12, 0.12, 0.02), iron)

export_current_as("LanternPost", "Moon1")
print("[gen_lantern_post] Done")
