"""
Cottage_B_v2.py — Smaller cottage variant (5m x 3.5m x 4m), plaster walls + slate roof.

Variant differences from A:
  - Wall material: painted_plaster_wall (white plaster instead of stone)
  - Smaller footprint (5x3.5m vs 6x4m)
  - Lower roof rise (1.2m vs 1.5m)
  - 1 front door + 2 small windows (no side window)
  - Same roof_slates_03 + black_painted_planks for door
"""
import bpy, os, sys, math
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(_HERE))
from _common import PROJECT_ROOT, reset_scene

ASSET_NEW = os.path.join(PROJECT_ROOT, "NEW ASSETS MAY 2626")
TEX_PLASTER = os.path.join(ASSET_NEW, "painted_plaster_wall_4k.blend", "textures")
TEX_ROOF = os.path.join(ASSET_NEW, "roof_slates_03_4k.blend", "textures")
TEX_WOOD = os.path.join(ASSET_NEW, "black_painted_planks_4k.blend", "textures")

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Buildings", "Blender_v2")
os.makedirs(OUT_DIR, exist_ok=True)
OUT_FBX = os.path.join(OUT_DIR, "Cottage_B.fbx")
OUT_BLEND = os.path.join(OUT_DIR, "Cottage_B.blend")

W, D, H_WALL, H_ROOF = 5.0, 3.5, 2.8, 1.2
WALL_THICK = 0.25
DOOR_W, DOOR_H = 0.9, 1.9
WIN_W, WIN_H, WIN_Y = 0.7, 0.7, 1.5


def make_polyhaven_material(name, tex_dir, base_name):
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


def add_cube(name, loc, scale):
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def bool_diff(target, cutter):
    m = target.modifiers.new(name="Bool", type="BOOLEAN")
    m.operation = "DIFFERENCE"
    m.object = cutter
    bpy.context.view_layer.objects.active = target
    bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.data.objects.remove(cutter, do_unlink=True)


def bevel_obj(obj, width=0.04, segments=2):
    m = obj.modifiers.new(name="Bevel", type="BEVEL")
    m.width = width
    m.segments = segments
    m.limit_method = "ANGLE"
    m.angle_limit = math.radians(30)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=m.name)


def set_pivot_bottom_center(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bbox = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    min_y = min(v.y for v in bbox)
    avg_x = sum(v.x for v in bbox) / 8
    avg_z = sum(v.z for v in bbox) / 8
    bpy.context.scene.cursor.location = (avg_x, min_y, avg_z)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    obj.location = (0, 0, 0)


def smart_uv(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")


def build():
    reset_scene()
    walls = add_cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    door = add_cube("DoorCut", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK))
    bool_diff(walls, door)
    for x_off in [-W/2+1.0, W/2-1.0]:
        w = add_cube("WinCut", (x_off, WIN_Y, D/2), (WIN_W/2, WIN_H/2, WALL_THICK))
        bool_diff(walls, w)
    bpy.context.view_layer.objects.active = walls
    mod = walls.modifiers.new(name="Solidify", type="SOLIDIFY")
    mod.thickness = -WALL_THICK
    mod.offset = 1.0
    bpy.ops.object.modifier_apply(modifier=mod.name)
    bevel_obj(walls, 0.03, 2)
    # Roof
    bpy.ops.mesh.primitive_cube_add(location=(0, H_WALL + H_ROOF/2, 0), size=2.0)
    roof = bpy.context.active_object
    roof.name = "Roof"
    roof.scale = (W/2 + 0.15, H_ROOF/2, D/2 + 0.15)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    import bmesh
    bpy.context.view_layer.objects.active = roof
    bpy.ops.object.mode_set(mode="EDIT")
    bm = bmesh.from_edit_mesh(roof.data)
    for v in bm.verts:
        if v.co.y > 0:
            v.co.x = 0
    bmesh.update_edit_mesh(roof.data)
    bpy.ops.object.mode_set(mode="OBJECT")
    bevel_obj(roof, 0.025, 1)
    # Door plane
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object
    door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    # Materials
    walls.data.materials.append(make_polyhaven_material("Cottage_B_Plaster", TEX_PLASTER, "painted_plaster_wall"))
    roof.data.materials.append(make_polyhaven_material("Cottage_B_RoofSlate", TEX_ROOF, "roof_slates_03"))
    door_p.data.materials.append(make_polyhaven_material("Cottage_B_DoorWood", TEX_WOOD, "black_painted_planks"))
    for o in (walls, roof, door_p):
        smart_uv(o)
        o.select_set(False)
    walls.select_set(True); roof.select_set(True); door_p.select_set(True)
    bpy.context.view_layer.objects.active = walls
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Cottage_B"
    set_pivot_bottom_center(o)
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
    print(f"[Cottage_B] DONE verts={len(c.data.vertices)} faces={len(c.data.polygons)}")


if __name__ == "__main__":
    main()
