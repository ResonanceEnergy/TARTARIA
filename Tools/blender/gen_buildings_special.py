"""8 special buildings — watchtower, lighthouse, greenhouse, observatory,
crystal hall, sky temple, ruined foundation, apothecary.
Per CLAUDE.md no-stubs mandate — every building is complete.
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

# 1. Watchtower — tall slim stone tower
# Spec target: ~15m (docs/15 §7 — meant as a lookout visible from afar).
# Prior: 5.5m body + 0.4 battlements + 0.8 cone gave ~6.6m. Stretched body
# to 12m so battlements sit at ~12m and roof beacon tip lands at ~15m.
reset_scene()
stone = make_material("WT_stone", (0.55, 0.50, 0.45), roughness=0.85)
roof_m = make_material("WT_roof", (0.20, 0.30, 0.40), roughness=0.70)
flame = make_material("WT_flame", (0.95, 0.40, 0.10), roughness=0.20,
                      emission=(1.0, 0.50, 0.10), emission_strength=2.5)
TOWER_H = 12.6
cyl("body", 1.0, TOWER_H, (0, 0, TOWER_H/2.0), stone, verts=18)
# Stone band quartiles for vertical readability at distance
for band_z in (TOWER_H * 0.30, TOWER_H * 0.60):
    cyl(f"band_{int(band_z*10)}", 1.05, 0.20, (0, 0, band_z), stone, verts=18)
# Battlements at top of body
top_z = TOWER_H + 0.30
for i in range(8):
    a = i*(math.pi/4)
    cube(f"crenel_{i}", (math.cos(a)*1.05, math.sin(a)*1.05, top_z), (0.16, 0.10, 0.40), stone)
# Roof beacon + flame — places overall tip at ~15m
cone("roof", 1.0, 0.10, 1.60, (0, 0, top_z + 0.95), roof_m, verts=16)
sphere("flame", 0.30, (0, 0, top_z + 1.80), flame, segs=10, rings=8)
# Door at ground
cube("door", (0, -1.02, 1.0), (0.30, 0.04, 1.80), make_material("WT_door", (0.30, 0.18, 0.10), roughness=0.80))
# Arrow-slit windows on body
for slit_z in (4.0, 7.0, 10.0):
    cube(f"slit_n_{int(slit_z)}", (0, -1.02, slit_z), (0.10, 0.04, 0.60),
         make_material(f"WT_slit_{int(slit_z)}", (0.05, 0.05, 0.05), roughness=0.90))
finalize("Watchtower", "Moon1")

# 2. Lighthouse (Moon 2)
reset_scene()
white = make_material("LH_white", (0.92, 0.90, 0.85), roughness=0.65)
red = make_material("LH_red", (0.75, 0.20, 0.15), roughness=0.65)
beam = make_material("LH_beam", (0.95, 0.92, 0.80), roughness=0.10,
                     emission=(1.0, 0.95, 0.80), emission_strength=3.0)
window = make_material("LH_win", (0.40, 0.55, 0.75), roughness=0.20, metallic=0.2)
base_m = make_material("LH_base", (0.45, 0.42, 0.40), roughness=0.85)
cyl("base", 1.8, 0.50, (0, 0, 0.25), base_m, verts=20)
cyl("section1", 1.4, 2.5, (0, 0, 1.75), white, verts=20)
cyl("section2", 1.4, 2.0, (0, 0, 4.0), red, verts=20)
cyl("section3", 1.2, 2.0, (0, 0, 6.0), white, verts=18)
cyl("lamp_room", 1.3, 0.90, (0, 0, 7.45), beam, verts=20)
cone("roof", 1.4, 0.20, 1.0, (0, 0, 8.40), red, verts=20)
# Catwalk
torus("catwalk", 1.35, 0.06, (0, 0, 7.00), make_material("LH_rail", (0.20, 0.18, 0.15), roughness=0.50, metallic=0.3))
finalize("Lighthouse", "Moon2")

# 3. Greenhouse (Moon 3) — glass dome over planter
reset_scene()
glass = make_material("GH_glass", (0.70, 0.92, 0.80), roughness=0.10, metallic=0.1,
                       emission=(0.40, 0.65, 0.50), emission_strength=0.3)
frame = make_material("GH_frame", (0.80, 0.72, 0.55), roughness=0.55, metallic=0.6)
soil = make_material("GH_soil", (0.30, 0.20, 0.15), roughness=0.95)
leaf = make_material("GH_leaf", (0.25, 0.55, 0.20), roughness=0.85)
base_m = make_material("GH_base", (0.55, 0.48, 0.42), roughness=0.85)
cube("base", (0, 0, 0.20), (2.0, 2.0, 0.20), base_m)
sphere("dome", 1.9, (0, 0, 1.6), glass, segs=24, rings=16)
# Frame ribs (8)
for i in range(8):
    a = i*(math.pi/4)
    torus(f"rib_{i}", 1.9, 0.05, (0, 0, 1.6), frame, mseg=20, miseg=4, rot=(0, a, 0))
# Center planter
cube("planter", (0, 0, 0.55), (0.80, 0.80, 0.15), make_material("GH_pot", (0.60, 0.40, 0.25), roughness=0.85))
cube("soil_top", (0, 0, 0.68), (0.78, 0.78, 0.04), soil)
sphere("plant", 0.40, (0, 0, 1.05), leaf, segs=14, rings=10)
finalize("Greenhouse", "Moon3")

# 4. Observatory (Moon 4) — domed astronomy building with telescope slit
reset_scene()
white2 = make_material("Obs_white", (0.80, 0.78, 0.72), roughness=0.65)
copper = make_material("Obs_copper", (0.60, 0.40, 0.25), roughness=0.40, metallic=0.7)
inside = make_material("Obs_inside", (0.10, 0.10, 0.15), roughness=0.85,
                       emission=(0.20, 0.30, 0.55), emission_strength=0.8)
brass = make_material("Obs_brass", (0.78, 0.65, 0.30), roughness=0.30, metallic=0.8)
cube("base", (0, 0, 0.25), (2.5, 2.5, 0.25), make_material("Obs_base", (0.40, 0.35, 0.30), roughness=0.85))
cyl("body", 2.0, 1.8, (0, 0, 1.4), white2, verts=20)
sphere("dome", 2.0, (0, 0, 2.3), copper, segs=20, rings=14)
# Slit opening
cube("slit_inside", (0, 0, 2.30), (0.30, 2.10, 0.40), inside)
# Telescope tube
cyl("scope", 0.28, 1.6, (0, 0, 2.50), brass, rot=(0, 0.4, 0), verts=18)
# Door
cube("door", (0, -2.02, 1.2), (0.45, 0.04, 1.0), make_material("Obs_door", (0.30, 0.18, 0.10), roughness=0.80))
finalize("Observatory", "Moon4")

# 5. Crystal Hall (Moon 5) — hexagonal hall with crystal pillars
reset_scene()
hall_m = make_material("CH_hall", (0.85, 0.85, 0.92), roughness=0.30)
crystal = make_material("CH_crystal", (0.55, 0.78, 0.92), roughness=0.15, metallic=0.2,
                         emission=(0.40, 0.70, 0.95), emission_strength=1.6)
roof_m2 = make_material("CH_roof", (0.65, 0.78, 0.88), roughness=0.40)
# Hex floor
for i in range(6):
    a = i*(math.pi/3)
    cone(f"floor_{i}", 2.0, 1.8, 0.20, (math.cos(a)*1.0, math.sin(a)*1.0, 0.10),
         hall_m, rot=(0, 0, a), verts=4)
# Hex pillars (6 corners)
for i in range(6):
    a = i*(math.pi/3)
    cone(f"pillar_{i}", 0.18, 0.08, 3.5, (math.cos(a)*2.4, math.sin(a)*2.4, 1.95), crystal, verts=6)
# Central crystal floating
cone("center_crystal", 0.40, 0.10, 1.20, (0, 0, 1.50), crystal, verts=6)
# Conical roof
cone("roof", 3.0, 0.10, 1.50, (0, 0, 4.50), roof_m2, verts=6)
finalize("CrystalHall", "Moon5")

# 6. Sky Temple (Moon 6) — floating temple platform
reset_scene()
marble = make_material("ST_marble", (0.95, 0.92, 0.85), roughness=0.30)
gold = make_material("ST_gold", (0.95, 0.75, 0.25), roughness=0.25, metallic=0.85,
                     emission=(0.80, 0.60, 0.15), emission_strength=0.5)
sky_glow = make_material("ST_glow", (0.65, 0.85, 1.0), roughness=0.20,
                          emission=(0.60, 0.85, 1.0), emission_strength=2.2)
# Octagonal base
cone("base", 3.0, 2.8, 0.50, (0, 0, 0.25), marble, verts=8)
# 8 columns
for i in range(8):
    a = i*(math.pi/4)
    cyl(f"col_{i}", 0.20, 3.0, (math.cos(a)*2.50, math.sin(a)*2.50, 2.0), marble, verts=16)
    sphere(f"cap_{i}", 0.25, (math.cos(a)*2.50, math.sin(a)*2.50, 3.55), gold, segs=10, rings=8)
# Domed roof
sphere("dome", 2.8, (0, 0, 4.5), marble, segs=24, rings=16)
sphere("dome_top", 0.40, (0, 0, 6.20), gold, segs=14, rings=10)
# Glow underneath
cone("glow", 2.5, 0.40, 0.30, (0, 0, -0.20), sky_glow, verts=8)
finalize("SkyTemple", "Moon6")

# 7. Ruined Foundation — decayed building (collapsed)
reset_scene()
ruin = make_material("Ruin_stone", (0.45, 0.42, 0.38), roughness=0.95)
moss = make_material("Ruin_moss", (0.20, 0.35, 0.15), roughness=0.95)
# Foundation slab
cube("found", (0, 0, 0.10), (2.5, 2.0, 0.20), ruin)
# Broken walls — 3 partial standing pieces
cube("wall_1", (-2.0, -1.0, 1.0), (0.30, 0.30, 1.50), ruin)
cube("wall_2", ( 2.0,  0.5, 0.7), (0.30, 0.30, 0.90), ruin)
cube("wall_3", (-0.5,  1.50, 0.5), (0.40, 0.30, 0.60), ruin)
# Fallen blocks
cube("block_1", (1.0, -0.8, 0.30), (0.50, 0.50, 0.30), ruin, rot=(0, 0, 0.5))
cube("block_2", (-1.0, 0.3, 0.20), (0.40, 0.40, 0.20), ruin, rot=(0, 0, -0.7))
cube("block_3", (0.5, 1.0, 0.18), (0.35, 0.35, 0.16), ruin, rot=(0, 0, 0.3))
# Moss patches
for i, (x, y) in enumerate([(-1.5, -0.5), (1.5, 0.0), (-0.2, 1.2), (1.8, -1.5)]):
    cube(f"moss_{i}", (x, y, 0.22), (0.30, 0.30, 0.03), moss)
finalize("RuinedFoundation", "Shared")

# 8. Apothecary — herbal shop with hanging bundles
# Spec target: ~5m (docs/15 §7). Prior: 2m walls + 0.85m roof + 0.05 ridge
# yielded ~3.8m. v2 (1.5m half-wall + 1.45 roof) measured 4.63m. v3 bumps
# walls + roof a touch more to land at ~5m.
reset_scene()
plaster2 = make_material("Ap_plaster", (0.92, 0.86, 0.70), roughness=0.85)
beam = make_material("Ap_beam", (0.30, 0.18, 0.10), roughness=0.80)
roof_m3 = make_material("Ap_roof", (0.42, 0.22, 0.12), roughness=0.78)
sign_m = make_material("Ap_sign", (0.40, 0.55, 0.30), roughness=0.80)
herb = make_material("Ap_herb", (0.35, 0.45, 0.20), roughness=0.85)
window = make_material("Ap_win", (0.65, 0.85, 0.60), roughness=0.25,
                       emission=(0.45, 0.65, 0.40), emission_strength=0.6)
cube("base", (0, 0, 0.10), (1.6, 1.3, 0.10), make_material("Ap_base", (0.38, 0.32, 0.28), roughness=0.85))
cube("wall", (0, 0, 1.90), (1.6, 1.3, 1.70), plaster2)
# Visible beams
cube("beam_v_l", (-1.50, -1.20, 1.90), (0.06, 0.06, 1.70), beam)
cube("beam_v_r", ( 1.50, -1.20, 1.90), (0.06, 0.06, 1.70), beam)
cube("beam_h", (0, -1.30, 3.60), (1.55, 0.06, 0.06), beam)
# Roof — taller cone so ridge lands near 5m
cone("roof", 1.9, 0.05, 1.40, (0, 0, 4.30), roof_m3, verts=4)
cube("door", (0, -1.33, 0.95), (0.30, 0.03, 0.85), make_material("Ap_door", (0.30, 0.18, 0.10), roughness=0.80))
cube("win", (-0.50, -1.32, 2.30), (0.30, 0.02, 0.30), window)
cube("sign", (0.65, -1.45, 2.70), (0.30, 0.04, 0.20), sign_m)
# Hanging herb bundles
for i, x in enumerate([-0.40, 0.0, 0.40]):
    cube(f"bundle_{i}", (x, -1.20, 3.10), (0.06, 0.04, 0.20), herb)
finalize("Apothecary", "Moon1")

print("done gen_buildings_special: 8 buildings")
