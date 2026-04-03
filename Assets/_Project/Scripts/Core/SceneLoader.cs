using System.Collections;
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

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            GameStateManager.Instance.TransitionTo(GameState.Loading);
            float startTime = Time.realtimeSinceStartup;

            // Load gameplay scene
            var gameplayOp = SceneManager.LoadSceneAsync(gameplayScene, LoadSceneMode.Additive);
            if (gameplayOp != null)
            {
                gameplayOp.allowSceneActivation = true;
                while (!gameplayOp.isDone)
                    yield return null;
            }
            else
            {
                Debug.LogError($"[SceneLoader] Failed to load scene: {gameplayScene}");
            }

            // Load UI overlay additively
            var uiOp = SceneManager.LoadSceneAsync(uiOverlayScene, LoadSceneMode.Additive);
            if (uiOp != null)
            {
                uiOp.allowSceneActivation = true;
                while (!uiOp.isDone)
                    yield return null;
            }
            else
            {
                Debug.LogWarning($"[SceneLoader] UI overlay scene not found: {uiOverlayScene}");
            }

            // Minimum loading time
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < minimumLoadTime)
                yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);

            // Enforce exactly one AudioListener — gameplay camera takes priority
            var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length > 1)
            {
                for (int i = 1; i < listeners.Length; i++)
                    Destroy(listeners[i]);
                Debug.Log($"[SceneLoader] Removed {listeners.Length - 1} duplicate AudioListener(s).");
            }

            // Transition to exploration
            GameStateManager.Instance.TransitionTo(GameState.Exploration);
            Debug.Log("[SceneLoader] Gameplay + UI scenes loaded. Entering Exploration.");
        }
    }
}
