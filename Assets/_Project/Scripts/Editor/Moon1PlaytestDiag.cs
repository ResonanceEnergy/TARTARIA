#if UNITY_EDITOR
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Tartaria.Editor
{
    /// <summary>
    /// Inspect Moon 1 scene state — find PlayerSpawn vs PlayerSpawner vs Player,
    /// detect duplicate hierarchy entries, locate camera rig, report spawn position.
    /// Run from Editor (not Play mode) to see what's wired pre-Play.
    /// </summary>
    public static class Moon1PlaytestDiag
    {
        [MenuItem("Tartaria/7 Diagnose/Player + Camera State (Moon 1)", priority = 760)]
        public static void Run()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MOON 1 PLAYTEST DIAGNOSTIC ===\n");

            // 1. Spawn points
            sb.AppendLine("--- Spawn objects in scene ---");
            FindAndLog(sb, "PlayerSpawn");
            FindAndLog(sb, "PlayerSpawner");
            FindAndLog(sb, "_SpawnPlatform");
            FindAndLog(sb, "Player");
            FindAndLog(sb, "Milo");

            // 2. Camera rig
            sb.AppendLine("\n--- Camera objects ---");
            var cams = Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None);
            sb.AppendLine("Camera count: " + cams.Length);
            foreach (var c in cams)
            {
                sb.AppendLine("  " + c.name + " @ " + c.transform.position +
                              " rot " + c.transform.eulerAngles + " enabled=" + c.enabled);
            }

            // 3. Duplicate detection
            sb.AppendLine("\n--- Duplicate top-level GameObjects ---");
            var counts = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (!counts.ContainsKey(root.name)) counts[root.name] = 0;
                counts[root.name]++;
            }
            foreach (var kvp in counts)
            {
                if (kvp.Value > 1) sb.AppendLine("  DUP " + kvp.Value + "x " + kvp.Key);
            }

            // 4. Player prefab on disk
            sb.AppendLine("\n--- Player.prefab on disk ---");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/Player.prefab");
            if (prefab == null) sb.AppendLine("  MISSING");
            else
            {
                sb.AppendLine("  Found: " + prefab.name);
                var cc = prefab.GetComponent<CharacterController>();
                sb.AppendLine("  CharacterController: " + (cc != null ? "yes" : "NO"));
                var rb = prefab.GetComponent<Rigidbody>();
                sb.AppendLine("  Rigidbody: " + (rb != null ? "yes" : "no"));
                var anims = prefab.GetComponent<Animator>();
                sb.AppendLine("  Animator: " + (anims != null ? "yes" : "no"));
            }

            // 5. Auto-bootstrap candidates
            sb.AppendLine("\n--- Auto-bootstrap class files present ---");
            string[] classes = { "Moon1Braziers", "Moon1EnvironmentDetail", "Moon1MudPoolPuzzle",
                                 "Moon1AnastasiaRocker", "Moon1VillagerAmbient", "Moon1CombatDirector",
                                 "Moon1AudioAtmosphere", "Moon1CinematicMoments", "Moon1ProgressPersistence",
                                 "QuestObjectiveTrackerUI" };
            foreach (var n in classes)
            {
                var t = System.Type.GetType("Tartaria.Integration." + n + ", Tartaria.Integration")
                      ?? System.Type.GetType("Tartaria.UI." + n + ", Tartaria.UI");
                sb.AppendLine("  " + (t != null ? "OK  " : "MISS ") + n);
            }

            Debug.Log(sb.ToString());
            EditorUtility.DisplayDialog("Playtest Diag", sb.ToString(), "OK");
        }

        static void FindAndLog(StringBuilder sb, string name)
        {
            var hits = new System.Collections.Generic.List<GameObject>();
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name == name) hits.Add(go);
            }
            if (hits.Count == 0) sb.AppendLine("  " + name + ": NONE");
            else
            {
                sb.AppendLine("  " + name + " (" + hits.Count + ")");
                foreach (var h in hits)
                    sb.AppendLine("    @ " + h.transform.position + "  active=" + h.activeInHierarchy);
            }
        }
    }
}
#endif
