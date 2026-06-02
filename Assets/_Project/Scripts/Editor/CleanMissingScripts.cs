using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Tartaria.Editor
{
    /// <summary>
    /// Finds and removes missing script references (broken MonoBehaviour GUIDs).
    /// Run via Menu: Tartaria → Clean Missing Scripts
    /// </summary>
    public static class CleanMissingScripts
    {
        [MenuItem("Tartaria/6 Scene Tools/Clean Missing Scripts", priority = 680)]
        static void FindAndCleanAll()
        {
            int totalRemoved = 0;

            // Clean current scene
            totalRemoved += CleanCurrentScene();

            // Clean prefabs in Assets
            totalRemoved += CleanPrefabs();

            Debug.Log($"[CleanMissingScripts] Removed {totalRemoved} missing script references");

            if (totalRemoved > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorSceneManager.SaveOpenScenes();
            }
        }

        static int CleanCurrentScene()
        {
            int removed = 0;
            Scene scene = SceneManager.GetActiveScene();

            if (!scene.isLoaded)
                return 0;

            Debug.Log($"[CleanMissingScripts] Scanning scene: {scene.name}");

            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                removed += CleanGameObject(root);
            }

            return removed;
        }

        static int CleanPrefabs()
        {
            int removed = 0;
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project" });

            foreach (string guid in prefabPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                int beforeCount = CountMissingScripts(prefab);
                if (beforeCount > 0)
                {
                    Debug.Log($"[CleanMissingScripts] Cleaning prefab: {path} ({beforeCount} missing)");

                    // Use PrefabUtility for safe prefab editing
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject instance = PrefabUtility.LoadPrefabContents(prefabPath);

                    int cleanedCount = CleanGameObject(instance);

                    if (cleanedCount > 0)
                    {
                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        removed += cleanedCount;
                    }

                    PrefabUtility.UnloadPrefabContents(instance);
                }
            }

            return removed;
        }

        static int CleanGameObject(GameObject go)
        {
            int removed = 0;

            // Check this GameObject
            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"[CleanMissingScripts] Removing missing script from: {GetFullPath(go)}", go);
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    removed++;
                    break; // RemoveMonoBehavioursWithMissingScript removes all at once
                }
            }

            // Recurse to children
            foreach (Transform child in go.transform)
            {
                removed += CleanGameObject(child.gameObject);
            }

            return removed;
        }

        static int CountMissingScripts(GameObject go)
        {
            int count = 0;

            Component[] components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c == null)
                    count++;
            }

            foreach (Transform child in go.transform)
            {
                count += CountMissingScripts(child.gameObject);
            }

            return count;
        }

        static string GetFullPath(GameObject go)
        {
            if (go.transform.parent == null)
                return go.name;
            return GetFullPath(go.transform.parent.gameObject) + "/" + go.name;
        }

        [InitializeOnLoadMethod]
        static void AutoCleanOnLoad()
        {
            // 2026-06-02 permanent fix per Unity Manual on EditorSceneManager:
            // The previous delayCall-only path called SaveOpenScenes() but the active
            // scene was sometimes not yet marked dirty, so the clean didn't persist
            // and the same warnings reappeared each session. Hardening to:
            // - Hook delayCall (startup) + sceneOpened (every subsequent open)
            // - Explicit MarkSceneDirty + SaveScene + SaveAssets to flush to disk
            EditorApplication.delayCall += () => CleanAndSave("startup");
            EditorSceneManager.sceneOpened -= OnSceneOpenedClean;
            EditorSceneManager.sceneOpened += OnSceneOpenedClean;
        }

        static void OnSceneOpenedClean(Scene scene, OpenSceneMode mode)
        {
            CleanAndSave($"sceneOpened:{scene.name}");
        }

        static void CleanAndSave(string trigger)
        {
            int removed = CleanCurrentScene();
            if (removed <= 0) return;
            Debug.Log($"[CleanMissingScripts] ({trigger}) Auto-cleaned {removed} missing scripts. Forcing scene persist to disk.");
            var scene = SceneManager.GetActiveScene();
            if (scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
