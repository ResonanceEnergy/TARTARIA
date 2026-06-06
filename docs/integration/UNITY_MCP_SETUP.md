# UNITY_MCP_SETUP.md — Bridge Cowork to Unity Editor

> **v2 · 2026-06-02 · Restored after sibling-agent loss.**
> Lets Cowork drive Unity by structured API call (`mcp__unity-tartaria__*`) instead of pixel-clicking menus.

---

## Why this exists

Without the bridge, Cowork drives Unity via `computer-use` — taking screenshots, calculating menu pixel coordinates, clicking. That works but is fragile (elevated processes intercept input, focus-loss kills clicks, menu items shift position after compile).

With the bridge, Cowork calls `mcp__unity-tartaria__execute_menu("Tartaria/Content/Rebind Moon 1 NPC Prefabs")` and gets a structured response. Console reads, scene queries, Play-mode toggles all become API calls.

---

## Architecture

```
Cowork session
    ↓ stdio
npx mcp-remote http://127.0.0.1:8080/mcp
    ↓ HTTP/SSE
uvx mcpforunityserver (Python, started by Unity)
    ↓ WebSocket /hub/plugin
Unity Editor (MCP for Unity dock, port 8080)
    ↓ in-process
[MenuItem] handlers, Scene/Inspector APIs, Console
```

## One-time install per project

### Step 1 — Package in `Packages/manifest.json`

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```

Unity Package Manager resolves it from GitHub on next focus. The repo path **must** be `?path=/MCPForUnity#main` — the legacy `/UnityMcpBridge` path the README originally suggested is gone; CoplayDev restructured the package in mid-2025.

### Step 2 — Cowork client config

Edit `%LOCALAPPDATA%\Packages\Claude_pzs8sxrjxfjjc\LocalCache\Roaming\Claude\claude_desktop_config.json` and add:

```json
"mcpServers": {
  "unity-tartaria": {
    "command": "npx",
    "args": ["-y", "mcp-remote", "http://127.0.0.1:8080/mcp"]
  }
}
```

(Adjust port if Unity uses something other than 8080 — check the dock's HTTP URL field.)

Cowork reads this file **at session start only.** Adding the entry mid-session does not hot-load; you must restart Cowork.

---

## Per-Unity-boot startup

1. Focus Unity → package auto-resolves (watch Package Manager spinner if it's the first time)
2. **Window → MCP for Unity** → opens the docked panel
3. Click the green **"Start Server"** button — confirm the dialog
4. A new terminal window appears running `uvx mcpforunityserver`. FastMCP banner prints, then:
   ```
   INFO: Started server process [...]
   INFO: Uvicorn running on http://127.0.0.1:8080
   Plugin registered: TARTARIA_new (...)
   Registered N tools for session ...
   ```
5. Dock shows red **"Stop Server"** button — bridge is live

**Verification (PowerShell):** `Test-NetConnection -ComputerName 127.0.0.1 -Port 8080 -InformationLevel Quiet` should return `True`.

---

## Per-Cowork-session startup

After Unity's bridge is live, restart Cowork (close session, reopen). On reopen, the deferred tools list will include `mcp__unity-tartaria__*` entries that load via `ToolSearch`.

---

## What the bridge gives Cowork

| Cowork tool | What it does |
|---|---|
| `mcp__unity-tartaria__execute_menu(path)` | Runs a `[MenuItem(path)]` handler — drives any Tartaria/Window menu without clicking |
| `mcp__unity-tartaria__read_console(level)` | Returns structured console messages — no OCR of screenshots |
| `mcp__unity-tartaria__query_scene_object(name)` | Returns a GameObject's transform + components as JSON |
| `mcp__unity-tartaria__toggle_play` | Enter/exit Play mode |
| `mcp__unity-tartaria__get_compile_errors` | Returns the live compile error list |
| `mcp__unity-tartaria__list_tools` | Discovery — what else the bridge exposes |

---

## Fallback order when the bridge is down

1. Try `mcp__unity-tartaria__*` first — fastest, structured
2. If "tool not found" / connection refused → check `Test-NetConnection 8080` and the MCP dock state
3. If bridge truly down: `mcp__Windows-MCP__Click` with `label="Tartaria"` (uses Windows UI Automation, robust to position shifts but slower)
4. Last resort: `mcp__computer-use__left_click` with pixel coords (fragile, gets blocked by elevated processes)

---

## Known failure modes

| Symptom | Cause | Fix |
|---|---|---|
| `Test-NetConnection 8080 = False` | Server terminal got closed | Re-click "Start Server" in MCP dock |
| `mcp__unity-tartaria__*` not in tool list | Cowork session started before config was added | Restart Cowork |
| Package resolution fails with `pathspec 'UnityMcpBridge'` | Stale legacy install path | Already fixed — manifest uses `?path=/MCPForUnity#main` |
| Bridge port 8080 already taken | Another service is binding it | Change port in MCP dock + update `claude_desktop_config.json` to match |
| "Configure All Detected Clients" doesn't list Claude Desktop / Cowork | Cowork is in non-standard install location | Use manual config path above — don't rely on the wizard |

---

## Verification smoke test (after restart)

From Cowork in a new session:
```
1. list_tools                        → expect: execute_menu, read_console, ...
2. read_console(level="Error")        → expect: empty or known errors
3. execute_menu("Tartaria/9 Debug/Verify Unity MCP Bridge")
                                      → expect: dialog "MCP server type found"
4. query_scene_object("Main Camera")  → expect: transform JSON
5. execute_menu("Tartaria/Content/Rebind Moon 1 NPC Prefabs")
                                      → expect: rebind log + scene save
```

If all 5 land, bridge is live and Cowork can drive Unity fully via API.

---

## Anti-patterns

- ❌ Don't depend on the MCP bridge for **runtime** gameplay — Editor-only tool
- ❌ Don't commit your MCP client config with API keys
- ❌ Don't run multiple MCP servers on port 8080
- ❌ Don't expect tool calls to work mid-session if config was added after Cowork launched

---

*v2 · 2026-06-02 · restored after sibling-agent loss. Originally written at session boot.*
