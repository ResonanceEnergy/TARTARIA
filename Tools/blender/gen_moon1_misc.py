"""Moon 1 lighting + containers — 10 assets: lanterns, candles, barrels, urns, crates."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon1"

def asset_HangingLantern():
    reset_scene()
    iron = make_material("HL_Iron", (0.25,0.22,0.20), roughness=0.6, metallic=0.5)
    glass = make_material("HL_Glass", (0.95,0.85,0.50), roughness=0.1, emission=(1.0,0.78,0.30), emission_strength=3.0)
    cone("Top", 0.10, 0.04, 0.12, (0,0,0.85), iron, verts=6)
    cyl("Frame", 0.09, 0.30, (0,0,0.65), iron, verts=6)
    cyl("Glass", 0.075, 0.26, (0,0,0.65), glass, verts=6)
    sphere("Flame", 0.05, (0,0,0.65), glass)
    cyl("Loop", 0.018, 0.005, (0,0,0.95), iron, rot=(math.pi/2,0,0))
    export_current_as("HangingLantern", MOON)

def asset_TableLantern():
    reset_scene()
    brass = make_material("TL_Brass", (0.80,0.65,0.30), roughness=0.4, metallic=0.7)
    glass = make_material("TL_Glass", (0.95,0.85,0.50), roughness=0.1, emission=(1.0,0.78,0.30), emission_strength=3.0)
    cyl("Base", 0.10, 0.04, (0,0,0.02), brass, verts=12)
    cyl("Shaft", 0.025, 0.10, (0,0,0.10), brass)
    cone("Cone", 0.07, 0.04, 0.08, (0,0,0.18), brass, verts=12)
    sphere("Globe", 0.07, (0,0,0.22), glass)
    cyl("Handle", 0.015, 0.03, (0,0,0.32), brass)
    export_current_as("TableLantern", MOON)

def asset_CandleSet():
    reset_scene()
    wax = make_material("Cand_Wax", (0.92, 0.88, 0.75), roughness=0.5, emission=(1.0,0.85,0.50), emission_strength=2.0)
    iron = make_material("Cand_Iron", (0.25,0.22,0.20), roughness=0.6, metallic=0.5)
    # Triple candelabra
    cyl("StandShaft", 0.025, 0.25, (0,0,0.125), iron)
    cyl("StandBase", 0.12, 0.02, (0,0,0.01), iron, verts=12)
    # 3 cup holders
    for i in range(3):
        a = i * 2*math.pi/3
        x, y = math.cos(a)*0.10, math.sin(a)*0.10
        cyl(f"Cup_{i}", 0.03, 0.02, (x, y, 0.27), iron, verts=8)
        cyl(f"Candle_{i}", 0.025, 0.10, (x, y, 0.33), wax, verts=8)
        sphere(f"Flame_{i}", 0.018, (x, y, 0.40), wax)
    export_current_as("CandelabraTriple", MOON)

def asset_WoodenBarrel():
    reset_scene()
    oak = make_material("B_Oak", (0.42,0.28,0.16), roughness=0.7)
    iron = make_material("B_Iron", (0.30,0.28,0.25), roughness=0.6, metallic=0.5)
    cyl("Body", 0.30, 0.70, (0,0,0.40), oak, verts=12)
    # Iron hoops
    for h in [0.10, 0.40, 0.70]:
        torus(f"Hoop_{h}", 0.31, 0.012, (0,0,h), iron, mseg=20, miseg=6)
    # Top
    cyl("Top", 0.28, 0.03, (0,0,0.78), oak, verts=12)
    export_current_as("WoodenBarrel", MOON)

def asset_WoodenCrate():
    reset_scene()
    oak = make_material("C_Oak", (0.42,0.28,0.16), roughness=0.7)
    iron = make_material("C_Iron", (0.30,0.28,0.25), roughness=0.6, metallic=0.5)
    cube("Body", (0,0,0.25), (0.50, 0.50, 0.50), oak)
    # Plank divisions on each face
    for f in [(0.51, 0, 0.25, 0.02, 0.50, 0.50, "FrontPlank"),
              (-0.51, 0, 0.25, 0.02, 0.50, 0.50, "BackPlank"),
              (0, 0.51, 0.25, 0.50, 0.02, 0.50, "RightPlank"),
              (0, -0.51, 0.25, 0.50, 0.02, 0.50, "LeftPlank")]:
        x,y,z,sx,sy,sz,name = f
        # 3 horizontal planks per face
        for ph in [0.15, 0.30, 0.45]:
            if "Front" in name or "Back" in name:
                cube(f"{name}_{ph}", (x*1.01, y, ph - 0.10), (0.02, sy*0.9, 0.04), oak)
            else:
                cube(f"{name}_{ph}", (x, y*1.01, ph - 0.10), (sx*0.9, 0.02, 0.04), oak)
    # Corner brackets
    for (x,y) in [(-0.25,-0.25),(0.25,-0.25),(-0.25,0.25),(0.25,0.25)]:
        cube(f"Bracket_{x}_{y}", (x, y, 0.25), (0.03, 0.03, 0.55), iron)
    export_current_as("WoodenCrate", MOON)

def asset_ClayUrn():
    reset_scene()
    clay = make_material("U_Clay", (0.55, 0.38, 0.25), roughness=0.75)
    cyl("Base", 0.12, 0.04, (0,0,0.02), clay, verts=16)
    sphere("Belly", 0.25, (0,0,0.30), clay, segs=24, rings=18)
    cyl("Neck", 0.10, 0.10, (0,0,0.60), clay, verts=16)
    cyl("Rim", 0.13, 0.04, (0,0,0.67), clay, verts=24)
    # Two handles
    for sx in [-0.20, 0.20]:
        torus(f"Handle_{sx}", 0.08, 0.015, (sx,0,0.40), clay, mseg=16, miseg=6, rot=(0,math.pi/2,0))
    export_current_as("ClayUrn", MOON)

def asset_GrainSack():
    reset_scene()
    burlap = make_material("Sack_Burlap", (0.60,0.50,0.32), roughness=0.95)
    rope = make_material("Sack_Rope", (0.40,0.30,0.18), roughness=0.85)
    # Bulging shape
    sphere("Body", 0.30, (0,0,0.30), burlap, segs=24, rings=12)
    cyl("Neck", 0.08, 0.10, (0,0,0.50), burlap)
    torus("RopeKnot", 0.085, 0.015, (0,0,0.50), rope, mseg=16, miseg=6)
    cube("Tie", (0, 0.10, 0.55), (0.02, 0.02, 0.08), rope, rot=(math.pi/8,0,0))
    export_current_as("GrainSack", MOON)

def asset_MetalBucket():
    reset_scene()
    iron = make_material("Buck_Iron", (0.35,0.32,0.28), roughness=0.5, metallic=0.6)
    cyl("Body", 0.12, 0.20, (0,0,0.10), iron, verts=16)
    cyl("Rim", 0.13, 0.02, (0,0,0.20), iron, verts=24)
    # Handle arc — torus half
    torus("Handle", 0.13, 0.012, (0,0,0.30), iron, mseg=20, miseg=6, rot=(math.pi/2,0,0))
    export_current_as("MetalBucket", MOON)

def asset_TorchOnPost():
    reset_scene()
    wood = make_material("Torch_Wood", (0.42,0.28,0.16), roughness=0.7)
    cloth = make_material("Torch_Cloth", (0.65,0.50,0.30), roughness=0.85)
    flame = make_material("Torch_Flame", (1.0,0.55,0.18), roughness=0.2, emission=(1.0,0.50,0.15), emission_strength=4.0)
    cyl("Post", 0.06, 2.0, (0,0,1.0), wood)
    cyl("CrossBracket", 0.015, 0.30, (0, 0.15, 2.05), wood, rot=(math.pi/2,0,0))
    cyl("TorchShaft", 0.025, 0.40, (0, 0.30, 2.20), wood)
    cyl("ClothWrap", 0.05, 0.10, (0, 0.30, 2.45), cloth, verts=8)
    sphere("Flame", 0.10, (0, 0.30, 2.55), flame)
    export_current_as("TorchOnPost", MOON)

def asset_StoneFireBrazier():
    reset_scene()
    stone = make_material("Br_Stone", (0.50,0.46,0.40), roughness=0.85)
    ember = make_material("Br_Ember", (1.0,0.4,0.1), roughness=0.3, emission=(1.0,0.4,0.1), emission_strength=3.0)
    cyl("Base", 0.28, 0.10, (0,0,0.05), stone, verts=12)
    cyl("Shaft", 0.10, 0.50, (0,0,0.30), stone, verts=8)
    cyl("Bowl", 0.25, 0.10, (0,0,0.55), stone, verts=16)
    cyl("BowlRim", 0.27, 0.03, (0,0,0.60), stone, verts=24)
    sphere("Embers", 0.18, (0,0,0.62), ember)
    export_current_as("StoneFireBrazier", MOON)

for fn in [asset_HangingLantern, asset_TableLantern, asset_CandleSet, asset_WoodenBarrel,
           asset_WoodenCrate, asset_ClayUrn, asset_GrainSack, asset_MetalBucket,
           asset_TorchOnPost, asset_StoneFireBrazier]:
    fn()
print("[TARTARIA] Moon 1 misc set done (10 assets).")
