"""15 architectural details — archway, doorway-door, window-stained-glass,
staircase, balcony, gable, 3 pillar styles, buttress, arch keystone, gargoyle,
weather vane, finial, dormer.
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

stone = lambda n: make_material(n, (0.62, 0.58, 0.52), roughness=0.85)
stone_d = lambda n: make_material(n, (0.45, 0.42, 0.38), roughness=0.85)
wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
glass = lambda n, c, em: make_material(n, c, roughness=0.15, emission=em, emission_strength=1.5)

# 1. Archway (free-standing)
reset_scene()
cube("col_l", (-0.40, 0, 1.0), (0.10, 0.20, 1.0), stone("AW_cl"))
cube("col_r", ( 0.40, 0, 1.0), (0.10, 0.20, 1.0), stone("AW_cr"))
torus("arch", 0.40, 0.10, (0, 0, 2.0), stone("AW_arch"), mseg=22, miseg=6, rot=(1.5708, 0, 0))
# Keystone
cube("keystone", (0, 0, 2.20), (0.10, 0.21, 0.10), stone_d("AW_key"))
finalize("Archway")

# 2. Doorway With Door
reset_scene()
cube("frame_l", (-0.42, 0, 1.10), (0.04, 0.10, 1.10), stone("DD_fl"))
cube("frame_r", ( 0.42, 0, 1.10), (0.04, 0.10, 1.10), stone("DD_fr"))
cube("frame_top", (0, 0, 2.20), (0.46, 0.10, 0.10), stone("DD_ft"))
cube("door", (0, -0.04, 1.10), (0.36, 0.04, 1.05), wood("DD_door"))
# Iron straps
for z in (0.40, 1.10, 1.80):
    cube(f"strap_{z}", (0, -0.06, z), (0.36, 0.005, 0.04), iron(f"DD_s{z}"))
# Handle ring
torus("handle", 0.04, 0.008, (0.10, -0.07, 1.10), iron("DD_hand"), mseg=14, miseg=3)
# Nail studs
for x in (-0.12, 0.0, 0.12):
    for z in (0.40, 1.10, 1.80):
        sphere(f"stud_{x}_{z}", 0.010, (x, -0.065, z), iron(f"DD_st{x}{z}"), segs=8, rings=6)
finalize("DoorwayWithDoor")

# 3. Window Stained Glass (rose pattern)
reset_scene()
cube("frame_t", (0, 0, 0.45), (0.50, 0.05, 0.04), stone_d("WSG_ft"))
cube("frame_b", (0, 0, -0.45), (0.50, 0.05, 0.04), stone_d("WSG_fb"))
cube("frame_l", (-0.46, 0, 0), (0.04, 0.05, 0.50), stone_d("WSG_fl"))
cube("frame_r", ( 0.46, 0, 0), (0.04, 0.05, 0.50), stone_d("WSG_fr"))
# Cross-mullion
cube("mull_h", (0, 0, 0.0), (0.44, 0.05, 0.02), stone_d("WSG_mh"))
cube("mull_v", (0, 0, 0.0), (0.02, 0.05, 0.44), stone_d("WSG_mv"))
# 4 stained glass panels (4 colors)
cube("p_tl", (-0.22, 0.0, 0.22), (0.20, 0.02, 0.20), glass("WSG_tl", (0.85, 0.20, 0.20), (0.85, 0.20, 0.20)))
cube("p_tr", ( 0.22, 0.0, 0.22), (0.20, 0.02, 0.20), glass("WSG_tr", (0.20, 0.30, 0.85), (0.20, 0.30, 0.85)))
cube("p_bl", (-0.22, 0.0, -0.22), (0.20, 0.02, 0.20), glass("WSG_bl", (0.20, 0.80, 0.30), (0.20, 0.80, 0.30)))
cube("p_br", ( 0.22, 0.0, -0.22), (0.20, 0.02, 0.20), glass("WSG_br", (0.95, 0.80, 0.20), (0.95, 0.80, 0.20)))
finalize("WindowStainedGlass")

# 4. Staircase (6 steps)
reset_scene()
for i in range(6):
    z = i*0.20 + 0.10
    cube(f"step_{i}", (0, -0.10*i, z), (0.50, 0.20, 0.10), stone(f"St_s{i}"))
# Banister
cube("rail_l", (-0.55, -0.50, 0.85), (0.04, 1.50, 0.04), wood("St_rl"), rot=(0.5, 0, 0))
cube("rail_r", ( 0.55, -0.50, 0.85), (0.04, 1.50, 0.04), wood("St_rr"), rot=(0.5, 0, 0))
finalize("Staircase")

# 5. Balcony (stone with balusters)
reset_scene()
cube("slab", (0, 0, 0.10), (0.80, 0.30, 0.10), stone("Bal_slab"))
cube("rail_top", (0, 0.15, 0.50), (0.80, 0.04, 0.04), stone("Bal_rt"))
# Balusters (6)
for i in range(6):
    x = -0.30 + i*0.12
    cyl(f"bal_{i}", 0.025, 0.40, (x, 0.15, 0.30), stone(f"Bal_b{i}"), verts=10)
# Corner posts
cube("post_l", (-0.38, 0.15, 0.30), (0.04, 0.04, 0.40), stone_d("Bal_pl"))
cube("post_r", ( 0.38, 0.15, 0.30), (0.04, 0.04, 0.40), stone_d("Bal_pr"))
finalize("BalconyRail")

# 6. Gable End (triangular roof piece)
reset_scene()
# 4 stones — large triangle approximated by stacked cones
cone("gable", 0.50, 0.10, 0.40, (0, 0, 0.50), stone("Gb_g"), verts=3)
cube("base_beam", (0, 0, 0.20), (0.50, 0.10, 0.04), wood("Gb_bb"))
# Decorative finial
cone("finial", 0.04, 0.005, 0.10, (0, 0, 0.85), stone_d("Gb_fin"), verts=8)
finalize("GableEnd")

# 7-9. Pillars (3 styles: Doric, Ionic, Corinthian)
def pillar(name, style):
    reset_scene()
    s = stone(name+"_s")
    # Base
    cube("base", (0, 0, 0.05), (0.18, 0.18, 0.05), s)
    # Shaft
    cyl("shaft", 0.12, 2.0, (0, 0, 1.10), s, verts=20)
    # Capital — varies by style
    if style == "Doric":
        cyl("cap", 0.14, 0.06, (0, 0, 2.13), s, verts=18)
        cube("abacus", (0, 0, 2.19), (0.16, 0.16, 0.03), s)
    elif style == "Ionic":
        cyl("cap_base", 0.14, 0.04, (0, 0, 2.12), s, verts=18)
        # Volutes (2 swirls front)
        sphere("vol_l", 0.05, (-0.10, 0, 2.15), s, segs=12, rings=10)
        sphere("vol_r", 0.05, ( 0.10, 0, 2.15), s, segs=12, rings=10)
        cube("abacus", (0, 0, 2.20), (0.18, 0.18, 0.03), s)
    elif style == "Corinthian":
        # Bell-shaped with acanthus (approximated by torus rings)
        cyl("cap_base", 0.14, 0.18, (0, 0, 2.19), s, verts=18)
        for i in range(4):
            a = i*(math.pi/2)
            sphere(f"leaf_{i}", 0.06, (math.cos(a)*0.13, math.sin(a)*0.13, 2.16), s, segs=12, rings=10)
        cube("abacus", (0, 0, 2.30), (0.18, 0.18, 0.03), s)
    finalize(name)

pillar("PillarDoric", "Doric")
pillar("PillarIonic", "Ionic")
pillar("PillarCorinthian", "Corinthian")

# 10. Buttress (flying)
reset_scene()
# Pier (lower vertical)
cube("pier", (0, 0, 1.5), (0.20, 0.30, 1.5), stone("Bu_pier"))
# Flying arm
cube("arm", (-0.40, 0, 2.3), (0.50, 0.20, 0.20), stone("Bu_arm"), rot=(0, -0.5, 0))
# Cap stone
cone("cap", 0.18, 0.05, 0.20, (0, 0, 3.10), stone_d("Bu_cap"), verts=4)
finalize("FlyingButtress")

# 11. Arch Keystone (decorated)
reset_scene()
cube("body", (0, 0, 0.10), (0.10, 0.20, 0.20), stone("AK_b"))
# Carved face — sphere with shadow eyes
sphere("face", 0.08, (0, -0.12, 0.18), stone_d("AK_face"), segs=12, rings=10)
sphere("eye_l", 0.012, (-0.025, -0.18, 0.20), make_material("AK_e", (0.05, 0.05, 0.05), roughness=0.95), segs=6, rings=4)
sphere("eye_r", 0.012, ( 0.025, -0.18, 0.20), make_material("AK_e2", (0.05, 0.05, 0.05), roughness=0.95), segs=6, rings=4)
finalize("ArchKeystone")

# 12. Gargoyle (water spout)
reset_scene()
g = stone_d("Gg_b")
# Crouching body
cube("body", (0, 0, 0.15), (0.12, 0.20, 0.15), g)
# Head with open mouth (spout)
sphere("head", 0.10, (0, -0.18, 0.20), g, segs=12, rings=10)
cyl("mouth", 0.025, 0.08, (0, -0.28, 0.20), g, rot=(1.5708, 0, 0), verts=10)
# Wings folded
cube("wing_l", (-0.10, 0.06, 0.25), (0.06, 0.06, 0.12), g, rot=(0, 0, 0.3))
cube("wing_r", ( 0.10, 0.06, 0.25), (0.06, 0.06, 0.12), g, rot=(0, 0, -0.3))
# Horns
cone("horn_l", 0.018, 0.005, 0.06, (-0.05, -0.15, 0.30), g, rot=(0.3, 0, 0), verts=6)
cone("horn_r", 0.018, 0.005, 0.06, ( 0.05, -0.15, 0.30), g, rot=(0.3, 0, 0), verts=6)
# Claws
for x in (-0.06, 0.06):
    cone(f"claw_{x}", 0.012, 0.003, 0.04, (x, -0.10, 0.04), g, verts=6)
finalize("Gargoyle")

# 13. Weather Vane
reset_scene()
pole = make_material("WV_pole", (0.20, 0.18, 0.15), roughness=0.55, metallic=0.4)
cyl("pole", 0.012, 0.80, (0, 0, 0.40), pole, verts=10)
# Cardinal cross arms (NSEW)
cube("ns", (0, 0, 0.70), (0.005, 0.30, 0.005), pole)
cube("ew", (0, 0, 0.70), (0.30, 0.005, 0.005), pole)
# Letter markers
for label_pos, label_color in [((0, 0.15, 0.70), gold("WV_N"))]:
    sphere("N_dot", 0.012, label_pos, label_color, segs=8, rings=6)
# Rooster
sphere("rooster_body", 0.05, (0, 0, 0.85), make_material("WV_rb", (0.85, 0.40, 0.20), roughness=0.40, metallic=0.6), segs=12, rings=10)
cube("rooster_tail", (-0.06, 0, 0.88), (0.05, 0.005, 0.06), make_material("WV_rt", (0.65, 0.30, 0.15), roughness=0.50, metallic=0.5))
cone("rooster_comb", 0.018, 0.005, 0.025, (0.02, 0, 0.91), make_material("WV_rc", (0.85, 0.10, 0.10), roughness=0.40), verts=6)
finalize("WeatherVane")

# 14. Finial (decorative roof topper)
reset_scene()
gold_m = gold("Fn_g")
# Tiered cone
cyl("base", 0.06, 0.04, (0, 0, 0.02), gold_m, verts=14)
sphere("ball_1", 0.05, (0, 0, 0.09), gold_m, segs=12, rings=10)
cone("cone", 0.04, 0.005, 0.10, (0, 0, 0.18), gold_m, verts=10)
sphere("ball_2", 0.025, (0, 0, 0.26), gold_m, segs=10, rings=8)
cone("tip", 0.02, 0.002, 0.06, (0, 0, 0.32), gold_m, verts=8)
finalize("Finial")

# 15. Dormer (roof window jutting out)
reset_scene()
roof_m = make_material("Dm_roof", (0.25, 0.30, 0.45), roughness=0.65)
wall_m = make_material("Dm_wall", (0.85, 0.78, 0.55), roughness=0.85)
# Side walls
cube("wall_l", (-0.20, 0.0, 0.20), (0.04, 0.30, 0.20), wall_m)
cube("wall_r", ( 0.20, 0.0, 0.20), (0.04, 0.30, 0.20), wall_m)
# Front wall with window
cube("front", (0, -0.30, 0.20), (0.20, 0.04, 0.20), wall_m)
# Window
cube("window", (0, -0.32, 0.20), (0.12, 0.02, 0.14), glass("Dm_w", (0.40, 0.55, 0.75), (0.30, 0.45, 0.65)))
# Pitched roof (triangle approximated)
cone("roof", 0.30, 0.04, 0.20, (0, 0, 0.50), roof_m, verts=4)
finalize("Dormer")

print("done gen_arch_details: 15 details")
