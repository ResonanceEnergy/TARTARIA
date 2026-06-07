"""
R77 — Anastasia Rocker prefab bake (gap #1 from Sprint 13 punch list).
Per docs/15 §8 — Anastasia sits in a rocking chair near the village edge,
sings a lullaby on Day 13. Mecanim-ready Mesh: chair + character placeholder.

Output: Assets/_Project/Models/Blender/Moon1/AnastasiaRocker.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    chair_wood = make_material("ChairWood",
                                base_color=(0.55, 0.32, 0.18, 1.0),
                                roughness=0.7, metallic=0.0)
    velvet = make_material("ChairVelvet",
                            base_color=(0.5, 0.15, 0.2, 1.0),
                            roughness=0.85, metallic=0.0)
    skin = make_material("AnastasiaSkin",
                          base_color=(0.95, 0.78, 0.65, 1.0),
                          roughness=0.6, metallic=0.0)
    dress = make_material("AnastasiaDress",
                           base_color=(0.85, 0.78, 0.92, 1.0),
                           roughness=0.7, metallic=0.0,
                           emission=(0.6, 0.55, 0.75),
                           emission_strength=0.3)
    hair = make_material("AnastasiaHair",
                          base_color=(0.95, 0.75, 0.35, 1.0),
                          roughness=0.5, metallic=0.0)

    # ── Rocking chair — 2 curved runners ──
    for i, x in enumerate([-0.4, 0.4]):
        bpy.ops.mesh.primitive_torus_add(major_radius=0.7, minor_radius=0.05,
                                          location=(x, 0, 0.1),
                                          rotation=(math.pi/2, 0, 0))
        runner = bpy.context.active_object
        runner.name = f"Runner{i}"
        # Only take bottom half by scaling
        runner.scale = (1, 1, 0.5)
        bpy.ops.object.transform_apply(scale=True)
        runner.data.materials.append(chair_wood)

    # ── Chair seat ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.6))
    seat = bpy.context.active_object
    seat.name = "Seat"
    seat.scale = (0.9, 0.7, 0.12)
    bpy.ops.object.transform_apply(scale=True)
    seat.data.materials.append(velvet)

    # ── Chair backrest ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.30, 1.2))
    back = bpy.context.active_object
    back.name = "Backrest"
    back.scale = (0.9, 0.1, 1.0)
    bpy.ops.object.transform_apply(scale=True)
    back.data.materials.append(chair_wood)

    # ── Backrest pillow ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.20, 1.1))
    pillow = bpy.context.active_object
    pillow.name = "Pillow"
    pillow.scale = (0.7, 0.15, 0.6)
    bpy.ops.object.transform_apply(scale=True)
    pillow.data.materials.append(velvet)

    # ── Armrests ──
    for i, x in enumerate([-0.45, 0.45]):
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x, 0, 0.85))
        arm = bpy.context.active_object
        arm.name = f"Armrest{i}"
        arm.scale = (0.08, 0.7, 0.08)
        bpy.ops.object.transform_apply(scale=True)
        arm.data.materials.append(chair_wood)

    # ── Anastasia body (seated) ──
    # Torso (oval)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.35, location=(0, -0.05, 1.1))
    torso = bpy.context.active_object
    torso.name = "Torso"
    torso.scale = (0.8, 0.7, 1.1)
    bpy.ops.object.transform_apply(scale=True)
    torso.data.materials.append(dress)

    # Head
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.22, location=(0, -0.05, 1.7))
    head = bpy.context.active_object
    head.name = "Head"
    head.data.materials.append(skin)

    # Hair (golden, slightly larger sphere on top of head)
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.24, location=(0, -0.10, 1.75))
    hair_obj = bpy.context.active_object
    hair_obj.name = "Hair"
    hair_obj.scale = (1, 1, 0.8)
    bpy.ops.object.transform_apply(scale=True)
    hair_obj.data.materials.append(hair)

    # Legs (folded in front, 2 cylinders)
    for i, x in enumerate([-0.15, 0.15]):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.10, depth=0.6,
                                            location=(x, -0.3, 0.55),
                                            rotation=(math.radians(25), 0, 0))
        leg = bpy.context.active_object
        leg.name = f"Leg{i}"
        leg.data.materials.append(dress)

    # Arms resting on armrests
    for i, x in enumerate([-0.45, 0.45]):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.07, depth=0.5,
                                            location=(x, -0.1, 0.95))
        arm = bpy.context.active_object
        arm.name = f"Arm{i}"
        arm.data.materials.append(skin)

    # Join everything
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = seat
    bpy.ops.object.join()
    bpy.context.active_object.name = "AnastasiaRocker"

    export_fbx("AnastasiaRocker")
    print("[gen_anastasia_rocker] DONE")


if __name__ == "__main__":
    main()
