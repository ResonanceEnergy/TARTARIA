"""Lirael - the echo-guardian humanoid stand-in (Sprint 8 Lane 8).

Lirael is the 432 Hz Harmonic-band sentinel of Echohaven. Silver-haired,
robed in deep blue, faintly luminous sigil at her collar. NOT a literal
angel - no wings.

Canonical export name: "LiraelGuardian" -> Assets/_Project/Models/Blender/Moon1/LiraelGuardian.fbx

Per CLAUDE.md NIGHT MANDATE (2026-06-04): preserve legacy filename so Moon 1 wiring
(BlenderImportPostprocessor.NPC_FILENAMES, AssignNPCModels, prefab AssetRefs) auto-resolves
on next reimport. Bare-name "Lirael.fbx" was orphaned.

Scale (2026-06-04): H = 0.946 so visible model height ~= 1.80m.
Formula: visible_h = 1.865 * H + 0.035. Prior H = 1.78 produced ~3.3m
mesh due to author confusion (H used as multiplier on percentage positions).
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import (reset_scene, make_material, export_current_as, cube, cyl,
                     sphere, cone, make_humanoid_armature,
                     join_meshes_then_bind, join_meshes_then_bind_with_overrides,
                     tag_part_for_override)
import bpy

NAME = "LiraelGuardian"  # was "Lirael" — legacy preserved per CLAUDE.md NIGHT MANDATE

# Palette - Harmonic-band guardian (silver / midnight-blue / 432Hz glow)
PAL_SKIN        = (0.95, 0.93, 0.90)   # cool porcelain
PAL_ROBE        = (0.10, 0.14, 0.32)   # midnight blue
PAL_ROBE_TRIM   = (0.62, 0.72, 0.88)   # silver-blue trim
PAL_HAIR        = (0.82, 0.85, 0.90)   # silver
PAL_EYE         = (0.45, 0.85, 1.00)   # luminous cyan
PAL_BOOT        = (0.06, 0.07, 0.14)   # near-black leather
PAL_SIGIL_GLOW  = (0.40, 0.85, 1.00)   # 432 Hz harmonic glow

H = 0.946  # calibrated so visible mesh ~= 1.80m


def build():
    reset_scene()

    skin  = make_material(NAME + "_skin",  PAL_SKIN, roughness=0.55)
    robe  = make_material(NAME + "_robe",  PAL_ROBE, roughness=0.7)
    trim  = make_material(NAME + "_trim",  PAL_ROBE_TRIM, roughness=0.6)
    hair  = make_material(NAME + "_hair",  PAL_HAIR, roughness=0.45)
    iris  = make_material(NAME + "_iris",  PAL_EYE,  roughness=0.30)
    boot  = make_material(NAME + "_boot",  PAL_BOOT, roughness=0.6)
    sigil = make_material(NAME + "_sigil", PAL_SIGIL_GLOW, roughness=0.3,
                          emission=PAL_SIGIL_GLOW, emission_strength=2.4)

    # Torso
    cube("torso", (0, 0, 1.05 * H), (0.30, 0.20, 0.40 * H), robe)
    # Robe skirt - cone flaring outward at hem
    cone("robe_skirt", r1=0.42 * H, r2=0.30 * H, d=0.55 * H,
         loc=(0, 0, 0.45 * H), mat=robe, verts=20)
    # Collar trim band
    cyl("collar", 0.24 * H, 0.05 * H, (0, 0, 1.42 * H), trim, verts=20)
    # Collar sigil (front-facing tiny cube with emission)
    cube("collar_sigil", (0, -0.22 * H, 1.42 * H), (0.05, 0.01, 0.05), sigil)

    # Head
    sphere("head", 0.21 * H, (0, 0, 1.58 * H), skin, segs=18, rings=14)
    # Eyes
    sphere("eye_l", 0.035 * H, (-0.075, -0.18 * H, 1.61 * H), iris, segs=10, rings=8)
    sphere("eye_r", 0.035 * H, ( 0.075, -0.18 * H, 1.61 * H), iris, segs=10, rings=8)

    # Hair - silver, slightly offset back, longer drape behind shoulders
    sphere("hair_crown", 0.225 * H, (0, 0.03 * H, 1.60 * H), hair, segs=16, rings=12)
    hair_drape = cyl("hair_drape", 0.18 * H, 0.40 * H, (0, 0.10 * H, 1.30 * H), hair, verts=16)
    # Stage B (2026-06-04): hair drape extends past shoulders; auto-weight
    # would split it across Chest/UpperChest/UpperArm. Force to Head bone.
    tag_part_for_override(hair_drape, "HairDrape")

    # Arms - slightly inset under robe sleeve
    cyl("arm_l", 0.07 * H, 0.62 * H, (-0.32, 0, 0.95 * H), robe, verts=14)
    cyl("arm_r", 0.07 * H, 0.62 * H, ( 0.32, 0, 0.95 * H), robe, verts=14)
    # Hand spheres
    sphere("hand_l", 0.075 * H, (-0.32, 0, 0.60 * H), skin, segs=10, rings=8)
    sphere("hand_r", 0.075 * H, ( 0.32, 0, 0.60 * H), skin, segs=10, rings=8)
    # Sleeve trim cuffs
    cyl("cuff_l", 0.09 * H, 0.05 * H, (-0.32, 0, 0.66 * H), trim, verts=14)
    cyl("cuff_r", 0.09 * H, 0.05 * H, ( 0.32, 0, 0.66 * H), trim, verts=14)

    # Legs (under skirt - visible only at boots)
    cyl("leg_l", 0.09 * H, 0.45 * H, (-0.12, 0, 0.18 * H), skin, verts=12)
    cyl("leg_r", 0.09 * H, 0.45 * H, ( 0.12, 0, 0.18 * H), skin, verts=12)
    cube("boot_l", (-0.12, 0.05, -0.04 * H), (0.10, 0.16, 0.07), boot)
    cube("boot_r", ( 0.12, 0.05, -0.04 * H), (0.10, 0.16, 0.07), boot)

    # Stage B NPC armature (2026-06-04): T-pose humanoid with HairDrape -> Head.
    arm = make_humanoid_armature(NAME, height=1.80)
    join_meshes_then_bind_with_overrides(NAME, arm, overrides={"HairDrape": "Head"})
    # Canonical export (helper detects armature and takes skinned-export path)
    export_current_as(NAME, "Moon1")


build()
print("[TARTARIA] gen_npc_lirael complete -> Moon1/%s.fbx" % NAME)
