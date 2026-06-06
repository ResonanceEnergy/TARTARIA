"""Collectible parchment scroll on a small stone pedestal. Sparkles in-game via VFX."""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()
PARCHMENT = make_material("Lore_Parchment", (0.93, 0.85, 0.65), roughness=0.7)
SEAL      = make_material("Lore_RedSeal",   (0.65, 0.10, 0.10), roughness=0.4)
STONE     = make_material("Lore_PedestalStone", (0.55, 0.52, 0.48), roughness=0.85)
GOLD      = make_material("Lore_GoldGlow", (0.90, 0.75, 0.30), roughness=0.25, metallic=0.85,
                          emission=(0.90, 0.70, 0.20), emission_strength=2.0)

# Pedestal
bpy.ops.mesh.primitive_cube_add(size=1, location=(0,0,0.15))
ped = bpy.context.active_object; ped.name="Pedestal"; ped.scale=(0.45,0.45,0.15); ped.data.materials.append(STONE)

# Scroll body (cylinder)
bpy.ops.mesh.primitive_cylinder_add(radius=0.12, depth=0.32, location=(0,0,0.45), rotation=(math.pi/2,0,0))
sc = bpy.context.active_object; sc.name="ScrollBody"; sc.data.materials.append(PARCHMENT)
# Scroll end-caps
for x in [-0.16, 0.16]:
    bpy.ops.mesh.primitive_cylinder_add(radius=0.13, depth=0.04, location=(x,0,0.45), rotation=(0,math.pi/2,0))
    bpy.context.active_object.name=f"Cap_{x}"; bpy.context.active_object.data.materials.append(GOLD)

# Wax seal in middle
bpy.ops.mesh.primitive_uv_sphere_add(radius=0.06, location=(0,-0.13,0.45))
bpy.context.active_object.name="WaxSeal"; bpy.context.active_object.data.materials.append(SEAL)

bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.join()
bpy.context.active_object.name = "LoreArtifactScroll"
export_fbx("LoreArtifactScroll")
print("[TARTARIA] LoreArtifactScroll done.")
