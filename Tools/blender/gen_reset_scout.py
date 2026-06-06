"""Reset Scout — Moon 1 §11 antagonist (Victorian-coated patrol enemy).

Per docs/15_MVP_BUILD_SPEC.md §11: paranormal-archive agent in Victorian
greatcoat + top hat with a mechanical eye that glows when they detect the
player. ~1.7m tall, slim humanoid.

Exports to Assets/_Project/Models/Blender/Moon1/ResetScout.fbx so the
Moon 1 enemy spawner can pick it up alongside MudGolem.fbx.

There is an older Shared/ResetScout export inside gen_characters_enemies.py
(line ~33). That one is kept for now but uses Shared moon path; Moon 1
prefab wiring resolves Moon1 first then falls back. This file is the
canonical Moon 1 ResetScout source.
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import (
    reset_scene,
    make_material,
    export_current_as,
    cube,
    cyl,
    sphere,
    cone,
)
import bpy


reset_scene()

# Materials — Victorian black wool coat, brass mechanical eye, copper hat band.
coat = make_material(
    "M_ResetScout_Coat",
    base_color=(0.15, 0.10, 0.15),
    roughness=0.40,
    metallic=0.0,
)
skin = make_material(
    "M_ResetScout_Skin",
    base_color=(0.78, 0.70, 0.62),
    roughness=0.55,
)
hat_felt = make_material(
    "M_ResetScout_HatFelt",
    base_color=(0.06, 0.06, 0.08),
    roughness=0.80,
)
hat_band = make_material(
    "M_ResetScout_HatBand",
    base_color=(0.55, 0.35, 0.18),
    roughness=0.40,
    metallic=0.7,
)
brass = make_material(
    "M_ResetScout_Brass",
    base_color=(0.80, 0.62, 0.28),
    roughness=0.35,
    metallic=0.80,
)
eye_glow = make_material(
    "M_ResetScout_EyeGlow",
    base_color=(1.0, 0.20, 0.20),
    roughness=0.20,
    metallic=0.0,
    emission=(1.0, 0.20, 0.20),
    emission_strength=3.0,
)
boot = make_material(
    "M_ResetScout_Boot",
    base_color=(0.08, 0.06, 0.06),
    roughness=0.55,
    metallic=0.10,
)

# Body — slim torso (Victorian frock coat silhouette, narrow shoulders).
# Total target height ~1.7m: legs 0-0.85, torso 0.85-1.45, head 1.45-1.70,
# hat brim 1.70-1.74, hat crown 1.74-1.95 (final bb 1.7m at hair, hat above).
cube("torso", (0, 0, 1.10), (0.26, 0.18, 0.50), coat)
# Coat tails — slightly flared past torso bottom
cube("coat_tail", (0, 0, 0.78), (0.30, 0.20, 0.18), coat)

# Neck + head — head centered at 1.52m so eye line lands ~1.55m (face mid).
cyl("neck", 0.07, 0.08, (0, 0, 1.39), skin, verts=12)
sphere("head", 0.13, (0, 0, 1.52), skin, segs=14, rings=10)

# Mechanical eye (right) — brass housing + red emission lens.
cyl("eye_housing", 0.045, 0.04, (0.06, -0.10, 1.54), brass,
    rot=(1.5708, 0, 0), verts=14)
sphere("eye_lens", 0.030, (0.06, -0.12, 1.54), eye_glow, segs=10, rings=8)

# Organic eye (left) — small dark sphere.
sphere("eye_organic", 0.018,
       (-0.05, -0.12, 1.55),
       make_material("M_ResetScout_EyeOrganic", (0.08, 0.08, 0.10), roughness=0.20),
       segs=8, rings=6)

# Top hat — brim disc + tall crown.
cyl("hat_brim", 0.18, 0.025, (0, 0, 1.66), hat_felt, verts=22)
cyl("hat_crown", 0.115, 0.18, (0, 0, 1.77), hat_felt, verts=20)
# Hat band (copper)
cyl("hat_band", 0.117, 0.025, (0, 0, 1.69), hat_band, verts=22)

# Arms — coat sleeves down to wrists ~0.7m below shoulder.
cyl("arm_l", 0.055, 0.62, (-0.21, 0, 1.05), coat, verts=12)
cyl("arm_r", 0.055, 0.62, (0.21, 0, 1.05), coat, verts=12)
# Gloved hands
sphere("hand_l", 0.06, (-0.21, 0.01, 0.71),
       make_material("M_ResetScout_Glove", (0.06, 0.06, 0.08), roughness=0.65),
       segs=10, rings=8)
sphere("hand_r", 0.06, (0.21, 0.01, 0.71),
       make_material("M_ResetScout_Glove2", (0.06, 0.06, 0.08), roughness=0.65),
       segs=10, rings=8)

# Legs — narrow trousers.
cyl("leg_l", 0.075, 0.78, (-0.08, 0, 0.39), coat, verts=12)
cyl("leg_r", 0.075, 0.78, (0.08, 0, 0.39), coat, verts=12)

# Boots — short cylindrical Victorian boots.
cyl("boot_l", 0.085, 0.10, (-0.08, 0.01, 0.05), boot, verts=14)
cyl("boot_r", 0.085, 0.10, (0.08, 0.01, 0.05), boot, verts=14)

# Coat collar — small lapel suggestion.
cube("collar", (0, -0.08, 1.36), (0.18, 0.04, 0.05), coat)

# Pocket watch chain hint (a small brass dot at hip)
sphere("watch_chain", 0.015, (-0.10, -0.10, 1.00), brass, segs=8, rings=6)

# Walking cane (thin cylinder in right hand, ~1m long, slightly angled).
cyl("cane", 0.012, 0.95,
    (0.30, 0.05, 0.50),
    make_material("M_ResetScout_Cane", (0.20, 0.12, 0.06), roughness=0.45, metallic=0.20),
    rot=(0.10, 0, 0), verts=10)
sphere("cane_grip", 0.022, (0.30, 0.05, 0.97), brass, segs=10, rings=6)

export_current_as("ResetScout", "Moon1")
print("[TARTARIA] ResetScout — Moon 1 antagonist, exported to Moon1/ResetScout.fbx")
