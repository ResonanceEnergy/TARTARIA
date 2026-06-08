"""
Veg_Library — 14 canonical vegetation variants per R171 no-purchases directive.

Replaces would-have-been Quaternius Nature Pack. All matte PBR per R171 lock.
Authored ONCE, recolor per biome via material variants for all 13 Moons.

Plants authored:
  Trees (3): AetherPine, TwistedOak, DeadTartarianTree
  Shrubs (3): CrystalShrub, AetherFern, MossClumpRock
  Ground (3): WildGrassTuft, GlowMossPatch, LilyPadCluster
  Tall (3): ReedCluster, Cattail, SkybloomFlower
  Hero (2): MemoryRoseTrio, VineTendril
"""
import os, sys, math, bpy, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb,
    make_character_mat, make_aether_emissive,
)
from _lib_canon import OUT_DIR
from mathutils import Vector


def make_pbr(name, color, rough=0.85, metal=0.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes): nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = rough
    bsdf.inputs["Metallic"].default_value = metal
    return mat


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
    print(f"[Veg_{name}] verts={len(o.data.vertices)} -> {fbx}")


# 1. AETHER PINE — cyan-tipped tall conifer 4m
def bake_aether_pine():
    reset_scene()
    parts = []
    trunk = cylinder_y("Trunk", (0, 2.0, 0), 0.18, 4.0)
    trunk.data.materials.append(make_pbr("Pine_Trunk", (0.30, 0.20, 0.14, 1.0)))
    parts.append(trunk)
    # Conifer cone layers
    for i, (y, r) in enumerate([(3.5, 1.2), (4.2, 0.9), (4.7, 0.6), (5.1, 0.35)]):
        bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=r, radius2=0, depth=0.8, location=(0, y, 0))
        c = bpy.context.active_object
        c.name = f"Cone_{i}"
        c.data.materials.append(make_pbr("Pine_Needles", (0.22, 0.38, 0.28, 1.0)))
        parts.append(c)
    # Cyan tip orb
    tip = uv_orb("CyanTip", (0, 5.3, 0), 0.10)
    tip.data.materials.append(make_aether_emissive("Pine_TipCyan", (0.55, 0.85, 1.0, 1.0), 3.0))
    parts.append(tip)
    join_export(parts, "Veg_AetherPine")


# 2. TWISTED OAK — gold-leaf oak 3m
def bake_twisted_oak():
    reset_scene()
    parts = []
    trunk = cylinder_y("Trunk", (0, 1.0, 0), 0.20, 2.0)
    trunk.data.materials.append(make_pbr("Oak_Trunk", (0.35, 0.22, 0.14, 1.0)))
    parts.append(trunk)
    # 3 branches splaying
    for i in range(3):
        ang = math.radians(i * 120)
        bx = 0.4 * math.cos(ang)
        bz = 0.4 * math.sin(ang)
        branch = cylinder_y(f"Branch_{i}", (bx, 1.9, bz), 0.08, 0.8)
        branch.rotation_euler = (0, -ang, math.radians(35))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        branch.data.materials.append(make_pbr("Oak_Trunk", (0.35, 0.22, 0.14, 1.0)))
        parts.append(branch)
    # Canopy: 4 ico-sphere clumps
    canopy_mat = make_pbr("Oak_Leaves", (0.45, 0.42, 0.25, 1.0), 0.85)
    for i, (x, y, z, r) in enumerate([(0, 2.6, 0, 0.7), (0.5, 2.4, 0.2, 0.45), (-0.4, 2.5, -0.3, 0.5), (0, 2.9, 0, 0.45)]):
        c = ico_orb(f"Clump_{i}", (x, y, z), r)
        c.data.materials.append(canopy_mat)
        parts.append(c)
    # 5 gold leaf accents
    gold = make_aether_emissive("Oak_GoldLeaf", (1.0, 0.85, 0.45, 1.0), 3.0)
    for i in range(5):
        ang = math.radians(i * 72)
        gx = 0.65 * math.cos(ang)
        gz = 0.65 * math.sin(ang)
        gl = uv_orb(f"GoldLeaf_{i}", (gx, 2.7 + (i % 2) * 0.15, gz), 0.07)
        gl.data.materials.append(gold)
        parts.append(gl)
    join_export(parts, "Veg_TwistedOak")


# 3. DEAD TARTARIAN TREE — petrified tree with gold glyph 3m
def bake_dead_tartarian_tree():
    reset_scene()
    parts = []
    trunk = cylinder_y("Trunk", (0, 1.4, 0), 0.16, 2.8)
    trunk.data.materials.append(make_pbr("DeadTree_Petrified", (0.55, 0.52, 0.48, 1.0)))
    parts.append(trunk)
    # 3 bare branches
    for sx in [-0.3, 0.35, 0]:
        sz = 0 if sx != 0 else 0.3
        b = cylinder_y(f"Branch_{sx}_{sz}", (sx, 2.4, sz), 0.06, 0.8)
        b.rotation_euler = (math.radians(30) * (1 if sz > 0 else 0), 0, math.radians(45) * (1 if sx > 0 else -1 if sx < 0 else 0))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        b.data.materials.append(make_pbr("DeadTree_Petrified", (0.55, 0.52, 0.48, 1.0)))
        parts.append(b)
    # Gold glyph at base
    glyph = cube_at("Glyph", (0, 0.40, 0.18), (0.18, 0.30, 0.02))
    glyph.data.materials.append(make_aether_emissive("DeadTree_Glyph", (1.0, 0.85, 0.45, 1.0), 4.0))
    parts.append(glyph)
    join_export(parts, "Veg_DeadTartarianTree")


# 4. CRYSTAL SHRUB — small spiky cyan crystal cluster 0.6m
def bake_crystal_shrub():
    reset_scene()
    parts = []
    base = ico_orb("Base", (0, 0.10, 0), 0.20)
    base.scale = (1.0, 0.4, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    base.data.materials.append(make_pbr("CrystalShrub_Stone", (0.45, 0.42, 0.40, 1.0)))
    parts.append(base)
    # 6 crystal spikes
    cy = make_aether_emissive("CrystalShrub_Cyan", (0.55, 0.85, 1.0, 1.0), 3.5)
    for i in range(6):
        ang = math.radians(i * 60)
        x = 0.10 * math.cos(ang)
        z = 0.10 * math.sin(ang)
        bpy.ops.mesh.primitive_cone_add(vertices=6, radius1=0.04, radius2=0, depth=0.4 + (i % 2) * 0.15, location=(x, 0.30, z))
        c = bpy.context.active_object
        c.rotation_euler = (math.radians(15 * (1 if i % 2 else -1)), math.radians(i * 30), 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        c.data.materials.append(cy)
        parts.append(c)
    join_export(parts, "Veg_CrystalShrub")


# 5. AETHER FERN — fanned green-cyan fronds 0.8m
def bake_aether_fern():
    reset_scene()
    parts = []
    mat_frond = make_pbr("Fern_Frond", (0.25, 0.42, 0.28, 1.0))
    # 7 radial fronds
    for i in range(7):
        ang = math.radians(i * (360 / 7))
        x = 0.20 * math.cos(ang)
        z = 0.20 * math.sin(ang)
        frond = cube_at(f"Frond_{i}", (x, 0.35, z), (0.04, 0.65, 0.16))
        frond.rotation_euler = (math.radians(-25), -ang, math.radians(10 * (1 if i % 2 else -1)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        frond.data.materials.append(mat_frond)
        parts.append(frond)
    # Center cyan glow
    glow = uv_orb("FernGlow", (0, 0.15, 0), 0.07)
    glow.data.materials.append(make_aether_emissive("Fern_Glow", (0.55, 0.85, 1.0, 1.0), 2.0))
    parts.append(glow)
    join_export(parts, "Veg_AetherFern")


# 6. MOSS CLUMP ROCK — mossy boulder 0.6m
def bake_moss_rock():
    reset_scene()
    parts = []
    rock = ico_orb("Rock", (0, 0.30, 0), 0.40)
    rock.scale = (1.2, 0.8, 1.0)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    rock.data.materials.append(make_pbr("Rock_Stone", (0.45, 0.42, 0.40, 1.0)))
    parts.append(rock)
    # 3 moss patches on top
    moss_mat = make_pbr("Moss_Green", (0.22, 0.40, 0.18, 1.0))
    for x, z in [(0.0, 0.10), (-0.15, -0.10), (0.18, -0.08)]:
        m = ico_orb(f"Moss_{x}_{z}", (x, 0.58, z), 0.10)
        m.scale = (1.2, 0.4, 1.2)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        m.data.materials.append(moss_mat)
        parts.append(m)
    join_export(parts, "Veg_MossRock")


# 7. WILD GRASS TUFT — clump of grass blades 0.4m
def bake_grass_tuft():
    reset_scene()
    parts = []
    mat = make_pbr("Grass_Green", (0.42, 0.55, 0.22, 1.0))
    random.seed(7)
    for i in range(8):
        x = random.uniform(-0.12, 0.12)
        z = random.uniform(-0.12, 0.12)
        h = random.uniform(0.20, 0.40)
        blade = cube_at(f"Blade_{i}", (x, h / 2, z), (0.015, h, 0.03))
        blade.rotation_euler = (math.radians(random.uniform(-15, 15)), math.radians(random.uniform(0, 360)), math.radians(random.uniform(-15, 15)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        blade.data.materials.append(mat)
        parts.append(blade)
    join_export(parts, "Veg_GrassTuft")


# 8. GLOW MOSS PATCH — flat emissive ground patch 1m
def bake_glow_moss():
    reset_scene()
    parts = []
    patch = cube_at("Patch", (0, 0.025, 0), (1.0, 0.05, 1.0))
    patch.data.materials.append(make_aether_emissive("GlowMoss", (0.42, 0.78, 0.45, 1.0), 1.8))
    parts.append(patch)
    # 6 small dots
    for i in range(6):
        ang = math.radians(i * 60)
        x = 0.30 * math.cos(ang)
        z = 0.30 * math.sin(ang)
        d = uv_orb(f"Dot_{i}", (x, 0.08, z), 0.04)
        d.data.materials.append(make_aether_emissive("GlowMoss_Dot", (1.0, 0.85, 0.45, 1.0), 4.0))
        parts.append(d)
    join_export(parts, "Veg_GlowMossPatch")


# 9. LILY PAD CLUSTER — 4 floating lily pads 0.8m
def bake_lily_pads():
    reset_scene()
    parts = []
    mat_pad = make_pbr("LilyPad_Green", (0.30, 0.50, 0.25, 1.0))
    mat_flower = make_aether_emissive("LilyFlower_Cyan", (0.85, 0.92, 1.0, 1.0), 3.0)
    for i, (x, z, r) in enumerate([(0, 0, 0.30), (0.40, 0.15, 0.25), (-0.30, 0.30, 0.22), (0.15, -0.35, 0.20)]):
        bpy.ops.mesh.primitive_cylinder_add(vertices=8, radius=r, depth=0.04, location=(x, 0.02, z))
        pad = bpy.context.active_object
        pad.name = f"Pad_{i}"
        pad.data.materials.append(mat_pad)
        parts.append(pad)
        if i == 0:  # central flower
            f = uv_orb("Flower", (x, 0.10, z), 0.10)
            f.data.materials.append(mat_flower)
            parts.append(f)
    join_export(parts, "Veg_LilyPadCluster")


# 10. REED CLUSTER — tall thin reeds near water 1.2m
def bake_reed_cluster():
    reset_scene()
    parts = []
    mat = make_pbr("Reed_Stem", (0.50, 0.45, 0.20, 1.0))
    random.seed(10)
    for i in range(7):
        x = random.uniform(-0.15, 0.15)
        z = random.uniform(-0.15, 0.15)
        h = random.uniform(0.80, 1.30)
        r = cylinder_y(f"Reed_{i}", (x, h / 2, z), 0.012, h)
        r.rotation_euler = (math.radians(random.uniform(-5, 5)), 0, math.radians(random.uniform(-5, 5)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        r.data.materials.append(mat)
        parts.append(r)
    join_export(parts, "Veg_ReedCluster")


# 11. CATTAIL — single cattail with brown puff 1.0m
def bake_cattail():
    reset_scene()
    parts = []
    stem = cylinder_y("Stem", (0, 0.55, 0), 0.015, 1.10)
    stem.data.materials.append(make_pbr("Cattail_Stem", (0.45, 0.50, 0.25, 1.0)))
    parts.append(stem)
    # Brown puff at top
    puff = cylinder_y("Puff", (0, 1.08, 0), 0.05, 0.15)
    puff.data.materials.append(make_pbr("Cattail_Puff", (0.35, 0.22, 0.12, 1.0)))
    parts.append(puff)
    join_export(parts, "Veg_Cattail")


# 12. SKYBLOOM FLOWER — tall pink hero flower 1.0m
def bake_skybloom():
    reset_scene()
    parts = []
    stem = cylinder_y("Stem", (0, 0.45, 0), 0.018, 0.90)
    stem.data.materials.append(make_pbr("Skybloom_Stem", (0.22, 0.40, 0.22, 1.0)))
    parts.append(stem)
    # 2 leaves
    for sx in [-0.05, 0.05]:
        leaf = cube_at(f"Leaf_{sx}", (sx, 0.45, 0), (0.10, 0.025, 0.06))
        leaf.rotation_euler = (0, math.radians(40 if sx > 0 else -40), math.radians(25 if sx > 0 else -25))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        leaf.data.materials.append(make_pbr("Skybloom_Leaf", (0.22, 0.40, 0.22, 1.0)))
        parts.append(leaf)
    # Flower head - 5 pink petals
    petal_mat = make_aether_emissive("Skybloom_Petal", (1.0, 0.7, 0.85, 1.0), 1.5)
    for i in range(5):
        ang = math.radians(i * 72)
        px = 0.10 * math.cos(ang)
        pz = 0.10 * math.sin(ang)
        p = uv_orb(f"Petal_{i}", (px, 0.95, pz), 0.06)
        p.data.materials.append(petal_mat)
        parts.append(p)
    # Center
    c = uv_orb("Pollen", (0, 0.98, 0), 0.04)
    c.data.materials.append(make_aether_emissive("Skybloom_Pollen", (1.0, 0.85, 0.45, 1.0), 4.0))
    parts.append(c)
    join_export(parts, "Veg_SkybloomFlower")


# 13. MEMORY ROSE TRIO — 3 small white roses on stalks 0.4m
def bake_memory_rose():
    reset_scene()
    parts = []
    stem_mat = make_pbr("Rose_Stem", (0.22, 0.38, 0.20, 1.0))
    rose_mat = make_pbr("Rose_Petal", (0.92, 0.90, 0.85, 1.0))
    pollen_mat = make_aether_emissive("Rose_Pollen", (1.0, 0.85, 0.45, 1.0), 3.0)
    for i, (x, z, h) in enumerate([(0, 0, 0.40), (0.10, 0.06, 0.32), (-0.08, -0.05, 0.36)]):
        stem = cylinder_y(f"Stem_{i}", (x, h / 2, z), 0.010, h)
        stem.data.materials.append(stem_mat)
        parts.append(stem)
        # Bloom
        bloom = ico_orb(f"Bloom_{i}", (x, h, z), 0.05)
        bloom.data.materials.append(rose_mat)
        parts.append(bloom)
        # Pollen dot
        pol = uv_orb(f"Pollen_{i}", (x, h, z), 0.018)
        pol.data.materials.append(pollen_mat)
        parts.append(pol)
    join_export(parts, "Veg_MemoryRoseTrio")


# 14. VINE TENDRIL — climbing vine 1.5m
def bake_vine():
    reset_scene()
    parts = []
    mat = make_pbr("Vine_Green", (0.30, 0.45, 0.20, 1.0))
    # 5-segment curving vine
    for i in range(5):
        y = i * 0.30 + 0.15
        x = math.sin(i * 0.6) * 0.20
        seg = cylinder_y(f"VineSeg_{i}", (x, y, 0), 0.025, 0.30)
        seg.rotation_euler = (0, 0, math.radians(15 * math.sin(i * 0.8)))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        seg.data.materials.append(mat)
        parts.append(seg)
    # 3 leaves at top
    for i in range(3):
        ang = math.radians(i * 120)
        lx = 0.10 * math.cos(ang)
        lz = 0.10 * math.sin(ang)
        leaf = cube_at(f"Leaf_{i}", (lx, 1.40, lz), (0.08, 0.025, 0.10))
        leaf.rotation_euler = (0, -ang, math.radians(20))
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
        leaf.data.materials.append(make_pbr("Vine_Leaf", (0.32, 0.50, 0.22, 1.0)))
        parts.append(leaf)
    join_export(parts, "Veg_VineTendril")


def main():
    print("[Veg_Library] Baking 14 vegetation variants per R171")
    bake_aether_pine()
    bake_twisted_oak()
    bake_dead_tartarian_tree()
    bake_crystal_shrub()
    bake_aether_fern()
    bake_moss_rock()
    bake_grass_tuft()
    bake_glow_moss()
    bake_lily_pads()
    bake_reed_cluster()
    bake_cattail()
    bake_skybloom()
    bake_memory_rose()
    bake_vine()
    print("[Veg_Library] ALL 14 BAKED")


if __name__ == "__main__":
    main()
