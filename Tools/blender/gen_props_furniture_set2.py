"""12 decor/civic furniture — banner pole, heraldic shield, candle holders,
pillar capital, stone urn, garden statue, sundial, fountain head, wind chime,
tapestry, painting frame, mirror.
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
stone = lambda n: make_material(n, (0.75, 0.72, 0.65), roughness=0.85)
gold = lambda n: make_material(n, (0.92, 0.75, 0.25), roughness=0.30, metallic=0.85)
brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
cloth_r = lambda n: make_material(n, (0.65, 0.18, 0.20), roughness=0.85)

# 1. Banner Pole — town/festival
reset_scene()
cyl("pole", 0.04, 2.5, (0, 0, 1.25), wood("BP_w"), verts=10)
sphere("top", 0.06, (0, 0, 2.55), gold("BP_g"), segs=10, rings=8)
cube("flag", (0.30, 0, 2.20), (0.30, 0.02, 0.40), cloth_r("BP_f"))
finalize("BannerPole")

# 2. Heraldic Shield (wall-mounted)
reset_scene()
sh = make_material("HS_field", (0.45, 0.12, 0.16), roughness=0.65)
sh_g = make_material("HS_gold", (0.92, 0.75, 0.25), roughness=0.30, metallic=0.8)
sh_iron = make_material("HS_iron", (0.32, 0.30, 0.28), roughness=0.55, metallic=0.6)
# Kite shape via 2 cones + cube
cube("body", (0, 0, 0.40), (0.30, 0.04, 0.40), sh)
cone("tip", 0.30, 0.02, 0.30, (0, 0, -0.05), sh, rot=(1.5708, 0, 0), verts=8)
torus("rim", 0.32, 0.015, (0, 0, 0.40), sh_iron, mseg=18, miseg=4, rot=(1.5708, 0, 0))
# Cross emblem
cube("cross_v", (0, -0.05, 0.40), (0.04, 0.01, 0.25), sh_g)
cube("cross_h", (0, -0.05, 0.40), (0.18, 0.01, 0.04), sh_g)
finalize("HeraldicShield")

# 3. Candle Holder (table)
reset_scene()
cyl("base", 0.10, 0.04, (0, 0, 0.02), brass("CHT_b"), verts=16)
cyl("stem", 0.025, 0.18, (0, 0, 0.13), brass("CHT_s"), verts=12)
cyl("cup", 0.05, 0.04, (0, 0, 0.24), brass("CHT_c"), verts=14)
cyl("candle", 0.025, 0.12, (0, 0, 0.32), make_material("CHT_wax", (0.95, 0.92, 0.85), roughness=0.65), verts=10)
cone("flame", 0.03, 0.005, 0.05, (0, 0, 0.42), make_material("CHT_flame", (1.0, 0.7, 0.2), roughness=0.10, emission=(1.0, 0.7, 0.2), emission_strength=3.0), verts=8)
finalize("CandleHolderTable")

# 4. Candle Holder (wall sconce — different from existing WallSconceIron)
reset_scene()
cube("plate", (0, 0.04, 0.0), (0.10, 0.02, 0.10), brass("CHW_p"))
cyl("arm", 0.02, 0.18, (0, -0.06, 0.0), brass("CHW_a"), rot=(1.5708, 0, 0), verts=10)
cyl("cup", 0.05, 0.04, (0, -0.18, 0.0), brass("CHW_c"), verts=14)
cyl("candle", 0.025, 0.14, (0, -0.18, 0.10), make_material("CHW_wax", (0.95, 0.92, 0.85), roughness=0.65), verts=10)
cone("flame", 0.03, 0.005, 0.05, (0, -0.18, 0.20), make_material("CHW_flame", (1.0, 0.7, 0.2), roughness=0.10, emission=(1.0, 0.7, 0.2), emission_strength=3.0), verts=8)
finalize("CandleHolderWall")

# 5. Pillar Capital (decorative column top)
reset_scene()
cyl("base", 0.30, 0.06, (0, 0, 0.03), stone("PC_b"))
# Volutes (4 corners — scroll spirals approximated by small spheres)
for i in range(4):
    a = i*(math.pi/2)
    sphere(f"volute_{i}", 0.06, (math.cos(a)*0.28, math.sin(a)*0.28, 0.10), stone(f"PC_v{i}"), segs=10, rings=8)
cube("abacus", (0, 0, 0.18), (0.32, 0.32, 0.05), stone("PC_a"))
finalize("PillarCapital")

# 6. Stone Urn
reset_scene()
sphere("body", 0.30, (0, 0, 0.35), stone("Urn_b"), segs=18, rings=14)
cyl("neck", 0.10, 0.10, (0, 0, 0.70), stone("Urn_n"), verts=14)
torus("rim", 0.12, 0.02, (0, 0, 0.75), stone("Urn_r"), mseg=18, miseg=4)
cyl("base", 0.18, 0.05, (0, 0, 0.02), stone("Urn_base"), verts=14)
finalize("StoneUrn")

# 7. Garden Statue (cherub on pedestal)
reset_scene()
cube("pedestal", (0, 0, 0.30), (0.18, 0.18, 0.30), stone("GS_p"))
# Figure
sphere("head", 0.10, (0, 0, 0.70), stone("GS_h"), segs=12, rings=10)
cube("torso", (0, 0, 0.50), (0.10, 0.06, 0.18), stone("GS_t"))
cyl("arm_l", 0.03, 0.16, (-0.10, 0, 0.50), stone("GS_al"), rot=(0, 0.5, 0), verts=8)
cyl("arm_r", 0.03, 0.16, ( 0.10, 0, 0.50), stone("GS_ar"), rot=(0, -0.5, 0), verts=8)
cyl("leg_l", 0.04, 0.18, (-0.04, 0, 0.34), stone("GS_ll"), verts=8)
cyl("leg_r", 0.04, 0.18, ( 0.04, 0, 0.34), stone("GS_lr"), verts=8)
# Wings (small)
cube("wing_l", (-0.10, 0.05, 0.55), (0.06, 0.02, 0.12), stone("GS_wl"), rot=(0, 0.3, 0))
cube("wing_r", ( 0.10, 0.05, 0.55), (0.06, 0.02, 0.12), stone("GS_wr"), rot=(0, -0.3, 0))
finalize("GardenStatueCherub")

# 8. Sundial
reset_scene()
cyl("pedestal", 0.20, 0.40, (0, 0, 0.20), stone("SD_p"), verts=16)
cyl("dial", 0.30, 0.04, (0, 0, 0.42), stone("SD_d"), verts=18)
# Gnomon (triangle)
cube("gnomon", (0, 0, 0.55), (0.02, 0.20, 0.10), brass("SD_g"), rot=(0.6, 0, 0))
# Hour markers (4 cardinal)
for i in range(8):
    a = i*(math.pi/4)
    cube(f"mark_{i}", (math.cos(a)*0.25, math.sin(a)*0.25, 0.45), (0.02, 0.02, 0.01), brass(f"SD_m{i}"))
finalize("Sundial")

# 9. Fountain Head (wall spout — used in HarmonicFountain)
reset_scene()
mask = make_material("FH_mask", (0.65, 0.65, 0.60), roughness=0.55)
sphere("head", 0.18, (0, 0, 0.25), mask, segs=14, rings=10)
# Open mouth (small sphere darker)
sphere("mouth", 0.04, (0, -0.16, 0.22), make_material("FH_mouth", (0.20, 0.20, 0.20), roughness=0.85), segs=8, rings=6)
# Hair/wreath
torus("wreath", 0.22, 0.025, (0, 0, 0.35), make_material("FH_wreath", (0.55, 0.40, 0.20), roughness=0.85), mseg=18, miseg=4)
# Spout
cyl("spout", 0.025, 0.10, (0, -0.20, 0.18), brass("FH_spout"), rot=(1.5708, 0, 0), verts=12)
finalize("FountainHead")

# 10. Wind Chime
reset_scene()
cyl("ring", 0.15, 0.015, (0, 0, 0.30), wood("WC_r"), verts=18)
# 6 hanging tubes of varied length
for i in range(6):
    a = i*(math.pi/3)
    L = 0.30 + 0.06*i
    cyl(f"tube_{i}", 0.015, L, (math.cos(a)*0.10, math.sin(a)*0.10, 0.30 - L/2 - 0.02),
        brass(f"WC_t{i}"), verts=10)
# Central clapper
sphere("clapper", 0.03, (0, 0, 0.10), wood("WC_c"), segs=10, rings=8)
finalize("WindChime")

# 11. Tapestry (wall-hanging)
reset_scene()
border = make_material("Tap_border", (0.40, 0.20, 0.10), roughness=0.85)
field = make_material("Tap_field", (0.45, 0.20, 0.15), roughness=0.80)
emblem = make_material("Tap_em", (0.92, 0.75, 0.25), roughness=0.45, metallic=0.4)
cube("rod", (0, 0.04, 0.70), (0.50, 0.04, 0.03), wood("Tap_rod"))
cube("field", (0, 0.02, 0.30), (0.40, 0.02, 0.50), field)
cube("border_t", (0, 0.01, 0.55), (0.42, 0.02, 0.04), border)
cube("border_b", (0, 0.01, 0.05), (0.42, 0.02, 0.04), border)
cube("border_l", (-0.41, 0.01, 0.30), (0.02, 0.02, 0.30), border)
cube("border_r", ( 0.41, 0.01, 0.30), (0.02, 0.02, 0.30), border)
# Sun emblem
sphere("emblem", 0.10, (0, 0.00, 0.30), emblem, segs=14, rings=10)
for i in range(8):
    a = i*(math.pi/4)
    cube(f"ray_{i}", (math.cos(a)*0.18, 0.0, 0.30+math.sin(a)*0.18),
         (0.04, 0.02, 0.04), emblem, rot=(0, a, 0))
finalize("Tapestry")

# 12. Painting Frame (gilt frame)
reset_scene()
canvas = make_material("PF_canvas", (0.85, 0.80, 0.65), roughness=0.85)
# Front canvas
cube("canvas", (0, 0.0, 0), (0.35, 0.02, 0.45), canvas)
# Gilt frame (4 thick borders)
cube("frame_t", (0, 0.0, 0.50), (0.40, 0.04, 0.05), gold("PF_g"))
cube("frame_b", (0, 0.0, -0.50), (0.40, 0.04, 0.05), gold("PF_g2"))
cube("frame_l", (-0.39, 0.0, 0), (0.05, 0.04, 0.55), gold("PF_g3"))
cube("frame_r", ( 0.39, 0.0, 0), (0.05, 0.04, 0.55), gold("PF_g4"))
finalize("PaintingFrame")

# 13. Standing Mirror
reset_scene()
mirror = make_material("Mir_glass", (0.85, 0.92, 0.95), roughness=0.05, metallic=0.95)
cube("mirror_face", (0, 0.0, 0.85), (0.20, 0.02, 0.55), mirror)
cube("frame_t", (0, 0.0, 1.42), (0.24, 0.04, 0.04), gold("Mir_g"))
cube("frame_b", (0, 0.0, 0.30), (0.24, 0.04, 0.04), gold("Mir_g2"))
cube("frame_l", (-0.22, 0.0, 0.85), (0.04, 0.04, 0.60), gold("Mir_g3"))
cube("frame_r", ( 0.22, 0.0, 0.85), (0.04, 0.04, 0.60), gold("Mir_g4"))
# Stand base
cube("base", (0, 0, 0.05), (0.30, 0.30, 0.05), wood("Mir_base"))
cyl("post_l", 0.025, 0.30, (-0.15, 0, 0.20), wood("Mir_p1"), verts=10)
cyl("post_r", 0.025, 0.30, ( 0.15, 0, 0.20), wood("Mir_p2"), verts=10)
finalize("StandingMirror")

print("done gen_props_furniture_set2: 13 props (1 bonus)")
