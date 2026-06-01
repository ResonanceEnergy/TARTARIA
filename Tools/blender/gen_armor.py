"""10 armor pieces — 3 helmets, 2 breastplates, gauntlets, pauldrons, greaves,
boots, kite shield.
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

steel = lambda n: make_material(n, (0.78, 0.78, 0.82), roughness=0.35, metallic=0.85)
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
leather = lambda n: make_material(n, (0.40, 0.22, 0.10), roughness=0.85)
plume = lambda n: make_material(n, (0.55, 0.20, 0.20), roughness=0.85)

# 1. Knight Helm (with visor)
reset_scene()
sphere("dome", 0.15, (0, 0, 0.20), steel("KH_d"), segs=14, rings=12)
# Visor (frontal piece with slit)
cube("visor", (0, -0.08, 0.18), (0.12, 0.04, 0.10), steel("KH_v"))
cube("slit", (0, -0.10, 0.20), (0.10, 0.02, 0.012), make_material("KH_slit", (0.05, 0.05, 0.05), roughness=0.95))
# Plume holder
cyl("plume_base", 0.012, 0.04, (0, 0, 0.36), iron("KH_pb"), verts=8)
# Plume
cube("plume", (0, 0.04, 0.40), (0.025, 0.04, 0.08), plume("KH_plume"))
finalize("HelmKnight")

# 2. Roman Helm
reset_scene()
sphere("cap", 0.13, (0, 0, 0.18), iron("RH_cap"), segs=14, rings=12)
# Cheek guards
cube("cheek_l", (-0.10, -0.05, 0.10), (0.025, 0.06, 0.06), iron("RH_cl"))
cube("cheek_r", ( 0.10, -0.05, 0.10), (0.025, 0.06, 0.06), iron("RH_cr"))
# Crest (red plume strip front-to-back)
cube("crest", (0, 0, 0.32), (0.012, 0.12, 0.04), plume("RH_crest"))
# Neck guard (back flap)
cube("neck", (0, 0.10, 0.10), (0.10, 0.025, 0.05), iron("RH_n"))
finalize("HelmRoman")

# 3. Open Pot Helm
reset_scene()
cyl("body", 0.13, 0.15, (0, 0, 0.10), iron("PH_b"), verts=18)
cone("top", 0.13, 0.06, 0.08, (0, 0, 0.22), iron("PH_t"), verts=18)
# Brow ridge
torus("brow", 0.135, 0.012, (0, 0, 0.10), iron("PH_brow"), mseg=18, miseg=4)
finalize("HelmPot")

# 4. Breastplate (full)
reset_scene()
# Front plate (chest)
cube("front", (0, -0.08, 0.40), (0.30, 0.04, 0.40), steel("BP_front"))
# Curved chest detail (sphere)
sphere("chest", 0.18, (0, -0.10, 0.42), steel("BP_chest"), segs=14, rings=12)
# Back plate
cube("back", (0, 0.08, 0.40), (0.30, 0.04, 0.40), steel("BP_back"))
# Shoulder straps
cube("strap_l", (-0.20, 0, 0.65), (0.04, 0.10, 0.04), leather("BP_sl"))
cube("strap_r", ( 0.20, 0, 0.65), (0.04, 0.10, 0.04), leather("BP_sr"))
# Decorative cross emblem
cube("cross_v", (0, -0.12, 0.45), (0.012, 0.005, 0.10), gold("BP_cv"))
cube("cross_h", (0, -0.12, 0.45), (0.04, 0.005, 0.012), gold("BP_ch"))
finalize("BreastplateFull")

# 5. Breastplate (Lamellar)
reset_scene()
# 4 rows × 6 cols of small overlapping plates
for row in range(5):
    for col in range(6):
        x = -0.20 + col*0.075
        z = 0.20 + row*0.10
        cube(f"plate_{row}_{col}", (x, -0.04, z), (0.035, 0.005, 0.045), iron(f"BL_p{row}{col}"))
# Backplate (single piece)
cube("back", (0, 0.06, 0.40), (0.24, 0.02, 0.42), iron("BL_back"))
finalize("BreastplateLamellar")

# 6. Gauntlets (pair)
reset_scene()
def gauntlet(x_off, mat_prefix):
    cube(f"wrist_{mat_prefix}", (x_off, 0, 0.12), (0.05, 0.05, 0.10), steel(mat_prefix+"_w"))
    cube(f"back_{mat_prefix}", (x_off, -0.02, 0.18), (0.045, 0.025, 0.06), steel(mat_prefix+"_back"))
    # 4 finger plates
    for i in range(4):
        x = x_off + (i-1.5)*0.012
        cube(f"finger_{mat_prefix}_{i}", (x, -0.03, 0.22), (0.005, 0.015, 0.04), steel(f"{mat_prefix}_f{i}"))
gauntlet(-0.15, "GL")
gauntlet( 0.15, "GR")
finalize("GauntletsPair")

# 7. Pauldrons (shoulder pair)
reset_scene()
def pauldron(x_off, mat_prefix):
    sphere(f"shoulder_{mat_prefix}", 0.12, (x_off, 0, 0.20), steel(mat_prefix+"_s"), segs=14, rings=10)
    # 3 articulated lames going down
    for i in range(3):
        z = 0.10 - i*0.06
        cube(f"lame_{mat_prefix}_{i}", (x_off, 0, z), (0.13 - i*0.005, 0.025, 0.025), iron(f"{mat_prefix}_l{i}"))
pauldron(-0.20, "PL")
pauldron( 0.20, "PR")
finalize("PauldronsPair")

# 8. Greaves (shin armor pair)
reset_scene()
def greave(x_off, mat_prefix):
    cube(f"shin_{mat_prefix}", (x_off, -0.02, 0.30), (0.05, 0.025, 0.30), steel(mat_prefix+"_s"))
    cube(f"shin_inner_{mat_prefix}", (x_off, 0.02, 0.30), (0.05, 0.012, 0.28), iron(f"{mat_prefix}_i"))
    # Knee cap
    sphere(f"knee_{mat_prefix}", 0.06, (x_off, -0.01, 0.55), steel(mat_prefix+"_k"), segs=12, rings=10)
greave(-0.10, "GrL")
greave( 0.10, "GrR")
finalize("GreavesPair")

# 9. Armored Boots (pair)
reset_scene()
def boot(x_off, mat_prefix):
    # Sole
    cube(f"sole_{mat_prefix}", (x_off, 0.04, 0.02), (0.05, 0.12, 0.02), leather(mat_prefix+"_sole"))
    # Upper
    cube(f"upper_{mat_prefix}", (x_off, 0.04, 0.10), (0.05, 0.10, 0.06), leather(mat_prefix+"_u"))
    # Steel plates
    cube(f"plate_top_{mat_prefix}", (x_off, 0.04, 0.14), (0.05, 0.10, 0.02), steel(mat_prefix+"_pt"))
    cube(f"plate_toe_{mat_prefix}", (x_off, -0.06, 0.04), (0.05, 0.03, 0.04), steel(mat_prefix+"_pte"))
boot(-0.10, "BL")
boot( 0.10, "BR")
finalize("ArmoredBootsPair")

# 10. Kite Shield (tall)
reset_scene()
plank = make_material("KS_plank", (0.55, 0.40, 0.20), roughness=0.85)
boss = iron("KS_boss")
rim = iron("KS_rim")
# Body — taper from wide top to point bottom
cube("body_top", (0, 0, 0.50), (0.20, 0.04, 0.25), plank)
cube("body_mid", (0, 0, 0.20), (0.18, 0.04, 0.20), plank)
cone("body_pt", 0.18, 0.02, 0.20, (0, 0, -0.10), plank, rot=(1.5708, 0, 0), verts=8)
# Cross emblem
cube("cross_v", (0, -0.04, 0.30), (0.012, 0.005, 0.40), gold("KS_cv"))
cube("cross_h", (0, -0.04, 0.40), (0.18, 0.005, 0.012), gold("KS_ch"))
# Rim trim
torus("rim_top", 0.21, 0.008, (0, 0, 0.75), rim, mseg=20, miseg=4)
# Center boss
sphere("boss", 0.05, (0, -0.05, 0.40), boss, segs=12, rings=10)
finalize("KiteShield")

print("done gen_armor: 10 armor pieces")
