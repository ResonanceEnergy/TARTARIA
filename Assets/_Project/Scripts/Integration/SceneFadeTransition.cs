using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
namespace Tartaria.Integration
{
    /// <summary>
    /// Singleton DontDestroyOnLoad that renders a black IMGUI overlay and fades it in/out
    /// when warping between scenes. Intercept any SceneManager.LoadScene call via
    /// <c>SceneFadeTransition.LoadScene(sceneName)</c> instead of calling SceneManager directly.
    ///
    /// On scene load completion, automatically fades back out.
    /// Safe to call multiple times -- re-entrant load cancels the previous coroutine.
    /// </summary>
    [DisallowMultipleComponent]
    public class SceneFadeTransition : MonoBehaviour
    {
        // ── singleton ──────────────────────────────────────────────────────────
        static SceneFadeTransition _instance;
        public static SceneFadeTransition Instance => _instance;

        // ── config ─────────────────────────────────────────────────────────────
        const float FADE_OUT_DURATION = 0.35f;   // black-in  (screen → black)
        const float FADE_IN_DURATION  = 0.45f;   // black-out (black  → scene)
        const float HOLD_DURATION     = 0.10f;   // full-black hold before actual load

        // ── state ──────────────────────────────────────────────────────────────
        float _alpha      = 0f;
        bool  _isFading   = false;
        Coroutine _active = null;

        // ── bootstrap ──────────────────────────────────────────────────────────
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (_instance != null) return;
            var go = new GameObject("SceneFadeTransition");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<SceneFadeTransition>();
        }

        // ── public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Call instead of SceneManager.LoadScene() to get a smooth black fade.
        /// Falls back to direct load if singleton isn't ready yet.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (_instance == null || !_instance.gameObject.activeInHierarchy)
            {
                // Fallback: direct load
                TryDirectLoad(sceneName);
                return;
            }
            if (_instance._active != null)
                _instance.StopCoroutine(_instance._active);
            _instance._active = _instance.StartCoroutine(_instance.FadeAndLoad(sceneName));
        }

        // ── coroutine ──────────────────────────────────────────────────────────

        IEnumerator FadeAndLoad(string sceneName)
        {
            _isFading = true;

            // Fade to black
            float t = 0f;
            while (t < FADE_OUT_DURATION)
            {
                t += Time.unscaledDeltaTime;
                _alpha = Mathf.Clamp01(t / FADE_OUT_DURATION);
                yield return null;
            }
            _alpha = 1f;

            // Hold on black
            yield return new WaitForSecondsRealtime(HOLD_DURATION);

            // Load scene
            SceneManager.sceneLoaded += OnSceneLoaded;
            TryDirectLoad(sceneName);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_active != null) StopCoroutine(_active);
            _active = StartCoroutine(FadeIn());
        }

        IEnumerator FadeIn()
        {
            _alpha = 1f;
            float t = 0f;
            while (t < FADE_IN_DURATION)
            {
                t += Time.unscaledDeltaTime;
                _alpha = Mathf.Clamp01(1f - t / FADE_IN_DURATION);
                yield return null;
            }
            _alpha    = 0f;
            _isFading = false;
            _active   = null;
        }

        // ── IMGUI overlay ──────────────────────────────────────────────────────

        void OnGUI()
        {
            if (_alpha <= 0.005f) return;
            var prev = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, _alpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }

        // ── helpers ────────────────────────────────────────────────────────────

        static void TryDirectLoad(string sceneName)
        {
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SceneFade] Could not load '{sceneName}': {e.Message}");
            }
        }
    }
}
