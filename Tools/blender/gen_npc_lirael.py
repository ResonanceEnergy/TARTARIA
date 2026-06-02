"""Lirael — echo guardian humanoid stand-in (Sprint 8 Lane 8).

Upgrades the primitive CapsuleCollider stand-in to a low-poly humanoid:
torso + head + 2 arm cylinders + 2 leg cylinders + hair shape + robe taper.
Tinted with Lirael's palette: sky-blue robe, pale skin, silver hair, faint
432Hz harmonic emission on her collar sigil.

Canonical export name: "Lirael"  ->  Assets/_Project/Models/Blender/Moon1/Lirael.fbx
BlenderImportPostprocessor will auto-create the prefab variant under
Assets/_Project/Prefabs/Moon1/Blender/Lirael.prefab.

No external dependencies — pure bpy primitives. Reset scene first, join at end.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

NAME = "Lirael"

# Palette (rgba via make_material — alpha is implicit 1.0)
PAL_ROBE        = (0.55, 0.75, 0.92)   # sky blue
PAL_ROBE_TRIM   = (0.30, 0.50, 0.78)   # darker indigo trim
PAL_SKIN        = (0.94, 0.88, 0.83)   # pale skin
PAL_HAIR        = (0.86, 0.88, 0.92)   # silver-white
PAL_EYE         = (0.20, 0.55, 0.78)   # cyan iris
PAL_BOOT        = (0.18, 0.18, 0.22)   # near-black leather
PAL_SIGIL_GLOW  = (0.40, 0.85, 1.00)   # 432 Hz harmonic glow

H = 1.78  # adult height scale

def build():
    reset_scene()

    skin   = make_material(NAME+"_skin",  PAL_SKIN, roughness=0.55)
    robe   = make_material(NAME+"_robe",  PAL_ROBE, roughness=0.7)
    trim   = make_material(NAME+"_trim",  PAL_ROBE_TRIM, roughness=0.6)
    hair   = make_material(NAME+"_hair",  PAL_HAIR, roughness=0.45)
    iris   = make_material(NAME+"_iris",  PAL_EYE,  roughness=0.30)
    boot   = make_material(NAME+"_boot",  PAL_BOOT, roughness=0.6)
    sigil  = make_material(NAME+"_sigil", PAL_SIGIL_GLOW, roughness=0.3,
                           emission=PAL_SIGIL_GLOW, emission_strength=2.4)

    # Torso (slightly tapered: cone narrower at shoulders, wider at hem via skirt cone)
    cube("torso", (0, 0, 1.05*H), (0.30, 0.20, 0.40*H), robe)
    # Robe skirt — cone flaring outward at hem
    cone("robe_skirt", r1=0.42*H, r2=0.30*H, d=0.55*H,
         loc=(0, 0, 0.45*H), mat=robe, verts=20)
    # Collar trim band
    cyl("collar", 0.24*H, 0.05*H, (0, 0, 1.42*H), trim, verts=20)
    # Collar sigil (front-facing tiny cube with emission)
    cube("collar_sigil", (0, -0.22*H, 1.42*H), (0.05, 0.01, 0.05), sigil)

    # Head
    sphere("head", 0.21*H, (0, 0, 1.58*H), skin, segs=18, rings=14)
    # Eyes
    sphere("eye_l", 0.035*H, (-0.075, -0.18*H, 1.61*H), iris, segs=10, rings=8)
    sphere("eye_r", 0.035*H, ( 0.075, -0.18*H, 1.61*H), iris, segs=10, rings=8)

    # Hair — silver, slightly offset back, longer drape behind shoulders
    sphere("hair_crown", 0.225*H, (0, 0.03*H, 1.60*H), hair, segs=16, rings=12)
    cyl("hair_drape", 0.18*H, 0.40*H, (0, 0.10*H, 1.30*H), hair, verts=16)

    # Arms — slightly inset under robe sleeve
    cyl("arm_l", 0.07*H, 0.62*H, (-0.32, 0, 0.95*H), robe, verts=14)
    cyl("arm_r", 0.07*H, 0.62*H, ( 0.32, 0, 0.95*H), robe, verts=14)
    # Hand spheres
    sphere("hand_l", 0.075*H, (-0.32, 0, 0.60*H), skin, segs=10, rings=8)
    sphere("hand_r", 0.075*H, ( 0.32, 0, 0.60*H), skin, segs=10, rings=8)
    # Sleeve trim cuffs
    cyl("cuff_l", 0.09*H, 0.05*H, (-0.32, 0, 0.66*H), trim, verts=14)
    cyl("cuff_r", 0.09*H, 0.05*H, ( 0.32, 0, 0.66*H), trim, verts=14)

    # Legs (under skirt — visible only at boots)
    cyl("leg_l", 0.09*H, 0.45*H, (-0.12, 0, 0.18*H), skin, verts=12)
    cyl("leg_r", 0.09*H, 0.45*H, ( 0.12, 0, 0.18*H), skin, verts=12)
    cube("boot_l", (-0.12, 0.05, -0.04*H), (0.10, 0.16, 0.07), boot)
    cube("boot_r", ( 0.12, 0.05, -0.04*H), (0.10, 0.16, 0.07), boot)

    # Join + canonical export
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = NAME
    export_current_as(NAME, "Moon1")

build()
print(f"[TARTARIA] gen_npc_lirael complete -> Moon1/{NAME}.fbx")
