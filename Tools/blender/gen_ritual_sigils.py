"""10 ritual sigil sites + decor — stone circle, pentagram floor,
triskele tile, vesica piscis floor, ankh wall plaque, eye-of-providence relief,
ouroboros ring large, sephiroth pillar trio, zodiac wheel, lunar phase wheel.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone, torus
import bpy, math

def finalize(name, moon="Shared"):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)

stone = lambda n: make_material(n, (0.55, 0.50, 0.45), roughness=0.85)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85,
                                emission=(0.85, 0.65, 0.20), emission_strength=0.6)
glow = lambda n, c, em=1.5: make_material(n, c, roughness=0.30, emission=c, emission_strength=em)

# 1. Stone Circle (4 large standing stones + center altar)
reset_scene()
for i in range(4):
    a = i*(math.pi/2)
    cube(f"stone_{i}", (math.cos(a)*1.5, math.sin(a)*1.5, 0.75), (0.30, 0.20, 0.75), stone(f"SC_s{i}"), rot=(0, 0, a))
# Center altar
cyl("altar", 0.40, 0.20, (0, 0, 0.10), stone("SC_altar"), verts=8)
# Glow sigil on altar
torus("sigil", 0.30, 0.012, (0, 0, 0.22), glow("SC_sigil", (0.40, 0.70, 1.0)), mseg=22, miseg=4)
finalize("StoneCircle")

# 2. Pentagram Floor
reset_scene()
stone_d = make_material("Pn_floor", (0.30, 0.28, 0.25), roughness=0.85)
g = glow("Pn_glow", (0.95, 0.20, 0.10))
cyl("floor", 1.0, 0.02, (0, 0, 0.01), stone_d, verts=22)
# Circle around pentagram
torus("circle", 0.85, 0.020, (0, 0, 0.03), g, mseg=30, miseg=4)
# 5 inner-star segments (just 5 elongated bars at 5 angles)
for i in range(5):
    a = i*(2*math.pi/5) + math.pi/2
    cube(f"line_{i}", (math.cos(a)*0.5, math.sin(a)*0.5, 0.03), (0.05, 0.01, 0.50), g, rot=(0, 0, a))
finalize("PentagramFloor")

# 3. Triskele Tile (Celtic 3-spiral)
reset_scene()
base = stone("Tri_base")
g = glow("Tri_g", (0.40, 0.85, 0.40))
cube("tile", (0, 0, 0.02), (0.50, 0.50, 0.02), base)
# 3 spirals as small spheres at corners
for i in range(3):
    a = i*(2*math.pi/3) + math.pi/2
    cx = math.cos(a) * 0.18
    cy = math.sin(a) * 0.18
    for j in range(6):
        t = j / 5.0
        r = 0.12 * (1 - t * 0.7)
        sa = j*0.7 + a
        sphere(f"spiral_{i}_{j}", 0.025, (cx + math.cos(sa)*r, cy + math.sin(sa)*r, 0.05), g, segs=8, rings=6)
finalize("TriskeleTile")

# 4. Vesica Piscis Floor (2 overlapping circles)
reset_scene()
base = stone("VP_base")
g = glow("VP_g", (0.55, 0.30, 0.95))
cube("tile", (0, 0, 0.02), (0.90, 0.50, 0.02), base)
torus("c1", 0.30, 0.012, (-0.15, 0, 0.04), g, mseg=24, miseg=4)
torus("c2", 0.30, 0.012, ( 0.15, 0, 0.04), g, mseg=24, miseg=4)
finalize("VesicaPiscisFloor")

# 5. Ankh Wall Plaque
reset_scene()
plaque = stone("Ank_p")
ankh_m = gold("Ank_g")
cube("plaque", (0, 0.04, 0.30), (0.30, 0.04, 0.45), plaque)
# Ankh loop on top
torus("loop", 0.10, 0.018, (0, 0.02, 0.52), ankh_m, mseg=20, miseg=4, rot=(1.5708, 0, 0))
# Vertical bar
cube("bar_v", (0, 0.02, 0.30), (0.025, 0.005, 0.18), ankh_m)
# Horizontal bar
cube("bar_h", (0, 0.02, 0.36), (0.16, 0.005, 0.025), ankh_m)
finalize("AnkhWallPlaque")

# 6. Eye of Providence Relief
reset_scene()
plaque = stone("EoP_p")
g_iris = glow("EoP_iris", (0.50, 0.80, 1.0), em=2.0)
gold_m = gold("EoP_g")
cube("plaque", (0, 0.04, 0.30), (0.40, 0.04, 0.40), plaque)
# Triangle outline (3 thin bars)
for i in range(3):
    a = i*(2*math.pi/3) + math.pi/2
    x = math.cos(a) * 0.20
    z = 0.30 + math.sin(a) * 0.20
    cube(f"side_{i}", (x, 0.02, z), (0.18, 0.005, 0.015), gold_m, rot=(0, 0, a))
# Eye in center
sphere("eye_white", 0.08, (0, 0.02, 0.30), make_material("EoP_w", (0.95, 0.92, 0.85), roughness=0.40), segs=12, rings=10)
sphere("iris", 0.035, (0, 0.0, 0.30), g_iris, segs=10, rings=8)
sphere("pupil", 0.015, (0, -0.02, 0.30), make_material("EoP_pup", (0.05, 0.05, 0.05), roughness=0.30), segs=8, rings=6)
finalize("EyeOfProvidenceRelief")

# 7. Ouroboros Ring Large (snake eating tail — floor ring)
reset_scene()
g_scale = make_material("Ou_scale", (0.30, 0.55, 0.40), roughness=0.55, metallic=0.3,
                         emission=(0.40, 0.85, 0.55), emission_strength=0.8)
torus("body", 0.80, 0.10, (0, 0, 0.10), g_scale, mseg=36, miseg=10)
# Head merging with tail at one point
sphere("head", 0.14, (0.80, 0, 0.10), g_scale, segs=14, rings=10)
# Eye
sphere("eye", 0.025, (0.92, 0.08, 0.12), make_material("Ou_e", (0.95, 0.30, 0.10), roughness=0.20, emission=(1.0, 0.30, 0.10), emission_strength=1.5), segs=8, rings=6)
finalize("OuroborosRingLarge")

# 8. Sephiroth Pillar Trio (Tree of Life — Pillar of Severity, Mercy, Mildness)
reset_scene()
def sephiroth_pillar(x, color_hex, glow_color, name):
    base_m = make_material(name+"_b", color_hex, roughness=0.55, metallic=0.4)
    g_m = glow(name+"_g", glow_color)
    cyl(f"{name}_pillar", 0.10, 2.0, (x, 0, 1.0), base_m, verts=14)
    # 3 sephiroth glyphs on each pillar
    for i, z in enumerate([0.50, 1.20, 1.90]):
        sphere(f"{name}_sef_{i}", 0.13, (x, -0.12, z), g_m, segs=12, rings=10)

sephiroth_pillar(-0.50, (0.95, 0.20, 0.18), (0.95, 0.30, 0.20), "Severity")  # red - Boaz
sephiroth_pillar( 0.0,  (0.85, 0.85, 0.85), (0.90, 0.92, 0.95), "Mildness")  # white - Middle
sephiroth_pillar( 0.50, (0.30, 0.45, 0.95), (0.40, 0.55, 1.0),  "Mercy")     # blue - Jachin
finalize("SephirothPillarTrio")

# 9. Zodiac Wheel (12 segments)
reset_scene()
stone_d = make_material("Zw_base", (0.30, 0.30, 0.32), roughness=0.85)
g = glow("Zw_g", (0.95, 0.78, 0.20))
cyl("disc", 0.80, 0.04, (0, 0, 0.02), stone_d, verts=24)
# Outer ring
torus("ring_out", 0.75, 0.012, (0, 0, 0.05), g, mseg=30, miseg=4)
torus("ring_in", 0.55, 0.010, (0, 0, 0.05), g, mseg=24, miseg=4)
# 12 divider spokes
for i in range(12):
    a = i*(math.pi/6)
    cube(f"spoke_{i}", (math.cos(a)*0.65, math.sin(a)*0.65, 0.05), (0.20, 0.012, 0.012), g, rot=(0, 0, a))
# Center solar symbol
sphere("sun", 0.10, (0, 0, 0.08), g, segs=14, rings=10)
finalize("ZodiacWheel")

# 10. Lunar Phase Wheel (8 phases)
reset_scene()
base = make_material("LP_base", (0.20, 0.25, 0.40), roughness=0.55, metallic=0.4)
moon_full = make_material("LP_full", (0.95, 0.92, 0.85), roughness=0.20, emission=(0.95, 0.92, 0.85), emission_strength=1.5)
moon_dark = make_material("LP_dark", (0.30, 0.30, 0.40), roughness=0.50)
cyl("disc", 0.80, 0.04, (0, 0, 0.02), base, verts=24)
# 8 moon discs around the rim
for i in range(8):
    a = i*(math.pi/4)
    x = math.cos(a) * 0.60
    y = math.sin(a) * 0.60
    # Full sphere
    sphere(f"moon_{i}", 0.10, (x, y, 0.10), moon_full, segs=14, rings=10)
    # Shadow occlusion — partial dark sphere offset
    occlude = (i / 7.0)  # 0 = new, 1 = full
    if occlude < 0.95:
        offset_x = (0.5 - occlude) * 0.16
        sphere(f"shadow_{i}", 0.10, (x + offset_x, y, 0.12), moon_dark, segs=12, rings=10)
# Center earth
sphere("earth", 0.12, (0, 0, 0.08), make_material("LP_earth", (0.20, 0.55, 0.30), roughness=0.55, emission=(0.30, 0.65, 0.95), emission_strength=0.4), segs=14, rings=10)
finalize("LunarPhaseWheel")

print("done gen_ritual_sigils: 10 sigils")
