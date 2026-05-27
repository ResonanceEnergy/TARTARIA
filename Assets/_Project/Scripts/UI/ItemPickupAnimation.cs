using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tartaria.UI
{
    /// <summary>
    /// Item Pickup Animation — creates floating text and icon when player picks up items.
    ///
    /// Features:
    /// - Spawns temporary UI element at pickup location
    /// - Slides toward inventory UI with scale tween
    /// - Fades out after reaching destination
    /// - Shows item icon + count
    ///
    /// Usage:
    /// - Place ItemPickupAnimation prefab in UI Canvas
    /// - Call SpawnPickupNotification(itemIcon, itemName, count, worldPosition)
    ///
    /// NEW (Agent 14): 2026 AAA polish for item feedback
    /// </summary>
    public class ItemPickupAnimation : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField] GameObject pickupNotificationPrefab;
        [SerializeField] RectTransform inventoryTargetPosition;  // Where notifications slide to

        [Header("Animation Settings")]
        [SerializeField] float slideDuration = 0.8f;
        [SerializeField] float fadeDelay = 0.5f;
        [SerializeField] Vector3 spawnOffset = new Vector3(0, 50, 0);

        Canvas _canvas;
        UnityEngine.Camera _mainCamera; // Explicit namespace to avoid conflict with Tartaria.Camera

        public static ItemPickupAnimation Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _canvas = GetComponentInParent<Canvas>();
            _mainCamera = UnityEngine.Camera.main; // Explicit namespace
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Spawns a pickup notification with item icon and count.
        /// </summary>
        public void SpawnPickupNotification(Sprite itemIcon, string itemName, int count, Vector3 worldPosition)
        {
            if (pickupNotificationPrefab == null)
            {
                Debug.LogWarning("[ItemPickupAnimation] Pickup notification prefab not assigned");
                return;
            }

            // Instantiate notification UI
            var notificationGO = Instantiate(pickupNotificationPrefab, transform);
            var rectTransform = notificationGO.GetComponent<RectTransform>();

            if (rectTransform == null)
            {
                Debug.LogError("[ItemPickupAnimation] Pickup notification prefab must have RectTransform");
                Destroy(notificationGO);
                return;
            }

            // Set icon and text
            var iconImage = notificationGO.GetComponentInChildren<Image>();
            var textComponent = notificationGO.GetComponentInChildren<TMP_Text>();

            if (iconImage != null)
                iconImage.sprite = itemIcon;

            if (textComponent != null)
                textComponent.text = count > 1 ? $"{itemName} x{count}" : itemName;

            // Convert world position to screen position
            Vector2 screenPos;
            if (_mainCamera != null && _canvas != null)
            {
                Vector2 viewportPos = _mainCamera.WorldToViewportPoint(worldPosition);
                screenPos = new Vector2(
                    viewportPos.x * _canvas.pixelRect.width,
                    viewportPos.y * _canvas.pixelRect.height
                );
            }
            else
            {
                screenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            }

            // Set initial position
            rectTransform.anchoredPosition = screenPos + (Vector2)spawnOffset;

            // Animate slide to inventory
            Vector2 targetPos = inventoryTargetPosition != null
                ? inventoryTargetPosition.anchoredPosition
                : new Vector2(-Screen.width * 0.4f, Screen.height * 0.4f);

            AnimatePickup(rectTransform, targetPos, notificationGO);
        }

        void AnimatePickup(RectTransform rectTransform, Vector2 targetPos, GameObject notificationGO)
        {
            var canvasGroup = notificationGO.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = notificationGO.AddComponent<CanvasGroup>();

            // Scale bounce on spawn
            rectTransform.localScale = Vector3.zero;
            LeanTween.scale(notificationGO, Vector3.one, 0.3f).setEaseOutBack();

            // Slide to inventory position
            LeanTween.move(rectTransform, targetPos, slideDuration)
                .setEaseInOutQuad()
                .setDelay(0.2f);

            // Fade out near end
            LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f)
                .setDelay(slideDuration + fadeDelay)
                .setOnComplete(() =>
                {
                    Destroy(notificationGO);
                });
        }

        /// <summary>
        /// Spawns notification at screen center (for abstract pickups).
        /// </summary>
        public void SpawnPickupNotificationCentered(Sprite itemIcon, string itemName, int count)
        {
            SpawnPickupNotification(itemIcon, itemName, count, _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f)));
        }
    }
}
