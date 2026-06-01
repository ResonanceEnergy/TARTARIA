"""Master batch — runs all gen_*_set.py + individual gen scripts."""
import os as _os_module
_path_exists = _os_module.path.exists
_path_join = _os_module.path.join
_path_dirname = _os_module.path.dirname

SCRIPTS = [
    "gen_anastasia_chair.py", "gen_brazier.py", "gen_aether_crystals.py",
    "gen_bobs_inn.py", "gen_tuning_pedestal.py", "gen_mud_pool_basin.py",
    "gen_lore_artifact_scroll.py", "gen_giant_skeleton_key.py",
    "gen_skeleton_remains.py", "gen_pipe_organ.py",
    "gen_moon1_polish.py", "gen_moon1_furniture.py", "gen_moon1_misc.py",
    "gen_moon2_set.py", "gen_moon3_set.py", "gen_moon4_set.py",
    "gen_moon5_set.py", "gen_moon6_set.py", "gen_moon7_set.py",
    "gen_moon8_set.py", "gen_moon9_set.py", "gen_moon10_set.py",
    "gen_moon11_set.py", "gen_moon12_set.py", "gen_moon13_set.py",
    "gen_shared_props.py",
]

base = _path_dirname(__file__)
for s in SCRIPTS:
    p = _path_join(base, s)
    if not _path_exists(p):
        print(f"\n=== SKIP {s} (not found) ===")
        continue
    print(f"\n=== Running {s} ===")
    try:
        exec(compile(open(p).read(), s, 'exec'))
    except Exception as e:
        print(f"!! ERROR in {s}: {e}")

print("\n[TARTARIA] ALL Moon 1-13 + Shared Blender batch finished.")
