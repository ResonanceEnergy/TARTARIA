"""
mixamo-download-v2.py — Improved Mixamo automation with robust selectors.

Uses better waiting strategies and multiple selector fallbacks to handle
Mixamo's React-based UI changes.

Usage:
  python mixamo-download-v2.py
  python mixamo-download-v2.py --debug    # enable screenshots for debugging
"""
import argparse
import sys
import time
from pathlib import Path

try:
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout, expect
except ImportError:
    import os
    os.system(f'"{sys.executable}" -m pip install --quiet playwright')
    os.system(f'"{sys.executable}" -m playwright install --with-deps chromium')
    from playwright.sync_api import sync_playwright, TimeoutError as PWTimeout, expect

PROFILE_DIR = Path.home() / ".mixamo-profile"
OUT_DIR = Path(r"C:\dev\TARTARIA_new\Assets\_Project\Models\Characters")
OUT_DIR.mkdir(parents=True, exist_ok=True)

CHARACTERS = [
    ("Adventurer", "Player_Mesh.fbx", "Male adventurer"),
    ("Queen", "Anastasia_Mesh.fbx", "Female royal"),
    ("Worker", "Milo_Mesh.fbx", "Male engineer"),
    ("Mutant", "MudGolem_Mesh.fbx", "Creature enemy"),
]

ap = argparse.ArgumentParser()
ap.add_argument("--debug", action="store_true", help="Save screenshots for debugging")
ap.add_argument("--slow", action="store_true", help="Slow down for visual debugging")
args = ap.parse_args()

print(f"[V2] Output: {OUT_DIR}")
print(f"[V2] Profile: {PROFILE_DIR}")

with sync_playwright() as p:
    browser = p.chromium.launch_persistent_context(
        str(PROFILE_DIR),
        headless=False,
        viewport={"width": 1600, "height": 1000},
        slow_mo=1000 if args.slow else 0,
    )
    page = browser.pages[0] if browser.pages else browser.new_page()

    print("[V2] Loading Mixamo...")
    page.goto("https://www.mixamo.com/#/?page=1&type=Character", wait_until="domcontentloaded")
    
    # Wait for login
    try:
        page.wait_for_selector("input[type='search'], input[placeholder*='earch']", timeout=15000)
        print("[V2] ✓ Logged in")
    except PWTimeout:
        print("[V2] ⚠ Login required - logging in...")
        page.wait_for_selector("input[type='search'], input[placeholder*='earch']", timeout=180000)
        print("[V2] ✓ Login complete")

    if args.debug:
        page.screenshot(path=str(OUT_DIR / "debug_main.png"))

    for query, filename, desc in CHARACTERS:
        outfile = OUT_DIR / filename
        if outfile.exists():
            print(f"[V2] SKIP: {filename} (exists)")
            continue

        print(f"\n[V2] Downloading '{query}' → {filename}")
        
        try:
            # Clear search and type query
            search_selectors = [
                "input[type='search']",
                "input[placeholder*='earch']",
                "input[placeholder*='Search']",
                ".search-input input",
                "#search input"
            ]
            
            search = None
            for selector in search_selectors:
                try:
                    search = page.locator(selector).first
                    search.wait_for(state="visible", timeout=3000)
                    break
                except:
                    continue
            
            if not search:
                raise Exception("Could not find search box")
            
            search.click()
            search.fill("")
            search.fill(query)
            page.keyboard.press("Enter")
            
            print(f"[V2]   Waiting for search results...")
            time.sleep(3)  # Wait for React to render results
            
            if args.debug:
                page.screenshot(path=str(OUT_DIR / f"debug_search_{query}.png"))
            
            # Try multiple selectors for character cards
            card_selectors = [
                "a[href*='#/?character_id=']",  # Direct character links
                "[class*='character-card']",
                "[class*='CharacterCard']",
                "[class*='grid-item']",
                "a[href*='character']",
                "article",
                "[role='article']",
                ".thumbnail-container",
            ]
            
            card = None
            for selector in card_selectors:
                try:
                    cards = page.locator(selector).all()
                    print(f"[V2]   Found {len(cards)} elements with selector: {selector}")
                    if cards and len(cards) > 0:
                        # Skip "Learn & Support" links
                        for c in cards:
                            href = c.get_attribute("href") or ""
                            if "helpx.adobe.com" not in href and "support" not in href.lower():
                                card = c
                                break
                        if card:
                            print(f"[V2]   Using selector: {selector}")
                            break
                except Exception as e:
                    print(f"[V2]   Selector '{selector}' failed: {e}")
                    continue
            
            if not card:
                raise Exception("Could not find character card")
            
            print(f"[V2]   Clicking character card...")
            card.click()
            time.sleep(2)
            
            if args.debug:
                page.screenshot(path=str(OUT_DIR / f"debug_selected_{query}.png"))
            
            # Find and click download button
            download_selectors = [
                "button:has-text('Download')",
                "button:has-text('DOWNLOAD')",
                "[class*='download-button']",
                "[class*='Download']",
                "button[class*='primary']",
            ]
            
            download_btn = None
            for selector in download_selectors:
                try:
                    download_btn = page.locator(selector).first
                    download_btn.wait_for(state="visible", timeout=5000)
                    print(f"[V2]   Found download button: {selector}")
                    break
                except:
                    continue
            
            if not download_btn:
                raise Exception("Could not find download button")
            
            print(f"[V2]   Clicking download...")
            
            # Start download
            with page.expect_download(timeout=60000) as download_info:
                download_btn.click()
                time.sleep(1)
                
                # In the modal, click the final download button
                try:
                    modal_download = page.locator("button:has-text('Download'), button:has-text('DOWNLOAD')").nth(1)
                    modal_download.click()
                except:
                    # Modal might auto-download with default settings
                    pass
            
            download = download_info.value
            download.save_as(outfile)
            
            size_mb = outfile.stat().st_size / (1024 * 1024)
            print(f"[V2] ✓ Downloaded: {filename} ({size_mb:.2f} MB)")
            
        except Exception as e:
            print(f"[V2] ✗ FAILED: {query}")
            print(f"[V2]   Error: {e}")
            if args.debug:
                try:
                    page.screenshot(path=str(OUT_DIR / f"debug_error_{query}.png"))
                except:
                    pass

    browser.close()

print("\n[V2] Complete!")
print(f"[V2] Check: {OUT_DIR}")
