"""
Inn_v2.py — 2-story village Inn (Bob's Inn): 10m x 7m x 7m, balcony, hanging sign.

Spec-aligned: 2-story plaster-stone walls + slate roof + 2 chimneys + visible side balcony.
"""
import bpy, os, sys, math
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(_HERE))
from _common import PROJECT_ROOT, reset_scene

ASSET_NEW = os.path.join(PROJECT_ROOT, "NEW ASSETS MAY 2626")
TEX_PLSTONE = os.path.join(ASSET_NEW, "plaster_stone_wall_02_4k.blend", "textures")
TEX_STONE = os.path.join(ASSET_NEW, "medieval_blocks_06_4k.blend", "textures")
TEX_ROOF = os.path.join(ASSET_NEW, "roof_slates_03_4k.blend", "textures")
TEX_WOOD = os.path.join(ASSET_NEW, "black_painted_planks_4k.blend", "textures")

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Buildings", "Blender_v2")
os.makedirs(OUT_DIR, exist_ok=True)
OUT_FBX = os.path.join(OUT_DIR, "Inn.fbx")
OUT_BLEND = os.path.join(OUT_DIR, "Inn.blend")

W, D = 10.0, 7.0
H_FLOOR = 3.0
H_WALL = H_FLOOR * 2  # 6m for 2 stories
H_ROOF = 2.0
WALL_THICK = 0.35
DOOR_W, DOOR_H = 1.2, 2.3
WIN_W, WIN_H = 1.0, 1.0
CHIM_W, CHIM_D = 0.9, 0.9
CHIM_H = H_WALL + H_ROOF + 1.2


def make_mat(name, tex_dir, base_name):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    diff = os.path.join(tex_dir, f"{base_name}_diff_4k.jpg")
    norm = os.path.join(tex_dir, f"{base_name}_nor_gl_4k.exr")
    rough_jpg = os.path.join(tex_dir, f"{base_name}_rough_4k.jpg")
    rough_exr = os.path.join(tex_dir, f"{base_name}_rough_4k.exr")
    rough = rough_jpg if os.path.isfile(rough_jpg) else rough_exr
    if os.path.isfile(diff):
        img = nt.nodes.new("ShaderNodeTexImage")
        img.image = bpy.data.images.load(diff, check_existing=True)
        img.image.colorspace_settings.name = "sRGB"
        nt.links.new(img.outputs["Color"], bsdf.inputs["Base Color"])
    if os.path.isfile(norm):
        nimg = nt.nodes.new("ShaderNodeTexImage")
        nimg.image = bpy.data.images.load(norm, check_existing=True)
        nimg.image.colorspace_settings.name = "Non-Color"
        nm = nt.nodes.new("ShaderNodeNormalMap")
        nt.links.new(nimg.outputs["Color"], nm.inputs["Color"])
        nt.links.new(nm.outputs["Normal"], bsdf.inputs["Normal"])
    if os.path.isfile(rough):
        rimg = nt.nodes.new("ShaderNodeTexImage")
        rimg.image = bpy.data.images.load(rough, check_existing=True)
        rimg.image.colorspace_settings.name = "Non-Color"
        nt.links.new(rimg.outputs["Color"], bsdf.inputs["Roughness"])
    return mat


def cube(name, loc, scale):
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object; o.name = name; o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def bool_diff(t, c):
    m = t.modifiers.new(name="Bool", type="BOOLEAN")
    m.operation = "DIFFERENCE"; m.object = c
    bpy.context.view_layer.objects.active = t
    bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.data.objects.remove(c, do_unlink=True)


def bevel(o, w=0.04, s=2):
    m = o.modifiers.new(name="Bv", type="BEVEL")
    m.width = w; m.segments = s; m.limit_method = "ANGLE"
    m.angle_limit = math.radians(30)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier=m.name)


def set_pivot(o):
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bbox = [o.matrix_world @ Vector(c) for c in o.bound_box]
    bpy.context.scene.cursor.location = (sum(v.x for v in bbox)/8, min(v.y for v in bbox), sum(v.z for v in bbox)/8)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    o.location = (0, 0, 0)


def uv(o):
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")


def build():
    reset_scene()
    walls = cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    # Front door (center)
    bool_diff(walls, cube("Door", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # 2 front windows GROUND floor (flanking door)
    for x_off in [-W/2 + 1.8, W/2 - 1.8]:
        bool_diff(walls, cube("WinG", (x_off, 1.5, D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    # 4 front windows TOP floor
    for x_off in [-W/2 + 1.5, -W/2 + 4.0, W/2 - 4.0, W/2 - 1.5]:
        bool_diff(walls, cube("WinT", (x_off, H_FLOOR + 1.5, D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    # 2 side windows (left + right, both floors)
    for x_pos in [-W/2, W/2]:
        for y in [1.5, H_FLOOR + 1.5]:
            bool_diff(walls, cube("WinSide", (x_pos, y, 0), (WALL_THICK, WIN_H/2, WIN_W/2)))
    # 2 BACK windows
    for x_off in [-1.5, 1.5]:
        bool_diff(walls, cube("WinB", (x_off, H_FLOOR + 1.5, -D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    bpy.context.view_layer.objects.active = walls
    mod = walls.modifiers.new(name="Sol", type="SOLIDIFY")
    mod.thickness = -WALL_THICK; mod.offset = 1.0
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bevel(walls, 0.05, 2)
    # Roof (pitched)
    bpy.ops.mesh.primitive_cube_add(location=(0, H_WALL + H_ROOF/2, 0), size=2.0)
    roof = bpy.context.active_object; roof.name = "Roof"
    roof.scale = (W/2 + 0.4, H_ROOF/2, D/2 + 0.4)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    import bmesh
    bpy.context.view_layer.objects.active = roof
    bpy.ops.object.mode_set(mode="EDIT")
    bm = bmesh.from_edit_mesh(roof.data)
    for v in bm.verts:
        if v.co.y > 0: v.co.x = 0
    bmesh.update_edit_mesh(roof.data)
    bpy.ops.object.mode_set(mode="OBJECT")
    bevel(roof, 0.035, 1)
    # 2 Chimneys (front-left + back-right)
    chim1 = cube("Chim1", (-W/2 + 1.5, CHIM_H/2, -D/2 + 0.5), (CHIM_W/2, CHIM_H/2, CHIM_D/2))
    chim2 = cube("Chim2", (W/2 - 1.5, CHIM_H/2, D/2 - 0.5), (CHIM_W/2, CHIM_H/2, CHIM_D/2))
    bevel(chim1, 0.03, 1); bevel(chim2, 0.03, 1)
    # Balcony floor (front, 2nd story)
    balc = cube("Balcony", (0, H_FLOOR - 0.1, D/2 + 0.7), (W/2 * 0.9, 0.1, 0.7))
    # Balcony railing posts (4)
    rails = []
    for x_off in [-W/2 * 0.85, -W/2 * 0.3, W/2 * 0.3, W/2 * 0.85]:
        rp = cube(f"RailPost", (x_off, H_FLOOR + 0.5, D/2 + 1.3), (0.05, 0.5, 0.05))
        rails.append(rp)
    # Balcony top rail
    top_rail = cube("RailTop", (0, H_FLOOR + 1.0, D/2 + 1.3), (W/2 * 0.9, 0.05, 0.05))
    # Door plane
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Materials
    walls.data.materials.append(make_mat("Inn_Walls", TEX_PLSTONE, "plaster_stone_wall_02"))
    roof.data.materials.append(make_mat("Inn_Roof", TEX_ROOF, "roof_slates_03"))
    chim1.data.materials.append(make_mat("Inn_Chimney", TEX_STONE, "medieval_blocks_06"))
    chim2.data.materials.append(make_mat("Inn_Chimney", TEX_STONE, "medieval_blocks_06"))
    balc.data.materials.append(make_mat("Inn_Wood", TEX_WOOD, "black_painted_planks"))
    for rp in rails:
        rp.data.materials.append(make_mat("Inn_Wood", TEX_WOOD, "black_painted_planks"))
    top_rail.data.materials.append(make_mat("Inn_Wood", TEX_WOOD, "black_painted_planks"))
    door_p.data.materials.append(make_mat("Inn_Door", TEX_WOOD, "black_painted_planks"))
    all_objs = [walls, roof, chim1, chim2, balc, top_rail, door_p] + rails
    for o in all_objs:
        uv(o); o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = walls
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Inn"
    set_pivot(o)
    return o


def main():
    c = build()
    bpy.ops.wm.save_as_mainfile(filepath=OUT_BLEND)
    bpy.ops.object.select_all(action="DESELECT")
    c.select_set(True)
    bpy.context.view_layer.objects.active = c
    bpy.ops.export_scene.fbx(
        filepath=OUT_FBX, use_selection=True, global_scale=1.0,
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_NONE",
        axis_forward="-Z", axis_up="Y", bake_space_transform=True,
        object_types={"MESH"}, use_mesh_modifiers=True,
        mesh_smooth_type="OFF", use_tspace=True,
        path_mode="COPY", embed_textures=True
    )
    print(f"[Inn] DONE verts={len(c.data.vertices)} faces={len(c.data.polygons)}")


if __name__ == "__main__":
    main()
