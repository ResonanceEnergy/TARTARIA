"""
R75 — Real village house mesh for Moon 1 9-village.
Cube base (4x3x4) + pyramid roof + door + 2 windows + chimney.
One geometry, applied to all 9 buildings with material color variations.

Output: Assets/_Project/Models/Blender/Moon1/VillageHouse.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    plaster = make_material("HousePlaster",
                             base_color=(0.88, 0.82, 0.72, 1.0),
                             roughness=0.85, metallic=0.0)
    wood = make_material("HouseWood",
                          base_color=(0.40, 0.25, 0.15, 1.0),
                          roughness=0.7, metallic=0.0)
    roof = make_material("HouseRoof",
                          base_color=(0.55, 0.30, 0.20, 1.0),
                          roughness=0.6, metallic=0.0)
    door = make_material("HouseDoor",
                          base_color=(0.30, 0.18, 0.10, 1.0),
                          roughness=0.7, metallic=0.0)
    window_glow = make_material("HouseWindow",
                                  base_color=(1.0, 0.85, 0.45, 1.0),
                                  roughness=0.2, metallic=0.0,
                                  emission=(1.0, 0.85, 0.45),
                                  emission_strength=1.5)
    chimney = make_material("HouseChimney",
                              base_color=(0.40, 0.36, 0.32, 1.0),
                              roughness=0.85, metallic=0.0)

    # ── Body (cube 4x3x4) ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 1.5))
    body = bpy.context.active_object
    body.name = "Body"
    body.scale = (4.0, 4.0, 3.0)
    bpy.ops.object.transform_apply(scale=True)
    body.data.materials.append(plaster)

    # ── Pyramid roof (cone with 4 sides) ──
    bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=3.0, radius2=0.0,
                                    depth=2.5, location=(0, 0, 4.25))
    roof_obj = bpy.context.active_object
    roof_obj.name = "Roof"
    roof_obj.rotation_euler = (0, 0, math.radians(45))  # align with body
    roof_obj.data.materials.append(roof)

    # ── Door (small front cube) ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, -2.05, 0.9))
    door_obj = bpy.context.active_object
    door_obj.name = "Door"
    door_obj.scale = (0.7, 0.05, 1.4)
    bpy.ops.object.transform_apply(scale=True)
    door_obj.data.materials.append(door)

    # ── 2 windows (front+back) ──
    for ydir, name in [(-2.05, "WindowFront"), (2.05, "WindowBack")]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, ydir, 1.8))
        win = bpy.context.active_object
        win.name = name
        win.scale = (1.0, 0.05, 0.6)
        bpy.ops.object.transform_apply(scale=True)
        win.data.materials.append(window_glow)

    # ── 2 side windows ──
    for xdir, name in [(-2.05, "WindowLeft"), (2.05, "WindowRight")]:
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(xdir, 0, 1.8))
        win = bpy.context.active_object
        win.name = name
        win.scale = (0.05, 0.8, 0.5)
        bpy.ops.object.transform_apply(scale=True)
        win.data.materials.append(window_glow)

    # ── Chimney ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(1.3, 1.3, 5.2))
    ch = bpy.context.active_object
    ch.name = "Chimney"
    ch.scale = (0.5, 0.5, 1.4)
    bpy.ops.object.transform_apply(scale=True)
    ch.data.materials.append(chimney)

    # ── Doorframe ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, -2.02, 0.9))
    df = bpy.context.active_object
    df.name = "DoorFrame"
    df.scale = (0.95, 0.06, 1.6)
    bpy.ops.object.transform_apply(scale=True)
    df.data.materials.append(wood)

    # ── Foundation stone slab ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.05))
    foundation = bpy.context.active_object
    foundation.name = "Foundation"
    foundation.scale = (4.4, 4.4, 0.1)
    bpy.ops.object.transform_apply(scale=True)
    foundation.data.materials.append(chimney)

    # ── Join all ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.join()
    bpy.context.active_object.name = "VillageHouse"

    export_fbx("VillageHouse")
    print("[gen_village_house] DONE")


if __name__ == "__main__":
    main()
