"""
Dome — "Listeners' Hall" — 25m diameter × 18m height, 80% buried.

Per docs/15 §7 + docs/32 Art Bible:
- Dodecagonal drum base (12 sides — sacred 12-fold geometry)
- Hemispherical cap with central oculus (Tartarian listen-to-Earth chamber)
- 4 "rose-window" arched cuts in the drum (cardinal directions, golden ratio width)
- Matte warm stone + Aether-Gold seam emissive at:
  - drum-to-dome junction (1 horizontal ring)
  - 12 vertical seams up the drum (one per dodecagon facet)
  - oculus ring at apex
- Total 18m: drum 9m + dome 9m hemisphere rise
- Style touchstone: Hollow Knight silhouette + Tunic sacred geometry
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_canon import (
    reset_scene, dodecagon, hemisphere, cube, cube as cube_,
    boolean_diff, bevel, solidify, shade_smooth,
    make_matte_stone, make_aether_emissive,
    save_and_export, set_pivot_bottom_center,
    WARM_STONE, AETHER_GOLD,
)
import bpy

R = 12.5    # 25m diameter
# R155: drum height set to 5.5m so total = drum(5.5) + hemisphere rise(R=12.5) = 18m per spec.
# This avoids the scale_y hack that was eaten by bake_space_transform=True on FBX export.
H_DRUM = 5.5
H_DOME = R   # hemisphere always rises by its radius
THICK = 0.5   # wall thickness

# Rose-window arch dimensions (4 cardinal cuts)
ARCH_W = 4.0   # ~25m × (1/φ × 0.3) — heroic, not noisy
ARCH_H = 5.5


def main():
    reset_scene()
    print("[Dome_Listeners] building canon hemisphere on dodecagonal drum")

    # 1. DRUM — 12-sided prism
    drum = dodecagon("Drum", (0, H_DRUM / 2, 0), R, H_DRUM)
    bevel(drum, 0.08, 2)

    # 2. Cut 4 ROSE-WINDOW arches (N/E/S/W) — each is a tall cube + half cylinder cap
    for angle_deg in [0, 90, 180, 270]:
        rad = math.radians(angle_deg)
        cx = R * 1.05 * math.cos(rad)
        cz = R * 1.05 * math.sin(rad)
        # Rectangular cutter
        cutter = cube("RoseRect", (cx, ARCH_H / 2, cz), (ARCH_W / 2, ARCH_H / 2, THICK * 1.5))
        cutter.rotation_euler = (0, -rad, 0)
        boolean_diff(drum, cutter)
        # Arch cap (half-cylinder above)
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=16, radius=ARCH_W / 2, depth=THICK * 3.5,
            location=(cx, ARCH_H, cz),
            rotation=(0, -rad, 0),
        )
        cap = bpy.context.active_object
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        boolean_diff(drum, cap)

    bevel(drum, 0.03, 1)

    # 3. HEMISPHERE DOME — half-sphere cap (rises by exactly R from drum top)
    dome = hemisphere("Dome", (0, H_DRUM, 0), R)
    shade_smooth(dome)

    # 4. OCULUS — cylinder cut through apex
    oc_r = 2.0
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=24, radius=oc_r, depth=R * 1.5,
        location=(0, H_DRUM + H_DOME, 0),
        rotation=(0, 0, 0),  # Y-axis vertical = matches Blender default Y up after FBX export... actually Z-up Blender
    )
    # Blender is Z-up by default but we set rotation=(90,0,0) in cylinder helper to make cylinder Y-axis vertical in OUR engine
    # For oculus we want it vertical through dome apex — easier with native Z-axis cylinder
    oculus = bpy.context.active_object
    oculus.name = "OculusCutter"
    # Rotate so cylinder axis aligns with world Y (the dome's vertical axis)
    oculus.rotation_euler = (math.radians(90), 0, 0)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    boolean_diff(dome, oculus)

    # 5. GOLD SEAM at drum-dome junction (low-profile ring)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=R + 0.05,
        minor_radius=0.18,
        major_segments=48,
        minor_segments=8,
        location=(0, H_DRUM, 0),
    )
    seam_ring = bpy.context.active_object
    seam_ring.name = "GoldSeam_Ring"

    # 6. OCULUS RING (gold around oculus opening)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=oc_r + 0.1,
        minor_radius=0.15,
        major_segments=24,
        minor_segments=8,
        location=(0, H_DRUM + H_DOME - 0.1, 0),
    )
    oculus_ring = bpy.context.active_object
    oculus_ring.name = "GoldSeam_Oculus"

    # 7. 12 VERTICAL drum seams (one per dodecagon edge, Aether-Gold)
    seams_v = []
    for i in range(12):
        angle_deg = i * 30 + 15  # offset so seams sit on edges not faces
        rad = math.radians(angle_deg)
        # Thin vertical bar at each edge
        bx = (R + 0.03) * math.cos(rad)
        bz = (R + 0.03) * math.sin(rad)
        bar = cube(f"Seam_V_{i}", (bx, H_DRUM / 2, bz), (0.08, H_DRUM / 2 * 0.95, 0.08))
        seams_v.append(bar)

    # MATERIALS
    mat_stone = make_matte_stone("Dome_Stone", WARM_STONE)
    mat_gold = make_aether_emissive("Dome_GoldSeam", AETHER_GOLD, 3.0)

    drum.data.materials.append(mat_stone)
    dome.data.materials.append(mat_stone)
    seam_ring.data.materials.append(mat_gold)
    oculus_ring.data.materials.append(mat_gold)
    for s in seams_v:
        s.data.materials.append(mat_gold)

    # JOIN ALL
    all_objs = [drum, dome, seam_ring, oculus_ring] + seams_v
    for o in all_objs:
        o.select_set(False)
    for o in all_objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = drum
    bpy.ops.object.join()
    o = bpy.context.active_object
    o.name = "Dome_ListenersHall"
    set_pivot_bottom_center(o)
    save_and_export(o, "Dome_ListenersHall")


if __name__ == "__main__":
    main()
