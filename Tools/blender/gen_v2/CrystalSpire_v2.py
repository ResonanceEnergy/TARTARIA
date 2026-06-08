"""CrystalSpire hero — 3m base x 15m tall stone tower w/ glowing crystal apex, per docs/15 §7."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

R_BASE, R_TOP = 1.5, 0.9  # tapered tower
H_TOWER = 12.0            # stone tower
H_CRYSTAL = 3.0           # crystal apex on top = 15m total
WALL_THICK = 0.3
DOOR_W, DOOR_H = 0.9, 2.0


def main():
    reset_scene()
    # Tapered hexagonal tower (6 sides)
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=R_BASE, depth=H_TOWER, location=(0, H_TOWER/2, 0), rotation=(math.radians(90), 0, 0))
    tower = bpy.context.active_object; tower.name = "Tower"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Door
    bool_diff(tower, cube("Door", (0, DOOR_H/2, R_BASE), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # 4 narrow windows up the tower
    for y in [3.0, 5.5, 8.0, 10.5]:
        bool_diff(tower, cube("Win", (0, y, R_BASE), (0.15, 0.5, WALL_THICK)))
    bevel(tower, 0.04, 2)
    # Crystal apex — octahedron stretched (icosahedron simplified)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=0.8, location=(0, H_TOWER + H_CRYSTAL/2 + 0.3, 0))
    crystal = bpy.context.active_object; crystal.name = "Crystal"
    crystal.scale = (0.7, H_CRYSTAL/1.6, 0.7)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Crystal base ring
    bpy.ops.mesh.primitive_torus_add(major_radius=R_TOP + 0.1, minor_radius=0.15, major_segments=24, minor_segments=8, location=(0, H_TOWER + 0.2, 0))
    ring = bpy.context.active_object; ring.name = "Ring"
    # Materials
    tower.data.materials.append(make_polyhaven_material("Spire_Stone", "medieval_blocks_06"))
    ring.data.materials.append(make_polyhaven_material("Spire_Ring", "medieval_blocks_06"))
    # Crystal material — emissive blue (no Polyhaven, custom)
    crys_mat = bpy.data.materials.new(name="Spire_Crystal")
    crys_mat.use_nodes = True
    bsdf = crys_mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (0.3, 0.6, 1.0, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.1
        bsdf.inputs["Metallic"].default_value = 0.3
        if "Emission Color" in bsdf.inputs:
            bsdf.inputs["Emission Color"].default_value = (0.3, 0.7, 1.0, 1.0)
            bsdf.inputs["Emission Strength"].default_value = 3.0
    crystal.data.materials.append(crys_mat)
    # UV unwrap
    for o in (tower, crystal, ring):
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
        o.select_set(False)
    tower.select_set(True); crystal.select_set(True); ring.select_set(True)
    bpy.context.view_layer.objects.active = tower
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "CrystalSpire"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "CrystalSpire")


if __name__ == "__main__":
    main()
