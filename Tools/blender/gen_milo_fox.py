"""Milo — Aether-fox companion (canonical species per docs/15 §10).

Lore spec (docs/15_MVP_BUILD_SPEC.md §10):
  Species : Fox-like creature, Tartarian origin
  Size    : 40cm tall (thigh-height to player)
  Visual  : Luminous fur, golden-tipped ears, eyes that glow with Aether
  Function: Tutorial guide, lore delivery, emotional anchor

Topology: low-poly quadruped — body cylinder (lengthwise), four leg cylinders,
head sphere with snout cone, two triangular ears with golden tips, fluffy
tail cone. NO upright humanoid — Milo was previously authored as a child-
humanoid `MiloBoy.fbx` which contradicted the lore. Removed 2026-06-04.

Target footprint:
  Height (paws -> ear tips) :  ~0.40 m
  Length (snout -> tail)    :  ~0.80 m   (with tail extended)
  Width  (shoulder)         :  ~0.18 m   (body) / ~0.60 m if you measure the
                                 tail-swing pose, but the static rest pose
                                 sits within ~0.30 m wide once joined.

Canonical export name : "MiloBoy"  (kept for backward-compat with the
  BlenderImportPostprocessor.NPC_FILENAMES list + AssignNPCModels lookups
  that already reference Assets/_Project/Models/Blender/Moon1/MiloBoy.fbx).
Once Milo's prefab/scene references are updated, rename to "Milo".

Pure bpy primitives (cylinder/sphere/cone) — geometry-baked at creation, so
no unapplied scales. Joins and exports via _common.export_current_as which
also applies transforms as a defence-in-depth pass.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

NAME = "MiloBoy"  # legacy filename; canonical species = Milo Aether-fox

# Palette
PAL_FUR        = (0.78, 0.55, 0.22)    # luminous rust-orange (Aether fox)
PAL_BELLY      = (0.96, 0.90, 0.78)    # cream chest/belly
PAL_EAR_TIP    = (1.00, 0.82, 0.30)    # golden ear tip
PAL_PAW        = (0.18, 0.12, 0.08)    # dark paws
PAL_NOSE       = (0.10, 0.08, 0.07)    # black nose
PAL_EYE_GLOW   = (0.45, 0.90, 1.00)    # Aether-blue glowing eyes
PAL_TAIL_TIP   = (1.00, 0.94, 0.85)    # bright tail tip


def build():
    reset_scene()

    fur     = make_material(NAME+"_fur",     PAL_FUR,      roughness=0.85)
    belly   = make_material(NAME+"_belly",   PAL_BELLY,    roughness=0.85)
    ear_tip = make_material(NAME+"_ear_tip", PAL_EAR_TIP,  roughness=0.55, metallic=0.15,
                            emission=PAL_EAR_TIP, emission_strength=0.4)
    paw     = make_material(NAME+"_paw",     PAL_PAW,      roughness=0.7)
    nose    = make_material(NAME+"_nose",    PAL_NOSE,     roughness=0.4)
    eye     = make_material(NAME+"_eye",     PAL_EYE_GLOW, roughness=0.2,
                            emission=PAL_EYE_GLOW, emission_strength=3.0)
    tail_tip= make_material(NAME+"_tail_tip",PAL_TAIL_TIP, roughness=0.7,
                            emission=PAL_TAIL_TIP, emission_strength=0.6)

    # --- Body coordinate system ---------------------------------------------
    # +Y = forward (snout direction), +Z = up, +X = right.
    # Ground plane Z = 0. Paws sit on ground.
    # Leg length ~0.18m, body centre Z ~ 0.22m, head centre Z ~ 0.28m,
    # ear tips ~ 0.40m.

    # --- Body --------------------------------------------------------------
    # Main torso: cylinder along Y axis (forward). depth = body length 0.35m,
    # radius 0.075m -> shoulder ~0.15m wide. rot=(rotate cylinder so axis = Y)
    cyl("torso", r=0.075, d=0.35, loc=(0, -0.02, 0.22), mat=fur,
        rot=(1.5708, 0, 0), verts=18)   # 90deg around X -> axis along Y
    # Belly underside (lighter, slightly offset down)
    cyl("belly", r=0.060, d=0.30, loc=(0, -0.02, 0.205), mat=belly,
        rot=(1.5708, 0, 0), verts=14)

    # --- Head --------------------------------------------------------------
    # Head sphere forward of torso
    sphere("head", r=0.085, loc=(0, 0.20, 0.27), mat=fur, segs=18, rings=14)
    # Snout cone — narrower in front
    cone("snout", r1=0.055, r2=0.020, d=0.09, loc=(0, 0.28, 0.255),
         mat=fur, rot=(1.5708, 0, 0), verts=14)
    # Nose tip
    sphere("nose", r=0.015, loc=(0, 0.325, 0.25), mat=nose, segs=8, rings=6)
    # Eyes — small glowing spheres
    sphere("eye_l", r=0.018, loc=(-0.040, 0.245, 0.295), mat=eye, segs=10, rings=8)
    sphere("eye_r", r=0.018, loc=( 0.040, 0.245, 0.295), mat=eye, segs=10, rings=8)

    # --- Ears (triangular cones, fur + golden tip) -------------------------
    # Left ear
    cone("ear_l_base", r1=0.030, r2=0.005, d=0.07,
         loc=(-0.050, 0.16, 0.345), mat=fur,
         rot=(0.20, -0.15, 0), verts=10)
    # Right ear
    cone("ear_r_base", r1=0.030, r2=0.005, d=0.07,
         loc=( 0.050, 0.16, 0.345), mat=fur,
         rot=(0.20,  0.15, 0), verts=10)
    # Golden ear tips
    sphere("ear_l_tip", r=0.012, loc=(-0.058, 0.150, 0.385), mat=ear_tip, segs=8, rings=6)
    sphere("ear_r_tip", r=0.012, loc=( 0.058, 0.150, 0.385), mat=ear_tip, segs=8, rings=6)

    # --- Legs (four short cylinders) ---------------------------------------
    # leg radius 0.025, length 0.18m so paws at ground Z=0.
    LEG_R = 0.025
    LEG_D = 0.18
    LEG_Z = 0.09           # cylinder centre = 0.18 / 2
    # Front pair
    cyl("leg_fl", LEG_R, LEG_D, (-0.05, 0.10, LEG_Z), fur, verts=10)
    cyl("leg_fr", LEG_R, LEG_D, ( 0.05, 0.10, LEG_Z), fur, verts=10)
    # Back pair
    cyl("leg_bl", LEG_R, LEG_D, (-0.05, -0.13, LEG_Z), fur, verts=10)
    cyl("leg_br", LEG_R, LEG_D, ( 0.05, -0.13, LEG_Z), fur, verts=10)
    # Paws (darker squashed spheres at the foot of each leg)
    sphere("paw_fl", 0.030, (-0.05, 0.10, 0.012), paw, segs=10, rings=6)
    sphere("paw_fr", 0.030, ( 0.05, 0.10, 0.012), paw, segs=10, rings=6)
    sphere("paw_bl", 0.030, (-0.05, -0.13, 0.012), paw, segs=10, rings=6)
    sphere("paw_br", 0.030, ( 0.05, -0.13, 0.012), paw, segs=10, rings=6)

    # --- Tail (fluffy cone trailing back+up) -------------------------------
    cone("tail", r1=0.050, r2=0.015, d=0.18,
         loc=(0, -0.27, 0.27), mat=fur,
         rot=(1.2, 0, 0), verts=12)
    sphere("tail_tip", r=0.025, loc=(0, -0.34, 0.34), mat=tail_tip, segs=10, rings=8)

    # --- Export (helper applies transforms, joins, writes FBX) -------------
    export_current_as(NAME, "Moon1")


build()
print(f"[TARTARIA] gen_milo_fox complete -> Moon1/{NAME}.fbx (canonical Aether-fox per docs/15 §10)")
