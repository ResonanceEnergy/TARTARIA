"""
Tuning Node pedestal — ornate stone pillar with crystal slot on top.
Triggers TuningMiniGame when player presses E nearby.
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()

STONE_LIGHT = make_material("Pedestal_Stone_Light", (0.65, 0.62, 0.58), roughness=0.85)
STONE_DARK  = make_material("Pedestal_Stone_Dark",  (0.40, 0.38, 0.34), roughness=0.85)
GOLD_INLAY  = make_material("Pedestal_Gold", (0.90, 0.75, 0.30), roughness=0.3, metallic=0.85,
                            emission=(0.8, 0.6, 0.2), emission_strength=1.5)

def cyl(name, r, d, loc, mat, verts=16):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=d, location=loc, vertices=verts)
    ob = bpy.context.active_object
    ob.name = name
    ob.data.materials.append(mat)
    return ob

# Base (wide low ring)
cyl("Base1", 0.55, 0.08, (0, 0, 0.04), STONE_DARK)
cyl("Base2", 0.50, 0.05, (0, 0, 0.105), STONE_LIGHT)
# Column shaft (hexagonal feel — use 6-sided cylinder)
cyl("Shaft", 0.30, 0.95, (0, 0, 0.62), STONE_LIGHT, verts=6)
# Mid ring detail
cyl("Ring1", 0.34, 0.05, (0, 0, 0.40), STONE_DARK, verts=12)
cyl("Ring2", 0.34, 0.05, (0, 0, 0.85), STONE_DARK, verts=12)
# Gold inlay band
cyl("GoldBand", 0.31, 0.04, (0, 0, 0.62), GOLD_INLAY, verts=12)
# Crown / capital
cyl("Capital", 0.42, 0.06, (0, 0, 1.13), STONE_LIGHT, verts=12)
cyl("Capital2", 0.36, 0.05, (0, 0, 1.18), STONE_DARK, verts=12)
# Crystal slot — recessed circle on top
cyl("Slot", 0.18, 0.04, (0, 0, 1.22), STONE_DARK, verts=24)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "TuningPedestal"
export_fbx("TuningPedestal")
print("[TARTARIA] TuningPedestal done.")
