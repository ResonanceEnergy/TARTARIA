"""Moon 4 — DeepForge: 6 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon4"

def asset_GiantAnvil():
    reset_scene()
    iron = make_material("Ga_Iron", (0.18,0.18,0.20), roughness=0.4, metallic=0.7)
    stone = make_material("Ga_Stone", (0.45,0.42,0.38), roughness=0.85)
    # Wood/stone block base
    cube("Base", (0,0,0.45), (0.8, 0.45, 0.9), stone)
    # Anvil top
    cube("AnvBody", (0,0,1.05), (0.8, 0.30, 0.15), iron)
    cone("Horn", 0.10, 0.02, 0.40, (0.55, 0, 1.05), iron, verts=8, rot=(0, math.pi/2, 0))
    cube("Step", (-0.30, 0, 1.20), (0.20, 0.30, 0.10), iron)
    cyl("HoleHardy", 0.04, 0.06, (0.0, 0, 1.13), iron, verts=8)
    export_current_as("GiantAnvil", MOON)

def asset_ForgeBellows():
    reset_scene()
    wood = make_material("Fb_Wood", (0.42,0.28,0.16), roughness=0.7)
    leather = make_material("Fb_Leather", (0.55,0.32,0.18), roughness=0.7)
    iron = make_material("Fb_Iron", (0.30,0.28,0.25), roughness=0.6, metallic=0.5)
    cube("BottomBoard", (0,0,0.04), (1.0, 0.5, 0.06), wood)
    cube("TopBoard", (0,0,0.36), (1.0, 0.5, 0.06), wood)
    # Triangular leather body — approximated by tapered shape (use cylinder rotated 90°)
    cone("Body", 0.30, 0.10, 0.80, (0.30, 0, 0.20), leather, verts=16, rot=(0,math.pi/2,0))
    # Spout
    cyl("Spout", 0.05, 0.40, (-0.70, 0, 0.20), iron, rot=(0,math.pi/2,0))
    # Lever handle
    cube("Lever", (0.50, 0, 0.55), (0.04, 0.04, 0.40), wood, rot=(0, math.radians(-20), 0))
    export_current_as("ForgeBellows", MOON)

def asset_ForgeHammer():
    reset_scene()
    iron = make_material("Fh_Iron", (0.25,0.22,0.20), roughness=0.4, metallic=0.7)
    wood = make_material("Fh_Wood", (0.42,0.28,0.16), roughness=0.7)
    cube("Head", (0,0,0.45), (0.16, 0.10, 0.10), iron)
    cyl("Handle", 0.025, 0.40, (0,0,0.25), wood)
    export_current_as("ForgeHammer", MOON)

def asset_GiantForge():
    reset_scene()
    stone = make_material("Fg_Stone", (0.45,0.42,0.38), roughness=0.85)
    iron = make_material("Fg_Iron", (0.18,0.18,0.20), roughness=0.4, metallic=0.7)
    ember = make_material("Fg_Ember", (1.0,0.4,0.05), roughness=0.2, emission=(1.0,0.4,0.05), emission_strength=5.0)
    # Stone surround
    cube("Body", (0,0,0.8), (2.0, 1.0, 1.6), stone)
    # Hood
    cube("Hood", (0,0.30,2.0), (2.0, 0.4, 0.8), stone, rot=(math.radians(15),0,0))
    # Chimney
    cube("Chimney", (0, 0.15, 3.0), (0.8, 0.8, 1.5), stone)
    # Coal bed
    cube("CoalBed", (0,0,1.5), (1.4, 0.6, 0.10), iron)
    cyl("Embers", 0.50, 0.10, (0, 0.0, 1.60), ember, verts=24)
    export_current_as("GiantForge", MOON)

def asset_IngotMold():
    reset_scene()
    iron = make_material("Im_Iron", (0.25,0.22,0.20), roughness=0.5, metallic=0.6)
    cube("Block", (0,0,0.10), (0.4, 0.2, 0.20), iron)
    # Inset trough
    cube("Trough", (0,0,0.16), (0.30, 0.10, 0.08), make_material("Im_Soot",(0.05,0.05,0.05),0.95))
    export_current_as("IngotMold", MOON)

def asset_AnvilHorn():
    reset_scene()
    iron = make_material("Ah_Iron", (0.18,0.18,0.20), roughness=0.4, metallic=0.7)
    cone("Horn", 0.20, 0.04, 0.60, (0,0,0.30), iron, verts=8)
    cube("Base", (0,0,0.04), (0.25, 0.25, 0.08), iron)
    export_current_as("AnvilHorn", MOON)

for fn in [asset_GiantAnvil, asset_ForgeBellows, asset_ForgeHammer, asset_GiantForge,
           asset_IngotMold, asset_AnvilHorn]:
    fn()
print("[TARTARIA] Moon 4 set done (6 assets).")
