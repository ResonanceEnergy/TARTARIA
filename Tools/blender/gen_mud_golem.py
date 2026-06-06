"""gen_mud_golem.py — Author the MudGolem real Blender mesh for Moon 1.

Per docs/15 §11 Mud Golem is a 2.5m tall clay/mud humanoid enemy (Moon 1 wave 1).
This is the standalone authoring script. The same composition is also embedded in
gen_characters_enemies.py (entry #1) but that variant exports to "Shared/" and is
sized ~1.85m (default humanoid). This standalone variant ships the FULL 2.5m
spec scale plus a lumpy / asymmetric "mud" silhouette for visual readability.

Output: Assets/_Project/Models/Blender/Moon1/MudGolem.fbx (Kaydara 7400 binary).
Final scale target: max axis ~2.5m, width ~1.2m, depth ~0.8m.

Run:
    blender --background --python tools/blender/gen_mud_golem.py
or from Unity:
    Tartaria → Moon 1 → Run Blender Batch
or interactively (open _common.py path detection finds the repo root).
"""
import os
import sys
import math
import random

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import (
    reset_scene,
    make_material,
    export_current_as,
    cube,
    cyl,
    sphere,
)
import bpy

# ---------------------------------------------------------------------------
# Deterministic "lumpiness" — same script run twice produces same FBX.
random.seed(2026_06_04)

reset_scene()

# Materials — clay/mud body + glowing core eyes for silhouette read.
mud = make_material(
    "M_MudGolem",
    base_color=(0.35, 0.20, 0.10),
    roughness=0.85,
    metallic=0.0,
)
core = make_material(
    "M_MudGolem_Core",
    base_color=(0.55, 0.30, 0.15),
    roughness=0.80,
    metallic=0.0,
    emission=(0.30, 0.15, 0.05),
    emission_strength=0.6,
)


def jitter(base, amp=0.12):
    """Asymmetric lumpiness factor — multiplier in [1-amp, 1+amp]."""
    return base * (1.0 + random.uniform(-amp, amp))


# ---------------------------------------------------------------------------
# Build a 2.5m-tall mud humanoid out of primitives.
#
# Vertical budget (z, world-space, base on ground at z=0):
#   legs:    z 0.05 .. 1.00  (depth 0.95)
#   torso:   z 1.00 .. 1.95  (cube, depth 0.95)
#   neck:    z 1.95 .. 2.10
#   head:    z 2.05 .. 2.50  (sphere r=0.225, center 2.275)
# Width budget (x, world-space):
#   torso width ~0.55 (half-extent at shoulders),
#   arms outset to x=±0.65,
#   overall bbox half-width ~0.65 → total ~1.30 width (spec ~1.2m).
# Depth budget (y, world-space):
#   torso depth half ~0.40, total ~0.80 (spec target).

# --- TORSO — cube, lumpy ---
torso = cube(
    "torso",
    loc=(0.0, 0.0, 1.45),
    scale=(jitter(0.55), jitter(0.40), jitter(0.95)),
    mat=mud,
)

# --- SHOULDER LUMPS — spheres on top of torso ---
sphere("shoulder_l", r=jitter(0.22), loc=(-0.50, 0.0, 1.90), mat=mud, segs=14, rings=10)
sphere("shoulder_r", r=jitter(0.22), loc=( 0.50, 0.0, 1.90), mat=mud, segs=14, rings=10)

# --- NECK — short cylinder ---
cyl("neck", r=0.13, d=0.18, loc=(0.0, 0.0, 2.02), mat=mud, verts=14)

# --- HEAD — sphere, lumpy + asymmetric ---
sphere(
    "head",
    r=jitter(0.225),
    loc=(0.0, 0.0, 2.30),
    mat=mud,
    segs=16,
    rings=12,
)
# Brow ridge — slight asymmetric chunk for "mud" feel.
sphere("brow_ridge", r=0.12, loc=(0.02, -0.18, 2.36), mat=mud, segs=10, rings=8)

# --- EYES — glowing core spheres for silhouette read ---
sphere("eye_l", r=0.05, loc=(-0.10, -0.20, 2.32), mat=core, segs=10, rings=8)
sphere("eye_r", r=0.05, loc=( 0.10, -0.20, 2.32), mat=core, segs=10, rings=8)

# --- ARMS — two cylinders, asymmetric scale per limb ---
#   Upper arm slung from shoulder down to fist height (~0.70).
arm_l_len = jitter(1.10, amp=0.08)
arm_r_len = jitter(1.10, amp=0.08)
cyl(
    "arm_l",
    r=jitter(0.16, amp=0.10),
    d=arm_l_len,
    loc=(-0.62, 0.0, 1.40),
    mat=mud,
    verts=14,
)
cyl(
    "arm_r",
    r=jitter(0.16, amp=0.10),
    d=arm_r_len,
    loc=( 0.62, 0.0, 1.38),  # slightly lower for asymmetry
    mat=mud,
    verts=14,
)

# --- FISTS — big lumpy spheres for melee silhouette ---
sphere("fist_l", r=jitter(0.26, amp=0.08), loc=(-0.62, 0.0, 0.78), mat=mud, segs=12, rings=10)
sphere("fist_r", r=jitter(0.26, amp=0.08), loc=( 0.62, 0.0, 0.76), mat=mud, segs=12, rings=10)

# --- LEGS — two cylinders, slightly different lengths for "limp" gait read ---
leg_l_len = jitter(0.95, amp=0.06)
leg_r_len = jitter(0.95, amp=0.06)
cyl(
    "leg_l",
    r=jitter(0.20, amp=0.08),
    d=leg_l_len,
    loc=(-0.22, 0.0, 0.50),
    mat=mud,
    verts=14,
)
cyl(
    "leg_r",
    r=jitter(0.20, amp=0.08),
    d=leg_r_len,
    loc=( 0.22, 0.0, 0.50),
    mat=mud,
    verts=14,
)

# --- FEET — sphere clods at leg base ---
sphere("foot_l", r=0.18, loc=(-0.22, 0.05, 0.10), mat=mud, segs=12, rings=10)
sphere("foot_r", r=0.18, loc=( 0.22, 0.05, 0.10), mat=mud, segs=12, rings=10)

# --- EXTRA MUD CLODS on torso for "wet clay" silhouette ---
sphere("clod_chest", r=0.18, loc=(0.0, -0.32, 1.55), mat=mud, segs=12, rings=10)
sphere("clod_back", r=0.16, loc=(0.0,  0.34, 1.60), mat=mud, segs=12, rings=10)
sphere("clod_hip_l", r=0.14, loc=(-0.30, 0.0, 1.05), mat=mud, segs=10, rings=8)
sphere("clod_hip_r", r=0.14, loc=( 0.30, 0.0, 1.05), mat=mud, segs=10, rings=8)

# ---------------------------------------------------------------------------
# Finalize — join, apply transforms, export to Moon1/.
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.join()
bpy.context.active_object.name = "MudGolem"
out_path = export_current_as("MudGolem", "Moon1")
print("[TARTARIA] gen_mud_golem.py — MudGolem exported to %s" % out_path)
