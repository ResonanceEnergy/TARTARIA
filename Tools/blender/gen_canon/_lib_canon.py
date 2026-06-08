"""
_lib_canon.py — Blender helpers for canon-aligned Moon 1 hero buildings.

Style law (docs/32_ART_BIBLE.md):
  - Matte stone (NO 4K PBR, NO normal/roughness maps)
  - Aether-Gold #FFD973 emissive seams
  - Sacred geometry: hexagonal/dodecagonal floor plans, φ=1.618 ratios, 3-6-9 element counts
  - Bloom threshold 1.1 — emissive seams should be HEROIC, not blown

Canon dimensions per docs/15_MVP_BUILD_SPEC.md §7:
  - Dome (Listeners' Hall): 25m dia × 18m height
  - Fountain (Thread of Memory): 8m basin × 5m column (8/5 = φ)
  - Spire (First Note): 3m base × 15m height

Pipeline (corrects gen_v2 mistakes):
  reset_scene()
  build with extrude/inset/bevel — NOT primitive Boolean stacks
  apply matte stone material (no PBR maps)
  add gold seam emissive material on key edges
  set pivot bottom-center
  save .blend + export .fbx (NO embed_textures — we use solid colors)
"""
import bpy, os, sys, math
from mathutils import Vector

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(_HERE))
from _common import PROJECT_ROOT, reset_scene  # noqa

OUT_DIR = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Buildings", "Blender_canon")
os.makedirs(OUT_DIR, exist_ok=True)

PHI = 1.6180339887

# Art Bible palette
AETHER_GOLD = (1.000, 0.851, 0.451, 1.0)      # #FFD973
AETHER_CYAN = (0.549, 0.851, 1.000, 1.0)      # #8CD9FF
AETHER_VIOLET = (0.851, 0.549, 1.000, 1.0)    # #D98CFF
WARM_STONE = (0.502, 0.475, 0.420, 1.0)       # warm neutral matte stone (within 3-hue + neutrals rule)
COOL_STONE = (0.451, 0.451, 0.475, 1.0)       # cool neutral matte stone


def make_matte_stone(name, base_color=WARM_STONE):
    """Matte stone — NO normal/roughness maps. Roughness 1.0. Sacred = matte."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = base_color
    bsdf.inputs["Roughness"].default_value = 1.0    # FULLY matte
    bsdf.inputs["Metallic"].default_value = 0.0
    if "Specular IOR Level" in bsdf.inputs:
        bsdf.inputs["Specular IOR Level"].default_value = 0.3  # low spec
    return mat


def make_aether_emissive(name, color=AETHER_GOLD, strength=3.0):
    """Glowing seam — Aether-Gold default, 3x strength for bloom-threshold 1.1 break."""
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    nt = mat.node_tree
    for n in list(nt.nodes):
        nt.nodes.remove(n)
    out = nt.nodes.new("ShaderNodeOutputMaterial")
    bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
    nt.links.new(bsdf.outputs["BSDF"], out.inputs["Surface"])
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = 0.6
    if "Emission Color" in bsdf.inputs:
        bsdf.inputs["Emission Color"].default_value = color
        bsdf.inputs["Emission Strength"].default_value = strength
    elif "Emission" in bsdf.inputs:
        bsdf.inputs["Emission"].default_value = color
        bsdf.inputs["Emission Strength"].default_value = strength
    return mat


def hexagon(name, loc, radius, depth, vertices=6):
    """Hexagonal prism (sacred geometry 6-fold). Pre-rotated to vertical-Y in Blender.

    NOTE: Y-up convention used throughout building scripts. Loc Y = vertical.
    """
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=loc,
        rotation=(math.radians(90), 0, 0),
    )
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def dodecagon(name, loc, radius, depth):
    """12-sided prism (sacred geometry 12-fold). For dome drum."""
    return hexagon(name, loc, radius, depth, vertices=12)


def hemisphere(name, loc, radius, segments=24, ring_count=12):
    """Half-sphere (dome cap). UPPER half kept (Y > 0). Y-up convention.

    R154: keeps top half (v.co.y > 0) of a sphere centered at loc. Bottom-half
    (v.co.y < 0) is deleted so the dome opens downward.
    """
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=segments, ring_count=ring_count, radius=radius, location=loc,
    )
    o = bpy.context.active_object
    o.name = name
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT")
    import bmesh
    bm = bmesh.from_edit_mesh(o.data)
    bm.verts.ensure_lookup_table()
    cy = loc[1]
    for v in bm.verts:
        if v.co.y < cy - 0.001:
            v.select = True
        else:
            v.select = False
    bmesh.update_edit_mesh(o.data)
    bpy.ops.mesh.delete(type="VERT")
    bpy.ops.object.mode_set(mode="OBJECT")
    return o


def cone(name, loc, radius_bottom, radius_top, depth, vertices=8):
    """Tapered cone. Pre-rotated to vertical-Y in Blender."""
    bpy.ops.mesh.primitive_cone_add(
        vertices=vertices, radius1=radius_bottom, radius2=radius_top,
        depth=depth, location=loc, rotation=(math.radians(90), 0, 0),
    )
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def cube(name, loc, scale):
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def bevel(obj, width=0.04, segments=2, angle_deg=30):
    m = obj.modifiers.new(name="Bv", type="BEVEL")
    m.width = width
    m.segments = segments
    m.limit_method = "ANGLE"
    m.angle_limit = math.radians(angle_deg)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=m.name)


def solidify(obj, thickness):
    m = obj.modifiers.new(name="Sol", type="SOLIDIFY")
    m.thickness = thickness
    m.offset = 1.0
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_apply(modifier=m.name)


def boolean_diff(target, cutter, remove=True):
    m = target.modifiers.new(name="Bool", type="BOOLEAN")
    m.operation = "DIFFERENCE"
    m.object = cutter
    bpy.context.view_layer.objects.active = target
    bpy.ops.object.modifier_apply(modifier=m.name)
    if remove:
        bpy.data.objects.remove(cutter, do_unlink=True)


def shade_smooth(obj, angle_deg=30):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.shade_auto_smooth(angle=math.radians(angle_deg))


def set_pivot_bottom_center(obj):
    """R125 fix: pivot at floor, not mesh center. Critical."""
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


def join_and_finalize(objs, final_name, primary_mat_idx=0):
    """Join into single object + set pivot at bottom-center."""
    for o in objs:
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objs[primary_mat_idx]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = final_name
    set_pivot_bottom_center(joined)
    return joined


def save_and_export(obj, name):
    blend_path = os.path.join(OUT_DIR, f"{name}.blend")
    fbx_path = os.path.join(OUT_DIR, f"{name}.fbx")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    # bake_space_transform=True keeps correct scale (1m=1u in Unity).
    # The model lays on its side after import — we rotate -90X in Unity per R153b.
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
        embed_textures=False,
    )
    print(f"[{name}] DONE verts={len(obj.data.vertices)} faces={len(obj.data.polygons)} -> {fbx_path}")
