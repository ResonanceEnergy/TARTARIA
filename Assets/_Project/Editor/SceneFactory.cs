using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Editor
{
    /// <summary>
    /// Creates the Boot and UI_Overlay scenes required by the vertical slice.
    /// Boot: initialization, ECS world setup, splash → loads Echohaven.
    /// UI_Overlay: additive scene with HUD Canvas, dialogue, notifications.
    ///
    /// Menu: Tartaria > Create Missing Scenes
    /// </summary>
    public static class SceneFactory
    {
        const string ScenePath = "Assets/_Project/Scenes";

        [MenuItem("Tartaria/Create Missing Scenes", false, 3)]
        public static void CreateAllMissingScenes()
        {
            int created = 0;
            if (CreateBootScene()) created++;
            if (CreateUIOverlayScene()) created++;

            if (created > 0)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                UpdateBuildSettings();
            }

            Debug.Log($"[Tartaria] SceneFactory complete — {created} scene(s) created.");
        }

        [MenuItem("Tartaria/Create Missing Scenes/Boot", false, 4)]
        public static bool CreateBootScene()
        {
            string path = $"{ScenePath}/Boot.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                Debug.Log("[Tartaria] Boot.unity already exists, skipping.");
                return false;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Camera (splash) ──
            var camGO = new GameObject("BootCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);
            cam.orthographic = true;
            camGO.AddComponent<AudioListener>();

            // ── ECS Bootstrap ──
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<Core.GameBootstrap>();

            // ── Persistent Managers (DontDestroyOnLoad) ──
            var persist = new GameObject("--- PERSISTENT ---");

            var audioMgr = new GameObject("AudioManager");
            audioMgr.transform.SetParent(persist.transform);
            audioMgr.AddComponent<Audio.AudioManager>();

            var saveMgr = new GameObject("SaveManager");
            saveMgr.transform.SetParent(persist.transform);
            saveMgr.AddComponent<Save.SaveManager>();

            var accessMgr = new GameObject("AccessibilityManager");
            accessMgr.transform.SetParent(persist.transform);
            accessMgr.AddComponent<UI.AccessibilityManager>();

            var hapticMgr = new GameObject("HapticFeedbackManager");
            hapticMgr.transform.SetParent(persist.transform);
            hapticMgr.AddComponent<Input.HapticFeedbackManager>();

            // ── Scene Loader ──
            var loader = new GameObject("SceneLoader");
            loader.AddComponent<Core.SceneLoader>();

            // ── Light ──
            var light = new GameObject("DirectionalLight");
            var l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.9f, 0.85f, 0.75f);
            l.intensity = 0.5f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Tartaria] Created {path}");
            return true;
        }

        [MenuItem("Tartaria/Create Missing Scenes/UI Overlay", false, 5)]
        public static bool CreateUIOverlayScene()
        {
            string path = $"{ScenePath}/UI_Overlay.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
            {
                Debug.Log("[Tartaria] UI_Overlay.unity already exists, skipping.");
                return false;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // ── UI Canvas ──
            var canvasGO = new GameObject("GameCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // ── HUD root ──
            var hud = CreateUIPanel(canvasGO.transform, "HUD", true);

            // RS Gauge placeholder
            var rsGauge = CreateUIPanel(hud.transform, "RSGauge", false);
            var rsRT = rsGauge.GetComponent<RectTransform>();
            rsRT.anchorMin = new Vector2(0f, 1f);
            rsRT.anchorMax = new Vector2(0f, 1f);
            rsRT.pivot = new Vector2(0f, 1f);
            rsRT.anchoredPosition = new Vector2(20f, -20f);
            rsRT.sizeDelta = new Vector2(200f, 40f);

            // Dialogue panel placeholder
            var dialogue = CreateUIPanel(hud.transform, "DialoguePanel", false);
            var dlgRT = dialogue.GetComponent<RectTransform>();
            dlgRT.anchorMin = new Vector2(0.1f, 0f);
            dlgRT.anchorMax = new Vector2(0.9f, 0f);
            dlgRT.pivot = new Vector2(0.5f, 0f);
            dlgRT.anchoredPosition = new Vector2(0f, 30f);
            dlgRT.sizeDelta = new Vector2(0f, 100f);

            // Tutorial prompt placeholder
            var tutorial = CreateUIPanel(hud.transform, "TutorialPrompt", false);
            var tutRT = tutorial.GetComponent<RectTransform>();
            tutRT.anchorMin = new Vector2(0.5f, 0.7f);
            tutRT.anchorMax = new Vector2(0.5f, 0.7f);
            tutRT.pivot = new Vector2(0.5f, 0.5f);
            tutRT.sizeDelta = new Vector2(400f, 60f);

            // Notification area
            CreateUIPanel(hud.transform, "NotificationArea", false);

            // ── Pause Menu root (inactive by default) ──
            var pause = CreateUIPanel(canvasGO.transform, "PauseMenu", false);
            pause.SetActive(false);

            // ── Managers ──
            var managers = new GameObject("--- UI MANAGERS ---");
            managers.AddComponent<UI.UIManager>();
            managers.AddComponent<UI.HUDController>();

            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Tartaria] Created {path}");
            return true;
        }

        static GameObject CreateUIPanel(Transform parent, string name, bool stretch)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            if (stretch)
            {
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }
            return go;
        }

        static void UpdateBuildSettings()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(
                EditorBuildSettings.scenes);

            TryAddScene(scenes, $"{ScenePath}/Boot.unity", 0);
            TryAddScene(scenes, $"{ScenePath}/Echohaven_VerticalSlice.unity", 1);
            TryAddScene(scenes, $"{ScenePath}/UI_Overlay.unity", 2);

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[Tartaria] Build settings updated with scene order: Boot → Echohaven → UI_Overlay");
        }

        static void TryAddScene(System.Collections.Generic.List<EditorBuildSettingsScene> scenes,
            string path, int preferredIndex)
        {
            // Don't add duplicates
            foreach (var s in scenes)
                if (s.path == path) return;

            var entry = new EditorBuildSettingsScene(path, true);
            if (preferredIndex >= 0 && preferredIndex <= scenes.Count)
                scenes.Insert(preferredIndex, entry);
            else
                scenes.Add(entry);
        }
    }
}
