"""Moon 7 — Auroral Ring / Sky Shrine: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon7"

def asset_AuroralRing():
    reset_scene()
    aurora_green = make_material("Ar_Green", (0.40,0.85,0.55), roughness=0.1, emission=(0.40,0.85,0.55), emission_strength=4.0)
    aurora_purple = make_material("Ar_Purple", (0.65,0.40,0.85), roughness=0.1, emission=(0.65,0.40,0.85), emission_strength=4.0)
    silver = make_material("Ar_Silver", (0.92,0.92,0.95), roughness=0.2, metallic=0.85)
    torus("Outer", 2.0, 0.12, (0,0,0), silver, mseg=48, miseg=12)
    torus("Mid", 1.6, 0.08, (0,0,0), aurora_green, mseg=48, miseg=12)
    torus("Inner", 1.2, 0.06, (0,0,0), aurora_purple, mseg=48, miseg=12)
    # Floating spheres around
    for i in range(8):
        a = i * math.pi/4
        x,y = math.cos(a)*2.0, math.sin(a)*2.0
        sphere(f"Sph_{i}", 0.10, (x,y,0), aurora_green if i%2==0 else aurora_purple)
    export_current_as("AuroralRing", MOON)

def asset_SkyShrine():
    reset_scene()
    marble = make_material("Ss_Marble", (0.96,0.95,0.92), roughness=0.3)
    aurora = make_material("Ss_Aurora", (0.55,0.85,0.75), roughness=0.1, emission=(0.55,0.85,0.75), emission_strength=2.5)
    cyl("Base", 1.2, 0.30, (0,0,0.15), marble, verts=12)
    # 6 pillars
    for i in range(6):
        a = i*math.pi/3
        x,y = math.cos(a)*0.9, math.sin(a)*0.9
        cyl(f"P_{i}", 0.10, 2.0, (x,y,1.30), marble, verts=10)
    # Dome
    sphere("Dome", 1.0, (0,0,2.3), aurora, segs=20, rings=10)
    export_current_as("SkyShrine", MOON)

def asset_AuroralBeacon():
    reset_scene()
    bronze = make_material("Ab_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    light = make_material("Ab_Light", (0.85,0.92,1.0), roughness=0.05, emission=(0.85,0.92,1.0), emission_strength=6.0)
    cyl("Stand", 0.15, 0.8, (0,0,0.4), bronze, verts=12)
    sphere("Lamp", 0.30, (0,0,0.95), light)
    cone("Reflector", 0.40, 0.10, 0.40, (0,0,1.30), bronze, verts=12, rot=(math.pi,0,0))
    export_current_as("AuroralBeacon", MOON)

def asset_CloudPlatform():
    reset_scene()
    cloud = make_material("Cp_Cloud", (0.98,0.98,1.0), roughness=0.95, emission=(0.85,0.92,1.0), emission_strength=0.4)
    # Lumpy cloud disc
    import random; random.seed(7)
    for i in range(20):
        x = random.uniform(-1.3, 1.3); y = random.uniform(-1.3, 1.3)
        r = random.uniform(0.3, 0.6)
        sphere(f"Cl_{i}", r, (x,y,0), cloud, segs=12, rings=8)
    export_current_as("CloudPlatform", MOON)

def asset_StarBeacon():
    reset_scene()
    gold = make_material("Sb_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=3.0)
    # 6-point star (2 tetrahedra)
    cone("Up", 0.40, 0.0, 0.8, (0,0,0.4), gold, verts=4)
    cone("Down", 0.40, 0.0, 0.8, (0,0,0.4), gold, verts=4, rot=(math.pi,0,0))
    sphere("Core", 0.10, (0,0,0.4), gold)
    export_current_as("StarBeacon", MOON)

for fn in [asset_AuroralRing, asset_SkyShrine, asset_AuroralBeacon, asset_CloudPlatform, asset_StarBeacon]:
    fn()
print("[TARTARIA] Moon 7 set done (5 assets).")
