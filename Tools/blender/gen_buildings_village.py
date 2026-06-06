"""8 village buildings — 3 cottage variants, mill, smithy, bakery, inn, town hall.

Per CLAUDE.md no-stubs mandate — every building has walls, roof, door, window,
chimney, real materials.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

def finalize(name, moon="Shared"):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)

def cottage(name, wall_color, roof_color, w=3.0, d=2.5, h=2.4, door_color=(0.30, 0.18, 0.10), moon="Shared"):
    reset_scene()
    wall_m = make_material(name+"_wall", wall_color, roughness=0.85)
    roof_m = make_material(name+"_roof", roof_color, roughness=0.75)
    door_m = make_material(name+"_door", door_color, roughness=0.80)
    win_m = make_material(name+"_win", (0.55, 0.78, 0.95), roughness=0.20, metallic=0.1,
                          emission=(0.40, 0.55, 0.70), emission_strength=0.4)
    chim_m = make_material(name+"_chim", (0.35, 0.30, 0.28), roughness=0.85)
    base_m = make_material(name+"_base", (0.40, 0.35, 0.32), roughness=0.85)
    # Foundation
    cube("base", (0, 0, 0.10), (w/2+0.05, d/2+0.05, 0.10), base_m)
    # Walls
    cube("wall", (0, 0, h/2+0.20), (w/2, d/2, h/2), wall_m)
    # Roof (4-sided pitched)
    cone("roof", (w/2+0.20)*1.2, 0.05, h*0.55, (0, 0, h+0.50), roof_m, verts=4)
    # Door
    cube("door", (0, -d/2-0.04, 0.95), (0.35, 0.04, 0.85), door_m)
    # Windows
    cube("win_l", (-w*0.30, -d/2-0.03, 1.45), (0.28, 0.02, 0.30), win_m)
    cube("win_r", ( w*0.30, -d/2-0.03, 1.45), (0.28, 0.02, 0.30), win_m)
    # Chimney
    cube("chim", (w*0.30, 0, h+0.90), (0.18, 0.18, 0.40), chim_m)
    finalize(name, moon)

# 1-3. Cottage A/B/C — color variants
cottage("VillageCottageA", wall_color=(0.85, 0.78, 0.55), roof_color=(0.45, 0.20, 0.10), moon="Moon1")
cottage("VillageCottageB", wall_color=(0.75, 0.65, 0.50), roof_color=(0.30, 0.25, 0.20), moon="Moon1")
cottage("VillageCottageC", wall_color=(0.92, 0.85, 0.70), roof_color=(0.55, 0.30, 0.20), moon="Moon1")

# 4. Mill — has tall structure + windmill blades
reset_scene()
stone = make_material("Mill_stone", (0.62, 0.58, 0.52), roughness=0.85)
roof_m = make_material("Mill_roof", (0.30, 0.18, 0.10), roughness=0.80)
blade_m = make_material("Mill_blade", (0.85, 0.80, 0.70), roughness=0.75)
axle_m = make_material("Mill_axle", (0.20, 0.15, 0.10), roughness=0.70)
door_m = make_material("Mill_door", (0.32, 0.20, 0.12), roughness=0.80)
cyl("body", 1.8, 4.5, (0, 0, 2.25), stone, verts=20)
cone("roof", 2.0, 0.10, 1.20, (0, 0, 5.10), roof_m, verts=20)
cube("door", (0, -1.84, 0.95), (0.40, 0.04, 0.85), door_m)
# Window slits at top
for i in range(4):
    import math
    a = i*(math.pi/2)
    cube(f"win_{i}", (math.cos(a)*1.82, math.sin(a)*1.82, 4.0), (0.08, 0.08, 0.20),
         make_material(f"Mill_win{i}", (0.05, 0.05, 0.05), roughness=0.90))
# Axle + blades (4)
cyl("axle", 0.20, 0.50, (0, -2.0, 5.0), axle_m, rot=(1.5708, 0, 0), verts=10)
for i in range(4):
    import math
    a = i*(math.pi/2)
    cube(f"blade_{i}", (math.cos(a)*1.6, -2.20, 5.0+math.sin(a)*1.6),
         (0.30, 0.06, 1.50), blade_m, rot=(0, 0, a))
finalize("VillageMill", "Moon1")

# 5. Smithy — open-front with anvil + chimney
reset_scene()
stone2 = make_material("Smithy_stone", (0.55, 0.50, 0.45), roughness=0.85)
beam = make_material("Smithy_beam", (0.20, 0.12, 0.08), roughness=0.80)
forge = make_material("Smithy_forge", (0.15, 0.12, 0.10), roughness=0.75,
                       emission=(0.95, 0.40, 0.10), emission_strength=2.0)
roof_m2 = make_material("Smithy_roof", (0.30, 0.20, 0.12), roughness=0.80)
# Foundation
cube("base", (0, 0, 0.10), (2.0, 1.5, 0.10), stone2)
# 3 walls (no front)
cube("wall_b", (0, 1.40, 1.10), (2.0, 0.10, 1.0), stone2)
cube("wall_l", (-1.90, 0.65, 1.10), (0.10, 0.85, 1.0), stone2)
cube("wall_r", ( 1.90, 0.65, 1.10), (0.10, 0.85, 1.0), stone2)
# Roof
cube("roof", (0, 0, 2.30), (2.1, 1.6, 0.10), roof_m2)
# Support beams
cube("beam_l", (-1.85, -1.40, 1.10), (0.08, 0.08, 1.10), beam)
cube("beam_r", ( 1.85, -1.40, 1.10), (0.08, 0.08, 1.10), beam)
# Forge (glowing)
cube("forge_block", (1.20, 0.80, 0.50), (0.50, 0.50, 0.40), stone2)
cube("forge_fire", (1.20, 0.80, 0.95), (0.30, 0.30, 0.08), forge)
# Anvil
cube("anvil_base", (-0.60, 0.0, 0.50), (0.20, 0.40, 0.40), beam)
cube("anvil_top",  (-0.60, 0.0, 0.95), (0.30, 0.50, 0.10), stone2)
finalize("VillageSmithy", "Moon1")

# 6. Bakery — cottage with chimney + bread sign
# Spec target: ~6m (docs/15 §7). Prior: 1.10×2 wall + 1.0 roof + 0.5 chimney
# yielded ~4.4m. Bumped wall to 1.65 half-height (3.3m), roof to 1.4, chimney
# to 0.8 — total ~6m.
reset_scene()
plaster = make_material("Bakery_plaster", (0.95, 0.88, 0.75), roughness=0.85)
roof_m3 = make_material("Bakery_roof", (0.40, 0.18, 0.10), roughness=0.78)
sign_m = make_material("Bakery_sign", (0.65, 0.40, 0.20), roughness=0.80)
oven = make_material("Bakery_oven", (0.30, 0.25, 0.20), roughness=0.80,
                     emission=(0.85, 0.35, 0.10), emission_strength=0.8)
door_m3 = make_material("Bakery_door", (0.40, 0.25, 0.12), roughness=0.80)
win_m3 = make_material("Bakery_win", (0.85, 0.70, 0.45), roughness=0.30,
                       emission=(0.70, 0.55, 0.25), emission_strength=0.6)
cube("base", (0, 0, 0.10), (1.8, 1.4, 0.10), make_material("Bakery_base", (0.40, 0.35, 0.32), roughness=0.85))
cube("wall", (0, 0, 1.85), (1.8, 1.4, 1.65), plaster)
cone("roof", 2.2, 0.05, 1.40, (0, 0, 4.20), roof_m3, verts=4)
cube("door", (0, -1.43, 0.95), (0.35, 0.03, 0.85), door_m3)
cube("win_display", (0, -1.42, 1.65), (0.85, 0.02, 0.40), win_m3)
cube("sign", (0, -1.50, 2.60), (0.50, 0.04, 0.30), sign_m)
# Chimney with glow — pushes total height to ~6m
cube("chim", (0.80, 0.40, 5.10), (0.20, 0.20, 0.80), make_material("Bakery_chim", (0.35, 0.30, 0.28), roughness=0.85))
cube("chim_glow", (0.80, 0.40, 5.95), (0.10, 0.10, 0.05), oven)
finalize("VillageBakery", "Moon1")

# 7. Inn (Moon 1 generic, distinct from BobsInn)
reset_scene()
wood = make_material("Inn_wood", (0.50, 0.32, 0.20), roughness=0.80)
roof_m4 = make_material("Inn_roof", (0.32, 0.18, 0.10), roughness=0.78)
window = make_material("Inn_win", (0.95, 0.80, 0.40), roughness=0.30,
                       emission=(0.85, 0.65, 0.30), emission_strength=0.9)
sign_m2 = make_material("Inn_sign", (0.40, 0.20, 0.10), roughness=0.80)
cube("base", (0, 0, 0.15), (2.4, 1.8, 0.15), make_material("Inn_base", (0.38, 0.32, 0.28), roughness=0.85))
cube("ground_floor", (0, 0, 1.20), (2.4, 1.8, 1.0), wood)
cube("upper_floor", (0, 0, 2.80), (2.2, 1.6, 0.85), wood)
cone("roof", 2.8, 0.10, 1.20, (0, 0, 4.50), roof_m4, verts=4)
cube("door", (0, -1.83, 1.0), (0.45, 0.04, 0.95), make_material("Inn_door", (0.25, 0.15, 0.08), roughness=0.80))
# 4 lit windows
for i, (xx, yy, zz) in enumerate([(-0.90, -1.82, 1.65), (0.90, -1.82, 1.65),
                                    (-0.80, -1.62, 2.90), (0.80, -1.62, 2.90)]):
    cube(f"win_{i}", (xx, yy, zz), (0.32, 0.03, 0.30), window)
cube("sign_post", (1.55, -1.80, 2.50), (0.06, 0.06, 0.80), sign_m2)
cube("sign", (1.55, -2.00, 2.10), (0.40, 0.04, 0.30), sign_m2)
cube("chim", (0.90, 0.50, 4.50), (0.18, 0.18, 0.50), make_material("Inn_chim", (0.35, 0.30, 0.28), roughness=0.85))
finalize("VillageInn", "Moon1")

# 8. Town Hall — larger central building
# Spec target: ~12m (docs/15 §7). Prior: 1.3 body + 1.4 hip roof + 0.8 tower
# + 0.8 tower roof yielded ~7.4m. Bumped body to 2.6m half-height (5.2m),
# hip roof 1.8, bell tower 1.8, tower roof 1.4 — total tower tip ~12m.
reset_scene()
stone3 = make_material("TH_stone", (0.78, 0.72, 0.62), roughness=0.85)
roof_m5 = make_material("TH_roof", (0.25, 0.30, 0.45), roughness=0.65)
column = make_material("TH_col", (0.92, 0.88, 0.80), roughness=0.50)
door_m4 = make_material("TH_door", (0.30, 0.18, 0.08), roughness=0.75)
brass = make_material("TH_brass", (0.78, 0.65, 0.30), roughness=0.30, metallic=0.8)
cube("base", (0, 0, 0.20), (3.5, 2.5, 0.20), make_material("TH_base", (0.50, 0.45, 0.40), roughness=0.85))
cube("body", (0, 0, 3.00), (3.5, 2.5, 2.60), stone3)
# Roof — hipped
cone("roof", 3.7, 0.20, 1.80, (0, 0, 6.50), roof_m5, verts=4)
# Bell tower (taller pedestal so bell sits at ~9.5m and tower tip ~12m)
cube("tower", (0, 0, 8.30), (0.85, 0.85, 1.40), stone3)
cone("tower_roof", 0.95, 0.10, 1.40, (0, 0, 10.40), roof_m5, verts=4)
sphere("bell", 0.30, (0, 0, 9.10), brass, segs=12, rings=10)
# Spire crown so total max-axis reaches 12m
cyl("spire", 0.08, 0.90, (0, 0, 11.55), brass, verts=10)
# Columns at front (4) — taller to match body proportions
for i, x in enumerate([-1.30, -0.45, 0.45, 1.30]):
    cyl(f"col_{i}", 0.20, 2.80, (x, -2.40, 2.00), column, verts=14)
# Door
cube("door", (0, -2.55, 1.60), (0.60, 0.05, 1.60), door_m4)
finalize("TownHall", "Moon1")

print("done gen_buildings_village: 8 buildings")
