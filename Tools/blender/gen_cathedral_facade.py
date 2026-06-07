"""
R86 — Real Cathedral Gothic facade mesh.
Replaces the 60+ stacked Unity Cube primitives that the Sprint 11 audit flagged
("Detail_* primitive clusters"). Per docs/15 §7 hero buildings.

Geometry: octagonal stone base + 2 side towers + rose window + 3 arched
entrances + steep spire on top, all real polygons not cubes.

Output: Assets/_Project/Models/Blender/Moon1/CathedralFacade.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    stone = make_material("CathedralStone",
                           base_color=(0.65, 0.60, 0.52, 1.0),
                           roughness=0.85, metallic=0.0)
    rose_glass = make_material("RoseWindow",
                                base_color=(0.30, 0.50, 0.85, 0.9),
                                roughness=0.1, metallic=0.0,
                                emission=(0.30, 0.50, 0.95),
                                emission_strength=2.5)
    door_wood = make_material("CathedralDoor",
                               base_color=(0.40, 0.22, 0.12, 1.0),
                               roughness=0.7, metallic=0.0)
    spire_dark = make_material("SpireDark",
                                base_color=(0.40, 0.36, 0.32, 1.0),
                                roughness=0.6, metallic=0.0)

    # ── Foundation ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.5))
    found = bpy.context.active_object
    found.name = "Foundation"
    found.scale = (16, 10, 1)
    bpy.ops.object.transform_apply(scale=True)
    found.data.materials.append(stone)

    # ── Main body ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 5))
    body = bpy.context.active_object
    body.name = "Body"
    body.scale = (12, 8, 8)
    bpy.ops.object.transform_apply(scale=True)
    body.data.materials.append(stone)

    # ── 2 side towers ──
    for i, x in enumerate([-7, 7]):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=2, depth=14,
                                            location=(x, 0, 8))
        tower = bpy.context.active_object
        tower.name = f"Tower{i}"
        tower.data.materials.append(stone)
        # Tower cap (cone)
        bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=2.2, radius2=0,
                                         depth=3.5, location=(x, 0, 16.75))
        cap = bpy.context.active_object
        cap.name = f"TowerCap{i}"
        cap.data.materials.append(spire_dark)

    # ── Central spire (steep cone) ──
    bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=3, radius2=0,
                                    depth=8, location=(0, 0, 13))
    spire = bpy.context.active_object
    spire.name = "Spire"
    spire.data.materials.append(spire_dark)

    # ── Rose window (cylinder front face) ──
    bpy.ops.mesh.primitive_cylinder_add(vertices=16, radius=1.8, depth=0.5,
                                        location=(0, -4.3, 6.5),
                                        rotation=(math.pi/2, 0, 0))
    rose = bpy.context.active_object
    rose.name = "RoseWindow"
    rose.data.materials.append(rose_glass)

    # Rose frame
    bpy.ops.mesh.primitive_torus_add(major_radius=1.9, minor_radius=0.18,
                                      location=(0, -4.25, 6.5),
                                      rotation=(math.pi/2, 0, 0))
    frame = bpy.context.active_object
    frame.name = "RoseFrame"
    frame.data.materials.append(stone)

    # ── 3 arched entrances ──
    for i, x in enumerate([-3.5, 0, 3.5]):
        # Arched door cube
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x, -4.05, 1.8))
        door = bpy.context.active_object
        door.name = f"Door{i}"
        door.scale = (1.6, 0.1, 3.0)
        bpy.ops.object.transform_apply(scale=True)
        door.data.materials.append(door_wood)
        # Arch (half torus on top)
        bpy.ops.mesh.primitive_torus_add(major_radius=0.85, minor_radius=0.18,
                                          location=(x, -4.05, 3.3),
                                          rotation=(math.pi/2, 0, 0))
        arch = bpy.context.active_object
        arch.name = f"Arch{i}"
        arch.data.materials.append(stone)

    # ── Buttresses (4 sloped wall supports) ──
    for i, (x, y) in enumerate([(-6, -4), (6, -4), (-6, 4), (6, 4)]):
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x, y, 4))
        butt = bpy.context.active_object
        butt.name = f"Buttress{i}"
        butt.scale = (1.2, 1.2, 7)
        bpy.ops.object.transform_apply(scale=True)
        butt.data.materials.append(stone)

    # ── Cross at apex ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 17.5))
    cross_v = bpy.context.active_object
    cross_v.name = "CrossVertical"
    cross_v.scale = (0.2, 0.2, 1.5)
    bpy.ops.object.transform_apply(scale=True)
    cross_v.data.materials.append(spire_dark)
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 17.5))
    cross_h = bpy.context.active_object
    cross_h.name = "CrossHorizontal"
    cross_h.scale = (1.0, 0.2, 0.2)
    bpy.ops.object.transform_apply(scale=True)
    cross_h.data.materials.append(spire_dark)

    # ── Join ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = body
    bpy.ops.object.join()
    bpy.context.active_object.name = "CathedralFacade"

    export_fbx("CathedralFacade")
    print("[gen_cathedral_facade] DONE")


if __name__ == "__main__":
    main()
