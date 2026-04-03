using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tartaria.Editor
{
    /// <summary>
    /// Batch-mode readiness validator. Runs all PlayReadinessWindow checks
    /// headlessly and exits with code 0 (all pass) or 1 (failures).
    ///
    /// Entry point:
    ///   Unity.exe -batchmode -projectPath ... -executeMethod Tartaria.Editor.BatchReadinessValidator.Validate -quit
    ///
    /// Also usable from GUI: Tartaria > Validate (Batch-style)
    /// </summary>
    public static class BatchReadinessValidator
    {
        static int _pass;
        static int _fail;
        static readonly List<string> _failures = new();

        [MenuItem("Tartaria/Validate (Batch-style)", false, -40)]
        public static void ValidateMenuItem()
        {
            Validate();
        }

        public static void Validate()
        {
            _pass = 0;
            _fail = 0;
            _failures.Clear();

            Debug.Log("[Tartaria] ═══════════════════════════════════════");
            Debug.Log("[Tartaria] READINESS VALIDATION");
            Debug.Log("[Tartaria] ═══════════════════════════════════════");

            // ── Scenes ──
            Check("Boot.unity",      AssetExists("Assets/_Project/Scenes/Boot.unity"));
            Check("Echohaven scene",  AssetExists("Assets/_Project/Scenes/Echohaven_VerticalSlice.unity"));
            Check("UI_Overlay.unity", AssetExists("Assets/_Project/Scenes/UI_Overlay.unity"));

            // ── Prefabs ──
            Check("Player prefab",   AssetExists("Assets/_Project/Prefabs/Characters/Player.prefab"));
            Check("Milo prefab",     AssetExists("Assets/_Project/Prefabs/Characters/Milo.prefab"));
            Check("MudGolem prefab", AssetExists("Assets/_Project/Prefabs/Characters/MudGolem.prefab"));

            // ── ScriptableObjects ──
            var zoneGuids = AssetDatabase.FindAssets("t:ZoneDefinition", new[] { "Assets/_Project/Config/Zones" });
            Check($"Zone definitions ({zoneGuids.Length}/13)", zoneGuids.Length >= 13);

            var questGuids = AssetDatabase.FindAssets("t:QuestDefinition", new[] { "Assets/_Project/Config/Quests" });
            Check($"Quest definitions ({questGuids.Length})", questGuids.Length > 0);

            var buildingGuids = AssetDatabase.FindAssets("t:BuildingDefinition", new[] { "Assets/_Project/Config" });
            Check($"Building definitions ({buildingGuids.Length})", buildingGuids.Length > 0);

            // ── Input ──
            Check("Input actions asset", AssetExists("Assets/_Project/Input/TartariaInputActions.inputactions"));

            // ── Build Settings ──
            var scenes = EditorBuildSettings.scenes;
            bool hasThree = scenes != null && scenes.Length >= 3;
            bool correctOrder = hasThree
                && scenes[0].path.Contains("Boot")
                && scenes[1].path.Contains("Echohaven")
                && scenes[2].path.Contains("UI_Overlay");
            Check("Build settings: 3+ scenes", hasThree);
            Check("Build settings: correct order", correctOrder);

            // ── Scene Managers (open Boot or Echohaven to check) ──
            string echoPath = "Assets/_Project/Scenes/Echohaven_VerticalSlice.unity";
            if (AssetExists(echoPath))
            {
                EditorSceneManager.OpenScene(echoPath, OpenSceneMode.Single);
                CheckComponent<Core.GameBootstrap>("GameBootstrap");
                Check("GameStateManager", Core.GameStateManager.Instance != null);
                CheckComponent<Integration.GameLoopController>("GameLoopController");
                CheckComponent<Core.SceneLoader>("SceneLoader");
                CheckComponent<Integration.PlayerSpawner>("PlayerSpawner");
                CheckComponent<Integration.BuildingSpawner>("BuildingSpawner");
                CheckComponent<Integration.TutorialSystem>("TutorialSystem");
                CheckComponent<Integration.QuestManager>("QuestManager");
                CheckComponent<Audio.AudioManager>("AudioManager");
                CheckComponent<UI.UIManager>("UIManager");
                CheckComponent<UI.HUDController>("HUDController");
                CheckComponent<Integration.RuntimeGlueBridge>("RuntimeGlueBridge");
                CheckComponent<Integration.RuntimeBootValidator>("RuntimeBootValidator");

                // Reopen Boot scene for play
                string bootPath = "Assets/_Project/Scenes/Boot.unity";
                if (AssetExists(bootPath))
                    EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
            }

            // ── Summary ──
            Debug.Log("[Tartaria] ───────────────────────────────────────");
            if (_fail == 0)
            {
                Debug.Log($"[Tartaria] ALL {_pass} CHECKS PASSED");
            }
            else
            {
                Debug.LogError($"[Tartaria] {_fail} FAILED / {_pass} passed");
                foreach (var f in _failures)
                    Debug.LogError($"[Tartaria]   FAIL: {f}");
            }
            Debug.Log("[Tartaria] ═══════════════════════════════════════");

            // In batch mode, exit with appropriate code
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                EditorApplication.Exit(_fail > 0 ? 1 : 0);
            }
        }

        static void Check(string label, bool passed)
        {
            if (passed)
            {
                _pass++;
                Debug.Log($"[Tartaria]  OK  {label}");
            }
            else
            {
                _fail++;
                _failures.Add(label);
                Debug.LogError($"[Tartaria] FAIL {label}");
            }
        }

        static void CheckComponent<T>(string label) where T : Component
        {
            var all = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            Check(label, all.Length > 0);
        }

        static bool AssetExists(string path)
        {
            return AssetDatabase.LoadMainAssetAtPath(path) != null;
        }
    }
}
