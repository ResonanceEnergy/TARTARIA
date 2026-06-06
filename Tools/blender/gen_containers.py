"""10 containers — crate small/med/large, barrel small/large, sack burlap,
sack canvas, basket woven, jar clay, locked strongbox.
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
burlap = lambda n: make_material(n, (0.55, 0.45, 0.30), roughness=0.95)
canvas = lambda n: make_material(n, (0.78, 0.72, 0.55), roughness=0.85)
clay = lambda n: make_material(n, (0.65, 0.40, 0.25), roughness=0.85)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)

def crate(name, scale, moon="Shared"):
    reset_scene()
    w = 0.25 * scale
    # 6 face planks → simplify to 6 faces
    cube("box", (0, 0, w/2), (w, w, w/2), wood(name+"_b"))
    # Cross plank reinforcement on each visible face (2 horizontal bars)
    for z in (0.25*w, 0.75*w):
        cube(f"front_h_{z}", (0, -w-0.005, z), (w*1.05, 0.005, 0.02), iron(name+f"_fh{z}"))
    finalize(name, moon)

crate("CrateSmall", 0.6)
crate("CrateMed", 1.0)
crate("CrateLarge", 1.4)

# 4-5. Barrels
def barrel(name, scale, moon="Shared"):
    reset_scene()
    r_mid = 0.16 * scale
    r_end = 0.13 * scale
    h = 0.40 * scale
    # Body — cylinder
    cyl("body", r_mid, h, (0, 0, h/2), wood(name+"_b"), verts=20)
    # End caps tighter
    cyl("cap_t", r_end, 0.02, (0, 0, h - 0.01), wood(name+"_ct"), verts=18)
    cyl("cap_b", r_end, 0.02, (0, 0, 0.01), wood(name+"_cb"), verts=18)
    # 3 iron bands
    for z in (h*0.20, h*0.50, h*0.80):
        torus(f"band_{z}", r_mid+0.005, 0.008, (0, 0, z), iron(name+f"_b{z}"), mseg=20, miseg=4)
    finalize(name, moon)

barrel("BarrelSmall", 0.7)
barrel("BarrelLarge", 1.3)

# 6. Sack Burlap (puffy bag)
reset_scene()
sphere("body", 0.18, (0, 0, 0.16), burlap("SBu_b"), segs=14, rings=10)
# Top cinched
cyl("top", 0.08, 0.08, (0, 0, 0.32), burlap("SBu_t"), verts=12)
# Rope tie
torus("rope", 0.08, 0.012, (0, 0, 0.32), make_material("SBu_rope", (0.50, 0.40, 0.20), roughness=0.85), mseg=14, miseg=3)
finalize("SackBurlap")

# 7. Sack Canvas (smaller, neater)
reset_scene()
cube("body", (0, 0, 0.12), (0.15, 0.10, 0.12), canvas("SCa_b"))
cube("top", (0, 0, 0.26), (0.08, 0.06, 0.04), canvas("SCa_t"))
# Drawstring
torus("string", 0.07, 0.008, (0, 0, 0.26), make_material("SCa_str", (0.50, 0.40, 0.20), roughness=0.85), mseg=14, miseg=3)
finalize("SackCanvas")

# 8. Basket Woven
reset_scene()
weave = make_material("Bsk_w", (0.65, 0.50, 0.30), roughness=0.85)
cyl("body", 0.18, 0.20, (0, 0, 0.10), weave, verts=20)
# Rim
torus("rim", 0.19, 0.012, (0, 0, 0.20), weave, mseg=22, miseg=4)
# Handle (arched)
torus("handle", 0.10, 0.015, (0, 0, 0.30), weave, mseg=18, miseg=4, rot=(1.5708, 0, 0))
# Horizontal weave bands (3)
for z in (0.05, 0.10, 0.15):
    torus(f"weave_{z}", 0.18, 0.008, (0, 0, z), make_material(f"Bsk_h{z}", (0.55, 0.40, 0.20), roughness=0.85), mseg=20, miseg=3)
finalize("BasketWoven")

# 9. Jar Clay
reset_scene()
sphere("body", 0.12, (0, 0, 0.12), clay("Jar_b"), segs=16, rings=12)
cyl("neck", 0.05, 0.06, (0, 0, 0.25), clay("Jar_n"), verts=14)
torus("rim", 0.06, 0.012, (0, 0, 0.28), clay("Jar_r"), mseg=16, miseg=4)
# Decorative band of stripes (3 thin horizontals)
for z in (0.08, 0.12, 0.16):
    torus(f"stripe_{z}", 0.12, 0.004, (0, 0, z), make_material(f"Jar_s{z}", (0.30, 0.20, 0.10), roughness=0.85), mseg=20, miseg=3)
finalize("JarClay")

# 10. Locked Strongbox
reset_scene()
case_dark = make_material("Sb_case", (0.20, 0.18, 0.15), roughness=0.55, metallic=0.3)
cube("base", (0, 0, 0.18), (0.30, 0.22, 0.18), case_dark)
cube("lid_top", (0, 0, 0.37), (0.30, 0.22, 0.02), case_dark)
# Iron edge bands (4 vertical)
for x in (-0.30, 0.30):
    for y in (-0.22, 0.22):
        cube(f"corner_{x}_{y}", (x*0.99, y*0.99, 0.18), (0.03, 0.03, 0.20), iron(f"Sb_c{x}{y}"))
# Top bands
for x in (-0.16, 0.0, 0.16):
    cube(f"band_t_{x}", (x, 0, 0.37), (0.025, 0.22, 0.025), iron(f"Sb_bt{x}"))
# Lock plate
cube("lock", (0, -0.22, 0.20), (0.06, 0.005, 0.10), gold("Sb_lock"))
cyl("keyhole", 0.012, 0.02, (0, -0.23, 0.20), make_material("Sb_kh", (0.05, 0.05, 0.05), roughness=0.95), rot=(1.5708, 0, 0), verts=10)
# Rivets
for x in (-0.10, 0.10):
    for y in (-0.05, 0.05):
        sphere(f"rivet_{x}_{y}", 0.008, (x, -0.22, 0.20+y), iron(f"Sb_r{x}{y}"), segs=8, rings=6)
finalize("LockedStrongbox")

print("done gen_containers: 10 containers")
