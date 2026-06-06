"""TARTARIA Blender helpers - cross-platform, multi-asset friendly."""
import bpy, os, sys, math
from mathutils import Vector


def _detect_project_root():
    env = os.environ.get("TARTARIA_ROOT")
    if env and os.path.isdir(env):
        return env
    if sys.platform == "win32":
        win_path = r"C:\dev\TARTARIA_new"
        if os.path.isdir(win_path):
            return win_path
    for p in [
        "/sessions/clever-eager-johnson/mnt/TARTARIA_new",
        "/mnt/c/dev/TARTARIA_new",
        os.path.expanduser("~/TARTARIA_new"),
    ]:
        if os.path.isdir(p):
            return p
    return os.getcwd()


PROJECT_ROOT = _detect_project_root()


def export_dir_for(moon):
    """Return export dir for a given moon, e.g. 'Moon1', 'Moon5', 'Shared'."""
    d = os.path.join(PROJECT_ROOT, "Assets", "_Project", "Models", "Blender", moon)
    os.makedirs(d, exist_ok=True)
    return d


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in list(bpy.data.meshes):
        bpy.data.meshes.remove(block)
    for block in list(bpy.data.materials):
        bpy.data.materials.remove(block)


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
    """Apply transforms, join, and export as FBX named `name` under `moon` dir.

    Root-cause fix 2026-06-04: previously the helper joined and exported with
    apply_scale_options='FBX_SCALE_NONE' while leaving unapplied object scales
    on cube-based parts (the cube() helper sets ob.scale = scale). Unity's
    importer with bakeAxisConversion=true then combined Blender's unit-scale
    with the unapplied object scale, blowing NPC bounds out to 27-37m instead
    of the intended ~1.7-1.8m. We now:
      1. Apply Rotation+Scale on every mesh before joining (geometry baked,
         object transforms reset to identity).
      2. Set FBX export to FBX_SCALE_ALL so any residual unit-scale gets
         baked into geometry as well.
    Cylinder/sphere/cone/torus parts use radius/depth at creation so they
    were already correct; this change is a no-op for them but fixes cubes.

    NPC armature pipeline Stage A (2026-06-04): when scene contains an
    ARMATURE object, switch export behavior:
      * Apply rotation+scale to meshes only (NOT armature - that breaks rest
        pose / parent matrix).
      * Do NOT join armature + mesh - keep them as two separate objects so
        the FBX node hierarchy preserves the bone tree.
      * Select armature + every child mesh (skip empties, cameras, lights).
      * Export with armature_nodetype='NULL', add_leaf_bones=False, and
        Y/X bone axes which match Unity's humanoid avatar expectations.
    """
    # Detect armature presence - this drives the export branch
    armature_obj = None
    for ob in list(bpy.data.objects):
        if ob.type == "ARMATURE":
            armature_obj = ob
            break

    if armature_obj is not None:
        # ARMATURE EXPORT PATH (skinned NPC)
        # Apply transforms to meshes only - armature must keep its rest pose
        for ob in list(bpy.data.objects):
            if ob.type != "MESH":
                continue
            # Skip if mesh is parented to armature with auto-weights - applying
            # scale on a skinned mesh corrupts vertex groups. The armature parent
            # propagates the unit scale.
            has_armature_mod = any(m.type == "ARMATURE" for m in ob.modifiers)
            if has_armature_mod:
                continue
            bpy.ops.object.select_all(action="DESELECT")
            ob.select_set(True)
            bpy.context.view_layer.objects.active = ob
            try:
                bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
            except RuntimeError as ex:
                print("[TARTARIA] transform_apply warning on %s: %s" % (ob.name, ex))

        # Rename armature to canonical name so the avatar root is predictable
        armature_obj.name = name + "_Armature"

        # Select armature + all mesh children for export
        bpy.ops.object.select_all(action="DESELECT")
        armature_obj.select_set(True)
        for ob in list(bpy.data.objects):
            if ob.type == "MESH":
                ob.select_set(True)
        bpy.context.view_layer.objects.active = armature_obj

        out = os.path.join(export_dir_for(moon), name + ".fbx")
        bpy.ops.export_scene.fbx(
            filepath=out,
            use_selection=True,
            apply_unit_scale=True,
            global_scale=1.0,
            apply_scale_options="FBX_SCALE_ALL",
            axis_forward="-Z",
            axis_up="Y",
            bake_anim=False,
            mesh_smooth_type="FACE",
            use_mesh_modifiers=True,
            add_leaf_bones=False,
            primary_bone_axis="Y",
            secondary_bone_axis="X",
            armature_nodetype="NULL",
            use_armature_deform_only=False,
            path_mode="COPY",
            embed_textures=True,
            object_types={"ARMATURE", "MESH"},
        )
        print("[TARTARIA] Exported (armature): %s" % out)
        return out

    # LEGACY STATIC-MESH PATH (no armature)
    bpy.ops.object.select_all(action="SELECT")
    last_mesh = None
    for ob in list(bpy.context.selected_objects):
        if ob.type == "MESH":
            last_mesh = ob
    if last_mesh is not None:
        bpy.context.view_layer.objects.active = last_mesh
        try:
            bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        except RuntimeError as ex:
            print("[TARTARIA] transform_apply warning during export of %s: %s" % (name, ex))

    bpy.ops.object.select_all(action="SELECT")
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()
    if bpy.context.active_object:
        bpy.context.active_object.name = name

    out = os.path.join(export_dir_for(moon), name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=out,
        use_selection=True,
        apply_unit_scale=True,
        global_scale=1.0,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_anim=False,
        mesh_smooth_type="FACE",
        use_mesh_modifiers=True,
        path_mode="COPY",
        embed_textures=True,
    )
    print("[TARTARIA] Exported: %s" % out)
    return out


# ============================================================
# Humanoid armature pipeline (Stage A, 2026-06-04)
# ============================================================
#
# Goal: ship Moon 1 NPCs (Anastasia / Lirael / Cassian / Bob) with a
# skinned armature so Unity's auto-humanoid mapping can build a valid
# Avatar from AnimationType.Humanoid + autoGenerateAvatarMappingIfUnspecified.
#
# Bone naming convention: we use Unity's HumanBodyBones canonical names
# (Hips, Spine, Chest, UpperChest, Neck, Head, ...ShoulderLeft, LeftUpperArm,
# LeftLowerArm, LeftHand, ...). When Unity sees these names alongside
# autoGenerateAvatarMappingIfUnspecified=1, the AvatarBuilder maps them
# automatically. See:
# https://docs.unity3d.com/Manual/AvatarCreationandSetup.html
# https://docs.unity3d.com/ScriptReference/HumanBodyBones.html
#
# Proportions (head=13% / torso=40% / legs=47%) match the classical
# "8-head" canon used by Unity's Mecanim auto-mapping heuristic.

# Stage B (2026-06-04) NPC armature pipeline upgrade:
#  * Added UpperChest between Chest and Neck (Unity Humanoid Avatar mapping
#    prefers this for richer AC_KayKit_Medium retargeting). Chest top moved
#    down to 0.70, UpperChest spans 0.70->0.78, Neck head lifts to 0.84.
#  * Added LeftEye / RightEye / Jaw bones parented to Head for better
#    LookAt + jaw retarget. Per Unity HumanBodyBones these are OPTIONAL —
#    importer tolerates absence but maps cleanly when present.
#  * Switched arm rest pose from "arms straight down" to strict T-pose:
#    shoulder->upper->lower->hand chain runs along the X axis at 0.78 z,
#    matching KayKit's rest pose for clean retargeting. Hands sit at the
#    canonical x = +/- 0.43 H position (~ 25% beyond shoulder edge),
#    extended along the world X axis at chest height.
#
# Tuple format: (name, head_x_frac, head_z_frac, tail_x_frac, tail_z_frac).
# x_frac/z_frac are fractions of total height H. Negative x is LEFT in
# Blender (we keep that convention; FBX export with axis_forward=-Z, axis_up=Y
# preserves the orientation for Unity).
_HUMANOID_BONES = [
    # Spine chain (root -> head)
    ("Hips",            ( 0.000, 0.470), ( 0.000, 0.530)),
    ("Spine",           ( 0.000, 0.530), ( 0.000, 0.620)),
    ("Chest",           ( 0.000, 0.620), ( 0.000, 0.700)),
    ("UpperChest",      ( 0.000, 0.700), ( 0.000, 0.780)),
    ("Neck",            ( 0.000, 0.840), ( 0.000, 0.870)),
    ("Head",            ( 0.000, 0.870), ( 0.000, 0.970)),
    # Optional facial bones (parented to Head). These improve Mecanim's
    # LookAt + JawOpen avatar mapping. Unity tolerates their absence.
    ("LeftEye",         (-0.040, 0.920), (-0.040, 0.910)),
    ("RightEye",        ( 0.040, 0.920), ( 0.040, 0.910)),
    ("Jaw",             ( 0.000, 0.890), ( 0.000, 0.870)),
    # Left arm chain — T-POSE: extends along NEGATIVE X (Blender's left)
    # at z = 0.78 (shoulder line). Bone tails point further out along -X.
    ("LeftShoulder",    (-0.090, 0.780), (-0.180, 0.780)),
    ("LeftUpperArm",    (-0.180, 0.780), (-0.430, 0.780)),
    ("LeftLowerArm",    (-0.430, 0.780), (-0.680, 0.780)),
    ("LeftHand",        (-0.680, 0.780), (-0.760, 0.780)),
    # Right arm chain — T-POSE: extends along POSITIVE X.
    ("RightShoulder",   ( 0.090, 0.780), ( 0.180, 0.780)),
    ("RightUpperArm",   ( 0.180, 0.780), ( 0.430, 0.780)),
    ("RightLowerArm",   ( 0.430, 0.780), ( 0.680, 0.780)),
    ("RightHand",       ( 0.680, 0.780), ( 0.760, 0.780)),
    # Left leg chain (hip branches off Hips)
    ("LeftUpperLeg",    (-0.075, 0.470), (-0.075, 0.260)),
    ("LeftLowerLeg",    (-0.075, 0.260), (-0.075, 0.060)),
    ("LeftFoot",        (-0.075, 0.060), (-0.075, 0.020)),
    # Right leg chain
    ("RightUpperLeg",   ( 0.075, 0.470), ( 0.075, 0.260)),
    ("RightLowerLeg",   ( 0.075, 0.260), ( 0.075, 0.060)),
    ("RightFoot",       ( 0.075, 0.060), ( 0.075, 0.020)),
]

# Parent-child relationships matching Unity HumanBodyBones tree.
# Stage B: Neck now parented to UpperChest (not Chest); eyes/jaw to Head.
_HUMANOID_PARENTS = {
    "Hips":           None,
    "Spine":          "Hips",
    "Chest":          "Spine",
    "UpperChest":     "Chest",
    "Neck":           "UpperChest",
    "Head":           "Neck",
    "LeftEye":        "Head",
    "RightEye":       "Head",
    "Jaw":            "Head",
    "LeftShoulder":   "UpperChest",
    "LeftUpperArm":   "LeftShoulder",
    "LeftLowerArm":   "LeftUpperArm",
    "LeftHand":       "LeftLowerArm",
    "RightShoulder":  "UpperChest",
    "RightUpperArm":  "RightShoulder",
    "RightLowerArm":  "RightUpperArm",
    "RightHand":      "RightLowerArm",
    "LeftUpperLeg":   "Hips",
    "LeftLowerLeg":   "LeftUpperLeg",
    "LeftFoot":       "LeftLowerLeg",
    "RightUpperLeg":  "Hips",
    "RightLowerLeg":  "RightUpperLeg",
    "RightFoot":      "RightLowerLeg",
}


def make_humanoid_armature(name, height):
    """Create a Unity-Mecanim-friendly humanoid armature.

    Stage B (2026-06-04) build:
      * 23-bone skeleton (was 19): added UpperChest, LeftEye, RightEye, Jaw.
      * Strict T-pose: arm chains extend along world X axis at shoulder z.
      * UpperChest sits at 70-78%H; Neck head lifts to 0.84*H.

    Bone names match Unity HumanBodyBones canonical strings so the importer
    autoGenerateAvatarMappingIfUnspecified flag builds the avatar without
    manual mapping.

    Args:
      name:   armature object name (typically the NPC name)
      height: target full-body height in meters

    Returns:
      The armature object (already in scene, OBJECT mode).
    """
    # Create armature data block + object
    arm_data = bpy.data.armatures.new(name + "_Armature_Data")
    arm_obj  = bpy.data.objects.new(name + "_Armature", arm_data)
    bpy.context.collection.objects.link(arm_obj)
    bpy.context.view_layer.objects.active = arm_obj
    arm_obj.select_set(True)

    # Enter edit mode to construct bones
    bpy.ops.object.mode_set(mode="EDIT")
    edit_bones = arm_data.edit_bones

    H = height
    created = {}
    for bname, (head_x, head_z), (tail_x, tail_z) in _HUMANOID_BONES:
        eb = edit_bones.new(bname)
        eb.head = Vector((head_x * H, 0.0, head_z * H))
        # Bone tail position — uses explicit (x, z) tuple in Stage B so we
        # can lay arms out along the X axis (T-pose) instead of Z.
        # If head == tail (degenerate zero-length), nudge tail along +Y so
        # Blender doesn't reject the bone.
        if abs(tail_x - head_x) < 0.0005 and abs(tail_z - head_z) < 0.0005:
            eb.tail = Vector((head_x * H, 0.06 * H, head_z * H))
        else:
            eb.tail = Vector((tail_x * H, 0.0, tail_z * H))
        created[bname] = eb

    # Parent bones per humanoid tree
    for bname, parent_name in _HUMANOID_PARENTS.items():
        if parent_name is None:
            continue
        if bname in created and parent_name in created:
            created[bname].parent = created[parent_name]
            # Use connect=False so child head positions stay where we placed them
            created[bname].use_connect = False

    # Back to object mode so subsequent ops on this armature work
    bpy.ops.object.mode_set(mode="OBJECT")

    print("[TARTARIA] Built humanoid armature %s with %d bones, height %.2fm (Stage B T-pose)"
          % (name, len(_HUMANOID_BONES), H))
    return arm_obj


def bind_with_manual_parent_overrides(mesh_obj, armature_obj, overrides=None):
    """Stage B helper: bind a mesh to an armature with vertex-group overrides.

    Pre-assigns specified vertex groups to specific bones BEFORE auto-weight,
    for accessories that the heat-map auto-weighter assigns poorly:
      * Anastasia's herb basket sits near her left hip but auto-weight might
        split it between Hips / LeftUpperLeg / LeftHand depending on proximity.
        Forcing it to a single bone (Hips) keeps it attached cleanly.
      * Lirael's hair drape hangs from the head but extends past the shoulders.
      * Cassian's pauldron sits on the right shoulder but extends out.
      * Bob's hair sphere ditto.

    overrides: dict {mesh_part_name_substring: bone_name}. Matches the *name*
               assigned to the primitive at creation time (cube/cyl/sphere
               'name=' arg in the gen scripts), e.g.
               {"basket_body": "Hips", "basket_rim": "Hips"}.

    Approach:
      1. Run auto-weight bind normally (mesh already joined into a single
         object, so vertex-groups for each bone now exist).
      2. For each override mesh_name in the dict, find all vertices of the
         joined mesh that came from that primitive (we track this by
         materials + vertex group naming convention).
      3. Wipe their existing weights and re-assign weight=1.0 to the
         specified target bone.

    Caveat: this helper currently uses the JOINED mesh's vertex-group names.
    The cleanest approach is to call it on the joined mesh AFTER bind, naming
    the parts we want to override via separate vertex_groups created on the
    geometry BEFORE join. The gen scripts' new pattern is:
      1. Build all parts via cube/cyl/sphere as usual.
      2. Tag override parts by name (already done — sphere("basket_body", ...))
      3. Call join_meshes_with_part_groups() to track per-part vertex ranges.
      4. Call bind_with_manual_parent_overrides(mesh, arm, {part_name: bone}).

    For Stage B we take a simpler path: build a vertex group per override
    part NAME before join, then after bind/auto-weight, the heat-map will
    have written tiny competing weights into those vertices; we wipe and
    reassign weight 1.0 from the override map.

    Args:
      mesh_obj: joined mesh (single MESH object)
      armature_obj: armature returned by make_humanoid_armature
      overrides: dict {part_vgroup_name: target_bone_name}

    Returns:
      mesh_obj (with Armature modifier and override weights applied).
    """
    overrides = overrides or {}
    if mesh_obj is None or armature_obj is None:
        return mesh_obj

    # First, do the normal auto-weight bind (creates one vgroup per bone).
    bind_mesh_to_armature(mesh_obj, armature_obj)

    if not overrides:
        return mesh_obj

    # For each override entry: pull the pre-existing part vgroup (created
    # at primitive-build time via name=), enumerate its vertex indices,
    # then zero them out across ALL bone vertex groups and re-assign
    # weight=1.0 to the target bone vgroup.
    bpy.ops.object.select_all(action="DESELECT")
    mesh_obj.select_set(True)
    bpy.context.view_layer.objects.active = mesh_obj

    for part_vgroup_name, target_bone in overrides.items():
        part_vg = mesh_obj.vertex_groups.get(part_vgroup_name)
        if part_vg is None:
            print("[TARTARIA] override: part vgroup '%s' not found, skipping"
                  % part_vgroup_name)
            continue
        target_vg = mesh_obj.vertex_groups.get(target_bone)
        if target_vg is None:
            print("[TARTARIA] override: target bone vgroup '%s' missing, skipping"
                  % target_bone)
            continue

        # Collect vertex indices belonging to this part
        part_indices = []
        for v in mesh_obj.data.vertices:
            for g in v.groups:
                if g.group == part_vg.index and g.weight > 0.0:
                    part_indices.append(v.index)
                    break

        if not part_indices:
            print("[TARTARIA] override: part '%s' had no verts assigned" % part_vgroup_name)
            continue

        # Zero existing bone weights on these verts (any vgroup name that
        # matches a bone in _HUMANOID_PARENTS)
        bone_names = set(_HUMANOID_PARENTS.keys())
        for vg in list(mesh_obj.vertex_groups):
            if vg.name in bone_names and vg.name != target_bone:
                try:
                    vg.remove(part_indices)
                except RuntimeError:
                    pass

        # Assign weight 1.0 to target bone vgroup
        target_vg.add(part_indices, 1.0, "REPLACE")

        print("[TARTARIA] override: %d verts of '%s' reassigned to bone '%s'"
              % (len(part_indices), part_vgroup_name, target_bone))

    return mesh_obj


def bind_mesh_to_armature(mesh_obj, armature_obj):
    """Bind a mesh to an armature with automatic-weights vertex groups.

    Uses bpy.ops.object.parent_set(type='ARMATURE_AUTO') which adds an
    Armature modifier to the mesh, sets armature as the parent, and computes
    heat-map vertex weights per bone. Result: skinned mesh that deforms with
    pose changes.

    Caveat: heat-map weights are coarse. Tight cloth/clothing meshes may
    need manual weight painting in Stage B. Acceptable for placeholder
    rigging where the goal is "joints bend, no T-pose lock".

    Args:
      mesh_obj:     single mesh object (post-join is OK; mesh-objs-only)
      armature_obj: armature returned by make_humanoid_armature()

    Returns:
      mesh_obj (with Armature modifier attached).
    """
    if mesh_obj is None or armature_obj is None:
        print("[TARTARIA] bind_mesh_to_armature: null mesh or armature, skipping")
        return mesh_obj
    if mesh_obj.type != "MESH":
        print("[TARTARIA] bind_mesh_to_armature: %s is not a MESH, skipping" % mesh_obj.name)
        return mesh_obj
    if armature_obj.type != "ARMATURE":
        print("[TARTARIA] bind_mesh_to_armature: %s is not an ARMATURE, skipping" % armature_obj.name)
        return mesh_obj

    # Select mesh first (will become child), then armature (will become parent/active)
    bpy.ops.object.select_all(action="DESELECT")
    mesh_obj.select_set(True)
    armature_obj.select_set(True)
    bpy.context.view_layer.objects.active = armature_obj

    try:
        bpy.ops.object.parent_set(type="ARMATURE_AUTO")
    except RuntimeError as ex:
        print("[TARTARIA] parent_set ARMATURE_AUTO failed for %s: %s" % (mesh_obj.name, ex))
        # Fallback: add Armature modifier manually, no auto-weights
        mod = mesh_obj.modifiers.new(name="Armature", type="ARMATURE")
        mod.object = armature_obj
        mod.use_vertex_groups = True

    # Verify modifier was attached
    has_arm_mod = any(m.type == "ARMATURE" for m in mesh_obj.modifiers)
    if not has_arm_mod:
        print("[TARTARIA] WARN: Armature modifier not present on %s after bind" % mesh_obj.name)
    else:
        print("[TARTARIA] Bound %s to %s (Armature modifier OK)" % (mesh_obj.name, armature_obj.name))

    return mesh_obj


def tag_part_for_override(mesh_obj, vgroup_name):
    """Stage B helper: stamp a vertex group on a mesh-object so that, after
    join_meshes_with_overrides() + bind_with_manual_parent_overrides()
    runs, the verts originating from this primitive can be looked up by
    `vgroup_name` and reassigned to a target bone.

    Call this immediately after creating an accessory primitive whose
    auto-weighting would otherwise be poor (basket, hair drape, pauldron).

    Args:
      mesh_obj:     a MESH object (e.g. returned by cube/cyl/sphere).
      vgroup_name:  short name for the part, used as the key in the
                    overrides dict later.

    Idempotent: replaces any existing vertex group with the same name.
    """
    if mesh_obj is None or mesh_obj.type != "MESH":
        return
    # Remove any prior vgroup of this name (idempotent)
    existing = mesh_obj.vertex_groups.get(vgroup_name)
    if existing is not None:
        mesh_obj.vertex_groups.remove(existing)
    vg = mesh_obj.vertex_groups.new(name=vgroup_name)
    all_indices = [v.index for v in mesh_obj.data.vertices]
    vg.add(all_indices, 1.0, "REPLACE")


def join_meshes_then_bind_with_overrides(name, armature_obj, overrides=None):
    """Stage B join+bind helper with accessory overrides.

    Same as join_meshes_then_bind but pipes through
    bind_with_manual_parent_overrides which post-processes the joined
    mesh's vertex weights to force accessory parts onto the correct bone.

    overrides: dict {part_vgroup_name: bone_name}. The part vgroup names
               must have been tagged via tag_part_for_override() BEFORE
               this function runs.
    """
    overrides = overrides or {}

    # Select MESH objects only - armature was created last so it's in scene
    bpy.ops.object.select_all(action="DESELECT")
    last_mesh = None
    for ob in list(bpy.data.objects):
        if ob.type == "MESH":
            ob.select_set(True)
            last_mesh = ob
    if last_mesh is None:
        print("[TARTARIA] join_meshes_then_bind_with_overrides: no meshes to join")
        return None
    bpy.context.view_layer.objects.active = last_mesh

    try:
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    except RuntimeError as ex:
        print("[TARTARIA] transform_apply warning before bind: %s" % ex)

    # Join into single mesh — preserves vertex groups created via
    # tag_part_for_override() because Blender's join op merges vgroups by name.
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = name + "_Body"

    # Stage B bind with overrides
    bind_with_manual_parent_overrides(joined, armature_obj, overrides=overrides)
    return joined


def join_meshes_then_bind(name, armature_obj):
    """Helper: join all current MESH objects into one, then bind to armature.

    The NPC scripts build geometry from many primitive parts. They previously
    called export_current_as() which selected-all + joined + exported in one
    step. With the armature pipeline we need:
      1. Join meshes BEFORE binding (one Armature modifier per mesh is cleaner)
      2. Bind the joined mesh to the armature
      3. THEN call export_current_as() which will detect the armature and
         take the armature export path.

    Returns the joined mesh object.
    """
    # Select MESH objects only - armature was created last so it's in scene
    bpy.ops.object.select_all(action="DESELECT")
    last_mesh = None
    for ob in list(bpy.data.objects):
        if ob.type == "MESH":
            ob.select_set(True)
            last_mesh = ob
    if last_mesh is None:
        print("[TARTARIA] join_meshes_then_bind: no meshes to join")
        return None
    bpy.context.view_layer.objects.active = last_mesh

    # Apply rotation+scale on every mesh BEFORE join, BEFORE bind
    # (auto-weights heat-map requires consistent world transforms)
    try:
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    except RuntimeError as ex:
        print("[TARTARIA] transform_apply warning before bind: %s" % ex)

    # Join into single mesh
    if len(bpy.context.selected_objects) > 1:
        bpy.ops.object.join()
    joined = bpy.context.active_object
    joined.name = name + "_Body"

    # Bind to armature
    bind_mesh_to_armature(joined, armature_obj)
    return joined


def export_fbx(name):
    """Legacy alias (Moon1 default)."""
    return export_current_as(name, "Moon1")


def cube(name, loc, scale, mat=None, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    ob = bpy.context.active_object
    ob.name = name
    ob.scale = scale
    if mat:
        ob.data.materials.append(mat)
    return ob


def cyl(name, r, d, loc, mat=None, rot=(0, 0, 0), verts=24):
    bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=d, location=loc, rotation=rot, vertices=verts)
    ob = bpy.context.active_object
    ob.name = name
    if mat:
        ob.data.materials.append(mat)
    return ob


def sphere(name, r, loc, mat=None, segs=16, rings=12):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=r, location=loc, segments=segs, ring_count=rings)
    ob = bpy.context.active_object
    ob.name = name
    if mat:
        ob.data.materials.append(mat)
    return ob


def torus(name, major, minor, loc, mat=None, mseg=24, miseg=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, location=loc,
                                     major_segments=mseg, minor_segments=miseg, rotation=rot)
    ob = bpy.context.active_object
    ob.name = name
    if mat:
        ob.data.materials.append(mat)
    return ob


def cone(name, r1, r2, d, loc, mat=None, rot=(0, 0, 0), verts=16):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=d, location=loc, rotation=rot)
    ob = bpy.context.active_object
    ob.name = name
    if mat:
        ob.data.materials.append(mat)
    return ob
