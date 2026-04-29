"""
Fetch-AmbientCG.py — bulk download CC0 PBR texture sets from ambientCG.com.

Uses ambientCG's public JSON API (no auth, CC0 licensed).
Downloads 2K-PNG PBR sets (Color/NormalGL/Roughness/AO/Displacement/Metalness)
into Assets/_Project/Textures/PBR/{set_id}/.

Usage:
  python Fetch-AmbientCG.py
  python Fetch-AmbientCG.py --resolution 4K-PNG
  python Fetch-AmbientCG.py --only Mud002
"""
import argparse
import io
import sys
import urllib.request
import urllib.parse
import zipfile
from pathlib import Path

OUT_DIR = Path(r"C:\dev\TARTARIA_new\Assets\_Project\Textures\PBR")

# Curated set: Tartaria-relevant materials (verified IDs from ambientCG API)
SETS = [
    "Ground037",         # mud / packed dirt
    "Ground054",         # cracked mud
    "Rocks023",          # weathered stone rubble
    "Marble006",         # white marble (cathedral floors)
    "Plaster001",        # smooth interior wall
    "Metal047B",         # weathered copper
    "Metal048A",         # gold leaf / gilded
    "Metal032",          # rusted iron
    "MetalPlates006",    # riveted bronze panels
    "Bricks075A",        # old red brick
    "PavingStones150",   # cobblestone street
    "Wood063",           # aged planks
]

ap = argparse.ArgumentParser()
ap.add_argument("--resolution", default="2K-PNG", help="2K-PNG, 4K-PNG, 8K-PNG, etc.")
ap.add_argument("--only", help="Only fetch this set ID")
ap.add_argument("--overwrite", action="store_true")
args = ap.parse_args()

OUT_DIR.mkdir(parents=True, exist_ok=True)
print(f"[ACG] Output    : {OUT_DIR}")
print(f"[ACG] Resolution: {args.resolution}")

targets = [s for s in SETS if not args.only or s.lower() == args.only.lower()]
if args.only and not targets:
    targets = [args.only]  # let user fetch arbitrary IDs

success, failed, skipped = [], [], []
total_bytes = 0

for set_id in targets:
    out_sub = OUT_DIR / set_id
    if out_sub.exists() and any(out_sub.iterdir()) and not args.overwrite:
        print(f"  SKIP {set_id} (exists)")
        skipped.append(set_id)
        continue

    # ambientCG download URL: https://ambientcg.com/get?file={set}_{resolution}.zip
    file_name = f"{set_id}_{args.resolution}.zip"
    url = f"https://ambientcg.com/get?file={urllib.parse.quote(file_name)}"
    print(f"  GET  {set_id}  -> {file_name}")

    try:
        req = urllib.request.Request(url, headers={"User-Agent": "TartariaOpenClaw/1.0"})
        with urllib.request.urlopen(req, timeout=120) as resp:
            blob = resp.read()
        size_mb = len(blob) / (1024 * 1024)
        total_bytes += len(blob)

        out_sub.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(io.BytesIO(blob)) as z:
            members = [m for m in z.namelist()
                       if m.lower().endswith((".png", ".jpg", ".jpeg", ".exr", ".tx"))]
            for m in members:
                target = out_sub / Path(m).name
                with z.open(m) as src, open(target, "wb") as dst:
                    dst.write(src.read())
        print(f"    OK   {size_mb:.1f} MB  ({len(members)} maps)")
        success.append(set_id)
    except Exception as e:
        print(f"    FAIL {set_id}: {e}")
        failed.append(set_id)

print()
print(f"[ACG] Success: {len(success)}  Skipped: {len(skipped)}  Failed: {len(failed)}")
print(f"[ACG] Downloaded: {total_bytes / (1024 * 1024):.1f} MB")
if failed:
    print(f"[ACG] Failed sets: {', '.join(failed)}")
sys.exit(0 if not failed else 1)
