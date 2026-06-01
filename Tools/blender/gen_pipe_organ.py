"""
The Pipe Organ centerpiece — Moon 1 canonical puzzle prop.
Verticle organ pipes + manuals + pedalboard + carved console.
Per docs/03: massive pipe organ thundering 432 Hz when restored.
"""
import bpy, sys, os, math
sys.path.append(os.path.dirname(__file__))
from _common import reset_scene, make_material, export_fbx

reset_scene()
WOOD_DARK = make_material("Organ_WoodDark", (0.18, 0.10, 0.06), roughness=0.5)
WOOD_RICH = make_material("Organ_WoodRich", (0.30, 0.16, 0.08), roughness=0.5)
PIPE_BRASS = make_material("Organ_Brass", (0.85, 0.70, 0.30), roughness=0.25, metallic=0.85)
PIPE_DULL = make_material("Organ_DullPipe", (0.50, 0.45, 0.35), roughness=0.7, metallic=0.6)
IVORY = make_material("Organ_Ivory", (0.95, 0.92, 0.85), roughness=0.4)

# Console base (the body the player stands at)
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, 0.5))
base = bpy.context.active_object; base.name="Console"; base.scale=(2.5, 1.0, 1.0); base.data.materials.append(WOOD_DARK)

# Keyboard surface (slanted)
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0.45, 1.05))
kb = bpy.context.active_object; kb.name="KeyboardSurface"; kb.scale=(2.0, 0.5, 0.06); kb.data.materials.append(WOOD_RICH)
# Ivory keys (5 stops)
for i in range(13):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(-1.2 + i*0.2, 0.45, 1.10))
    k = bpy.context.active_object
    k.scale = (0.08, 0.30, 0.02)
    k.data.materials.append(IVORY if i%2==0 else WOOD_DARK)
    k.name=f"Key_{i}"

# Pipe rank wall — back of organ, 21 brass pipes in 3 rows of 7
for row in range(3):
    for col in range(7):
        x = -2.2 + col*0.65
        h = 2.0 + (3-row)*0.6 - abs(col-3)*0.2
        z = 1.0 + h/2
        r = 0.10 + row*0.02
        bpy.ops.mesh.primitive_cylinder_add(radius=r, depth=h, location=(x, -0.8, z))
        p = bpy.context.active_object
        p.name = f"Pipe_{row}_{col}"
        # Mix brass and dull pipes for visual variety
        p.data.materials.append(PIPE_BRASS if (row+col)%2==0 else PIPE_DULL)
        # Pipe cap
        bpy.ops.mesh.primitive_cone_add(radius1=r*1.05, radius2=r*0.5, depth=0.15, location=(x, -0.8, z+h/2+0.07))
        bpy.context.active_object.name = f"PipeCap_{row}_{col}"
        bpy.context.active_object.data.materials.append(PIPE_BRASS)

# Pedalboard (foot pedals)
for i in range(9):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(-1.6 + i*0.4, 0.7, 0.10))
    bpy.context.active_object.scale = (0.15, 0.5, 0.05)
    bpy.context.active_object.data.materials.append(WOOD_DARK)
    bpy.context.active_object.name=f"Pedal_{i}"

# Ornate top cresting
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, -0.8, 4.1))
crest = bpy.context.active_object; crest.name="Crest"; crest.scale=(3.0, 0.4, 0.4); crest.data.materials.append(WOOD_DARK)

bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.join()
bpy.context.active_object.name = "PipeOrganCathedral"
export_fbx("PipeOrganCathedral")
print("[TARTARIA] PipeOrganCathedral done.")
