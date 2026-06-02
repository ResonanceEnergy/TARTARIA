#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tartaria.UI;

namespace Tartaria.Editor
{
    /// <summary>
    /// Moon1BuildCreditsScene — authors Assets/_Project/Scenes/Credits.unity from scratch
    /// with a fullscreen Canvas + ScrollRect + TextMeshProUGUI + CreditsScroll component,
    /// AND copies docs/credits/credits_roll.md → Assets/_Project/Resources/credits_roll.txt
    /// (the runtime asset CreditsScroll loads via Resources.Load).
    ///
    /// Pipeline: edit docs/credits/credits_roll.md → run this menu → scene + txt updated.
    /// Per CLAUDE.md no-stubs mandate: everything wired, no TODO bodies.
    /// </summary>
    public static class Moon1BuildCreditsScene
    {
        private const string ScenePath = "Assets/_Project/Scenes/Credits.unity";
        private const string ResourcesDir = "Assets/_Project/Resources";
        private const string ResourcesTxtPath = "Assets/_Project/Resources/credits_roll.txt";
        private const string CreditsSourceRelative = "docs/credits/credits_roll.md";

        [MenuItem("Tartaria/UI/Build Credits Scene", priority = 200)]
        public static void Run()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Build Credits Scene",
                    "Stop Play mode first, then run again.", "OK");
                return;
            }

            // 1. Copy source-of-truth .md → Resources/credits_roll.txt
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError("[Moon1BuildCreditsScene] Could not resolve project root from Application.dataPath.");
                return;
            }

            string sourcePath = Path.Combine(projectRoot, CreditsSourceRelative).Replace('\\', '/');
            if (!File.Exists(sourcePath))
            {
                Debug.LogError($"[Moon1BuildCreditsScene] Source credits file missing: '{sourcePath}'. " +
                               $"Create '{CreditsSourceRelative}' before running this menu.");
                EditorUtility.DisplayDialog("Build Credits Scene",
                    $"Source file missing:\n{sourcePath}\n\nCreate docs/credits/credits_roll.md first.", "OK");
                return;
            }

            if (!Directory.Exists(ResourcesDir))
            {
                Directory.CreateDirectory(ResourcesDir);
                Debug.Log($"[Moon1BuildCreditsScene] Created Resources directory: {ResourcesDir}");
            }

            string content = File.ReadAllText(sourcePath);
            File.WriteAllText(ResourcesTxtPath, content);
            AssetDatabase.ImportAsset(ResourcesTxtPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Moon1BuildCreditsScene] Copied '{sourcePath}' -> '{ResourcesTxtPath}' ({content.Length} chars)");

            // 2. Build / overwrite the Credits scene.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // -- Camera (orthographic UI cam, dark background)
            var camGo = new GameObject("Credits Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.04f, 1f);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.cullingMask = ~0;
            cam.tag = "MainCamera";
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = new Vector3(0, 0, -10);

            // -- EventSystem (so UI input works even though we don't need clicks)
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // -- Canvas
            var canvasGo = new GameObject("CreditsCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // -- Background image (full screen, dark)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.02f, 0.02f, 0.04f, 1f);
            bgImg.raycastTarget = false;

            // -- Viewport (the "window" the credits scroll behind)
            var viewportGo = new GameObject("Viewport");
            viewportGo.transform.SetParent(canvasGo.transform, false);
            var viewportRect = viewportGo.AddComponent<RectTransform>();
            // Take full screen
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImg = viewportGo.AddComponent<Image>();
            viewportImg.color = new Color(0, 0, 0, 0.01f); // nearly transparent — needed for RectMask2D
            viewportImg.raycastTarget = false;
            viewportGo.AddComponent<RectMask2D>();

            // -- Content (the scrolling text container, anchored to bottom of viewport)
            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(1f, 0f);
            contentRect.pivot = new Vector2(0.5f, 0f);
            contentRect.anchoredPosition = new Vector2(0f, 0f); // starts at bottom; CreditsScroll moves up
            contentRect.sizeDelta = new Vector2(0f, 4000f); // tall enough for the entire roll; will be resized in Start

            // -- Text (TMP)
            var textGo = new GameObject("CreditsText");
            textGo.transform.SetParent(contentGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            // 200px side margins at 1920 ref width
            textRect.offsetMin = new Vector2(200f, 0f);
            textRect.offsetMax = new Vector2(-200f, 0f);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "Credits loading...";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 36f;
            tmp.color = new Color(0.92f, 0.92f, 1f, 1f);
            tmp.enableWordWrapping = true;
            tmp.richText = true;
            tmp.raycastTarget = false;
            tmp.margin = new Vector4(0f, 20f, 0f, 20f);

            // -- CreditsScroll component on the canvas
            var scrollComp = canvasGo.AddComponent<CreditsScroll>();
            scrollComp.WireReferences(contentRect, tmp, viewportRect);

            // 3. Save the scene.
            bool ok = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!ok)
            {
                Debug.LogError($"[Moon1BuildCreditsScene] Failed to save scene at '{ScenePath}'.");
                return;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 4. Make sure scene is in Build Settings (so SceneManager.LoadScene by name works
            //    when MainMenu wants to transition into Credits, and so the credits scene can
            //    load MainMenu by name at end-of-roll).
            EnsureSceneInBuildSettings(ScenePath);

            Debug.Log($"[Moon1BuildCreditsScene] Credits scene built at '{ScenePath}' " +
                      $"with Canvas + Viewport + RectMask2D + Content + TextMeshProUGUI + CreditsScroll. " +
                      $"Runtime asset: {ResourcesTxtPath}");

            EditorUtility.DisplayDialog("Build Credits Scene",
                $"OK.\n\nScene: {ScenePath}\nResources: {ResourcesTxtPath}\n\n" +
                $"Edit docs/credits/credits_roll.md and re-run this menu to refresh.",
                "OK");
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    if (!scenes[i].enabled)
                    {
                        scenes[i].enabled = true;
                        EditorBuildSettings.scenes = scenes;
                        Debug.Log($"[Moon1BuildCreditsScene] Re-enabled existing build settings entry for '{scenePath}'.");
                    }
                    return;
                }
            }
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            System.Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
            Debug.Log($"[Moon1BuildCreditsScene] Added '{scenePath}' to Build Settings.");
        }
    }
}
#endif
