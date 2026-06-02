# UNITY_MCP_SETUP.md — Bridge Claude / VS Code to Unity Editor
*2026-06-02 · Replaces the computer-use mouse-click loop with structured MCP calls.*

---

## What this gets you

| Before MCP bridge | After MCP bridge |
|---|---|
| Cowork clicks `x=350, y=23` to open `Tartaria → 8 Fix` menu via computer-use | `unity_execute_menu("Tartaria/8 Fix/Force All Spawn Refs To (0,2,15)")` |
| Cowork OCRs screenshots to read the console | `unity_read_console(level="Error")` returns structured array |
| Cowork eyeballs "did the capsule move?" | `unity_query_scene_object(name="Player")` returns transform position JSON |
| Cowork drives Play button by pixel-click | `unity_toggle_play()` |

---

## Install (automated)

Already landed in this repo by Cowork:

1. **Packages/manifest.json** — added `"com.unity.ai.assistant": "2.0.0-pre.1"`. Unity resolves on next focus.
2. **Assets/_Project/Scripts/Editor/VerifyUnityMcpBridge.cs** — `Tartaria → 9 Debug → Verify Unity MCP Bridge` smoke-test menu.

## Steps for NATRIX after pulling

1. **Pull + focus Unity** so it resolves `com.unity.ai.assistant`. Watch the Package Manager spinner.
2. **Enable the MCP server** — `Edit → Preferences → AI Assistant → MCP Server` → toggle on. Note the port (typically `8090` or similar).
3. **Run the smoke-test** — `Tartaria → 9 Debug → Verify Unity MCP Bridge`. Should report "MCP server type found".
4. **Configure your MCP client** (see below).

---

## Client config snippets

### Claude Desktop (Mac / Windows)

Edit `%APPDATA%\Claude\claude_desktop_config.json` (Windows) or `~/Library/Application Support/Claude/claude_desktop_config.json` (Mac):

```json
{
  "mcpServers": {
    "unity-tartaria": {
      "command": "node",
      "args": ["-e", "process.stdin.pipe(require('net').connect(8090)).pipe(process.stdout)"]
    }
  }
}
```

(Adjust port if Unity reports something else.)

### VS Code (Copilot Chat with MCP)

Edit `%APPDATA%\Code\User\settings.json`:

```json
{
  "github.copilot.chat.experimental.mcp.servers": {
    "unity-tartaria": {
      "url": "http://localhost:8090/mcp"
    }
  }
}
```

### Cowork / Claude Code SDK

Add to your project's `.mcp.json` or invoke `claude mcp add unity-tartaria http://localhost:8090/mcp`.

---

## Smoke-test sequence after first connect

From your MCP client:

```
1. list_tools           → expect: execute_menu, read_console, query_scene, toggle_play, get_compile_errors
2. read_console(level=Error)  → expect: empty or known errors
3. execute_menu("Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems")
4. read_console(since=last)   → expect: "Moon1_Systems bootstrapped"
5. query_scene_object("Main Camera") → expect: Transform JSON
```

If all 5 land, the bridge is live and we retire the computer-use mouse loop for Unity work.

---

## Fallback options

If `com.unity.ai.assistant` doesn't resolve cleanly on Unity 6.3.6f1 (it's a pre-release package), use one of:

- **CoplayDev/unity-mcp** — Package Manager → Add by git URL: `https://github.com/CoplayDev/unity-mcp.git?path=/UnityMcpBridge`
- **CoderGamester/mcp-unity** — see https://github.com/CoderGamester/mcp-unity for Node.js server-side install

These provide overlapping APIs; pick the one that resolves first.

---

## Anti-patterns

- ❌ Don't depend on the MCP bridge for runtime gameplay — it's an Editor-only tool
- ❌ Don't commit your MCP client config with API keys to git
- ❌ Don't run multiple MCP servers on the same port

---

*v1.0 · 2026-06-02 · Update when the bridge ships its v1 stable + the menu/tool names settle.*
