"""Moon 11 — Planetary Nexus: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon11"

def asset_PlanetaryNexusGlobe():
    reset_scene()
    earth = make_material("Pn_Earth", (0.3,0.5,0.4), roughness=0.5, emission=(0.2,0.5,0.4), emission_strength=0.8)
    grid = make_material("Pn_Grid", (0.85,0.65,0.20), roughness=0.3, emission=(0.85,0.65,0.20), emission_strength=2.0)
    bronze = make_material("Pn_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    sphere("Globe", 1.5, (0,0,1.8), earth, segs=32, rings=24)
    torus("Equator", 1.50, 0.025, (0,0,1.8), grid, mseg=32, miseg=4)
    torus("Meridian", 1.50, 0.025, (0,0,1.8), grid, mseg=32, miseg=4, rot=(0,math.pi/2,0))
    cyl("Stand", 0.2, 0.4, (0,0,0.2), bronze)
    torus("Cradle", 1.5, 0.03, (0,0,1.8), bronze, mseg=24, miseg=4, rot=(math.pi/4,0,0))
    export_current_as("PlanetaryNexusGlobe", MOON)

def asset_LeyLineNode():
    reset_scene()
    crystal = make_material("Ll_Crystal", (0.85,0.65,0.20), roughness=0.3, emission=(0.85,0.65,0.20), emission_strength=4.0)
    stone = make_material("Ll_Stone", (0.45,0.42,0.38), roughness=0.85)
    # Hexagonal base
    cyl("Base", 0.6, 0.20, (0,0,0.1), stone, verts=6)
    cone("Crystal", 0.30, 0.0, 1.0, (0,0,0.7), crystal, verts=6)
    torus("Ring", 0.35, 0.04, (0,0,0.5), crystal, mseg=20, miseg=4)
    export_current_as("LeyLineNode", MOON)

def asset_WaterGridChannel():
    reset_scene()
    stone = make_material("Wc_Stone", (0.45,0.42,0.38), roughness=0.85)
    water = make_material("Wc_Water", (0.20,0.50,0.75), roughness=0.15, emission=(0.20,0.50,0.75), emission_strength=1.5)
    cube("ChannelBody", (0,0,0.15), (2.0, 0.4, 0.30), stone)
    cube("Water", (0,0,0.20), (1.95, 0.30, 0.05), water)
    cube("LipL", (0,-0.20,0.30), (2.0, 0.02, 0.05), stone)
    cube("LipR", (0,0.20,0.30), (2.0, 0.02, 0.05), stone)
    export_current_as("WaterGridChannel", MOON)

def asset_NexusObelisk():
    reset_scene()
    crystal = make_material("No_Crystal", (0.40,0.85,0.95), roughness=0.2, emission=(0.40,0.85,0.95), emission_strength=2.5)
    stone = make_material("No_Stone", (0.50,0.46,0.40), roughness=0.85)
    cube("Base", (0,0,0.15), (0.8, 0.8, 0.30), stone)
    cube("Body", (0,0,2.0), (0.40, 0.40, 3.5), crystal)
    cone("Top", 0.40, 0.0, 0.5, (0,0,4.0), crystal, verts=4)
    export_current_as("NexusObelisk", MOON)

def asset_GridIntersection():
    reset_scene()
    bronze = make_material("Gi_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    glow = make_material("Gi_Glow", (0.85,0.65,0.20), roughness=0.3, emission=(0.85,0.65,0.20), emission_strength=3.0)
    cyl("Pad", 0.8, 0.10, (0,0,0.05), bronze, verts=24)
    # Cross
    cube("BarX", (0,0,0.10), (1.5, 0.10, 0.05), glow)
    cube("BarY", (0,0,0.10), (0.10, 1.5, 0.05), glow)
    sphere("Center", 0.15, (0,0,0.15), glow)
    export_current_as("GridIntersection", MOON)

for fn in [asset_PlanetaryNexusGlobe, asset_LeyLineNode, asset_WaterGridChannel,
           asset_NexusObelisk, asset_GridIntersection]:
    fn()
print("[TARTARIA] Moon 11 set done (5 assets).")
