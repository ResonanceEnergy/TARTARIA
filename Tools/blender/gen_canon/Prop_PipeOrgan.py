"""
Tartarian Pipe Organ — interior of the Dome (Listeners' Hall central instrument).

Per docs/15: the dome's central function. Player tunes this organ via mini-game.
Silhouette: 4m wide × 3m tall, 7 vertical pipes arranged in φ-ratio heights.
Gold seam emissive at base + at each pipe top.
"""
import os, sys, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from _lib_characters import (
    reset_scene, cube_at, cylinder_y, uv_orb, join_character,
    make_character_mat, make_aether_emissive,
    save_and_export,
    AETHER_GOLD, AETHER_CYAN,
)
import bpy


PHI = 1.618


def main():
    reset_scene()
    print("[Prop_PipeOrgan] Dome interior organ - 4m x 3m")

    parts = []

    # Base — wide stone platform
    base = cube_at("Base", (0, 0.15, 0), (4.0, 0.30, 1.2))
    parts.append(base)

    # Console / keyboard slab
    console = cube_at("Console", (0, 0.50, 0.55), (3.5, 0.20, 0.5))
    parts.append(console)

    # 7 pipes arranged in φ-ratio heights
    pipe_heights = [3.0, 2.7, 2.4, 2.2, 2.4, 2.7, 3.0]   # symmetric pyramid
    for i, h in enumerate(pipe_heights):
        x = -2.4 + i * 0.80
        pipe = cylinder_y(f"Pipe_{i}", (x, 0.30 + h / 2, -0.10), 0.30, h)
        parts.append(pipe)
        # Each pipe top gets a gold cap orb
        cap = uv_orb(f"PipeCap_{i}", (x, 0.30 + h + 0.12, -0.10), 0.18)
        parts.append(cap)

    # Bellows side housings - wide flat boxes flanking sides
    for sx in [-2.5, 2.5]:
        bel = cube_at(f"Bellows_{sx}", (sx, 1.4, -0.30), (0.5, 1.6, 0.7))
        parts.append(bel)

    # Resonance focus — central glowing cyan orb above console
    orb = uv_orb("ResonanceOrb", (0, 1.0, 0.85), 0.18)
    parts.append(orb)

    # Materials
    mat_base = make_character_mat("Organ_Stone", (0.50, 0.47, 0.42, 1.0), roughness=0.95)
    mat_pipe = make_character_mat("Organ_Pipe", (0.65, 0.55, 0.30, 1.0), roughness=0.4, metallic=0.7)  # brass
    mat_cap = make_aether_emissive("Organ_PipeCap", AETHER_GOLD, 3.5)
    mat_orb = make_aether_emissive("Organ_ResonanceOrb", AETHER_CYAN, 5.0)

    for p in parts:
        if p.name == "ResonanceOrb":
            p.data.materials.append(mat_orb)
        elif p.name.startswith("PipeCap_"):
            p.data.materials.append(mat_cap)
        elif p.name.startswith("Pipe_"):
            p.data.materials.append(mat_pipe)
        else:
            p.data.materials.append(mat_base)

    o = join_character(parts, "Prop_PipeOrgan")
    save_and_export(o, "Prop_PipeOrgan")


if __name__ == "__main__":
    main()
