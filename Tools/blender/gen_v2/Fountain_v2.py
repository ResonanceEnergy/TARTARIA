"""Fountain — 8m diameter basin + 5m central column (Thread of Memory, spec §7)."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, cyl, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # Outer basin ring (large cylinder)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=4.0, depth=1.2, location=(0, 0.6, 0), rotation=(math.radians(90), 0, 0))
    basin = bpy.context.active_object; basin.name = "Basin"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Hollow it
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=3.7, depth=1.0, location=(0, 0.7, 0), rotation=(math.radians(90), 0, 0))
    hollow = bpy.context.active_object
    bool_diff(basin, hollow)
    bevel(basin, 0.04, 2)
    # Water surface (inside basin)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=3.65, depth=0.05, location=(0, 0.95, 0), rotation=(math.radians(90), 0, 0))
    water = bpy.context.active_object; water.name = "Water"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Central column
    column = cyl("Column", (0, 2.5, 0), 0.4, 5.0)
    # Column base (wider)
    col_base = cyl("ColBase", (0, 0.3, 0), 0.7, 0.6)
    # Top finial (sphere)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=0.5, location=(0, 5.2, 0))
    finial = bpy.context.active_object; finial.name = "Finial"
    # 4 water-spout bowls below column top
    bowls = []
    for angle in [0, 90, 180, 270]:
        r = math.radians(angle)
        bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.35, depth=0.15, location=(math.cos(r) * 0.65, 4.0, math.sin(r) * 0.65), rotation=(math.radians(90), 0, 0))
        bowl = bpy.context.active_object; bowl.name = f"Bowl_{angle}"
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        bowls.append(bowl)
    # Materials
    basin.data.materials.append(make_polyhaven_material("Fountain_Marble", "marble_cliff_01"))
    water_mat = bpy.data.materials.new("Fountain_Water")
    water_mat.use_nodes = True
    bsdf = water_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (0.2, 0.5, 0.8, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.05
        bsdf.inputs["Metallic"].default_value = 0.1
        # Translucent water
        if "Transmission Weight" in bsdf.inputs:
            bsdf.inputs["Transmission Weight"].default_value = 0.7
        elif "Transmission" in bsdf.inputs:
            bsdf.inputs["Transmission"].default_value = 0.7
    water.data.materials.append(water_mat)
    column.data.materials.append(make_polyhaven_material("Fountain_Column", "marble_cliff_01"))
    col_base.data.materials.append(make_polyhaven_material("Fountain_Base", "marble_cliff_01"))
    finial.data.materials.append(make_polyhaven_material("Fountain_Finial", "medieval_blocks_06"))
    for b in bowls:
        b.data.materials.append(make_polyhaven_material("Fountain_Bowl", "marble_cliff_01"))
    all_objs = [basin, water, column, col_base, finial] + bowls
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
    o = bpy.context.active_object; o.name = "Fountain"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "Fountain")


if __name__ == "__main__":
    main()
