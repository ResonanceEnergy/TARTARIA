"""Moon 10 — Celestial Observatory: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon10"

def asset_CelestialOrrery():
    reset_scene()
    bronze = make_material("Co_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    sun = make_material("Co_Sun", (1.0,0.7,0.2), roughness=0.2, emission=(1.0,0.7,0.2), emission_strength=4.0)
    planet1 = make_material("Co_P1", (0.3,0.5,0.8), roughness=0.5, emission=(0.2,0.4,0.7), emission_strength=1.0)
    planet2 = make_material("Co_P2", (0.7,0.3,0.2), roughness=0.5)
    planet3 = make_material("Co_P3", (0.4,0.6,0.4), roughness=0.5)
    cyl("Base", 0.5, 0.2, (0,0,0.1), bronze, verts=16)
    cyl("Stand", 0.10, 1.0, (0,0,0.7), bronze)
    sphere("Sun", 0.30, (0,0,1.5), sun)
    torus("Orb1", 0.6, 0.02, (0,0,1.5), bronze, mseg=32)
    torus("Orb2", 0.9, 0.02, (0,0,1.5), bronze, mseg=32)
    torus("Orb3", 1.3, 0.02, (0,0,1.5), bronze, mseg=32)
    sphere("Planet1", 0.10, (0.6,0,1.5), planet1)
    sphere("Planet2", 0.13, (0,0.9,1.5), planet2)
    sphere("Planet3", 0.17, (-1.0,0.7,1.5), planet3)
    export_current_as("CelestialOrrery", MOON)

def asset_TelescopeBrass():
    reset_scene()
    brass = make_material("Tb_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    iron = make_material("Tb_Iron", (0.25,0.22,0.20), roughness=0.6, metallic=0.5)
    cyl("Tube", 0.10, 1.5, (0,0,1.4), brass, rot=(math.radians(-30),0,0))
    cyl("Eyepiece", 0.06, 0.15, (0,0.55,2.05), brass, rot=(math.radians(-30),0,0))
    cone("Objective", 0.14, 0.08, 0.12, (0,-0.50,0.85), brass, verts=16, rot=(math.radians(150),0,0))
    # Tripod
    for i in range(3):
        a = i * 2*math.pi/3
        cyl(f"Leg_{i}", 0.03, 1.4, (math.cos(a)*0.3, math.sin(a)*0.3, 0.7), iron, rot=(math.radians(15)*math.cos(a), math.radians(15)*math.sin(a),0))
    cyl("Mount", 0.10, 0.10, (0,0,1.0), iron, verts=12)
    export_current_as("TelescopeBrass", MOON)

def asset_StarMapTable():
    reset_scene()
    wood = make_material("Sm_Wood", (0.32,0.20,0.12), roughness=0.5)
    paper = make_material("Sm_Paper", (0.85,0.78,0.62), roughness=0.85)
    gold = make_material("Sm_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=1.0)
    cube("Top", (0,0,0.8), (1.0,0.7,0.06), wood)
    # 4 legs
    for (x,y) in [(-0.45,-0.3),(0.45,-0.3),(-0.45,0.3),(0.45,0.3)]:
        cube(f"L_{x}_{y}", (x,y,0.4), (0.06,0.06,0.8), wood)
    # Star map sheet
    cube("Map", (0,0,0.84), (0.8,0.55,0.005), paper)
    # 8 star markings
    for i in range(8):
        x = ((i%4)-1.5) * 0.18
        y = ((i//4)-0.5) * 0.30
        sphere(f"Star_{i}", 0.025, (x,y,0.85), gold)
    export_current_as("StarMapTable", MOON)

def asset_ZodiacWheel():
    reset_scene()
    bronze = make_material("Zw_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    gold = make_material("Zw_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85)
    cyl("Disc", 1.0, 0.05, (0,0,0), bronze, verts=24)
    torus("OuterRing", 1.0, 0.04, (0,0,0.03), gold, mseg=32, miseg=6)
    torus("InnerRing", 0.6, 0.03, (0,0,0.03), gold, mseg=24, miseg=4)
    # 12 zodiac markings
    for i in range(12):
        a = i * math.pi/6
        cube(f"Sign_{i}", (math.cos(a)*0.8, math.sin(a)*0.8, 0.04), (0.06,0.06,0.04), gold)
    sphere("Center", 0.10, (0,0,0.05), gold)
    export_current_as("ZodiacWheel", MOON)

def asset_ObservatoryDome():
    reset_scene()
    copper = make_material("Od_Copper", (0.45,0.65,0.50), roughness=0.5, metallic=0.7)
    stone = make_material("Od_Stone", (0.55,0.52,0.48), roughness=0.85)
    cyl("Drum", 2.0, 1.0, (0,0,0.5), stone, verts=20)
    sphere("Dome", 2.0, (0,0,1.0), copper, segs=24, rings=12)
    # Slit (dark line)
    cube("Slit", (0,0,2.4), (0.30, 2.0, 0.02), stone)
    export_current_as("ObservatoryDome", MOON)

for fn in [asset_CelestialOrrery, asset_TelescopeBrass, asset_StarMapTable,
           asset_ZodiacWheel, asset_ObservatoryDome]:
    fn()
print("[TARTARIA] Moon 10 set done (5 assets).")
