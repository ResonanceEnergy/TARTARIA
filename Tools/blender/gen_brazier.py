"""
Generate an ornate iron brazier (Echohaven village perimeter + hero entrance).
Replaces procedural cylinder+sphere fallback.
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()

IRON_DARK = make_material("Iron_Dark", (0.18, 0.16, 0.14), roughness=0.5, metallic=0.7)
IRON_RUST = make_material("Iron_Rust", (0.32, 0.18, 0.10), roughness=0.7, metallic=0.4)
EMBER     = make_material("Ember_Glow", (1.0, 0.55, 0.18), roughness=0.3,
                          emission=(1.0, 0.45, 0.10), emission_strength=4.0)

def add_cyl(name, r, d, loc, rot=(0,0,0), mat=None, verts=24):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=d, location=loc, rotation=rot, vertices=verts)
    ob = bpy.context.active_object
    ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

def add_sphere(name, r, loc, mat=None, segs=16, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=segs, ring_count=rings)
    ob = bpy.context.active_object
    ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

def add_torus(name, major, minor, loc, mat=None, major_segs=24, minor_segs=8):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, location=loc,
                                      major_segments=major_segs, minor_segments=minor_segs)
    ob = bpy.context.active_object
    ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

# Central column (fluted)
add_cyl("Column", 0.12, 0.9, (0, 0, 0.45), mat=IRON_DARK)
# Base flare
add_cyl("ColumnBase", 0.22, 0.06, (0, 0, 0.03), mat=IRON_DARK)
# Ground plate (claw foot base)
for i in range(3):
    a = i * (2*math.pi/3)
    x, y = math.cos(a) * 0.28, math.sin(a) * 0.28
    add_cyl(f"Claw_{i}", 0.04, 0.12, (x, y, 0.06), rot=(math.radians(-20)*math.cos(a), math.radians(-20)*math.sin(a), 0), mat=IRON_RUST)

# Brazier bowl — sphere bottom-half + rim torus
add_sphere("Bowl", 0.35, (0, 0, 1.0), mat=IRON_DARK)
add_torus("BowlRim", 0.36, 0.04, (0, 0, 1.15), mat=IRON_DARK)

# Ember pile (visible from above)
add_sphere("Ember1", 0.18, (0, 0, 1.12), mat=EMBER)
for i in range(5):
    a = i * (2*math.pi/5) + 0.3
    add_sphere(f"Ember{i+2}", 0.08, (math.cos(a)*0.16, math.sin(a)*0.16, 1.15), mat=EMBER)

# Decorative cross-bars on column
for i, h in enumerate([0.35, 0.55, 0.75]):
    add_torus(f"Ring_{i}", 0.13, 0.012, (0, 0, h), mat=IRON_RUST, major_segs=16, minor_segs=6)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "EchohavenBrazier"
export_fbx("EchohavenBrazier")
print("[TARTARIA] EchohavenBrazier done.")
