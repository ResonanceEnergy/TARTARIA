"""Village bell — bronze bell on wooden A-frame stand.

~2m tall A-frame, 0.6m diameter bell. Centerpiece prop for plaza /
'restoration day' bell-ring beat.

2026-06-05 — Moon 1 DEEP hammer.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, cone, sphere
import bpy

reset_scene()
wood = make_material("Bel_wood", (0.42, 0.28, 0.18), roughness=0.90)
bronze = make_material("Bel_bronze", (0.72, 0.50, 0.20), roughness=0.50, metallic=0.65)
rope = make_material("Bel_rope", (0.62, 0.48, 0.30), roughness=0.95)

# Base plates
cube("base_l", (-1.0, 0, 0.05), (0.3, 0.3, 0.05), wood)
cube("base_r", ( 1.0, 0, 0.05), (0.3, 0.3, 0.05), wood)

# A-frame legs (angled — front-back pair on each side)
# Left A
cube("leg_l_f", (-0.92, 0.20, 1.0), (0.06, 0.06, 1.0), wood, rot=(0, 0.08, 0))
cube("leg_l_b", (-1.08, -0.20, 1.0), (0.06, 0.06, 1.0), wood, rot=(0, -0.08, 0))
# Right A
cube("leg_r_f", ( 1.08, 0.20, 1.0), (0.06, 0.06, 1.0), wood, rot=(0, -0.08, 0))
cube("leg_r_b", ( 0.92, -0.20, 1.0), (0.06, 0.06, 1.0), wood, rot=(0, 0.08, 0))

# Cross brace (top horizontal beam, supports bell)
cube("cross_beam", (0, 0, 2.0), (1.2, 0.10, 0.10), wood)

# Bell hanger ring on beam
cyl("hanger", 0.04, 0.10, (0, 0, 1.93), bronze, verts=8)

# Bell body — frustum (wide bottom narrowing to top)
cone("bell_body", 0.30, 0.18, 0.55, (0, 0, 1.55), bronze, verts=18)
# Bell crown (top dome where hanger attaches)
sphere("bell_crown", 0.18, (0, 0, 1.85), bronze, segs=12, rings=8)
# Bell rim (bottom thickening)
cyl("bell_rim", 0.32, 0.05, (0, 0, 1.27), bronze, verts=18)
# Clapper (inside)
sphere("clapper", 0.05, (0, 0, 1.40), bronze, segs=8, rings=6)
cyl("clapper_rod", 0.01, 0.40, (0, 0, 1.55), bronze, verts=6)

# Rope hanging down from clapper for player to pull
cyl("rope", 0.015, 1.0, (0.10, 0, 0.95), rope, verts=6)
sphere("rope_grip", 0.04, (0.10, 0, 0.45), rope, segs=8, rings=6)

export_current_as("VillageBell", "Moon1")
print("[gen_village_bell] Done")
