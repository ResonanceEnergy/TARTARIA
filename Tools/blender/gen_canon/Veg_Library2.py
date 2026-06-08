"""
Veg_Library2 — 6 MORE vegetation variants per R171.

Adds: Mushroom_Cluster, BarrelCactus, FallenLog, CrystalPillar, ThornBush, IvyVine
Total Blender vegetation library now 20 variants.
"""
import os, sys, math, bpy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb,
    make_character_mat, make_aether_emissive,
)
from _lib_canon import OUT_DIR


def join_export(parts, name):
    for o in parts: o.select_set(False)
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    fbx = os.path.join(OUT_DIR, f"{name}.fbx")
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=fbx, use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_NONE",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True,
        object_types={"MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", path_mode="COPY", embed_textures=False,
    )
    print(f"[Veg2_{name}] verts={len(o.data.vertices)} -> {fbx}")


def bake_mushroom_cluster():
    reset_scene()
    parts = []
    cap_mat = make_aether_emissive("Mushroom_Cap", (0.85, 0.35, 0.45, 1.0), 1.5)
    stem_mat = make_character_mat("Mushroom_Stem", (0.92, 0.88, 0.82, 1.0))
    spot_mat = make_character_mat("Mushroom_Spot", (0.95, 0.92, 0.85, 1.0))
    for i, (x, y, z, scale) in enumerate([(0, 0, 0, 1.0), (0.25, 0, 0.18, 0.7), (-0.2, 0, 0.15, 0.6), (0.05, 0, -0.22, 0.85)]):
        stem = cylinder_y(f"Stem_{i}", (x, 0.12 * scale, z), 0.04 * scale, 0.24 * scale)
        stem.data.materials.append(stem_mat)
        parts.append(stem)
        cap = uv_orb(f"Cap_{i}", (x, 0.28 * scale, z), 0.14 * scale)
        cap.scale = (1.2, 0.55, 1.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        cap.data.materials.append(cap_mat)
        parts.append(cap)
        for j in range(3):
            sx_off = 0.05 * math.cos(j * 2.0)
            sz_off = 0.05 * math.sin(j * 2.0)
            spot = uv_orb(f"Spot_{i}_{j}", (x + sx_off * scale, 0.32 * scale, z + sz_off * scale), 0.025 * scale)
            spot.data.materials.append(spot_mat)
            parts.append(spot)
    join_export(parts, "Veg_MushroomCluster")


def bake_barrel_cactus():
    reset_scene()
    parts = []
    body = uv_orb("Body", (0, 0.35, 0), 0.30)
    body.scale = (1.0, 1.4, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    body.data.materials.append(make_character_mat("Cactus_Body", (0.30, 0.45, 0.25, 1.0)))
    parts.append(body)
    # Spines
    spine_mat = make_character_mat("Cactus_Spine", (0.85, 0.80, 0.65, 1.0))
    for i in range(16):
        ang = math.radians(i * 22.5)
        for ring_y in [0.20, 0.40, 0.55]:
            sx = 0.30 * math.cos(ang)
            sz = 0.30 * math.sin(ang)
            sp = cube_at(f"Spine_{i}_{ring_y}", (sx, ring_y, sz), (0.015, 0.05, 0.015))
            sp.data.materials.append(spine_mat)
            parts.append(sp)
    # Flower on top
    flower = uv_orb("Flower", (0, 0.78, 0), 0.10)
    flower.data.materials.append(make_aether_emissive("Cactus_Flower", (1.0, 0.6, 0.7, 1.0), 2.5))
    parts.append(flower)
    join_export(parts, "Veg_BarrelCactus")


def bake_fallen_log():
    reset_scene()
    parts = []
    # Log lying on side
    log = cylinder_y("Log", (0, 0.25, 0), 0.30, 2.0)
    log.rotation_euler = (0, 0, math.radians(90))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    log.data.materials.append(make_character_mat("Log_Bark", (0.32, 0.20, 0.12, 1.0)))
    parts.append(log)
    # Moss patches on top
    moss_mat = make_aether_emissive("Log_Moss", (0.30, 0.55, 0.32, 1.0), 1.0)
    for i, x in enumerate([-0.6, 0, 0.5]):
        moss = ico_orb(f"Moss_{i}", (x, 0.50, 0), 0.18)
        moss.scale = (1.4, 0.3, 1.4)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        moss.data.materials.append(moss_mat)
        parts.append(moss)
    # Stub end (cut end visible)
    end = cylinder_y("EndCut", (1.0, 0.25, 0), 0.30, 0.05)
    end.rotation_euler = (0, 0, math.radians(90))
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    end.data.materials.append(make_character_mat("Log_End", (0.65, 0.50, 0.30, 1.0)))
    parts.append(end)
    join_export(parts, "Veg_FallenLog")


def bake_crystal_pillar():
    reset_scene()
    parts = []
    # 3 tall crystal spikes
    cyan = make_aether_emissive("Pillar_Crystal", (0.45, 0.80, 1.0, 1.0), 4.0)
    sizes = [(0, 0, 1.0), (0.10, -0.05, 0.7), (-0.08, 0.05, 0.6)]
    for i, (x, z, h) in enumerate(sizes):
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.10, radius2=0, depth=h, location=(x, h / 2, z))
        c = bpy.context.active_object
        c.name = f"Crystal_{i}"
        c.rotation_euler = (math.radians(5 * (1 if i % 2 == 0 else -1)), 0, math.radians(3 * i))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        c.data.materials.append(cyan)
        parts.append(c)
    # Base rock
    base = ico_orb("Base", (0, 0.06, 0), 0.20)
    base.scale = (1.5, 0.4, 1.5)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    base.data.materials.append(make_character_mat("Pillar_Rock", (0.42, 0.40, 0.38, 1.0)))
    parts.append(base)
    join_export(parts, "Veg_CrystalPillar")


def bake_thorn_bush():
    reset_scene()
    parts = []
    bush_mat = make_character_mat("Thorn_Body", (0.32, 0.28, 0.22, 1.0))
    # 5 random ico spheres bunched
    import random
    random.seed(192)
    for i in range(5):
        x = random.uniform(-0.25, 0.25)
        y = random.uniform(0.20, 0.55)
        z = random.uniform(-0.25, 0.25)
        r = random.uniform(0.18, 0.28)
        b = ico_orb(f"Bush_{i}", (x, y, z), r)
        b.scale = (1.0, 0.7, 1.0)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        b.data.materials.append(bush_mat)
        parts.append(b)
    # Thorn spikes
    thorn_mat = make_character_mat("Thorn_Spike", (0.18, 0.14, 0.10, 1.0))
    for i in range(8):
        ang = math.radians(i * 45)
        sx = 0.30 * math.cos(ang)
        sz = 0.30 * math.sin(ang)
        sp = cube_at(f"Thorn_{i}", (sx, 0.45, sz), (0.02, 0.20, 0.02))
        sp.rotation_euler = (math.radians(20), 0, math.radians(15 * (1 if i % 2 == 0 else -1)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        sp.data.materials.append(thorn_mat)
        parts.append(sp)
    # Red berry
    berry = uv_orb("Berry", (0, 0.55, 0), 0.06)
    berry.data.materials.append(make_aether_emissive("Thorn_Berry", (0.85, 0.20, 0.20, 1.0), 3.0))
    parts.append(berry)
    join_export(parts, "Veg_ThornBush")


def bake_ivy_vine():
    reset_scene()
    parts = []
    leaf_mat = make_character_mat("Ivy_Leaf", (0.22, 0.45, 0.25, 1.0))
    stem_mat = make_character_mat("Ivy_Stem", (0.28, 0.30, 0.18, 1.0))
    # Trailing vine — series of segments going up at angle
    for i in range(8):
        y = i * 0.30 + 0.15
        x = math.sin(i * 0.5) * 0.15
        seg = cylinder_y(f"Stem_{i}", (x, y, 0), 0.018, 0.30)
        seg.rotation_euler = (0, 0, math.radians(15 * math.sin(i * 0.8)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        seg.data.materials.append(stem_mat)
        parts.append(seg)
        # 2 leaves per segment
        for j, side in enumerate([-1, 1]):
            leaf = cube_at(f"Leaf_{i}_{j}", (x + side * 0.10, y, 0.05), (0.08, 0.02, 0.05))
            leaf.rotation_euler = (math.radians(30 * side), math.radians(30 * j), math.radians(20 * side))
            bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
            leaf.data.materials.append(leaf_mat)
            parts.append(leaf)
    join_export(parts, "Veg_IvyVine")


def main():
    print("[Veg_Library2] Baking 6 more vegetation variants")
    bake_mushroom_cluster()
    bake_barrel_cactus()
    bake_fallen_log()
    bake_crystal_pillar()
    bake_thorn_bush()
    bake_ivy_vine()
    print("[Veg_Library2] ALL 6 BAKED. Total library 20 variants.")


if __name__ == "__main__":
    main()
