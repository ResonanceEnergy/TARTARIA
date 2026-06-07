"""
R35 — Harmonic Pattern pedestal. Stone column topped with 3 concentric resonance rings.
Pairs with TuningVariantC_Pattern mini-game (player matches a target ring sequence).
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx, cyl, torus, sphere, cube

reset_scene()

STONE_LIGHT = make_material("Pedestal_Stone_Light_HP", (0.62, 0.60, 0.56), roughness=0.85)
STONE_DARK  = make_material("Pedestal_Stone_Dark_HP",  (0.38, 0.36, 0.32), roughness=0.85)
COPPER      = make_material("Pedestal_Copper_HP",      (0.75, 0.45, 0.28), roughness=0.40, metallic=0.7,
                            emission=(0.55, 0.30, 0.18), emission_strength=1.0)
RING_OUTER  = make_material("Pedestal_RingOuter_HP",   (0.55, 0.20, 0.45), roughness=0.30,
                            emission=(1.6, 0.5, 1.5), emission_strength=2.0)
RING_MID    = make_material("Pedestal_RingMid_HP",     (0.60, 0.25, 0.55), roughness=0.30,
                            emission=(1.9, 0.6, 1.8), emission_strength=3.0)
RING_INNER  = make_material("Pedestal_RingInner_HP",   (0.65, 0.30, 0.65), roughness=0.30,
                            emission=(2.2, 0.7, 2.1), emission_strength=4.0)
GEM_CORE    = make_material("Pedestal_GemCore_HP",     (0.95, 0.55, 0.95), roughness=0.10,
                            emission=(3.5, 1.5, 3.5), emission_strength=5.0)

# Pedestal base — matches TuningPedestal silhouette
cyl("Base1", 0.55, 0.08, (0, 0, 0.04), mat=STONE_DARK, verts=16)
cyl("Base2", 0.50, 0.05, (0, 0, 0.105), mat=STONE_LIGHT, verts=16)
# Column shaft (6-sided)
cyl("Shaft", 0.30, 0.95, (0, 0, 0.62), mat=STONE_LIGHT, verts=6)
cyl("Ring1", 0.34, 0.05, (0, 0, 0.40), mat=STONE_DARK, verts=12)
cyl("Ring2", 0.34, 0.05, (0, 0, 0.85), mat=STONE_DARK, verts=12)
cyl("CopperBand", 0.31, 0.04, (0, 0, 0.62), mat=COPPER, verts=12)
cyl("Capital", 0.42, 0.06, (0, 0, 1.13), mat=STONE_LIGHT, verts=12)
cyl("Capital2", 0.36, 0.05, (0, 0, 1.18), mat=STONE_DARK, verts=12)

# Resonance ring stack — 3 horizontal toruses, brightness ramps inward
TOP_Z = 1.30
# Outer ring (largest, dimmest)
torus("OuterRing", 0.32, 0.025, (0, 0, TOP_Z + 0.00), mat=RING_OUTER, mseg=48, miseg=12)
# Mid ring
torus("MidRing",   0.22, 0.022, (0, 0, TOP_Z + 0.10), mat=RING_MID,   mseg=48, miseg=12)
# Inner ring (smallest, brightest)
torus("InnerRing", 0.12, 0.020, (0, 0, TOP_Z + 0.20), mat=RING_INNER, mseg=48, miseg=12)

# Floating gem core in the middle of the ring stack
sphere("GemCore", 0.05, (0, 0, TOP_Z + 0.13), mat=GEM_CORE, segs=20, rings=16)

# Copper mount stem under outer ring connecting to capital
cyl("MountStem", 0.06, 0.16, (0, 0, 1.22), mat=COPPER, verts=16)
# 3 small copper riser nubs to lift the rings (subtle support)
for i in range(3):
    a = i * (2 * math.pi / 3)
    x = math.cos(a) * 0.18
    y = math.sin(a) * 0.18
    cyl(f"Riser_{i}", 0.018, 0.06, (x, y, TOP_Z - 0.02), mat=COPPER, verts=8)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "HarmonicPedestal"
export_fbx("HarmonicPedestal")
print("[TARTARIA] HarmonicPedestal done.")
