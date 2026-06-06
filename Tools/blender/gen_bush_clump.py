"""Detailed bush — 5 overlapping leaf clusters at varied heights.

Replaces flat green spheres. ~0.8m tall, ~1.2m wide. Use as ground cover under
trees + around hero buildings.

2026-06-05 — Autonomous Blender content for Moon 1 polish loop.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, sphere
import bpy

reset_scene()

leaf_a = make_material("Bush_a", (0.24, 0.42, 0.18), roughness=0.88)
leaf_b = make_material("Bush_b", (0.30, 0.50, 0.22), roughness=0.85)
leaf_c = make_material("Bush_c", (0.36, 0.56, 0.26), roughness=0.82)

# 5 overlapping clusters — clumpy bush shape
clusters = [
    ((0.00, 0.00, 0.42), 0.45, leaf_b),  # center
    ((-0.30, 0.10, 0.30), 0.32, leaf_a),  # left
    ((0.28, -0.05, 0.32), 0.34, leaf_a),  # right
    ((0.05, 0.25, 0.55), 0.30, leaf_c),  # back-top
    ((-0.10, -0.20, 0.55), 0.28, leaf_c),  # front-top
]
for i, (loc, r, mat) in enumerate(clusters):
    sphere(f"clump_{i}", r, loc, mat, segs=8, rings=6)

export_current_as("BushClump", "Moon1")
print("[gen_bush_clump] Done")
