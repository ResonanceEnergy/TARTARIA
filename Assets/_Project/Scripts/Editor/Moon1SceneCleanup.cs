#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon 1 scene cleanup pass:
    ///   1. Strip every missing-script component off any GameObject named "Moon1_Systems".
    ///   2. Delete known placeholder GameObjects (CrystalSpire_Placeholder, HarmonicFountain_Placeholder,
    ///      StarDome_Placeholder) that duplicate the real Building_echohaven_* prefab instances and
    ///      cause z-fighting + ambiguous tuning triggers.
    ///   3. Save the scene.
    ///
    /// Complements <see cref="CleanMissingScripts"/> (which auto-scans on Editor load but does NOT
    /// know about Moon 1 placeholders). This menu is the manual force-run + placeholder purger.
    /// Idempotent — safe to invoke repeatedly.
    ///
    /// Menu: Tartaria/8 Fix/Moon 1 Scene Cleanup (Missing Refs + Placeholders)
    /// </summary>
    public static class Moon1SceneCleanup
    {
        const string SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const string TARGET_GO_NAME = "Moon1_Systems";

        static readonly string[] PLACEHOLDER_NAMES =
        {
            "CrystalSpire_Placeholder",
            "HarmonicFountain_Placeholder",
            "StarDome_Placeholder",
        };

        [MenuItem("Tartaria/8 Fix/Moon 1 Scene Cleanup (Missing Refs + Placeholders)", priority = 805)]
        public static void Run()
        {
            // --- 1. Ensure the Echohaven scene is loaded ----------------------------------
            Scene scene = EnsureSceneLoaded();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog(
                    "Moon 1 Scene Cleanup",
                    $"Could not load scene at:\n{SCENE_PATH}",
                    "OK");
                return;
            }

            var log = new StringBuilder();
            log.AppendLine($"[Moon1SceneCleanup] Scene: {scene.name}");

            // --- 2. Strip missing-script components from every Moon1_Systems GameObject --
            var perGoRemoved = new List<(string path, int removed)>();
            int totalMissingRemoved = 0;

            var moon1SystemsGos = FindAllByExactName(scene, TARGET_GO_NAME);
            if (moon1SystemsGos.Count == 0)
            {
                log.AppendLine($"[Moon1SceneCleanup] No GameObject named '{TARGET_GO_NAME}' found.");
            }

            foreach (var go in moon1SystemsGos)
            {
                int before = CountMissingScripts(go);
                if (before > 0)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    int after = CountMissingScripts(go);
                    int removed = before - after;
                    totalMissingRemoved += removed;
                    perGoRemoved.Add((GetFullPath(go), removed));
                    log.AppendLine($"  - {GetFullPath(go)}: removed {removed} missing component(s).");
                }
                else
                {
                    perGoRemoved.Add((GetFullPath(go), 0));
                    log.AppendLine($"  - {GetFullPath(go)}: no missing components.");
                }
            }

            // --- 3. Delete placeholder GameObjects ----------------------------------------
            var deletedPlaceholders = new List<string>();
            var ambiguityNotes = new List<string>();

            foreach (string targetName in PLACEHOLDER_NAMES)
            {
                // First try exact name match.
                var exactMatches = FindAllByExactName(scene, targetName);

                if (exactMatches.Count > 0)
                {
                    foreach (var go in exactMatches)
                    {
                        string path = GetFullPath(go);
                        Undo.DestroyObjectImmediate(go);
                        deletedPlaceholders.Add($"{targetName} (exact: {path})");
                        log.AppendLine($"  - DELETED placeholder (exact): {path}");
                    }
                }
                else
                {
                    // Fall back to case-insensitive substring match.
                    var substringMatches = FindAllBySubstring(scene, targetName);
                    if (substringMatches.Count == 0)
                    {
                        string note = $"'{targetName}' not found by exact or substring match.";
                        ambiguityNotes.Add(note);
                        log.AppendLine($"  - SKIP: {note}");
                    }
                    else
                    {
                        foreach (var go in substringMatches)
                        {
                            string path = GetFullPath(go);
                            Undo.DestroyObjectImmediate(go);
                            deletedPlaceholders.Add($"{targetName} (substring: {path})");
                            log.AppendLine($"  - DELETED placeholder (substring match for '{targetName}'): {path}");
                            ambiguityNotes.Add($"'{targetName}' matched via substring on '{go.name}'.");
                        }
                    }
                }
            }

            // --- 4. Save scene -----------------------------------------------------------
            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene);
            log.AppendLine($"[Moon1SceneCleanup] Saved: {saved}");

            int totalChanges = totalMissingRemoved + deletedPlaceholders.Count;

            // --- 5. Report ---------------------------------------------------------------
            Debug.Log(log.ToString());

            var summary = new StringBuilder();
            summary.AppendLine($"Scene: {scene.name}");
            summary.AppendLine();
            summary.AppendLine($"Missing-script components removed: {totalMissingRemoved}");
            if (perGoRemoved.Count == 0)
            {
                summary.AppendLine($"  (no '{TARGET_GO_NAME}' GameObjects found)");
            }
            else
            {
                foreach (var entry in perGoRemoved)
                    summary.AppendLine($"  - {entry.path}: {entry.removed}");
            }
            summary.AppendLine();
            summary.AppendLine($"Placeholders deleted: {deletedPlaceholders.Count}");
            foreach (string p in deletedPlaceholders)
                summary.AppendLine($"  - {p}");

            if (ambiguityNotes.Count > 0)
            {
                summary.AppendLine();
                summary.AppendLine("Notes:");
                foreach (string n in ambiguityNotes)
                    summary.AppendLine($"  - {n}");
            }

            summary.AppendLine();
            summary.AppendLine($"Total changes: {totalChanges}");
            summary.AppendLine($"Scene saved: {saved}");

            EditorUtility.DisplayDialog("Moon 1 Scene Cleanup", summary.ToString(), "OK");
        }

        // -----------------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------------

        static Scene EnsureSceneLoaded()
        {
            Scene active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.isLoaded && active.path == SCENE_PATH)
                return active;

            // Check if already open in another slot.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.path == SCENE_PATH && s.isLoaded)
                {
                    SceneManager.SetActiveScene(s);
                    return s;
                }
            }

            // Prompt to save any dirty scenes before opening.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return default;

            return EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        static List<GameObject> FindAllByExactName(Scene scene, string exactName)
        {
            var results = new List<GameObject>();
            foreach (var root in scene.GetRootGameObjects())
                CollectByExactName(root, exactName, results);
            return results;
        }

        static void CollectByExactName(GameObject go, string exactName, List<GameObject> results)
        {
            if (go.name == exactName)
                results.Add(go);
            foreach (Transform child in go.transform)
                CollectByExactName(child.gameObject, exactName, results);
        }

        static List<GameObject> FindAllBySubstring(Scene scene, string needle)
        {
            var results = new List<GameObject>();
            string needleLower = needle.ToLowerInvariant();
            foreach (var root in scene.GetRootGameObjects())
                CollectBySubstring(root, needleLower, results);
            return results;
        }

        static void CollectBySubstring(GameObject go, string needleLower, List<GameObject> results)
        {
            if (go.name.ToLowerInvariant().Contains(needleLower))
                results.Add(go);
            foreach (Transform child in go.transform)
                CollectBySubstring(child.gameObject, needleLower, results);
        }

        static int CountMissingScripts(GameObject go)
        {
            int count = 0;
            var components = go.GetComponents<Component>();
            foreach (var c in components)
                if (c == null) count++;
            return count;
        }

        static string GetFullPath(GameObject go)
        {
            if (go == null) return "<null>";
            if (go.transform.parent == null) return go.name;
            return GetFullPath(go.transform.parent.gameObject) + "/" + go.name;
        }
    }
}
#endif
