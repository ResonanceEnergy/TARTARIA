// Moon1MCPAutoStart.cs
// One-shot: enables Unity MCP HTTP auto-start so future Unity launches bring the bridge up
// automatically. Runs once per session via SessionState guard. Per CLAUDE.md mandates:
// no silent catches, full context logging.
//
// 2026-06-02 — Authored to avoid pixel-clicking the Window→MCP for Unity→Start Server button
// every Unity boot. The CoplayDev package's HttpAutoStartHandler reads
// EditorPrefs[MCPForUnity.AutoStartOnLoad] on [InitializeOnLoad] — we just flip that bool ON.
using System;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    [InitializeOnLoad]
    internal static class Moon1MCPAutoStart
    {
        private const string AutoStartKey = "MCPForUnity.AutoStartOnLoad";
        private const string UseHttpKey   = "MCPForUnity.UseHttpTransport";
        private const string ScopeKey     = "MCPForUnity.HttpTransportScope";
        private const string SessionDoneKey = "Tartaria.Moon1MCPAutoStart.SessionApplied";

        static Moon1MCPAutoStart()
        {
            // Once per editor session — don't spam logs.
            if (SessionState.GetBool(SessionDoneKey, false)) return;
            SessionState.SetBool(SessionDoneKey, true);

            try
            {
                bool autoStartBefore = EditorPrefs.GetBool(AutoStartKey, false);
                bool useHttpBefore   = EditorPrefs.GetBool(UseHttpKey, false);
                string scopeBefore   = EditorPrefs.GetString(ScopeKey, "");

                if (!autoStartBefore) EditorPrefs.SetBool(AutoStartKey, true);
                if (!useHttpBefore)   EditorPrefs.SetBool(UseHttpKey, true);
                if (scopeBefore != "local") EditorPrefs.SetString(ScopeKey, "local");

                Debug.Log(
                    $"[Moon1MCPAutoStart] EditorPrefs applied. " +
                    $"AutoStartOnLoad: {autoStartBefore}->true. " +
                    $"UseHttpTransport: {useHttpBefore}->true. " +
                    $"Scope: '{scopeBefore}'->'local'. " +
                    $"Next domain reload will trigger HttpAutoStartHandler to start the bridge " +
                    $"on http://127.0.0.1:8080/mcp.");
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[Moon1MCPAutoStart] Failed to apply EditorPrefs: " +
                    $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
