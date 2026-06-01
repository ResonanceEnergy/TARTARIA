"""
Generate 3 Aether resonance crystals (E3=cool blue, A3=amber, D4=pale green).
Faceted octahedron with internal glow channels.
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

VARIANTS = [
    ("Aether_E3_Crystal_BlueIce",  (0.30, 0.65, 0.85), 165),
    ("Aether_A3_Crystal_Amber",    (0.85, 0.60, 0.30), 220),
    ("Aether_D4_Crystal_PaleGreen",(0.55, 0.85, 0.55), 294),
]

def add_octahedron(name, scale, loc, mat):
    # Subdivided cone (4-sided, 2 ends) becomes a faceted bipyramid
    bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=1, radius2=0, depth=2,
                                     end_fill_type='TRIFAN', location=loc)
    ob_top = bpy.context.active_object
    ob_top.name = name + "_Top"
    ob_top.scale = scale
    ob_top.data.materials.append(mat)
    # Mirror for bottom half
    bpy.ops.object.duplicate()
    ob_bot = bpy.context.active_object
    ob_bot.name = name + "_Bot"
    ob_bot.rotation_euler.x = math.pi
    return [ob_top, ob_bot]

for name, color, hz in VARIANTS:
    reset_scene()
    glow = make_material(name + "_Mat", color, roughness=0.25, metallic=0.05,
                         emission=color, emission_strength=3.5)
    objs = add_octahedron(name, (0.35, 0.35, 0.55), (0, 0, 1), glow)
    # Inner glow shaft (smaller cylinder along Z, brighter emission)
    bpy.ops.mesh.primitive_cylinder_add(radius=0.05, depth=1.0, location=(0, 0, 1))
    inner = bpy.context.active_object
    inner.name = name + "_Core"
    inner.data.materials.append(make_material(name + "_Core_Mat", color, roughness=0.0,
                                              emission=(1.0,1.0,1.0), emission_strength=8.0))
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_fbx(name)
    print(f"[TARTARIA] {name} done (tuned to {hz} Hz).")
