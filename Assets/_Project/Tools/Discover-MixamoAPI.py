"""
Discover-MixamoAPI.py
Records every /api/v1/* request Mixamo makes during a real download flow,
so we can replay it correctly from PowerShell.

Usage:
  python Discover-MixamoAPI.py
  -> opens browser, log in (or reuse profile)
  -> click any animation thumbnail
  -> click DOWNLOAD button
  -> click DOWNLOAD in the format dialog (FBX for Unity, default settings)
  -> wait until file actually downloads in the browser
  -> close browser
  -> outputs C:\\Users\\gripa\\.mixamo_api_trace.json
"""
import json
import sys
import time
from pathlib import Path

try:
    from playwright.sync_api import sync_playwright
except ImportError:
    import os
    os.system(f'"{sys.executable}" -m pip install --quiet playwright')
    os.system(f'"{sys.executable}" -m playwright install --with-deps chromium')
    from playwright.sync_api import sync_playwright

PROFILE_DIR = Path.home() / ".mixamo-profile"
TRACE_FILE  = Path.home() / ".mixamo_api_trace.json"

calls = []

def on_request(req):
    if "/api/" not in req.url:
        return
    try:
        body = req.post_data
    except Exception:
        body = None
    calls.append({
        "method": req.method,
        "url": req.url,
        "headers": dict(req.headers),
        "body": body,
    })

def on_response(res):
    if "/api/" not in res.url:
        return
    # Attach response status to last matching call.
    for c in reversed(calls):
        if c["url"] == res.url and "status" not in c:
            c["status"] = res.status
            try:
                if "json" in (res.headers.get("content-type") or ""):
                    c["response_preview"] = (res.text() or "")[:500]
            except Exception:
                pass
            break

with sync_playwright() as p:
    ctx = p.chromium.launch_persistent_context(
        user_data_dir=str(PROFILE_DIR),
        headless=False,
        viewport={"width": 1280, "height": 900},
        accept_downloads=True,
    )
    page = ctx.pages[0] if ctx.pages else ctx.new_page()
    page.on("request", on_request)
    page.on("response", on_response)

    print("[Discover] Opening mixamo.com ...")
    page.goto("https://www.mixamo.com/#/", wait_until="domcontentloaded", timeout=60000)

    print("[Discover] === STEPS ===")
    print("[Discover] 1. Click any animation thumbnail.")
    print("[Discover] 2. Click the green DOWNLOAD button.")
    print("[Discover] 3. In the dialog: keep defaults (FBX for Unity), click DOWNLOAD.")
    print("[Discover] 4. Wait until your browser actually downloads the .fbx.")
    print("[Discover] 5. Close the browser window.")
    print("[Discover] Recording all /api/v1 traffic ...")

    # Wait until user closes the window.
    try:
        while True:
            time.sleep(1)
            if not ctx.pages:
                break
    except KeyboardInterrupt:
        pass

# Save trace.
TRACE_FILE.write_text(json.dumps(calls, indent=2), encoding="utf-8")
print(f"\n[Discover] Captured {len(calls)} API call(s)")
print(f"[Discover] Trace saved to: {TRACE_FILE}")

# Print summary of unique endpoints.
seen = set()
print("\n[Discover] === ENDPOINTS USED ===")
for c in calls:
    key = (c["method"], c["url"].split("?")[0])
    if key in seen:
        continue
    seen.add(key)
    status = c.get("status", "?")
    print(f"  {c['method']:6} {status} {c['url'][:120]}")
