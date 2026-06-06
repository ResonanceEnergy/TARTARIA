"""Shared utility props — work across many moons: rocks, mushrooms, signs, debris."""
import bpy, sys, os, math, random
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Shared"

def asset_BoulderLarge():
    reset_scene()
    rock = make_material("Bl_Rock", (0.40,0.38,0.35), roughness=0.85)
    sphere("Body", 1.0, (0,0,0.8), rock, segs=12, rings=8)
    sphere("Bump1", 0.4, (0.6,0,0.7), rock, segs=10, rings=8)
    sphere("Bump2", 0.3, (-0.3,0.5,1.1), rock, segs=10, rings=8)
    export_current_as("BoulderLarge", MOON)

def asset_BoulderMed():
    reset_scene()
    rock = make_material("Bm_Rock", (0.45,0.42,0.38), roughness=0.85)
    sphere("Body", 0.5, (0,0,0.4), rock, segs=12, rings=8)
    sphere("B1", 0.2, (0.3,0,0.3), rock, segs=10, rings=8)
    export_current_as("BoulderMed", MOON)

def asset_BoulderSmall():
    reset_scene()
    rock = make_material("Bs_Rock", (0.50,0.46,0.42), roughness=0.85)
    sphere("Body", 0.20, (0,0,0.2), rock, segs=10, rings=8)
    export_current_as("BoulderSmall", MOON)

def asset_MushroomRed():
    reset_scene()
    cap = make_material("Mr_Cap", (0.75,0.15,0.10), roughness=0.4, emission=(0.6,0.10,0.05), emission_strength=0.6)
    stem = make_material("Mr_Stem", (0.95,0.92,0.85), roughness=0.5)
    spot = make_material("Mr_Spot", (1.0,0.98,0.95), roughness=0.4)
    cyl("Stem", 0.05, 0.25, (0,0,0.125), stem, verts=12)
    sphere("Cap", 0.20, (0,0,0.30), cap, segs=16, rings=8)
    # White spots
    for i in range(5):
        a = i * 2*math.pi/5
        sphere(f"Spot_{i}", 0.04, (math.cos(a)*0.13, math.sin(a)*0.13, 0.40), spot)
    export_current_as("MushroomRed", MOON)

def asset_MushroomBlueGlow():
    reset_scene()
    glow = make_material("Mb_Cap", (0.40,0.55,0.85), roughness=0.3, emission=(0.40,0.65,0.95), emission_strength=3.0)
    stem = make_material("Mb_Stem", (0.85,0.90,0.95), roughness=0.5)
    cyl("Stem", 0.03, 0.20, (0,0,0.10), stem, verts=12)
    sphere("Cap", 0.10, (0,0,0.22), glow, segs=12, rings=8)
    export_current_as("MushroomBlueGlow", MOON)

def asset_FallenLog():
    reset_scene()
    bark = make_material("Fl_Bark", (0.32,0.22,0.14), roughness=0.85)
    inner = make_material("Fl_Inner", (0.55,0.40,0.25), roughness=0.7)
    cyl("Log", 0.25, 1.8, (0,0,0.25), bark, verts=16, rot=(0,math.pi/2,0))
    cyl("EndA", 0.25, 0.05, (-0.92,0,0.25), inner, verts=16, rot=(0,math.pi/2,0))
    cyl("EndB", 0.25, 0.05, (0.92,0,0.25), inner, verts=16, rot=(0,math.pi/2,0))
    export_current_as("FallenLog", MOON)

def asset_TreeStump():
    reset_scene()
    bark = make_material("Ts_Bark", (0.32,0.22,0.14), roughness=0.85)
    inner = make_material("Ts_Inner", (0.55,0.40,0.25), roughness=0.7)
    cyl("Trunk", 0.30, 0.50, (0,0,0.25), bark, verts=16)
    cyl("Top", 0.28, 0.04, (0,0,0.52), inner, verts=24)
    export_current_as("TreeStump", MOON)

def asset_RuinedColumn():
    reset_scene()
    stone = make_material("Rc_Stone", (0.52,0.50,0.46), roughness=0.85)
    cyl("Base", 0.30, 0.10, (0,0,0.05), stone, verts=16)
    cyl("Shaft", 0.22, 1.2, (0,0,0.70), stone, verts=16, rot=(math.radians(10),0,0))
    # Broken top — irregular
    cyl("BrokenTop", 0.22, 0.10, (-0.20, 0, 1.30), stone, verts=12, rot=(math.radians(10),0,0))
    export_current_as("RuinedColumn", MOON)

def asset_CrackedFlagstone():
    reset_scene()
    stone = make_material("Cf_Stone", (0.55,0.52,0.48), roughness=0.85)
    cube("Tile", (0,0,0.025), (0.5, 0.5, 0.05), stone)
    cube("CrackA", (0.1,0,0.052), (0.005, 0.4, 0.005), make_material("Cf_Crack",(0.05,0.05,0.05),0.95))
    cube("CrackB", (-0.15,0.1,0.052), (0.005, 0.2, 0.005), make_material("Cf_Crack2",(0.05,0.05,0.05),0.95))
    export_current_as("CrackedFlagstone", MOON)

def asset_AncientStoneSign():
    reset_scene()
    stone = make_material("As_Stone", (0.50,0.46,0.40), roughness=0.85)
    glyph = make_material("As_Glyph", (0.85,0.70,0.30), roughness=0.4, emission=(0.85,0.70,0.30), emission_strength=1.2)
    cube("Stand", (0,0,0.30), (0.15, 0.15, 0.60), stone)
    cube("Head", (0,0,0.75), (0.45, 0.10, 0.30), stone)
    cube("Glyph", (0,0.06,0.75), (0.35, 0.005, 0.20), glyph)
    export_current_as("AncientStoneSign", MOON)

for fn in [asset_BoulderLarge, asset_BoulderMed, asset_BoulderSmall, asset_MushroomRed,
           asset_MushroomBlueGlow, asset_FallenLog, asset_TreeStump, asset_RuinedColumn,
           asset_CrackedFlagstone, asset_AncientStoneSign]:
    fn()
print("[TARTARIA] Shared utility set done (10 assets).")
