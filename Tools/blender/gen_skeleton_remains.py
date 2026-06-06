"""Skeleton remains at Carved Stone POI — partial: skull, ribs, femur, scattered finger bones."""
import bpy, sys, os, math, random
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()
random.seed(42)
BONE_OLD = make_material("Bone_Old", (0.78, 0.74, 0.62), roughness=0.75)
BONE_DIRT = make_material("Bone_Dirty", (0.55, 0.50, 0.40), roughness=0.85)

# Skull (sphere + 2 eye sockets)
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.22, location=(0, 0.5, 0.20))
bpy.context.active_object.name="Skull"
bpy.context.active_object.data.materials.append(BONE_OLD)
# Eye sockets (dark spheres set inside)
for x in [-0.08, 0.08]:
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.04, location=(x, 0.37, 0.20))
    bpy.context.active_object.name=f"EyeSocket_{x}"
    bpy.context.active_object.data.materials.append(BONE_DIRT)

# Jaw (small cube under skull)
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.5, 0.06))
jaw = bpy.context.active_object; jaw.name="Jaw"; jaw.scale=(0.16, 0.10, 0.04); jaw.data.materials.append(BONE_OLD)

# Spine — 6 vertebrae
for i in range(6):
    bpy.ops.mesh.primitive_uv_sphere_add(radius=0.06, location=(0, 0.5 - 0.18*i - 0.18, 0.10))
    bpy.context.active_object.name=f"Vertebra_{i}"
    bpy.context.active_object.data.materials.append(BONE_OLD)

# Ribcage — 6 ribs arcing left+right
for i in range(6):
    y_off = 0.30 - i*0.10
    for side, sx in enumerate([-1, 1]):
        bpy.ops.mesh.primitive_torus_add(major_radius=0.22, minor_radius=0.018,
                                          location=(sx*0.18, y_off, 0.12),
                                          rotation=(0, math.pi/2, 0))
        ob = bpy.context.active_object
        ob.name = f"Rib_{i}_{side}"
        ob.scale = (0.5, 0.5, 0.5)
        ob.data.materials.append(BONE_OLD)

# Femurs (long bones for legs)
for side, sx in enumerate([-0.10, 0.10]):
    bpy.ops.mesh.primitive_cylinder_add(radius=0.04, depth=0.65, location=(sx, -0.6, 0.10),
                                         rotation=(math.pi/2, 0, 0))
    bpy.context.active_object.name = f"Femur_{side}"
    bpy.context.active_object.data.materials.append(BONE_OLD)

# Scattered finger bones (small cylinders around the chest)
for i in range(8):
    a = random.uniform(0, 2*math.pi)
    r = random.uniform(0.4, 0.7)
    bpy.ops.mesh.primitive_cylinder_add(radius=0.015, depth=0.08,
                                         location=(math.cos(a)*r, math.sin(a)*r, 0.05),
                                         rotation=(random.uniform(0,math.pi/2), random.uniform(0,math.pi), 0))
    bpy.context.active_object.name = f"FingerBone_{i}"
    bpy.context.active_object.data.materials.append(BONE_DIRT)

bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.join()
bpy.context.active_object.name = "SkeletonRemains"
export_fbx("SkeletonRemains")
print("[TARTARIA] SkeletonRemains done.")
