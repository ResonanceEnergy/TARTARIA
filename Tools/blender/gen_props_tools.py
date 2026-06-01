"""12 tools/utility props — pickaxe, shovel, hammer, anvil, bellows, loom,
spinning wheel, cauldron, mortar+pestle, ladder, cart wheel, wooden bucket.
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
iron = lambda n: make_material(n, (0.32, 0.30, 0.28), roughness=0.50, metallic=0.7)
rope = lambda n: make_material(n, (0.55, 0.42, 0.25), roughness=0.85)

# 1. Pickaxe
reset_scene()
cyl("handle", 0.04, 1.10, (0, 0, 0.55), wood("Pick_w"), verts=10)
cube("head", (0, 0, 1.10), (0.30, 0.05, 0.06), iron("Pick_i"))
cone("tip_l", 0.08, 0.01, 0.10, (-0.32, 0, 1.10), iron("Pick_i2"), rot=(0, -1.5708, 0), verts=6)
cone("tip_r", 0.08, 0.01, 0.10, ( 0.32, 0, 1.10), iron("Pick_i3"), rot=(0, 1.5708, 0), verts=6)
finalize("Pickaxe")

# 2. Shovel
reset_scene()
cyl("handle", 0.04, 1.10, (0, 0, 0.55), wood("Shovel_w"), verts=10)
cyl("grip", 0.06, 0.10, (0, 0, 1.15), wood("Shovel_w2"), verts=10)
cube("blade", (0, 0, 0.0), (0.12, 0.02, 0.20), iron("Shovel_i"))
finalize("Shovel")

# 3. Hammer
reset_scene()
cyl("handle", 0.035, 0.40, (0, 0, 0.20), wood("Hammer_w"), verts=10)
cube("head", (0, 0, 0.45), (0.10, 0.06, 0.10), iron("Hammer_i"))
finalize("Hammer")

# 4. Anvil
reset_scene()
cube("base", (0, 0, 0.15), (0.18, 0.35, 0.15), iron("Anvil_i"))
cube("waist", (0, 0, 0.35), (0.14, 0.30, 0.10), iron("Anvil_i2"))
cube("top", (0, 0, 0.50), (0.20, 0.50, 0.08), iron("Anvil_i3"))
cone("horn", 0.06, 0.01, 0.20, (0, -0.55, 0.50), iron("Anvil_i4"), rot=(-1.5708, 0, 0), verts=10)
finalize("Anvil")

# 5. Bellows (smithy)
reset_scene()
leather = make_material("Bellow_l", (0.45, 0.25, 0.15), roughness=0.85)
brass = make_material("Bellow_b", (0.78, 0.60, 0.28), roughness=0.30, metallic=0.7)
cube("body_top", (0, 0, 0.30), (0.30, 0.25, 0.08), wood("Bellow_w"))
cube("body_bot", (0, 0, 0.10), (0.30, 0.25, 0.08), wood("Bellow_w2"))
cube("leather_mid", (0, 0, 0.20), (0.28, 0.23, 0.16), leather)
cyl("nozzle", 0.04, 0.40, (0, -0.45, 0.20), brass, rot=(1.5708, 0, 0), verts=10)
cube("handle_t", (0, 0.30, 0.40), (0.30, 0.03, 0.03), wood("Bellow_w3"))
cube("handle_b", (0, 0.30, 0.00), (0.30, 0.03, 0.03), wood("Bellow_w4"))
finalize("Bellows")

# 6. Loom
reset_scene()
cube("frame_l", (-0.50, 0, 0.70), (0.05, 0.20, 0.70), wood("Loom_w"))
cube("frame_r", ( 0.50, 0, 0.70), (0.05, 0.20, 0.70), wood("Loom_w2"))
cube("frame_top", (0, 0, 1.40), (0.55, 0.10, 0.05), wood("Loom_w3"))
cube("frame_bot", (0, 0, 0.05), (0.55, 0.10, 0.05), wood("Loom_w4"))
# Warp threads
threads = make_material("Loom_thread", (0.92, 0.88, 0.75), roughness=0.85)
for i in range(6):
    x = -0.40 + i*0.16
    cube(f"warp_{i}", (x, 0, 0.70), (0.005, 0.02, 0.65), threads)
# Cloth roll
cyl("cloth", 0.10, 1.0, (0, 0, 0.15), make_material("Loom_cloth", (0.65, 0.45, 0.30), roughness=0.85),
    rot=(0, 1.5708, 0), verts=14)
finalize("Loom")

# 7. Spinning Wheel
reset_scene()
cyl("base_l", 0.04, 0.50, (-0.18, 0, 0.25), wood("SW_w"), verts=10)
cyl("base_r", 0.04, 0.50, ( 0.18, 0, 0.25), wood("SW_w2"), verts=10)
cube("table", (0, 0, 0.52), (0.30, 0.18, 0.04), wood("SW_w3"))
# Big wheel
torus("wheel", 0.30, 0.025, (-0.20, 0.10, 0.85), wood("SW_wheel"), mseg=20, miseg=4, rot=(1.5708, 0, 0))
# Spokes (6)
for i in range(6):
    a = i*(math.pi/3)
    cube(f"spoke_{i}", (-0.20, 0.10, 0.85), (0.30, 0.02, 0.02), wood(f"SW_spoke{i}"), rot=(0, a, 0))
# Spindle
cyl("spindle", 0.02, 0.20, (0.30, -0.10, 0.85), iron("SW_spindle"), rot=(0, 1.5708, 0), verts=8)
finalize("SpinningWheel")

# 8. Cauldron
reset_scene()
iron2 = make_material("Caul_i", (0.18, 0.16, 0.14), roughness=0.55, metallic=0.6)
brew = make_material("Caul_brew", (0.20, 0.55, 0.25), roughness=0.30,
                     emission=(0.20, 0.55, 0.25), emission_strength=0.6)
sphere("body", 0.30, (0, 0, 0.30), iron2, segs=18, rings=12)
cyl("rim", 0.32, 0.04, (0, 0, 0.55), iron2, verts=20)
cyl("brew_top", 0.27, 0.02, (0, 0, 0.54), brew, verts=20)
torus("handle_l", 0.06, 0.015, (-0.32, 0, 0.50), iron2, mseg=14, miseg=4, rot=(1.5708, 0, 1.5708))
torus("handle_r", 0.06, 0.015, ( 0.32, 0, 0.50), iron2, mseg=14, miseg=4, rot=(1.5708, 0, 1.5708))
# 3 stubby legs
for i in range(3):
    a = i*(2*math.pi/3)
    cyl(f"leg_{i}", 0.04, 0.10, (math.cos(a)*0.20, math.sin(a)*0.20, 0.10), iron2, verts=8)
finalize("Cauldron")

# 9. Mortar & Pestle
reset_scene()
stone_m = make_material("MP_stone", (0.40, 0.38, 0.36), roughness=0.85)
cyl("mortar", 0.12, 0.12, (0, 0, 0.06), stone_m, verts=18)
# Bowl interior — small inset cyl
cyl("bowl", 0.08, 0.08, (0, 0, 0.10), make_material("MP_inside", (0.30, 0.28, 0.26), roughness=0.85), verts=16)
# Pestle
cyl("pestle", 0.02, 0.15, (0.18, 0, 0.16), stone_m, rot=(0, 0.6, 0), verts=10)
sphere("pestle_tip", 0.03, (0.10, 0, 0.10), stone_m, segs=10, rings=8)
finalize("MortarAndPestle")

# 10. Ladder
reset_scene()
cube("rail_l", (-0.20, 0, 1.0), (0.04, 0.04, 1.0), wood("Lad_w"))
cube("rail_r", ( 0.20, 0, 1.0), (0.04, 0.04, 1.0), wood("Lad_w2"))
for i in range(6):
    z = 0.10 + i*0.35
    cube(f"rung_{i}", (0, 0, z), (0.22, 0.04, 0.02), wood(f"Lad_r{i}"))
finalize("Ladder")

# 11. Cart Wheel
reset_scene()
torus("rim", 0.40, 0.05, (0, 0, 0.05), wood("CW_rim"), mseg=24, miseg=6, rot=(1.5708, 0, 0))
# Hub
cyl("hub", 0.10, 0.10, (0, 0, 0.05), wood("CW_hub"), rot=(1.5708, 0, 0), verts=12)
# Spokes (8)
for i in range(8):
    a = i*(math.pi/4)
    cube(f"spoke_{i}", (0, 0, 0.05), (0.30, 0.025, 0.025), wood(f"CW_sp{i}"), rot=(0, a, 0))
finalize("CartWheel")

# 12. Wooden Bucket
reset_scene()
cyl("body", 0.18, 0.25, (0, 0, 0.13), wood("Buck_w"), verts=16)
torus("band_top", 0.19, 0.012, (0, 0, 0.24), iron("Buck_i"), mseg=20, miseg=4)
torus("band_bot", 0.19, 0.012, (0, 0, 0.02), iron("Buck_i2"), mseg=20, miseg=4)
torus("handle", 0.15, 0.012, (0, 0, 0.35), iron("Buck_i3"), mseg=20, miseg=4, rot=(1.5708, 0, 0))
finalize("WoodenBucket")

print("done gen_props_tools: 12 tools")
