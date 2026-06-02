"""Cassian — antagonist humanoid stand-in (Sprint 8 Lane 8).

Upgrades the primitive CapsuleCollider stand-in to a low-poly humanoid:
torso + head + 2 arm cylinders + 2 leg cylinders + hair shape + cowl.
Antagonist palette: charcoal robe, pale skin, jet-black hair, ember-red
shoulder accent (faint emission to read as "ominous" at gameplay distance).

Canonical export name: "Cassian"  ->  Assets/_Project/Models/Blender/Moon1/Cassian.fbx
BlenderImportPostprocessor will auto-create the prefab variant under
Assets/_Project/Prefabs/Moon1/Blender/Cassian.prefab.

No external dependencies — pure bpy primitives. Reset scene first, join at end.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

NAME = "Cassian"

# Palette — antagonist (charcoal + ember accent, NO swastika/SS/political imagery)
PAL_ROBE        = (0.14, 0.14, 0.16)   # charcoal
PAL_ROBE_TRIM   = (0.08, 0.08, 0.10)   # near-black inner trim
PAL_SHOULDER    = (0.55, 0.12, 0.08)   # ember red
PAL_SKIN        = (0.88, 0.82, 0.78)   # pale, slightly ashen
PAL_HAIR        = (0.05, 0.04, 0.05)   # jet black
PAL_EYE         = (0.50, 0.15, 0.10)   # ember-tinged iris
PAL_BOOT        = (0.10, 0.08, 0.08)   # black leather
PAL_EMBER_GLOW  = (0.85, 0.22, 0.08)   # shoulder pauldron glow

H = 1.82  # tall (antagonist silhouette)

def build():
    reset_scene()

    skin   = make_material(NAME+"_skin",  PAL_SKIN, roughness=0.5)
    robe   = make_material(NAME+"_robe",  PAL_ROBE, roughness=0.78)
    trim   = make_material(NAME+"_trim",  PAL_ROBE_TRIM, roughness=0.7)
    pauld  = make_material(NAME+"_pauldron", PAL_SHOULDER, roughness=0.5,
                           emission=PAL_EMBER_GLOW, emission_strength=1.2)
    hair   = make_material(NAME+"_hair",  PAL_HAIR, roughness=0.4)
    iris   = make_material(NAME+"_iris",  PAL_EYE, roughness=0.3,
                           emission=PAL_EMBER_GLOW, emission_strength=0.8)
    boot   = make_material(NAME+"_boot",  PAL_BOOT, roughness=0.5)

    # Torso — squarer than Lirael's (more imposing)
    cube("torso", (0, 0, 1.08*H), (0.34, 0.22, 0.42*H), robe)
    # Robe skirt — long cone, very dark
    cone("robe_skirt", r1=0.46*H, r2=0.34*H, d=0.62*H,
         loc=(0, 0, 0.42*H), mat=robe, verts=20)
    # Cowl / hood draped at neck
    sphere("cowl", 0.26*H, (0, 0.05*H, 1.52*H), robe, segs=14, rings=10)
    # Inner cowl trim
    cyl("cowl_trim", 0.26*H, 0.04*H, (0, 0, 1.38*H), trim, verts=22)

    # Head
    sphere("head", 0.22*H, (0, 0, 1.60*H), skin, segs=18, rings=14)
    # Eyes — ember-tinted, faintly emissive
    sphere("eye_l", 0.038*H, (-0.078, -0.19*H, 1.63*H), iris, segs=10, rings=8)
    sphere("eye_r", 0.038*H, ( 0.078, -0.19*H, 1.63*H), iris, segs=10, rings=8)

    # Hair — jet black, slicked back (low sphere offset behind)
    sphere("hair_crown", 0.235*H, (0, 0.04*H, 1.62*H), hair, segs=16, rings=12)

    # Arms
    cyl("arm_l", 0.08*H, 0.64*H, (-0.36, 0, 0.96*H), robe, verts=14)
    cyl("arm_r", 0.08*H, 0.64*H, ( 0.36, 0, 0.96*H), robe, verts=14)
    sphere("hand_l", 0.085*H, (-0.36, 0, 0.60*H), skin, segs=10, rings=8)
    sphere("hand_r", 0.085*H, ( 0.36, 0, 0.60*H), skin, segs=10, rings=8)

    # Pauldron — single ember-red shoulder accent on the right (asymmetric = sinister)
    sphere("pauldron_r", 0.13*H, (0.38, 0, 1.30*H), pauld, segs=12, rings=10)
    # Small chest clasp echoing the pauldron color
    cube("chest_clasp", (0, -0.21, 1.20*H), (0.04, 0.01, 0.06), pauld)

    # Legs (mostly hidden by robe — keep proportions readable from above)
    cyl("leg_l", 0.10*H, 0.48*H, (-0.13, 0, 0.20*H), trim, verts=12)
    cyl("leg_r", 0.10*H, 0.48*H, ( 0.13, 0, 0.20*H), trim, verts=12)
    cube("boot_l", (-0.13, 0.06, -0.04*H), (0.11, 0.18, 0.07), boot)
    cube("boot_r", ( 0.13, 0.06, -0.04*H), (0.11, 0.18, 0.07), boot)

    # Join + canonical export
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = NAME
    export_current_as(NAME, "Moon1")

build()
print(f"[TARTARIA] gen_npc_cassian complete -> Moon1/{NAME}.fbx")
