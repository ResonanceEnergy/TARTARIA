"""Moon 13 — Throne of Seven: 6 assets — final crescendo."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon13"

def asset_ThroneOfSeven():
    reset_scene()
    gold = make_material("To_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=2.0)
    marble = make_material("To_Marble", (0.96,0.95,0.92), roughness=0.3)
    crystal = make_material("To_Crystal", (0.85,0.92,1.0), roughness=0.1, emission=(0.85,0.92,1.0), emission_strength=3.0)
    # Throne base
    cube("Base", (0,0,0.5), (2.5, 2.0, 1.0), marble)
    cube("Seat", (0,0,1.05), (2.0, 1.5, 0.10), gold)
    cube("Back", (0,-0.7,2.5), (2.0, 0.20, 2.8), gold)
    # 7 crystal spires on the back
    for i in range(7):
        off = (i-3) * 0.30
        h = 1.0 + (3 - abs(i-3))*0.4
        cone(f"Spire_{i}", 0.10, 0.0, h, (off, -0.80, 3.5+h/2), crystal, verts=6)
    # Armrests
    cube("ArmL", (-0.95, 0.20, 1.30), (0.20, 1.0, 0.30), gold)
    cube("ArmR", ( 0.95, 0.20, 1.30), (0.20, 1.0, 0.30), gold)
    export_current_as("ThroneOfSeven", MOON)

def asset_CrownOfResonance():
    reset_scene()
    gold = make_material("Cr_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=2.0)
    gem = make_material("Cr_Gem", (0.40,0.85,0.95), roughness=0.1, emission=(0.40,0.85,0.95), emission_strength=4.0)
    # Crown band
    cyl("Band", 0.40, 0.15, (0,0,0.075), gold, verts=24)
    # 7 spires (varying height)
    for i in range(7):
        a = i * 2*math.pi/7
        x,y = math.cos(a)*0.40, math.sin(a)*0.40
        h = 0.20 + (i%3)*0.05
        cone(f"Sp_{i}", 0.04, 0.0, h, (x, y, 0.15+h/2), gold, verts=4)
        sphere(f"Gem_{i}", 0.025, (x, y, 0.15+h+0.02), gem)
    export_current_as("CrownOfResonance", MOON)

def asset_PlanetaryGrid():
    reset_scene()
    bronze = make_material("Pg_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    glow = make_material("Pg_Glow", (0.40,0.85,0.55), roughness=0.2, emission=(0.40,0.85,0.55), emission_strength=4.0)
    sphere("Globe", 1.0, (0,0,1.2), bronze, segs=20, rings=14)
    # Grid lines (latitude + longitude)
    for i in range(6):
        a = i * math.pi/6
        torus(f"Lat_{i}", 1.02, 0.015, (0,0,1.2), glow, mseg=24, miseg=4, rot=(a,0,0))
    for i in range(6):
        a = i * math.pi/6
        torus(f"Lon_{i}", 1.02, 0.015, (0,0,1.2), glow, mseg=24, miseg=4, rot=(0,a,0))
    # Stand
    cyl("Stand", 0.15, 0.4, (0,0,0.2), bronze)
    cyl("StandBase", 0.40, 0.06, (0,0,0.03), bronze, verts=16)
    export_current_as("PlanetaryGrid", MOON)

def asset_ResonanceLion():
    reset_scene()
    bronze = make_material("Rl_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    cube("Body", (0,0,0.6), (0.8, 0.3, 0.5), bronze)
    sphere("Head", 0.22, (0,0.45,0.75), bronze)
    # Mane — torus
    torus("Mane", 0.25, 0.10, (0,0.45,0.75), bronze, mseg=20, miseg=8)
    # 4 paws
    for (sx, sy) in [(-0.25, -0.20),(0.25,-0.20),(-0.25,0.20),(0.25,0.20)]:
        cube(f"Paw_{sx}_{sy}", (sx, sy, 0.20), (0.10, 0.10, 0.40), bronze)
    cube("Tail", (0,-0.45,0.7), (0.05, 0.30, 0.05), bronze, rot=(math.radians(30),0,0))
    export_current_as("ResonanceLion", MOON)

def asset_GoldenBoughTree():
    reset_scene()
    gold = make_material("Gb_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=1.5)
    bark = make_material("Gb_Bark", (0.32,0.20,0.12), roughness=0.7)
    cyl("Trunk", 0.20, 2.0, (0,0,1.0), bark, verts=12)
    # Spreading canopy made of 8 spheres
    import random; random.seed(13)
    for i in range(8):
        a = random.uniform(0, 2*math.pi)
        r = random.uniform(0.5, 0.9)
        sphere(f"Lf_{i}", random.uniform(0.30, 0.50), (math.cos(a)*r, math.sin(a)*r, 2.2 + random.uniform(-0.2, 0.4)), gold)
    export_current_as("GoldenBoughTree", MOON)

def asset_ThirteenthCrescendoOrb():
    reset_scene()
    light = make_material("Tc_Light", (1.0,1.0,0.95), roughness=0.05, emission=(1.0,0.98,0.85), emission_strength=8.0)
    gold = make_material("Tc_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85)
    sphere("Core", 0.5, (0,0,1.0), light, segs=32, rings=24)
    # 7 orbital rings
    for i in range(7):
        a = i * math.pi/7
        torus(f"Ring_{i}", 0.6 + i*0.04, 0.015, (0,0,1.0), gold, mseg=32, miseg=4, rot=(a, a*0.3, 0))
    cyl("Pedestal", 0.30, 0.5, (0,0,0.25), gold, verts=8)
    cyl("PedestalBase", 0.60, 0.10, (0,0,0.05), gold, verts=16)
    export_current_as("ThirteenthCrescendoOrb", MOON)

for fn in [asset_ThroneOfSeven, asset_CrownOfResonance, asset_PlanetaryGrid,
           asset_ResonanceLion, asset_GoldenBoughTree, asset_ThirteenthCrescendoOrb]:
    fn()
print("[TARTARIA] Moon 13 set done (6 assets).")
