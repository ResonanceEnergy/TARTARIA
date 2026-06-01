"""TARTARIA Blender helpers — cross-platform, multi-asset friendly."""
import bpy, os, sys, math

def _detect_project_root():
    env = os.environ.get("TARTARIA_ROOT")
    if env and os.path.isdir(env): return env
    if sys.platform == "win32":
        win_path = r"C:\dev\TARTARIA_new"
        if os.path.isdir(win_path): return win_path
    for p in ["/sessions/clever-eager-johnson/mnt/TARTARIA_new",
              "/mnt/c/dev/TARTARIA_new",
              os.path.expanduser("~/TARTARIA_new")]:
        if os.path.isdir(p): return p
    return os.getcwd()

PROJECT_ROOT = _detect_project_root()

def export_dir_for(moon):
    """Return export dir for a given moon, e.g. 'Moon1', 'Moon5', 'Shared'."""
    d = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Blender", moon)
    os.makedirs(d, exist_ok=True)
    return d

def reset_scene():
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for block in list(bpy.data.meshes):     bpy.data.meshes.remove(block)
    for block in list(bpy.data.materials):  bpy.data.materials.remove(block)

def make_material(name, base_color, roughness=0.5, metallic=0.0, emission=None, emission_strength=2.0):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf is not None:
        bsdf.inputs["Base Color"].default_value = (*base_color, 1.0) if len(base_color) == 3 else base_color
        bsdf.inputs["Roughness"].default_value = roughness
        bsdf.inputs["Metallic"].default_value = metallic
        if emission is not None:
            if "Emission Color" in bsdf.inputs:
                bsdf.inputs["Emission Color"].default_value = (*emission, 1.0)
                bsdf.inputs["Emission Strength"].default_value = emission_strength
            elif "Emission" in bsdf.inputs:
                bsdf.inputs["Emission"].default_value = (*emission, 1.0)
                bsdf.inputs["Emission Strength"].default_value = emission_strength
    return mat

def export_current_as(name, moon="Moon1"):
    """Join everything visible and export as FBX named `name` under `moon` dir."""
    bpy.ops.object.select_all(action='SELECT')
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()
    if bpy.context.active_object:
        bpy.context.active_object.name = name
    out = os.path.join(export_dir_for(moon), name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=out, use_selection=True,
        apply_unit_scale=True, global_scale=1.0,
        apply_scale_options='FBX_SCALE_NONE',
        axis_forward='-Z', axis_up='Y',
        bake_anim=False, mesh_smooth_type='FACE',
        use_mesh_modifiers=True, path_mode='COPY', embed_textures=True,
    )
    print(f"[TARTARIA] Exported: {out}")
    return out

# Legacy alias (Moon1 default)
def export_fbx(name):
    return export_current_as(name, "Moon1")

# Quick primitive helpers
def cube(name, loc, scale, mat=None, rot=(0,0,0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    ob = bpy.context.active_object; ob.name = name; ob.scale = scale
    if mat: ob.data.materials.append(mat)
    return ob

def cyl(name, r, d, loc, mat=None, rot=(0,0,0), verts=24):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=d, location=loc, rotation=rot, vertices=verts)
    ob = bpy.context.active_object; ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

def sphere(name, r, loc, mat=None, segs=16, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=segs, ring_count=rings)
    ob = bpy.context.active_object; ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

def torus(name, major, minor, loc, mat=None, mseg=24, miseg=8, rot=(0,0,0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, location=loc,
                                      major_segments=mseg, minor_segments=miseg, rotation=rot)
    ob = bpy.context.active_object; ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob

def cone(name, r1, r2, d, loc, mat=None, rot=(0,0,0), verts=16):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=d, location=loc, rotation=rot)
    ob = bpy.context.active_object; ob.name = name
    if mat: ob.data.materials.append(mat)
    return ob
