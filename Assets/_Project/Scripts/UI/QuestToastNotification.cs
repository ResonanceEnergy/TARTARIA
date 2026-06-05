using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Integration  // Lives in Integration assembly (bridges Quest→UI)
{
    /// <summary>
    /// Quest Toast Notification — displays quest start/complete notifications.
    /// Auto-animates in from top-right, holds 3s, fades out. Queues multiple toasts.
    /// Integrates with QuestManager OnQuestStatusChanged events.
    /// </summary>
    public class QuestToastNotification : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI titleText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] Image iconImage;
        [SerializeField] Image backgroundImage;

        [Header("Animation")]
        [SerializeField] float slideInDuration = 0.5f;
        [SerializeField] float holdDuration = 3f;
        [SerializeField] float fadeOutDuration = 0.5f;
        [SerializeField] float xOffset = 400f;  // Slide distance from right

        [Header("Colors")]
        [SerializeField] Color questStartColor = new Color(0.2f, 0.7f, 1f, 1f); // Blue
        [SerializeField] Color questCompleteColor = new Color(0.3f, 1f, 0.3f, 1f); // Green

        RectTransform _rectTransform;
        Vector2 _hiddenPosition;
        Vector2 _visiblePosition;
        bool _isAnimating;
        Queue<QuestToastData> _toastQueue = new Queue<QuestToastData>();

        struct QuestToastData
        {
            public string title;
            public string description;
            public bool isComplete;
        }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            // Calculate positions (slide from right)
            _visiblePosition = _rectTransform.anchoredPosition;
            _hiddenPosition = _visiblePosition + Vector2.right * xOffset;

            // Start hidden
            _rectTransform.anchoredPosition = _hiddenPosition;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        void Start()
        {
            // Subscribe to QuestManager events
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged += OnQuestStatusChangedHandler;
            }

            gameObject.SetActive(false);  // Hidden until triggered
        }

        void OnDestroy()
        {
            // Unsubscribe
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestStatusChanged -= OnQuestStatusChangedHandler;
            }
        }

        void OnQuestStatusChangedHandler(string questId, Core.Enums.QuestStatus newStatus)
        {
            if (string.IsNullOrEmpty(questId)) return;

            // Only show notifications for Active and Completed states
            if (newStatus != Core.Enums.QuestStatus.Active && newStatus != Core.Enums.QuestStatus.Completed)
                return;

            // Get quest definition from QuestManager
            var questMgr = QuestManager.Instance;
            if (questMgr == null) return;

            // Try to get quest display name (fallback to ID)
            string displayName = questId.Replace("_", " ").ToUpper();
            string description = "";

            if (newStatus == Core.Enums.QuestStatus.Active)
            {
                description = "New Quest Started";
                Debug.Log($"[QuestToast] Quest Started: {displayName}");
            }
            else if (newStatus == Core.Enums.QuestStatus.Completed)
            {
                description = "Quest Complete!";
                Debug.Log($"[QuestToast] Quest Completed: {displayName}");
            }

            EnqueueToast(displayName, description, newStatus == Core.Enums.QuestStatus.Completed);
        }

        void EnqueueToast(string title, string description, bool isComplete)
        {
            var toastData = new QuestToastData
            {
                title = title,
                description = description,
                isComplete = isComplete
            };

            _toastQueue.Enqueue(toastData);

            // Start showing toasts if not already animating
            if (!_isAnimating)
            {
                StartCoroutine(ProcessToastQueue());
            }
        }

        IEnumerator ProcessToastQueue()
        {
            while (_toastQueue.Count > 0)
            {
                var toast = _toastQueue.Dequeue();
                yield return ShowToast(toast.title, toast.description, toast.isComplete);
            }
        }

        IEnumerator ShowToast(string title, string description, bool isComplete)
        {
            _isAnimating = true;

            // Set text content
            if (titleText != null) titleText.text = title;
            if (descriptionText != null) descriptionText.text = description;

            // Set color based on quest state
            Color targetColor = isComplete ? questCompleteColor : questStartColor;
            if (backgroundImage != null) backgroundImage.color = targetColor;

            gameObject.SetActive(true);

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

            // Slide out (back to right)
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeOutDuration);

                _rectTransform.anchoredPosition = Vector2.Lerp(_visiblePosition, _hiddenPosition, t);
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
            _toastQueue.Clear();
            _rectTransform.anchoredPosition = _hiddenPosition;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _isAnimating = false;
        }

        /// <summary>
        /// Manual trigger for testing or custom quest events.
        /// </summary>
        public void ShowManual(string title, string description, bool isComplete = false)
        {
            EnqueueToast(title, description, isComplete);
        }
    }
}
