"""Watchtower — 4m x 4m x 12m stone tower with crenellated top + arrow slits."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel, solidify,
                   make_polyhaven_material, join_and_finalize, save_and_export)

W, D, H_TOWER = 4.0, 4.0, 11.0
WALL_THICK = 0.5
DOOR_W, DOOR_H = 1.0, 2.0
SLIT_W, SLIT_H = 0.15, 0.8  # arrow slits


def main():
    reset_scene()
    walls = cube("Walls", (0, H_TOWER/2, 0), (W/2, H_TOWER/2, D/2))
    # Door (front)
    bool_diff(walls, cube("Door", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # Arrow slits at 3 heights on all 4 sides
    for y in [3.5, 6.5, 9.0]:
        # Front
        bool_diff(walls, cube("Slit", (0, y, D/2), (SLIT_W/2, SLIT_H/2, WALL_THICK)))
        # Back
        bool_diff(walls, cube("Slit", (0, y, -D/2), (SLIT_W/2, SLIT_H/2, WALL_THICK)))
        # Left
        bool_diff(walls, cube("Slit", (-W/2, y, 0), (WALL_THICK, SLIT_H/2, SLIT_W/2)))
        # Right
        bool_diff(walls, cube("Slit", (W/2, y, 0), (WALL_THICK, SLIT_H/2, SLIT_W/2)))
    solidify(walls, WALL_THICK)
    bevel(walls, 0.04, 2)
    # Battlement / crenellation top — 8 small cubes arranged around perimeter
    crenels = []
    half = W/2
    positions = [
        (-half, 0), (-half + 1.5, 0), (0, 0), (half - 1.5, 0), (half, 0),   # front row pieces
    ]
    # 12 crenels around the perimeter
    crenel_h = 0.6
    crenel_w = 0.4
    for x in [-half, -half + 1.2, half - 1.2, half]:
        for z in [-half, -half + 1.2, half - 1.2, half]:
            if abs(x) >= half or abs(z) >= half:  # on perimeter
                c = cube(f"Cr_{x}_{z}", (x, H_TOWER + crenel_h/2, z), (crenel_w/2, crenel_h/2, crenel_w/2))
                crenels.append(c)
    # Crenel base ring (a flat thicker slab where they sit)
    base_slab = cube("BattlementSlab", (0, H_TOWER + 0.05, 0), (W/2 + 0.2, 0.1, D/2 + 0.2))
    # Door plank
    import bpy
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    walls.data.materials.append(make_polyhaven_material("Watch_Stone", "medieval_blocks_06"))
    base_slab.data.materials.append(make_polyhaven_material("Watch_Slab", "medieval_blocks_06"))
    for c in crenels:
        c.data.materials.append(make_polyhaven_material("Watch_Crenel", "medieval_blocks_06"))
    door_p.data.materials.append(make_polyhaven_material("Watch_Door", "black_painted_planks"))
    o = join_and_finalize([walls, base_slab, door_p] + crenels, "Watchtower")
    save_and_export(o, "Watchtower")


if __name__ == "__main__":
    main()
