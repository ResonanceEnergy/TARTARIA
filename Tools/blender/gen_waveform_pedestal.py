"""
R35 — Waveform Trace pedestal. Stone column with oscilloscope-style cyan screen on top.
Pairs with TuningVariantB_Waveform mini-game (player traces a target sine/square waveform).
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx, cyl, cube, sphere

reset_scene()

STONE_LIGHT = make_material("Pedestal_Stone_Light_WF", (0.62, 0.60, 0.56), roughness=0.85)
STONE_DARK  = make_material("Pedestal_Stone_Dark_WF",  (0.38, 0.36, 0.32), roughness=0.85)
BRASS       = make_material("Pedestal_Brass_WF",       (0.78, 0.65, 0.32), roughness=0.35, metallic=0.7,
                            emission=(0.6, 0.45, 0.18), emission_strength=1.2)
SCREEN_BEZEL = make_material("Pedestal_Bezel_WF",      (0.18, 0.18, 0.22), roughness=0.55, metallic=0.3)
CYAN_GLOW   = make_material("Pedestal_CyanScreen_WF",  (0.08, 0.55, 0.85), roughness=0.25,
                            emission=(0.0, 1.6, 2.2), emission_strength=3.5)
GRID_LINE   = make_material("Pedestal_GridLine_WF",    (0.15, 0.85, 1.0), roughness=0.4,
                            emission=(0.4, 1.4, 1.8), emission_strength=2.5)

# Pedestal base — matches TuningPedestal silhouette so the trio reads as kit
cyl("Base1", 0.55, 0.08, (0, 0, 0.04), mat=STONE_DARK, verts=16)
cyl("Base2", 0.50, 0.05, (0, 0, 0.105), mat=STONE_LIGHT, verts=16)
# Column shaft (6-sided like TuningPedestal)
cyl("Shaft", 0.30, 0.95, (0, 0, 0.62), mat=STONE_LIGHT, verts=6)
# Mid + capital rings
cyl("Ring1", 0.34, 0.05, (0, 0, 0.40), mat=STONE_DARK, verts=12)
cyl("Ring2", 0.34, 0.05, (0, 0, 0.85), mat=STONE_DARK, verts=12)
cyl("BrassBand", 0.31, 0.04, (0, 0, 0.62), mat=BRASS, verts=12)
cyl("Capital", 0.42, 0.06, (0, 0, 1.13), mat=STONE_LIGHT, verts=12)

# Screen rig — angled toward player (tilt back ~15deg around X)
TILT = math.radians(15)
SCREEN_CENTER_Z = 1.55
# Bezel — slightly larger than glow plate
cube("Bezel", (0, 0, SCREEN_CENTER_Z), (0.86, 0.10, 0.62), mat=SCREEN_BEZEL, rot=(TILT, 0, 0))
# Screen glow plate
cube("Screen", (0, -0.045, SCREEN_CENTER_Z), (0.78, 0.04, 0.52), mat=CYAN_GLOW, rot=(TILT, 0, 0))

# Embossed waveform trace — 12 thin vertical bars along the front face of screen
# Modulated by sine to suggest a live trace
for i in range(12):
    t = i / 11.0
    x_local = (t - 0.5) * 0.72
    amp = math.sin(t * math.pi * 3.0) * 0.18  # 1.5 cycles across the screen
    bar_h = 0.04 + abs(amp) * 0.6
    z = SCREEN_CENTER_Z + amp * math.cos(TILT)
    # Tilt the bar to match screen plane
    cube(f"Trace_{i}", (x_local, -0.07, z), (0.012, 0.005, bar_h),
         mat=GRID_LINE, rot=(TILT, 0, 0))

# Brass mount underneath bezel — connects to capital
cyl("MountStem", 0.08, 0.18, (0, 0, 1.27), mat=BRASS, verts=16)
cyl("MountFlare", 0.16, 0.04, (0, 0, 1.21), mat=BRASS, verts=16)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "WaveformPedestal"
export_fbx("WaveformPedestal")
print("[TARTARIA] WaveformPedestal done.")
