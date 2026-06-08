"""TownHall — 12m x 8m x 6m large civic hall with steeple + columns."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, cyl, bool_diff, bevel, solidify, pitched_roof,
                   make_polyhaven_material, join_and_finalize, save_and_export)

W, D, H_WALL, H_ROOF = 12.0, 8.0, 4.5, 2.5
WALL_THICK = 0.4
DOOR_W, DOOR_H = 2.0, 3.0  # grand double doors
WIN_W, WIN_H = 1.2, 1.8
COL_R, COL_H = 0.3, H_WALL  # 4 entrance columns


def main():
    reset_scene()
    walls = cube("Walls", (0, H_WALL/2, 0), (W/2, H_WALL/2, D/2))
    bool_diff(walls, cube("Door", (0, DOOR_H/2, D/2), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # 4 front windows (2 left + 2 right of door)
    for x_off in [-W/2 + 1.5, -W/2 + 4.0, W/2 - 4.0, W/2 - 1.5]:
        bool_diff(walls, cube("WinF", (x_off, 2.2, D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    # 3 side windows each
    for x_pos in [-W/2, W/2]:
        for z in [-2.5, 0, 2.5]:
            bool_diff(walls, cube("WinS", (x_pos, 2.2, z), (WALL_THICK, WIN_H/2, WIN_W/2)))
    # Back windows
    for x_off in [-W/2 + 3, W/2 - 3]:
        bool_diff(walls, cube("WinB", (x_off, 2.2, -D/2), (WIN_W/2, WIN_H/2, WALL_THICK)))
    solidify(walls, WALL_THICK)
    bevel(walls, 0.05, 2)
    roof = pitched_roof("Roof", (0, H_WALL + H_ROOF/2, 0), W + 0.5, D + 0.5, H_ROOF)
    bevel(roof, 0.04, 1)
    # Central steeple — short tower + spire on top of roof
    steeple_base = cube("SteepleBase", (0, H_WALL + H_ROOF + 0.5, 0), (1.0, 0.5, 1.0))
    import bpy
    bpy.ops.mesh.primitive_cone_add(vertices=12, radius1=1.0, radius2=0, depth=2.0, location=(0, H_WALL + H_ROOF + 2.0, 0), rotation=(math.radians(90), 0, 0))
    spire = bpy.context.active_object; spire.name = "Spire"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bevel(spire, 0.02, 1)
    # 4 entrance columns flanking the door (2 each side)
    cols = []
    for x_off in [-1.5, -1.0, 1.0, 1.5]:
        c = cyl(f"Col_{x_off}", (x_off, COL_H/2, D/2 + 0.6), COL_R, COL_H)
        cols.append(c)
    # Steps in front of door (3 wide steps)
    steps = []
    for i in range(3):
        s = cube(f"Step_{i}", (0, 0.15 + i*0.15, D/2 + 1.0 + i*0.3), (3.0, 0.075, 0.3))
        steps.append(s)
    # Door plane
    bpy.ops.mesh.primitive_plane_add(location=(0, DOOR_H/2, D/2 - WALL_THICK/2), size=1.0, rotation=(math.radians(90), 0, 0))
    door_p = bpy.context.active_object; door_p.name = "Door"
    door_p.scale = (DOOR_W, DOOR_H, 1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    walls.data.materials.append(make_polyhaven_material("Hall_Walls", "plaster_stone_wall_02"))
    roof.data.materials.append(make_polyhaven_material("Hall_Roof", "roof_slates_03"))
    steeple_base.data.materials.append(make_polyhaven_material("Hall_Steeple", "medieval_blocks_06"))
    spire.data.materials.append(make_polyhaven_material("Hall_Spire", "medieval_blocks_06"))
    for c in cols:
        c.data.materials.append(make_polyhaven_material("Hall_Column", "medieval_blocks_06"))
    for s in steps:
        s.data.materials.append(make_polyhaven_material("Hall_Step", "medieval_blocks_06"))
    door_p.data.materials.append(make_polyhaven_material("Hall_Door", "black_painted_planks"))
    o = join_and_finalize([walls, roof, steeple_base, spire, door_p] + cols + steps, "TownHall")
    save_and_export(o, "TownHall")


if __name__ == "__main__":
    main()
