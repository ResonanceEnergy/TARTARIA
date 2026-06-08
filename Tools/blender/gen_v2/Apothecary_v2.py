"""Apothecary — 5m x 4.5m x 6m (taller, narrow). Plaster + many small herb-display windows + hanging sign post."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel, solidify, pitched_roof,
                   make_polyhaven_material, join_and_finalize, save_and_export)

W, D = 5.0, 4.5
H_WALL = 5.0  # taller — 2 story compact
H_ROOF = 1.5
WALL_THICK = 0.3
DOOR_W, DOOR_H = 0.9, 2.0
WIN_W, WIN_H = 0.5, 0.6  # small herb display windows
SIGN_W, SIGN_H, SIGN_D = 1.2, 0.5, 0.05


def main():
    reset_scene()
    walls = cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    bool_diff(walls, cube("Door", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # Multiple small windows in 2 rows (3 per row)
    for y in [1.4, 3.5]:
        for x_off in [-W/2 + 0.9, 0, W/2 - 0.9]:
            if not (y < 2.2 and abs(x_off) < 0.5):  # skip overlap with door
                bool_diff(walls, cube("Win", (x_off, y, D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    # Side windows
    for x_pos in [-W/2, W/2]:
        bool_diff(walls, cube("WinS", (x_pos, 2.5, 0), (WALL_THICK, WIN_H/2, WIN_W/2)))
    solidify(walls, WALL_THICK)
    bevel(walls, 0.04, 2)
    roof = pitched_roof("Roof", (0, H_WALL + H_ROOF/2, 0), W + 0.3, D + 0.3, H_ROOF)
    bevel(roof, 0.03, 1)
    # Hanging sign post (bracket + plank above the door)
    bracket = cube("Bracket", (0, DOOR_H + 0.3, D/2 + 0.4), (0.05, 0.05, 0.4))
    sign = cube("Sign", (0, DOOR_H + 0.0, D/2 + 0.8), (SIGN_W/2, SIGN_H/2, SIGN_D/2))
    # Door plane
    import bpy
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    walls.data.materials.append(make_polyhaven_material("Apo_Plaster", "painted_plaster_wall"))
    roof.data.materials.append(make_polyhaven_material("Apo_Roof", "roof_slates_03"))
    bracket.data.materials.append(make_polyhaven_material("Apo_Wood", "black_painted_planks"))
    sign.data.materials.append(make_polyhaven_material("Apo_Sign", "black_painted_planks"))
    door_p.data.materials.append(make_polyhaven_material("Apo_Door", "black_painted_planks"))
    o = join_and_finalize([walls, roof, bracket, sign, door_p], "Apothecary")
    save_and_export(o, "Apothecary")


if __name__ == "__main__":
    main()
