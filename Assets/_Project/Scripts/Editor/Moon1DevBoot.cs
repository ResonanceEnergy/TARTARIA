#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor {
    public static class Moon1DevBoot {
        const string ScenePath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";

        [MenuItem("Tartaria/0 ★ MASTER/Dev Boot Direct To Echohaven", priority = 1)]
        public static void DevBoot() {
            if (EditorApplication.isPlaying) {
                Debug.LogWarning("[Moon1DevBoot] Already in Play mode — Stop first.");
                return;
            }

            Debug.Log("[Moon1DevBoot] Step 1/6 — Open Echohaven scene");
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (!scene.IsValid()) {
                Debug.LogError($"[Moon1DevBoot] Failed to open scene at '{ScenePath}'");
                return;
            }

            Debug.Log("[Moon1DevBoot] Step 2/6 — Bootstrap All Moon 1 Systems");
            InvokeMenu("Tartaria/0 ★ MASTER/Bootstrap All Moon 1 Systems");

            Debug.Log("[Moon1DevBoot] Step 3/6 — Force All Spawn Refs To (0,2,15)");
            InvokeMenu("Tartaria/8 Fix/Force All Spawn Refs To (0,2,15)");

            Debug.Log("[Moon1DevBoot] Step 4/6 — Wire ALL Scene Prefab Refs");
            InvokeMenu("Tartaria/0 ★ MASTER/Wire ALL Scene Prefab Refs (full sweep, Blender-only)");

            Debug.Log("[Moon1DevBoot] Step 5/6 — Save scene");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[Moon1DevBoot] Step 6/6 — Enter Play");
            EditorApplication.delayCall += () => { EditorApplication.isPlaying = true; };
        }

        static void InvokeMenu(string menuPath) {
            bool ok = EditorApplication.ExecuteMenuItem(menuPath);
            if (!ok) Debug.LogError($"[Moon1DevBoot] Menu not found or failed to execute: '{menuPath}'");
        }
    }
}
#endif
