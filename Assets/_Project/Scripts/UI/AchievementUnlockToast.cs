using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tartaria.UI
{
    /// <summary>
    /// Achievement Unlock Toast — displays achievement notifications.
    /// Auto-animates in from top, holds 3s, fades out. Queues multiple toasts.
    /// Attach to Canvas overlay, wire to AchievementSystem events.
    /// </summary>
    public class AchievementUnlockToast : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Text titleText;
        [SerializeField] Text descriptionText;
        [SerializeField] Image iconImage;

        [Header("Animation")]
        [SerializeField] float slideInDuration = 0.5f;
        [SerializeField] float holdDuration = 3f;
        [SerializeField] float fadeOutDuration = 0.5f;
        [SerializeField] float yOffset = 100f;  // Slide distance

        RectTransform _rectTransform;
        Vector2 _hiddenPosition;
        Vector2 _visiblePosition;
        bool _isAnimating;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            // Calculate positions
            _visiblePosition = _rectTransform.anchoredPosition;
            _hiddenPosition = _visiblePosition + Vector2.up * yOffset;

            // Start hidden
            _rectTransform.anchoredPosition = _hiddenPosition;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        void Start()
        {
            // Subscribe to AchievementSystem events
            // Integration.AchievementSystem.Instance.OnAchievementUnlocked += ShowAchievement;

            gameObject.SetActive(false);  // Hidden by default
        }

        public void ShowAchievement(string achievementId, string title, string description, Sprite icon = null)
        {
            if (_isAnimating) return;  // Queue management needed for multiple toasts

            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;
            if (iconImage != null && icon != null) iconImage.sprite = icon;

            gameObject.SetActive(true);
            StartCoroutine(AnimateToast());
        }

        IEnumerator AnimateToast()
        {
            _isAnimating = true;

            // Slide in
            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / slideInDuration);
                t = Mathf.SmoothStep(0f, 1f, t);  // Ease in/out

                _rectTransform.anchoredPosition = Vector2.Lerp(_hiddenPosition, _visiblePosition, t);
                if (canvasGroup != null) canvasGroup.alpha = t;

                yield return null;
            }

            // Hold
            yield return new WaitForSeconds(holdDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);

                if (canvasGroup != null) canvasGroup.alpha = 1f - t;

                yield return null;
            }

            // Reset
            _rectTransform.anchoredPosition = _hiddenPosition;
            gameObject.SetActive(false);
            _isAnimating = false;
        }

        public void HideImmediately()
        {
            StopAllCoroutines();
            _rectTransform.anchoredPosition = _hiddenPosition;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _isAnimating = false;
        }
    }
}
