using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Tartaria.UI
{
    /// <summary>
    /// Agent 15: Enemy health bar — floats above enemy heads, fades after 2s of no damage.
    /// Attach to enemy prefabs or create dynamically via EnemyHealthBarManager.
    /// 
    /// Features:
    /// - Worldspace canvas billboard toward camera
    /// - Smooth health drain animation
    /// - Auto-fade after 2s of no damage
    /// - Color shifts from green → yellow → red based on HP fraction
    /// 
    /// Performance: <0.5ms per frame per active bar
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image fillImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] float fadeDelay = 2f;
        [SerializeField] float fadeSpeed = 3f;
        [SerializeField] Vector3 offset = new Vector3(0f, 2.5f, 0f);

        Transform _target;
        float _currentHealth;
        float _maxHealth;
        float _displayedFraction;
        float _lastDamageTime;
        bool _isVisible;
        Canvas _canvas;

        void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = UnityEngine.Camera.main;

            // Auto-create UI elements if not assigned
            if (fillImage == null || backgroundImage == null || canvasGroup == null)
                CreateUIElements();

            _isVisible = false;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        void CreateUIElements()
        {
            // Create canvas group if missing
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Create background
            if (backgroundImage == null)
            {
                var bgGO = new GameObject("Background");
                bgGO.transform.SetParent(transform, false);
                backgroundImage = bgGO.AddComponent<Image>();
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                var bgRect = backgroundImage.rectTransform;
                bgRect.sizeDelta = new Vector2(100f, 12f);
            }

            // Create fill
            if (fillImage == null)
            {
                var fillGO = new GameObject("Fill");
                fillGO.transform.SetParent(backgroundImage.transform, false);
                fillImage = fillGO.AddComponent<Image>();
                fillImage.color = Color.green;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                fillImage.fillAmount = 1f;
                var fillRect = fillImage.rectTransform;
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.sizeDelta = Vector2.zero;
            }
        }

        public void Initialize(Transform target, float maxHealth)
        {
            _target = target;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
            _displayedFraction = 1f;
            _lastDamageTime = Time.time;
            _isVisible = false;

            if (fillImage != null)
                fillImage.fillAmount = 1f;
        }

        public void UpdateHealth(float currentHealth)
        {
            _currentHealth = currentHealth;
            _lastDamageTime = Time.time;
            _isVisible = true;
        }

        void LateUpdate()
        {
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            // Position above target
            transform.position = _target.position + offset;

            // Billboard toward camera
            if (UnityEngine.Camera.main != null)
                transform.rotation = UnityEngine.Camera.main.transform.rotation;

            // Smooth health drain
            float targetFraction = Mathf.Clamp01(_currentHealth / Mathf.Max(1f, _maxHealth));
            _displayedFraction = Mathf.MoveTowards(_displayedFraction, targetFraction, Time.deltaTime * 2f);

            if (fillImage != null)
            {
                fillImage.fillAmount = _displayedFraction;

                // Color shift based on HP fraction
                if (_displayedFraction > 0.6f)
                    fillImage.color = Color.Lerp(Color.yellow, Color.green, (_displayedFraction - 0.6f) / 0.4f);
                else if (_displayedFraction > 0.3f)
                    fillImage.color = Color.Lerp(Color.red, Color.yellow, (_displayedFraction - 0.3f) / 0.3f);
                else
                    fillImage.color = Color.red;
            }

            // Auto-fade after delay
            if (canvasGroup != null)
            {
                float timeSinceDamage = Time.time - _lastDamageTime;
                if (timeSinceDamage > fadeDelay)
                {
                    _isVisible = false;
                }

                float targetAlpha = _isVisible ? 1f : 0f;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }
        }
    }
}
