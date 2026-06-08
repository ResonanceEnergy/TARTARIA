"""
Kit_ModularBuildings — the canonical 12-piece modular kit per CLAUDE.md R171.

Builds ALL 13 Moons' architecture via palette-swap. Authored ONCE in Blender.

1m snap grid. Pivots at corner for snap placement.

12 pieces:
  Wall pieces (3m tall × 1m wide × 0.3m thick, pivot bottom-left-back):
    1. wall_straight
    2. wall_corner
    3. wall_window
    4. wall_door
  Floor pieces (1m × 0.2m × 1m, pivot bottom-left-back):
    5. floor_square
    6. floor_edge
  Roof pieces (1m × 0.3m × 1m, pivot bottom-left-back):
    7. roof_flat
    8. roof_slope
  Vertical accents (pivot bottom-center):
    9. column (1.5m tall × 0.4m dia)
    10. arch (2m wide × 3m tall archway)
    11. stair (1m wide × 0.5m tall × 0.5m deep)
    12. capstone (decorative top, 1m × 0.3m × 1m)

Per R171: PBR matte stone (Roughness 0.85, Metallic 0) + Aether-Gold emissive seams.
"""
import os, sys, math, bpy
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, ico_orb,
    make_character_mat, make_aether_emissive, set_pivot_bottom_center,
    AETHER_GOLD,
)
from _lib_canon import OUT_DIR


def make_pbr_stone(name, base_color=(0.50, 0.47, 0.42, 1.0), roughness=0.85):
    """R171 locked: PBR matte stone, Roughness 0.85+, Metallic 0."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = base_color
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = 0.0
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.4
    return mat


def join_and_export(parts, name):
    """Join all parts + set pivot to (0,0,0) + export FBX."""
    for o in parts: o.select_set(False)
    for o in parts: o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = name
    # Set pivot at corner (0,0,0) for snap placement
    bpy.context.scene.cursor.location = (0, 0, 0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)
    fbx_path = os.path.join(OUT_DIR, f"{name}.fbx")
    bpy.ops.object.select_all(action="DESELECT")
    o.select_set(True)
    bpy.ops.export_scene.fbx(
        filepath=fbx_path, use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_NONE",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True,
        object_types={"MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", path_mode="COPY", embed_textures=False,
    )
    print(f"[Kit_{name}] DONE verts={len(o.data.vertices)} faces={len(o.data.polygons)} -> {fbx_path}")


# ──────── PIECE 1: wall_straight ────────
def bake_wall_straight():
    reset_scene()
    stone = make_pbr_stone("Kit_Stone")
    gold = make_aether_emissive("Kit_GoldSeam", AETHER_GOLD, 2.5)
    parts = []
    # Body: 1m wide, 3m tall, 0.3m thick. Centered at (0.5, 1.5, 0.15)
    body = cube_at("Body", (0.5, 1.5, 0.15), (1.0, 3.0, 0.3))
    body.data.materials.append(stone)
    parts.append(body)
    # Aether-Gold seam: 1 thin gold line at top
    seam = cube_at("TopSeam", (0.5, 2.92, 0.15), (1.0, 0.06, 0.32))
    seam.data.materials.append(gold)
    parts.append(seam)
    join_and_export(parts, "Kit_wall_straight")


# ──────── PIECE 2: wall_corner ────────
def bake_wall_corner():
    reset_scene()
    stone = make_pbr_stone("Kit_Stone")
    gold = make_aether_emissive("Kit_GoldSeam", AETHER_GOLD, 2.5)
    parts = []
    # L-shape: two walls meeting at 90°
    body_x = cube_at("BodyX", (0.5, 1.5, 0.15), (1.0, 3.0, 0.3))
    body_z = cube_at("BodyZ", (0.15, 1.5, 0.5), (0.3, 3.0, 1.0))
    for b in [body_x, body_z]:
        b.data.materials.append(stone)
        parts.append(b)
    # Corner buttress
    butt = cube_at("CornerButtress", (0.20, 1.5, 0.20), (0.4, 3.0, 0.4))
    butt.data.materials.append(stone)
    parts.append(butt)
    # Gold seam on top of buttress
    seam = cube_at("CornerSeam", (0.20, 2.92, 0.20), (0.45, 0.06, 0.45))
    seam.data.materials.append(gold)
    parts.append(seam)
    join_and_export(parts, "Kit_wall_corner")


# ──────── PIECE 3: wall_window ────────
def bake_wall_window():
    reset_scene()
    stone = make_pbr_stone("Kit_Stone")
    gold = make_aether_emissive("Kit_GoldSeam", AETHER_GOLD, 2.5)
    parts = []
    # Wall minus window cutout — make as 4 frame pieces
    # Top piece
    top = cube_at("Top", (0.5, 2.55, 0.15), (1.0, 0.9, 0.3))
    # Bottom piece
    bot = cube_at("Bottom", (0.5, 0.5, 0.15), (1.0, 1.0, 0.3))
    # Left jamb
    lj = cube_at("LJamb", (0.15, 1.55, 0.15), (0.3, 1.1, 0.3))
    # Right jamb
    rj = cube_at("RJamb", (0.85, 1.55, 0.15), (0.3, 1.1, 0.3))
    for b in [top, bot, lj, rj]:
        b.data.materials.append(stone)
        parts.append(b)
    # Window frame gold accent
    sill = cube_at("WindowSill", (0.5, 1.0, 0.30), (0.85, 0.07, 0.08))
    sill.data.materials.append(gold)
    parts.append(sill)
    # Top seam
    top_seam = cube_at("TopSeam", (0.5, 2.92, 0.15), (1.0, 0.06, 0.32))
    top_seam.data.materials.append(gold)
    parts.append(top_seam)
    join_and_export(parts, "Kit_wall_window")


# ──────── PIECE 4: wall_door ────────
def bake_wall_door():
    reset_scene()
    stone = make_pbr_stone("Kit_Stone")
    gold = make_aether_emissive("Kit_GoldSeam", AETHER_GOLD, 2.5)
    wood = make_pbr_stone("Kit_Door_Wood", (0.32, 0.20, 0.12, 1.0), 0.95)
    parts = []
    # Wall around door (3 pieces forming inverted U)
    top = cube_at("Top", (0.5, 2.65, 0.15), (1.0, 0.7, 0.3))
    lj = cube_at("LJamb", (0.15, 1.15, 0.15), (0.3, 2.3, 0.3))
    rj = cube_at("RJamb", (0.85, 1.15, 0.15), (0.3, 2.3, 0.3))
    for b in [top, lj, rj]:
        b.data.materials.append(stone)
        parts.append(b)
    # Wooden door slab
    door = cube_at("Door", (0.5, 1.10, 0.15), (0.55, 2.10, 0.06))
    door.data.materials.append(wood)
    parts.append(door)
    # Door arch seam
    arch_seam = cube_at("ArchSeam", (0.5, 2.32, 0.15), (0.7, 0.06, 0.32))
    arch_seam.data.materials.append(gold)
    parts.append(arch_seam)
    # Top seam
    top_seam = cube_at("TopSeam", (0.5, 2.92, 0.15), (1.0, 0.06, 0.32))
    top_seam.data.materials.append(gold)
    parts.append(top_seam)
    join_and_export(parts, "Kit_wall_door")


# ──────── PIECE 5: floor_square ────────
def bake_floor_square():
    reset_scene()
    stone = make_pbr_stone("Kit_Floor")
    parts = []
    floor = cube_at("Floor", (0.5, 0.10, 0.5), (1.0, 0.20, 1.0))
    floor.data.materials.append(stone)
    parts.append(floor)
    join_and_export(parts, "Kit_floor_square")


# ──────── PIECE 6: floor_edge ────────
def bake_floor_edge():
    reset_scene()
    stone = make_pbr_stone("Kit_Floor")
    gold = make_aether_emissive("Kit_FloorEdge_Gold", AETHER_GOLD, 2.0)
    parts = []
    floor = cube_at("Floor", (0.5, 0.10, 0.5), (1.0, 0.20, 1.0))
    floor.data.materials.append(stone)
    parts.append(floor)
    # Gold edge trim along the +Z edge (forward edge)
    edge = cube_at("EdgeTrim", (0.5, 0.18, 0.97), (1.0, 0.05, 0.05))
    edge.data.materials.append(gold)
    parts.append(edge)
    join_and_export(parts, "Kit_floor_edge")


# ──────── PIECE 7: roof_flat ────────
def bake_roof_flat():
    reset_scene()
    stone = make_pbr_stone("Kit_Roof", (0.42, 0.40, 0.36, 1.0))
    parts = []
    roof = cube_at("Roof", (0.5, 0.15, 0.5), (1.0, 0.30, 1.0))
    roof.data.materials.append(stone)
    parts.append(roof)
    join_and_export(parts, "Kit_roof_flat")


# ──────── PIECE 8: roof_slope ────────
def bake_roof_slope():
    reset_scene()
    stone = make_pbr_stone("Kit_Roof", (0.42, 0.40, 0.36, 1.0))
    parts = []
    # Sloped triangular roof — use scaled+rotated cube
    roof = cube_at("Roof", (0.5, 0.4, 0.5), (1.0, 0.8, 1.0))
    # Tilt 25° on X axis (slope down toward +Z)
    roof.rotation_euler = (math.radians(25), 0, 0)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    roof.data.materials.append(stone)
    parts.append(roof)
    join_and_export(parts, "Kit_roof_slope")


# ──────── PIECE 9: column ────────
def bake_column():
    reset_scene()
    stone = make_pbr_stone("Kit_Column")
    gold = make_aether_emissive("Kit_ColumnRing", AETHER_GOLD, 2.5)
    parts = []
    # Base disk
    base = cylinder_y("Base", (0, 0.08, 0), 0.25, 0.16)
    base.data.materials.append(stone)
    parts.append(base)
    # Shaft
    shaft = cylinder_y("Shaft", (0, 1.50, 0), 0.18, 2.80)
    shaft.data.materials.append(stone)
    parts.append(shaft)
    # Capital
    cap = cylinder_y("Capital", (0, 2.92, 0), 0.25, 0.20)
    cap.data.materials.append(stone)
    parts.append(cap)
    # Gold ring at 1m + 2m heights
    for y in [1.0, 2.0]:
        bpy.ops.mesh.primitive_torus_add(major_radius=0.20, minor_radius=0.025, major_segments=18, minor_segments=6, location=(0, y, 0))
        ring = bpy.context.active_object
        ring.name = f"Ring_{y}"
        ring.data.materials.append(gold)
        parts.append(ring)
    join_and_export(parts, "Kit_column")


# ──────── PIECE 10: arch ────────
def bake_arch():
    reset_scene()
    stone = make_pbr_stone("Kit_Arch")
    gold = make_aether_emissive("Kit_ArchKeystone", AETHER_GOLD, 3.0)
    parts = []
    # 2 pillars
    for sx in [0.2, 1.8]:
        p = cube_at(f"Pillar_{sx}", (sx, 1.0, 0.15), (0.4, 2.0, 0.3))
        p.data.materials.append(stone)
        parts.append(p)
    # Lintel (horizontal beam)
    lintel = cube_at("Lintel", (1.0, 2.20, 0.15), (2.0, 0.4, 0.4))
    lintel.data.materials.append(stone)
    parts.append(lintel)
    # Arched top piece (cylinder half)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=0.6, depth=0.4, location=(1.0, 2.60, 0.15), rotation=(math.radians(90), 0, 0))
    arch_top = bpy.context.active_object
    arch_top.name = "ArchTop"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=False)
    arch_top.data.materials.append(stone)
    parts.append(arch_top)
    # Keystone gold accent
    key = uv_orb("Keystone", (1.0, 2.95, 0.18), 0.15)
    key.data.materials.append(gold)
    parts.append(key)
    join_and_export(parts, "Kit_arch")


# ──────── PIECE 11: stair ────────
def bake_stair():
    reset_scene()
    stone = make_pbr_stone("Kit_Stair")
    gold = make_aether_emissive("Kit_StairNose", AETHER_GOLD, 2.0)
    parts = []
    # 3 steps rising +Y, +Z
    for i in range(3):
        y = 0.075 + i * 0.15
        z = 0.5 - i * 0.15
        step = cube_at(f"Step_{i}", (0.5, y, z), (1.0, 0.15, 0.30))
        step.data.materials.append(stone)
        parts.append(step)
        # Gold nosing at front of each step
        nose = cube_at(f"Nose_{i}", (0.5, y + 0.07, z - 0.13), (1.0, 0.025, 0.02))
        nose.data.materials.append(gold)
        parts.append(nose)
    join_and_export(parts, "Kit_stair")


# ──────── PIECE 12: capstone ────────
def bake_capstone():
    reset_scene()
    stone = make_pbr_stone("Kit_Capstone")
    gold = make_aether_emissive("Kit_CapstoneOrb", AETHER_GOLD, 4.0)
    parts = []
    # Wide flat base
    base = cube_at("Base", (0.5, 0.10, 0.5), (1.0, 0.20, 1.0))
    base.data.materials.append(stone)
    parts.append(base)
    # Pyramidal top
    pyramid = cube_at("Pyramid", (0.5, 0.45, 0.5), (0.7, 0.50, 0.7))
    pyramid.scale = (1.0, 1.0, 1.0)
    parts.append(pyramid)
    pyramid.data.materials.append(stone)
    # Orb on top
    orb = uv_orb("CapOrb", (0.5, 0.85, 0.5), 0.18)
    orb.data.materials.append(gold)
    parts.append(orb)
    join_and_export(parts, "Kit_capstone")


def main():
    print("[Kit_ModularBuildings] Authoring 12-piece kit per R171 unify mandate")
    bake_wall_straight()
    bake_wall_corner()
    bake_wall_window()
    bake_wall_door()
    bake_floor_square()
    bake_floor_edge()
    bake_roof_flat()
    bake_roof_slope()
    bake_column()
    bake_arch()
    bake_stair()
    bake_capstone()
    print("[Kit_ModularBuildings] ALL 12 PIECES BAKED")


if __name__ == "__main__":
    main()
