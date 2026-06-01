#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1AddSceneCamera — drops a Main Camera at a known-good 3rd-person view.
    /// Safe in Edit mode only.
    /// </summary>
    public static class Moon1AddSceneCamera
    {
        [MenuItem("Tartaria/8 Fix/Add + Position Main Camera (Echohaven 3rd-person)", priority = 90)]
        public static void Run()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Add Main Camera", "Stop Play first, then run again.", "OK");
                return;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                EditorUtility.DisplayDialog("Add Main Camera", "No active scene.", "OK");
                return;
            }

            // Find existing camera or create one
            GameObject cam = null;
            var existing = global::UnityEngine.Camera.main;
            if (existing != null)
            {
                cam = existing.gameObject;
            }
            else
            {
                var allCams = Object.FindObjectsByType<global::UnityEngine.Camera>(FindObjectsSortMode.None);
                if (allCams.Length > 0) cam = allCams[0].gameObject;
            }

            if (cam == null)
            {
                cam = new GameObject("Main Camera");
                cam.AddComponent<global::UnityEngine.Camera>();
                cam.AddComponent<AudioListener>();
            }

            cam.name = "Main Camera";
            cam.tag = "MainCamera";

            // 3rd-person overlook of Echohaven center
            cam.transform.position = new Vector3(0f, 12f, -18f);
            cam.transform.rotation = Quaternion.Euler(25f, 0f, 0f);

            var c = cam.GetComponent<global::UnityEngine.Camera>();
            c.clearFlags = CameraClearFlags.Skybox;
            c.fieldOfView = 60f;
            c.nearClipPlane = 0.3f;
            c.farClipPlane = 1500f;

            // Ensure AudioListener present
            if (cam.GetComponent<AudioListener>() == null)
                cam.AddComponent<AudioListener>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Selection.activeGameObject = cam;

            EditorUtility.DisplayDialog("Main Camera Restored",
                $"Camera at (0, 12, -18) rot (25, 0, 0).\nTagged: MainCamera.\nScene saved.\n\nHit Play.",
                "OK");
        }
    }
}
#endif
