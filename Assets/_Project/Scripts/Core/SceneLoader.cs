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
        [SerializeField] float minimumLoadTime = 1.5f;

        bool _loaded;

        static void Canary(string msg)
        {
            try
            {
                string dir = Path.Combine(Application.dataPath, "_Project/Logs");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "sceneloader-canary.txt"),
                    $"[{Time.realtimeSinceStartup:F2}] {msg}\n");
            }
            catch { /* ignore IO errors during diagnostics */ }
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (Instance == this)
            {
                Debug.LogWarning("[SceneLoader] Instance destroyed -- coroutines will stop!");
                Instance = null;
            }
        }

        /// <summary>
        /// Called by GameBootstrap after ECS init succeeds.
        /// </summary>
        public void LoadGameplayScenes()
        {
            if (_loaded) return;
            _loaded = true;
            StartCoroutine(LoadSequence());
        }

        IEnumerator LoadSequence()
        {
            Debug.Log("[SceneLoader] LoadSequence started.");

            // Wrap in exception-safe helper to detect silent coroutine death
            IEnumerator inner = LoadSequenceInner();
            while (true)
            {
                bool hasNext;
                try { hasNext = inner.MoveNext(); }
                catch (System.Exception ex)
                {
                    Canary($"EXCEPTION: {ex}");
                    Debug.LogError($"[SceneLoader] Coroutine exception: {ex}");
                    yield break;
                }
                if (!hasNext) yield break;
                yield return inner.Current;
            }
        }

        IEnumerator LoadSequenceInner()
        {
            Canary("LoadSequenceInner START");
            GameStateManager.Instance.TransitionTo(GameState.Loading);
            float startTime = Time.realtimeSinceStartup;

            // Load gameplay scene
            Canary($"Loading gameplay scene: {gameplayScene}");
            Debug.Log($"[SceneLoader] Loading gameplay scene: {gameplayScene}");
            var gameplayOp = SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
            Canary($"gameplayOp null? {gameplayOp == null}");
            if (gameplayOp != null)
            {
                gameplayOp.allowSceneActivation = true;
                while (!gameplayOp.isDone)
                    yield return null;
                Canary($"Gameplay scene loaded OK ({Time.realtimeSinceStartup - startTime:F1}s)");
                Debug.Log($"[SceneLoader] Gameplay scene loaded OK ({Time.realtimeSinceStartup - startTime:F1}s).");
            }
            else
            {
                Canary("FAILED to load gameplay scene — aborting");
                Debug.LogError($"[SceneLoader] CRITICAL: Scene not found: {gameplayScene}. Aborting load.");
                yield break;
            }

            // Load UI overlay additively
            Canary($"Loading UI overlay: {uiOverlayScene}");
            Debug.Log($"[SceneLoader] Loading UI overlay: {uiOverlayScene}");
            var uiOp = SceneManager.LoadSceneAsync(uiOverlayScene, LoadSceneMode.Additive);
            Canary($"uiOp null? {uiOp == null}");
            if (uiOp != null)
            {
                uiOp.allowSceneActivation = true;
                while (!uiOp.isDone)
                    yield return null;
                Canary($"UI overlay loaded OK ({Time.realtimeSinceStartup - startTime:F1}s)");
                Debug.Log($"[SceneLoader] UI overlay loaded OK ({Time.realtimeSinceStartup - startTime:F1}s).");
            }
            else
            {
                Canary("UI overlay scene not found — aborting");
                Debug.LogError($"[SceneLoader] CRITICAL: Scene not found: {uiOverlayScene}. Aborting load.");
                yield break;
            }

            // Minimum loading time
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minimumLoadTime)
            {
                Canary($"Waiting min load time ({minimumLoadTime - elapsed:F1}s)");
                Debug.Log($"[SceneLoader] Waiting minimum load time ({minimumLoadTime - elapsed:F1}s remaining).");
                yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);
            }

            // Disable Boot scene camera — gameplay CameraRig takes over
            var bootCam = GameObject.Find("BootCamera");
            if (bootCam != null)
            {
                // Destroy the Boot AudioListener before deactivating so it can't win the FindObjectsByType race
                var bootListener = bootCam.GetComponent<AudioListener>();
                if (bootListener != null) Destroy(bootListener);
                bootCam.SetActive(false);
                Debug.Log("[SceneLoader] Disabled BootCamera — CameraRig takes over.");
            }

            // Enforce exactly one AudioListener — gameplay camera takes priority
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 1)
            {
                // Keep the one on the active main camera; destroy the rest
                var mainCam = UnityEngine.Camera.main;
                AudioListener kept = null;
                foreach (var listener in listeners)
                {
                    if (kept == null && mainCam != null && listener.gameObject == mainCam.gameObject)
                    {
                        kept = listener;
                        continue;
                    }
                    if (kept == null && listener.isActiveAndEnabled)
                    {
                        kept = listener;
                        continue;
                    }
                    Destroy(listener);
                }
                Debug.Log($"[SceneLoader] Removed {listeners.Length - 1} duplicate AudioListener(s).");
            }
            else if (listeners.Length == 0)
            {
                // BootCamera was the only listener and is now disabled — add one to CameraRig
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
    }
}
