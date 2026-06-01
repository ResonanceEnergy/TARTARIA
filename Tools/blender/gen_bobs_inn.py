"""
Bob's Inn — small cabin with thatched roof, warm windows, signpost.
Triggers Moon 2 transition when player rests.
"""
import bpy, math
import sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()

WOOD       = make_material("Inn_Wood", (0.45, 0.30, 0.18), roughness=0.7)
WOOD_DARK  = make_material("Inn_WoodDark", (0.30, 0.20, 0.12), roughness=0.7)
STONE      = make_material("Inn_Stone", (0.50, 0.48, 0.44), roughness=0.85)
THATCH     = make_material("Inn_Thatch", (0.55, 0.42, 0.22), roughness=0.95)
WINDOW     = make_material("Inn_Window", (1.0, 0.85, 0.45), roughness=0.2,
                           emission=(1.0, 0.78, 0.30), emission_strength=2.5)

def cube(name, loc, sz, mat):
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc)
    ob = bpy.context.active_object
    ob.name = name
    ob.scale = sz
    ob.data.materials.append(mat)
    return ob

# Stone foundation (low, wider than walls)
cube("Foundation", (0, 0, 0.15), (3.6, 4.6, 0.30), STONE)

# Wooden walls (3.0 wide × 4.0 deep × 2.4 tall)
cube("WallFront", (0, 2.0, 1.5), (3.0, 0.12, 2.4), WOOD)
cube("WallBack",  (0,-2.0, 1.5), (3.0, 0.12, 2.4), WOOD)
cube("WallLeft", (-1.5, 0, 1.5), (0.12, 4.0, 2.4), WOOD)
cube("WallRight",( 1.5, 0, 1.5), (0.12, 4.0, 2.4), WOOD)

# Door (front center)
cube("Door", (0, 2.06, 0.9), (0.8, 0.06, 1.6), WOOD_DARK)

# Windows (front sides + side walls, glowing)
cube("WinFront_L", (-1.0, 2.06, 1.7), (0.5, 0.06, 0.6), WINDOW)
cube("WinFront_R", ( 1.0, 2.06, 1.7), (0.5, 0.06, 0.6), WINDOW)
cube("WinLeft",   (-1.56, 0.5, 1.7), (0.06, 0.5, 0.6), WINDOW)
cube("WinRight",  ( 1.56, 0.5, 1.7), (0.06, 0.5, 0.6), WINDOW)

# Thatched roof (4 sloped quads — approximated with rotated cubes)
import math
roof_h = 1.5
# Front slope
roof_f = cube("RoofFront", (0, 1.0, 2.7+roof_h/2), (3.4, 2.5, 0.16), THATCH)
roof_f.rotation_euler.x = math.radians(35)
# Back slope
roof_b = cube("RoofBack", (0,-1.0, 2.7+roof_h/2), (3.4, 2.5, 0.16), THATCH)
roof_b.rotation_euler.x = math.radians(-35)
# Ridge
cube("Roof_Ridge", (0, 0, 3.4), (3.4, 0.16, 0.16), WOOD_DARK)

# Chimney (back-right)
cube("Chimney", (1.0, -1.5, 3.2), (0.5, 0.5, 1.5), STONE)
cube("ChimneyCap", (1.0, -1.5, 4.05), (0.6, 0.6, 0.1), STONE)

# Signpost (front-left)
cube("SignPost",  (-1.7, 2.5, 1.2), (0.08, 0.08, 1.8), WOOD)
cube("SignBoard", (-2.1, 2.5, 1.9), (0.9, 0.04, 0.45), WOOD_DARK)
cube("SignArm",   (-1.9, 2.5, 1.9), (0.30, 0.04, 0.04), WOOD)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.join()
bpy.context.active_object.name = "BobsInn"
export_fbx("BobsInn")
print("[TARTARIA] BobsInn done.")
