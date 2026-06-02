"""Anastasia — the herb-keeper humanoid stand-in (Sprint 8 Lane 8).

Per CLAUDE.md political-risk callouts: Anastasia is the HERB-KEEPER, NOT a
Romanov princess. No imperial crown, no fleur-de-lis, no Russian Orthodox
motifs. Keep it generic peasant/village garb. Forest-green dress, warm skin,
dark brown hair, simple leather basket strap across the chest.

Canonical export name: "Anastasia"  ->  Assets/_Project/Models/Blender/Moon1/Anastasia.fbx
BlenderImportPostprocessor will auto-create the prefab variant under
Assets/_Project/Prefabs/Moon1/Blender/Anastasia.prefab.

Pure bpy primitives + Boolean/Bevel-free joins. Reset scene first, join at end.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone, torus
import bpy

NAME = "Anastasia"

# Palette — village herb-keeper (NO regal/Romanov iconography)
PAL_DRESS      = (0.20, 0.42, 0.22)   # forest green
PAL_DRESS_TRIM = (0.55, 0.45, 0.25)   # ochre trim (woven)
PAL_APRON      = (0.78, 0.70, 0.55)   # cream apron
PAL_SKIN       = (0.93, 0.78, 0.65)   # warm peasant skin
PAL_HAIR       = (0.22, 0.14, 0.08)   # dark brown
PAL_EYE        = (0.30, 0.20, 0.10)   # warm brown
PAL_BOOT       = (0.28, 0.20, 0.12)   # leather brown
PAL_STRAP      = (0.40, 0.28, 0.16)   # basket strap leather
PAL_BASKET     = (0.65, 0.48, 0.28)   # woven basket

H = 1.70  # adult, slightly shorter than Lirael

def build():
    reset_scene()

    skin  = make_material(NAME+"_skin",  PAL_SKIN, roughness=0.55)
    dress = make_material(NAME+"_dress", PAL_DRESS, roughness=0.75)
    trim  = make_material(NAME+"_trim",  PAL_DRESS_TRIM, roughness=0.65)
    apron = make_material(NAME+"_apron", PAL_APRON, roughness=0.8)
    hair  = make_material(NAME+"_hair",  PAL_HAIR, roughness=0.55)
    iris  = make_material(NAME+"_iris",  PAL_EYE, roughness=0.3)
    boot  = make_material(NAME+"_boot",  PAL_BOOT, roughness=0.6)
    strap = make_material(NAME+"_strap", PAL_STRAP, roughness=0.5)
    bask  = make_material(NAME+"_basket", PAL_BASKET, roughness=0.7)

    # Torso (dress bodice)
    cube("torso", (0, 0, 1.05*H), (0.28, 0.18, 0.40*H), dress)
    # Apron — slightly in front of torso
    cube("apron", (0, -0.19, 1.00*H), (0.22, 0.01, 0.32*H), apron)
    # Skirt — wider flare than Lirael (peasant cut)
    cone("skirt", r1=0.46*H, r2=0.30*H, d=0.55*H,
         loc=(0, 0, 0.45*H), mat=dress, verts=20)
    # Skirt hem trim (woven ochre band)
    cyl("skirt_hem", 0.46*H, 0.04*H, (0, 0, 0.18*H), trim, verts=24)
    # Bodice neckline trim
    cyl("bodice_trim", 0.22*H, 0.04*H, (0, 0, 1.40*H), trim, verts=20)

    # Head
    sphere("head", 0.20*H, (0, 0, 1.56*H), skin, segs=18, rings=14)
    sphere("eye_l", 0.035*H, (-0.072, -0.17*H, 1.59*H), iris, segs=10, rings=8)
    sphere("eye_r", 0.035*H, ( 0.072, -0.17*H, 1.59*H), iris, segs=10, rings=8)

    # Hair — dark brown, gathered back (peasant bun + drape)
    sphere("hair_crown", 0.215*H, (0, 0.03*H, 1.58*H), hair, segs=16, rings=12)
    sphere("hair_bun", 0.10*H, (0, 0.20*H, 1.50*H), hair, segs=12, rings=10)

    # Arms
    cyl("arm_l", 0.07*H, 0.60*H, (-0.30, 0, 0.95*H), dress, verts=14)
    cyl("arm_r", 0.07*H, 0.60*H, ( 0.30, 0, 0.95*H), dress, verts=14)
    sphere("hand_l", 0.075*H, (-0.30, 0, 0.60*H), skin, segs=10, rings=8)
    sphere("hand_r", 0.075*H, ( 0.30, 0, 0.60*H), skin, segs=10, rings=8)

    # Legs (mostly hidden by skirt)
    cyl("leg_l", 0.09*H, 0.42*H, (-0.12, 0, 0.17*H), skin, verts=12)
    cyl("leg_r", 0.09*H, 0.42*H, ( 0.12, 0, 0.17*H), skin, verts=12)
    cube("boot_l", (-0.12, 0.05, -0.04*H), (0.10, 0.16, 0.07), boot)
    cube("boot_r", ( 0.12, 0.05, -0.04*H), (0.10, 0.16, 0.07), boot)

    # Basket strap diagonal across chest (right-shoulder to left-hip)
    cube("basket_strap", (-0.05, -0.18, 1.10*H), (0.30, 0.02, 0.04), strap,
         rot=(0, 1.05, 0))  # ~60deg tilt

    # Small herb basket at left hip
    cyl("basket_body", 0.13*H, 0.16*H, (-0.32, 0.08, 0.75*H), bask, verts=14)
    torus("basket_rim", 0.13*H, 0.015*H, (-0.32, 0.08, 0.83*H), strap,
          mseg=16, miseg=6)

    # Join + canonical export
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = NAME
    export_current_as(NAME, "Moon1")

build()
print(f"[TARTARIA] gen_npc_anastasia complete -> Moon1/{NAME}.fbx")
