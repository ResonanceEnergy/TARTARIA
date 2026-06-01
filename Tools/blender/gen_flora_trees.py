"""12 trees + large flora — oak, pine, birch, willow, dead oak, ancient sequoia,
palm, cypress, magnolia, hawthorn, big mushroom tree, world tree (small).
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

bark_brown = lambda n: make_material(n, (0.32, 0.20, 0.10), roughness=0.95)
bark_grey = lambda n: make_material(n, (0.55, 0.50, 0.45), roughness=0.95)
bark_white = lambda n: make_material(n, (0.88, 0.85, 0.80), roughness=0.85)
leaves_g = lambda n: make_material(n, (0.25, 0.55, 0.20), roughness=0.85)
leaves_d = lambda n: make_material(n, (0.18, 0.40, 0.18), roughness=0.85)
leaves_y = lambda n: make_material(n, (0.85, 0.65, 0.20), roughness=0.85)
leaves_pink = lambda n: make_material(n, (0.92, 0.75, 0.82), roughness=0.85)
leaves_glow = lambda n: make_material(n, (0.40, 0.85, 0.50), roughness=0.40, emission=(0.40, 0.85, 0.50), emission_strength=1.2)

# 1. Oak Tree
reset_scene()
cyl("trunk", 0.20, 2.0, (0, 0, 1.0), bark_brown("Oak_t"), verts=14)
for i, (x, y, z, r) in enumerate([(0, 0, 2.4, 0.95), (0.4, 0.2, 2.3, 0.65), (-0.35, 0.1, 2.45, 0.6),
                                    (0.0, -0.3, 2.55, 0.55), (-0.2, -0.2, 2.20, 0.45)]):
    sphere(f"canopy_{i}", r, (x, y, z), leaves_g(f"Oak_c{i}"), segs=16, rings=12)
finalize("OakTree")

# 2. Pine Tree
reset_scene()
cyl("trunk", 0.10, 2.5, (0, 0, 1.25), bark_brown("Pine_t"), verts=12)
# Conical layers
for i in range(4):
    r = 0.7 - i*0.15
    z = 1.5 + i*0.45
    cone(f"cone_{i}", r, 0.10, 0.50, (0, 0, z), leaves_d(f"Pine_c{i}"), verts=12)
finalize("PineTree")

# 3. Birch Tree (white bark)
reset_scene()
cyl("trunk", 0.10, 3.0, (0, 0, 1.5), bark_white("Birch_t"), verts=12)
# Dark bark patches (rings)
for z in (0.50, 1.0, 1.50, 2.0, 2.50):
    torus(f"patch_{z}", 0.105, 0.012, (0, 0, z), make_material(f"Birch_p{z}", (0.20, 0.18, 0.15), roughness=0.90), mseg=14, miseg=3)
# Sparse leaves at top
for i, (x, y, z) in enumerate([(0, 0, 3.20), (0.20, 0.10, 3.10), (-0.20, 0.10, 3.10),
                                  (0.10, -0.20, 3.15), (0, 0, 3.40)]):
    sphere(f"leaf_{i}", 0.35, (x, y, z), leaves_y(f"Birch_l{i}"), segs=14, rings=10)
finalize("BirchTree")

# 4. Willow (weeping)
reset_scene()
cyl("trunk", 0.16, 1.8, (0, 0, 0.9), bark_brown("Wil_t"), verts=14)
# Crown
sphere("crown", 0.80, (0, 0, 1.90), leaves_g("Wil_c"), segs=18, rings=12)
# Weeping branches (long drooping cylinders)
for i in range(12):
    a = i*(math.pi*2/12)
    cyl(f"branch_{i}", 0.02, 1.50, (math.cos(a)*0.60, math.sin(a)*0.60, 1.20), leaves_g(f"Wil_b{i}"), rot=(0.3 if i%2==0 else -0.3, 0, 0), verts=8)
finalize("WillowTree")

# 5. Dead Oak (no leaves)
reset_scene()
cyl("trunk", 0.20, 2.0, (0, 0, 1.0), bark_grey("DO_t"), verts=14)
# Bare branches (4)
cyl("br_1", 0.04, 0.80, (0.30, 0.10, 2.20), bark_grey("DO_b1"), rot=(0, 0.7, 0), verts=8)
cyl("br_2", 0.04, 0.70, (-0.25, 0.05, 2.15), bark_grey("DO_b2"), rot=(0, -0.5, 0), verts=8)
cyl("br_3", 0.03, 0.50, (0.10, -0.20, 2.40), bark_grey("DO_b3"), rot=(0.4, 0, 0), verts=8)
cyl("br_4", 0.03, 0.45, (-0.15, -0.10, 2.30), bark_grey("DO_b4"), rot=(0.5, 0, 0), verts=8)
finalize("DeadOak")

# 6. Ancient Sequoia (massive)
reset_scene()
cyl("trunk_base", 0.50, 1.0, (0, 0, 0.5), bark_brown("Seq_tb"), verts=20)
cyl("trunk_mid", 0.42, 3.0, (0, 0, 2.5), bark_brown("Seq_tm"), verts=18)
cyl("trunk_top", 0.32, 2.0, (0, 0, 5.0), bark_brown("Seq_tt"), verts=16)
# Canopy
for i, (x, y, z, r) in enumerate([(0, 0, 6.5, 1.2), (0.6, 0.4, 6.2, 0.8), (-0.6, 0.4, 6.2, 0.8),
                                    (0.4, -0.5, 6.0, 0.7), (-0.4, -0.5, 6.0, 0.7), (0, 0, 7.2, 0.9)]):
    sphere(f"c_{i}", r, (x, y, z), leaves_d(f"Seq_c{i}"), segs=16, rings=12)
finalize("AncientSequoia")

# 7. Palm Tree (Moon 2+ tropical)
reset_scene()
cyl("trunk", 0.10, 4.0, (0, 0, 2.0), bark_grey("Palm_t"), verts=10)
# Ring grooves
for z in (0.5, 1.0, 1.5, 2.0, 2.5, 3.0, 3.5):
    torus(f"ring_{z}", 0.105, 0.008, (0, 0, z), bark_brown(f"Palm_r{z}"), mseg=12, miseg=3)
# 6 fronds at top
for i in range(6):
    a = i*(math.pi/3)
    cube(f"frond_{i}", (math.cos(a)*0.55, math.sin(a)*0.55, 4.2), (0.45, 0.06, 0.02), leaves_g(f"Palm_f{i}"), rot=(0, -0.4, a))
# Coconuts
for i in range(3):
    a = i*(math.pi*2/3)
    sphere(f"coconut_{i}", 0.06, (math.cos(a)*0.10, math.sin(a)*0.10, 3.95), bark_brown(f"Palm_co{i}"), segs=10, rings=8)
finalize("PalmTree")

# 8. Cypress (tall narrow)
reset_scene()
cyl("trunk", 0.12, 2.5, (0, 0, 1.25), bark_brown("Cy_t"), verts=12)
# Tall narrow conical foliage
cone("foliage", 0.40, 0.05, 2.5, (0, 0, 2.75), leaves_d("Cy_f"), verts=14)
finalize("Cypress")

# 9. Magnolia (pink blossom)
reset_scene()
cyl("trunk", 0.12, 1.5, (0, 0, 0.75), bark_grey("Mg_t"), verts=12)
# Crown of pink puffs
for i, (x, y, z, r) in enumerate([(0, 0, 1.7, 0.55), (0.30, 0.15, 1.65, 0.40), (-0.30, 0.15, 1.65, 0.40),
                                    (0.10, -0.25, 1.75, 0.35), (-0.10, -0.20, 1.70, 0.30)]):
    sphere(f"blossom_{i}", r, (x, y, z), leaves_pink(f"Mg_b{i}"), segs=14, rings=10)
finalize("MagnoliaTree")

# 10. Hawthorn (small thorned)
reset_scene()
cyl("trunk", 0.08, 1.2, (0, 0, 0.6), bark_grey("Hw_t"), verts=10)
sphere("crown", 0.45, (0, 0, 1.30), leaves_g("Hw_c"), segs=14, rings=10)
# Red berries
for i, (x, y, z) in enumerate([(0.30, 0.10, 1.35), (-0.25, 0.20, 1.40), (0.15, -0.30, 1.30),
                                  (-0.20, -0.10, 1.45), (0.35, 0.0, 1.20)]):
    sphere(f"berry_{i}", 0.025, (x, y, z), make_material(f"Hw_be{i}", (0.85, 0.20, 0.18), roughness=0.40, emission=(0.85, 0.20, 0.18), emission_strength=0.3), segs=8, rings=6)
finalize("HawthornTree")

# 11. Big Mushroom Tree (giant glow shroom)
reset_scene()
cap = make_material("GMT_cap", (0.55, 0.20, 0.50), roughness=0.85)
stem = make_material("GMT_stem", (0.92, 0.85, 0.75), roughness=0.85)
spots = make_material("GMT_spots", (0.95, 0.92, 0.85), roughness=0.85, emission=(0.85, 0.65, 0.20), emission_strength=0.4)
cyl("stem", 0.25, 2.0, (0, 0, 1.0), stem, verts=14)
sphere("cap", 0.85, (0, 0, 2.20), cap, segs=20, rings=14)
# Spots on cap
for i in range(6):
    a = i*(math.pi/3)
    sphere(f"spot_{i}", 0.10, (math.cos(a)*0.50, math.sin(a)*0.50, 2.50), spots, segs=10, rings=8)
finalize("BigMushroomTree")

# 12. World Tree (small) — glowing sacred tree
reset_scene()
cyl("trunk", 0.30, 3.0, (0, 0, 1.5), bark_white("WT_t"), verts=18)
# Glowing canopy
sphere("canopy_main", 1.2, (0, 0, 4.0), leaves_glow("WT_c1"), segs=20, rings=14)
sphere("canopy_l", 0.7, (0.85, 0.40, 3.80), leaves_glow("WT_c2"), segs=16, rings=12)
sphere("canopy_r", 0.7, (-0.85, 0.40, 3.80), leaves_glow("WT_c3"), segs=16, rings=12)
sphere("canopy_b", 0.7, (0.0, -0.85, 3.70), leaves_glow("WT_c4"), segs=16, rings=12)
sphere("canopy_t", 0.5, (0, 0, 5.0), leaves_glow("WT_c5"), segs=14, rings=10)
finalize("WorldTreeSmall")

print("done gen_flora_trees: 12 trees")
