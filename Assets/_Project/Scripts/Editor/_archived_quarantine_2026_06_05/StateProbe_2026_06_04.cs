// Auto-generated verification probe — 2026-06-04 audit-and-fix sweep.
// Reports Player visual nesting, NPC Animator wiring, MudGolem prefab health, music layers.

using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tartaria.Editor
{
    public static class StateProbe_2026_06_04
    {
        [MenuItem("Tartaria/9 Debug/State Probe 2026-06-04")]
        public static void RunProbe()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== STATE PROBE 2026-06-04 ===");

            // Player visual
            var player = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab");
            if (player == null)
            {
                sb.AppendLine("Player.prefab: NOT FOUND");
            }
            else
            {
                var visual = player.transform.Find("_CharacterVisual");
                sb.AppendLine("Player _CharacterVisual children: " + (visual != null ? visual.childCount.ToString() : "NULL"));
                if (visual != null)
                {
                    for (int i = 0; i < visual.childCount; i++)
                    {
                        var c = visual.GetChild(i);
                        var rends = c.GetComponentsInChildren<Renderer>(true).Length;
                        sb.AppendLine("  - " + c.name + " (renderers: " + rends + ")");
                    }
                }
            }

            // 4 NPC animators wired
            sb.AppendLine();
            sb.AppendLine("--- NPC Animator wiring ---");
            foreach (var n in new[] { "Milo", "Anastasia", "Lirael", "Cassian" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/" + n + ".prefab");
                if (prefab == null)
                {
                    sb.AppendLine(n + ": prefab NOT FOUND");
                    continue;
                }
                var an = prefab.GetComponentInChildren<Animator>();
                var col = prefab.GetComponentInChildren<CapsuleCollider>();
                string colStr = col != null
                    ? "h=" + col.height.ToString("F2") + " r=" + col.radius.ToString("F2") + " c=" + col.center.y.ToString("F2")
                    : "NONE";
                sb.AppendLine(n + ": ctrl=" + (an != null && an.runtimeAnimatorController != null ? an.runtimeAnimatorController.name : "null")
                    + " avatar=" + (an != null && an.avatar != null ? an.avatar.name : "null")
                    + " collider=" + colStr);
            }

            // MudGolem prefab AI
            sb.AppendLine();
            sb.AppendLine("--- MudGolem prefab ---");
            var mg = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/MudGolem.prefab");
            if (mg == null)
            {
                sb.AppendLine("MudGolem.prefab: NOT FOUND");
            }
            else
            {
                var comps = mg.GetComponents<MonoBehaviour>();
                sb.AppendLine("MudGolem tag=" + mg.tag + " renderers=" + mg.GetComponentsInChildren<Renderer>(true).Length + " MBs=" + comps.Length);
                foreach (var c in comps)
                {
                    sb.AppendLine("  MB: " + (c != null ? c.GetType().FullName : "<null/missing>"));
                }
            }

            // Music layers
            sb.AppendLine();
            sb.AppendLine("--- Music layers ---");
            foreach (var m in new[] { "ambient_layer1", "ambient_layer2", "ambient_layer3", "ambient_layer4" })
            {
                var c = Resources.Load<AudioClip>("Audio/Music/" + m);
                sb.AppendLine("  " + m + ": " + (c != null ? c.length.ToString("F1") + "s" : "NULL"));
            }

            // HUD_Root.prefab on disk
            sb.AppendLine();
            sb.AppendLine("--- HUD_Root.prefab ---");
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Resources/Prefabs/UI/HUD_Root.prefab");
            if (hud == null) hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/UI/HUD_Root.prefab");
            sb.AppendLine("HUD_Root.prefab: " + (hud != null ? ("EXISTS children=" + hud.transform.childCount + " path=" + AssetDatabase.GetAssetPath(hud)) : "MISSING"));

            var report = sb.ToString();
            Debug.Log(report);
            try
            {
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "STATE_PROBE_2026_06_04.txt"),
                    report);
                Debug.Log("[StateProbe] Wrote STATE_PROBE_2026_06_04.txt at project root.");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[StateProbe] Write failed: " + e.Message);
            }
        }
    }
}
