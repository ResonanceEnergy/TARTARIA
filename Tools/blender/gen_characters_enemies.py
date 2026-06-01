"""8 enemies — Mud Golem, Reset Scout, Dissonance Crystal, Crystal Sentry,
Resonance Drone, Shadow Stalker, Hollow Knight, Cathedral Choir Spirit.

Per CLAUDE.md no-stubs mandate — every enemy has real geometry + URP-safe materials.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import reset_scene, make_material, export_current_as, cube, cyl, sphere, cone, torus
import bpy

def finalize(name, moon="Shared"):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.join()
    bpy.context.active_object.name = name
    export_current_as(name, moon)

# 1. Mud Golem — Moon 1 sluggish enemy (mud-covered humanoid)
reset_scene()
mud = make_material("MudGolem_mud", (0.32, 0.22, 0.14), roughness=0.95)
core = make_material("MudGolem_core", (0.45, 0.30, 0.20), roughness=0.85, emission=(0.22, 0.12, 0.05), emission_strength=0.4)
sphere("torso", 0.55, (0, 0, 1.0), mud, segs=14, rings=12)
sphere("head", 0.32, (0, 0, 1.65), mud, segs=14, rings=10)
sphere("eye_l", 0.07, (-0.12, -0.28, 1.70), core, segs=10, rings=8)
sphere("eye_r", 0.07, ( 0.12, -0.28, 1.70), core, segs=10, rings=8)
cyl("arm_l", 0.16, 0.85, (-0.62, 0, 0.95), mud, verts=14)
cyl("arm_r", 0.16, 0.85, ( 0.62, 0, 0.95), mud, verts=14)
sphere("fist_l", 0.22, (-0.62, 0, 0.50), mud, segs=12, rings=10)
sphere("fist_r", 0.22, ( 0.62, 0, 0.50), mud, segs=12, rings=10)
cyl("leg_l", 0.20, 0.85, (-0.20, 0, 0.40), mud, verts=14)
cyl("leg_r", 0.20, 0.85, ( 0.20, 0, 0.40), mud, verts=14)
finalize("MudGolem", "Shared")

# 2. Reset Scout — Victorian-costumed paranormal agent
reset_scene()
coat = make_material("ResetScout_coat", (0.10, 0.10, 0.12), roughness=0.65)
skin2 = make_material("ResetScout_skin", (0.85, 0.78, 0.72), roughness=0.55)
goggle = make_material("ResetScout_goggle", (0.40, 0.30, 0.10), roughness=0.30, metallic=0.6, emission=(0.80, 0.30, 0.10), emission_strength=0.5)
hat = make_material("ResetScout_hat", (0.06, 0.06, 0.08), roughness=0.80)
# Tall Victorian figure
cube("torso", (0, 0, 1.1), (0.28, 0.18, 0.50), coat)
cyl("neck", 0.10, 0.10, (0, 0, 1.65), skin2, verts=12)
sphere("head", 0.20, (0, 0, 1.80), skin2, segs=14, rings=10)
# Goggles
cyl("goggle_l", 0.07, 0.05, (-0.08, -0.20, 1.85), goggle, rot=(1.5708, 0, 0), verts=18)
cyl("goggle_r", 0.07, 0.05, ( 0.08, -0.20, 1.85), goggle, rot=(1.5708, 0, 0), verts=18)
# Top hat
cyl("hat_brim", 0.28, 0.04, (0, 0, 2.02), hat, verts=20)
cyl("hat_crown", 0.18, 0.30, (0, 0, 2.20), hat, verts=18)
# Arms — long coat sleeves
cyl("arm_l", 0.08, 0.75, (-0.32, 0, 1.0), coat, verts=12)
cyl("arm_r", 0.08, 0.75, ( 0.32, 0, 1.0), coat, verts=12)
# Legs
cyl("leg_l", 0.10, 0.85, (-0.12, 0, 0.30), coat, verts=12)
cyl("leg_r", 0.10, 0.85, ( 0.12, 0, 0.30), coat, verts=12)
finalize("ResetScout", "Shared")

# 3. Dissonance Crystal — Moon 2 floating shard formation
reset_scene()
dis = make_material("DC_crystal", (0.40, 0.10, 0.50), roughness=0.20, metallic=0.3,
                    emission=(0.80, 0.20, 0.90), emission_strength=2.0)
# Central shard
cone("core", 0.40, 0.10, 1.40, (0, 0, 0.7), dis, verts=8)
# 6 orbital shards
import math
for i in range(6):
    a = i * (2*math.pi/6)
    cone(f"shard_{i}", 0.18, 0.04, 0.80, (math.cos(a)*0.6, math.sin(a)*0.6, 0.6),
         dis, rot=(0, 0, a), verts=6)
finalize("DissonanceCrystal", "Moon2")

# 4. Crystal Sentry — Moon 2 standing crystal guardian
reset_scene()
cs = make_material("CS_body", (0.20, 0.55, 0.80), roughness=0.30, metallic=0.4,
                   emission=(0.30, 0.70, 1.0), emission_strength=1.5)
cyl("base", 0.40, 0.10, (0, 0, 0.05), cs, verts=10)
cone("torso", 0.32, 0.18, 1.20, (0, 0, 0.7), cs, verts=8)
sphere("head", 0.25, (0, 0, 1.55), cs, segs=12, rings=10)
# 4 angular limbs
cone("arm_l", 0.10, 0.04, 0.90, (-0.40, 0, 1.0), cs, rot=(0, 0.4, 0), verts=6)
cone("arm_r", 0.10, 0.04, 0.90, ( 0.40, 0, 1.0), cs, rot=(0, -0.4, 0), verts=6)
finalize("CrystalSentry", "Moon2")

# 5. Resonance Drone — Moon 3+ small flying enemy
reset_scene()
drone = make_material("RD_body", (0.30, 0.30, 0.35), roughness=0.40, metallic=0.5,
                      emission=(0.20, 0.60, 0.40), emission_strength=1.2)
sphere("body", 0.30, (0, 0, 1.0), drone, segs=14, rings=12)
torus("ring1", 0.45, 0.04, (0, 0, 1.0), drone, mseg=24, miseg=8)
torus("ring2", 0.40, 0.04, (0, 0, 1.0), drone, mseg=24, miseg=8, rot=(1.5708, 0, 0))
# 3 emitters
for i in range(3):
    a = i * (2*math.pi/3)
    sphere(f"emit_{i}", 0.06, (math.cos(a)*0.45, math.sin(a)*0.45, 1.0), drone, segs=8, rings=6)
finalize("ResonanceDrone", "Shared")

# 6. Shadow Stalker — Moon 4 stealthy enemy
reset_scene()
shad = make_material("SS_body", (0.04, 0.04, 0.08), roughness=0.95)
glow = make_material("SS_eyes", (0.0, 0.0, 0.0), roughness=0.20,
                     emission=(0.90, 0.10, 0.10), emission_strength=2.5)
# Tall thin figure
cyl("torso", 0.20, 1.50, (0, 0, 1.0), shad, verts=10)
sphere("head", 0.18, (0, 0, 1.85), shad, segs=12, rings=10)
sphere("eye_l", 0.04, (-0.07, -0.16, 1.88), glow, segs=8, rings=6)
sphere("eye_r", 0.04, ( 0.07, -0.16, 1.88), glow, segs=8, rings=6)
# Long arms with claws
cyl("arm_l", 0.05, 1.10, (-0.30, 0, 1.0), shad, verts=10)
cyl("arm_r", 0.05, 1.10, ( 0.30, 0, 1.0), shad, verts=10)
cone("claw_l", 0.08, 0.01, 0.20, (-0.30, 0, 0.35), shad, verts=6)
cone("claw_r", 0.08, 0.01, 0.20, ( 0.30, 0, 0.35), shad, verts=6)
finalize("ShadowStalker", "Shared")

# 7. Hollow Knight — Moon 5 armored undead
reset_scene()
armor = make_material("HK_armor", (0.30, 0.28, 0.25), roughness=0.45, metallic=0.7)
void = make_material("HK_void", (0.0, 0.0, 0.0), roughness=1.0,
                     emission=(0.20, 0.30, 0.60), emission_strength=1.2)
# Bulky armored torso
cube("torso", (0, 0, 1.1), (0.42, 0.28, 0.50), armor)
# Helmet
sphere("helmet", 0.28, (0, 0, 1.70), armor, segs=14, rings=12)
sphere("eye_l", 0.05, (-0.10, -0.22, 1.72), void, segs=8, rings=6)
sphere("eye_r", 0.05, ( 0.10, -0.22, 1.72), void, segs=8, rings=6)
# Pauldrons
sphere("pauldron_l", 0.18, (-0.42, 0, 1.35), armor, segs=12, rings=10)
sphere("pauldron_r", 0.18, ( 0.42, 0, 1.35), armor, segs=12, rings=10)
# Arms
cyl("arm_l", 0.12, 0.80, (-0.45, 0, 0.95), armor, verts=12)
cyl("arm_r", 0.12, 0.80, ( 0.45, 0, 0.95), armor, verts=12)
# Legs
cube("leg_l", (-0.15, 0, 0.30), (0.14, 0.14, 0.45), armor)
cube("leg_r", ( 0.15, 0, 0.30), (0.14, 0.14, 0.45), armor)
# Sword
cube("sword_blade", (0.65, 0, 1.0), (0.04, 0.04, 0.60), armor)
cyl("sword_hilt", 0.06, 0.20, (0.65, 0, 0.42), armor, verts=10)
finalize("HollowKnight", "Shared")

# 8. Cathedral Choir Spirit — Moon 1 ambient phantasm
reset_scene()
spirit = make_material("CCS_body", (0.85, 0.92, 1.0), roughness=0.20,
                       emission=(0.95, 0.95, 0.80), emission_strength=1.8)
# Robed phantom
cone("robe", 0.40, 0.15, 1.40, (0, 0, 0.7), spirit, verts=12)
sphere("head", 0.20, (0, 0, 1.55), spirit, segs=14, rings=10)
# 2 ghostly arms
cone("arm_l", 0.08, 0.02, 0.50, (-0.30, 0, 1.10), spirit, rot=(0, 0.3, 0), verts=6)
cone("arm_r", 0.08, 0.02, 0.50, ( 0.30, 0, 1.10), spirit, rot=(0, -0.3, 0), verts=6)
# Halo
torus("halo", 0.25, 0.025, (0, 0, 1.80), spirit, mseg=18, miseg=6)
finalize("CathedralChoirSpirit", "Moon1")

print("done gen_characters_enemies: 8 enemies")
