"""
R68 — 3-6-9 Tesla lore stone for Moon 1 Day 1-5 first prophecy fragment.
Per docs/15 + CLAUDE.md lore: standing menhir with 3-6-9 numerals carved in,
golden glow indicates undiscovered prophecy fragment.

Output: Assets/_Project/Models/Blender/Moon1/LoreStone369.fbx
"""
import bpy, math, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
from _common import reset_scene, make_material, export_fbx


def main():
    reset_scene()

    granite = make_material("Granite",
                             base_color=(0.42, 0.40, 0.36, 1.0),
                             roughness=0.85, metallic=0.0)
    gold_glow = make_material("ProphecyGold",
                               base_color=(1.0, 0.78, 0.25, 1.0),
                               roughness=0.4, metallic=0.6,
                               emission=(1.0, 0.72, 0.20),
                               emission_strength=3.5)

    # ── Menhir (standing stone) — rough hexagonal column ──
    bpy.ops.mesh.primitive_cylinder_add(vertices=6, radius=0.5, depth=2.4,
                                        location=(0, 0, 1.2))
    menhir = bpy.context.active_object
    menhir.name = "Menhir"
    # Slight tilt
    menhir.rotation_euler = (math.radians(5), 0, 0)
    menhir.data.materials.append(granite)

    # Add a bevel modifier for chipped stone look
    bevel = menhir.modifiers.new(name="Bevel", type='BEVEL')
    bevel.width = 0.08
    bevel.segments = 2
    bpy.ops.object.modifier_apply(modifier="Bevel")

    # ── Rounded top cap ──
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.5, location=(0, 0, 2.45))
    cap = bpy.context.active_object
    cap.name = "Cap"
    cap.scale = (1, 1, 0.4)
    cap.data.materials.append(granite)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # ── 3 glowing rings carved into the menhir face (Tesla's 3-6-9) ──
    # Use small toruses pressed into front face
    for i, (z, r) in enumerate([(2.0, 0.06), (1.5, 0.10), (1.0, 0.14)]):
        bpy.ops.mesh.primitive_torus_add(major_radius=r, minor_radius=0.015,
                                          location=(0, -0.45, z),
                                          rotation=(math.pi / 2, 0, 0))
        ring = bpy.context.active_object
        ring.name = f"GlyphRing{i+1}"
        ring.data.materials.append(gold_glow)

    # ── Base pedestal (slate slab) ──
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.75, depth=0.15,
                                        location=(0, 0, 0.075))
    pedestal = bpy.context.active_object
    pedestal.name = "Pedestal"
    pedestal.data.materials.append(granite)

    # ── Glow orb above stone (interact indicator) ──
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.12, location=(0, 0, 3.0))
    orb = bpy.context.active_object
    orb.name = "ProphecyOrb"
    orb.data.materials.append(gold_glow)

    # ── Join all ──
    bpy.ops.object.select_all(action='SELECT')
    bpy.context.view_layer.objects.active = menhir
    bpy.ops.object.join()
    bpy.context.active_object.name = "LoreStone369"

    export_fbx("LoreStone369")
    print("[gen_lore_stone_369] DONE")


if __name__ == "__main__":
    main()
