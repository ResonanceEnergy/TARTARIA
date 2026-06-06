#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Mac autonomous loop verification harness.
    ///
    /// Called by tools/local-llm/mac/run_tickets.sh after each batch of
    /// LLM-applied tickets, via:
    ///   Unity -batchmode -projectPath . -executeMethod Tartaria.Editor.AutoLoop.RunSmokeShot -force-metal -quit
    ///
    /// Per Unity 6.4 manual + verified pattern:
    ///   - Do NOT pass -nographics on Mac, otherwise ScreenCapture/Camera.Render
    ///     produces black frames (Unity Discussions #754535).
    ///   - -force-metal selects Apple Silicon's native GPU API.
    ///
    /// Saves PNG to Logs/smoke-shots/shot_YYYYMMDD_HHMMSS.png so the orchestrator
    /// can rotate-into-doc or surface to NATRIX after run.
    /// </summary>
    public static class AutoLoop
    {
        const string SCENE_PATH = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
        const int W = 1920, H = 1080;

        public static void RunSmokeShot()
        {
            try
            {
                // Open the canonical Moon 1 scene
                EditorSceneManager.OpenScene(SCENE_PATH);

                var cam = UnityEngine.Camera.main ?? Object.FindAnyObjectByType<UnityEngine.Camera>();
                if (cam == null)
                {
                    Debug.LogError("[AutoLoop] No camera in scene — cannot capture.");
                    EditorApplication.Exit(2);
                    return;
                }

                // Render → texture → PNG
                var rt = new RenderTexture(W, H, 24);
                cam.targetTexture = rt;
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(W, H, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, W, H), 0, 0);
                tex.Apply();

                string outPath = Path.Combine(
                    Application.dataPath, "..",
                    "Logs", "smoke-shots",
                    $"shot_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                File.WriteAllBytes(outPath, tex.EncodeToPNG());

                // Cleanup
                cam.targetTexture = null;
                RenderTexture.active = prev;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);

                Debug.Log($"[AutoLoop] Smoke shot written: {outPath}");
                EditorApplication.Exit(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AutoLoop] RunSmokeShot threw: {e}");
                EditorApplication.Exit(3);
            }
        }

        // Optional: longer flow that enters Play mode, ticks N frames, then captures.
        // Wired but not invoked by default — call via:
        //   -executeMethod Tartaria.Editor.AutoLoop.RunPlayModeShot
        public static void RunPlayModeShot()
        {
            EditorSceneManager.OpenScene(SCENE_PATH);
            EditorApplication.EnterPlaymode();
            // Yield N frames then capture would require a coroutine via EditorApplication.update.
            // Left as TODO when NATRIX needs gameplay-state shots.
        }
    }
}
#endif
