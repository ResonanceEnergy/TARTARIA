"""Brazier — 0.6m diameter bronze basin on 1.2m tripod stand."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, cyl, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # 3 tripod legs (cylinders) tilted outward from center
    legs = []
    for angle in [0, 120, 240]:
        r = math.radians(angle)
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.05, depth=1.4, location=(math.cos(r) * 0.25, 0.7, math.sin(r) * 0.25), rotation=(math.radians(90 + 10*math.cos(r)), 0, math.radians(10*math.sin(r))))
        leg = bpy.context.active_object; leg.name = f"Leg_{angle}"
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        legs.append(leg)
    # Basin (squat torus + cylinder bottom)
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.45, depth=0.3, location=(0, 1.4, 0), rotation=(math.radians(90), 0, 0))
    basin = bpy.context.active_object; basin.name = "Basin"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Hollow out top
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=0.4, depth=0.25, location=(0, 1.5, 0), rotation=(math.radians(90), 0, 0))
    hollow = bpy.context.active_object
    bool_diff(basin, hollow)
    bevel(basin, 0.02, 1)
    # Flame inner (emissive icosphere — represents fire)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.25, location=(0, 1.6, 0))
    flame = bpy.context.active_object; flame.name = "Flame"
    flame.scale = (0.7, 1.3, 0.7)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Materials
    for leg in legs:
        leg.data.materials.append(make_polyhaven_material("Brazier_Iron", "green_metal_rust"))
    basin.data.materials.append(make_polyhaven_material("Brazier_Bronze", "green_metal_rust"))
    flame_mat = bpy.data.materials.new("Brazier_Flame")
    flame_mat.use_nodes = True
    bsdf = flame_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (1.0, 0.6, 0.1, 1.0)
        if "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (1.0, 0.5, 0.1, 1.0)
            bsdf.inputs["Emission Strength"].default_value = 5.0
    flame.data.materials.append(flame_mat)
    # Join
    all_objs = legs + [basin, flame]
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
    bpy.context.view_layer.objects.active = basin
    bpy.ops.object.join()
    o = bpy.context.active_object; o.name = "Brazier"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "Brazier")


if __name__ == "__main__":
    main()
