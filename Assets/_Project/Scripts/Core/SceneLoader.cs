using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tartaria.Core
{
    /// <summary>
    /// Scene Loader — handles async scene loading after boot.
    /// Sits in the Boot scene; loads Echohaven then UI_Overlay additively.
    /// Survives scene transitions via DontDestroyOnLoad.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] string gameplayScene = "Echohaven_VerticalSlice";
        [SerializeField] string uiOverlayScene = "UI_Overlay";

        [Header("Timing")]
#pragma warning disable CS0414 // Assigned but never used - future timing control
        [SerializeField] float minimumLoadTime = 1.5f;
#pragma warning restore CS0414

        bool _loaded;

        // Scene loading state (event-based, no coroutines or Update())
        float _startTime;
        int _scenesLoadedCount;
        bool _expectingSceneLoad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("SceneLoader");
            DontDestroyOnLoad(go);
            go.AddComponent<SceneLoader>();
        }

        static void Canary(string msg)
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "_Project/Logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "sceneloader-canary.txt"),
                    $"[{Time.realtimeSinceStartup:F2}] {msg}\n");
            }
            catch (System.Exception ex) { Debug.LogWarning($"[SceneLoader] Canary write failed: {ex.Message}"); }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            Application.quitting += () => _quitting = true;

            // Subscribe to scene loaded event (ONLY way to detect scene load completion)
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        bool _quitting;

        void OnDestroy()
        {
            StopAllCoroutines();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (Instance == this)
            {
                if (!_quitting)
                    Debug.LogWarning("[SceneLoader] Instance destroyed -- coroutines will stop!");
                Instance = null;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!_expectingSceneLoad) return;

            Canary($"OnSceneLoaded: {scene.name}, mode={mode}");
            _scenesLoadedCount++;

            // Wait for both gameplay + UI overlay scenes
            if (_scenesLoadedCount >= 2)
            {
                _expectingSceneLoad = false;
                _waitingForPlayerSpawn = true;
                Canary("Both scenes loaded - waiting for PlayerSpawner.Start() to complete");

                // CRITICAL: sceneLoaded fires AFTER OnEnable but BEFORE Start().
                // Coroutines CANNOT yield during this window (scheduler frozen).
                // Solution: PlayerSpawner.Start() will call OnPlayerSpawnComplete() as callback.
            }
        }

        static bool _waitingForPlayerSpawn = false;

        /// <summary>
        /// Called by PlayerSpawner after spawning player in Start().
        /// Completes scene load process and transitions to Exploration.
        /// Static method - no coroutines.
        /// </summary>
        public static void OnPlayerSpawnComplete()
        {
            if (!_waitingForPlayerSpawn) return;
            _waitingForPlayerSpawn = false;

            if (Instance != null)
            {
                Canary("OnPlayerSpawnComplete - PlayerSpawner.Start() finished, calling FinishSceneLoad");
                Instance.FinishSceneLoad();
            }
        }

        /// <summary>
        /// Called by GameBootstrap after ECS init succeeds.
        /// Starts async scene loads - completion detected via SceneManager.sceneLoaded event.
        /// NO coroutines, NO Update(), NO .isDone checks - event-driven only.
        /// </summary>
        public void LoadGameplayScenes()
        {
            if (_loaded) return;
            _loaded = true;

            Canary("LoadGameplayScenes START (event-driven)");
            GameStateManager.Instance.TransitionTo(GameState.Loading);
            _startTime = Time.realtimeSinceStartup;
            _scenesLoadedCount = 0;
            _expectingSceneLoad = true;

            // Disable BootCamera IMMEDIATELY
            DisableBootCamera();

            // Start gameplay scene load
            Canary($"Loading gameplay scene: {gameplayScene}");
            Debug.Log($"[SceneLoader] Loading gameplay scene: {gameplayScene}");
            var gameplayOp = SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
            if (gameplayOp != null)
            {
                gameplayOp.allowSceneActivation = true;
                Canary($"Gameplay scene load started (async) - will complete via sceneLoaded event");
            }
            else
            {
                Canary("FAILED to load gameplay scene — aborting");
                Debug.LogError($"[SceneLoader] CRITICAL: Scene not found: {gameplayScene}. Aborting load.");
                return;
            }

            // Start UI overlay load
            Canary($"Loading UI overlay: {uiOverlayScene}");
            Debug.Log($"[SceneLoader] Loading UI overlay: {uiOverlayScene}");
            var uiOp = SceneManager.LoadSceneAsync(uiOverlayScene, LoadSceneMode.Additive);
            if (uiOp != null)
            {
                uiOp.allowSceneActivation = true;
                Canary($"UI overlay load started (async) - will complete via sceneLoaded event");
            }
            else
            {
                Canary("UI overlay scene not found — aborting");
                Debug.LogError($"[SceneLoader] CRITICAL: Scene not found: {uiOverlayScene}. Aborting load.");
                return;
            }

            Canary("LoadGameplayScenes END - waiting for sceneLoaded events");
        }

        void FinishSceneLoad()
        {
            Canary($"FinishSceneLoad - time={Time.realtimeSinceStartup - _startTime:F2}s");

            float totalElapsed = Time.realtimeSinceStartup - _startTime;
            Debug.Log($"[SceneLoader] Scenes loaded in {totalElapsed:F1}s");

            // Disable Boot scene camera (idempotent)
            DisableBootCamera();

            // Enforce exactly one AudioListener
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 1)
            {
                var mainCam = UnityEngine.Camera.main;
                AudioListener kept = null;

                if (mainCam != null)
                    foreach (var l in listeners)
                        if (l.gameObject == mainCam.gameObject) { kept = l; break; }

                if (kept == null)
                    foreach (var l in listeners)
                        if (l.isActiveAndEnabled) { kept = l; break; }

                if (kept == null)
                    kept = listeners[0];

                foreach (var l in listeners)
                    if (l != kept) Destroy(l);

                Debug.Log($"[SceneLoader] Kept AudioListener on {kept.gameObject.name}, removed {listeners.Length - 1} duplicate(s).");
            }
            else if (listeners.Length == 0)
            {
                var mainCam = UnityEngine.Camera.main;
                if (mainCam != null && mainCam.GetComponent<AudioListener>() == null)
                {
                    mainCam.gameObject.AddComponent<AudioListener>();
                    Debug.Log("[SceneLoader] Added AudioListener to Main Camera.");
                }
            }

            // Transition to exploration
            Canary("Transitioning to Exploration");
            Debug.Log("[SceneLoader] Transitioning to Exploration...");
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
            Debug.Log("[SceneLoader] Gameplay + UI scenes loaded. Entering Exploration.");
        }

        /// <summary>
        /// M2: Called from PauseMenu "Save & Quit to Menu". Unloads gameplay + returns to Boot/main menu state.
        /// </summary>
        public void LoadMainMenuScene()
        {
            Debug.Log("[SceneLoader] Returning to Main Menu (M2 Pause flow)");
            StartCoroutine(ReturnToMainMenu());
        }

        IEnumerator ReturnToMainMenu()
        {
            Time.timeScale = 1f;

            // Unload additive gameplay + UI overlay if present
            if (SceneManager.GetSceneByName(gameplayScene).isLoaded)
                yield return SceneManager.UnloadSceneAsync(gameplayScene);
            if (SceneManager.GetSceneByName(uiOverlayScene).isLoaded)
                yield return SceneManager.UnloadSceneAsync(uiOverlayScene);

            // Reload Boot so MainMenuOverlay re-activates cleanly
            yield return SceneManager.LoadSceneAsync("Boot", LoadSceneMode.Single);
        }

        /// <summary>
        /// Disable the Boot scene's bootstrap camera so the gameplay CameraRig owns
        /// the screen. Safe to call multiple times — no-op once BootCamera is gone.
        /// </summary>
        static void DisableBootCamera()
        {
            var bootCam = GameObject.Find("BootCamera");
            if (bootCam == null) return;
            // Destroy the Boot AudioListener first so it can't win FindObjectsByType races.
            var bootListener = bootCam.GetComponent<AudioListener>();
            if (bootListener != null) Destroy(bootListener);
            // Disable the Camera component as well as the GameObject — defends against
            // anything that re-activates the parent without re-checking the camera.
            var bootCameraComp = bootCam.GetComponent<UnityEngine.Camera>();
            if (bootCameraComp != null) bootCameraComp.enabled = false;
            bootCam.SetActive(false);
            Debug.Log("[SceneLoader] Disabled BootCamera — CameraRig takes over.");
        }
    }
}
