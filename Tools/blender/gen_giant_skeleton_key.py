"""Giant Skeleton Key #1 — Moon 1 keystone collectible. 3 segments, gold-veined stone."""
import bpy, sys, os
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()
STONE_PALE = make_material("Key_PaleStone", (0.85, 0.80, 0.70), roughness=0.6)
GOLD_VEIN  = make_material("Key_GoldVein", (0.92, 0.78, 0.32), roughness=0.3, metallic=0.85,
                           emission=(0.95, 0.75, 0.25), emission_strength=2.5)

# Keystone shape: trapezoidal head + cylindrical shaft + bit teeth
# Head — wide block tapering down
bpy.ops.mesh.primitive_cube_add(size=1, location=(0,0,1.4))
head = bpy.context.active_object; head.name="KeyHead"; head.scale=(0.45,0.18,0.32); head.data.materials.append(STONE_PALE)

# Gold inlay band on head
bpy.ops.mesh.primitive_cube_add(size=1, location=(0,0,1.4))
band = bpy.context.active_object; band.name="GoldBand"; band.scale=(0.48,0.20,0.05); band.data.materials.append(GOLD_VEIN)

# Shaft
bpy.ops.mesh.primitive_cylinder_add(radius=0.06, depth=1.0, location=(0,0,0.55))
shaft = bpy.context.active_object; shaft.name="KeyShaft"; shaft.data.materials.append(STONE_PALE)

# Bit teeth (3 tines at bottom)
for i, off in enumerate([-0.14, 0.0, 0.14]):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(off, 0, 0.10))
    t = bpy.context.active_object; t.name=f"Tooth_{i}"
    t.scale=(0.05, 0.05, 0.10 + abs(off)*0.5)
    t.data.materials.append(STONE_PALE)

# Decorative ring around shaft mid
bpy.ops.mesh.primitive_torus_add(major_radius=0.10, minor_radius=0.02, location=(0,0,0.55))
ring = bpy.context.active_object; ring.name="ShaftRing"; ring.data.materials.append(GOLD_VEIN)

bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.join()
bpy.context.active_object.name = "GiantSkeletonKey"
export_fbx("GiantSkeletonKey")
print("[TARTARIA] GiantSkeletonKey done.")
