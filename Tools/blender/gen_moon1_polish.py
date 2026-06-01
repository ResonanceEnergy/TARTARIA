"""Moon 1 polish props — 8 assets: rose window, water font, obelisk, mercury ball variant, milo satchel, watch helm, signpost, lantern."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon1"

def asset_RoseWindowCymatic():
    reset_scene()
    STONE = make_material("RW_Stone", (0.55, 0.50, 0.42), roughness=0.85)
    GOLD  = make_material("RW_Gold",  (0.90, 0.75, 0.30), roughness=0.3, metallic=0.85, emission=(0.95,0.75,0.25), emission_strength=2.0)
    GLASS = make_material("RW_Glass", (0.40, 0.85, 0.95, 0.6), roughness=0.1, emission=(0.40,0.85,0.95), emission_strength=2.5)
    # Outer ring
    torus("OuterRing", 2.0, 0.18, (0,0,0), STONE, mseg=48, miseg=12)
    # 8 spoke divisions
    for i in range(8):
        a = i * math.pi/4
        cube(f"Spoke_{i}", (math.cos(a)*1.1, 0, math.sin(a)*1.1), (0.12, 0.20, 1.9), STONE, rot=(0,a,0))
    # 8 outer petals (glass)
    for i in range(8):
        a = i * math.pi/4 + math.pi/8
        sphere(f"Petal_{i}", 0.38, (math.cos(a)*1.4, 0, math.sin(a)*1.4), GLASS)
    # Center medallion
    cyl("Center", 0.45, 0.12, (0,0,0), GOLD, rot=(math.pi/2,0,0))
    sphere("CenterJewel", 0.18, (0,0.08,0), GLASS)
    export_current_as("RoseWindowCymatic", MOON)

def asset_PureWaterFont():
    reset_scene()
    MARBLE = make_material("Font_Marble", (0.92, 0.90, 0.85), roughness=0.4)
    GOLD   = make_material("Font_Gold", (0.90, 0.75, 0.30), roughness=0.3, metallic=0.85)
    WATER  = make_material("Font_Water", (0.55, 0.85, 0.95), roughness=0.15, emission=(0.55,0.85,0.95), emission_strength=1.2)
    # Octagonal base
    cyl("Base", 0.85, 0.25, (0,0,0.125), MARBLE, verts=8)
    cyl("BaseRing", 0.90, 0.05, (0,0,0.27), GOLD, verts=24)
    # Bowl pedestal
    cyl("PedSh", 0.30, 0.6, (0,0,0.6), MARBLE)
    cyl("PedRing1", 0.34, 0.05, (0,0,0.30), GOLD, verts=24)
    cyl("PedRing2", 0.34, 0.05, (0,0,0.90), GOLD, verts=24)
    # Bowl
    cyl("Bowl", 0.65, 0.22, (0,0,1.05), MARBLE, verts=24)
    cyl("BowlInner", 0.55, 0.18, (0,0,1.10), WATER, verts=24)
    cyl("BowlRim", 0.70, 0.05, (0,0,1.18), GOLD, verts=24)
    # Central spout (small obelisk in middle)
    cone("Spout", 0.10, 0.04, 0.35, (0,0,1.35), GOLD)
    sphere("SpoutDrop", 0.06, (0,0,1.55), WATER)
    export_current_as("PureWaterFont", MOON)

def asset_CarvedStoneObelisk():
    reset_scene()
    STONE = make_material("Obelisk_Stone", (0.50, 0.46, 0.40), roughness=0.85)
    GLYPH = make_material("Obelisk_Glyph", (0.85, 0.70, 0.30), roughness=0.4, emission=(0.85,0.70,0.30), emission_strength=1.0)
    # Stepped base
    cube("Step1", (0,0,0.10), (0.8, 0.8, 0.20), STONE)
    cube("Step2", (0,0,0.30), (0.65, 0.65, 0.20), STONE)
    # Obelisk body (square pillar that tapers)
    cube("Body", (0,0,1.4), (0.30, 0.30, 1.8), STONE)
    # Pyramidion top
    cone("Top", 0.30, 0.0, 0.40, (0,0,2.5), STONE, verts=4)
    # Glyph bands
    for h in [0.8, 1.4, 2.0]:
        cube(f"Glyph_{h}", (0, 0.31, h), (0.22, 0.01, 0.10), GLYPH)
    export_current_as("CarvedStoneObelisk", MOON)

def asset_MercuryBallSpire():
    reset_scene()
    STONE = make_material("Spire_Stone", (0.65, 0.62, 0.58), roughness=0.7)
    MERCURY = make_material("Spire_Mercury", (0.92, 0.92, 0.94), roughness=0.05, metallic=1.0)
    GOLD = make_material("Spire_Gold", (0.90, 0.75, 0.30), roughness=0.3, metallic=0.85, emission=(0.90,0.75,0.25), emission_strength=1.5)
    # Wide base
    cyl("SBase", 0.45, 0.30, (0,0,0.15), STONE, verts=12)
    # Tapered shaft
    cone("Shaft", 0.35, 0.12, 2.5, (0,0,1.55), STONE, verts=12)
    # Bands
    for h in [0.6, 1.2, 1.8, 2.4]:
        r = 0.34 - (h-0.6)*0.08
        torus(f"Band_{h}", r, 0.03, (0,0,h), GOLD, mseg=12, miseg=6)
    # Mercury ball orb
    sphere("MercuryBall", 0.30, (0,0,3.1), MERCURY, segs=32, rings=20)
    # Crown ring on orb
    torus("OrbRing", 0.32, 0.025, (0,0,3.1), GOLD, mseg=24, miseg=6, rot=(math.pi/2,0,0))
    export_current_as("MercuryBallSpireHero", MOON)

def asset_MiloSatchel():
    reset_scene()
    LEATHER = make_material("Satchel_Leather", (0.42, 0.28, 0.18), roughness=0.7)
    BRASS = make_material("Satchel_Brass", (0.80, 0.65, 0.30), roughness=0.4, metallic=0.7)
    GLASS = make_material("Satchel_Glass", (0.95, 0.85, 0.50), roughness=0.1, emission=(0.95,0.78,0.30), emission_strength=2.5)
    # Satchel body
    cube("Bag", (0,0,0.2), (0.25, 0.10, 0.18), LEATHER)
    # Flap
    cube("Flap", (0,-0.10, 0.20), (0.25, 0.02, 0.20), LEATHER)
    # Buckle
    cube("Buckle", (0, -0.11, 0.10), (0.04, 0.02, 0.04), BRASS)
    # Strap
    torus("Strap", 0.35, 0.012, (0, 0.04, 0.40), LEATHER, mseg=24, miseg=6)
    # Hanging lantern
    cyl("LanternBody", 0.07, 0.16, (0.18, 0, 0.08), BRASS, verts=8)
    cyl("LanternGlass", 0.055, 0.12, (0.18, 0, 0.08), GLASS, verts=8)
    cone("LanternCap", 0.08, 0.02, 0.05, (0.18, 0, 0.18), BRASS, verts=8)
    sphere("LanternFlame", 0.025, (0.18, 0, 0.08), GLASS)
    export_current_as("MiloSatchelAndLantern", MOON)

def asset_VillagerSignpost():
    reset_scene()
    WOOD = make_material("Sign_Wood", (0.42, 0.28, 0.16), roughness=0.7)
    PAINT = make_material("Sign_PaintBlack", (0.05, 0.04, 0.03), roughness=0.6)
    METAL = make_material("Sign_Iron", (0.30, 0.28, 0.25), roughness=0.6, metallic=0.5)
    # Post
    cyl("Post", 0.06, 2.4, (0,0,1.2), WOOD)
    # Crossbar
    cube("Cross", (0.3, 0, 2.0), (0.5, 0.04, 0.04), WOOD)
    # Sign board (arrow-shaped — approximate with 2 cubes)
    cube("Board", (0.55, 0, 1.7), (0.5, 0.04, 0.3), WOOD)
    cube("BoardArrow", (0.9, 0, 1.7), (0.2, 0.04, 0.18), WOOD, rot=(0,math.pi/4,0))
    # Nails/rivets
    for x in [0.35, 0.75]:
        sphere(f"Nail_{x}", 0.015, (x, -0.02, 1.7), METAL)
    # Iron strap at base
    torus("Strap", 0.075, 0.012, (0,0,0.15), METAL, mseg=24, miseg=6, rot=(math.pi/2,0,0))
    export_current_as("VillagerSignpost", MOON)

def asset_WallSconce():
    reset_scene()
    IRON = make_material("Sconce_Iron", (0.18, 0.16, 0.14), roughness=0.6, metallic=0.6)
    FLAME = make_material("Sconce_Flame", (1.0, 0.55, 0.20), roughness=0.2, emission=(1.0,0.5,0.15), emission_strength=4.0)
    # Wall plate
    cube("Plate", (0,0,0.6), (0.18, 0.04, 0.50), IRON)
    # Arm extending out
    cyl("Arm", 0.025, 0.25, (0, 0.15, 0.65), IRON, rot=(math.pi/2,0,0))
    # Bowl
    cyl("Bowl", 0.10, 0.06, (0, 0.30, 0.65), IRON)
    sphere("Flame", 0.09, (0, 0.30, 0.72), FLAME)
    # Decorative top
    cone("Finial", 0.04, 0.0, 0.10, (0, 0, 0.92), IRON)
    export_current_as("WallSconceIron", MOON)

def asset_VillageWell():
    reset_scene()
    STONE = make_material("Well_Stone", (0.55, 0.52, 0.48), roughness=0.85)
    WOOD = make_material("Well_Wood", (0.42, 0.28, 0.16), roughness=0.7)
    METAL = make_material("Well_Iron", (0.30, 0.28, 0.25), roughness=0.6, metallic=0.5)
    WATER = make_material("Well_Water", (0.20, 0.40, 0.60), roughness=0.2, emission=(0.2,0.4,0.6), emission_strength=0.4)
    # Circular stone base
    cyl("Base", 0.85, 0.50, (0,0,0.25), STONE, verts=16)
    cyl("Rim", 0.90, 0.10, (0,0,0.55), STONE, verts=24)
    # Water inside (visible at top)
    cyl("Water", 0.70, 0.05, (0,0,0.48), WATER, verts=16)
    # 2 wooden posts
    for sx in [-0.95, 0.95]:
        cube(f"Post_{sx}", (sx, 0, 1.5), (0.10, 0.10, 1.8), WOOD)
    # Crossbeam
    cube("Crossbeam", (0, 0, 2.4), (1.0, 0.12, 0.12), WOOD)
    # Roof (peaked — 2 slabs)
    cube("RoofL", (-0.45, 0, 2.65), (0.6, 1.1, 0.06), WOOD, rot=(0, math.radians(-25), 0))
    cube("RoofR", ( 0.45, 0, 2.65), (0.6, 1.1, 0.06), WOOD, rot=(0, math.radians(25), 0))
    # Bucket on rope
    cyl("Bucket", 0.10, 0.20, (0, 0, 1.6), WOOD, verts=8)
    cyl("BucketHoop", 0.105, 0.03, (0, 0, 1.7), METAL, verts=12)
    cyl("Rope", 0.005, 0.40, (0, 0, 1.9), WOOD)
    export_current_as("VillageWell", MOON)

# Build all
for fn in [asset_RoseWindowCymatic, asset_PureWaterFont, asset_CarvedStoneObelisk,
           asset_MercuryBallSpire, asset_MiloSatchel, asset_VillagerSignpost,
           asset_WallSconce, asset_VillageWell]:
    fn()
print("[TARTARIA] Moon 1 polish set done (8 assets).")
