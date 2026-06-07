"""
R98 — Real humanoid Player visual mesh.
Replaces the bronze-pumpkin Player_Limbs capsule with a stylized
medieval traveler: head + torso + arms + legs + cape.

Output: Assets/_Project/Models/Blender/Moon1/PlayerHumanoid.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    # ── Materials ──
    skin = make_material("PlayerSkin",
                          base_color=(0.95, 0.78, 0.65, 1.0),
                          roughness=0.6, metallic=0.0)
    tunic = make_material("PlayerTunic",
                           base_color=(0.45, 0.32, 0.22, 1.0),
                           roughness=0.85, metallic=0.0)
    leather = make_material("PlayerLeather",
                             base_color=(0.30, 0.20, 0.12, 1.0),
                             roughness=0.75, metallic=0.0)
    hair = make_material("PlayerHair",
                          base_color=(0.55, 0.32, 0.18, 1.0),
                          roughness=0.5, metallic=0.0)
    cape = make_material("PlayerCape",
                          base_color=(0.30, 0.45, 0.60, 1.0),  # blue cape
                          roughness=0.85, metallic=0.0)

    # ── Head (skin-tone sphere) ──
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.20, location=(0, 0, 1.75))
    head = bpy.context.active_object
    head.name = "Head"
    head.data.materials.append(skin)

    # ── Hair cap (slightly larger sphere on top) ──
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.22, location=(0, -0.02, 1.82))
    hair_obj = bpy.context.active_object
    hair_obj.name = "Hair"
    hair_obj.scale = (1, 1, 0.7)
    bpy.ops.object.transform_apply(scale=True)
    hair_obj.data.materials.append(hair)

    # ── Torso (elongated cube tunic) ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 1.15))
    torso = bpy.context.active_object
    torso.name = "Torso"
    torso.scale = (0.45, 0.30, 0.65)
    bpy.ops.object.transform_apply(scale=True)
    torso.data.materials.append(tunic)

    # ── Belt (thin leather band) ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0, 0.85))
    belt = bpy.context.active_object
    belt.name = "Belt"
    belt.scale = (0.48, 0.32, 0.06)
    bpy.ops.object.transform_apply(scale=True)
    belt.data.materials.append(leather)

    # ── Arms (2 cylinders) ──
    for i, x in enumerate([-0.32, 0.32]):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.08, depth=0.7, location=(x, 0, 1.15))
        arm = bpy.context.active_object
        arm.name = f"Arm{i}"
        arm.data.materials.append(tunic)
        # Hand (small sphere at bottom)
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.09, location=(x, 0, 0.80))
        hand = bpy.context.active_object
        hand.name = f"Hand{i}"
        hand.data.materials.append(skin)

    # ── Legs (2 cylinders) ──
    for i, x in enumerate([-0.13, 0.13]):
        bpy.ops.mesh.primitive_cylinder_add(radius=0.10, depth=0.85, location=(x, 0, 0.42))
        leg = bpy.context.active_object
        leg.name = f"Leg{i}"
        leg.data.materials.append(tunic)
        # Boot (small cube at foot)
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=(x, 0.06, 0.05))
        boot = bpy.context.active_object
        boot.name = f"Boot{i}"
        boot.scale = (0.13, 0.20, 0.07)
        bpy.ops.object.transform_apply(scale=True)
        boot.data.materials.append(leather)

    # ── Cape (back drape) ──
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=(0, 0.22, 1.10))
    cape_obj = bpy.context.active_object
    cape_obj.name = "Cape"
    cape_obj.scale = (0.42, 0.05, 0.8)
    bpy.ops.object.transform_apply(scale=True)
    cape_obj.rotation_euler = (math.radians(-5), 0, 0)
    cape_obj.data.materials.append(cape)

    # ── Join ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = torso
    bpy.ops.object.join()
    bpy.context.active_object.name = "PlayerHumanoid"

    export_fbx("PlayerHumanoid")
    print("[gen_player_humanoid] DONE")


if __name__ == "__main__":
    main()
