"""Market stall — 4 corner posts + canvas roof + counter + crate goods.

~2m wide, 2m deep, 2.5m tall. Place around village plaza for commerce hint.

2026-06-05 — Moon 1 DEEP hammer.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl
import bpy

reset_scene()
wood = make_material("Mkt_wood", (0.45, 0.30, 0.18), roughness=0.92)
canvas = make_material("Mkt_canvas", (0.78, 0.62, 0.38), roughness=0.85)
crate = make_material("Mkt_crate", (0.48, 0.32, 0.20), roughness=0.88)
goods_a = make_material("Mkt_goods_a", (0.78, 0.42, 0.20), roughness=0.85)
goods_b = make_material("Mkt_goods_b", (0.55, 0.78, 0.40), roughness=0.82)

# 4 corner posts
for x in [-1.0, 1.0]:
    for y in [-1.0, 1.0]:
        cube(f"post_{x}_{y}", (x, y, 1.25), (0.06, 0.06, 1.25), wood)

# Canvas roof — slight slope back-to-front
cube("canvas_back",  (0, 1.0, 2.50), (1.1, 0.05, 0.04), wood)
cube("canvas_front", (0, -1.0, 2.30), (1.1, 0.05, 0.04), wood)
# Roof panel (3 horizontal canvas strips)
for i, z in enumerate([2.46, 2.40, 2.34]):
    cube(f"roof_{i}", (0, -0.4 + i * 0.4, z), (1.05, 0.36, 0.02), canvas)

# Counter / front display
cube("counter_top", (0, -0.95, 1.0), (1.0, 0.10, 0.04), wood)
cube("counter_front_panel", (0, -1.05, 0.50), (1.0, 0.04, 0.50), wood)
# Counter shelf
cube("counter_shelf", (0, -0.85, 0.60), (0.95, 0.18, 0.03), wood)

# Crate stacks behind counter
cube("crate1", (-0.6, 0.7, 0.40), (0.30, 0.30, 0.30), crate)
cube("crate2", ( 0.6, 0.7, 0.40), (0.30, 0.30, 0.30), crate)
cube("crate3", ( 0.6, 0.7, 0.85), (0.25, 0.25, 0.20), crate)

# Display goods on counter (pyramid of fruit + sack)
cube("goods_a1", (-0.5, -0.95, 1.08), (0.10, 0.08, 0.07), goods_a)
cube("goods_a2", (-0.3, -0.95, 1.08), (0.10, 0.08, 0.07), goods_a)
cube("goods_a3", (-0.4, -0.95, 1.18), (0.08, 0.06, 0.06), goods_a)
cube("goods_b1", ( 0.4, -0.95, 1.10), (0.18, 0.18, 0.18), goods_b)

# Sign hung from front-top
cube("sign", (0, -1.07, 2.0), (0.30, 0.02, 0.15), wood)
cube("sign_chain_l", (-0.20, -1.06, 2.15), (0.02, 0.01, 0.20), wood)
cube("sign_chain_r", ( 0.20, -1.06, 2.15), (0.02, 0.01, 0.20), wood)

export_current_as("MarketStall", "Moon1")
print("[gen_market_stall] Done")
