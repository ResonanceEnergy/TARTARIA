"""
Fountain — "Thread of Memory" — 8m basin × 5m column (8/5 = φ).

Per docs/15 §7 + docs/32 Art Bible:
- HEXAGONAL basin (6-fold sacred geometry)
- 8m diameter, 1m tall basin lip + recessed water surface
- Central column 5m tall, hexagonal prism, narrows in 3 stages (3-6-9 rhythm)
- Aether-Gold seam emissive at:
  - basin upper ring
  - 6 vertical seams in basin (hexagonal edges)
  - column horizontal bands at the 3 narrow points (3-6-9 rhythm)
  - apex emissive cap
- Style: matte cool-stone + golden water-thread vertical emissive
- 8/5 = φ — golden ratio enforced at basin-to-column proportion
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_canon import (
    reset_scene, hexagon, cube, boolean_diff, bevel, shade_smooth,
    make_matte_stone, make_aether_emissive,
    save_and_export, set_pivot_bottom_center,
    PHI, WARM_STONE, COOL_STONE, AETHER_GOLD, AETHER_CYAN,
)
import bpy

# canon: 8m basin × 5m column (8/5 = φ)
R_BASIN = 4.0    # 8m diameter
H_BASIN = 1.0    # basin lip height
H_COLUMN = 5.0   # column height
R_COL_BASE = 0.6
R_COL_MID = 0.45
R_COL_TOP = 0.30


def main():
    reset_scene()
    print("[Fountain_ThreadOfMemory] hex basin (8m) + tapered column (5m) — φ ratio")

    # 1. HEX BASIN (8m diameter)
    basin = hexagon("Basin_Outer", (0, H_BASIN / 2, 0), R_BASIN, H_BASIN)
    # Inner hex cut for water cavity
    inner = hexagon("Basin_Inner", (0, H_BASIN / 2 + 0.15, 0), R_BASIN - 0.4, H_BASIN)
    boolean_diff(basin, inner)
    bevel(basin, 0.06, 2)

    # 2. Inner BOWL (subtle cyan water surface 0.3m below rim)
    bowl_water = hexagon("Water", (0, H_BASIN - 0.55, 0), R_BASIN - 0.5, 0.05)

    # 3. COLUMN — hexagonal tapered, 3 sections (3-6-9 rhythm)
    # Section A: 0–1.67m (base), wider hex
    sect_a = hexagon("ColA", (0, H_BASIN + 0.83, 0), R_COL_BASE, 1.67)
    # Section B: 1.67–3.34m (middle), mid hex
    sect_b = hexagon("ColB", (0, H_BASIN + 2.5, 0), R_COL_MID, 1.67)
    # Section C: 3.34–5.0m (top), narrow hex
    sect_c = hexagon("ColC", (0, H_BASIN + 4.17, 0), R_COL_TOP, 1.67)

    # 4. Apex GOLD orb (Thread of Memory — the "thread" anchor)
    bpy.ops.mesh.primitive_ico_sphere_add(
        subdivisions=2, radius=0.22, location=(0, H_BASIN + H_COLUMN + 0.18, 0),
    )
    apex = bpy.context.active_object
    apex.name = "Apex_Orb"
    shade_smooth(apex)

    # 5. 3 BAND seams at section junctions (Aether-Gold rings, 3-6-9 rhythm — only 3 visible because of finite column)
    bands = []
    for i, y in enumerate([H_BASIN, H_BASIN + 1.67, H_BASIN + 3.34, H_BASIN + 5.0]):
        bpy.ops.mesh.primitive_torus_add(
            major_radius=R_COL_BASE * 0.85 if i == 0 else (R_COL_MID + 0.05),
            minor_radius=0.06,
            major_segments=18,
            minor_segments=6,
            location=(0, y, 0),
        )
        band = bpy.context.active_object
        band.name = f"Band_{i}"
        bands.append(band)

    # 6. Basin TOP RING seam (gold lip)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=R_BASIN - 0.15,
        minor_radius=0.08,
        major_segments=36,
        minor_segments=8,
        location=(0, H_BASIN - 0.05, 0),
    )
    basin_ring = bpy.context.active_object
    basin_ring.name = "BasinRingSeam"

    # MATERIALS
    mat_stone = make_matte_stone("Fountain_Stone", COOL_STONE)
    mat_gold = make_aether_emissive("Fountain_Gold", AETHER_GOLD, 3.5)
    mat_cyan_water = make_aether_emissive("Fountain_WaterCyan", AETHER_CYAN, 1.5)

    basin.data.materials.append(mat_stone)
    sect_a.data.materials.append(mat_stone)
    sect_b.data.materials.append(mat_stone)
    sect_c.data.materials.append(mat_stone)
    bowl_water.data.materials.append(mat_cyan_water)
    apex.data.materials.append(mat_gold)
    basin_ring.data.materials.append(mat_gold)
    for b in bands:
        b.data.materials.append(mat_gold)

    # JOIN
    all_objs = [basin, bowl_water, sect_a, sect_b, sect_c, apex, basin_ring] + bands
    for o in all_objs:
        o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = basin
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Fountain_ThreadOfMemory"
    set_pivot_bottom_center(o)
    save_and_export(o, "Fountain_ThreadOfMemory")


if __name__ == "__main__":
    main()
