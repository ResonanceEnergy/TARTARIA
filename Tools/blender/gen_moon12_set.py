"""Moon 12 — Bell Tower / Scalar Waves: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon12"

def asset_GrandBellTower():
    reset_scene()
    stone = make_material("Bt_Stone", (0.50,0.46,0.40), roughness=0.85)
    brass = make_material("Bt_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cube("Base", (0,0,0.75), (1.5,1.5,1.5), stone)
    cube("Tier1", (0,0,2.5), (1.2,1.2,2.0), stone)
    cube("Tier2", (0,0,5.0), (0.9,0.9,2.0), stone)
    cube("BellChamber", (0,0,7.0), (1.0,1.0,2.0), stone)
    # Bell hanging in chamber
    cyl("Bell", 0.5, 0.6, (0,0,6.8), brass, verts=24)
    sphere("BellTop", 0.15, (0,0,7.20), brass)
    # Pyramid roof
    cone("Roof", 0.8, 0.0, 1.2, (0,0,8.6), stone, verts=4)
    export_current_as("GrandBellTower", MOON)

def asset_GrandBellLarge():
    reset_scene()
    brass = make_material("Gb_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    iron = make_material("Gb_Iron", (0.25,0.22,0.20), roughness=0.5, metallic=0.6)
    cyl("Body", 0.8, 0.9, (0,0,0.45), brass, verts=24)
    sphere("Top", 0.30, (0,0,1.0), brass)
    cyl("Crown", 0.20, 0.10, (0,0,1.20), iron, verts=12)
    cyl("RingTop", 0.10, 0.02, (0,0,1.30), iron, rot=(math.pi/2,0,0))
    cyl("Clapper", 0.06, 0.40, (0,0,0.4), iron)
    export_current_as("GrandBellLarge", MOON)

def asset_ScalarWaveResonator():
    reset_scene()
    bronze = make_material("Sw_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    glow = make_material("Sw_Glow", (0.50,0.85,0.95), roughness=0.2, emission=(0.50,0.85,0.95), emission_strength=4.0)
    cyl("Base", 0.6, 0.15, (0,0,0.075), bronze, verts=16)
    cyl("Shaft", 0.10, 1.5, (0,0,0.9), bronze)
    sphere("Coil", 0.40, (0,0,1.8), glow, segs=20, rings=14)
    for i in range(5):
        a = i*math.pi/2.5
        torus(f"Ring_{i}", 0.45 - i*0.05, 0.02, (0,0,1.8), bronze, mseg=24, miseg=4, rot=(a,0,0))
    export_current_as("ScalarWaveResonator", MOON)

def asset_TempleBellLantern():
    reset_scene()
    paper = make_material("Tb_Paper", (0.95,0.85,0.50), roughness=0.5, emission=(0.95,0.78,0.30), emission_strength=2.0)
    wood = make_material("Tb_Wood", (0.42,0.28,0.16), roughness=0.7)
    cube("Frame", (0,0,0.4), (0.30, 0.30, 0.50), wood)
    cube("PaperBox", (0,0,0.4), (0.27, 0.27, 0.48), paper)
    cone("CapTop", 0.25, 0.05, 0.10, (0,0,0.70), wood, verts=4)
    cyl("Loop", 0.02, 0.005, (0,0,0.78), wood, rot=(math.pi/2,0,0))
    export_current_as("TempleBellLantern", MOON)

def asset_TollerStaff():
    reset_scene()
    wood = make_material("Ts_Wood", (0.42,0.28,0.16), roughness=0.7)
    bronze = make_material("Ts_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    cyl("Staff", 0.02, 2.0, (0,0,1.0), wood)
    sphere("Knob", 0.06, (0,0,2.05), bronze)
    cyl("Wrap", 0.025, 0.20, (0,0,0.5), bronze, verts=12)
    export_current_as("TollerStaff", MOON)

for fn in [asset_GrandBellTower, asset_GrandBellLarge, asset_ScalarWaveResonator,
           asset_TempleBellLantern, asset_TollerStaff]:
    fn()
print("[TARTARIA] Moon 12 set done (5 assets).")
