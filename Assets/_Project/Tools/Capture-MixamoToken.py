"""
Capture-MixamoToken.py
Automates Mixamo bearer token capture.

Flow:
  1. Opens Chromium with a persistent profile (~/.mixamo-profile).
  2. Navigates to mixamo.com.
  3. If not logged in: you log in once (Adobe SSO). Profile is saved.
  4. Intercepts the next /api/v1/* request to read the Authorization header.
  5. Writes token to:
       - %USERPROFILE%\.mixamo_token  (single line)
       - $env:MIXAMO_TOKEN for current shell (printed for capture)

Usage:
  python C:\\dev\\TARTARIA_new\\Assets\\_Project\\Tools\\Capture-MixamoToken.py
  # First run: log in manually in the browser window. Subsequent runs: auto.

Requires: pip install playwright && playwright install chromium
"""
import os
import sys
import time
from pathlib import Path

try:
    from playwright.sync_api import sync_playwright
except ImportError:
    print("[Capture] Installing playwright...", flush=True)
    os.system(f'"{sys.executable}" -m pip install --quiet playwright')
    os.system(f'"{sys.executable}" -m playwright install --with-deps chromium')
    from playwright.sync_api import sync_playwright

PROFILE_DIR = Path.home() / ".mixamo-profile"
TOKEN_FILE  = Path.home() / ".mixamo_token"
PROFILE_DIR.mkdir(exist_ok=True)

print(f"[Capture] Profile : {PROFILE_DIR}")
print(f"[Capture] Output  : {TOKEN_FILE}")

captured = {"token": None}

def on_request(request):
    if captured["token"]:
        return
    if "/api/v1/" not in request.url:
        return
    auth = request.headers.get("authorization") or request.headers.get("Authorization")
    if auth and auth.lower().startswith("bearer "):
        captured["token"] = auth.split(" ", 1)[1]
        print(f"[Capture] TOKEN CAPTURED ({len(captured['token'])} chars) from {request.url}")

with sync_playwright() as p:
    ctx = p.chromium.launch_persistent_context(
        user_data_dir=str(PROFILE_DIR),
        headless=False,
        viewport={"width": 1280, "height": 900},
        args=["--disable-blink-features=AutomationControlled"],
    )
    page = ctx.pages[0] if ctx.pages else ctx.new_page()
    page.on("request", on_request)

    print("[Capture] Opening mixamo.com ...")
    page.goto("https://www.mixamo.com/#/", wait_until="domcontentloaded", timeout=60000)

    # Wait for login if needed.
    print("[Capture] If not logged in, please log in now (Adobe SSO).")
    print("[Capture] After login, browse the animation grid to trigger an /api/v1/ call.")
    print("[Capture] Token capture is automatic. Closing browser will exit.")

    # Try to nudge a request: click the search/animation grid.
    try:
        page.wait_for_selector("body", timeout=30000)
    except Exception:
        pass

    # Poll for token up to 5 minutes; allow user time to log in + click.
    start = time.time()
    while not captured["token"] and time.time() - start < 300:
        # Periodically reload the products endpoint to force an /api/v1/ call once logged in.
        try:
            page.evaluate("fetch('/api/v1/products?type=Motion&limit=1', {credentials:'include'}).catch(()=>{})")
        except Exception:
            pass
        time.sleep(2)
        if not page.context.browser and not ctx.pages:
            break

    ctx.close()

if captured["token"]:
    TOKEN_FILE.write_text(captured["token"], encoding="utf-8")
    print(f"\n[Capture] Token saved to: {TOKEN_FILE}")
    # Emit a marker line for PowerShell wrapper to parse.
    print(f"MIXAMO_TOKEN={captured['token']}")
    sys.exit(0)
else:
    print("\n[Capture] FAILED — no token captured. Did you log in and click an animation?")
    sys.exit(1)
