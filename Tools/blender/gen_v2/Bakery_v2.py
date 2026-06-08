"""Bakery — 6m x 4.5m x 4m. Plaster + chimney + bread display window. Larger window front."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel, solidify, pitched_roof,
                   make_polyhaven_material, join_and_finalize, save_and_export)

W, D, H_WALL, H_ROOF = 6.0, 4.5, 3.0, 1.4
WALL_THICK = 0.3
DOOR_W, DOOR_H = 1.0, 2.0
BIG_WIN_W, BIG_WIN_H = 1.8, 1.2  # bread display window
WIN_Y_BIG = 0.9
CHIM_W, CHIM_D = 0.9, 0.9
CHIM_H = H_WALL + H_ROOF + 1.5


def main():
    reset_scene()
    walls = cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    # Door (right side of front)
    bool_diff(walls, cube("Door", (W/2 - 1.0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # Big bread-display window (left side of front)
    bool_diff(walls, cube("BigWin", (-W/2 + 1.8, WIN_Y_BIG + BIG_WIN_H/2, D/2), (BIG_WIN_W/2, BIG_WIN_H/2, WALL_THICK)))
    # Small side window
    bool_diff(walls, cube("SideWin", (-W/2, 1.7, 0), (WALL_THICK, 0.4, 0.4)))
    solidify(walls, WALL_THICK)
    bevel(walls, 0.04, 2)
    roof = pitched_roof("Roof", (0, H_WALL + H_ROOF/2, 0), W + 0.3, D + 0.3, H_ROOF)
    bevel(roof, 0.03, 1)
    chim = cube("Chim", (W/2 - 1.2, CHIM_H/2, -D/2 + 0.6), (CHIM_W/2, CHIM_H/2, CHIM_D/2))
    bevel(chim, 0.03, 1)
    import bpy
    bpy.ops.mesh.primitive_plane_add(location=(W/2 - 1.0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "DoorP"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    walls.data.materials.append(make_polyhaven_material("Bakery_Plaster", "painted_plaster_wall"))
    roof.data.materials.append(make_polyhaven_material("Bakery_Roof", "roof_slates_03"))
    chim.data.materials.append(make_polyhaven_material("Bakery_Chimney", "medieval_blocks_06"))
    door_p.data.materials.append(make_polyhaven_material("Bakery_Door", "black_painted_planks"))
    o = join_and_finalize([walls, roof, chim, door_p], "Bakery")
    save_and_export(o, "Bakery")


if __name__ == "__main__":
    main()
