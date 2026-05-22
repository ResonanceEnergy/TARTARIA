using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

namespace Tartaria.UI
{
    /// <summary>
    /// Scene Fade Transition — smooth fade to black for scene loads.
    /// Singleton canvas overlay. Call LoadScene(sceneName) to fade + load.
    /// Auto-creates CanvasGroup for fade control.
    /// </summary>
    public class SceneFadeTransition : MonoBehaviour
    {
        public static SceneFadeTransition Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] float fadeOutDuration = 0.5f;
        [SerializeField] float fadeInDuration = 0.5f;
        [SerializeField] Color fadeColor = Color.black;

        CanvasGroup _canvasGroup;
        Image _fadeImage;
        bool _isFading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;

            // Create persistent canvas overlay
            var go = new GameObject("SceneFadeTransition");
            DontDestroyOnLoad(go);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;  // Top-most overlay

            var canvasScaler = go.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);

            go.AddComponent<GraphicRaycaster>();

            Instance = go.AddComponent<SceneFadeTransition>();
            Instance.CreateFadeImage();
        }

        void CreateFadeImage()
        {
            // Fullscreen fade image
            var fadeGO = new GameObject("FadeImage");
            fadeGO.transform.SetParent(transform, false);

            _fadeImage = fadeGO.AddComponent<Image>();
            _fadeImage.color = fadeColor;

            var rect = fadeGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            _canvasGroup = fadeGO.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;  // Start transparent
            _canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Fade to black, load scene, fade in.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (Instance == null)
            {
                // Fallback: direct load if instance missing
                SceneManager.LoadScene(sceneName);
                return;
            }

            if (Instance._isFading) return;  // Already fading

            Instance.StartCoroutine(Instance.FadeAndLoadCoroutine(sceneName));
        }

        IEnumerator FadeAndLoadCoroutine(string sceneName)
        {
            _isFading = true;
            _canvasGroup.blocksRaycasts = true;

            // Fade out (to black)
            yield return FadeOut();

            // Load scene
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Fade in (from black)
            yield return FadeIn();

            _canvasGroup.blocksRaycasts = false;
            _isFading = false;
        }

        IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Instant fade to black (for immediate cuts).
        /// </summary>
        public static void FadeToBlackImmediate()
        {
            if (Instance != null && Instance._canvasGroup != null)
            {
                Instance._canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Instant clear to transparent (for immediate reveals).
        /// </summary>
        public static void ClearFadeImmediate()
        {
            if (Instance != null && Instance._canvasGroup != null)
            {
                Instance._canvasGroup.alpha = 0f;
            }
        }
    }
}
