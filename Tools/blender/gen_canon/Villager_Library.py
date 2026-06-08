"""
Villager_Library — 6 generic villager archetypes per R171 no-purchases directive.

Replaces would-have-been Mixamo + KayKit Adventurers. All authored in Blender,
~1.7m tall humans. Distinct silhouettes via outfit + proportions.

Archetypes:
1. Farmer (broad shoulders, straw hat, apron)
2. Merchant (heavy robe, beard, satchel)
3. Blacksmith (muscular, leather apron, hammer)
4. Healer (slim, hood, satchel)
5. Child (small 1.0m)
6. Elder (stooped, staff, long beard)
"""
import os, sys, math, bpy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb, join_character,
    make_character_mat, make_aether_emissive,
)
from _lib_canon import OUT_DIR


def export_villager(parts, name):
    for o in parts: o.select_set(False)
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    # Pivot at feet (bottom-center)
    from mathutils import Vector
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (
        sum(v.x for v in bbox) / 8,
        min(v.y for v in bbox),
        sum(v.z for v in bbox) / 8,
    )
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
    print(f"[Villager_{name}] verts={len(o.data.vertices)} -> {fbx}")


def base_humanoid(scale_y=1.0, broad=1.0):
    """Returns list of body parts for a base humanoid. scale_y for height, broad for width."""
    parts = []
    # Boots
    for sx in [-0.10, 0.10]:
        b = cube_at(f"Boot_{sx}", (sx, 0.08, 0.05), (0.16, 0.16, 0.30))
        parts.append(b)
    # Legs
    for sx in [-0.10, 0.10]:
        l = cylinder_y(f"Leg_{sx}", (sx, 0.50 * scale_y, 0), 0.08, 0.85 * scale_y)
        parts.append(l)
    # Pelvis
    p = cube_at("Pelvis", (0, 1.00 * scale_y, 0), (0.32 * broad, 0.20, 0.22))
    parts.append(p)
    # Torso
    t = cube_at("Torso", (0, 1.32 * scale_y, 0), (0.40 * broad, 0.50, 0.26))
    parts.append(t)
    # Shoulders
    sh = cube_at("Shoulders", (0, 1.55 * scale_y, 0), (0.50 * broad, 0.10, 0.26))
    parts.append(sh)
    # Arms
    for sx in [-0.27 * broad, 0.27 * broad]:
        ua = cylinder_y(f"UpperArm_{sx:.2f}", (sx, 1.30 * scale_y, 0), 0.07, 0.40)
        parts.append(ua)
        fa = cylinder_y(f"Forearm_{sx:.2f}", (sx, 0.90 * scale_y, 0.05), 0.06, 0.38)
        parts.append(fa)
        h = uv_orb(f"Hand_{sx:.2f}", (sx, 0.65 * scale_y, 0.05), 0.06)
        parts.append(h)
    # Neck
    n = cylinder_y("Neck", (0, 1.65 * scale_y, 0), 0.05, 0.10)
    parts.append(n)
    # Head
    head = uv_orb("Head", (0, 1.75 * scale_y, 0), 0.13)
    head.scale = (1.0, 1.1, 0.95)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(head)
    return parts


def apply_skin_to(parts, skin_mat):
    """Apply skin material to hands/head/neck (filtered by name)."""
    for p in parts:
        if p.name in ("Head", "Neck") or p.name.startswith("Hand_"):
            p.data.materials.append(skin_mat)


def apply_outfit_to(parts, outfit_mat, exclude_names=None):
    """Apply outfit material to torso/legs/pelvis/shoulders/arms (filtered by name)."""
    exclude = exclude_names or []
    for p in parts:
        if len(p.data.materials) > 0:
            continue
        if p.name in ("Torso", "Pelvis", "Shoulders") or p.name.startswith("Leg_") or p.name.startswith("UpperArm_") or p.name.startswith("Forearm_") or p.name.startswith("Boot_"):
            if p.name not in exclude:
                p.data.materials.append(outfit_mat)


# 1. FARMER — broad shoulders, straw hat, apron
def bake_farmer():
    reset_scene()
    parts = base_humanoid(scale_y=1.0, broad=1.15)
    skin = make_character_mat("Farmer_Skin", (0.85, 0.72, 0.58, 1.0), 0.65)
    shirt = make_character_mat("Farmer_Shirt", (0.62, 0.50, 0.32, 1.0), 0.80)
    apron = make_character_mat("Farmer_Apron", (0.45, 0.32, 0.22, 1.0), 0.90)
    hat = make_character_mat("Farmer_Hat", (0.78, 0.65, 0.30, 1.0), 0.95)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, shirt)
    # Apron over torso
    apron_part = cube_at("Apron", (0, 1.32, 0.14), (0.44, 0.55, 0.04))
    apron_part.data.materials.append(apron)
    parts.append(apron_part)
    # Straw hat — wide brim disc + small cone top
    brim = cylinder_y("HatBrim", (0, 1.90, 0), 0.32, 0.04)
    brim.data.materials.append(hat)
    parts.append(brim)
    cone = cube_at("HatTop", (0, 1.98, 0), (0.18, 0.10, 0.18))
    cone.data.materials.append(hat)
    parts.append(cone)
    export_villager(parts, "Villager_Farmer")


# 2. MERCHANT — robe, beard, satchel
def bake_merchant():
    reset_scene()
    parts = base_humanoid(scale_y=1.0, broad=1.05)
    skin = make_character_mat("Merchant_Skin", (0.82, 0.70, 0.55, 1.0), 0.65)
    robe = make_character_mat("Merchant_Robe", (0.32, 0.18, 0.18, 1.0), 0.85)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, robe)
    # Beard
    beard = cube_at("Beard", (0, 1.65, 0.10), (0.18, 0.12, 0.05))
    beard.data.materials.append(make_character_mat("Merchant_Beard", (0.35, 0.28, 0.22, 1.0), 0.95))
    parts.append(beard)
    # Belt
    belt = cube_at("Belt", (0, 1.06, 0), (0.42, 0.05, 0.28))
    belt.data.materials.append(make_character_mat("Merchant_Belt", (0.22, 0.14, 0.10, 1.0), 0.90))
    parts.append(belt)
    # Satchel at hip
    sat = cube_at("Satchel", (0.25, 0.95, 0.15), (0.18, 0.20, 0.08))
    sat.data.materials.append(make_character_mat("Merchant_Satchel", (0.42, 0.28, 0.18, 1.0), 0.95))
    parts.append(sat)
    # Gold coin pouch glint
    coin = uv_orb("CoinPouch", (-0.25, 1.0, 0.13), 0.05)
    coin.data.materials.append(make_aether_emissive("Merchant_Coin", (1.0, 0.85, 0.45, 1.0), 3.0))
    parts.append(coin)
    export_villager(parts, "Villager_Merchant")


# 3. BLACKSMITH — muscular, leather apron, hammer
def bake_blacksmith():
    reset_scene()
    parts = base_humanoid(scale_y=1.05, broad=1.25)
    skin = make_character_mat("Smith_Skin", (0.80, 0.62, 0.45, 1.0), 0.60)
    leather = make_character_mat("Smith_Leather", (0.32, 0.18, 0.10, 1.0), 0.90)
    metal = make_character_mat("Smith_Metal", (0.55, 0.50, 0.45, 1.0), 0.30, metallic=0.8)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, leather)
    # Leather apron front
    apron = cube_at("Apron", (0, 1.20, 0.16), (0.50, 0.65, 0.04))
    apron.data.materials.append(leather)
    parts.append(apron)
    # Hammer in right hand
    handle = cylinder_y("HammerHandle", (0.35, 0.55, 0.15), 0.025, 0.45)
    handle.data.materials.append(make_character_mat("Smith_Wood", (0.32, 0.22, 0.14, 1.0), 0.95))
    parts.append(handle)
    head = cube_at("HammerHead", (0.35, 0.35, 0.15), (0.12, 0.10, 0.08))
    head.data.materials.append(metal)
    parts.append(head)
    export_villager(parts, "Villager_Blacksmith")


# 4. HEALER — slim, hooded, satchel
def bake_healer():
    reset_scene()
    parts = base_humanoid(scale_y=1.0, broad=0.92)
    skin = make_character_mat("Healer_Skin", (0.88, 0.78, 0.70, 1.0), 0.65)
    robe = make_character_mat("Healer_Robe", (0.85, 0.85, 0.82, 1.0), 0.85)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, robe)
    # Hood
    hood = uv_orb("Hood", (0, 1.78, -0.05), 0.20)
    hood.scale = (1.05, 0.8, 1.10)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    hood.data.materials.append(robe)
    parts.append(hood)
    # Healing pendant (cyan)
    p = uv_orb("Pendant", (0, 1.40, 0.13), 0.06)
    p.data.materials.append(make_aether_emissive("Healer_Pendant", (0.55, 0.85, 1.0, 1.0), 4.0))
    parts.append(p)
    # Belt satchel
    sat = cube_at("Satchel", (-0.22, 0.95, 0.15), (0.18, 0.18, 0.08))
    sat.data.materials.append(make_character_mat("Healer_Satchel", (0.42, 0.32, 0.22, 1.0), 0.95))
    parts.append(sat)
    export_villager(parts, "Villager_Healer")


# 5. CHILD — small 1.0m
def bake_child():
    reset_scene()
    parts = base_humanoid(scale_y=0.58, broad=0.85)
    skin = make_character_mat("Child_Skin", (0.92, 0.80, 0.72, 1.0), 0.55)
    tunic = make_character_mat("Child_Tunic", (0.58, 0.48, 0.32, 1.0), 0.85)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, tunic)
    # Hair
    hair = uv_orb("Hair", (0, 1.05, -0.03), 0.16)
    hair.scale = (1.05, 0.85, 1.10)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    hair.data.materials.append(make_character_mat("Child_Hair", (0.42, 0.28, 0.18, 1.0), 0.85))
    parts.append(hair)
    export_villager(parts, "Villager_Child")


# 6. ELDER — stooped, staff, long beard
def bake_elder():
    reset_scene()
    parts = base_humanoid(scale_y=0.95, broad=0.92)
    skin = make_character_mat("Elder_Skin", (0.78, 0.70, 0.62, 1.0), 0.70)
    robe = make_character_mat("Elder_Robe", (0.32, 0.30, 0.32, 1.0), 0.90)
    apply_skin_to(parts, skin)
    apply_outfit_to(parts, robe)
    # Long white beard
    beard1 = cube_at("BeardUpper", (0, 1.60, 0.10), (0.20, 0.10, 0.05))
    beard2 = cube_at("BeardLower", (0, 1.43, 0.10), (0.16, 0.18, 0.05))
    for b in [beard1, beard2]:
        b.data.materials.append(make_character_mat("Elder_Beard", (0.85, 0.85, 0.82, 1.0), 0.90))
        parts.append(b)
    # Staff
    staff = cylinder_y("Staff", (0.30, 0.85, 0.05), 0.03, 1.70)
    staff.data.materials.append(make_character_mat("Elder_Staff", (0.42, 0.28, 0.18, 1.0), 0.95))
    parts.append(staff)
    # Glowing orb on top
    orb = uv_orb("StaffOrb", (0.30, 1.72, 0.05), 0.08)
    orb.data.materials.append(make_aether_emissive("Elder_StaffOrb", (1.0, 0.85, 0.45, 1.0), 4.5))
    parts.append(orb)
    export_villager(parts, "Villager_Elder")


def main():
    print("[Villager_Library] Baking 6 villager archetypes per R171")
    bake_farmer()
    bake_merchant()
    bake_blacksmith()
    bake_healer()
    bake_child()
    bake_elder()
    print("[Villager_Library] ALL 6 BAKED")


if __name__ == "__main__":
    main()
