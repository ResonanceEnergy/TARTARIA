"""Moon 5 — White City: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon5"

def asset_WhiteCitySpire():
    reset_scene()
    marble = make_material("Wc_Marble", (0.96,0.95,0.92), roughness=0.3)
    gold = make_material("Wc_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=1.5)
    crystal = make_material("Wc_Crystal", (0.85,0.92,1.0), roughness=0.1, emission=(0.85,0.92,1.0), emission_strength=2.5)
    # Base
    cyl("Base", 0.9, 0.30, (0,0,0.15), marble, verts=16)
    cyl("Tier1", 0.6, 0.6, (0,0,0.6), marble, verts=12)
    cyl("Tier2", 0.45, 0.6, (0,0,1.20), marble, verts=12)
    cone("Shaft", 0.40, 0.10, 2.5, (0,0,2.75), marble, verts=12)
    # Gold bands
    for h in [0.5, 1.0, 1.5, 2.0, 2.5]:
        torus(f"B_{h}", 0.40 - (h-0.5)*0.1, 0.025, (0,0,h+0.6), gold, mseg=24, miseg=6)
    sphere("Apex", 0.18, (0,0,4.2), crystal)
    cone("Pinnacle", 0.08, 0.0, 0.3, (0,0,4.45), gold)
    export_current_as("WhiteCitySpire", MOON)

def asset_AlabasterColumn():
    reset_scene()
    marble = make_material("Ac_Marble", (0.96,0.95,0.92), roughness=0.3)
    cyl("Base", 0.20, 0.10, (0,0,0.05), marble, verts=16)
    cyl("Shaft", 0.16, 2.0, (0,0,1.10), marble, verts=20)
    cyl("Capital", 0.22, 0.15, (0,0,2.20), marble, verts=24)
    export_current_as("AlabasterColumn", MOON)

def asset_DomedRotunda():
    reset_scene()
    marble = make_material("Dr_Marble", (0.96,0.95,0.92), roughness=0.3)
    gold = make_material("Dr_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85)
    # Cylindrical drum
    cyl("Drum", 1.5, 1.5, (0,0,0.75), marble, verts=20)
    # Hemisphere dome
    sphere("Dome", 1.5, (0,0,1.5), marble, segs=24, rings=12)
    # Gold band
    torus("DrumRing", 1.50, 0.05, (0,0,1.5), gold, mseg=24, miseg=8)
    # Oculus
    cyl("Oculus", 0.30, 0.08, (0,0,2.95), gold, verts=16)
    export_current_as("DomedRotunda", MOON)

def asset_ResonanceAltar():
    reset_scene()
    marble = make_material("Ra_Marble", (0.96,0.95,0.92), roughness=0.3)
    gold = make_material("Ra_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=1.5)
    cube("Base", (0,0,0.30), (0.8,0.6,0.6), marble)
    cube("Top", (0,0,0.65), (0.85,0.65,0.05), marble)
    cube("GoldBand", (0,0,0.65), (0.86,0.66,0.03), gold)
    sphere("Globe", 0.15, (0,0,0.85), gold)
    export_current_as("ResonanceAltar", MOON)

def asset_VictoryArch():
    reset_scene()
    marble = make_material("Va_Marble", (0.96,0.95,0.92), roughness=0.3)
    gold = make_material("Va_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85)
    # 2 pillars
    cube("PillarL", (-1.5, 0, 1.5), (0.4, 0.4, 3.0), marble)
    cube("PillarR", ( 1.5, 0, 1.5), (0.4, 0.4, 3.0), marble)
    # Top architrave
    cube("Top", (0, 0, 3.15), (3.4, 0.5, 0.3), marble)
    cube("Frieze", (0, 0, 3.50), (3.0, 0.4, 0.1), gold)
    # Arch keystone
    torus("Arch", 1.0, 0.15, (0, 0, 1.8), marble, mseg=20, miseg=6, rot=(math.pi/2,0,0))
    export_current_as("VictoryArch", MOON)

for fn in [asset_WhiteCitySpire, asset_AlabasterColumn, asset_DomedRotunda,
           asset_ResonanceAltar, asset_VictoryArch]:
    fn()
print("[TARTARIA] Moon 5 set done (5 assets).")
