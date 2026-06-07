"""
R67 — Mercury-Ball Spire for Moon 1 Day 19-24 Buried Beacon landmark.
Per docs/03 + docs/15: a black spire topped with a mercury-silver orb that hums
during the 17th hour beat. Acts as a giant's hand reaching skyward.

Output: Assets/_Project/Models/Blender/Moon1/MercurySpire.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    # ── Materials ──
    obsidian = make_material("Obsidian",
                              base_color=(0.06, 0.06, 0.08, 1.0),
                              roughness=0.25, metallic=0.1)
    mercury = make_material("Mercury",
                             base_color=(0.85, 0.88, 0.92, 1.0),
                             roughness=0.05, metallic=1.0,
                             emission=(0.85, 0.90, 1.0),
                             emission_strength=1.8)
    bone = make_material("GiantBone",
                          base_color=(0.78, 0.72, 0.58, 1.0),
                          roughness=0.6, metallic=0.0)

    # ── Buried giant's hand fingers (5 angled bones) ──
    finger_lengths = [3.2, 4.0, 4.5, 4.0, 3.0]
    finger_angles_deg = [40, 25, 0, -25, -40]
    for i, (length, ang) in enumerate(zip(finger_lengths, finger_angles_deg)):
        a = math.radians(ang)
        # Bone position offset by spread
        x = math.sin(a) * 1.0
        z_base = 0.5
        # Bone (elongated cylinder)
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=0.18,
                                            depth=length, location=(x, 0, z_base + length * 0.5))
        bone_obj = bpy.context.active_object
        bone_obj.name = f"Finger{i}"
        bone_obj.rotation_euler = (0, a * 0.8, 0)
        bone_obj.data.materials.append(bone)
        # Knuckle joint
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.25, location=(x, 0, z_base))
        knuck = bpy.context.active_object
        knuck.name = f"Knuckle{i}"
        knuck.data.materials.append(bone)

    # ── Central black spire emerging from palm ──
    bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=0.5, radius2=0.15,
                                    depth=5.0, location=(0, 0, 5.5))
    spire = bpy.context.active_object
    spire.name = "Spire"
    spire.data.materials.append(obsidian)

    # Add tapering second segment
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.10,
                                        depth=2.5, location=(0, 0, 9.25))
    spire2 = bpy.context.active_object
    spire2.name = "SpireTip"
    spire2.data.materials.append(obsidian)

    # ── Mercury orb at apex ──
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.8, location=(0, 0, 11.0))
    orb = bpy.context.active_object
    orb.name = "MercuryOrb"
    orb.data.materials.append(mercury)

    # Smaller satellite orbs around the main (ritualistic)
    for i in range(3):
        ang = i * (2 * math.pi / 3)
        x = math.cos(ang) * 1.2
        y = math.sin(ang) * 1.2
        bpy.ops.mesh.primitive_uv_sphere_add(radius=0.25, location=(x, y, 11.3))
        sat = bpy.context.active_object
        sat.name = f"OrbSat{i}"
        sat.data.materials.append(mercury)

    # ── Join all into single mesh ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = spire
    bpy.ops.object.join()
    bpy.context.active_object.name = "MercurySpire"

    export_fbx("MercurySpire")
    print("[gen_mercury_spire] DONE — wrote MercurySpire.fbx")


if __name__ == "__main__":
    main()
