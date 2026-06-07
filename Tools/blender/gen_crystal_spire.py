"""
R66 — Bake a real CrystalSpire FBX for Moon 1 hero building #3.
Per docs/15 §3 ("CrystalSpire — luminous tower of stacked aether crystals").
The current scene placeholder is 3 dark pillars — replace with cluster of
6 angled crystal spikes around central column + glowing emissive cores.

Output: Assets/_Project/Models/Blender/Moon1/CrystalSpire.fbx
Auto-imports + URP/Lit converts + spawns Prefab Variant.
"""
import bpy, math, os, sys

# Stand-alone fallback paths (Blender headless invocation)
HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
try:
    from _common import reset_scene, make_material, export_fbx
except Exception:
    print("[gen_crystal_spire] FAIL: _common missing")
    sys.exit(1)


def main():
    reset_scene()

    # ── Material set ──
    base_stone = make_material("CrystalSpireBase",
                               base_color=(0.35, 0.32, 0.28, 1.0),
                               roughness=0.7, metallic=0.0)
    crystal_blue = make_material("CrystalShardBlue",
                                  base_color=(0.30, 0.50, 0.85, 0.9),
                                  roughness=0.15, metallic=0.1,
                                  emission=(0.20, 0.45, 0.95),
                                  emission_strength=2.2)
    crystal_white = make_material("CrystalShardWhite",
                                   base_color=(0.85, 0.90, 1.0, 0.9),
                                   roughness=0.10, metallic=0.05,
                                   emission=(0.95, 0.95, 1.0),
                                   emission_strength=3.0)

    # ── Stone base (octagonal pad) ──
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=2.2, depth=0.6, location=(0, 0, 0.3))
    base = bpy.context.active_object
    base.name = "CrystalSpire_Base"
    base.data.materials.append(base_stone)

    # Stepped riser
    bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=1.5, depth=0.5, location=(0, 0, 0.85))
    riser = bpy.context.active_object
    riser.name = "CrystalSpire_Riser"
    riser.data.materials.append(base_stone)

    # ── Central column ──
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.45, depth=4.5, location=(0, 0, 3.0))
    column = bpy.context.active_object
    column.name = "CrystalSpire_Column"
    column.data.materials.append(crystal_blue)

    # ── Central tip ──
    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=0.45, radius2=0.0, depth=1.5, location=(0, 0, 6.0))
    tip = bpy.context.active_object
    tip.name = "CrystalSpire_Tip"
    tip.data.materials.append(crystal_white)

    # ── 6 angled crystal shards around column ──
    for i in range(6):
        angle = i * (math.pi / 3)
        x = math.cos(angle) * 0.85
        y = math.sin(angle) * 0.85
        # Each shard tilted outward + slightly varied height
        shard_h = 2.5 + (i % 3) * 0.5
        # Crystal shard as an elongated cone with slight tilt
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.18, radius2=0.0,
                                        depth=shard_h, location=(x, y, 1.5 + shard_h * 0.5))
        shard = bpy.context.active_object
        shard.name = f"CrystalSpire_Shard{i}"
        # Tilt outward
        shard.rotation_euler = (math.radians(12) * math.sin(angle),
                                math.radians(12) * -math.cos(angle), 0)
        shard.data.materials.append(crystal_blue if i % 2 == 0 else crystal_white)

    # ── Floor crystal cluster at base ──
    for i in range(4):
        angle = i * (math.pi / 2) + math.pi / 4
        x = math.cos(angle) * 1.7
        y = math.sin(angle) * 1.7
        bpy.ops.mesh.primitive_cone_add(vertices=5, radius1=0.12, radius2=0.0,
                                        depth=0.8, location=(x, y, 1.0))
        floor_c = bpy.context.active_object
        floor_c.name = f"CrystalSpire_FloorCrystal{i}"
        floor_c.data.materials.append(crystal_blue)

    # ── Join all into single mesh ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = base
    bpy.ops.object.join()
    bpy.context.active_object.name = "CrystalSpire"

    export_fbx("CrystalSpire")
    print("[gen_crystal_spire] DONE — wrote CrystalSpire.fbx")


if __name__ == "__main__":
    main()
