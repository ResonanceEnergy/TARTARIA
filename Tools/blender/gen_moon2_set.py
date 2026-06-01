"""Moon 2 — Crystal Caverns + Dissonance — 8 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon2"

def asset_DissonanceCrystalBlack():
    reset_scene()
    mat = make_material("DC_Black", (0.05,0.05,0.08), roughness=0.3, metallic=0.5, emission=(0.4,0.05,0.4), emission_strength=1.5)
    cone("Spike1", 0.20, 0.0, 1.4, (0,0,0.7), mat, verts=4)
    cone("Spike2", 0.10, 0.0, 0.8, (0.15,0.15,0.4), mat, verts=4, rot=(0.3,0,0))
    cone("Spike3", 0.08, 0.0, 0.6, (-0.10,0.20,0.3), mat, verts=4, rot=(-0.2,0.3,0))
    cone("Spike4", 0.07, 0.0, 0.5, (-0.20,-0.10,0.25), mat, verts=4, rot=(0.4,-0.2,0))
    export_current_as("DissonanceCrystal_Black", MOON)

def asset_DissonanceCrystalRed():
    reset_scene()
    mat = make_material("DC_Red", (0.45,0.10,0.10), roughness=0.25, emission=(0.9,0.10,0.10), emission_strength=3.0)
    cone("Spike1", 0.18, 0.0, 1.2, (0,0,0.6), mat, verts=4)
    cone("Spike2", 0.10, 0.0, 0.6, (0.20,0,0.3), mat, verts=4, rot=(0,0.3,0))
    cone("Spike3", 0.10, 0.0, 0.6, (-0.20,0,0.3), mat, verts=4, rot=(0,-0.3,0))
    export_current_as("DissonanceCrystal_Red", MOON)

def asset_DissonanceCrystalGreen():
    reset_scene()
    mat = make_material("DC_Green", (0.20,0.40,0.20), roughness=0.3, emission=(0.20,0.85,0.20), emission_strength=2.5)
    sphere("Core", 0.20, (0,0,0.4), mat, segs=12, rings=8)
    for i in range(6):
        a = i * math.pi/3
        cone(f"S{i}", 0.06, 0.0, 0.4, (math.cos(a)*0.3, math.sin(a)*0.3, 0.4), mat, verts=4)
    export_current_as("DissonanceCrystal_Green", MOON)

def asset_CavernWallSegment():
    reset_scene()
    rock = make_material("Cav_Rock", (0.35, 0.32, 0.42), roughness=0.85)
    crystal = make_material("Cav_Crystal", (0.50, 0.70, 0.90), roughness=0.2, emission=(0.30,0.50,0.85), emission_strength=1.5)
    # Wall block
    cube("Wall", (0,0,1.5), (3.0, 0.30, 3.0), rock)
    # Embedded crystals scattered on face
    import random; random.seed(2)
    for i in range(15):
        x = random.uniform(-1.3, 1.3); z = random.uniform(0.3, 2.7)
        h = random.uniform(0.10, 0.40)
        cone(f"C{i}", random.uniform(0.04,0.10), 0.0, h, (x, 0.16, z), crystal, verts=4, rot=(random.uniform(0,0.5), 0, random.uniform(0,3.14)))
    export_current_as("CavernWallCrystals", MOON)

def asset_CrystalThrone():
    reset_scene()
    obsidian = make_material("Th_Obsidian", (0.06, 0.05, 0.08), roughness=0.3, metallic=0.5)
    crystal = make_material("Th_Crystal", (0.55, 0.20, 0.65), roughness=0.25, emission=(0.55,0.20,0.65), emission_strength=2.0)
    cube("Seat", (0,0,0.55), (1.2, 1.0, 0.10), obsidian)
    cube("Back", (0,-0.45,1.5), (1.2, 0.10, 1.8), obsidian)
    # Side crystals
    for sx in [-0.55, 0.55]:
        cone(f"SideCrystal_{sx}", 0.18, 0.0, 1.5, (sx, -0.30, 1.2), crystal, verts=6)
    # Crown of spires on back
    for i, off in enumerate([-0.4, -0.2, 0, 0.2, 0.4]):
        cone(f"Spire_{i}", 0.08, 0.0, 0.6 + abs(off)*0.5, (off, -0.50, 2.4), crystal, verts=6)
    export_current_as("CrystalThrone", MOON)

def asset_MicroGiantPortal():
    reset_scene()
    stone = make_material("MGP_Stone", (0.50,0.46,0.40), roughness=0.85)
    portal = make_material("MGP_Portal", (0.60, 0.40, 0.85), roughness=0.15, emission=(0.60,0.40,0.85), emission_strength=4.0)
    # Archway frame
    torus("Arch", 1.0, 0.15, (0,0,1.0), stone, mseg=24, miseg=8)
    # Inner portal disc
    cyl("Portal", 0.85, 0.05, (0,0,1.0), portal, rot=(math.pi/2,0,0))
    # Base steps
    cube("Step1", (0,0,0.10), (1.4, 0.6, 0.20), stone)
    cube("Step2", (0,0,0.30), (1.0, 0.4, 0.20), stone)
    export_current_as("MicroGiantPortal", MOON)

def asset_StalactiteCluster():
    reset_scene()
    rock = make_material("Sc_Rock", (0.35,0.30,0.28), roughness=0.85)
    drop = make_material("Sc_Drop", (0.6,0.7,0.85), roughness=0.2, emission=(0.4,0.6,0.85), emission_strength=0.8)
    # 7 hanging spires
    import random; random.seed(7)
    for i in range(7):
        x = random.uniform(-0.7, 0.7); z = random.uniform(0.7, 1.4)
        h = random.uniform(0.6, 1.2)
        cone(f"Stal_{i}", random.uniform(0.08, 0.18), 0.0, h, (x, 0, z), rock, verts=8, rot=(math.pi,0,0))
        sphere(f"Drop_{i}", 0.025, (x, 0, z - h/2 - 0.05), drop)
    export_current_as("StalactiteCluster", MOON)

def asset_ResonanceTuningFork():
    reset_scene()
    silver = make_material("Tf_Silver", (0.85,0.85,0.90), roughness=0.2, metallic=0.95)
    handle = make_material("Tf_Handle", (0.42,0.28,0.16), roughness=0.7)
    cyl("Handle", 0.025, 0.40, (0,0,0.20), handle)
    cube("Base", (0,0,0.42), (0.10, 0.04, 0.04), silver)
    cube("TineL", (-0.04,0,0.65), (0.025, 0.025, 0.50), silver)
    cube("TineR", (0.04,0,0.65), (0.025, 0.025, 0.50), silver)
    # Ball tops
    sphere("BallL", 0.035, (-0.04,0,0.92), silver)
    sphere("BallR", 0.035, (0.04,0,0.92), silver)
    export_current_as("ResonanceTuningFork", MOON)

for fn in [asset_DissonanceCrystalBlack, asset_DissonanceCrystalRed, asset_DissonanceCrystalGreen,
           asset_CavernWallSegment, asset_CrystalThrone, asset_MicroGiantPortal,
           asset_StalactiteCluster, asset_ResonanceTuningFork]:
    fn()
print("[TARTARIA] Moon 2 set done (8 assets).")
