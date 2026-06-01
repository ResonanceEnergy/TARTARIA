"""
Generate Anastasia's rocking chair as a proper Blender mesh.
Run inside Blender: File > Open this .py via Scripting workspace, or:
  blender --background --python tools/blender/gen_anastasia_chair.py
"""
import bpy, math
from mathutils import Vector
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()

OAK   = make_material("Oak_Wood",        (0.42, 0.28, 0.16), roughness=0.65)
DARK  = make_material("Oak_Wood_Dark",   (0.32, 0.20, 0.10), roughness=0.65)

def add_cube(name, size, location, scale=(1,1,1), rotation=(0,0,0), mat=None):
    bpy.ops.mesh.primitive_cube_add(size=size, location=location)
    ob = bpy.context.active_object
    ob.name = name
    ob.scale = scale
    ob.rotation_euler = rotation
    if mat: ob.data.materials.append(mat)
    return ob

def add_cylinder(name, radius, depth, location, rotation=(0,0,0), mat=None):
    bpy.ops.mesh.primitive_cylinder_add(radius=radius, depth=depth, location=location, rotation=rotation)
    ob = bpy.context.active_object
    ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

# --- Seat ---
seat = add_cube("Seat", 1, (0, 0, 0.42), scale=(0.30, 0.30, 0.04), mat=OAK)

# --- Backrest (5 vertical slats) ---
for i in range(5):
    x = (i - 2) * 0.10
    add_cube(f"BackSlat_{i}", 1, (x, -0.24, 0.85),
             scale=(0.025, 0.03, 0.42), mat=DARK)
# Top rail
add_cube("BackTopRail", 1, (0, -0.24, 1.26), scale=(0.30, 0.04, 0.04), mat=OAK)

# --- 4 legs ---
LEGS = [(-0.22, -0.22), (0.22, -0.22), (-0.22, 0.22), (0.22, 0.22)]
for i, (x, y) in enumerate(LEGS):
    add_cylinder(f"Leg_{i}", 0.025, 0.42, (x, y, 0.21), mat=OAK)

# --- Curved rockers (2 sides) ---
# Use 16-segment torus quadrant to fake the curve
for side, sx in enumerate([-0.22, 0.22]):
    # Bend a long cube via subdiv + simple deform later would be ideal;
    # for v1 we'll approximate with 5 short cubes arranged in a shallow arc
    arc_segs = 7
    arc_radius = 0.65
    for j in range(arc_segs):
        t = (j / (arc_segs - 1)) - 0.5  # -0.5 .. 0.5
        x = sx
        y = t * 0.7
        z = arc_radius - math.sqrt(arc_radius*arc_radius - (t*0.7)*(t*0.7)) - 0.02
        add_cube(f"Rocker_{side}_{j}", 1, (x, y, z),
                 scale=(0.04, 0.06, 0.04), mat=DARK)

# --- Armrests ---
for side, sx in enumerate([-0.30, 0.30]):
    add_cube(f"Arm_{side}", 1, (sx, 0, 0.65),
             scale=(0.03, 0.32, 0.03), mat=OAK)
    # Arm support post
    add_cylinder(f"ArmPost_{side}", 0.018, 0.18, (sx, 0.20, 0.55), mat=OAK)

# Set as a single empty parent for tidy export
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
joined = bpy.context.active_object
joined.name = "AnastasiaRockingChair"

export_fbx("AnastasiaRockingChair")
print("[TARTARIA] AnastasiaRockingChair done.")
