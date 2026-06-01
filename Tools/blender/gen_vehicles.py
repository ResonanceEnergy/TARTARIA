"""15 vehicles + mounts — wagon, sled, raft, rowboat, sailboat, hot-air balloon basket,
palanquin, horse, donkey, ox, mule, ram, eagle (large), serpent, wolf.
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
darkwood = lambda n: make_material(n, (0.28, 0.18, 0.10), roughness=0.85)
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
fur_b = lambda n: make_material(n, (0.35, 0.22, 0.12), roughness=0.92)
fur_g = lambda n: make_material(n, (0.55, 0.50, 0.42), roughness=0.92)
fur_w = lambda n: make_material(n, (0.88, 0.85, 0.78), roughness=0.92)
cloth = lambda n, c: make_material(n, c, roughness=0.85)

# 1. Wagon (empty 4-wheeled)
reset_scene()
cube("bed", (0, 0, 0.55), (0.95, 0.45, 0.06), wood("Wag_bed"))
cube("wall_l", (0, -0.46, 0.72), (0.95, 0.04, 0.30), wood("Wag_wl"))
cube("wall_r", (0, 0.46, 0.72), (0.95, 0.04, 0.30), wood("Wag_wr"))
cube("wall_b", (-0.98, 0, 0.72), (0.04, 0.45, 0.30), wood("Wag_wb"))
cube("wall_f", (0.98, 0, 0.72), (0.04, 0.45, 0.30), wood("Wag_wf"))
for x in (-0.55, 0.55):
    for y in (-0.50, 0.50):
        torus(f"wheel_{x}_{y}", 0.30, 0.06, (x, y, 0.30), darkwood(f"Wag_w_{x}_{y}"), mseg=20, miseg=5, rot=(1.5708, 0, 0))
cube("tongue", (1.40, 0, 0.50), (0.40, 0.04, 0.04), wood("Wag_tongue"))
finalize("Wagon")

# 2. Sled
reset_scene()
cube("deck", (0, 0, 0.20), (0.60, 0.35, 0.05), wood("Sled_d"))
cube("runner_l", (0, -0.32, 0.10), (0.70, 0.04, 0.10), darkwood("Sled_rl"))
cube("runner_r", (0, 0.32, 0.10), (0.70, 0.04, 0.10), darkwood("Sled_rr"))
cube("brace_f", (0.30, 0, 0.15), (0.04, 0.35, 0.05), darkwood("Sled_bf"))
cube("brace_b", (-0.30, 0, 0.15), (0.04, 0.35, 0.05), darkwood("Sled_bb"))
# Curved handle
cyl("handle_l", 0.025, 0.30, (-0.65, -0.32, 0.30), darkwood("Sled_hl"), rot=(0.4, 0, 0), verts=10)
cyl("handle_r", 0.025, 0.30, (-0.65, 0.32, 0.30), darkwood("Sled_hr"), rot=(0.4, 0, 0), verts=10)
finalize("Sled")

# 3. Raft
reset_scene()
for i in range(6):
    y = -0.30 + i*0.12
    cyl(f"log_{i}", 0.08, 1.20, (0, y, 0.04), wood(f"Raft_l{i}"), rot=(0, 1.5708, 0), verts=12)
# Lashing rope at ends
cyl("rope_f", 0.018, 0.80, (0.50, 0, 0.10), make_material("Raft_rope", (0.50, 0.40, 0.20), roughness=0.85), rot=(1.5708, 0, 0), verts=8)
cyl("rope_b", 0.018, 0.80, (-0.50, 0, 0.10), make_material("Raft_rope2", (0.50, 0.40, 0.20), roughness=0.85), rot=(1.5708, 0, 0), verts=8)
finalize("Raft")

# 4. Rowboat
reset_scene()
hull = wood("RB_hull")
# Bottom plank
cube("bottom", (0, 0, 0.04), (1.20, 0.30, 0.04), hull)
# Curved sides — simplified as angled cubes
cube("side_l", (0, -0.32, 0.18), (1.10, 0.04, 0.18), hull, rot=(0.30, 0, 0))
cube("side_r", (0,  0.32, 0.18), (1.10, 0.04, 0.18), hull, rot=(-0.30, 0, 0))
# Pointed bow + stern
cone("bow", 0.20, 0.02, 0.20, (1.30, 0, 0.18), hull, rot=(0, 1.5708, 0), verts=8)
cube("stern", (-1.20, 0, 0.18), (0.06, 0.30, 0.18), hull)
# Seat
cube("seat", (0, 0, 0.30), (0.20, 0.30, 0.03), darkwood("RB_seat"))
# Oars
cyl("oar_l", 0.02, 1.00, (-0.10, -0.45, 0.45), wood("RB_ol"), rot=(0.30, 0, 0), verts=10)
cyl("oar_r", 0.02, 1.00, (-0.10,  0.45, 0.45), wood("RB_or"), rot=(-0.30, 0, 0), verts=10)
finalize("Rowboat")

# 5. Sailboat (small)
reset_scene()
hull = wood("SB_hull")
cube("bottom", (0, 0, 0.10), (1.50, 0.40, 0.10), hull)
cube("side_l", (0, -0.40, 0.30), (1.50, 0.04, 0.30), hull)
cube("side_r", (0,  0.40, 0.30), (1.50, 0.04, 0.30), hull)
cone("bow", 0.30, 0.04, 0.40, (1.70, 0, 0.30), hull, rot=(0, 1.5708, 0), verts=8)
# Mast
cyl("mast", 0.05, 1.80, (0.30, 0, 1.20), darkwood("SB_mast"), verts=12)
# Sail
cube("sail", (0.30, 0, 1.30), (0.04, 0.90, 0.80), make_material("SB_sail", (0.92, 0.90, 0.82), roughness=0.85))
# Boom
cyl("boom", 0.03, 1.20, (0.30, 0, 0.50), darkwood("SB_boom"), rot=(0, 1.5708, 0), verts=10)
finalize("Sailboat")

# 6. Hot-air balloon basket
reset_scene()
weave = make_material("BB_weave", (0.65, 0.50, 0.30), roughness=0.85)
rope = make_material("BB_rope", (0.50, 0.40, 0.20), roughness=0.85)
# Cylindrical basket
cyl("body", 0.45, 0.50, (0, 0, 0.25), weave, verts=20)
torus("rim_top", 0.46, 0.02, (0, 0, 0.50), darkwood("BB_rim_t"), mseg=22, miseg=4)
torus("rim_bot", 0.46, 0.02, (0, 0, 0.0), darkwood("BB_rim_b"), mseg=22, miseg=4)
# 4 mooring lines going up
for i in range(4):
    a = i*(math.pi/2)
    cyl(f"line_{i}", 0.015, 0.80, (math.cos(a)*0.45, math.sin(a)*0.45, 0.95), rope, rot=(0, math.cos(a)*0.1, math.sin(a)*0.1), verts=8)
finalize("BalloonBasket")

# 7. Palanquin (carried litter)
reset_scene()
red = cloth("Pal_red", (0.65, 0.18, 0.18))
gold = make_material("Pal_gold", (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
cube("box", (0, 0, 0.40), (0.50, 0.40, 0.40), red)
cube("roof", (0, 0, 0.82), (0.55, 0.45, 0.05), gold)
cone("finial", 0.10, 0.02, 0.20, (0, 0, 0.95), gold, verts=10)
# 2 carrying poles
cyl("pole_l", 0.025, 1.20, (0, -0.36, 0.50), wood("Pal_pl"), rot=(0, 1.5708, 0), verts=10)
cyl("pole_r", 0.025, 1.20, (0,  0.36, 0.50), wood("Pal_pr"), rot=(0, 1.5708, 0), verts=10)
# Curtain hint
cube("curtain", (0, -0.21, 0.40), (0.48, 0.02, 0.35), red)
finalize("Palanquin")

# 8. Horse (quadruped block)
def quadruped(name, body_color, height=1.0, head_extra=None, horn=False, wolf=False, moon="Shared"):
    reset_scene()
    body = make_material(name+"_body", body_color, roughness=0.90)
    h = height
    cube("torso", (0, 0, 0.80*h), (0.60*h, 0.25*h, 0.32*h), body)
    # 4 legs
    cyl("leg_fl", 0.06*h, 0.65*h, ( 0.45*h, -0.18*h, 0.40*h), body, verts=10)
    cyl("leg_fr", 0.06*h, 0.65*h, ( 0.45*h,  0.18*h, 0.40*h), body, verts=10)
    cyl("leg_bl", 0.06*h, 0.65*h, (-0.45*h, -0.18*h, 0.40*h), body, verts=10)
    cyl("leg_br", 0.06*h, 0.65*h, (-0.45*h,  0.18*h, 0.40*h), body, verts=10)
    # Neck
    cyl("neck", 0.10*h, 0.30*h, (0.60*h, 0, 0.95*h), body, rot=(0, -0.5, 0), verts=10)
    # Head
    cube("head", (0.85*h, 0, 1.05*h), (0.18*h, 0.10*h, 0.10*h), body)
    # Ears
    cone("ear_l", 0.04*h, 0.01, 0.08*h, (0.82*h, -0.06*h, 1.18*h), body, verts=6)
    cone("ear_r", 0.04*h, 0.01, 0.08*h, (0.82*h,  0.06*h, 1.18*h), body, verts=6)
    # Tail
    cyl("tail", 0.03*h, 0.30*h, (-0.65*h, 0, 0.90*h), body, rot=(0, 1.2, 0), verts=8)
    if horn:
        horn_m = make_material(name+"_horn", (0.60, 0.55, 0.40), roughness=0.65)
        cone("horn_l", 0.04*h, 0.01, 0.15*h, (0.80*h, -0.06*h, 1.20*h), horn_m, rot=(0, 0.3, 0), verts=6)
        cone("horn_r", 0.04*h, 0.01, 0.15*h, (0.80*h,  0.06*h, 1.20*h), horn_m, rot=(0, 0.3, 0), verts=6)
    if wolf:
        eye = make_material(name+"_eye", (0.95, 0.85, 0.10), roughness=0.20, emission=(0.95, 0.85, 0.10), emission_strength=1.0)
        sphere("eye_l", 0.012*h, (0.92*h, -0.05*h, 1.07*h), eye, segs=8, rings=6)
        sphere("eye_r", 0.012*h, (0.92*h,  0.05*h, 1.07*h), eye, segs=8, rings=6)
    finalize(name, moon)

quadruped("Horse",   (0.45, 0.30, 0.18), height=1.0)
quadruped("Donkey",  (0.50, 0.45, 0.38), height=0.85)
quadruped("Ox",      (0.55, 0.40, 0.30), height=1.05, horn=True)
quadruped("Mule",    (0.40, 0.35, 0.28), height=0.92)
quadruped("Ram",     (0.85, 0.80, 0.72), height=0.80, horn=True)
quadruped("Wolf",    (0.45, 0.42, 0.38), height=0.70, wolf=True)

# 14. Eagle (large mountable bird — body + wings + head)
reset_scene()
brown = make_material("Eagle_b", (0.40, 0.28, 0.15), roughness=0.90)
white = make_material("Eagle_w", (0.92, 0.88, 0.80), roughness=0.90)
beak = make_material("Eagle_bk", (0.92, 0.75, 0.20), roughness=0.50, metallic=0.4)
eye = make_material("Eagle_e", (0.90, 0.70, 0.10), roughness=0.20, emission=(0.95, 0.75, 0.15), emission_strength=1.0)
# Body
cube("body", (0, 0, 0.50), (0.50, 0.20, 0.18), brown)
# Head
sphere("head", 0.16, (0.55, 0, 0.65), white, segs=14, rings=10)
cone("beak", 0.04, 0.01, 0.12, (0.72, 0, 0.62), beak, rot=(0, 1.5708, 0), verts=8)
sphere("eye_l", 0.025, (0.62, -0.10, 0.70), eye, segs=8, rings=6)
sphere("eye_r", 0.025, (0.62,  0.10, 0.70), eye, segs=8, rings=6)
# Wings (spread)
cube("wing_l", (-0.10, -0.75, 0.55), (0.30, 0.55, 0.04), brown)
cube("wing_r", (-0.10,  0.75, 0.55), (0.30, 0.55, 0.04), brown)
# Tail feathers
cube("tail", (-0.55, 0, 0.50), (0.20, 0.15, 0.03), white)
# Talons (4)
for x in (0.10, -0.10):
    for y in (-0.10, 0.10):
        cone(f"talon_{x}_{y}", 0.025, 0.005, 0.10, (x, y, 0.32), beak, verts=6)
finalize("Eagle")

# 15. Serpent (large, coiled)
reset_scene()
scale_m = make_material("Serp_scale", (0.20, 0.55, 0.30), roughness=0.55, metallic=0.2)
glow_e = make_material("Serp_eye", (0.95, 0.20, 0.10), roughness=0.20, emission=(0.95, 0.20, 0.10), emission_strength=1.5)
# Coiled body — 5 stacked rings
for i in range(5):
    r = 0.40 - i*0.04
    torus(f"coil_{i}", r, 0.10 - i*0.008, (0, 0, 0.15 + i*0.18), scale_m, mseg=24, miseg=8)
# Head — slightly elongated sphere
sphere("head", 0.18, (0.20, 0, 1.10), scale_m, segs=16, rings=12)
sphere("eye_l", 0.025, (0.30, -0.10, 1.18), glow_e, segs=8, rings=6)
sphere("eye_r", 0.025, (0.30,  0.10, 1.18), glow_e, segs=8, rings=6)
# Fangs (2 small cones)
cone("fang_l", 0.012, 0.002, 0.06, (0.30, -0.05, 1.02), make_material("Serp_fang", (0.95, 0.92, 0.85), roughness=0.30), verts=6)
cone("fang_r", 0.012, 0.002, 0.06, (0.30,  0.05, 1.02), make_material("Serp_fang2", (0.95, 0.92, 0.85), roughness=0.30), verts=6)
finalize("SerpentLarge")

print("done gen_vehicles: 15 vehicles+mounts")
