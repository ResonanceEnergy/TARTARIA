"""4 utility extras to push deliverables past 100:
WoodenSign, CartFull, ChestStudded, BannerWall.
Per CLAUDE.md no-stubs mandate — real geometry.
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

wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
cloth_b = lambda n: make_material(n, (0.20, 0.30, 0.65), roughness=0.80)

# 1. Wooden Sign on Post
reset_scene()
cyl("post", 0.04, 1.50, (0, 0, 0.75), wood("WS_p"), verts=10)
cube("plank", (0, 0, 1.35), (0.35, 0.04, 0.20), wood("WS_pl"))
cyl("nail_1", 0.01, 0.04, (-0.30, -0.04, 1.40), iron("WS_n1"), rot=(1.5708, 0, 0), verts=8)
cyl("nail_2", 0.01, 0.04, ( 0.30, -0.04, 1.40), iron("WS_n2"), rot=(1.5708, 0, 0), verts=8)
cyl("nail_3", 0.01, 0.04, (-0.30, -0.04, 1.30), iron("WS_n3"), rot=(1.5708, 0, 0), verts=8)
cyl("nail_4", 0.01, 0.04, ( 0.30, -0.04, 1.30), iron("WS_n4"), rot=(1.5708, 0, 0), verts=8)
finalize("WoodenSign")

# 2. Cart Full (cart with goods piled)
reset_scene()
cube("bed", (0, 0, 0.55), (0.70, 0.40, 0.06), wood("CF_bed"))
# Side walls
cube("wall_l", (0, -0.42, 0.70), (0.72, 0.04, 0.30), wood("CF_wl"))
cube("wall_r", (0, 0.42, 0.70), (0.72, 0.04, 0.30), wood("CF_wr"))
cube("wall_back", (-0.74, 0, 0.70), (0.04, 0.40, 0.30), wood("CF_wb"))
# Wheels (2)
torus("wheel_l", 0.30, 0.06, (-0.20, -0.46, 0.30), wood("CF_w1"), mseg=24, miseg=6, rot=(1.5708, 0, 0))
torus("wheel_r", 0.30, 0.06, (-0.20, 0.46, 0.30), wood("CF_w2"), mseg=24, miseg=6, rot=(1.5708, 0, 0))
# Cargo: sacks, barrels
sphere("sack_1", 0.14, (0.30, -0.10, 0.92), make_material("CF_sk1", (0.55, 0.45, 0.32), roughness=0.85), segs=12, rings=10)
sphere("sack_2", 0.14, (0.30,  0.18, 0.92), make_material("CF_sk2", (0.50, 0.42, 0.30), roughness=0.85), segs=12, rings=10)
cyl("barrel", 0.13, 0.22, (-0.20, 0.10, 0.91), wood("CF_bar"), verts=14)
finalize("CartFull")

# 3. Chest Studded (treasure chest with iron studs)
reset_scene()
body = wood("CS_body")
band = iron("CS_band")
gold = brass("CS_gold")
# Body
cube("base", (0, 0, 0.15), (0.30, 0.20, 0.15), body)
# Lid (curved — use 2 cubes)
cube("lid_base", (0, 0, 0.32), (0.30, 0.20, 0.03), body)
cube("lid_top", (0, 0, 0.38), (0.28, 0.18, 0.06), body)
# Iron bands (3 across top + 3 down sides)
cube("band_top1", (-0.20, 0, 0.40), (0.02, 0.21, 0.07), band)
cube("band_top2", (0, 0, 0.40), (0.02, 0.21, 0.07), band)
cube("band_top3", (0.20, 0, 0.40), (0.02, 0.21, 0.07), band)
cube("band_front", (0, -0.20, 0.20), (0.30, 0.02, 0.15), band)
# Lock
cube("lock", (0, -0.22, 0.30), (0.04, 0.02, 0.06), gold)
# Studs (rivets)
for x in (-0.26, -0.10, 0.10, 0.26):
    for z in (0.08, 0.22):
        sphere(f"stud_{x}_{z}", 0.012, (x, -0.21, z), band, segs=8, rings=6)
finalize("ChestStudded")

# 4. Wall Banner (long hanging banner, more decorative than tapestry)
reset_scene()
border = make_material("WB_border", (0.92, 0.75, 0.25), roughness=0.40, metallic=0.4)
field = cloth_b("WB_field")
emblem = make_material("WB_em", (0.95, 0.92, 0.85), roughness=0.65)
# Top horizontal rod
cube("rod", (0, 0.04, 1.30), (0.50, 0.04, 0.03), make_material("WB_rod", (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6))
# Long cloth body
cube("field", (0, 0.02, 0.65), (0.40, 0.02, 0.65), field)
# Border trim
cube("border_t", (0, 0.01, 1.27), (0.42, 0.02, 0.04), border)
cube("border_b", (0, 0.01, 0.05), (0.42, 0.02, 0.04), border)
# Pointed bottom (single triangle)
cone("tail", 0.40, 0.02, 0.15, (0, 0.02, -0.05), field, rot=(1.5708, 0, 0), verts=3)
# Star emblem center
sphere("emb_center", 0.10, (0, 0.0, 0.85), emblem, segs=14, rings=10)
for i in range(6):
    a = i*(math.pi/3)
    cube(f"emb_ray_{i}", (math.cos(a)*0.16, 0.0, 0.85+math.sin(a)*0.16),
         (0.04, 0.02, 0.03), emblem)
finalize("WallBanner")

print("done gen_extras_utility: 4 extras")
