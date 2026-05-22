using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Tartaria.UI
{
    /// <summary>
    /// UINotificationStack — toast message system for non-intrusive player notifications.
    /// Stacks vertically, auto-fades after duration, supports icons + colors.
    /// Used for: item pickup, quest updates, achievements, combat feedback.
    /// 
    /// Toast Types:
    /// - Info (blue) → general notifications
    /// - Success (green) → achievements, quest complete
    /// - Warning (yellow) → low health, cooldown ready
    /// - Error (red) → failed action, blocked
    /// 
    /// Features:
    /// - Slide-in from right
    /// - Auto-fade after 3s
    /// - Max 5 toasts visible
    /// - FIFO queue when overflow
    /// - Click to dismiss
    /// 
    /// Usage:
    /// - UINotificationStack.Instance.ShowToast("Item Collected: Crystal Shard", ToastType.Success)
    /// - UINotificationStack.Instance.ShowToastWithIcon("Achievement Unlocked", achievementSprite, ToastType.Success)
    /// 
    /// GDD refs: §05 (UI/UX Polish), §06 (Player Feedback)
    /// </summary>
    public class UINotificationStack : MonoBehaviour
    {
        public static UINotificationStack Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] Transform toastContainer;  // Parent for toast entries
        [SerializeField] GameObject toastPrefab;

        [Header("Settings")]
        [SerializeField] int maxVisibleToasts = 5;
        [SerializeField] float toastDuration = 3f;
        [SerializeField] float slideInDuration = 0.3f;
        [SerializeField] float fadeOutDuration = 0.3f;

        [Header("Colors")]
        [SerializeField] Color infoColor = new Color(0.3f, 0.7f, 1f);
        [SerializeField] Color successColor = new Color(0.3f, 1f, 0.4f);
        [SerializeField] Color warningColor = new Color(1f, 0.9f, 0.3f);
        [SerializeField] Color errorColor = new Color(1f, 0.3f, 0.3f);

        readonly Queue<ToastData> _toastQueue = new();
        readonly List<ToastEntry> _activeToasts = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Show toast notification with text only.
        /// </summary>
        public void ShowToast(string message, ToastType type = ToastType.Info)
        {
            ShowToastWithIcon(message, null, type);
        }

        /// <summary>
        /// Show toast notification with icon.
        /// </summary>
        public void ShowToastWithIcon(string message, Sprite icon, ToastType type = ToastType.Info)
        {
            var toastData = new ToastData
            {
                message = message,
                icon = icon,
                type = type
            };

            // Queue if at capacity
            if (_activeToasts.Count >= maxVisibleToasts)
            {
                _toastQueue.Enqueue(toastData);
                return;
            }

            SpawnToast(toastData);
        }

        void SpawnToast(ToastData data)
        {
            if (toastPrefab == null || toastContainer == null)
            {
                Debug.LogWarning("[UINotificationStack] Toast prefab or container missing");
                return;
            }

            GameObject toastGO = Instantiate(toastPrefab, toastContainer);
            var toast = toastGO.GetComponent<ToastEntry>();
            if (toast == null)
            {
                toast = toastGO.AddComponent<ToastEntry>();
            }

            Color toastColor = GetColorForType(data.type);
            toast.Initialize(data.message, data.icon, toastColor);
            toast.onDismissed += () => OnToastDismissed(toast);

            _activeToasts.Add(toast);

            // Animate in
            StartCoroutine(AnimateToastIn(toast));

            // Auto-dismiss after duration
            StartCoroutine(DismissToastAfterDelay(toast, toastDuration));

            Debug.Log($"[UINotificationStack] Toast: {data.message} ({data.type})");
        }

        IEnumerator AnimateToastIn(ToastEntry toast)
        {
            var rectTransform = toast.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            Vector3 startPos = rectTransform.anchoredPosition;
            startPos.x += 300f;  // Start off-screen right

            Vector3 endPos = rectTransform.anchoredPosition;

            rectTransform.anchoredPosition = startPos;

            float elapsed = 0f;
            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            rectTransform.anchoredPosition = endPos;
        }

        IEnumerator DismissToastAfterDelay(ToastEntry toast, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (toast != null && _activeToasts.Contains(toast))
            {
                DismissToast(toast);
            }
        }

        void DismissToast(ToastEntry toast)
        {
            StartCoroutine(AnimateToastOut(toast));
        }

        IEnumerator AnimateToastOut(ToastEntry toast)
        {
            var canvasGroup = toast.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = toast.gameObject.AddComponent<CanvasGroup>();
            }

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeOutDuration;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
                yield return null;
            }

            OnToastDismissed(toast);
        }

        void OnToastDismissed(ToastEntry toast)
        {
            _activeToasts.Remove(toast);
            Destroy(toast.gameObject);

            // Process queue
            if (_toastQueue.Count > 0)
            {
                var nextToast = _toastQueue.Dequeue();
                SpawnToast(nextToast);
            }
        }

        Color GetColorForType(ToastType type)
        {
            return type switch
            {
                ToastType.Info => infoColor,
                ToastType.Success => successColor,
                ToastType.Warning => warningColor,
                ToastType.Error => errorColor,
                _ => infoColor
            };
        }

        /// <summary>
        /// Clear all active toasts.
        /// </summary>
        public void ClearAllToasts()
        {
            foreach (var toast in _activeToasts)
            {
                if (toast != null) Destroy(toast.gameObject);
            }
            _activeToasts.Clear();
            _toastQueue.Clear();
        }

        struct ToastData
        {
            public string message;
            public Sprite icon;
            public ToastType type;
        }

        public enum ToastType : byte
        {
            Info = 0,
            Success = 1,
            Warning = 2,
            Error = 3
        }
    }

    /// <summary>
    /// Individual toast entry component.
    /// </summary>
    public class ToastEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] TextMeshProUGUI messageText;
        [SerializeField] Image iconImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] Button dismissButton;

        public System.Action onDismissed;

        public void Initialize(string message, Sprite icon, Color backgroundColor)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.gameObject.SetActive(true);
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(OnDismissClicked);
            }
        }

        void OnDismissClicked()
        {
            onDismissed?.Invoke();
        }
    }
}
