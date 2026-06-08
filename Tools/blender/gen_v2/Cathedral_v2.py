"""Cathedral hero (RESTORED state) — 18m wide x 14m deep x 16m tall Gothic stone with twin front towers."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel, solidify, pitched_roof,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

W, D = 18.0, 14.0
H_NAVE = 10.0
H_ROOF = 4.0
WALL_THICK = 0.5
H_TOWER = 16.0
TOWER_W = 3.0
DOOR_W, DOOR_H = 2.5, 4.5
ARCH_W, ARCH_H = 1.5, 3.5  # nave windows


def main():
    reset_scene()
    # NAVE (main body)
    nave = cube("Nave", (0, H_NAVE/2, 0), (W/2, H_NAVE/2, D/2))
    # Grand front door (center)
    bool_diff(nave, cube("Door", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # 6 tall nave windows on each long side
    for x_pos in [-W/2, W/2]:
        for z in [-5.0, -2.5, 0, 2.5, 5.0]:
            bool_diff(nave, cube("Arch", (x_pos, 5.5, z), (WALL_THICK, ARCH_H/2, ARCH_W/2)))
    # Rose window front (above door)
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=1.6, depth=WALL_THICK*2, location=(0, 8.0, D/2), rotation=(0, 0, 0))
    rose_cutter = bpy.context.active_object
    bool_diff(nave, rose_cutter)
    solidify(nave, WALL_THICK)
    bevel(nave, 0.05, 2)
    # Pitched roof
    roof = pitched_roof("Roof", (0, H_NAVE + H_ROOF/2, 0), W + 0.4, D + 0.4, H_ROOF)
    bevel(roof, 0.04, 1)
    # Twin front towers (left + right of facade)
    towers = []
    for x_off in [-W/2 + TOWER_W/2 + 0.5, W/2 - TOWER_W/2 - 0.5]:
        t = cube(f"Tower_{x_off}", (x_off, H_TOWER/2, D/2 + 0.5), (TOWER_W/2, H_TOWER/2, TOWER_W/2))
        # Bell windows in tower (top)
        bool_diff(t, cube("Bell", (0, H_TOWER - 2.0, 0), (TOWER_W/2 - 0.2, 1.0, TOWER_W/2)))
        bevel(t, 0.05, 2)
        towers.append(t)
    # Tower spires (cones on top of each tower)
    spires = []
    for x_off in [-W/2 + TOWER_W/2 + 0.5, W/2 - TOWER_W/2 - 0.5]:
        bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=TOWER_W/2 + 0.2, radius2=0, depth=3.0, location=(x_off, H_TOWER + 1.5, D/2 + 0.5), rotation=(math.radians(90), 0, 0))
        sp = bpy.context.active_object; sp.name = f"Spire_{x_off}"
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        bevel(sp, 0.03, 1)
        spires.append(sp)
    # 6 buttresses (flying buttress simplification — angled cubes)
    buttresses = []
    for x_pos, sign in [(-W/2, -1), (W/2, 1)]:
        for z in [-4, 0, 4]:
            b = cube(f"Buttress_{x_pos}_{z}", (x_pos + sign * 0.9, 3.0, z), (0.5, 3.0, 0.4))
            buttresses.append(b)
    # Door plane
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Materials
    nave.data.materials.append(make_polyhaven_material("Cath_Nave", "medieval_blocks_06"))
    roof.data.materials.append(make_polyhaven_material("Cath_Roof", "roof_slates_03"))
    for t in towers:
        t.data.materials.append(make_polyhaven_material("Cath_Tower", "medieval_blocks_06"))
    for sp in spires:
        sp.data.materials.append(make_polyhaven_material("Cath_Spire", "roof_slates_03"))
    for b in buttresses:
        b.data.materials.append(make_polyhaven_material("Cath_Buttress", "medieval_blocks_06"))
    door_p.data.materials.append(make_polyhaven_material("Cath_Door", "black_painted_planks"))
    all_objs = [nave, roof, door_p] + towers + spires + buttresses
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
    bpy.context.view_layer.objects.active = nave
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Cathedral"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "Cathedral")


if __name__ == "__main__":
    main()
