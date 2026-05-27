using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;

namespace Tartaria.Editor
{
    /// <summary>
    /// Emergency fix for missing script references in Echohaven scene.
    /// Run once to clean up placeholder/test objects blocking Play mode.
    /// </summary>
    public static class FixEchohavenMissingScripts
    {
        [MenuItem("Tartaria/FIX: Clean Echohaven Missing Scripts")]
        static void FixEchohavenNow()
        {
            Debug.Log("[FixEchohaven] Opening Echohaven scene...");

            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Echohaven_VerticalSlice.unity");

            int removed = 0;
            var allObjects = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject)
                .ToArray();

            Debug.Log($"[FixEchohaven] Scanning {allObjects.Length} GameObjects...");

            foreach (var go in allObjects)
            {
                // Remove missing scripts
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (count > 0)
                {
                    Debug.Log($"[FixEchohaven] Removing {count} missing scripts from '{go.name}'");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    removed += count;
                }
            }

            if (removed > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[FixEchohaven] ✓ Removed {removed} missing script references. Scene saved.");
            }
            else
            {
                Debug.Log("[FixEchohaven] No missing scripts found.");
            }

            AssetDatabase.Refresh();
        }
    }
}
