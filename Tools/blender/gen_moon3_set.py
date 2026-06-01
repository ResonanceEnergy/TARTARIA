"""Moon 3 — Electric Moon: Compassion & Rails. 6 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon3"

def asset_OrphanTrainCar():
    reset_scene()
    iron = make_material("Ot_Iron", (0.18,0.18,0.22), roughness=0.5, metallic=0.6)
    wood = make_material("Ot_Wood", (0.42,0.28,0.16), roughness=0.7)
    window = make_material("Ot_Window", (0.85,0.85,0.50), roughness=0.1, emission=(0.85,0.78,0.40), emission_strength=2.5)
    # Body
    cube("Body", (0,0,1.2), (2.0, 0.8, 0.8), iron)
    # Wood paneling
    cube("Panel", (0,0,1.2), (2.0, 0.82, 0.4), wood)
    # Windows
    for x in [-0.6, 0.0, 0.6]:
        for sy in [-0.82, 0.82]:
            cube(f"W_{x}_{sy}", (x, sy, 1.35), (0.35, 0.005, 0.30), window)
    # 4 wheels
    for x in [-0.7, 0.7]:
        for sy in [-0.45, 0.45]:
            cyl(f"Wheel_{x}_{sy}", 0.30, 0.10, (x, sy, 0.35), iron, rot=(math.pi/2,0,0), verts=20)
    # Roof + chimney
    cube("Roof", (0,0,1.7), (2.05, 0.85, 0.10), iron)
    cyl("Chimney", 0.10, 0.40, (-0.7, 0, 1.95), iron, verts=12)
    export_current_as("OrphanTrainCar", MOON)

def asset_MercurialPool():
    reset_scene()
    mercury = make_material("Mp_Mercury", (0.88, 0.88, 0.92), roughness=0.05, metallic=1.0)
    stone = make_material("Mp_Stone", (0.45,0.42,0.38), roughness=0.85)
    # Outer rim
    torus("Rim", 1.5, 0.25, (0,0,0.0), stone, mseg=32, miseg=12)
    # Mercury surface
    cyl("Surface", 1.45, 0.04, (0,0,-0.05), mercury, verts=32)
    # Bubble drops
    for i in range(8):
        a = i * math.pi/4
        sphere(f"Drop_{i}", 0.10, (math.cos(a)*0.8, math.sin(a)*0.8, 0.05), mercury, segs=16, rings=12)
    export_current_as("MercurialPool", MOON)

def asset_RailTrack():
    reset_scene()
    rail = make_material("Rl_Iron", (0.30,0.30,0.32), roughness=0.5, metallic=0.6)
    tie = make_material("Rl_Tie", (0.30,0.22,0.15), roughness=0.85)
    # 8 ties + 2 rails
    for i in range(8):
        z_off = (i-3.5) * 0.5
        cube(f"Tie_{i}", (z_off, 0, 0.04), (0.20, 0.80, 0.08), tie, rot=(0,0,math.pi/2))
    cyl("RailL", 0.04, 4.0, (0, -0.30, 0.12), rail, rot=(math.pi/2,0,math.pi/2), verts=8)
    cyl("RailR", 0.04, 4.0, (0,  0.30, 0.12), rail, rot=(math.pi/2,0,math.pi/2), verts=8)
    export_current_as("RailTrackSegment", MOON)

def asset_LullabyCrystal():
    reset_scene()
    crystal = make_material("Lc_Crystal", (0.85,0.78,0.95), roughness=0.15, emission=(0.85,0.78,0.95), emission_strength=3.0)
    silver = make_material("Lc_Silver", (0.92,0.92,0.95), roughness=0.2, metallic=0.85)
    cone("Top", 0.30, 0.0, 1.0, (0,0,0.8), crystal, verts=8)
    cone("Bot", 0.30, 0.0, 0.7, (0,0,0.1), crystal, verts=8, rot=(math.pi,0,0))
    torus("Ring", 0.32, 0.025, (0,0,0.4), silver, mseg=20, miseg=6)
    export_current_as("LullabyCrystal", MOON)

def asset_OrphanChildBust():
    reset_scene()
    bronze = make_material("Ob_Bronze", (0.50,0.35,0.20), roughness=0.4, metallic=0.7)
    stone = make_material("Ob_Stone", (0.55,0.52,0.48), roughness=0.85)
    cube("Plinth", (0,0,0.25), (0.4,0.4,0.5), stone)
    cube("PlinthTop", (0,0,0.55), (0.5,0.5,0.05), stone)
    # Bust head + shoulders
    sphere("Head", 0.18, (0,0,0.85), bronze)
    cube("Shoulders", (0,0,0.65), (0.30, 0.18, 0.18), bronze)
    export_current_as("OrphanChildBust", MOON)

def asset_CymaticGardenBed():
    reset_scene()
    stone = make_material("Cg_Stone", (0.50,0.46,0.40), roughness=0.85)
    plant = make_material("Cg_Plant", (0.30,0.55,0.25), roughness=0.7, emission=(0.3,0.5,0.2), emission_strength=0.3)
    flower = make_material("Cg_Flower", (0.85,0.30,0.65), roughness=0.4, emission=(0.85,0.30,0.65), emission_strength=1.0)
    # Octagonal bed
    cyl("Bed", 1.2, 0.20, (0,0,0.10), stone, verts=8)
    cyl("Soil", 1.0, 0.04, (0,0,0.22), make_material("Cg_Soil", (0.30,0.20,0.12),0.85), verts=24)
    # 12 plants
    import random; random.seed(3)
    for i in range(12):
        a = i * math.pi/6
        x, y = math.cos(a)*0.7, math.sin(a)*0.7
        cyl(f"Stem_{i}", 0.015, 0.30, (x,y,0.35), plant)
        sphere(f"Flower_{i}", 0.06, (x,y,0.55), flower)
    export_current_as("CymaticGardenBed", MOON)

for fn in [asset_OrphanTrainCar, asset_MercurialPool, asset_RailTrack,
           asset_LullabyCrystal, asset_OrphanChildBust, asset_CymaticGardenBed]:
    fn()
print("[TARTARIA] Moon 3 set done (6 assets).")
