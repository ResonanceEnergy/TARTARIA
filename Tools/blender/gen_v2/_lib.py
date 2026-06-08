"""
_lib.py — Shared Blender building helpers for gen_v2/*.

Pattern (real modeling, NOT primitive_cube_add stacks):
    reset_scene()
    walls = cube(...)
    bool_diff(walls, cube_cutter)
    bevel(walls); solidify(walls)
    apply_material(walls, polyhaven_mat)
    smart_uv(walls)
    join + set_pivot
    save_and_export()
"""
import bpy, os, sys, math
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(_HERE))
from _common import PROJECT_ROOT, reset_scene  # noqa

ASSET_NEW = os.path.join(PROJECT_ROOT, "NEW ASSETS MAY 2626")
OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Buildings", "Blender_v2")
os.makedirs(OUT_DIR, exist_ok=True)


def texdir(set_name):
    """Resolve Polyhaven texture folder for a set."""
    return os.path.join(ASSET_NEW, f"{set_name}_4k.blend", "textures")


def make_polyhaven_material(name, set_name):
    """Create a Principled BSDF wired to Polyhaven {set}_diff/nor_gl/rough."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    td = texdir(set_name)
    diff = os.path.join(td, f"{set_name}_diff_4k.jpg")
    norm = os.path.join(td, f"{set_name}_nor_gl_4k.exr")
    rough_jpg = os.path.join(td, f"{set_name}_rough_4k.jpg")
    rough_exr = os.path.join(td, f"{set_name}_rough_4k.exr")
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
    """Add a cube primitive at loc with scale, apply transform, return obj."""
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def cyl(name, loc, radius, height, verts=24):
    """Vertical cylinder along Y."""
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=height, location=loc, rotation=(math.radians(90), 0, 0))
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def cone(name, loc, radius, height, verts=24):
    """Vertical cone tapering up (for spire-tops). Apex points +Y."""
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=radius, radius2=0, depth=height, location=loc, rotation=(math.radians(90), 0, 0))
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def bool_diff(target, cutter):
    """Boolean DIFFERENCE: target -= cutter; cutter is removed."""
    m = target.modifiers.new(name="Bool", type="BOOLEAN")
    m.operation = "DIFFERENCE"
    m.object = cutter
    bpy.context.view_layer.objects.active = target
    bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.data.objects.remove(cutter, do_unlink=True)


def bool_union(target, other):
    """Boolean UNION: target += other; other is removed."""
    m = target.modifiers.new(name="Bool", type="BOOLEAN")
    m.operation = "UNION"
    m.object = other
    bpy.context.view_layer.objects.active = target
    bpy.ops.object.modifier_apply(modifier=m.name)
    bpy.data.objects.remove(other, do_unlink=True)


def bevel(obj, width=0.04, segments=2):
    m = obj.modifiers.new(name="Bv", type="BEVEL")
    m.width = width
    m.segments = segments
    m.limit_method = "ANGLE"
    m.angle_limit = math.radians(30)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=m.name)


def solidify(obj, thickness=0.3):
    """Add inward wall thickness (negative direction)."""
    m = obj.modifiers.new(name="Sol", type="SOLIDIFY")
    m.thickness = -thickness
    m.offset = 1.0
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=m.name)


def pitched_roof(name, loc, w, d, h):
    """Make a pitched cube → triangular prism by collapsing top edge to ridge."""
    import bmesh
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object
    o.name = name
    o.scale = (w/2, h/2, d/2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT")
    bm = bmesh.from_edit_mesh(o.data)
    for v in bm.verts:
        if v.co.y > 0:
            v.co.x = 0
    bmesh.update_edit_mesh(o.data)
    bpy.ops.object.mode_set(mode="OBJECT")
    return o


def set_pivot_bottom_center(obj):
    """Move pivot to floor-center so transform.position = floor (NOT mesh center).
    Critical R125 fix: kit pivots at mesh center = half-buried.
    """
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bbox = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    bpy.context.scene.cursor.location = (
        sum(v.x for v in bbox) / 8,
        min(v.y for v in bbox),
        sum(v.z for v in bbox) / 8,
    )
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR", center="MEDIAN")
    obj.location = (0, 0, 0)


def smart_uv(obj):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="SELECT")
    bpy.ops.uv.smart_project(angle_limit=66.0, island_margin=0.02)
    bpy.ops.object.mode_set(mode="OBJECT")


def join_and_finalize(objs, final_name):
    """Smart-UV each, join into final_name, set pivot to bottom-center."""
    for o in objs:
        smart_uv(o)
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = final_name
    set_pivot_bottom_center(joined)
    return joined


def save_and_export(obj, name):
    """Save .blend and export FBX to OUT_DIR/{name}.{blend,fbx}."""
    blend_path = os.path.join(OUT_DIR, f"{name}.blend")
    fbx_path = os.path.join(OUT_DIR, f"{name}.fbx")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(
        filepath=fbx_path,
        use_selection=True,
        global_scale=1.0,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_NONE",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        object_types={"MESH"},
        use_mesh_modifiers=True,
        mesh_smooth_type="OFF",
        use_tspace=True,
        path_mode="COPY",
        embed_textures=True,
    )
    print(f"[{name}] DONE verts={len(obj.data.vertices)} faces={len(obj.data.polygons)} -> {fbx_path}")
