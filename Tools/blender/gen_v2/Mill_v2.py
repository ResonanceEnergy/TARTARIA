"""Mill — 5m x 5m x 8m tall stone tower + 4-blade windmill on top. Iconic silhouette."""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib import (reset_scene, cube, cyl, bool_diff, bevel, solidify,
                   make_polyhaven_material, join_and_finalize, save_and_export)

R_BASE = 2.5  # tower base radius
R_TOP = 2.0   # tower top radius
H_TOWER = 7.0
WALL_THICK = 0.4
DOOR_W, DOOR_H = 1.0, 2.0
WIN_W, WIN_H = 0.7, 1.2


def main():
    reset_scene()
    # Tower — slightly tapered cylinder
    import bpy
    bpy.ops.mesh.primitive_cylinder_add(vertices=24, radius=R_BASE, depth=H_TOWER, location=(0, H_TOWER/2, 0), rotation=(math.radians(90), 0, 0))
    tower = bpy.context.active_object; tower.name = "Tower"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    # Door
    bool_diff(tower, cube("Door", (0, DOOR_H/2, R_BASE), (DOOR_W/2, DOOR_H/2, WALL_THICK)))
    # 3 windows up the tower at different heights
    for y in [2.5, 4.5, 6.0]:
        bool_diff(tower, cube("Win", (0, y, R_BASE), (WIN_W/2, WIN_H/2, WALL_THICK)))
    bevel(tower, 0.04, 1)
    # Conical roof
    bpy.ops.mesh.primitive_cone_add(vertices=24, radius1=R_TOP + 0.3, radius2=0, depth=2.0, location=(0, H_TOWER + 1.0, 0), rotation=(math.radians(90), 0, 0))
    roof = bpy.context.active_object; roof.name = "Roof"
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bevel(roof, 0.03, 1)
    # Windmill central hub (small cylinder horizontal on front)
    hub = cyl("Hub", (0, H_TOWER + 0.3, R_BASE + 0.4), 0.25, 0.5)
    hub.rotation_euler = (0, 0, math.radians(90))
    # 4 blades — flat boxes radiating from hub
    blades = []
    for angle in [0, 90, 180, 270]:
        bpy.ops.mesh.primitive_cube_add(location=(0, H_TOWER + 0.3, R_BASE + 0.45), size=2.0)
        b = bpy.context.active_object; b.name = f"Blade_{angle}"
        b.scale = (1.5, 0.05, 0.25)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        b.rotation_euler = (0, 0, math.radians(angle))
        # Offset to extend outward
        import math as M
        offset_x = 1.5 * M.cos(M.radians(angle))
        offset_y = 1.5 * M.sin(M.radians(angle))
        b.location = (offset_x, H_TOWER + 0.3 + offset_y, R_BASE + 0.45)
        blades.append(b)
    tower.data.materials.append(make_polyhaven_material("Mill_Stone", "medieval_blocks_06"))
    roof.data.materials.append(make_polyhaven_material("Mill_Roof", "roof_slates_03"))
    hub.data.materials.append(make_polyhaven_material("Mill_Wood", "black_painted_planks"))
    for b in blades:
        b.data.materials.append(make_polyhaven_material("Mill_BladeWood", "black_painted_planks"))
    o = join_and_finalize([tower, roof, hub] + blades, "Mill")
    save_and_export(o, "Mill")


if __name__ == "__main__":
    main()
