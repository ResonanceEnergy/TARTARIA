"""Cassian - the carter / ember-wagon antagonist stand-in (Sprint 8 Lane 8).

Tall silhouette, dark robe, asymmetric ember pauldron on right shoulder.
Black hair, ember-tinted eyes. Sinister but workmanlike, not cartoon-evil.

Canonical export name: "CassianCarter" -> Assets/_Project/Models/Blender/Moon1/CassianCarter.fbx

Per CLAUDE.md NIGHT MANDATE (2026-06-04): preserve legacy filename so Moon 1 wiring
(BlenderImportPostprocessor.NPC_FILENAMES, AssignNPCModels, prefab AssetRefs) auto-resolves
on next reimport. Bare-name "Cassian.fbx" was orphaned.

Scale (2026-06-04): H = 0.931 so visible model height ~= 1.80m (tall).
Formula: visible_h = 1.895 * H + 0.035. Prior H = 1.82 produced ~3.5m mesh.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import (reset_scene, make_material, export_current_as, cube, cyl,
                     sphere, cone, make_humanoid_armature,
                     join_meshes_then_bind, join_meshes_then_bind_with_overrides,
                     tag_part_for_override)
import bpy

NAME = "CassianCarter"  # was "Cassian" — legacy preserved per CLAUDE.md NIGHT MANDATE

PAL_SKIN        = (0.78, 0.62, 0.50)   # weathered
PAL_ROBE        = (0.08, 0.06, 0.05)   # near-black coal
PAL_ROBE_TRIM   = (0.18, 0.14, 0.10)   # dark brown trim
PAL_SHOULDER    = (0.92, 0.30, 0.10)   # ember red pauldron
PAL_HAIR        = (0.05, 0.04, 0.03)   # jet black
PAL_EYE         = (0.55, 0.18, 0.10)   # ember
PAL_BOOT        = (0.10, 0.08, 0.08)   # black leather
PAL_EMBER_GLOW  = (0.85, 0.22, 0.08)   # shoulder pauldron glow

H = 0.931  # calibrated so visible mesh ~= 1.80m (tall antagonist silhouette)


def build():
    reset_scene()

    skin  = make_material(NAME + "_skin",     PAL_SKIN, roughness=0.5)
    robe  = make_material(NAME + "_robe",     PAL_ROBE, roughness=0.78)
    trim  = make_material(NAME + "_trim",     PAL_ROBE_TRIM, roughness=0.7)
    pauld = make_material(NAME + "_pauldron", PAL_SHOULDER, roughness=0.5,
                          emission=PAL_EMBER_GLOW, emission_strength=1.2)
    hair  = make_material(NAME + "_hair",     PAL_HAIR, roughness=0.4)
    iris  = make_material(NAME + "_iris",     PAL_EYE, roughness=0.3,
                          emission=PAL_EMBER_GLOW, emission_strength=0.8)
    boot  = make_material(NAME + "_boot",     PAL_BOOT, roughness=0.5)

    # Torso - squarer than Lirael's (more imposing)
    cube("torso", (0, 0, 1.08 * H), (0.34, 0.22, 0.42 * H), robe)
    # Robe skirt - long cone, very dark
    cone("robe_skirt", r1=0.46 * H, r2=0.34 * H, d=0.62 * H,
         loc=(0, 0, 0.42 * H), mat=robe, verts=20)
    # Cowl / hood draped at neck
    sphere("cowl", 0.26 * H, (0, 0.05 * H, 1.52 * H), robe, segs=14, rings=10)
    # Inner cowl trim
    cyl("cowl_trim", 0.26 * H, 0.04 * H, (0, 0, 1.38 * H), trim, verts=22)

    # Head
    sphere("head", 0.22 * H, (0, 0, 1.60 * H), skin, segs=18, rings=14)
    # Eyes - ember-tinted, faintly emissive
    sphere("eye_l", 0.038 * H, (-0.078, -0.19 * H, 1.63 * H), iris, segs=10, rings=8)
    sphere("eye_r", 0.038 * H, ( 0.078, -0.19 * H, 1.63 * H), iris, segs=10, rings=8)

    # Hair - jet black, slicked back (low sphere offset behind)
    sphere("hair_crown", 0.235 * H, (0, 0.04 * H, 1.62 * H), hair, segs=16, rings=12)

    # Arms
    cyl("arm_l", 0.08 * H, 0.64 * H, (-0.36, 0, 0.96 * H), robe, verts=14)
    cyl("arm_r", 0.08 * H, 0.64 * H, ( 0.36, 0, 0.96 * H), robe, verts=14)
    sphere("hand_l", 0.085 * H, (-0.36, 0, 0.60 * H), skin, segs=10, rings=8)
    sphere("hand_r", 0.085 * H, ( 0.36, 0, 0.60 * H), skin, segs=10, rings=8)

    # Pauldron - single ember-red shoulder accent on the right (asymmetric)
    pauldron = sphere("pauldron_r", 0.13 * H, (0.38, 0, 1.30 * H), pauld, segs=12, rings=10)
    # Small chest clasp echoing the pauldron color
    cube("chest_clasp", (0, -0.21, 1.20 * H), (0.04, 0.01, 0.06), pauld)
    # Stage B (2026-06-04): pauldron extends past right shoulder; auto-weight
    # would split between RightShoulder/RightUpperArm/UpperChest. Force to RightShoulder.
    tag_part_for_override(pauldron, "Pauldron_R")

    # Legs (mostly hidden by robe - keep proportions readable from above)
    cyl("leg_l", 0.10 * H, 0.48 * H, (-0.13, 0, 0.20 * H), trim, verts=12)
    cyl("leg_r", 0.10 * H, 0.48 * H, ( 0.13, 0, 0.20 * H), trim, verts=12)
    cube("boot_l", (-0.13, 0.06, -0.04 * H), (0.11, 0.18, 0.07), boot)
    cube("boot_r", ( 0.13, 0.06, -0.04 * H), (0.11, 0.18, 0.07), boot)

    # Stage B NPC armature (2026-06-04): T-pose humanoid with Pauldron_R -> RightShoulder.
    arm = make_humanoid_armature(NAME, height=1.80)
    join_meshes_then_bind_with_overrides(NAME, arm, overrides={"Pauldron_R": "RightShoulder"})
    # Canonical export (helper detects armature and takes skinned-export path)
    export_current_as(NAME, "Moon1")


build()
print("[TARTARIA] gen_npc_cassian complete -> Moon1/%s.fbx" % NAME)
