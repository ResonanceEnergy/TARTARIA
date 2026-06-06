"""
Auto-MixamoFetch.py — fully automated Mixamo bulk downloader.

Drives the actual Mixamo web UI with Playwright (using your saved login profile):
  - Searches for each animation by name
  - Clicks the result
  - Clicks Download
  - Confirms FBX-for-Unity defaults in the dialog
  - Saves the downloaded .fbx into Assets/_Project/Models/Animations/

This replaces the broken hardcoded-API approach (Mixamo's API IDs are stale
since 2018; the only reliable path is driving the real UI).

Usage:
  python Auto-MixamoFetch.py
  python Auto-MixamoFetch.py --headless    # run hidden after first login
"""
import argparse
import sys
import time
from pathlib import Path

try:
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout
except ImportError:
    import os
    os.system(f'"{sys.executable}" -m pip install --quiet playwright')
    os.system(f'"{sys.executable}" -m playwright install --with-deps chromium')
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout

PROFILE_DIR = Path.home() / ".mixamo-profile"
OUT_DIR = Path(r"C:\dev\TARTARIA_new\Assets\_Project\Models\Animations")
OUT_DIR.mkdir(parents=True, exist_ok=True)

# (search query, output filename stem)
ANIMATIONS = [
    ("Idle",            "Idle"),
    ("Walking",         "Walk"),
    ("Running",         "Run"),
    ("Jump",            "Jump"),
    ("Falling",         "Fall"),
    ("Digging",         "Dig"),
    ("Sword Slash",     "SwordSlash"),
    ("Hit Reaction",    "TakeDamage"),
]

ap = argparse.ArgumentParser()
ap.add_argument("--headless", action="store_true")
ap.add_argument("--only", help="Only fetch this animation name (debug)")
args = ap.parse_args()

print(f"[Auto] Profile: {PROFILE_DIR}")
print(f"[Auto] Output : {OUT_DIR}")
print(f"[Auto] Headless: {args.headless}")

with sync_playwright() as p:
    ctx = p.chromium.launch_persistent_context(
        user_data_dir=str(PROFILE_DIR),
        headless=args.headless,
        viewport={"width": 1400, "height": 900},
        accept_downloads=True,
    )
    page = ctx.pages[0] if ctx.pages else ctx.new_page()

    print("[Auto] Loading mixamo.com ...")
    page.goto("https://www.mixamo.com/#/", wait_until="domcontentloaded", timeout=60000)

    # Wait for either the editor canvas (logged in) or login button.
    try:
        page.wait_for_selector("input[placeholder*='Search'], input[placeholder*='search']", timeout=30000)
        print("[Auto] Logged in — search box found.")
    except PWTimeout:
        print("[Auto] Not logged in yet. Please log in in the browser window.")
        if args.headless:
            print("[Auto] FAIL: --headless but no saved login. Re-run without --headless first.")
            ctx.close()
            sys.exit(1)
        # Wait up to 5 min for manual login.
        page.wait_for_selector("input[placeholder*='Search'], input[placeholder*='search']", timeout=300000)
        print("[Auto] Login detected.")

    success, failed = [], []
    targets = [a for a in ANIMATIONS if not args.only or a[0].lower() == args.only.lower()]

    for query, stem in targets:
        out_path = OUT_DIR / f"{stem}.fbx"
        if out_path.exists():
            print(f"  SKIP {stem} (exists)")
            continue
        print(f"  GET  {stem}  (search: '{query}')")

        try:
            # --- Search ---
            search = page.locator("input[placeholder*='Search'], input[placeholder*='search']").first
            search.click()
            search.fill("")
            search.fill(query)
            page.wait_for_timeout(1500)  # debounce

            # --- Click first result ---
            # Mixamo result tiles are <div class="product"> or anchors with the title.
            result = page.locator("div.product, a[href*='animation'], div[class*='product']").first
            result.wait_for(timeout=15000)
            result.click()
            page.wait_for_timeout(2000)

            # --- Click DOWNLOAD button (right panel) ---
            dl_btn = page.get_by_role("button", name="DOWNLOAD")
            dl_btn.wait_for(timeout=15000)
            dl_btn.click()
            page.wait_for_timeout(1500)

            # --- Format dialog appears. Click DOWNLOAD inside it. ---
            # Dialog has a second DOWNLOAD button.
            with page.expect_download(timeout=120000) as dl_info:
                # Click the modal's DOWNLOAD button (last visible matching one).
                modal_btn = page.get_by_role("button", name="DOWNLOAD").last
                modal_btn.click()
            download = dl_info.value
            download.save_as(str(out_path))
            size_kb = out_path.stat().st_size / 1024
            print(f"    OK   {stem}.fbx ({size_kb:.1f} KB)")
            success.append(stem)
            page.wait_for_timeout(1000)

        except PWTimeout as e:
            print(f"    FAIL {stem}: timeout — {e}")
            failed.append(stem)
        except Exception as e:
            print(f"    FAIL {stem}: {e}")
            failed.append(stem)

    print()
    print(f"[Auto] Success: {len(success)}  Failed: {len(failed)}")
    if failed:
        print(f"[Auto] Failed animations: {', '.join(failed)}")
    ctx.close()

sys.exit(0 if not failed else 1)
