using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    public static class CleanAllScenes
    {
        public static void CleanAllScenesInProject()
        {
            Debug.Log("[CleanAllScenes] Scanning all scenes in project...");

            string[] scenePaths = new string[]
            {
                "Assets/_Project/Scenes/Boot.unity",
                "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity",
                "Assets/_Project/Scenes/UI_Overlay.unity"
            };

            int totalRemoved = 0;

            foreach (var scenePath in scenePaths)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    Debug.LogWarning($"[CleanAllScenes] Scene not found: {scenePath}");
                    continue;
                }

                Debug.Log($"[CleanAllScenes] Opening scene: {scenePath}");
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                var rootObjects = scene.GetRootGameObjects();
                int sceneRemoved = 0;

                foreach (var rootObj in rootObjects)
                {
                    sceneRemoved += CleanGameObjectRecursive(rootObj);
                }

                if (sceneRemoved > 0)
                {
                    Debug.Log($"[CleanAllScenes] Removed {sceneRemoved} missing scripts from {scene.name}");
                    EditorSceneManager.SaveScene(scene);
                    totalRemoved += sceneRemoved;
                }
            }

            Debug.Log($"[CleanAllScenes] ✓ Total removed: {totalRemoved} missing script references");
        }

        private static int CleanGameObjectRecursive(GameObject go)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

            if (count > 0)
            {
                Debug.Log($"[CleanAllScenes] Removing {count} missing scripts from: {go.name}");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            }

            int totalRemoved = count;

            foreach (Transform child in go.transform)
            {
                totalRemoved += CleanGameObjectRecursive(child.gameObject);
            }

            return totalRemoved;
        }
    }
}
