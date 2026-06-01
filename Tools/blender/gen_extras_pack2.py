"""8 utility extras (pack 2) — fence panel, gate, well bucket+rope, hanging chain,
ladder folded, rope coil, pile of bricks, scaffold piece.
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
rope = lambda n: make_material(n, (0.55, 0.42, 0.25), roughness=0.85)
brick = lambda n: make_material(n, (0.60, 0.30, 0.20), roughness=0.85)

# 1. Fence Panel (5 vertical posts + 2 horizontal rails)
reset_scene()
cube("rail_t", (0, 0, 0.85), (0.80, 0.04, 0.04), wood("Fnc_rt"))
cube("rail_b", (0, 0, 0.30), (0.80, 0.04, 0.04), wood("Fnc_rb"))
for i in range(5):
    x = -0.65 + i*0.32
    cube(f"post_{i}", (x, 0, 0.55), (0.05, 0.05, 0.55), wood(f"Fnc_p{i}"))
finalize("FencePanel")

# 2. Gate (with iron hinges)
reset_scene()
cube("frame_l", (-0.50, 0, 0.55), (0.04, 0.06, 0.55), wood("Gt_fl"))
cube("frame_r", ( 0.50, 0, 0.55), (0.04, 0.06, 0.55), wood("Gt_fr"))
cube("frame_t", (0, 0, 1.05), (0.50, 0.06, 0.04), wood("Gt_ft"))
cube("frame_b", (0, 0, 0.10), (0.50, 0.06, 0.04), wood("Gt_fb"))
# X braces
cube("brace_1", (0, 0, 0.55), (0.55, 0.04, 0.025), wood("Gt_br1"), rot=(0, 0.6, 0))
cube("brace_2", (0, 0, 0.55), (0.55, 0.04, 0.025), wood("Gt_br2"), rot=(0, -0.6, 0))
# Hinges
cube("hinge_l_t", (-0.50, -0.05, 0.85), (0.10, 0.03, 0.05), iron("Gt_ht"))
cube("hinge_l_b", (-0.50, -0.05, 0.25), (0.10, 0.03, 0.05), iron("Gt_hb"))
# Latch handle
torus("latch", 0.04, 0.008, (0.50, -0.05, 0.65), iron("Gt_latch"), mseg=14, miseg=3)
finalize("Gate")

# 3. Well Bucket on Rope
reset_scene()
cyl("bucket_body", 0.10, 0.16, (0, 0, 0.08), wood("WB_b"), verts=14)
torus("rim", 0.105, 0.005, (0, 0, 0.16), iron("WB_rim"), mseg=18, miseg=3)
torus("bucket_handle", 0.09, 0.005, (0, 0, 0.24), iron("WB_h"), mseg=18, miseg=3, rot=(1.5708, 0, 0))
# Rope going up
for i in range(8):
    cyl(f"rope_{i}", 0.008, 0.10, (0, 0, 0.30 + i*0.10), rope(f"WB_r{i}"), verts=6)
finalize("WellBucket")

# 4. Hanging Chain (iron, 12 links)
reset_scene()
for i in range(12):
    z = i * 0.10
    rot = (1.5708, 0, 0) if i % 2 == 0 else (1.5708, 0, 1.5708)
    torus(f"link_{i}", 0.04, 0.008, (0, 0, 0.05 + z), iron(f"HC_l{i}"), mseg=14, miseg=4, rot=rot)
finalize("HangingChain")

# 5. Ladder Folded (carrying state)
reset_scene()
cube("rail_l", (-0.05, 0, 0.50), (0.025, 0.04, 0.50), wood("LF_rl"))
cube("rail_r", ( 0.05, 0, 0.50), (0.025, 0.04, 0.50), wood("LF_rr"))
for i in range(5):
    z = 0.10 + i*0.20
    cube(f"rung_{i}", (0, 0, z), (0.10, 0.025, 0.018), wood(f"LF_r{i}"))
finalize("LadderFolded")

# 6. Rope Coil (looped on ground)
reset_scene()
for i in range(10):
    t = i / 9.0
    r = 0.10 + (i % 3) * 0.005
    z = 0.02 + (i % 4) * 0.015
    torus(f"loop_{i}", r, 0.012, (0, 0, z), rope(f"RC_l{i}"), mseg=20, miseg=4, rot=(0.05, 0.05, t*math.pi))
finalize("RopeCoil")

# 7. Pile of Bricks
reset_scene()
for i in range(12):
    row = i // 4
    col = i % 4
    cube(f"brick_{i}", (-0.20 + col*0.12, 0, 0.04 + row*0.07), (0.05, 0.10, 0.035), brick(f"Br_{i}"), rot=(0, 0, 0.05*(col % 2)))
finalize("BrickPile")

# 8. Scaffold Piece (frame section)
reset_scene()
metal = make_material("Sc_m", (0.55, 0.45, 0.30), roughness=0.55, metallic=0.5)
plank = wood("Sc_p")
# Frame uprights (2)
cyl("upright_l", 0.04, 1.80, (-0.50, 0, 0.90), metal, verts=12)
cyl("upright_r", 0.04, 1.80, ( 0.50, 0, 0.90), metal, verts=12)
# Horizontal cross braces (3)
for z in (0.30, 1.0, 1.70):
    cyl(f"cross_{z}", 0.025, 1.05, (0, 0, z), metal, rot=(0, 1.5708, 0), verts=10)
# Plank deck
cube("deck", (0, 0, 1.05), (0.55, 0.30, 0.04), plank)
# Diagonal brace
cube("diag", (0, 0, 0.65), (0.04, 0.04, 0.75), metal, rot=(0, 0.6, 0))
finalize("ScaffoldPiece")

print("done gen_extras_pack2: 8 extras")
