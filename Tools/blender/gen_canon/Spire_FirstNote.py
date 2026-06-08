"""
Spire — "The First Note" — 3m base × 15m height (base-to-height ≈ φ²).

Per docs/15 §7 + docs/32 Art Bible:
- HEXAGONAL tapered tower (6-fold sacred geometry)
- 3m base, narrows to 1m at apex over 15m
- 3 vertical gold seam ribs (one every 2 hex faces — 3-6-9 rhythm)
- 3 horizontal band rings at φ-derived heights (3-6-9 rhythm)
- Apex emissive resonator (the "beacon" function — extends Aether visibility)
- Style: silhouette-first, reads at 200m as iconic tuning fork
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_canon import (
    reset_scene, hexagon, cone, cube, shade_smooth,
    bevel,
    make_matte_stone, make_aether_emissive,
    save_and_export, set_pivot_bottom_center,
    PHI, WARM_STONE, AETHER_GOLD, AETHER_CYAN,
)
import bpy

# canon: 3m base × 15m height
R_BASE = 1.5    # 3m diameter
R_APEX = 0.4
H_TOTAL = 15.0


def main():
    reset_scene()
    print("[Spire_FirstNote] hex tapered tower 3m -> 15m, 3-6-9 rhythm")

    # 1. Tapered hex TOWER — single cone primitive (6 sides for hexagonal silhouette)
    tower = cone("Tower", (0, H_TOTAL / 2, 0), R_BASE, R_APEX, H_TOTAL, vertices=6)
    bevel(tower, 0.05, 1)

    # 2. APEX RESONATOR — gold-glowing pyramid top + tiny orb
    apex_pyramid = cone("ApexPyramid", (0, H_TOTAL + 0.4, 0), R_APEX * 0.8, 0.0, 0.8, vertices=6)
    bpy.ops.mesh.primitive_ico_sphere_add(
        subdivisions=2, radius=0.18, location=(0, H_TOTAL + 1.0, 0),
    )
    apex_orb = bpy.context.active_object
    apex_orb.name = "ApexOrb"
    shade_smooth(apex_orb)

    # 3. THREE horizontal BAND rings at φ-derived heights (3-6-9 rhythm)
    # Heights: H/φ³, H/φ², H/φ ≈ 3.5, 5.7, 9.3 → cluster around middle of tower
    bands = []
    band_heights = [3.5, 5.7, 9.3]  # natural φ-derived spacing
    for i, h in enumerate(band_heights):
        # Tapered radius at h
        t = h / H_TOTAL
        r_h = R_BASE * (1 - t) + R_APEX * t
        bpy.ops.mesh.primitive_torus_add(
            major_radius=r_h + 0.05,
            minor_radius=0.07,
            major_segments=18,
            minor_segments=6,
            location=(0, h, 0),
        )
        band = bpy.context.active_object
        band.name = f"Band_{i}"
        bands.append(band)

    # 4. THREE vertical SEAM ribs (gold)
    # On 3 of 6 hex faces (every other face — 3-6-9 rhythm: 3 ribs)
    seams_v = []
    for i in range(3):
        angle_deg = i * 120 + 30  # 30, 150, 270 (offset onto edges)
        rad = math.radians(angle_deg)
        # Calculate tilted rib that follows the cone taper
        bx_b = R_BASE * math.cos(rad)
        bz_b = R_BASE * math.sin(rad)
        bx_t = R_APEX * math.cos(rad)
        bz_t = R_APEX * math.sin(rad)
        # Approximate the tapered rib as a tall thin box (will appear straight against tower)
        # Easier: a tall cube centered on tower side
        mid_x = (bx_b + bx_t) / 2
        mid_z = (bz_b + bz_t) / 2
        rib = cube(f"Seam_V_{i}", (mid_x * 1.05, H_TOTAL / 2, mid_z * 1.05), (0.06, H_TOTAL / 2 * 0.96, 0.06))
        # Rotate rib to face outward (tilt slightly per taper)
        rib.rotation_euler = (0, -rad + math.pi / 2, 0)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        seams_v.append(rib)

    # MATERIALS
    mat_stone = make_matte_stone("Spire_Stone", WARM_STONE)
    mat_gold = make_aether_emissive("Spire_Gold", AETHER_GOLD, 4.0)   # apex is the beacon — hottest emissive
    mat_cyan = make_aether_emissive("Spire_OrbCyan", AETHER_CYAN, 5.0)

    tower.data.materials.append(mat_stone)
    apex_pyramid.data.materials.append(mat_gold)
    apex_orb.data.materials.append(mat_cyan)   # beacon = cyan apex per Aether system Celestial 528Hz
    for b in bands:
        b.data.materials.append(mat_gold)
    for s in seams_v:
        s.data.materials.append(mat_gold)

    # JOIN
    all_objs = [tower, apex_pyramid, apex_orb] + bands + seams_v
    for o in all_objs:
        o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = tower
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Spire_FirstNote"
    set_pivot_bottom_center(o)
    save_and_export(o, "Spire_FirstNote")


if __name__ == "__main__":
    main()
