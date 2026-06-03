#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Tartaria/Input/DEEP DIAGNOSE Player Setup
    ///
    /// Walks every CharacterController in the active scene + lists every component
    /// on it + parents + scene-wide PlayerInputHandler /
    /// PlayerSpawner / Rigidbody count, plus Time.timeScale, InputSystem settings.
    /// Use BEFORE entering Play mode to spot what's competing with controller input.
    /// </summary>
    public static class DiagnosePlayerSetup
    {
        [MenuItem("Tartaria/7 Diagnose/Player Setup (dialog)", priority = 700)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Tartaria Player Setup Diagnostic ===\n");

            // Time.timeScale
            sb.AppendLine($"Time.timeScale = {Time.timeScale}");
            sb.AppendLine($"Application.runInBackground = {Application.runInBackground}");
            sb.AppendLine($"PlayerSettings.runInBackground = {PlayerSettings.runInBackground}");
#if ENABLE_INPUT_SYSTEM
            try
            {
                sb.AppendLine($"InputSystem.settings.backgroundBehavior = {UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior}");
                sb.AppendLine($"InputSystem.settings.editorInputBehaviorInPlayMode = {UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode}");
            }
            catch (System.Exception e) { sb.AppendLine("InputSystem settings read: " + e.Message); }
#endif
            sb.AppendLine();

            // Find all CharacterControllers in scene
            var ccs = Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
            sb.AppendLine($"CharacterController count in scene: {ccs.Length}");
            for (int i = 0; i < ccs.Length; i++)
            {
                var cc = ccs[i];
                sb.AppendLine($"\n--- CC[{i}] '{cc.gameObject.name}' ---");
                sb.AppendLine($"  pos = {cc.transform.position}");
                sb.AppendLine($"  enabled = {cc.enabled}  isGrounded = {cc.isGrounded}");
                sb.AppendLine($"  gameObject.activeInHierarchy = {cc.gameObject.activeInHierarchy}");
                sb.AppendLine($"  tag = '{cc.gameObject.tag}'  layer = {LayerMask.LayerToName(cc.gameObject.layer)}");

                var rb = cc.GetComponent<Rigidbody>();
                if (rb != null)
                    sb.AppendLine($"  ⚠ Rigidbody PRESENT: isKinematic={rb.isKinematic}, useGravity={rb.useGravity}, mass={rb.mass}");

                sb.AppendLine($"  Components on this GameObject:");
                var allMBs = cc.GetComponents<Component>();
                foreach (var m in allMBs)
                {
                    if (m == null) { sb.AppendLine($"    - <MISSING SCRIPT>"); continue; }
                    var mb = m as MonoBehaviour;
                    string enState = mb != null ? (mb.enabled ? "ON" : "OFF") : "-";
                    sb.AppendLine($"    - {m.GetType().Name}  ({enState})");
                }
            }

            // Find ALL PlayerInputHandler instances (retired bypass drivers Moon1HardOverrideDriver/Moon1GodMode/SimplePlayerDriver no longer scanned -- deleted P4.L5)
            sb.AppendLine();
            var allMBs2 = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int phiCount = 0;
            foreach (var m in allMBs2)
            {
                if (m == null) continue;
                string n = m.GetType().Name;
                if (n == "PlayerInputHandler") phiCount++;
            }
            sb.AppendLine($"PlayerInputHandler instances: {phiCount}");

            // Player tag GameObject
            var pTag = GameObject.FindGameObjectWithTag("Player");
            sb.AppendLine();
            if (pTag != null)
            {
                sb.AppendLine($"Player-tagged GO: '{pTag.name}' at {pTag.transform.position}");
                sb.AppendLine($"  Has CC: {pTag.GetComponent<CharacterController>() != null}");
                sb.AppendLine($"  Has Rigidbody: {pTag.GetComponent<Rigidbody>() != null}");
            }
            else sb.AppendLine("No GameObject with tag 'Player' found");

            string full = sb.ToString();
            Debug.Log("[DiagnosePlayerSetup]\n" + full);
            EditorUtility.DisplayDialog("Player Setup Diagnostic",
                full.Length > 1500 ? full.Substring(0, 1500) + "\n... (full in console)" : full,
                "OK");
        }
    }
}
#endif
