"""10 ritual / esoteric items — tuning fork x3 sizes, aether vial, spell tome,
hourglass, pocket watch, compass, telescope, lantern set.
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

brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
glass = lambda n, c, em: make_material(n, c, roughness=0.15, metallic=0.1, emission=em, emission_strength=1.5)

def tuning_fork(name, scale, glow_color, moon="Shared"):
    reset_scene()
    fork = brass(name+"_b")
    handle_h = 0.18 * scale
    prong_h = 0.40 * scale
    sep = 0.06 * scale
    cyl("handle", 0.025*scale, handle_h, (0, 0, handle_h/2), fork, verts=12)
    cube("yoke", (0, 0, handle_h + 0.02), (sep + 0.025, 0.025*scale, 0.02), fork)
    cyl("prong_l", 0.015*scale, prong_h, (-sep, 0, handle_h + 0.04 + prong_h/2), fork, verts=10)
    cyl("prong_r", 0.015*scale, prong_h, ( sep, 0, handle_h + 0.04 + prong_h/2), fork, verts=10)
    # Resonance glow ball above
    sphere("glow", 0.04*scale, (0, 0, handle_h + prong_h + 0.15), glass(name+"_g", (0.0,0.0,0.0), glow_color), segs=12, rings=10)
    finalize(name, moon)

# 1-3. Tuning fork sizes (small, medium, large) — E/A/D Aether bands
tuning_fork("TuningForkSmall_E3", 0.75, (0.30, 0.55, 0.95))   # cool blue
tuning_fork("TuningForkMed_A3", 1.00, (0.95, 0.65, 0.20))     # amber
tuning_fork("TuningForkLarge_D4", 1.30, (0.40, 0.85, 0.50))   # pale green

# 4. Aether Vial — glowing crystal vial
reset_scene()
vial = make_material("AV_glass", (0.90, 0.95, 1.0), roughness=0.10, metallic=0.0)
aether = make_material("AV_aether", (0.40, 0.70, 0.95), roughness=0.15,
                        emission=(0.60, 0.85, 1.0), emission_strength=2.0)
cork = wood("AV_cork")
sphere("body", 0.06, (0, 0, 0.08), vial, segs=14, rings=12)
cyl("neck", 0.02, 0.06, (0, 0, 0.18), vial, verts=12)
cyl("cork", 0.022, 0.04, (0, 0, 0.23), cork, verts=10)
# Liquid inside
sphere("liquid", 0.05, (0, 0, 0.08), aether, segs=14, rings=10)
finalize("AetherVial")

# 5. Spell Tome (closed book + lock)
reset_scene()
leather = make_material("ST_leather", (0.42, 0.18, 0.12), roughness=0.80)
gold = make_material("ST_gold", (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
pages = make_material("ST_pages", (0.95, 0.88, 0.75), roughness=0.85)
cube("cover_t", (0, 0, 0.06), (0.18, 0.25, 0.02), leather)
cube("cover_b", (0, 0, 0.0),  (0.18, 0.25, 0.02), leather)
cube("pages",   (0, 0, 0.03), (0.17, 0.24, 0.04), pages)
cube("spine",   (-0.18, 0, 0.03), (0.02, 0.25, 0.06), leather)
# Gold corner trim
for x in (-0.17, 0.17):
    for y in (-0.23, 0.23):
        cube(f"trim_{x}_{y}", (x, y, 0.07), (0.02, 0.02, 0.005), gold)
# Lock
cube("lock", (0.16, 0, 0.05), (0.02, 0.03, 0.04), gold)
# Embossed star
sphere("emboss", 0.04, (0, 0, 0.08), gold, segs=10, rings=8)
finalize("SpellTome")

# 6. Hourglass
reset_scene()
glass_m = make_material("HG_glass", (0.92, 0.92, 0.95), roughness=0.10, metallic=0.1)
sand = make_material("HG_sand", (0.92, 0.78, 0.50), roughness=0.85)
frame = wood("HG_frame")
# Top + bottom bulbs as cones meeting
cone("bulb_t", 0.06, 0.01, 0.10, (0, 0, 0.25), glass_m, verts=14)
cone("bulb_b", 0.01, 0.06, 0.10, (0, 0, 0.15), glass_m, verts=14)
cone("bulb_t2", 0.05, 0.01, 0.08, (0, 0, 0.26), sand, verts=12)
# Frame
cube("plate_t", (0, 0, 0.32), (0.10, 0.10, 0.015), frame)
cube("plate_b", (0, 0, 0.08), (0.10, 0.10, 0.015), frame)
for i in range(4):
    a = i*(math.pi/2)
    cyl(f"post_{i}", 0.008, 0.24, (math.cos(a)*0.08, math.sin(a)*0.08, 0.20), frame, verts=8)
finalize("Hourglass")

# 7. Pocket Watch (open clamshell)
reset_scene()
case = brass("PW_case")
face = make_material("PW_face", (0.95, 0.92, 0.85), roughness=0.60)
hand = make_material("PW_hand", (0.10, 0.10, 0.10), roughness=0.40, metallic=0.3)
chain = brass("PW_chain")
cyl("case_back", 0.06, 0.01, (0, 0, 0.0), case, verts=18)
cyl("face", 0.055, 0.005, (0, 0, 0.012), face, verts=18)
# Hands
cube("hour_h", (0, 0.012, 0.015), (0.005, 0.025, 0.002), hand)
cube("min_h",  (0, 0.012, 0.015), (0.003, 0.040, 0.002), hand, rot=(0, 0, 0.6))
# Crown
cyl("crown", 0.008, 0.012, (0, 0.065, 0.0), case, verts=10)
# Chain — torus
torus("chain", 0.08, 0.004, (0.04, 0.10, 0.0), chain, mseg=16, miseg=4, rot=(1.5708, 0, 0))
finalize("PocketWatch")

# 8. Compass
reset_scene()
cyl("case", 0.10, 0.025, (0, 0, 0.012), brass("Comp_c"), verts=18)
cyl("face", 0.092, 0.003, (0, 0, 0.026), make_material("Comp_face", (0.95, 0.92, 0.85), roughness=0.65), verts=18)
# Needle
cube("needle_n", (0, 0.04, 0.030), (0.01, 0.06, 0.005), make_material("Comp_n", (0.85, 0.20, 0.15), roughness=0.40))
cube("needle_s", (0, -0.04, 0.030), (0.01, 0.06, 0.005), make_material("Comp_s", (0.20, 0.30, 0.55), roughness=0.40))
# Glass cover
cyl("glass", 0.094, 0.005, (0, 0, 0.034), make_material("Comp_glass", (0.92, 0.95, 0.98), roughness=0.05, metallic=0.05), verts=18)
finalize("Compass")

# 9. Telescope (handheld brass)
reset_scene()
cyl("body", 0.05, 0.40, (0, 0, 0.20), brass("Tel_b"), verts=18)
cyl("extension", 0.04, 0.30, (0, 0, 0.55), brass("Tel_e"), verts=16)
cyl("eyepiece", 0.025, 0.06, (0, 0, 0.72), brass("Tel_ep"), verts=12)
# Front lens
cyl("lens_f", 0.05, 0.01, (0, 0, 0.005), make_material("Tel_lens", (0.20, 0.40, 0.55), roughness=0.10, metallic=0.2), verts=18)
finalize("Telescope")

# 10. Aether Lantern (variant of HangingLantern with emission)
reset_scene()
brass_m = brass("AL_brass")
glass_m = make_material("AL_glass", (0.95, 0.85, 0.50), roughness=0.10, metallic=0.0,
                          emission=(1.0, 0.85, 0.50), emission_strength=2.0)
cone("top", 0.06, 0.04, 0.08, (0, 0, 0.30), brass_m, verts=10)
cyl("frame_top", 0.07, 0.02, (0, 0, 0.24), brass_m, verts=12)
cyl("frame_bot", 0.07, 0.02, (0, 0, 0.04), brass_m, verts=12)
# Glass body
cyl("glass", 0.065, 0.20, (0, 0, 0.14), glass_m, verts=14)
# 4 vertical struts
for i in range(4):
    a = i*(math.pi/2)
    cyl(f"strut_{i}", 0.005, 0.20, (math.cos(a)*0.07, math.sin(a)*0.07, 0.14), brass_m, verts=6)
# Top ring for hanging
torus("ring", 0.04, 0.005, (0, 0, 0.36), brass_m, mseg=14, miseg=4)
finalize("AetherLantern")

print("done gen_props_ritual: 10 items")
