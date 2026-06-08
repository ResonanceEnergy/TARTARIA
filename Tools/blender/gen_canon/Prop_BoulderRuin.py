"""
Boulder Ruin — fractured Tartarian stone block per docs/15 environment.

Large irregular boulder (2.5m wide) with gold-carved sigil on flat face.
Scattered as terrain landmarks.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, uv_orb, ico_orb, join_character,
    make_character_mat, make_aether_emissive, save_and_export,
    AETHER_GOLD,
)
import bpy

def main():
    reset_scene()
    parts = []
    # Main body — large ico sphere
    body = ico_orb("BoulderBody", (0, 0.7, 0), 1.0)
    body.scale = (1.0, 0.8, 1.1)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    parts.append(body)
    # Smaller adjacent bulges (irregular shape)
    bul1 = ico_orb("Bulge1", (0.6, 0.5, -0.3), 0.5)
    parts.append(bul1)
    bul2 = ico_orb("Bulge2", (-0.5, 0.6, 0.2), 0.4)
    parts.append(bul2)
    # Sigil on flat face — gold crescent
    sigil = cube_at("Sigil", (0, 1.0, 1.0), (0.40, 0.05, 0.02))
    parts.append(sigil)
    sigil_orb = uv_orb("SigilOrb", (0, 1.0, 1.05), 0.06)
    parts.append(sigil_orb)
    mat_stone = make_character_mat("Boulder_Stone", (0.38, 0.35, 0.32, 1.0), roughness=0.95)
    mat_sigil = make_aether_emissive("Boulder_Sigil", AETHER_GOLD, 3.0)
    for p in parts:
        if p.name == "Sigil" or p.name == "SigilOrb":
            p.data.materials.append(mat_sigil)
        else:
            p.data.materials.append(mat_stone)
    o = join_character(parts, "Prop_BoulderRuin")
    save_and_export(o, "Prop_BoulderRuin")

if __name__ == "__main__":
    main()
