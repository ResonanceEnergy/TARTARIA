"""MarketStall — 2m x 1.5m wooden frame + striped fabric roof + display table."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # 4 corner posts
    posts = []
    for (x, z) in [(-0.9, -0.6), (0.9, -0.6), (-0.9, 0.6), (0.9, 0.6)]:
        p = cube(f"Post_{x}_{z}", (x, 1.0, z), (0.06, 1.0, 0.06))
        posts.append(p)
    # Roof crossbeams
    beam_front = cube("BeamF", (0, 2.0, 0.6), (0.95, 0.05, 0.05))
    beam_back = cube("BeamB", (0, 2.0, -0.6), (0.95, 0.05, 0.05))
    beam_left = cube("BeamL", (-0.9, 2.0, 0), (0.05, 0.05, 0.6))
    beam_right = cube("BeamR", (0.9, 2.0, 0), (0.05, 0.05, 0.6))
    # Tilted roof panel (slight forward tilt)
    bpy.ops.mesh.primitive_cube_add(location=(0, 2.15, 0), size=2.0)
    roof = bpy.context.active_object; roof.name = "Roof"
    roof.scale = (1.0, 0.025, 0.75)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    roof.rotation_euler = (math.radians(-10), 0, 0)
    # Display table (front edge)
    table = cube("Table", (0, 0.8, 0.55), (0.85, 0.04, 0.4))
    # Table legs / front skirt
    skirt = cube("Skirt", (0, 0.4, 0.55), (0.85, 0.35, 0.04))
    # Back wall (vertical board)
    back = cube("Back", (0, 1.2, -0.6), (0.9, 1.0, 0.04))
    # Materials
    all_objs = posts + [beam_front, beam_back, beam_left, beam_right, table, skirt, back]
    for o in all_objs:
        o.data.materials.append(make_polyhaven_material("Stall_Wood", "black_painted_planks"))
    # Roof gets fabric-like (slate slot for now)
    roof.data.materials.append(make_polyhaven_material("Stall_Fabric", "roof_slates_03"))
    all_objs.append(roof)
    for o in all_objs:
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
        o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = table
    bpy.ops.object.join()
    o = bpy.context.active_object; o.name = "MarketStall"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "MarketStall")


if __name__ == "__main__":
    main()
