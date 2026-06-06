"""Humanoid characters - simple block figure base + variant colors/heads.

Per CLAUDE.md no-stubs mandate: every figure has a real body, real materials,
real head shape. No "TODO add head" anywhere.

Scale calibration (2026-06-04 root-cause fix):
  Visible height of model = 1.82 * height_scale + 0.03    (no hat)
  Visible height of model = 2.05 * height_scale + 0.03    (with hat)

  Math: boot bottom (-0.05h - 0.03) to head top (1.55h + 0.22h)
  or hat crown top (1.90h + 0.10h). Set height_scale so visible height
  matches the docs/15 section 10 spec.

  Anastasia (1.7m adult)   -> h = 0.918
  Lirael    (1.8m adult)   -> h = 0.973
  Cassian   (1.8m w/ hat)  -> h = 0.864   (hat brings him to 1.80m at hat top)
  Bob       (1.75m adult)  -> h = 0.945
  Villager  (1.75m w/ hat) -> h = 0.839

Milo is NOT a humanoid - see tools/blender/gen_milo_fox.py for the
fox-like quadruped companion (docs/15 section 10: 40cm Tartarian fox).

Why the rescale was needed: the prior values (h = 1.30 - 1.758) were tuned
against a Blender->Unity FBX export bug where unapplied ob.scale on cube
parts inflated bounds. _common.export_current_as() now applies all
transforms before export, so geometry-baked scales match Unity bounds 1:1.
"""
import os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _common import (reset_scene, make_material, export_current_as, cube, cyl, sphere,
                     make_humanoid_armature, join_meshes_then_bind,
                     join_meshes_then_bind_with_overrides, tag_part_for_override)
import bpy


# Stage A NPC armature pipeline (2026-06-04): selectively rig the Moon 1
# main NPCs (Anastasia / Lirael / Cassian / Bob) and leave generic
# villagers as static joined meshes. Generic villagers don't need joint
# deformation - they're crowd-fill.
RIG_TARGETS = {"AnastasiaPrincess", "LiraelGuardian", "CassianCarter", "BobInnkeeper"}

# Visible height per RIG_TARGETS entry, in meters. Bone proportions in the
# humanoid armature reference this directly, NOT the bpy h-scale factor.
RIG_TARGET_HEIGHTS = {
    "AnastasiaPrincess": 1.70,
    "LiraelGuardian":    1.80,
    "CassianCarter":     1.80,  # hat sits on top; body itself ~1.77
    "BobInnkeeper":      1.75,
}


def humanoid(name, skin_color, shirt_color, pants_color, hat_color=None, hair_color=None, height_scale=0.945, moon="Moon1"):
    reset_scene()
    skin = make_material(name + "_skin", skin_color, roughness=0.55)
    shirt = make_material(name + "_shirt", shirt_color, roughness=0.7)
    pants = make_material(name + "_pants", pants_color, roughness=0.8)
    hair = make_material(name + "_hair", hair_color or (0.15, 0.10, 0.05), roughness=0.6) if hair_color else None
    hat_m = make_material(name + "_hat", hat_color, roughness=0.75) if hat_color else None
    h = height_scale
    # Torso
    cube("torso", (0, 0, 1.0 * h), (0.32, 0.20, 0.40 * h), shirt)
    # Head
    sphere("head", 0.22 * h, (0, 0, 1.55 * h), skin)
    # Eyes
    iris = make_material(name + "_iris", (0.08, 0.12, 0.20), roughness=0.3)
    sphere("eye_l", 0.04 * h, (-0.08, -0.18, 1.60 * h), iris, segs=10, rings=8)
    sphere("eye_r", 0.04 * h, ( 0.08, -0.18, 1.60 * h), iris, segs=10, rings=8)
    # Hat (optional)
    if hat_m:
        cyl("hat_brim", 0.30 * h, 0.04 * h, (0, 0, 1.78 * h), hat_m, verts=20)
        cyl("hat_crown", 0.18 * h, 0.20 * h, (0, 0, 1.90 * h), hat_m, verts=18)
    # Hair (optional)
    hair_obj = None
    if hair:
        hair_obj = sphere("hair", 0.23 * h, (0, 0.02, 1.62 * h), hair, segs=14, rings=10)
        # Stage B (2026-06-04): hair sphere extends past head crown; force to Head bone.
        tag_part_for_override(hair_obj, "HairSphere")
    # Arms
    cyl("arm_l", 0.07 * h, 0.60 * h, (-0.34, 0, 0.95 * h), shirt, verts=14)
    cyl("arm_r", 0.07 * h, 0.60 * h, ( 0.34, 0, 0.95 * h), shirt, verts=14)
    sphere("hand_l", 0.08 * h, (-0.34, 0, 0.60 * h), skin, segs=10, rings=8)
    sphere("hand_r", 0.08 * h, ( 0.34, 0, 0.60 * h), skin, segs=10, rings=8)
    # Legs
    cyl("leg_l", 0.10 * h, 0.70 * h, (-0.13, 0, 0.30 * h), pants, verts=14)
    cyl("leg_r", 0.10 * h, 0.70 * h, ( 0.13, 0, 0.30 * h), pants, verts=14)
    cube("boot_l", (-0.13, 0.06, -0.05 * h), (0.10, 0.16, 0.06), pants)
    cube("boot_r", ( 0.13, 0.06, -0.05 * h), (0.10, 0.16, 0.06), pants)

    # Stage B NPC armature pipeline (2026-06-04): if this character is in
    # RIG_TARGETS, build a T-pose humanoid armature with accessory overrides.
    # Hair-sphere parts get forced to the Head bone (heat-map would otherwise
    # split it across Neck/UpperChest depending on geometry overlap).
    if name in RIG_TARGETS:
        target_h = RIG_TARGET_HEIGHTS.get(name, 1.75)
        arm = make_humanoid_armature(name, height=target_h)
        overrides = {}
        if hair_obj is not None:
            overrides["HairSphere"] = "Head"
        if overrides:
            join_meshes_then_bind_with_overrides(name, arm, overrides=overrides)
        else:
            join_meshes_then_bind(name, arm)
    # NOTE: do NOT join/rename here - export_current_as() now applies
    # transforms, joins, and exports in one step (see _common.py).
    export_current_as(name, moon)


# NOTE: Milo is authored separately as a fox-like quadruped in gen_milo_fox.py
# (docs/15 section 10). The MiloBoy humanoid entry was retired 2026-06-04
# because it never matched the canonical fox spec.

# 1. Anastasia - herb-keeper / village adult (Moon 1)
# Target 1.70m -> h = (1.70 - 0.03) / 1.82 = 0.918
humanoid("AnastasiaPrincess",
         skin_color=(0.97, 0.86, 0.78),
         shirt_color=(0.52, 0.20, 0.35),
         pants_color=(0.30, 0.10, 0.20),
         hair_color=(0.80, 0.65, 0.35),
         height_scale=0.918)

# 2. Lirael - echo guardian (Moon 1, 432 Hz)
# Target 1.80m -> h = (1.80 - 0.03) / 1.82 = 0.973
humanoid("LiraelGuardian",
         skin_color=(0.85, 0.85, 0.95),
         shirt_color=(0.20, 0.30, 0.65),
         pants_color=(0.12, 0.15, 0.40),
         hair_color=(0.30, 0.45, 0.80),
         height_scale=0.973)

# 3. Cassian - carter / wagon master (Moon 1) - wears a hat
# Target 1.80m at hat top -> h = (1.80 - 0.03) / 2.05 = 0.864
humanoid("CassianCarter",
         skin_color=(0.78, 0.62, 0.45),
         shirt_color=(0.60, 0.40, 0.20),
         pants_color=(0.30, 0.20, 0.10),
         hat_color=(0.35, 0.20, 0.10),
         height_scale=0.864)

# 4. Bob - innkeeper (Moon 1, transition to Moon 2)
# Target 1.75m -> h = (1.75 - 0.03) / 1.82 = 0.945
humanoid("BobInnkeeper",
         skin_color=(0.92, 0.75, 0.60),
         shirt_color=(0.75, 0.20, 0.15),
         pants_color=(0.20, 0.15, 0.08),
         hair_color=(0.10, 0.08, 0.06),
         height_scale=0.945)

# 5. Generic villager - wears a hat
# Target 1.75m at hat top -> h = (1.75 - 0.03) / 2.05 = 0.839
humanoid("Villager_GenericA",
         skin_color=(0.88, 0.72, 0.55),
         shirt_color=(0.40, 0.35, 0.25),
         pants_color=(0.25, 0.20, 0.12),
         hat_color=(0.55, 0.40, 0.20),
         height_scale=0.839,
         moon="Shared")

print("done gen_characters_humanoid: 5 figures (Milo authored separately in gen_milo_fox.py)")
