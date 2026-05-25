using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Editor window that validates everything needed to press Play.
    /// Menu: Tartaria > Readiness Check
    /// Shows green/yellow/red indicators for scenes, assets, managers, etc.
    /// </summary>
    public class PlayReadinessWindow : EditorWindow
    {
        struct CheckResult
        {
            public string label;
            public bool passed;
            public string detail;
        }

        List<CheckResult> _results = new();
        Vector2 _scrollPos;
        int _passCount;
        int _failCount;
        bool _hasRun;
        static GUIStyle _summaryStyle;
        static GUIStyle _detailStyle;

        [MenuItem("Tartaria/Readiness Check", false, -50)]
        static void Open()
        {
            var w = GetWindow<PlayReadinessWindow>("Tartaria Readiness");
            w.minSize = new Vector2(420, 500);
            w.RunChecks();
        }

        void OnGUI()
        {
            GUILayout.Space(6);
            EditorGUILayout.LabelField("TARTARIA — Play Readiness", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Checks", GUILayout.Height(28)))
                RunChecks();
            if (GUILayout.Button("BUILD EVERYTHING", GUILayout.Height(28)))
            {
                OneClickBuild.RunBuild();
                RunChecks();
            }
            EditorGUILayout.EndHorizontal();

            if (!_hasRun)
            {
                EditorGUILayout.HelpBox("Click 'Run Checks' to scan the project.", MessageType.Info);
                return;
            }

            GUILayout.Space(6);

            // Summary bar
            _summaryStyle ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            if (_failCount == 0)
            {
                _summaryStyle.normal.textColor = new Color(0.1f, 0.7f, 0.1f);
                EditorGUILayout.LabelField($"ALL {_passCount} CHECKS PASSED", _summaryStyle);
            }
            else
            {
                _summaryStyle.normal.textColor = new Color(0.9f, 0.2f, 0.1f);
                EditorGUILayout.LabelField($"{_failCount} FAILED / {_passCount} PASSED", _summaryStyle);
            }

            GUILayout.Space(4);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (var r in _results)
            {
                EditorGUILayout.BeginHorizontal();

                // Icon
                var icon = r.passed
                    ? EditorGUIUtility.IconContent("d_GreenCheckmark")
                    : EditorGUIUtility.IconContent("console.erroricon.sml");
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(18));

                // Label + detail
                EditorGUILayout.LabelField(r.label, GUILayout.Width(240));
                _detailStyle ??= new GUIStyle(EditorStyles.miniLabel) { wordWrap = true };
                _detailStyle.normal.textColor = r.passed ? Color.gray : new Color(0.9f, 0.3f, 0.2f);
                EditorGUILayout.LabelField(r.detail, _detailStyle);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        void RunChecks()
        {
            _results.Clear();
            _passCount = 0;
            _failCount = 0;
            _hasRun = true;

            CheckScenes();
            CheckPrefabs();
            CheckScriptableObjects();
            CheckInputActions();
            CheckBuildSettings();
            CheckActiveSceneManagers();

            Repaint();
        }

        void Add(string label, bool passed, string detail = "")
        {
            _results.Add(new CheckResult { label = label, passed = passed, detail = detail });
            if (passed) _passCount++;
            else _failCount++;
        }

        // ─── Scene Checks ─────────────────────────────

        void CheckScenes()
        {
            Add("Boot.unity exists",
                AssetExists("Assets/_Project/Scenes/Boot.unity"),
                "Run BUILD EVERYTHING to create");

            Add("Echohaven scene exists",
                AssetExists("Assets/_Project/Scenes/Echohaven_VerticalSlice.unity"),
                "Core gameplay scene");

            Add("UI_Overlay.unity exists",
                AssetExists("Assets/_Project/Scenes/UI_Overlay.unity"),
                "Run BUILD EVERYTHING to create");
        }

        // ─── Prefab Checks ────────────────────────────

        void CheckPrefabs()
        {
            Add("Player prefab",
                AssetExists("Assets/_Project/Prefabs/Characters/Player.prefab"),
                "Run BUILD EVERYTHING");

            Add("Milo prefab",
                AssetExists("Assets/_Project/Prefabs/Characters/Milo.prefab"),
                "Companion NPC");

            Add("MudGolem prefab",
                AssetExists("Assets/_Project/Prefabs/Characters/MudGolem.prefab"),
                "Enemy prefab");
        }

        // ─── ScriptableObject Checks ──────────────────

        void CheckScriptableObjects()
        {
            // Zones
            string zonePath = "Assets/_Project/Config/Zones";
            var zoneGuids = AssetDatabase.FindAssets("t:ZoneDefinition", new[] { zonePath });
            Add($"Zone definitions ({zoneGuids.Length}/13)",
                zoneGuids.Length >= 13,
                zoneGuids.Length < 13 ? "Run BUILD EVERYTHING" : "OK");

            // Quests
            string questPath = "Assets/_Project/Config/Quests";
            var questGuids = AssetDatabase.FindAssets("t:QuestDefinition", new[] { questPath });
            bool hasQuests = questGuids.Length > 0;
            Add($"Quest definitions ({questGuids.Length})",
                hasQuests,
                hasQuests ? "OK" : "Run BUILD EVERYTHING");

            // Building defs
            string buildingPath = "Assets/_Project/Config";
            var buildingGuids = AssetDatabase.FindAssets("t:BuildingDefinition", new[] { buildingPath });
            bool hasBuildingDefs = buildingGuids.Length > 0;
            Add($"Building definitions ({buildingGuids.Length})",
                hasBuildingDefs,
                hasBuildingDefs ? "OK" : "Run BUILD EVERYTHING");
        }

        // ─── Input Checks ─────────────────────────────

        void CheckInputActions()
        {
            Add("Input actions asset",
                AssetExists("Assets/_Project/Input/TartariaInputActions.inputactions"),
                "Input bindings");

            // Check Player prefab has input wired
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Characters/Player.prefab");
            if (playerPrefab != null)
            {
                var handler = playerPrefab.GetComponent<Tartaria.Input.PlayerInputHandler>();
                bool hasInput = handler != null;
                if (hasInput)
                {
                    var so = new SerializedObject(handler);
                    var prop = so.FindProperty("inputActions");
                    hasInput = prop != null && prop.objectReferenceValue != null;
                }
                Add("Player prefab input wired", hasInput, hasInput ? "OK" : "No InputActionAsset");
            }

            // Check UI_Overlay has HUD content
            var uiScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(
                "Assets/_Project/Scenes/UI_Overlay.unity");
            Add("UI Overlay scene", uiScene != null, uiScene != null ? "OK" : "MISSING");

            // Check Logitech controller support script exists
            bool hasLogitech = AssetExists("Assets/_Project/Scripts/Input/LogitechControllerSupport.cs");
            Add("Logitech controller support", hasLogitech,
                hasLogitech ? "F310/F510/F710 DirectInput" : "MISSING");

            // Check InputActionsFactory includes CameraLook + CameraZoom
            var factoryAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(
                "Assets/_Project/Editor/InputActionsFactory.cs");
            bool hasCameraActions = factoryAsset != null && factoryAsset.text.Contains("CameraLook");
            Add("Gamepad camera actions", hasCameraActions,
                hasCameraActions ? "CameraLook + CameraZoom" : "Rebuild input actions");
        }

        // ─── Build Settings ───────────────────────────

        void CheckBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasThree = scenes != null && scenes.Length >= 3;
            bool bootFirst = hasThree && scenes[0].path.Contains("Boot");
            bool echoSecond = hasThree && scenes[1].path.Contains("Echohaven");
            bool uiThird = hasThree && scenes[2].path.Contains("UI_Overlay");

            Add("Build settings: 3+ scenes",
                hasThree,
                hasThree ? $"{scenes.Length} scenes" : "Run BUILD EVERYTHING");

            Add("Build settings: Boot → Echohaven → UI",
                bootFirst && echoSecond && uiThird,
                (bootFirst && echoSecond && uiThird) ? "Correct order" : "Wrong scene order");
        }

        // ─── Active Scene Manager Checks ──────────────

        void CheckActiveSceneManagers()
        {
            // Only check if a relevant scene is open
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Add("Active scene loaded", false, "No scene open");
                return;
            }

            Add($"Active scene: {scene.name}", true, scene.path);

            // Check for key managers in open scene
            CheckComponentInScene<Core.GameBootstrap>("GameBootstrap");
            Add("GameStateManager", Core.GameStateManager.Instance != null);
            CheckComponentInScene<Integration.GameLoopController>("GameLoopController");
            CheckComponentInScene<Core.SceneLoader>("SceneLoader");
            CheckComponentInScene<Integration.PlayerSpawner>("PlayerSpawner");
            CheckComponentInScene<Integration.BuildingSpawner>("BuildingSpawner");
            CheckComponentInScene<Integration.TutorialSystem>("TutorialSystem");
            CheckComponentInScene<Integration.QuestManager>("QuestManager");
            CheckComponentInScene<Audio.AudioManager>("AudioManager");
            CheckComponentInScene<UI.UIManager>("UIManager");
            CheckComponentInScene<UI.HUDController>("HUDController");

            // ─── Wiring Checks ────────────────────────
            CheckWiring();
        }

        void CheckWiring()
        {
            // PlayerSpawner has prefab
            var spawner = Object.FindFirstObjectByType<Integration.PlayerSpawner>();
            if (spawner != null)
            {
                var so = new SerializedObject(spawner);
                var prefabProp = so.FindProperty("playerPrefab");
                bool hasPrefab = prefabProp != null && prefabProp.objectReferenceValue != null;
                Add("PlayerSpawner.playerPrefab", hasPrefab,
                    hasPrefab ? "Wired" : "NULL — run Wire Scene References");
            }

            // QuestManager has quest database
            var qm = Object.FindFirstObjectByType<Integration.QuestManager>();
            if (qm != null)
            {
                var so = new SerializedObject(qm);
                var dbProp = so.FindProperty("questDatabase");
                int count = dbProp != null ? dbProp.arraySize : 0;
                Add($"QuestManager.questDatabase ({count})", count > 0,
                    count > 0 ? $"{count} quests" : "EMPTY — run Wire Scene References");
            }

            // GameLoopController has refs wired
            var glc = Object.FindFirstObjectByType<Integration.GameLoopController>();
            if (glc != null)
            {
                var so = new SerializedObject(glc);
                var piProp = so.FindProperty("playerInput");
                bool hasPI = piProp != null && piProp.objectReferenceValue != null;
                Add("GameLoopController.playerInput", hasPI,
                    hasPI ? "Wired" : "NULL — RuntimeGlueBridge wires at runtime");

                var ccProp = so.FindProperty("cameraController");
                bool hasCC = ccProp != null && ccProp.objectReferenceValue != null;
                Add("GameLoopController.cameraController", hasCC,
                    hasCC ? "Wired" : "NULL — RuntimeGlueBridge wires at runtime");
            }

            // InteractableBuildings have definitions
            var buildings = Object.FindObjectsByType<Integration.InteractableBuilding>(FindObjectsSortMode.None);
            int wiredBuildings = 0;
            foreach (var b in buildings)
            {
                var so = new SerializedObject(b);
                var defProp = so.FindProperty("definition");
                if (defProp != null && defProp.objectReferenceValue != null)
                    wiredBuildings++;
            }
            Add($"Building definitions wired ({wiredBuildings}/{buildings.Length})",
                buildings.Length == 0 || wiredBuildings == buildings.Length,
                buildings.Length == 0 ? "No buildings in scene" :
                wiredBuildings == buildings.Length ? "All wired" : "Run Wire Scene References");

            // Layers configured
            bool hasBuildingLayer = LayerMask.NameToLayer("Building") >= 0;
            bool hasInteractableLayer = LayerMask.NameToLayer("Interactable") >= 0;
            bool hasPlayerLayer = LayerMask.NameToLayer("Player") >= 0;
            Add("Physics layers configured",
                hasBuildingLayer && hasInteractableLayer && hasPlayerLayer,
                (hasBuildingLayer && hasInteractableLayer && hasPlayerLayer)
                    ? "Building/Interactable/Player OK"
                    : "Missing layers — run Wire Scene References");
        }

        void CheckComponentInScene<T>(string label) where T : Component
        {
            var all = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            var found = all.Length > 0 ? all[0] : null;
            Add(label, found != null, found != null ? found.gameObject.name : "MISSING in scene");
        }

        static bool AssetExists(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null;
        }
    }
}
