using UnityEngine;
using UnityEngine.SceneManagement;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Day-7: Lightweight cross-session run state.
    /// Persists total RS + last visited moon scene to PlayerPrefs.
    /// Companion to MoonProgressTracker (which owns the cleared-moon set).
    ///
    /// Subscribes to GameEvents.OnRSChanged for live accumulation, and to
    /// SceneManager.sceneLoaded so the "Continue" flow can resume on the
    /// player's last moon. Self-bootstraps before scene load.
    /// </summary>
    [DisallowMultipleComponent]
    public class RunProgressTracker : MonoBehaviour
    {
        const string PrefRS         = "TARTARIA_TotalRS";
        const string PrefLastScene  = "TARTARIA_LastScene";
        const string PrefSessionCnt = "TARTARIA_Sessions";

        public static RunProgressTracker Instance { get; private set; }

        public static event System.Action<float> OnTotalRSChanged;

        public float TotalRS { get; private set; }
        public string LastScene { get; private set; }
        public int SessionCount { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("RunProgressTracker");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<RunProgressTracker>();
            Instance.LoadFromPrefs();
        }

        void LoadFromPrefs()
        {
            TotalRS      = PlayerPrefs.GetFloat(PrefRS, 0f);
            LastScene    = PlayerPrefs.GetString(PrefLastScene, "");
            SessionCount = PlayerPrefs.GetInt(PrefSessionCnt, 0) + 1;
            PlayerPrefs.SetInt(PrefSessionCnt, SessionCount);
            PlayerPrefs.Save();
            Debug.Log($"[RunProgress] Loaded RS={TotalRS:F1}, lastScene='{LastScene}', session#{SessionCount}");
        }

        void OnEnable()
        {
            GameEvents.OnRSChanged += HandleRSDelta;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        void OnDisable()
        {
            GameEvents.OnRSChanged -= HandleRSDelta;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        void HandleRSDelta(float delta)
        {
            TotalRS = Mathf.Max(0f, TotalRS + delta);
            PlayerPrefs.SetFloat(PrefRS, TotalRS);
            // Cheap save — fine for ~few writes/sec; flush on scene change too.
            try { OnTotalRSChanged?.Invoke(TotalRS); } catch { /* swallow */ }
        }

        void HandleSceneLoaded(Scene s, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single) return;
            // Only remember gameplay scenes, not Boot/UI overlays.
            if (string.IsNullOrEmpty(s.name)) return;
            if (s.name == "Boot" || s.name.StartsWith("UI_")) return;
            LastScene = s.name;
            PlayerPrefs.SetString(PrefLastScene, LastScene);
            PlayerPrefs.Save();
        }

        public void ResetRun()
        {
            TotalRS = 0f;
            LastScene = "";
            PlayerPrefs.DeleteKey(PrefRS);
            PlayerPrefs.DeleteKey(PrefLastScene);
            PlayerPrefs.Save();
            Debug.Log("[RunProgress] Run reset.");
        }
    }
}
