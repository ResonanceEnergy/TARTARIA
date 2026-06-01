"""Moon 6 — Living Library: 5 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon6"

def asset_KnowledgePodium():
    reset_scene()
    wood = make_material("Kp_Wood", (0.25,0.16,0.10), roughness=0.5)
    gold = make_material("Kp_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=1.5)
    cyl("Post", 0.10, 1.2, (0,0,0.60), wood)
    cyl("Base", 0.30, 0.04, (0,0,0.02), wood, verts=16)
    cube("Top", (0,0,1.30), (0.50, 0.40, 0.06), wood, rot=(math.radians(20),0,0))
    cube("Book", (0,-0.05,1.35), (0.30, 0.04, 0.20), gold, rot=(math.radians(20),0,0))
    export_current_as("KnowledgePodium", MOON)

def asset_FloatingTome():
    reset_scene()
    leather = make_material("Ft_Leather", (0.55,0.28,0.15), roughness=0.6)
    gold = make_material("Ft_Gold", (0.95,0.82,0.32), roughness=0.3, metallic=0.85, emission=(0.95,0.78,0.28), emission_strength=2.0)
    paper = make_material("Ft_Paper", (0.92,0.88,0.75), roughness=0.85)
    cube("Cover", (0,0,1.5), (0.20, 0.05, 0.30), leather)
    cube("Spine", (-0.10,0,1.5), (0.02, 0.05, 0.30), gold)
    # Open pages effect — 2 angled cubes
    cube("PageL", (-0.05,0.04,1.5), (0.10, 0.005, 0.28), paper, rot=(0,-0.3,0))
    cube("PageR", (0.05,0.04,1.5), (0.10, 0.005, 0.28), paper, rot=(0,0.3,0))
    sphere("Glow", 0.25, (0,0.20,1.5), gold)
    export_current_as("FloatingTome", MOON)

def asset_LivingBookcase():
    reset_scene()
    wood = make_material("Lb_Wood", (0.18,0.10,0.06), roughness=0.5)
    cube("Frame", (0,0,2.0), (1.5, 0.30, 2.0), wood)
    # 8 shelves with random book colors
    import random; random.seed(6)
    for s in range(8):
        h = 0.30 + s*0.45
        cube(f"Sh_{s}", (0, 0.04, h), (1.45, 0.20, 0.03), wood)
        for b in range(12):
            mat = make_material(f"Bk_{s}_{b}", (random.uniform(0.2,0.7),random.uniform(0.1,0.5),random.uniform(0.1,0.3)), 0.6)
            cube(f"B_{s}_{b}", (-0.65 + b*0.115, 0.10, h+0.12), (0.04, 0.10, 0.20), mat)
    export_current_as("LivingBookcase", MOON)

def asset_ResearchOrb():
    reset_scene()
    bronze = make_material("Ro_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    glass = make_material("Ro_Glass", (0.55,0.85,0.95), roughness=0.1, emission=(0.55,0.85,0.95), emission_strength=2.5)
    sphere("Orb", 0.35, (0,0,0.6), glass)
    torus("RingX", 0.40, 0.025, (0,0,0.6), bronze, mseg=24, miseg=6)
    torus("RingY", 0.40, 0.025, (0,0,0.6), bronze, mseg=24, miseg=6, rot=(math.pi/2,0,0))
    torus("RingZ", 0.40, 0.025, (0,0,0.6), bronze, mseg=24, miseg=6, rot=(0,math.pi/2,0))
    cyl("Stand", 0.20, 0.04, (0,0,0.02), bronze, verts=12)
    cyl("StandShaft", 0.04, 0.20, (0,0,0.15), bronze)
    export_current_as("ResearchOrb", MOON)

def asset_KnowledgeColumnGlowing():
    reset_scene()
    stone = make_material("Kc_Stone", (0.55,0.52,0.48), roughness=0.85)
    glyph = make_material("Kc_Glyph", (0.40,0.75,0.95), roughness=0.3, emission=(0.40,0.75,0.95), emission_strength=2.5)
    cyl("Shaft", 0.20, 3.0, (0,0,1.5), stone, verts=12)
    cyl("Base", 0.30, 0.10, (0,0,0.05), stone, verts=16)
    cyl("Cap", 0.30, 0.10, (0,0,3.05), stone, verts=16)
    # Glyph bands
    for h in [0.8, 1.5, 2.2]:
        torus(f"Band_{h}", 0.21, 0.04, (0,0,h), glyph, mseg=20, miseg=4)
    export_current_as("KnowledgeColumnGlowing", MOON)

for fn in [asset_KnowledgePodium, asset_FloatingTome, asset_LivingBookcase,
           asset_ResearchOrb, asset_KnowledgeColumnGlowing]:
    fn()
print("[TARTARIA] Moon 6 set done (5 assets).")
