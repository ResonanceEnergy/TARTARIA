"""12 small flora — mushroom cluster, fern, sunflower, lily pad, lotus,
ivy vine, hanging moss, leaf pile, snow drift, crystal cluster, cattail reed,
glowing flower patch.
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

stem = lambda n: make_material(n, (0.25, 0.50, 0.20), roughness=0.85)
leaf = lambda n: make_material(n, (0.25, 0.55, 0.20), roughness=0.85)
brown = lambda n: make_material(n, (0.42, 0.28, 0.16), roughness=0.85)

# 1. Mushroom Cluster (5 toadstools)
reset_scene()
cap = make_material("MC_cap", (0.75, 0.15, 0.10), roughness=0.70)
spots = make_material("MC_spots", (0.95, 0.92, 0.85), roughness=0.85)
stem_m = make_material("MC_stem", (0.92, 0.88, 0.78), roughness=0.85)
for i, (x, y, s) in enumerate([(0, 0, 1.0), (0.08, 0.06, 0.7), (-0.08, 0.05, 0.65),
                                  (0.05, -0.08, 0.55), (-0.05, -0.07, 0.50)]):
    cyl(f"stem_{i}", 0.018*s, 0.10*s, (x, y, 0.05*s), stem_m, verts=10)
    sphere(f"cap_{i}", 0.05*s, (x, y, 0.10*s), cap, segs=12, rings=10)
    sphere(f"spot_{i}", 0.012*s, (x, y, 0.13*s), spots, segs=6, rings=4)
finalize("MushroomCluster")

# 2. Fern (large frond)
reset_scene()
cube("base", (0, 0, 0.02), (0.04, 0.04, 0.02), brown("Frn_b"))
# 6 fronds arcing outward
for i in range(6):
    a = i*(math.pi/3)
    for j in range(5):
        t = (j+1) / 5.0
        x = math.cos(a) * t * 0.30
        y = math.sin(a) * t * 0.30
        z = 0.08 + (j*0.04) - (t*t * 0.06)
        cube(f"leaf_{i}_{j}", (x, y, z), (0.04, 0.015, 0.012), leaf(f"Frn_l{i}{j}"), rot=(0, 0, a))
finalize("Fern")

# 3. Sunflower (single tall)
reset_scene()
cyl("stem", 0.012, 0.70, (0, 0, 0.35), stem("Sf_s"), verts=8)
# Center disk
cyl("disk", 0.08, 0.02, (0, 0, 0.72), brown("Sf_d"), verts=18)
# 12 petals
for i in range(12):
    a = i*(math.pi/6)
    cube(f"petal_{i}", (math.cos(a)*0.13, math.sin(a)*0.13, 0.73), (0.08, 0.025, 0.005), make_material(f"Sf_p{i}", (0.95, 0.80, 0.15), roughness=0.85), rot=(0, 0, a))
# 2 leaves on stem
cube("leaf_1", (0.06, 0, 0.45), (0.08, 0.04, 0.005), leaf("Sf_l1"), rot=(0, 0, 0.3))
cube("leaf_2", (-0.06, 0, 0.30), (0.08, 0.04, 0.005), leaf("Sf_l2"), rot=(0, 0, -0.3))
finalize("Sunflower")

# 4. Lily Pad (water lily on pond surface)
reset_scene()
cyl("pad", 0.20, 0.01, (0, 0, 0.005), leaf("LP_pad"), verts=20)
# Slot cutout (simulated with darker triangle)
cube("slot", (0, -0.15, 0.012), (0.04, 0.10, 0.003), make_material("LP_slot", (0.05, 0.20, 0.05), roughness=0.95))
# White flower on top
sphere("flower", 0.06, (0.06, 0.06, 0.04), make_material("LP_fl", (0.92, 0.88, 0.80), roughness=0.70), segs=12, rings=10)
sphere("flower_center", 0.02, (0.06, 0.06, 0.08), make_material("LP_fc", (0.92, 0.75, 0.20), roughness=0.40, emission=(0.95, 0.80, 0.20), emission_strength=0.4), segs=8, rings=6)
finalize("LilyPad")

# 5. Lotus Flower
reset_scene()
cyl("base", 0.05, 0.02, (0, 0, 0.01), leaf("Lot_b"), verts=14)
# 8 petals layered
for layer in range(3):
    petals = 8 - layer*2
    r = 0.10 - layer*0.025
    z = 0.04 + layer*0.03
    for i in range(petals):
        a = i*(math.pi*2/petals)
        cube(f"petal_{layer}_{i}", (math.cos(a)*r, math.sin(a)*r, z), (0.04, 0.018, 0.025), make_material(f"Lot_p{layer}{i}", (0.95, 0.78, 0.85), roughness=0.65), rot=(0.3, 0, a))
# Center
sphere("center", 0.02, (0, 0, 0.12), make_material("Lot_c", (0.92, 0.85, 0.30), roughness=0.65, emission=(0.95, 0.85, 0.30), emission_strength=0.5), segs=8, rings=6)
finalize("LotusFlower")

# 6. Ivy Vine (long trailing)
reset_scene()
for i in range(15):
    t = i / 14.0
    y = -0.30 + i*0.10
    z = 0.50 - i*0.04 + math.sin(i*0.7)*0.06
    sphere(f"leaf_{i}", 0.04 + 0.02*math.sin(i*0.3), (math.cos(i*0.5)*0.08, y, z), leaf(f"Iv_l{i}"), segs=10, rings=8)
finalize("IvyVine")

# 7. Hanging Moss (long drape)
reset_scene()
for i in range(8):
    a = i*(math.pi/4)
    cyl(f"strand_{i}", 0.02, 0.40 + i*0.05, (math.cos(a)*0.10, math.sin(a)*0.10, 0.25), make_material(f"HM_s{i}", (0.55, 0.65, 0.30), roughness=0.92), verts=8)
# Top mount
cyl("mount", 0.12, 0.05, (0, 0, 0.50), brown("HM_mount"), verts=12)
finalize("HangingMoss")

# 8. Leaf Pile (autumn)
reset_scene()
for i in range(20):
    a = (i*0.314)
    r = 0.05 + (i % 5) * 0.04
    x = math.cos(a) * 0.20 * (i/10.0)
    y = math.sin(a) * 0.20 * (i/10.0)
    cube(f"leaf_{i}", (x, y, 0.015 + (i % 3) * 0.012), (0.04, 0.03, 0.005),
         make_material(f"LP_l{i}",
                       (0.85, 0.45, 0.15) if i%3==0 else (0.65, 0.32, 0.10) if i%3==1 else (0.92, 0.70, 0.18),
                       roughness=0.85), rot=(0, 0, a))
finalize("LeafPile")

# 9. Snow Drift
reset_scene()
snow = make_material("SD_snow", (0.95, 0.95, 0.98), roughness=0.65)
# 4 overlapping spheres of varied size
sphere("d1", 0.30, (0, 0, 0.12), snow, segs=16, rings=12)
sphere("d2", 0.22, (0.20, 0.10, 0.08), snow, segs=14, rings=10)
sphere("d3", 0.18, (-0.18, 0.05, 0.06), snow, segs=14, rings=10)
sphere("d4", 0.15, (0.05, -0.15, 0.05), snow, segs=12, rings=10)
finalize("SnowDrift")

# 10. Crystal Cluster (small ground formation)
reset_scene()
crystal_b = make_material("CCl_b", (0.40, 0.55, 0.92), roughness=0.20, metallic=0.2,
                          emission=(0.50, 0.70, 0.95), emission_strength=1.5)
for i in range(7):
    a = i*(math.pi/3.5)
    r = 0.10 + (i % 3) * 0.05
    cone(f"shard_{i}", 0.05 + 0.02*(i%3), 0.005, 0.20 + 0.08*(i%4), (math.cos(a)*r, math.sin(a)*r, 0.10), crystal_b, rot=(0.2*(i%3-1), 0.1*(i%2), a), verts=6)
finalize("CrystalCluster")

# 11. Cattail Reed
reset_scene()
for i in range(5):
    x = -0.08 + i*0.04
    cyl(f"stalk_{i}", 0.010, 0.80, (x, 0, 0.40), stem(f"Cat_s{i}"), verts=8)
    cyl(f"head_{i}", 0.025, 0.12, (x, 0, 0.90), brown(f"Cat_h{i}"), verts=10)
# 3 long thin leaves
cube("leaf_1", (0.05, 0, 0.40), (0.005, 0.04, 0.50), leaf("Cat_l1"), rot=(0.4, 0, 0))
cube("leaf_2", (-0.05, 0, 0.45), (0.005, 0.04, 0.50), leaf("Cat_l2"), rot=(-0.3, 0, 0))
cube("leaf_3", (0, 0.04, 0.42), (0.005, 0.04, 0.48), leaf("Cat_l3"), rot=(0, 0, 0.5))
finalize("CattailReed")

# 12. Glowing Flower Patch
reset_scene()
glow_b = make_material("GF_glow_b", (0.40, 0.55, 0.95), roughness=0.30, emission=(0.50, 0.70, 1.0), emission_strength=1.4)
glow_p = make_material("GF_glow_p", (0.85, 0.40, 0.85), roughness=0.30, emission=(0.95, 0.55, 0.95), emission_strength=1.4)
for i in range(8):
    a = i*(math.pi/4)
    x = math.cos(a) * 0.18
    y = math.sin(a) * 0.18
    cyl(f"stem_{i}", 0.008, 0.20, (x, y, 0.10), stem(f"GF_s{i}"), verts=6)
    sphere(f"bloom_{i}", 0.035, (x, y, 0.22), glow_b if i%2==0 else glow_p, segs=10, rings=8)
finalize("GlowingFlowerPatch")

print("done gen_flora_small: 12 small flora")
