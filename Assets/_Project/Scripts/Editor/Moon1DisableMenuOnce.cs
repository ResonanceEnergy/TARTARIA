#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// R112 — One-shot helper to permanently disable the main menu in the scene
    /// so NATRIX can play without it blocking. Saves the scene after edit.
    ///
    /// MENU: Tartaria/9 Debug/R112 — Disable Main Menu Canvas + Save Scene
    /// </summary>
    public static class Moon1DisableMenuOnce
    {
        [MenuItem("Tartaria/9 Debug/R112 - Disable Main Menu Canvas + Save Scene")]
        public static void DisableMenuAndSave()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded || !scene.name.Contains("Echohaven"))
            {
                EditorUtility.DisplayDialog("R112", "Open Echohaven_VerticalSlice.unity first.", "OK");
                return;
            }

            int disabled = 0;
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in allCanvases)
            {
                if (c == null) continue;
                var n = c.gameObject.name;
                if (n.ToLower().Contains("mainmenu") || n.ToLower().Contains("titlescreen") || n == "MenuCanvas")
                {
                    c.gameObject.SetActive(false);
                    disabled++;
                    Debug.Log($"[R112] Disabled menu canvas: {n}");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("R112",
                $"Disabled {disabled} menu canvas(es).\nScene saved.\n\nNow hit Play.",
                "OK");
        }
    }
}
#endif
