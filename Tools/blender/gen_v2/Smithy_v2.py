"""Smithy — 7m x 5m x 5m. Stone walls (heat-resistant), wide front opening (forge), tall chimney."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, bool_diff, bevel, solidify, pitched_roof,
                   make_polyhaven_material, join_and_finalize, save_and_export)

W, D, H_WALL, H_ROOF = 7.0, 5.0, 3.5, 1.8
WALL_THICK = 0.4  # thicker for forge insulation
OPEN_W, OPEN_H = 2.5, 2.5  # forge front opening
CHIM_W, CHIM_D = 1.2, 1.2  # tall sturdy chimney
CHIM_H = H_WALL + H_ROOF + 2.0


def main():
    reset_scene()
    walls = cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    # Wide forge opening (front, no door)
    bool_diff(walls, cube("Opening", (0, OPEN_H/2, D/2), (OPEN_W/2, OPEN_H/2, WALL_THICK)))
    # Small windows on sides
    for x_pos in [-W/2, W/2]:
        bool_diff(walls, cube("Win", (x_pos, 2.4, 0), (WALL_THICK, 0.4, 0.4)))
    solidify(walls, WALL_THICK)
    bevel(walls, 0.05, 2)
    roof = pitched_roof("Roof", (0, H_WALL + H_ROOF/2, 0), W + 0.3, D + 0.3, H_ROOF)
    bevel(roof, 0.03, 1)
    # Big chimney at back-center
    chim = cube("Chim", (0, CHIM_H/2, -D/2 + 0.8), (CHIM_W/2, CHIM_H/2, CHIM_D/2))
    bevel(chim, 0.04, 2)
    # Forge floor stones (a low base inside the opening)
    floor_stone = cube("ForgeStone", (0, 0.15, D/2 - 0.6), (1.2, 0.15, 0.6))
    walls.data.materials.append(make_polyhaven_material("Smithy_Stone", "medieval_blocks_06"))
    roof.data.materials.append(make_polyhaven_material("Smithy_Roof", "roof_slates_03"))
    chim.data.materials.append(make_polyhaven_material("Smithy_Chimney", "medieval_blocks_06"))
    floor_stone.data.materials.append(make_polyhaven_material("Smithy_ForgeFloor", "medieval_blocks_06"))
    o = join_and_finalize([walls, roof, chim, floor_stone], "Smithy")
    save_and_export(o, "Smithy")


if __name__ == "__main__":
    main()
