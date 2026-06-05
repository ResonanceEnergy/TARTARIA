"""Detailed pine — tapered straight trunk + tiered conical needle layers.

Replaces cone-blob trees. ~6m tall pine. Wide ground footprint to give the
forest backdrop a different silhouette from the Oak.

2026-06-05 — Autonomous Blender content for Moon 1 polish loop.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone
import bpy

reset_scene()

bark = make_material("Pine_bark", (0.28, 0.18, 0.10), roughness=0.95)
needle_dark = make_material("Pine_needle_dark", (0.10, 0.28, 0.12), roughness=0.88)
needle_mid = make_material("Pine_needle_mid", (0.18, 0.40, 0.16), roughness=0.85)
needle_light = make_material("Pine_needle_light", (0.30, 0.52, 0.22), roughness=0.82)

# === Trunk — narrows toward top, 6m tall ===
cone("trunk", 0.30, 0.10, 6.0, (0, 0, 3.0), bark, verts=10)

# === Needle layers — 5 tiered cones, decreasing radius ===
# Tier 1 — widest, bottom
cone("needles_t1", 1.60, 0.10, 1.4, (0, 0, 1.8), needle_dark, verts=14)
# Tier 2
cone("needles_t2", 1.40, 0.10, 1.3, (0, 0, 2.8), needle_dark, verts=14)
# Tier 3
cone("needles_t3", 1.15, 0.10, 1.2, (0, 0, 3.7), needle_mid, verts=14)
# Tier 4
cone("needles_t4", 0.90, 0.08, 1.1, (0, 0, 4.5), needle_mid, verts=14)
# Tier 5 — top crown
cone("needles_t5", 0.60, 0.05, 1.0, (0, 0, 5.3), needle_light, verts=14)
# Tip
cone("needles_tip", 0.30, 0.02, 0.6, (0, 0, 6.0), needle_light, verts=10)

# === Exposed lower branches (3 short stubs at trunk base for character) ===
for i in range(3):
    a = i * 2.094  # 120 deg apart
    cyl(f"branch_stub_{i}", 0.08, 0.4,
        (math.cos(a) * 0.40, math.sin(a) * 0.40, 1.5),
        bark,
        rot=(0.3 * math.cos(a + math.pi / 2),
             0.3 * math.sin(a + math.pi / 2),
             0),
        verts=6)

export_current_as("PineTree", "Moon1")
print("[gen_pine_tree] Done")
