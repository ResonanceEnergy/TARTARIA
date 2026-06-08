"""StarDome hero — 25m diameter x 18m tall hemisphere on cylindrical drum, per docs/15 §7."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel,
                   make_polyhaven_material, save_and_export)
import bpy
from mathutils import Vector

R = 12.5      # dome radius (25m diameter)
DRUM_H = 6.0  # cylindrical drum supporting the dome
DOME_H = 12.0 # dome height (total H_WALL + dome rise = 18m)
DOOR_W, DOOR_H = 2.5, 3.5
WIN_W, WIN_H = 1.8, 2.5


def main():
    reset_scene()
    # Drum (large cylinder base)
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=R, depth=DRUM_H, location=(0, DRUM_H/2, 0), rotation=(math.radians(90), 0, 0))
    drum = bpy.context.active_object; drum.name = "Drum"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # 4 huge arched doors around the drum at cardinal directions
    for angle in [0, 90, 180, 270]:
        dx = R * math.cos(math.radians(angle))
        dz = R * math.sin(math.radians(angle))
        bpy.ops.mesh.primitive_cube_add(location=(dx, DOOR_H/2, dz), size=2.0)
        cutter = bpy.context.active_object
        cutter.scale = (DOOR_W/2, DOOR_H/2, 1.0)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        cutter.rotation_euler = (0, math.radians(-angle + 90), 0)
        bool_diff(drum, cutter)
    # 8 small windows between doors (around drum, higher up)
    for angle in [45, 135, 225, 315]:
        dx = R * math.cos(math.radians(angle))
        dz = R * math.sin(math.radians(angle))
        bpy.ops.mesh.primitive_cube_add(location=(dx, 4.5, dz), size=2.0)
        win = bpy.context.active_object
        win.scale = (WIN_W/2, WIN_H/2, 1.0)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        win.rotation_euler = (0, math.radians(-angle + 90), 0)
        bool_diff(drum, win)
    bevel(drum, 0.06, 2)
    # Dome (UV sphere, cut bottom half)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=48, ring_count=24, radius=R + 0.3, location=(0, DRUM_H, 0))
    dome = bpy.context.active_object; dome.name = "Dome"
    # Cut off bottom half via boolean
    bpy.ops.mesh.primitive_cube_add(location=(0, -R, 0), size=2*R + 2)
    cutter = bpy.context.active_object
    bool_diff(dome, cutter)
    # Scale to be slightly taller than half-sphere (oblate egg)
    dome.scale = (1.0, DOME_H / R, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Oculus at top
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=1.5, depth=R, location=(0, DRUM_H + DOME_H, 0), rotation=(math.radians(90), 0, 0))
    oculus_cutter = bpy.context.active_object
    bool_diff(dome, oculus_cutter)
    bevel(dome, 0.04, 1)
    # Decorative ring around drum-dome junction
    bpy.ops.mesh.primitive_torus_add(major_radius=R + 0.2, minor_radius=0.3, major_segments=48, minor_segments=8, location=(0, DRUM_H, 0))
    ring = bpy.context.active_object; ring.name = "Ring"
    # Materials
    drum.data.materials.append(make_polyhaven_material("Dome_Drum", "medieval_blocks_06"))
    dome.data.materials.append(make_polyhaven_material("Dome_Roof", "roof_slates_03"))
    ring.data.materials.append(make_polyhaven_material("Dome_Ring", "medieval_blocks_06"))
    # Smart UV + join
    for o in (drum, dome, ring):
        bpy.context.view_layer.objects.active = o
        o.select_set(True)
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
        bpy.ops.object.mode_set(mode="OBJECT")
        o.select_set(False)
    drum.select_set(True); dome.select_set(True); ring.select_set(True)
    bpy.context.view_layer.objects.active = drum
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "StarDome"
    # Pivot at bottom-center
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    save_and_export(o, "StarDome")


if __name__ == "__main__":
    main()
