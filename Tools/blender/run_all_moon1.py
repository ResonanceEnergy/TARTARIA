"""Master batch — Moon 1 Blender asset production. Run via:
  blender --background --python tools/blender/run_all_moon1.py
"""
import os
SCRIPTS = [
    # Tier 0 props (originally shipped)
    "gen_anastasia_chair.py",
    "gen_brazier.py",
    "gen_aether_crystals.py",
    "gen_bobs_inn.py",
    "gen_tuning_pedestal.py",
    # Tier 1 add-ons (this batch)
    "gen_mud_pool_basin.py",
    "gen_lore_artifact_scroll.py",
    "gen_giant_skeleton_key.py",
    "gen_skeleton_remains.py",
    "gen_pipe_organ.py",
]
base = os.path.dirname(__file__)
for s in SCRIPTS:
    print(f"\n=== Running {s} ===")
    exec(compile(open(os.path.join(base, s)).read(), s, 'exec'))
print("\n[TARTARIA] All Moon 1 Blender assets generated.")
