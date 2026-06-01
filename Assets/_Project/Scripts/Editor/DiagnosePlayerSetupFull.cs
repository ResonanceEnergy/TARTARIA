#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Input/DEEP DIAGNOSE -> WRITE TO FILE
    /// Same as DiagnosePlayerSetup but writes the full report to
    /// Assets/_player_diagnostic.txt so we can read it without truncation.
    /// </summary>
    public static class DiagnosePlayerSetupFull
    {
        const string OUT = "Assets/_player_diagnostic.txt";

        [MenuItem("Tartaria/7 Diagnose/Player Setup → write to file", priority = 710)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Tartaria Player Setup — FULL Diagnostic ===");
            sb.AppendLine($"Time.timeScale = {Time.timeScale}");
            sb.AppendLine($"Application.runInBackground = {Application.runInBackground}");
            sb.AppendLine($"PlayerSettings.runInBackground = {PlayerSettings.runInBackground}");
            sb.AppendLine($"EditorApplication.isPaused = {EditorApplication.isPaused}");
            sb.AppendLine($"EditorApplication.isPlaying = {EditorApplication.isPlaying}");
            sb.AppendLine($"EditorApplication.isCompiling = {EditorApplication.isCompiling}");
#if ENABLE_INPUT_SYSTEM
            try
            {
                sb.AppendLine($"InputSystem.settings.backgroundBehavior = {UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior}");
                sb.AppendLine($"InputSystem.settings.editorInputBehaviorInPlayMode = {UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode}");
                sb.AppendLine($"InputSystem.devices.Count = {UnityEngine.InputSystem.InputSystem.devices.Count}");
                foreach (var d in UnityEngine.InputSystem.InputSystem.devices)
                    sb.AppendLine($"  device: {d.displayName} ({d.layout}) added={d.added} enabled={d.enabled}");
            }
            catch (System.Exception e) { sb.AppendLine("InputSystem error: " + e.Message); }
#endif
            sb.AppendLine();

            var ccs = Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            sb.AppendLine($"CharacterController count in scene: {ccs.Length}");
            for (int i = 0; i < ccs.Length; i++)
            {
                var cc = ccs[i];
                sb.AppendLine($"\n--- CC[{i}] '{cc.gameObject.name}' ---");
                sb.AppendLine($"  pos = {cc.transform.position}");
                sb.AppendLine($"  parent = '{(cc.transform.parent != null ? cc.transform.parent.name : "(none)")}'");
                sb.AppendLine($"  enabled = {cc.enabled}  isGrounded = {cc.isGrounded}");
                sb.AppendLine($"  activeInHierarchy = {cc.gameObject.activeInHierarchy}  activeSelf = {cc.gameObject.activeSelf}");
                sb.AppendLine($"  tag = '{cc.gameObject.tag}'  layer = '{LayerMask.LayerToName(cc.gameObject.layer)}'");

                var rb = cc.GetComponent<Rigidbody>();
                if (rb != null)
                    sb.AppendLine($"  ⚠ Rigidbody PRESENT: isKinematic={rb.isKinematic}, useGravity={rb.useGravity}, mass={rb.mass}, linearVel={rb.linearVelocity}");

                sb.AppendLine($"  Components:");
                foreach (var m in cc.GetComponents<Component>())
                {
                    if (m == null) { sb.AppendLine("    - <MISSING SCRIPT>"); continue; }
                    var mb = m as MonoBehaviour;
                    string enState = mb != null ? (mb.enabled ? "ON" : "OFF") : "-";
                    sb.AppendLine($"    - {m.GetType().Name} ({enState})");
                }

                // Also scan children
                sb.AppendLine($"  Children scripts:");
                foreach (var mb in cc.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (mb == null) continue;
                    if (mb.gameObject == cc.gameObject) continue;
                    sb.AppendLine($"    [{mb.gameObject.name}] {mb.GetType().Name} ({(mb.enabled?"ON":"OFF")})");
                }
            }

            // Player tag
            var pTag = GameObject.FindGameObjectWithTag("Player");
            sb.AppendLine();
            sb.AppendLine($"Player-tagged GameObject: {(pTag != null ? pTag.name : "(NONE)")}");
            if (pTag != null)
            {
                sb.AppendLine($"  pos = {pTag.transform.position}");
                sb.AppendLine($"  Has CC: {pTag.GetComponent<CharacterController>() != null}");
                sb.AppendLine($"  Has Rigidbody: {pTag.GetComponent<Rigidbody>() != null}");
                sb.AppendLine($"  Components on Player tag:");
                foreach (var m in pTag.GetComponents<Component>())
                {
                    if (m == null) { sb.AppendLine("    - <MISSING SCRIPT>"); continue; }
                    var mb = m as MonoBehaviour;
                    string en = mb != null ? (mb.enabled?"ON":"OFF") : "-";
                    sb.AppendLine($"    - {m.GetType().Name} ({en})");
                }
            }

            // Count types of MBs that might be drivers
            sb.AppendLine();
            sb.AppendLine("=== Script-by-script counts (scene-wide) ===");
            var allMBs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var counts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var m in allMBs)
            {
                if (m == null) continue;
                string n = m.GetType().Name;
                counts[n] = counts.TryGetValue(n, out int c) ? c + 1 : 1;
            }
            string[] interest = { "PlayerInputHandler",
                                  "Moon1GodMode", "PlayerSpawner", "Moon1PlayerSetup", "InputProbeHUD",
                                  "RunInBackgroundGuard", "CameraController" };
            foreach (var n in interest)
                sb.AppendLine($"  {n}: {(counts.TryGetValue(n, out int c) ? c : 0)}");

            File.WriteAllText(OUT, sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"[DiagnosePlayerSetupFull] Wrote {sb.Length} chars to {OUT}");
            EditorUtility.DisplayDialog("Diagnostic written",
                $"Full diagnostic written to:\n{OUT}\n\nSize: {sb.Length} chars\n\nOpen that file in any text editor to read the whole thing.",
                "OK");
            // Auto-open
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(OUT));
        }
    }
}
#endif
