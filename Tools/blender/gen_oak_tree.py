"""Detailed oak tree — gnarled trunk + branching + leaf canopy clusters.

Replaces the cone-blob trees in Echohaven with something that reads as forest.
~5m tall, ~3.5m canopy. Use as Echohaven backdrop vegetation.

2026-06-05 — Autonomous Blender content for Moon 1 polish loop.
"""
import os, sys, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

random.seed(42)

reset_scene()

# Materials — warm earthy oak palette
bark = make_material("Oak_bark", (0.32, 0.22, 0.14), roughness=0.92)
bark_dark = make_material("Oak_bark_dark", (0.22, 0.14, 0.08), roughness=0.95)
leaf_dark = make_material("Oak_leaf_dark", (0.18, 0.34, 0.14), roughness=0.85)
leaf_mid = make_material("Oak_leaf_mid", (0.28, 0.48, 0.20), roughness=0.80)
leaf_light = make_material("Oak_leaf_light", (0.42, 0.62, 0.28), roughness=0.78)

# === Trunk ===
# Main trunk — slight taper, 4m tall
cyl("trunk_main", 0.32, 4.0, (0, 0, 2.0), bark, verts=12)
# Buttresses at base (4 directions) — gives ground-rooting feel
for i in range(4):
    a = i * math.pi / 2
    cube(f"buttress_{i}",
         (math.cos(a) * 0.42, math.sin(a) * 0.42, 0.30),
         (0.12, 0.18, 0.30),
         bark_dark,
         rot=(0, 0, a))

# === Main branches (3 thick branches forking from trunk @ 2.8m) ===
branch_angles = [0, 2.1, 4.2]  # 120 degrees apart
for i, theta in enumerate(branch_angles):
    # Branch base direction
    bx = math.cos(theta) * 0.7
    by = math.sin(theta) * 0.7
    # Diagonal upward branch — 1.2m long, tilted ~40 degrees from vertical
    cyl(f"branch_{i}", 0.14, 1.4,
        (bx * 0.5, by * 0.5, 3.0),
        bark,
        rot=(0.8 * math.cos(theta + math.pi / 2),
             0.8 * math.sin(theta + math.pi / 2),
             0),
        verts=8)
    # Secondary smaller branch
    cyl(f"branch_sub_{i}", 0.08, 0.9,
        (bx * 1.0, by * 1.0, 3.6),
        bark_dark,
        rot=(0.6 * math.cos(theta + math.pi / 2),
             0.6 * math.sin(theta + math.pi / 2),
             0),
        verts=6)

# === Leaf canopy — 12 overlapping spheres in 3 layers ===
canopy_centers = []
# Lower layer — 6 spheres at h=4.0, radius 1.2 ring
for i in range(6):
    a = (i / 6.0) * math.pi * 2 + 0.3
    canopy_centers.append((math.cos(a) * 1.2, math.sin(a) * 1.2, 4.1, leaf_dark, 1.05))
# Mid layer — 4 spheres at h=4.8
for i in range(4):
    a = (i / 4.0) * math.pi * 2 + 0.8
    canopy_centers.append((math.cos(a) * 0.85, math.sin(a) * 0.85, 4.7, leaf_mid, 1.15))
# Top crown — 2 spheres at h=5.5
canopy_centers.append((0.0, 0.0, 5.4, leaf_light, 1.2))
canopy_centers.append((0.3, -0.2, 5.7, leaf_light, 0.95))

for i, (x, y, z, mat, r) in enumerate(canopy_centers):
    sphere(f"leaf_{i}", r, (x, y, z), mat, segs=10, rings=8)

# === Export ===
export_current_as("OakTree", "Moon1")
print("[gen_oak_tree] Done")
