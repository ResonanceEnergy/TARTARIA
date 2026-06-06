"""
Auto-MixamoCharacters.py — automated Mixamo character model downloader.

Downloads 4 rigged character models from Mixamo for TARTARIA:
  1. Male Adventurer (Player)
  2. Female Royal/Queen (Anastasia)
  3. Male Engineer/Worker (Milo)
  4. Creature/Golem (MudGolem)

Each model is downloaded with T-pose rig, ready for Mixamo animation retargeting.

Usage:
  python Auto-MixamoCharacters.py
  python Auto-MixamoCharacters.py --headless    # run hidden after first login
"""
import argparse
import sys
from pathlib import Path
import time

try:
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout
except ImportError:
    import os
    os.system(f'"{sys.executable}" -m pip install --quiet playwright')
    os.system(f'"{sys.executable}" -m playwright install --with-deps chromium')
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout

PROFILE_DIR = Path.home() / ".mixamo-profile"
OUT_DIR = Path(r"C:\dev\TARTARIA_new\Assets\_Project\Models\Characters")
OUT_DIR.mkdir(parents=True, exist_ok=True)

# (search query, output filename, description)
CHARACTERS = [
    ("Adventurer", "Player_Mesh", "Male adventurer for Player prefab"),
    ("Queen", "Anastasia_Mesh", "Female royal for Anastasia NPC"),
    ("Worker", "Milo_Mesh", "Male engineer for Milo companion"),
    ("Mutant", "MudGolem_Mesh", "Creature for MudGolem enemy"),
]

ap = argparse.ArgumentParser()
ap.add_argument("--headless", action="store_true", help="Run browser hidden (requires prior login)")
ap.add_argument("--only", help="Only fetch this character name (debug)")
args = ap.parse_args()

with sync_playwright() as p:
    browser = p.chromium.launch_persistent_context(
        str(PROFILE_DIR),
        headless=args.headless,
        downloads_path=str(OUT_DIR),
    )
    page = browser.pages[0] if browser.pages else browser.new_page()

    # Navigate to Mixamo characters page
    print("[Characters] Opening Mixamo...")
    page.goto("https://www.mixamo.com/#/?page=1&type=Character", wait_until="domcontentloaded", timeout=60000)

    # Wait for search bar or login prompt
    try:
        page.wait_for_selector("input[placeholder*='Search'], input[placeholder*='search']", timeout=30000)
        print("[Characters] Already logged in.")
    except PWTimeout:
        if args.headless:
            print("[Characters] FAIL: --headless but no saved login. Re-run without --headless first.")
            sys.exit(1)
        print("[Characters] NOT logged in. Log in now (script waits)...")
        page.wait_for_selector("input[placeholder*='Search'], input[placeholder*='search']", timeout=300000)
        print("[Characters] Login detected. Proceeding.")

    targets = [c for c in CHARACTERS if not args.only or c[0].lower() == args.only.lower()]

    for query, filename, desc in targets:
        outfile = OUT_DIR / f"{filename}.fbx"
        if outfile.exists():
            print(f"[Characters] SKIP: {filename}.fbx already exists")
            continue

        print(f"\n[Characters] Fetching '{query}' → {filename}.fbx ({desc})")
        
        try:
            # Search for character
            search = page.locator("input[placeholder*='Search'], input[placeholder*='search']").first
            search.fill("")
            search.type(query, delay=50)
            time.sleep(2)  # Wait for search results

            # Click first character result (character cards have class 'character')
            result = page.locator("div.character, a[href*='character'], div[class*='character']").first
            result.wait_for(state="visible", timeout=10000)
            result.click()
            time.sleep(1)

            # Click DOWNLOAD button
            download_btn = page.locator("button:has-text('Download'), button:has-text('DOWNLOAD')").first
            download_btn.wait_for(state="visible", timeout=10000)
            
            with page.expect_download(timeout=60000) as dl_info:
                download_btn.click()
                time.sleep(2)
                
                # In the download dialog, confirm defaults (FBX for Unity, T-pose)
                # Click the final DOWNLOAD button in modal
                modal_dl = page.locator("button:has-text('Download'), button:has-text('DOWNLOAD')").last
                modal_dl.click()

            dl = dl_info.value
            dl.save_as(outfile)
            print(f"[Characters] ✓ Downloaded: {outfile}")

        except Exception as e:
            print(f"[Characters] ✗ FAILED: {query} — {e}")
            continue

    browser.close()

print("\n[Characters] Character download complete.")
print(f"[Characters] Output: {OUT_DIR}")
