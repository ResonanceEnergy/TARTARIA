using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Removes all missing MonoBehaviour script references from the active scene.
    /// Unity silently fails to enter Play mode when too many missing script refs exist.
    /// Run via Menu: Tartaria → Cleanup Missing Scripts
    /// </summary>
    public static class CleanupMissingScripts
    {
        [MenuItem("Tartaria/Cleanup Missing Scripts in Active Scene", priority = 100)]
        public static void CleanupActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[CleanupMissingScripts] No active scene loaded!");
                return;
            }

            Debug.Log($"[CleanupMissingScripts] Scanning scene: {scene.name}");

            int removedCount = 0;
            var rootObjects = scene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                removedCount += CleanupGameObject(root);
            }

            if (removedCount > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[CleanupMissingScripts] ✓ Removed {removedCount} missing script references from {scene.name}");
                Debug.Log($"[CleanupMissingScripts] Scene saved. Reload to apply changes.");
            }
            else
            {
                Debug.Log($"[CleanupMissingScripts] No missing scripts found in {scene.name}");
            }
        }

        [MenuItem("Tartaria/Cleanup Missing Scripts in ALL Scenes", priority = 101)]
        public static void CleanupAllScenes()
        {
            CleanupAllScenesBatch();
        }

        /// <summary>
        /// Batch-mode compatible cleanup - callable via -executeMethod from command line.
        /// </summary>
        public static void CleanupAllScenesBatch()
        {
            string[] scenePaths = new string[]
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity",
                "Assets/_Project/Scenes/UI_Overlay.unity"
            };

            int totalRemoved = 0;

            foreach (var path in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int removedCount = 0;
                var rootObjects = scene.GetRootGameObjects();

                foreach (var root in rootObjects)
                {
                    removedCount += CleanupGameObject(root);
                }

                if (removedCount > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[CleanupMissingScripts] ✓ {scene.name}: Removed {removedCount} missing script refs");
                }

                totalRemoved += removedCount;
            }

            Debug.Log($"[CleanupMissingScripts] ✓✓✓ Total: Removed {totalRemoved} missing script references across all scenes");
            
            if (totalRemoved > 0)
            {
                Debug.Log($"[CleanupMissingScripts] Scenes cleaned. Restart Unity to apply changes.");
            }
        }

        static int CleanupGameObject(GameObject obj)
        {
            int count = 0;

            // Get all components including missing ones
            var components = obj.GetComponents<Component>();
            var serializedObject = new SerializedObject(obj);
            var prop = serializedObject.FindProperty("m_Component");

            // Remove null components (missing scripts)
            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i] == null)
                {
                    // Found a missing script reference
                    var componentProp = prop.GetArrayElementAtIndex(i);
                    componentProp.DeleteArrayElementAtIndex(0); // Delete the reference
                    count++;
                }
            }

            if (count > 0)
            {
                serializedObject.ApplyModifiedProperties();
                Debug.Log($"[CleanupMissingScripts]   - {obj.name}: Removed {count} missing script(s)");
            }

            // Recurse into children
            foreach (Transform child in obj.transform)
            {
                count += CleanupGameObject(child.gameObject);
            }

            return count;
        }
    }
}
