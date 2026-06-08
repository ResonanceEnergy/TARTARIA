"""
Cottage_A_v2.py — proof-of-concept REAL building model.

Spec: ~6m wide x 4m deep x 4.5m tall medieval cottage with:
  - Stone block wall (Polyhaven medieval_blocks_06_4k 4K PBR)
  - Pitched roof slate tiles (Polyhaven roof_slates_03_4k 4K PBR)
  - Wood door (Polyhaven black_painted_planks_4k 4K PBR)
  - 2 windows cut via Boolean
  - 1 door cut via Boolean
  - Beveled stone edges (real architecture look)
  - Solidified wall thickness (40cm)
  - Bottom-center pivot (so transform.position is at FLOOR not center)
  - Smart UV Project on all faces

Run headless:
  "C:\\Program Files\\Blender Foundation\\Blender 5.0\\blender.exe" --background --python gen_v2/Cottage_A_v2.py

Output:
  Assets/_Project/Models/Buildings/Blender_v2/Cottage_A.fbx
  Assets/_Project/Models/Buildings/Blender_v2/Cottage_A.blend
"""

import bpy, os, sys, math
from mathutils import Vector

# Make _common importable when running headless
_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(_HERE))
from _common import PROJECT_ROOT, reset_scene

# ── Paths ──────────────────────────────────────────────────────────────────
ASSET_NEW = os.path.join(PROJECT_ROOT, "NEW ASSETS MAY 2626")
TEX_STONE = os.path.join(ASSET_NEW, "medieval_blocks_06_4k.blend")
TEX_ROOF = os.path.join(ASSET_NEW, "roof_slates_03_4k.blend")
TEX_WOOD = os.path.join(ASSET_NEW, "black_painted_planks_4k.blend")

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Buildings", "Blender_v2")
os.makedirs(OUT_DIR, exist_ok=True)
OUT_FBX = os.path.join(OUT_DIR, "Cottage_A.fbx")
OUT_BLEND = os.path.join(OUT_DIR, "Cottage_A.blend")

# ── Dimensions (meters — spec-aligned) ─────────────────────────────────────
W = 6.0   # width  (X)
D = 4.0   # depth  (Z)
H_WALL = 3.0   # wall height (Y)
H_ROOF = 1.5   # roof rise   (Y) — total height ~ 4.5m
WALL_THICK = 0.3
DOOR_W = 1.0
DOOR_H = 2.0
WIN_W = 0.8
WIN_H = 0.8
WIN_Y = 1.6   # window sill height


# ── Polyhaven material loader ──────────────────────────────────────────────
def make_polyhaven_material(name, tex_dir, base_name):
    """
    Create a Principled BSDF material wired to Polyhaven PBR maps.
    Expects files: {base}_diff_4k.jpg, {base}_nor_gl_4k.exr, {base}_rough_4k.jpg
    """
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)

    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])

    diff_path = os.path.join(tex_dir, f"{base_name}_diff_4k.jpg")
    norm_path = os.path.join(tex_dir, f"{base_name}_nor_gl_4k.exr")
    rough_path = os.path.join(tex_dir, f"{base_name}_rough_4k.jpg")

    # BaseColor / Albedo
    if os.path.isfile(diff_path):
        img_node = nt.nodes.new("ShaderNodeTexImage")
        img_node.image = bpy.data.images.load(diff_path, check_existing=True)
        img_node.image.colorspace_settings.name = "sRGB"
        nt.links.new(img_node.outputs["Color"], bsdf.inputs["Base Color"])

    # Normal (OpenGL convention from Polyhaven `_nor_gl_`)
    if os.path.isfile(norm_path):
        img_n = nt.nodes.new("ShaderNodeTexImage")
        img_n.image = bpy.data.images.load(norm_path, check_existing=True)
        img_n.image.colorspace_settings.name = "Non-Color"
        normal_map = nt.nodes.new("ShaderNodeNormalMap")
        nt.links.new(img_n.outputs["Color"], normal_map.inputs["Color"])
        nt.links.new(normal_map.outputs["Normal"], bsdf.inputs["Normal"])

    # Roughness
    if os.path.isfile(rough_path):
        img_r = nt.nodes.new("ShaderNodeTexImage")
        img_r.image = bpy.data.images.load(rough_path, check_existing=True)
        img_r.image.colorspace_settings.name = "Non-Color"
        nt.links.new(img_r.outputs["Color"], bsdf.inputs["Roughness"])
    else:
        bsdf.inputs["Roughness"].default_value = 0.7

    return mat


# ── Modeling helpers ───────────────────────────────────────────────────────
def add_cube_named(name, location, scale):
    bpy.ops.mesh.primitive_cube_add(location=location, size=2.0)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return obj


def boolean_diff(target, cutter):
    """Boolean DIFFERENCE: target -= cutter. Removes cutter."""
    mod = target.modifiers.new(name="BoolCut", type="BOOLEAN")
    mod.operation = "DIFFERENCE"
    mod.object = cutter
    bpy.context.view_layer.objects.active = target
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.data.objects.remove(cutter, do_unlink=True)


def bevel_object(obj, width=0.05, segments=2):
    mod = obj.modifiers.new(name="Bevel", type="BEVEL")
    mod.width = width
    mod.segments = segments
    mod.limit_method = "ANGLE"
    mod.angle_limit = math.radians(30)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=mod.name)


def set_origin_to_bottom_center(obj):
    """Move pivot to floor-center so transform.position is the FLOOR location.
    Critical for the R125 problem: kit pivots at mesh center = buildings half-buried.
    """
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    # Move 3D cursor to bottom-center of the bounding box
    bbox = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    min_y = min(v.y for v in bbox)
    avg_x = sum(v.x for v in bbox) / 8
    avg_z = sum(v.z for v in bbox) / 8
    bpy.context.scene.cursor.location = (avg_x, min_y, avg_z)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    # Move object so its origin is at world (0,0,0)
    obj.location = (0, 0, 0)


def smart_uv_unwrap(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")


# ── Build the cottage ──────────────────────────────────────────────────────
def build_cottage():
    reset_scene()
    print(f"[Cottage_A_v2] Building at OUT_FBX={OUT_FBX}")

    # 1. Wall box (hollow inside via solidify after boolean cuts)
    walls = add_cube_named("Cottage_Walls", (0, H_WALL / 2, 0), (W / 2, H_WALL / 2, D / 2))

    # 2. Cut DOOR (front face)
    door_cutter = add_cube_named(
        "DoorCutter",
        (0, DOOR_H / 2, D / 2),
        (DOOR_W / 2, DOOR_H / 2, WALL_THICK)
    )
    boolean_diff(walls, door_cutter)

    # 3. Cut 2 WINDOWS (front face, flanking door)
    for x_off in [-W / 2 + 1.3, W / 2 - 1.3]:
        win = add_cube_named(
            "WindowCutter",
            (x_off, WIN_Y, D / 2),
            (WIN_W / 2, WIN_H / 2, WALL_THICK)
        )
        boolean_diff(walls, win)

    # 4. Cut 1 WINDOW on left side
    side_win = add_cube_named(
        "SideWindowCutter",
        (-W / 2, WIN_Y, 0),
        (WALL_THICK, WIN_H / 2, WIN_W / 2)
    )
    boolean_diff(walls, side_win)

    # 5. Solidify walls (so they have thickness, not paper)
    bpy.context.view_layer.objects.active = walls
    mod = walls.modifiers.new(name="Solidify", type="SOLIDIFY")
    mod.thickness = -WALL_THICK
    mod.offset = 1.0
    bpy.ops.object.modifier_apply(modifier=mod.name)

    # 6. Bevel stone edges (architectural feel)
    bevel_object(walls, width=0.04, segments=2)

    # 7. Pitched ROOF — extrude a triangular prism along width
    bpy.ops.mesh.primitive_cube_add(
        location=(0, H_WALL + H_ROOF / 2, 0),
        size=2.0
    )
    roof = bpy.context.active_object
    roof.name = "Cottage_Roof"
    roof.scale = (W / 2 + 0.2, H_ROOF / 2, D / 2 + 0.2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # Make it a triangular prism by moving top edge vertices toward center
    import bmesh
    bpy.context.view_layer.objects.active = roof
    bpy.ops.object.mode_set(mode="EDIT")
    bm = bmesh.from_edit_mesh(roof.data)
    for v in bm.verts:
        if v.co.y > 0:  # top vertices
            v.co.x = 0  # collapse to ridge
    bmesh.update_edit_mesh(roof.data)
    bpy.ops.object.mode_set(mode="OBJECT")
    bevel_object(roof, width=0.03, segments=1)

    # 8. DOOR plane (visible wood door inside the cut opening)
    bpy.ops.mesh.primitive_plane_add(
        location=(0, DOOR_H / 2, D / 2 - WALL_THICK / 2),
        size=1.0,
        rotation=(math.radians(90), 0, 0)
    )
    door = bpy.context.active_object
    door.name = "Cottage_Door"
    door.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)

    # 9. Assign materials
    mat_stone = make_polyhaven_material("Cottage_Stone", TEX_STONE, "medieval_blocks_06")
    mat_roof = make_polyhaven_material("Cottage_RoofSlate", TEX_ROOF, "roof_slates_03")
    mat_wood = make_polyhaven_material("Cottage_DoorWood", TEX_WOOD, "black_painted_planks")
    walls.data.materials.append(mat_stone)
    roof.data.materials.append(mat_roof)
    door.data.materials.append(mat_wood)

    # 10. Smart UV unwrap each part
    for o in (walls, roof, door):
        smart_uv_unwrap(o)

    # 11. Join into single object for clean FBX export
    for o in (walls, roof, door):
        o.select_set(False)
    walls.select_set(True)
    roof.select_set(True)
    door.select_set(True)
    bpy.context.view_layer.objects.active = walls
    bpy.ops.object.join()
    cottage = bpy.context.active_object
    cottage.name = "Cottage_A"

    # 12. Set origin to bottom center (the R125 fix — pivot at floor)
    set_origin_to_bottom_center(cottage)

    return cottage


def main():
    cottage = build_cottage()

    # Save .blend
    bpy.ops.wm.save_as_mainfile(filepath=OUT_BLEND)
    print(f"[Cottage_A_v2] Saved .blend: {OUT_BLEND}")

    # Export FBX with Unity-friendly axes
    bpy.ops.object.select_all(action="DESELECT")
    cottage.select_set(True)
    bpy.context.view_layer.objects.active = cottage
    bpy.ops.export_scene.fbx(
        filepath=OUT_FBX,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_NONE",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        use_mesh_modifiers_render=True,
        mesh_smooth_type="OFF",
        use_subsurf=False,
        use_mesh_edges=False,
        use_tspace=True,  # tangents for proper normal map
        use_custom_props=False,
        path_mode="COPY",
        embed_textures=True,
    )
    print(f"[Cottage_A_v2] Exported FBX: {OUT_FBX}")
    print(f"[Cottage_A_v2] DONE. Verts: {len(cottage.data.vertices)}, Faces: {len(cottage.data.polygons)}")


if __name__ == "__main__":
    main()
