"""Mud Pool Basin — raised stone rim around the 8m mud pool. Replaces flat primitive."""
import bpy, math, sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()
STONE_WET = make_material("MudRim_StoneWet", (0.42, 0.38, 0.32), roughness=0.85)
MUD = make_material("MudPool_Surface", (0.18, 0.12, 0.08), roughness=0.95)

# Outer rim — torus
bpy.ops.mesh.primitive_torus_add(major_radius=4.2, minor_radius=0.35, location=(0,0,0.20),
                                  major_segments=32, minor_segments=8)
bpy.context.active_object.name = "Rim"
bpy.context.active_object.data.materials.append(STONE_WET)

# 8 weathered stones around the rim
for i in range(8):
    a = i * (math.pi/4)
    x, y = math.cos(a)*4.2, math.sin(a)*4.2
    bpy.ops.mesh.primitive_cube_add(size=0.5, location=(x, y, 0.35))
    ob = bpy.context.active_object
    ob.name = f"Stone_{i}"
    ob.scale = (0.4, 0.4, 0.3 + (i%3)*0.05)
    ob.rotation_euler = (0, 0, a + 0.3)
    ob.data.materials.append(STONE_WET)

# Inner mud surface — flat disc
bpy.ops.mesh.primitive_cylinder_add(radius=3.9, depth=0.04, location=(0,0,0.05), vertices=32)
bpy.context.active_object.name = "MudSurface"
bpy.context.active_object.data.materials.append(MUD)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "MudPoolBasin"
export_fbx("MudPoolBasin")
print("[TARTARIA] MudPoolBasin done.")
