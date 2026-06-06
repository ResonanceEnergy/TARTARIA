"""15 cooking + alchemy props — stove, kettle, frying pan, ladle, knife block,
cutting board, spice rack, distillation tower, alembic, retort, cooling coil,
3 glass beakers, brewing rack, big mortar.
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

iron = lambda n: make_material(n, (0.20, 0.18, 0.16), roughness=0.55, metallic=0.6)
copper = lambda n: make_material(n, (0.65, 0.40, 0.25), roughness=0.30, metallic=0.7)
brass = lambda n: make_material(n, (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
glass = lambda n, c=(0.92,0.95,0.98), em=None: make_material(n, c, roughness=0.10, metallic=0.05, emission=em, emission_strength=1.0)
wood = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)
ember = lambda n: make_material(n, (0.20, 0.10, 0.05), roughness=0.85, emission=(0.95, 0.40, 0.10), emission_strength=2.0)

# 1. Stove (cast iron)
reset_scene()
cube("body", (0, 0, 0.35), (0.40, 0.30, 0.35), iron("St_b"))
cube("top", (0, 0, 0.72), (0.42, 0.32, 0.03), iron("St_top"))
# 4 burner rings
for x, y in [(-0.15, -0.10), (-0.15, 0.10), (0.15, -0.10), (0.15, 0.10)]:
    torus(f"burner_{x}_{y}", 0.05, 0.005, (x, y, 0.745), iron(f"St_br_{x}_{y}"), mseg=16, miseg=4)
# Oven door
cube("door", (0, -0.32, 0.30), (0.30, 0.02, 0.20), iron("St_door"))
cyl("handle", 0.012, 0.20, (0, -0.34, 0.20), brass("St_handle"), rot=(0, 1.5708, 0), verts=8)
# Chimney
cyl("chim", 0.06, 0.40, (0.15, 0.15, 0.95), iron("St_chim"), verts=14)
# Glowing front grate
cube("grate", (0, -0.32, 0.45), (0.25, 0.005, 0.06), ember("St_ember"))
finalize("Stove")

# 2. Kettle
reset_scene()
sphere("body", 0.13, (0, 0, 0.15), copper("Kt_b"), segs=18, rings=12)
cyl("spout", 0.025, 0.18, (-0.14, 0, 0.18), copper("Kt_sp"), rot=(0, 1.4, 0), verts=10)
# Lid
cyl("lid", 0.08, 0.03, (0, 0, 0.30), copper("Kt_lid"), verts=14)
sphere("knob", 0.02, (0, 0, 0.33), brass("Kt_knob"), segs=10, rings=8)
# Handle (arched)
torus("handle", 0.10, 0.012, (0, 0, 0.30), copper("Kt_handle"), mseg=16, miseg=4, rot=(1.5708, 0, 0))
finalize("Kettle")

# 3. Frying Pan
reset_scene()
cyl("pan", 0.18, 0.04, (0, 0, 0.02), iron("FP_pan"), verts=20)
cyl("handle", 0.018, 0.30, (0.32, 0, 0.02), wood("FP_handle"), rot=(0, 1.5708, 0), verts=10)
finalize("FryingPan")

# 4. Ladle
reset_scene()
sphere("scoop", 0.06, (0, 0, 0.03), iron("L_scoop"), segs=14, rings=10)
cyl("handle", 0.015, 0.30, (0, 0.15, 0.20), wood("L_handle"), rot=(0.6, 0, 0), verts=10)
finalize("Ladle")

# 5. Knife Block
reset_scene()
cube("block", (0, 0, 0.10), (0.10, 0.16, 0.20), wood("KB_b"))
# 4 knife handles sticking out top
for i, x in enumerate([-0.06, -0.02, 0.02, 0.06]):
    cube(f"knife_{i}", (x, 0, 0.30), (0.012, 0.05, 0.04), make_material(f"KB_k{i}", (0.20, 0.18, 0.15), roughness=0.50, metallic=0.4))
    cyl(f"handle_{i}", 0.012, 0.05, (x, 0, 0.34), iron(f"KB_h{i}"), verts=8)
finalize("KnifeBlock")

# 6. Cutting Board
reset_scene()
cube("board", (0, 0, 0.02), (0.18, 0.12, 0.02), wood("CB_b"))
# Drip channel groove
cube("groove", (0, 0, 0.04), (0.16, 0.10, 0.003), make_material("CB_g", (0.30, 0.20, 0.12), roughness=0.85))
finalize("CuttingBoard")

# 7. Spice Rack
reset_scene()
cube("back", (0, 0.04, 0.20), (0.20, 0.02, 0.20), wood("SR_back"))
cube("shelf_1", (0, 0, 0.08), (0.20, 0.06, 0.02), wood("SR_s1"))
cube("shelf_2", (0, 0, 0.20), (0.20, 0.06, 0.02), wood("SR_s2"))
cube("shelf_3", (0, 0, 0.32), (0.20, 0.06, 0.02), wood("SR_s3"))
# 9 small jars (3 per shelf)
for i, z in enumerate([0.13, 0.25, 0.37]):
    for j, x in enumerate([-0.08, 0.0, 0.08]):
        sphere(f"jar_{i}_{j}", 0.022, (x, 0, z), glass(f"SR_j{i}{j}", c=(0.85+0.05*j, 0.70-0.10*j, 0.40+0.15*i)), segs=12, rings=8)
finalize("SpiceRack")

# 8. Distillation Tower (multi-stage)
reset_scene()
cyl("base", 0.18, 0.08, (0, 0, 0.04), copper("DT_base"), verts=18)
cyl("stage1", 0.13, 0.30, (0, 0, 0.23), copper("DT_s1"), verts=16)
sphere("bulb1", 0.16, (0, 0, 0.45), glass("DT_b1", em=(0.40, 0.85, 0.50)), segs=14, rings=10)
cyl("stage2", 0.10, 0.25, (0, 0, 0.65), copper("DT_s2"), verts=14)
sphere("bulb2", 0.13, (0, 0, 0.85), glass("DT_b2", em=(0.95, 0.65, 0.20)), segs=14, rings=10)
cyl("stage3", 0.08, 0.20, (0, 0, 1.02), copper("DT_s3"), verts=12)
cone("top", 0.08, 0.02, 0.15, (0, 0, 1.18), copper("DT_top"), verts=12)
# Side outlet pipe
cyl("pipe", 0.015, 0.20, (0.18, 0, 0.45), copper("DT_pipe"), rot=(0, 1.5708, 0), verts=8)
finalize("DistillationTower")

# 9. Alembic (classic alchemy vessel)
reset_scene()
sphere("body", 0.12, (0, 0, 0.15), glass("Al_b", em=(0.55, 0.30, 0.95)), segs=16, rings=12)
# Tall narrow neck curving down
cyl("neck", 0.025, 0.20, (0, 0, 0.32), glass("Al_n"), verts=12)
cyl("arm", 0.025, 0.25, (0.10, 0, 0.38), glass("Al_arm"), rot=(0, 1.0, 0), verts=12)
# Collection bulb at end
sphere("collect", 0.05, (0.22, 0, 0.30), glass("Al_col"), segs=12, rings=10)
finalize("Alembic")

# 10. Retort (curved glass)
reset_scene()
sphere("body", 0.10, (0, 0, 0.10), glass("Rt_b"), segs=14, rings=10)
# Long curved neck
cyl("neck_1", 0.018, 0.15, (0.08, 0, 0.12), glass("Rt_n1"), rot=(0, 1.5708, 0), verts=10)
cyl("neck_2", 0.018, 0.10, (0.20, 0, 0.10), glass("Rt_n2"), rot=(0, 1.0, 0), verts=10)
finalize("Retort")

# 11. Cooling Coil
reset_scene()
coil = copper("CC_coil")
# Spiral of small spheres
for i in range(20):
    t = i / 19.0
    z = 0.05 + t * 0.40
    a = t * 6 * math.pi
    sphere(f"loop_{i}", 0.025, (math.cos(a)*0.08, math.sin(a)*0.08, z), coil, segs=10, rings=8)
# Inlet/outlet
cyl("in", 0.012, 0.08, (0.10, 0, 0.05), coil, rot=(0, 1.5708, 0), verts=8)
cyl("out", 0.012, 0.08, (0.10, 0, 0.45), coil, rot=(0, 1.5708, 0), verts=8)
finalize("CoolingCoil")

# 12-14. Glass Beakers (small, medium, large)
def beaker(name, scale, fill_color):
    reset_scene()
    cyl("body", 0.06*scale, 0.12*scale, (0, 0, 0.06*scale), glass(name+"_b"), verts=16)
    # Liquid
    cyl("liquid", 0.055*scale, 0.08*scale, (0, 0, 0.04*scale), make_material(name+"_l", fill_color, roughness=0.30, emission=fill_color, emission_strength=0.8), verts=14)
    # Pour lip
    cube("lip", (0.05*scale, 0, 0.12*scale), (0.015*scale, 0.02*scale, 0.01*scale), glass(name+"_lip"))
    # Measure markings
    for i, z in enumerate([0.03*scale, 0.06*scale, 0.09*scale]):
        cube(f"mark_{i}", (0, -0.06*scale, z), (0.005, 0.001, 0.002), make_material(f"{name}_m{i}", (0.10, 0.10, 0.10), roughness=0.65))
    finalize(name)

beaker("BeakerSmall", 1.0, (0.40, 0.85, 0.50))
beaker("BeakerMed", 1.5, (0.95, 0.65, 0.20))
beaker("BeakerLarge", 2.2, (0.55, 0.30, 0.95))

# 15. Brewing Rack (large stand with 3 vessels)
reset_scene()
cube("base", (0, 0, 0.05), (0.40, 0.30, 0.05), wood("BR_base"))
cube("post_l", (-0.30, 0, 0.40), (0.04, 0.04, 0.65), wood("BR_pl"))
cube("post_r", ( 0.30, 0, 0.40), (0.04, 0.04, 0.65), wood("BR_pr"))
cube("top", (0, 0, 1.08), (0.30, 0.04, 0.04), wood("BR_top"))
# Three flasks hanging
for i, x in enumerate([-0.20, 0.0, 0.20]):
    sphere(f"flask_{i}", 0.06, (x, 0, 0.30), glass(f"BR_f{i}", em=(0.40+0.2*i, 0.65, 0.50+0.1*i)), segs=14, rings=10)
    cyl(f"flask_neck_{i}", 0.012, 0.06, (x, 0, 0.40), glass(f"BR_fn{i}"), verts=8)
    # Hanging chain
    cyl(f"chain_{i}", 0.006, 0.55, (x, 0, 0.75), brass(f"BR_c{i}"), verts=6)
finalize("BrewingRack")

# 16. Big Mortar (apothecary scale)
reset_scene()
stone = make_material("BM_stone", (0.45, 0.42, 0.38), roughness=0.85)
cyl("bowl", 0.18, 0.18, (0, 0, 0.09), stone, verts=20)
# Hollow center
cyl("hollow", 0.12, 0.12, (0, 0, 0.13), make_material("BM_inside", (0.30, 0.28, 0.25), roughness=0.90), verts=18)
# Pestle leaning
cyl("pestle", 0.025, 0.25, (0.25, 0, 0.20), stone, rot=(0, 0.7, 0), verts=12)
sphere("pestle_head", 0.04, (0.16, 0, 0.10), stone, segs=10, rings=8)
finalize("BigMortar")

print("done gen_cooking_alchemy: 16 props")
