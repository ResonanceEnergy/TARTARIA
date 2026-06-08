"""VillageLantern — 2.5m wooden pole + 4-sided glass lamp head with emissive interior."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, cyl, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # Pole (square cross-section)
    pole = cube("Pole", (0, 1.0, 0), (0.06, 1.0, 0.06))
    # Base
    base = cube("Base", (0, 0.05, 0), (0.2, 0.05, 0.2))
    bevel(base, 0.02, 1)
    # Lamp head crossbar (top)
    crossbar = cube("Cross", (0, 2.05, 0), (0.05, 0.05, 0.5))
    # Lantern body — 4-sided cube hollowed
    body = cube("Body", (0, 1.85, 0.5), (0.15, 0.18, 0.15))
    # Hollow it
    hollow = cube("Hollow", (0, 1.85, 0.5), (0.12, 0.15, 0.12))
    bool_diff(body, hollow)
    bevel(body, 0.01, 1)
    # Flame core (emissive icosphere)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.07, location=(0, 1.85, 0.5))
    flame = bpy.context.active_object; flame.name = "Flame"
    # Materials
    pole.data.materials.append(make_polyhaven_material("Lantern_Wood", "black_painted_planks"))
    base.data.materials.append(make_polyhaven_material("Lantern_Base", "medieval_blocks_06"))
    crossbar.data.materials.append(make_polyhaven_material("Lantern_CrossWood", "black_painted_planks"))
    body.data.materials.append(make_polyhaven_material("Lantern_Iron", "green_metal_rust"))
    flame_mat = bpy.data.materials.new("Lantern_Flame")
    flame_mat.use_nodes = True
    bsdf = flame_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (1.0, 0.8, 0.3, 1.0)
        if "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (1.0, 0.7, 0.3, 1.0)
            bsdf.inputs["Emission Strength"].default_value = 8.0
    flame.data.materials.append(flame_mat)
    all_objs = [pole, base, crossbar, body, flame]
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
    bpy.context.view_layer.objects.active = pole
    bpy.ops.object.join()
    o = bpy.context.active_object; o.name = "VillageLantern"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "VillageLantern")


if __name__ == "__main__":
    main()
