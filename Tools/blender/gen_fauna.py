"""10 fauna — fish trout, fish koi, fish bass, turtle, frog, butterfly,
dragonfly, sparrow, raven, owl.
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

def fish(name, body_color, fin_color, length=0.30):
    reset_scene()
    body = make_material(name+"_b", body_color, roughness=0.40, metallic=0.2)
    fin = make_material(name+"_f", fin_color, roughness=0.50)
    eye = make_material(name+"_e", (0.05, 0.05, 0.05), roughness=0.30)
    # Elongated body
    sphere("body", length*0.5, (0, 0, 0.10), body, segs=18, rings=12)
    # Tail fin
    cube("tail", (-length*0.55, 0, 0.10), (length*0.10, 0.025, length*0.30), fin)
    # Dorsal
    cube("dorsal", (0, 0, 0.18), (length*0.20, 0.012, length*0.12), fin)
    # Eye
    sphere("eye", 0.015, (length*0.30, 0.05, 0.13), eye, segs=8, rings=6)
    # Pectoral fins
    cube("pec_l", (length*0.10, -0.04, 0.08), (length*0.08, 0.012, 0.04), fin, rot=(0, 0, -0.3))
    cube("pec_r", (length*0.10,  0.04, 0.08), (length*0.08, 0.012, 0.04), fin, rot=(0, 0,  0.3))
    finalize(name)

# 1-3. Fish (trout, koi, bass)
fish("FishTrout", (0.45, 0.55, 0.50), (0.55, 0.45, 0.35), length=0.30)
fish("FishKoi",   (0.92, 0.55, 0.25), (0.95, 0.85, 0.70), length=0.40)
fish("FishBass",  (0.30, 0.40, 0.20), (0.40, 0.45, 0.35), length=0.45)

# 4. Turtle
reset_scene()
shell_top = make_material("Tur_st", (0.30, 0.45, 0.20), roughness=0.75)
shell_bot = make_material("Tur_sb", (0.65, 0.55, 0.30), roughness=0.85)
skin = make_material("Tur_sk", (0.45, 0.55, 0.30), roughness=0.85)
sphere("shell_top", 0.18, (0, 0, 0.12), shell_top, segs=18, rings=12)
cyl("shell_bot", 0.18, 0.04, (0, 0, 0.04), shell_bot, verts=18)
# Head
sphere("head", 0.06, (0.20, 0, 0.08), skin, segs=10, rings=8)
# Legs
for x, y in [(-0.15, -0.15), (-0.15, 0.15), (0.10, -0.15), (0.10, 0.15)]:
    cube(f"leg_{x}_{y}", (x, y, 0.04), (0.04, 0.04, 0.04), skin)
# Tail
cone("tail", 0.025, 0.005, 0.06, (-0.22, 0, 0.08), skin, rot=(0, -1.5708, 0), verts=6)
finalize("Turtle")

# 5. Frog
reset_scene()
green = make_material("Frog_g", (0.30, 0.65, 0.30), roughness=0.55)
pale = make_material("Frog_p", (0.85, 0.92, 0.65), roughness=0.55)
eye_y = make_material("Frog_e", (0.92, 0.85, 0.30), roughness=0.20)
eye_b = make_material("Frog_eb", (0.05, 0.05, 0.05), roughness=0.30)
# Body
sphere("body", 0.10, (0, 0, 0.08), green, segs=14, rings=10)
# Underbelly
sphere("belly", 0.09, (0, 0, 0.05), pale, segs=12, rings=10)
# Eyes (bulging on top)
sphere("eye_socket_l", 0.025, (0.04, 0.05, 0.16), green, segs=10, rings=8)
sphere("eye_socket_r", 0.025, (-0.04, 0.05, 0.16), green, segs=10, rings=8)
sphere("eye_l", 0.018, (0.04, 0.06, 0.17), eye_y, segs=8, rings=6)
sphere("eye_r", 0.018, (-0.04, 0.06, 0.17), eye_y, segs=8, rings=6)
sphere("pupil_l", 0.008, (0.04, 0.07, 0.17), eye_b, segs=6, rings=4)
sphere("pupil_r", 0.008, (-0.04, 0.07, 0.17), eye_b, segs=6, rings=4)
# Hind legs folded
cube("leg_l", (-0.08, -0.06, 0.06), (0.04, 0.04, 0.06), green, rot=(0, 0, 0.4))
cube("leg_r", ( 0.08, -0.06, 0.06), (0.04, 0.04, 0.06), green, rot=(0, 0, -0.4))
finalize("Frog")

# 6. Butterfly
reset_scene()
body = make_material("Bf_body", (0.10, 0.10, 0.10), roughness=0.80)
wing_p = make_material("Bf_wp", (0.95, 0.45, 0.20), roughness=0.50)
wing_y = make_material("Bf_wy", (0.95, 0.85, 0.20), roughness=0.50)
# Body
cyl("body", 0.012, 0.10, (0, 0, 0.05), body, verts=8)
# Wings (upper + lower, 2 each)
cube("wing_u_l", (-0.08, 0, 0.06), (0.08, 0.005, 0.06), wing_p)
cube("wing_u_r", ( 0.08, 0, 0.06), (0.08, 0.005, 0.06), wing_p)
cube("wing_d_l", (-0.06, 0, 0.02), (0.06, 0.005, 0.04), wing_y)
cube("wing_d_r", ( 0.06, 0, 0.02), (0.06, 0.005, 0.04), wing_y)
# Antennae
cyl("ant_l", 0.002, 0.04, (-0.01, 0, 0.12), body, rot=(0.3, 0, -0.2), verts=6)
cyl("ant_r", 0.002, 0.04, ( 0.01, 0, 0.12), body, rot=(0.3, 0,  0.2), verts=6)
finalize("Butterfly")

# 7. Dragonfly
reset_scene()
body_g = make_material("Df_b", (0.20, 0.55, 0.45), roughness=0.40, metallic=0.3, emission=(0.30, 0.65, 0.55), emission_strength=0.5)
wing = make_material("Df_w", (0.85, 0.90, 0.92), roughness=0.10, metallic=0.05)
# Long body
cyl("body", 0.012, 0.18, (0, 0, 0.04), body_g, rot=(0, 1.5708, 0), verts=10)
# Head
sphere("head", 0.022, (0.10, 0, 0.04), body_g, segs=10, rings=8)
# Eyes (large compound)
sphere("eye_l", 0.018, (0.10, 0.025, 0.05), make_material("Df_e", (0.30, 0.40, 0.20), roughness=0.30, metallic=0.4), segs=8, rings=6)
sphere("eye_r", 0.018, (0.10, -0.025, 0.05), make_material("Df_e2", (0.30, 0.40, 0.20), roughness=0.30, metallic=0.4), segs=8, rings=6)
# 4 wings (transparent)
cube("wing_fl", (0.04, -0.08, 0.06), (0.07, 0.04, 0.003), wing, rot=(0, 0, -0.3))
cube("wing_fr", (0.04,  0.08, 0.06), (0.07, 0.04, 0.003), wing, rot=(0, 0,  0.3))
cube("wing_bl", (-0.02, -0.08, 0.06), (0.06, 0.035, 0.003), wing, rot=(0, 0, -0.4))
cube("wing_br", (-0.02,  0.08, 0.06), (0.06, 0.035, 0.003), wing, rot=(0, 0,  0.4))
finalize("Dragonfly")

# 8. Sparrow
reset_scene()
brown = make_material("Sp_b", (0.55, 0.42, 0.25), roughness=0.85)
tan = make_material("Sp_t", (0.85, 0.72, 0.50), roughness=0.85)
beak = make_material("Sp_bk", (0.45, 0.30, 0.15), roughness=0.50)
sphere("body", 0.08, (0, 0, 0.12), brown, segs=14, rings=10)
sphere("head", 0.06, (0.06, 0, 0.18), brown, segs=12, rings=10)
sphere("belly", 0.07, (0, -0.02, 0.10), tan, segs=12, rings=10)
cone("beak", 0.018, 0.002, 0.04, (0.13, 0, 0.18), beak, rot=(0, 1.5708, 0), verts=6)
sphere("eye", 0.008, (0.09, 0.03, 0.20), make_material("Sp_e", (0.05, 0.05, 0.05), roughness=0.30), segs=6, rings=4)
# Wings tucked
cube("wing_l", (0, 0.06, 0.13), (0.06, 0.015, 0.05), brown)
cube("wing_r", (0, -0.06, 0.13), (0.06, 0.015, 0.05), brown)
# Tail
cube("tail", (-0.08, 0, 0.13), (0.05, 0.03, 0.012), brown)
# Legs
cyl("leg_l", 0.004, 0.05, (0, 0.02, 0.04), beak, verts=6)
cyl("leg_r", 0.004, 0.05, (0, -0.02, 0.04), beak, verts=6)
finalize("Sparrow")

# 9. Raven
reset_scene()
black = make_material("Rv_b", (0.05, 0.05, 0.07), roughness=0.65)
sphere("body", 0.12, (0, 0, 0.15), black, segs=14, rings=10)
sphere("head", 0.08, (0.10, 0, 0.25), black, segs=12, rings=10)
cone("beak", 0.025, 0.005, 0.08, (0.22, 0, 0.25), make_material("Rv_bk", (0.10, 0.10, 0.10), roughness=0.50, metallic=0.4), rot=(0, 1.5708, 0), verts=8)
sphere("eye_l", 0.012, (0.14, 0.05, 0.28), make_material("Rv_e", (0.92, 0.20, 0.10), roughness=0.20, emission=(1.0, 0.20, 0.10), emission_strength=0.8), segs=8, rings=6)
sphere("eye_r", 0.012, (0.14, -0.05, 0.28), make_material("Rv_e2", (0.92, 0.20, 0.10), roughness=0.20, emission=(1.0, 0.20, 0.10), emission_strength=0.8), segs=8, rings=6)
cube("wing_l", (0, 0.10, 0.16), (0.10, 0.020, 0.08), black)
cube("wing_r", (0, -0.10, 0.16), (0.10, 0.020, 0.08), black)
cube("tail", (-0.13, 0, 0.16), (0.08, 0.05, 0.018), black)
cyl("leg_l", 0.006, 0.06, (0, 0.03, 0.05), make_material("Rv_l", (0.15, 0.10, 0.05), roughness=0.50), verts=6)
cyl("leg_r", 0.006, 0.06, (0, -0.03, 0.05), make_material("Rv_l2", (0.15, 0.10, 0.05), roughness=0.50), verts=6)
finalize("Raven")

# 10. Owl
reset_scene()
feather_t = make_material("Ow_ft", (0.62, 0.50, 0.32), roughness=0.85)
feather_p = make_material("Ow_fp", (0.95, 0.92, 0.85), roughness=0.85)
beak = make_material("Ow_bk", (0.55, 0.40, 0.15), roughness=0.50)
eye_y = make_material("Ow_e", (0.95, 0.78, 0.20), roughness=0.20, emission=(0.95, 0.78, 0.20), emission_strength=1.0)
sphere("body", 0.16, (0, 0, 0.18), feather_t, segs=16, rings=12)
sphere("body_b", 0.14, (0, -0.02, 0.16), feather_p, segs=14, rings=10)
sphere("head", 0.12, (0, -0.02, 0.36), feather_t, segs=14, rings=10)
# Face disk
sphere("face", 0.10, (0, -0.10, 0.36), feather_p, segs=12, rings=10)
# Eyes
sphere("eye_l", 0.035, (-0.05, -0.14, 0.38), eye_y, segs=10, rings=8)
sphere("eye_r", 0.035, ( 0.05, -0.14, 0.38), eye_y, segs=10, rings=8)
sphere("pupil_l", 0.012, (-0.05, -0.16, 0.38), make_material("Ow_pl", (0.05, 0.05, 0.05), roughness=0.30), segs=6, rings=4)
sphere("pupil_r", 0.012, ( 0.05, -0.16, 0.38), make_material("Ow_pr", (0.05, 0.05, 0.05), roughness=0.30), segs=6, rings=4)
# Beak
cone("beak", 0.018, 0.002, 0.03, (0, -0.16, 0.34), beak, rot=(1.5708, 0, 0), verts=6)
# Wings tucked
cube("wing_l", (-0.12, 0, 0.18), (0.04, 0.06, 0.12), feather_t)
cube("wing_r", ( 0.12, 0, 0.18), (0.04, 0.06, 0.12), feather_t)
# Tufts ("horns")
cone("tuft_l", 0.012, 0.001, 0.04, (-0.06, -0.02, 0.46), feather_t, verts=6)
cone("tuft_r", 0.012, 0.001, 0.04, ( 0.06, -0.02, 0.46), feather_t, verts=6)
finalize("Owl")

print("done gen_fauna: 10 creatures")
