#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Tests
{
    /// <summary>
    /// Editor bootstrap for the QA Profiler_Baseline scene.
    /// Builds the scene programmatically (sun + ground plane 50x50 + PerformanceTest
    /// host + camera) — matches the Bootstrap_PlayerOnly pattern shipped last sprint.
    /// No binary .unity asset is committed; the scene is regenerated on demand and
    /// saved to Assets/_Project/Scenes/Tests/Profiler_Baseline.unity for the run.
    /// </summary>
    public static class Bootstrap_ProfilerBaseline
    {
        private const string SceneName = "Profiler_Baseline";
        private const string SceneFolder = "Assets/_Project/Scenes/Tests";
        private const string ScenePath = SceneFolder + "/" + SceneName + ".unity";

        [MenuItem("Tartaria/9 QA/Open Profiler_Baseline", false, 95)]
        public static void OpenProfilerBaseline()
        {
            var scene = BuildSceneInternal();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[Bootstrap_ProfilerBaseline] Scene built and saved: " + ScenePath);
        }

        [MenuItem("Tartaria/9 QA/Run Profiler Baseline", false, 96)]
        public static void RunProfilerBaseline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[Bootstrap_ProfilerBaseline] Editor is already entering/leaving play mode.");
                return;
            }

            var scene = BuildSceneInternal();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorApplication.isPlaying = true;
            Debug.Log("[Bootstrap_ProfilerBaseline] Entering play mode for: " + ScenePath);
        }

        private static Scene BuildSceneInternal()
        {
            EnsureSceneFolder();

            var scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            // Directional sun
            var sunGo = new GameObject("Directional Light");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = Color.white;
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ground plane 50x50 (Unity plane is 10x10 units → scale 5)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 5f);
            // URP-safe: set _BaseColor so the default lit shader renders mid-grey
            var groundMat = ground.GetComponent<MeshRenderer>().sharedMaterial;
            if (groundMat != null && groundMat.HasProperty("_BaseColor"))
            {
                // sharedMaterial mutation is fine for an editor-built throwaway scene
                groundMat.SetColor("_BaseColor", new Color(0.45f, 0.45f, 0.48f, 1f));
            }

            // PerformanceTest host (auto-runs via RuntimeInitializeOnLoadMethod, but
            // we add it explicitly so the GO is visible in the scene hierarchy)
            var perfGo = new GameObject("PerformanceTest");
            perfGo.AddComponent<PerformanceTest>();
            perfGo.transform.position = Vector3.zero;

            // Camera looking down on the walk circle
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 200f;
            camGo.transform.position = new Vector3(0f, 30f, -28f);
            camGo.transform.rotation = Quaternion.Euler(40f, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            // Scene.name is derived from the saved file path (Profiler_Baseline.unity),
            // which is what PerformanceTest's auto-bootstrap matches on.
            return scene;
        }

        private static void EnsureSceneFolder()
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                var parts = SceneFolder.Split('/');
                string acc = parts[0]; // "Assets"
                for (int i = 1; i < parts.Length; i++)
                {
                    string next = acc + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(acc, parts[i]);
                    }
                    acc = next;
                }
            }

            // Also make sure the OS folder exists (defensive for fresh clones)
            string osFolder = Path.Combine(Application.dataPath, "_Project/Scenes/Tests");
            if (!Directory.Exists(osFolder))
            {
                Directory.CreateDirectory(osFolder);
            }
        }
    }
}
#endif
