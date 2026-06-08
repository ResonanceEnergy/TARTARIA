"""
_lib_characters.py — Blender helpers for canon Moon 1 characters.

Style per docs/32 Art Bible:
- Matte stylized humanoid silhouettes (Hollow Knight + Tunic readability)
- Distinct ~2m tall canonical proportions
- Aether-colored materials per character lore role
- Gold seam emissive at key joints / aether points
- Low poly (200-600 verts each) for stylized silhouette reading
"""
import bpy, os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_canon import (
    reset_scene, make_matte_stone, make_aether_emissive, save_and_export,
    set_pivot_bottom_center, shade_smooth,
    PHI, AETHER_GOLD, AETHER_CYAN, AETHER_VIOLET, WARM_STONE, COOL_STONE,
)


def make_character_mat(name, base_color, roughness=0.7, metallic=0.0):
    """Character body material — slight sheen vs matte stone."""
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
    bsdf.inputs["Metallic"].default_value = metallic
    return mat


def ico_orb(name, loc, radius, subdiv=2):
    """Smooth orb (ico-sphere)."""
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=subdiv, radius=radius, location=loc)
    o = bpy.context.active_object
    o.name = name
    shade_smooth(o)
    return o


def uv_orb(name, loc, radius, segments=12, ring_count=8):
    """UV sphere — for heads, eyes, joints."""
    bpy.ops.mesh.primitive_uv_sphere_add(
        segments=segments, ring_count=ring_count, radius=radius, location=loc,
    )
    o = bpy.context.active_object
    o.name = name
    shade_smooth(o)
    return o


def cylinder_y(name, loc, radius, height):
    """Vertical cylinder along Blender Y (matches our convention)."""
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=10, radius=radius, depth=height, location=loc,
        rotation=(math.radians(90), 0, 0),
    )
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def cube_at(name, loc, size):
    """Cube of specified XYZ size at loc."""
    bpy.ops.mesh.primitive_cube_add(location=loc, size=2.0)
    o = bpy.context.active_object
    o.name = name
    o.scale = (size[0] / 2, size[1] / 2, size[2] / 2)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return o


def join_character(parts, final_name):
    """Join all parts + set pivot at feet (bottom center)."""
    for o in parts:
        o.select_set(False)
    for o in parts:
        o.select_set(True)
    bpy.context.view_layer.objects.active = parts[0]
    bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = final_name
    set_pivot_bottom_center(joined)
    return joined
