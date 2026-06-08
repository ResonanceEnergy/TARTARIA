"""PathStone — Single irregular flagstone for village path (place many)."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # Flat irregular hexagonal stone
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.45, depth=0.08, location=(0, 0.04, 0), rotation=(math.radians(90), 0, 0))
    stone = bpy.context.active_object; stone.name = "Stone"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Slight scale variation per axis to look natural
    stone.scale = (1.0, 1.0, 0.85)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel(stone, 0.015, 1)
    stone.data.materials.append(make_polyhaven_material("Path_Stone", "gray_rocks"))
    bpy.context.view_layer.objects.active = stone
    stone.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")
    bbox = [stone.matrix_world @ Vector(c) for c in stone.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    stone.location = (0, 0, 0)
    save_and_export(stone, "PathStone")


if __name__ == "__main__":
    main()
