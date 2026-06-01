"""15 musical instruments — flute, lute, harp, hand drum, tambourine, gong,
didgeridoo, ocarina, fiddle, bagpipe, music box, glass armonica, kalimba,
theremin, rattle.
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

wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.65)
darkwood = lambda n: make_material(n, (0.25, 0.15, 0.08), roughness=0.60)
brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
silver = lambda n: make_material(n, (0.85, 0.85, 0.88), roughness=0.30, metallic=0.85)
skin = lambda n: make_material(n, (0.85, 0.72, 0.50), roughness=0.80)
string = lambda n: make_material(n, (0.90, 0.85, 0.70), roughness=0.50)

# 1. Flute
reset_scene()
cyl("body", 0.018, 0.40, (0, 0, 0.20), silver("Fl_b"), rot=(0, 1.5708, 0), verts=14)
for x in (-0.10, -0.05, 0.0, 0.05, 0.10, 0.15):
    sphere(f"hole_{x}", 0.008, (x, 0, 0.218), make_material(f"Fl_h{x}", (0.05, 0.05, 0.05), roughness=0.9), segs=8, rings=6)
finalize("Flute")

# 2. Lute (pear-shaped body)
reset_scene()
body = wood("Lu_body")
sphere("bowl", 0.20, (0, 0, 0.30), body, segs=18, rings=12)
cube("face", (0.05, 0, 0.30), (0.02, 0.20, 0.25), darkwood("Lu_face"))
# Neck
cyl("neck", 0.025, 0.40, (-0.45, 0, 0.30), darkwood("Lu_neck"), rot=(0, 1.5708, 0), verts=12)
# Headstock + tuning pegs
cube("head", (-0.70, 0, 0.30), (0.06, 0.03, 0.10), darkwood("Lu_head"))
for y in (-0.04, 0.0, 0.04):
    cyl(f"peg_{y}", 0.008, 0.06, (-0.70, y, 0.40), brass(f"Lu_p{y}"), rot=(1.5708, 0, 0), verts=8)
# Strings
for y in (-0.02, 0.0, 0.02):
    cube(f"str_{y}", (-0.20, y, 0.31), (0.50, 0.002, 0.002), string(f"Lu_s{y}"))
finalize("Lute")

# 3. Harp
reset_scene()
frame = wood("Hp_frame")
str_g = make_material("Hp_str", (0.95, 0.85, 0.30), roughness=0.20, metallic=0.4)
# Curved column (top)
cyl("top_curve", 0.04, 0.40, (0, 0, 1.20), frame, rot=(0, 0.7, 0), verts=12)
# Pillar
cyl("pillar", 0.06, 1.20, (-0.35, 0, 0.60), frame, verts=14)
# Soundboard (slanted)
cube("soundboard", (0.15, 0, 0.55), (0.05, 0.16, 0.60), frame, rot=(0, -0.5, 0))
# Base
cube("base", (-0.10, 0, 0.05), (0.40, 0.18, 0.05), frame)
# Strings (8)
for i in range(8):
    x = -0.30 + i*0.075
    z_base = 0.10 + i*0.10
    z_top = 1.0 + i*0.04
    cube(f"str_{i}", ((x+0.10)*0.5, 0, (z_base+z_top)*0.5), (0.005, 0.002, abs(z_top-z_base)*0.5), str_g, rot=(0, 0.1+i*0.04, 0))
finalize("Harp")

# 4. Hand Drum
reset_scene()
cyl("body", 0.16, 0.10, (0, 0, 0.05), wood("HD_b"), verts=20)
cyl("skin_top", 0.16, 0.005, (0, 0, 0.105), skin("HD_st"), verts=20)
# Lashing rope
torus("rope", 0.165, 0.005, (0, 0, 0.05), make_material("HD_rope", (0.50, 0.40, 0.20), roughness=0.85), mseg=22, miseg=4)
finalize("HandDrum")

# 5. Tambourine
reset_scene()
torus("ring", 0.16, 0.025, (0, 0, 0.025), wood("Tb_ring"), mseg=22, miseg=6)
cyl("skin_face", 0.155, 0.003, (0, 0, 0.05), skin("Tb_face"), verts=20)
# 6 jingle disks around rim
for i in range(6):
    a = i*(math.pi/3)
    cyl(f"jingle_{i}", 0.022, 0.003, (math.cos(a)*0.16, math.sin(a)*0.16, 0.05), brass(f"Tb_j{i}"), verts=12)
finalize("Tambourine")

# 6. Gong
reset_scene()
gong_m = make_material("Gn_gong", (0.78, 0.60, 0.28), roughness=0.40, metallic=0.7)
cyl("disc", 0.45, 0.03, (0, 0, 0.80), gong_m, verts=24)
# Boss (center bump)
sphere("boss", 0.10, (0, 0, 0.84), gong_m, segs=14, rings=10)
# Stand — 2 vertical posts + cross bar
cube("post_l", (-0.55, 0, 0.45), (0.04, 0.04, 0.45), darkwood("Gn_pl"))
cube("post_r", ( 0.55, 0, 0.45), (0.04, 0.04, 0.45), darkwood("Gn_pr"))
cube("bar", (0, 0, 0.95), (0.55, 0.04, 0.04), darkwood("Gn_bar"))
cube("foot_l", (-0.55, 0, 0.02), (0.20, 0.10, 0.02), darkwood("Gn_fl"))
cube("foot_r", ( 0.55, 0, 0.02), (0.20, 0.10, 0.02), darkwood("Gn_fr"))
# Mallet leaning
cyl("mallet", 0.015, 0.40, (0.55, 0.20, 0.30), wood("Gn_mallet"), rot=(0.5, 0, 0), verts=10)
sphere("mallet_head", 0.04, (0.55, 0.40, 0.55), make_material("Gn_mh", (0.65, 0.50, 0.35), roughness=0.85), segs=10, rings=8)
finalize("Gong")

# 7. Didgeridoo (long horn)
reset_scene()
cyl("body", 0.04, 1.20, (0, 0, 0.06), darkwood("Dd_b"), rot=(0, 1.5708, 0), verts=14)
cyl("flare", 0.07, 0.10, (0.65, 0, 0.06), wood("Dd_flare"), rot=(0, 1.5708, 0), verts=16)
# Decoration band
torus("band", 0.045, 0.005, (0.0, 0, 0.06), brass("Dd_band"), mseg=14, miseg=4, rot=(0, 1.5708, 0))
finalize("Didgeridoo")

# 8. Ocarina
reset_scene()
clay = make_material("Oc_clay", (0.70, 0.45, 0.30), roughness=0.85)
sphere("body", 0.06, (0, 0, 0.06), clay, segs=14, rings=10)
# Mouthpiece
cyl("mouth", 0.012, 0.05, (-0.06, 0, 0.06), clay, rot=(0, 1.5708, 0), verts=8)
# Holes
for i, x in enumerate([-0.03, -0.01, 0.01, 0.03]):
    sphere(f"hole_{i}", 0.008, (x, -0.04, 0.085), make_material(f"Oc_h{i}", (0.05, 0.05, 0.05), roughness=0.9), segs=8, rings=6)
finalize("Ocarina")

# 9. Fiddle (small violin)
reset_scene()
body = wood("Fd_b")
# Curved body — 2 stacked rounded shapes
sphere("upper", 0.14, (0.15, 0, 0.0), body, segs=14, rings=10)
sphere("lower", 0.18, (-0.10, 0, 0.0), body, segs=14, rings=10)
# Neck
cyl("neck", 0.02, 0.30, (0.40, 0, 0.0), darkwood("Fd_n"), rot=(0, 1.5708, 0), verts=10)
# Scroll
sphere("scroll", 0.03, (0.58, 0, 0.0), darkwood("Fd_s"), segs=10, rings=8)
# Bridge
cube("bridge", (0.10, 0, 0.05), (0.02, 0.06, 0.02), darkwood("Fd_br"))
# Strings (4)
for y in (-0.02, -0.006, 0.006, 0.02):
    cube(f"str_{y}", (0.20, y, 0.06), (0.40, 0.002, 0.002), string(f"Fd_s{y}"))
# Bow
cyl("bow_stick", 0.008, 0.60, (0.20, 0.15, 0.10), darkwood("Fd_bow"), rot=(0, 1.5708, 0), verts=8)
cube("bow_hair", (0.20, 0.15, 0.08), (0.30, 0.005, 0.005), make_material("Fd_hair", (0.92, 0.88, 0.75), roughness=0.85))
finalize("Fiddle")

# 10. Bagpipe (skirling drone)
reset_scene()
plaid = make_material("BP_plaid", (0.55, 0.20, 0.20), roughness=0.85)
chanter = darkwood("BP_chant")
# Bag (large sphere)
sphere("bag", 0.18, (0, 0, 0.50), plaid, segs=16, rings=12)
# Drones (3 tubes sticking out top)
for i, x in enumerate([-0.10, 0.0, 0.10]):
    cyl(f"drone_{i}", 0.018, 0.50, (x, 0, 0.85), chanter, verts=10)
    sphere(f"drone_top_{i}", 0.025, (x, 0, 1.10), brass(f"BP_dt{i}"), segs=10, rings=8)
# Chanter (front pipe down)
cyl("chanter", 0.018, 0.35, (0, -0.20, 0.30), chanter, verts=10)
# Blowpipe
cyl("blowpipe", 0.012, 0.20, (0.18, 0, 0.62), chanter, rot=(0, -0.5, 0), verts=10)
finalize("Bagpipe")

# 11. Music Box (wood case with crank)
reset_scene()
case = wood("MB_case")
gold = make_material("MB_gold", (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
cube("base", (0, 0, 0.05), (0.18, 0.12, 0.05), case)
cube("body", (0, 0, 0.12), (0.16, 0.10, 0.07), case)
cube("lid", (0, 0, 0.20), (0.16, 0.10, 0.02), darkwood("MB_lid"))
# Crank
cyl("crank_axle", 0.008, 0.06, (0.16, 0, 0.13), gold, rot=(0, 1.5708, 0), verts=8)
cube("crank_arm", (0.21, 0, 0.13), (0.02, 0.005, 0.03), gold)
# Mechanism cylinder visible inside (small ridges)
cyl("mech", 0.04, 0.10, (0, 0, 0.14), gold, rot=(0, 1.5708, 0), verts=14)
finalize("MusicBox")

# 12. Glass Armonica
reset_scene()
glass = make_material("GA_glass", (0.92, 0.95, 0.98), roughness=0.10, metallic=0.1, emission=(0.40, 0.65, 0.85), emission_strength=0.5)
wood_m = darkwood("GA_wood")
# Stand
cube("base", (0, 0, 0.05), (0.40, 0.15, 0.05), wood_m)
cube("frame", (0, 0, 0.30), (0.40, 0.04, 0.25), wood_m)
# 7 glass bowls of decreasing size, nested
for i in range(7):
    r = 0.13 - i*0.012
    x = -0.30 + i*0.10
    sphere(f"bowl_{i}", r, (x, 0, 0.30), glass, segs=14, rings=10)
finalize("GlassArmonica")

# 13. Kalimba
reset_scene()
case = wood("Ka_case")
prong = make_material("Ka_prong", (0.78, 0.78, 0.80), roughness=0.30, metallic=0.7)
cube("box", (0, 0, 0.04), (0.10, 0.14, 0.04), case)
# 8 metal prongs
for i in range(8):
    x = -0.06 + i*0.017
    cube(f"prong_{i}", (x, 0.04, 0.08), (0.005, 0.10 - abs(i-3.5)*0.012, 0.003), prong)
# Bridge
cube("bridge", (0, 0.05, 0.07), (0.10, 0.02, 0.015), darkwood("Ka_br"))
finalize("Kalimba")

# 14. Theremin (futuristic — antenna + box)
reset_scene()
case = darkwood("Th_case")
antenna = silver("Th_ant")
cube("box", (0, 0, 0.10), (0.20, 0.14, 0.08), case)
cyl("antenna_v", 0.008, 0.50, (0.18, 0, 0.40), antenna, verts=10)
cyl("antenna_h", 0.008, 0.30, (-0.18, 0.05, 0.20), antenna, rot=(0, 1.5708, 0), verts=10)
# Glow indicator
sphere("led", 0.012, (0, -0.05, 0.18), make_material("Th_led", (0.95, 0.30, 0.20), roughness=0.20, emission=(1.0, 0.40, 0.20), emission_strength=2.5), segs=8, rings=6)
finalize("Theremin")

# 15. Rattle (gourd-style)
reset_scene()
gourd = make_material("Rt_gourd", (0.85, 0.72, 0.45), roughness=0.85)
sphere("head", 0.10, (0, 0, 0.20), gourd, segs=14, rings=10)
cyl("handle", 0.025, 0.20, (0, 0, 0.05), wood("Rt_handle"), verts=10)
# Decorative band
torus("band", 0.10, 0.005, (0, 0, 0.20), make_material("Rt_band", (0.55, 0.20, 0.20), roughness=0.80), mseg=18, miseg=4)
finalize("Rattle")

print("done gen_instruments: 15 instruments")
