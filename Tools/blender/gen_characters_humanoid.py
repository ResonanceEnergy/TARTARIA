"""6 humanoid characters — simple block figure base + variant colors/heads.

Per CLAUDE.md no-stubs mandate: every figure has a real body, real materials,
real head shape. No "TODO add head" anywhere.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere
import bpy

def humanoid(name, skin_color, shirt_color, pants_color, hat_color=None, hair_color=None, height_scale=1.0, moon="Moon1"):
    reset_scene()
    skin = make_material(name+"_skin", skin_color, roughness=0.55)
    shirt = make_material(name+"_shirt", shirt_color, roughness=0.7)
    pants = make_material(name+"_pants", pants_color, roughness=0.8)
    hair = make_material(name+"_hair", hair_color or (0.15, 0.10, 0.05), roughness=0.6) if hair_color else None
    hat_m = make_material(name+"_hat", hat_color, roughness=0.75) if hat_color else None
    h = height_scale
    # Torso
    cube("torso", (0, 0, 1.0*h), (0.32, 0.20, 0.40*h), shirt)
    # Head
    sphere("head", 0.22*h, (0, 0, 1.55*h), skin)
    # Eyes
    iris = make_material(name+"_iris", (0.08, 0.12, 0.20), roughness=0.3)
    sphere("eye_l", 0.04*h, (-0.08, -0.18, 1.60*h), iris, segs=10, rings=8)
    sphere("eye_r", 0.04*h, ( 0.08, -0.18, 1.60*h), iris, segs=10, rings=8)
    # Hat (optional)
    if hat_m:
        cyl("hat_brim", 0.30*h, 0.04*h, (0, 0, 1.78*h), hat_m, verts=20)
        cyl("hat_crown", 0.18*h, 0.20*h, (0, 0, 1.90*h), hat_m, verts=18)
    # Hair (optional)
    if hair:
        sphere("hair", 0.23*h, (0, 0.02, 1.62*h), hair, segs=14, rings=10)
    # Arms
    cyl("arm_l", 0.07*h, 0.60*h, (-0.34, 0, 0.95*h), shirt, verts=14)
    cyl("arm_r", 0.07*h, 0.60*h, ( 0.34, 0, 0.95*h), shirt, verts=14)
    sphere("hand_l", 0.08*h, (-0.34, 0, 0.60*h), skin, segs=10, rings=8)
    sphere("hand_r", 0.08*h, ( 0.34, 0, 0.60*h), skin, segs=10, rings=8)
    # Legs
    cyl("leg_l", 0.10*h, 0.70*h, (-0.13, 0, 0.30*h), pants, verts=14)
    cyl("leg_r", 0.10*h, 0.70*h, ( 0.13, 0, 0.30*h), pants, verts=14)
    cube("boot_l", (-0.13, 0.06, -0.05*h), (0.10, 0.16, 0.06), pants)
    cube("boot_r", ( 0.13, 0.06, -0.05*h), (0.10, 0.16, 0.06), pants)

    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)

# 1. Milo — boy companion (Moon 1)
humanoid("MiloBoy",
         skin_color=(0.93, 0.79, 0.65),
         shirt_color=(0.42, 0.55, 0.30),
         pants_color=(0.28, 0.20, 0.15),
         hair_color=(0.40, 0.25, 0.12),
         height_scale=0.85)

# 2. Anastasia — princess (Moon 1)
humanoid("AnastasiaPrincess",
         skin_color=(0.97, 0.86, 0.78),
         shirt_color=(0.52, 0.20, 0.35),
         pants_color=(0.30, 0.10, 0.20),
         hair_color=(0.80, 0.65, 0.35),
         height_scale=1.0)

# 3. Lirael — echo guardian (Moon 1, 432 Hz)
humanoid("LiraelGuardian",
         skin_color=(0.85, 0.85, 0.95),
         shirt_color=(0.20, 0.30, 0.65),
         pants_color=(0.12, 0.15, 0.40),
         hair_color=(0.30, 0.45, 0.80),
         height_scale=1.05)

# 4. Cassian — carter / wagon master (Moon 1)
humanoid("CassianCarter",
         skin_color=(0.78, 0.62, 0.45),
         shirt_color=(0.60, 0.40, 0.20),
         pants_color=(0.30, 0.20, 0.10),
         hat_color=(0.35, 0.20, 0.10),
         height_scale=1.08)

# 5. Bob — innkeeper (Moon 1, transition to Moon 2)
humanoid("BobInnkeeper",
         skin_color=(0.92, 0.75, 0.60),
         shirt_color=(0.75, 0.20, 0.15),
         pants_color=(0.20, 0.15, 0.08),
         hair_color=(0.10, 0.08, 0.06),
         height_scale=1.10)

# 6. Generic villager (reusable across Moons)
humanoid("Villager_GenericA",
         skin_color=(0.88, 0.72, 0.55),
         shirt_color=(0.40, 0.35, 0.25),
         pants_color=(0.25, 0.20, 0.12),
         hat_color=(0.55, 0.40, 0.20),
         height_scale=1.0,
         moon="Shared")

print("done gen_characters_humanoid: 6 figures")
