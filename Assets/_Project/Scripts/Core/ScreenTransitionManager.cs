using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tartaria.Core
{
    /// <summary>
    /// ScreenTransitionManager — cinematic screen wipes + fades for scene transitions.
    /// Supports multiple transition types: fade, wipe left/right/up/down, circle, diamond.
    /// Can overlay custom images (portal swirl, moon symbol) during transition.
    /// 
    /// Transition Types:
    /// - FadeToBlack → standard fade out/in
    /// - WipeLeft/Right/Up/Down → directional wipe
    /// - CircleExpand/Contract → circular transition
    /// - DiamondExpand → diamond-shaped wipe
    /// - Custom → overlay image with fade
    /// 
    /// Usage:
    /// - ScreenTransitionManager.Instance.FadeToBlack(duration, onComplete)
    /// - ScreenTransitionManager.Instance.WipeTransition(WipeDirection.Left, duration, onComplete)
    /// - ScreenTransitionManager.Instance.CustomTransition(portalImage, duration, onComplete)
    /// 
    /// GDD refs: §05 (UI/UX Polish), §10 (Moon Portal Transitions)
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenTransitionManager : MonoBehaviour
    {
        public static ScreenTransitionManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] Image transitionOverlay;
        [SerializeField] RectTransform overlayRect;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] Color fadeColor = Color.black;
        [SerializeField] AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        Coroutine _activeTransition;
        bool _isTransitioning;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Setup overlay
            if (transitionOverlay == null)
            {
                var overlayGO = new GameObject("TransitionOverlay");
                overlayGO.transform.SetParent(transform, false);

                transitionOverlay = overlayGO.AddComponent<Image>();
                transitionOverlay.color = fadeColor;

                overlayRect = overlayGO.GetComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.sizeDelta = Vector2.zero;
            }

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            // Start transparent
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;

            // Ensure canvas is overlay + top sorting order
            var canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
        }

        /// <summary>
        /// Simple fade to black and back.
        /// </summary>
        public void FadeToBlack(float duration, System.Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[ScreenTransition] Transition already active");
                return;
            }

            _activeTransition = StartCoroutine(FadeCoroutine(duration, onComplete));
        }

        /// <summary>
        /// Directional wipe transition.
        /// </summary>
        public void WipeTransition(WipeDirection direction, float duration, System.Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[ScreenTransition] Transition already active");
                return;
            }

            _activeTransition = StartCoroutine(WipeCoroutine(direction, duration, onComplete));
        }

        /// <summary>
        /// Circle expand/contract transition.
        /// </summary>
        public void CircleTransition(bool expand, float duration, System.Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[ScreenTransition] Transition already active");
                return;
            }

            _activeTransition = StartCoroutine(CircleCoroutine(expand, duration, onComplete));
        }

        /// <summary>
        /// Custom image overlay transition (e.g. portal swirl).
        /// </summary>
        public void CustomTransition(Sprite overlaySprite, float duration, System.Action onComplete = null)
        {
            if (_isTransitioning)
            {
                Debug.LogWarning("[ScreenTransition] Transition already active");
                return;
            }

            _activeTransition = StartCoroutine(CustomCoroutine(overlaySprite, duration, onComplete));
        }

        IEnumerator FadeCoroutine(float duration, System.Action onComplete)
        {
            _isTransitioning = true;
            canvasGroup.blocksRaycasts = true;

            transitionOverlay.color = fadeColor;

            // Fade out (black)
            yield return FadeCanvasGroup(canvasGroup, 0f, 1f, duration * 0.5f);

            // Callback at peak black
            onComplete?.Invoke();

            // Fade in (transparent)
            yield return FadeCanvasGroup(canvasGroup, 1f, 0f, duration * 0.5f);

            canvasGroup.blocksRaycasts = false;
            _isTransitioning = false;
        }

        IEnumerator WipeCoroutine(WipeDirection direction, float duration, System.Action onComplete)
        {
            _isTransitioning = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            transitionOverlay.color = fadeColor;

            Vector2 startAnchor = Vector2.zero;
            Vector2 endAnchor = Vector2.one;

            switch (direction)
            {
                case WipeDirection.Left:
                    overlayRect.anchorMin = new Vector2(1f, 0f);
                    overlayRect.anchorMax = new Vector2(1f, 1f);
                    endAnchor = new Vector2(0f, 1f);
                    break;
                case WipeDirection.Right:
                    overlayRect.anchorMin = new Vector2(0f, 0f);
                    overlayRect.anchorMax = new Vector2(0f, 1f);
                    endAnchor = new Vector2(1f, 1f);
                    break;
                case WipeDirection.Up:
                    overlayRect.anchorMin = new Vector2(0f, 0f);
                    overlayRect.anchorMax = new Vector2(1f, 0f);
                    endAnchor = new Vector2(1f, 1f);
                    break;
                case WipeDirection.Down:
                    overlayRect.anchorMin = new Vector2(0f, 1f);
                    overlayRect.anchorMax = new Vector2(1f, 1f);
                    endAnchor = new Vector2(1f, 0f);
                    break;
            }

            overlayRect.sizeDelta = Vector2.zero;

            // Wipe in
            yield return AnimateAnchorMax(overlayRect, endAnchor, duration * 0.5f);

            onComplete?.Invoke();

            // Wipe out (reverse)
            yield return AnimateAnchorMin(overlayRect, endAnchor, duration * 0.5f);

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            _isTransitioning = false;
        }

        IEnumerator CircleCoroutine(bool expand, float duration, System.Action onComplete)
        {
            _isTransitioning = true;
            canvasGroup.blocksRaycasts = true;

            // TODO: Use shader-based circle mask for proper circular transition
            // Fallback to fade for now
            yield return FadeCoroutine(duration, onComplete);

            _isTransitioning = false;
        }

        IEnumerator CustomCoroutine(Sprite overlaySprite, float duration, System.Action onComplete)
        {
            _isTransitioning = true;
            canvasGroup.blocksRaycasts = true;

            transitionOverlay.sprite = overlaySprite;
            transitionOverlay.color = Color.white;

            // Fade in overlay
            yield return FadeCanvasGroup(canvasGroup, 0f, 1f, duration * 0.5f);

            onComplete?.Invoke();

            // Fade out overlay
            yield return FadeCanvasGroup(canvasGroup, 1f, 0f, duration * 0.5f);

            // Reset to solid color
            transitionOverlay.sprite = null;
            transitionOverlay.color = fadeColor;

            canvasGroup.blocksRaycasts = false;
            _isTransitioning = false;
        }

        IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                cg.alpha = Mathf.Lerp(from, to, transitionCurve.Evaluate(t));
                yield return null;
            }

            cg.alpha = to;
        }

        IEnumerator AnimateAnchorMax(RectTransform rect, Vector2 target, float duration)
        {
            Vector2 start = rect.anchorMax;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rect.anchorMax = Vector2.Lerp(start, target, transitionCurve.Evaluate(t));
                yield return null;
            }

            rect.anchorMax = target;
        }

        IEnumerator AnimateAnchorMin(RectTransform rect, Vector2 target, float duration)
        {
            Vector2 start = rect.anchorMin;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rect.anchorMin = Vector2.Lerp(start, target, transitionCurve.Evaluate(t));
                yield return null;
            }

            rect.anchorMin = target;
        }

        /// <summary>
        /// Check if transition is active.
        /// </summary>
        public bool IsTransitioning() => _isTransitioning;

        /// <summary>
        /// Force cancel active transition.
        /// </summary>
        public void CancelTransition()
        {
            if (_activeTransition != null)
            {
                StopCoroutine(_activeTransition);
                _activeTransition = null;
            }

            _isTransitioning = false;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        public enum WipeDirection : byte
        {
            Left = 0,
            Right = 1,
            Up = 2,
            Down = 3
        }
    }
}
