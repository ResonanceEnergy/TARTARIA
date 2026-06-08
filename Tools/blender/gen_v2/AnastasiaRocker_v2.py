"""AnastasiaRocker — Wooden rocking chair (closes R119 missing Anastasia.prefab)."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

def main():
    reset_scene()
    # Seat
    seat = cube("Seat", (0, 0.5, 0), (0.4, 0.05, 0.4))
    # Back rest (3 vertical slats)
    slats = []
    for x_off in [-0.3, 0, 0.3]:
        s = cube(f"Slat_{x_off}", (x_off, 0.95, -0.4), (0.04, 0.4, 0.04))
        slats.append(s)
    # Back top crossbar
    back_top = cube("BackTop", (0, 1.35, -0.4), (0.35, 0.05, 0.05))
    # Arms (left + right)
    arms = []
    for x_pos in [-0.4, 0.4]:
        a = cube(f"Arm_{x_pos}", (x_pos, 0.75, 0), (0.05, 0.05, 0.35))
        arms.append(a)
    # Arm supports (front)
    arm_supports = []
    for x_pos in [-0.4, 0.4]:
        s = cube(f"ArmSup_{x_pos}", (x_pos, 0.65, 0.35), (0.04, 0.15, 0.04))
        arm_supports.append(s)
    # 4 legs
    legs = []
    for (x, z) in [(-0.35, -0.35), (0.35, -0.35), (-0.35, 0.35), (0.35, 0.35)]:
        l = cube(f"Leg_{x}_{z}", (x, 0.25, z), (0.04, 0.25, 0.04))
        legs.append(l)
    # 2 ROCKERS (curved sled-runners — approximate via tapered cubes)
    # Front-to-back curve approximated via wedges
    rockers = []
    for side_z in [-0.35, 0.35]:
        # Use a cylinder cap as a "rocker bottom" — wide axis along Z
        bpy.ops.mesh.primitive_cube_add(location=(0, 0.05, side_z), size=2.0)
        r = bpy.context.active_object; r.name = f"Rocker_{side_z}"
        r.scale = (0.5, 0.05, 0.05)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        bevel(r, 0.02, 1)
        rockers.append(r)
    # Materials
    all_objs = [seat, back_top] + slats + arms + arm_supports + legs + rockers
    for o in all_objs:
        o.data.materials.append(make_polyhaven_material("Rocker_Wood", "black_painted_planks"))
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
        o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = seat
    bpy.ops.object.join()
    o = bpy.context.active_object; o.name = "AnastasiaRocker"
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "AnastasiaRocker")


if __name__ == "__main__":
    main()
