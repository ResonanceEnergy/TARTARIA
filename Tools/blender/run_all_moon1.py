"""Moon 1 master batch — spawns one Blender subprocess per gen script.

ROOT-CAUSE FIX 2026-06-04: prior version used `exec(compile(open(...).read(), ...))`
to chain every gen_*.py inside one Blender process. Each gen script appends
materials / meshes / scene state into the same interpreter, and by ~5-10 scripts
in the accumulated bpy.data and Python module re-import state crashed mid-run.
The `Tartaria/4 Generate Art/Blender — Moon 1` Editor menu therefore went
broken halfway through every batch.

New design: spawn ONE `blender --background --python <script>` subprocess per
gen_*.py. Each subprocess starts with a fresh interpreter + clean default scene,
runs its script, exports its FBX, and exits. Failures in one script no longer
poison the next.

Usage (any of these):
  python tools/blender/run_all_moon1.py
  blender --background --python tools/blender/run_all_moon1.py     # legacy path also works
  (Unity menu) Tartaria → 4 Generate Art → Blender — Moon 1

Failures are collected and printed at the end; non-zero exit on any failure so
Editor batch invocations can surface the error.
"""
import os
import sys
import subprocess
import time
from pathlib import Path


HERE = Path(__file__).parent


def _resolve_blender():
    """Return absolute path to blender executable.

    Priority:
      1) BLENDER env var (full path or just 'blender' on PATH)
      2) Windows default install at Blender 5.0 then 4.5
      3) Linux 'blender' on PATH
    """
    env = os.environ.get("BLENDER")
    if env:
        return env
    if sys.platform == "win32":
        for candidate in (
            r"C:\Program Files\Blender Foundation\Blender 5.0\blender.exe",
            r"C:\Program Files\Blender Foundation\Blender 4.5\blender.exe",
            r"C:\Program Files\Blender Foundation\Blender 4.4\blender.exe",
            r"C:\Program Files\Blender Foundation\Blender 4.3\blender.exe",
        ):
            if os.path.isfile(candidate):
                return candidate
    # Fallback to PATH lookup
    return "blender"


BLENDER = _resolve_blender()


# Only Moon 1 ships to Assets/_Project/Models/Blender/Moon1/. Order is roughly
# characters first (NPCs / enemies) then buildings then props, so a failure
# early surfaces the most-visible regression.
SCRIPTS = [
    # --- Moon 1 NPCs ---
    "gen_npc_anastasia.py",
    "gen_npc_lirael.py",
    "gen_npc_cassian.py",
    "gen_milo_fox.py",
    # --- Moon 1 enemies ---
    "gen_mud_golem.py",
    "gen_reset_scout.py",
    # --- Moon 1 buildings (village + hero + apothecary/townhall/watchtower) ---
    "gen_buildings_village.py",
    "gen_buildings_special.py",
    "gen_bobs_inn.py",
    # --- Moon 1 furniture + interactives ---
    "gen_anastasia_chair.py",
    "gen_brazier.py",
    "gen_aether_crystals.py",
    "gen_tuning_pedestal.py",
    "gen_mud_pool_basin.py",
    "gen_lore_artifact_scroll.py",
    "gen_giant_skeleton_key.py",
    "gen_skeleton_remains.py",
    "gen_pipe_organ.py",
    "gen_moon1_furniture.py",
    "gen_moon1_misc.py",
    "gen_moon1_polish.py",
    # --- Moon 1 mini-game props (tuning bells / waveform pillar / slider stand) ---
    "gen_minigame_props.py",
    # --- 2026-06-05 detailed foliage replacement set (autonomous polish) ---
    "gen_oak_tree.py",
    "gen_pine_tree.py",
    "gen_bush_clump.py",
]


def run_one(script_name, timeout_s=180):
    """Run a single gen_*.py via `blender --background --python`.

    Returns (exit_code, stdout_tail, stderr_tail).
    """
    script_path = HERE / script_name
    if not script_path.exists():
        return (None, "", "[SKIP] %s not present on disk" % script_name)

    cmd = [BLENDER, "--background", "--python", str(script_path)]
    try:
        result = subprocess.run(
            cmd,
            capture_output=True,
            text=True,
            timeout=timeout_s,
            check=False,
        )
    except subprocess.TimeoutExpired:
        return (124, "", "TIMEOUT after %ds" % timeout_s)
    except FileNotFoundError:
        return (127, "", "Blender not found at %s" % BLENDER)

    out_tail = "\n".join(result.stdout.splitlines()[-6:]) if result.stdout else ""
    err_tail = "\n".join(result.stderr.splitlines()[-6:]) if result.stderr else ""
    return (result.returncode, out_tail, err_tail)


def main():
    print("[TARTARIA] Moon 1 batch — blender=%s" % BLENDER)
    print("[TARTARIA] %d scripts queued." % len(SCRIPTS))

    start = time.time()
    failed = []
    skipped = []
    succeeded = []

    for i, name in enumerate(SCRIPTS, start=1):
        prefix = "[%d/%d]" % (i, len(SCRIPTS))
        rc, out_tail, err_tail = run_one(name)
        if rc is None:
            print("%s [SKIP] %s — %s" % (prefix, name, err_tail))
            skipped.append(name)
            continue
        if rc == 0:
            print("%s [OK]   %s" % (prefix, name))
            succeeded.append(name)
        else:
            print("%s [FAIL] %s — exit %s" % (prefix, name, rc))
            if out_tail:
                print("        stdout tail: %s" % out_tail.replace("\n", " | "))
            if err_tail:
                print("        stderr tail: %s" % err_tail.replace("\n", " | "))
            failed.append((name, rc))

    elapsed = time.time() - start
    print("")
    print("[TARTARIA] === DONE in %.1fs ===" % elapsed)
    print("[TARTARIA] succeeded=%d failed=%d skipped=%d" % (
        len(succeeded), len(failed), len(skipped)))
    if skipped:
        print("[TARTARIA] skipped: %s" % ", ".join(skipped))
    if failed:
        print("[TARTARIA] FAILURES:")
        for name, rc in failed:
            print("           %s (exit %s)" % (name, rc))
        sys.exit(1)
    sys.exit(0)


if __name__ == "__main__":
    main()
