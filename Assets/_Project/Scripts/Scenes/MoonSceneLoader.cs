using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Scenes
{
    /// <summary>
    /// R281 — multi-scene additive loader for the 13 Moons.
    /// Loads a Moon scene additively (preserves Boot + UI + persistent systems),
    /// then unloads the previous Moon. Per CLAUDE.md Unity 6 best practices:
    /// "Most scenes should be constructed from Prefabs with minimal overrides...
    /// Boot + UI + Echohaven + Moon1_Systems already in use."
    ///
    /// Usage:
    ///   MoonSceneLoader.Instance.LoadMoon("Moon2_LunarMoon");
    ///   MoonSceneLoader.Instance.LoadMoon(2);  // by number
    /// </summary>
    public class MoonSceneLoader : MonoBehaviour
    {
        public static MoonSceneLoader Instance { get; private set; }

        // R278-R279 ship: 13 scene file names (sans .unity)
        public static readonly string[] MoonSceneNames = new[]
        {
            "Echohaven_VerticalSlice",   // Moon 1
            "Moon2_LunarMoon",
            "Moon3_ElectricMoon",
            "Moon4_BronzeMoon",
            "Moon5_ObsidianMoon",
            "Moon6_AquaSunken",
            "Moon7_FrostVault",
            "Moon8_AetherAirship",
            "Moon9_CinderSolar",
            "Moon10_VerdantGrove",
            "Moon11_MistFountain",
            "Moon12_MirrorBell",
            "Moon13_CosmicHarmony",
        };

        string _currentMoonSceneName;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMoon(int moonNumber)
        {
            if (moonNumber < 1 || moonNumber > 13)
            {
                Debug.LogError($"[MoonSceneLoader] Invalid moon number: {moonNumber} (must be 1-13)");
                return;
            }
            LoadMoon(MoonSceneNames[moonNumber - 1]);
        }

        public void LoadMoon(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[MoonSceneLoader] sceneName is null/empty");
                return;
            }
            if (sceneName == _currentMoonSceneName)
            {
                Debug.Log($"[MoonSceneLoader] Moon {sceneName} already loaded, ignoring.");
                return;
            }
            StartCoroutine(SwitchMoonRoutine(sceneName));
        }

        IEnumerator SwitchMoonRoutine(string newSceneName)
        {
            // 1. Load new additively
            Debug.Log($"[MoonSceneLoader] Loading {newSceneName} additively...");
            var loadOp = SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            while (loadOp != null && !loadOp.isDone) yield return null;

            // 2. Set new as active
            var newScene = SceneManager.GetSceneByName(newSceneName);
            if (newScene.IsValid()) SceneManager.SetActiveScene(newScene);

            // 3. Unload previous Moon (if any)
            if (!string.IsNullOrEmpty(_currentMoonSceneName))
            {
                var prev = SceneManager.GetSceneByName(_currentMoonSceneName);
                if (prev.IsValid() && prev.isLoaded)
                {
                    Debug.Log($"[MoonSceneLoader] Unloading previous {_currentMoonSceneName}...");
                    var unloadOp = SceneManager.UnloadSceneAsync(prev);
                    while (unloadOp != null && !unloadOp.isDone) yield return null;
                }
            }

            _currentMoonSceneName = newSceneName;
            Debug.Log($"[MoonSceneLoader] Active Moon: {_currentMoonSceneName}");
        }

        public string CurrentMoonSceneName => _currentMoonSceneName;
    }
}
