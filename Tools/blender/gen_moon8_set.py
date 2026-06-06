"""Moon 8 — Clockwork Citadel: 6 assets."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, torus, cone

MOON = "Moon8"

def asset_ClockworkGiantGear():
    reset_scene()
    brass = make_material("Cg_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    iron = make_material("Cg_Iron", (0.30,0.28,0.25), roughness=0.5, metallic=0.6)
    cyl("Body", 1.4, 0.18, (0,0,0), brass, verts=48)
    # Teeth: 24 trapezoidal cubes on outer ring
    for i in range(24):
        a = i * math.pi/12
        x, y = math.cos(a)*1.55, math.sin(a)*1.55
        cube(f"T_{i}", (x, y, 0), (0.18, 0.10, 0.20), brass, rot=(0,0,a))
    cyl("Hub", 0.3, 0.20, (0,0,0), iron, verts=24)
    cyl("Axle", 0.10, 0.5, (0,0,0), iron, verts=12)
    export_current_as("ClockworkGiantGear", MOON)

def asset_ClockworkSmallGear():
    reset_scene()
    brass = make_material("Cs_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cyl("Body", 0.40, 0.08, (0,0,0), brass, verts=24)
    for i in range(12):
        a = i * math.pi/6
        x, y = math.cos(a)*0.45, math.sin(a)*0.45
        cube(f"T_{i}", (x, y, 0), (0.06, 0.05, 0.10), brass, rot=(0,0,a))
    cyl("Hub", 0.08, 0.10, (0,0,0), brass, verts=12)
    export_current_as("ClockworkSmallGear", MOON)

def asset_ClockworkArm():
    reset_scene()
    brass = make_material("Ca_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cube("Arm", (0,0,0.5), (0.10, 0.05, 1.0), brass)
    cyl("Joint1", 0.10, 0.08, (0,0,0), brass, rot=(math.pi/2,0,0))
    cyl("Joint2", 0.10, 0.08, (0,0,1.0), brass, rot=(math.pi/2,0,0))
    cone("Tip", 0.08, 0.02, 0.15, (0,0,1.10), brass, verts=12)
    export_current_as("ClockworkArm", MOON)

def asset_PendulumWeight():
    reset_scene()
    brass = make_material("Pw_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cyl("Rod", 0.02, 1.5, (0,0,0.75), brass)
    sphere("Weight", 0.20, (0,0,0.0), brass, segs=24, rings=18)
    cyl("Pivot", 0.05, 0.04, (0,0,1.50), brass, rot=(math.pi/2,0,0))
    export_current_as("PendulumWeight", MOON)

def asset_ClockFace():
    reset_scene()
    bronze = make_material("Cf_Bronze", (0.55,0.40,0.20), roughness=0.4, metallic=0.7)
    paint = make_material("Cf_Paint", (0.95,0.92,0.85), roughness=0.5)
    iron = make_material("Cf_Iron", (0.18,0.18,0.20), roughness=0.5, metallic=0.6)
    cyl("Frame", 1.0, 0.10, (0,0,0), bronze, verts=32)
    cyl("Face", 0.92, 0.04, (0,0,0.06), paint, verts=32)
    # 12 hour marks
    for i in range(12):
        a = i * math.pi/6
        cube(f"M_{i}", (math.cos(a)*0.80, math.sin(a)*0.80, 0.07), (0.04, 0.04, 0.02), iron, rot=(0,0,a))
    # Hour hand + minute hand
    cube("Hour", (0.20, 0, 0.08), (0.40, 0.04, 0.02), iron)
    cube("Min", (0, 0.30, 0.08), (0.04, 0.60, 0.02), iron)
    sphere("Center", 0.06, (0,0,0.08), iron)
    export_current_as("ClockFace", MOON)

def asset_CitadelChimney():
    reset_scene()
    iron = make_material("Ch_Iron", (0.30,0.28,0.25), roughness=0.5, metallic=0.6)
    brass = make_material("Ch_Brass", (0.75,0.62,0.30), roughness=0.4, metallic=0.85)
    cyl("Stack", 0.30, 3.0, (0,0,1.5), iron, verts=16)
    for h in [1.0, 2.0]:
        torus(f"Band_{h}", 0.32, 0.04, (0,0,h), brass, mseg=20, miseg=4)
    cyl("Cap", 0.42, 0.10, (0,0,3.0), brass, verts=20)
    cone("Top", 0.42, 0.10, 0.30, (0,0,3.15), iron, verts=20)
    export_current_as("CitadelChimney", MOON)

for fn in [asset_ClockworkGiantGear, asset_ClockworkSmallGear, asset_ClockworkArm,
           asset_PendulumWeight, asset_ClockFace, asset_CitadelChimney]:
    fn()
print("[TARTARIA] Moon 8 set done (6 assets).")
