using UnityEngine;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// HUD Controller — manages the in-game overlay:
    ///   - RS gauge (bottom-center, circular with golden ratio spiral)
    ///   - Aether charge bar (top-right)
    ///   - Interaction prompt (contextual, center-bottom)
    ///   - Tuning mini-game overlay
    ///   - Compass / zone indicator (top-center)
    ///
    /// Uses Unity UI Toolkit (retained mode) as specified in tech spec.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("HUD References")]
        [SerializeField] RectTransform rsGauge;
        [SerializeField] UnityEngine.UI.Image rsFillImage;
        [SerializeField] TMPro.TextMeshProUGUI rsValueText;
        [SerializeField] UnityEngine.UI.Image aetherChargeBar;
        [SerializeField] TMPro.TextMeshProUGUI aetherValueText;
        [SerializeField] RectTransform interactionPrompt;
        [SerializeField] TMPro.TextMeshProUGUI interactionText;
        [SerializeField] TMPro.TextMeshProUGUI zoneNameText;

        [Header("RS Threshold Markers")]
        [SerializeField] GameObject[] thresholdMarkers; // 4 markers at 25/50/75/100

        [Header("Colors")]
        [SerializeField] Color rsLowColor = new Color(0.3f, 0.2f, 0.4f);      // dim purple
        [SerializeField] Color rsMidColor = new Color(0.6f, 0.5f, 0.1f);       // amber-gold
        [SerializeField] Color rsHighColor = new Color(0.9f, 0.85f, 0.3f);     // bright gold
        [SerializeField] Color rsMaxColor = new Color(1f, 0.95f, 0.6f);        // radiant
        [SerializeField] Color aetherColor = new Color(0.2f, 0.6f, 0.9f);      // cyan-blue

        float _displayRS;
        float _displayAether;
        float _promptFadeTimer;
        bool _promptVisible;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            UpdateRSDisplay();
            UpdateAetherDisplay();
            UpdatePromptFade();
        }

        // ─── RS Gauge ──────────────────────────────

        void UpdateRSDisplay()
        {
            // Smooth towards actual value
            float targetRS = _displayRS; // Set via UpdateRS()
            if (rsFillImage != null)
            {
                rsFillImage.fillAmount = Mathf.Lerp(rsFillImage.fillAmount, targetRS / 100f, Time.deltaTime * 4f);
                rsFillImage.color = GetRSColor(targetRS);
            }

            if (rsValueText != null)
                rsValueText.text = Mathf.RoundToInt(targetRS).ToString();

            // Threshold markers glow when reached
            if (thresholdMarkers != null)
            {
                for (int i = 0; i < thresholdMarkers.Length && i < 4; i++)
                {
                    float threshold = (i + 1) * 25f;
                    if (thresholdMarkers[i] != null)
                        thresholdMarkers[i].SetActive(targetRS >= threshold);
                }
            }
        }

        Color GetRSColor(float rs)
        {
            if (rs < 25f) return Color.Lerp(rsLowColor, rsMidColor, rs / 25f);
            if (rs < 50f) return Color.Lerp(rsMidColor, rsHighColor, (rs - 25f) / 25f);
            if (rs < 75f) return Color.Lerp(rsHighColor, rsMaxColor, (rs - 50f) / 25f);
            return rsMaxColor;
        }

        // ─── Aether Charge ──────────────────────────

        void UpdateAetherDisplay()
        {
            if (aetherChargeBar != null)
            {
                aetherChargeBar.fillAmount = Mathf.Lerp(aetherChargeBar.fillAmount,
                    _displayAether / 100f, Time.deltaTime * 6f);
                aetherChargeBar.color = aetherColor;
            }

            if (aetherValueText != null)
                aetherValueText.text = $"{Mathf.RoundToInt(_displayAether)}%";
        }

        // ─── Interaction Prompt ──────────────────────

        void UpdatePromptFade()
        {
            if (!_promptVisible && interactionPrompt != null)
            {
                _promptFadeTimer -= Time.deltaTime;
                if (_promptFadeTimer <= 0f)
                    interactionPrompt.gameObject.SetActive(false);
            }
        }

        // ─── Public API ──────────────────────────────

        public void UpdateRS(float currentRS)
        {
            _displayRS = currentRS;
        }

        public void UpdateAetherCharge(float charge)
        {
            _displayAether = charge;
        }

        public void ShowInteractionPrompt(string text)
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.gameObject.SetActive(true);
                _promptVisible = true;
            }
            if (interactionText != null)
                interactionText.text = text;
        }

        public void HideInteractionPrompt()
        {
            _promptVisible = false;
            _promptFadeTimer = 0.3f; // Brief fade delay
        }

        public void SetZoneName(string zoneName)
        {
            if (zoneNameText != null)
                zoneNameText.text = zoneName;
        }

        public void FlashRSGain(float amount)
        {
            // Pulse animation on RS gain
            if (rsGauge != null)
                StartCoroutine(PulseScale(rsGauge, 1.15f, 0.3f));
        }

        System.Collections.IEnumerator PulseScale(RectTransform target, float peakScale, float duration)
        {
            Vector3 original = target.localScale;
            Vector3 peak = original * peakScale;
            float half = duration * 0.5f;

            float t = 0;
            while (t < half)
            {
                target.localScale = Vector3.Lerp(original, peak, t / half);
                t += Time.deltaTime;
                yield return null;
            }
            t = 0;
            while (t < half)
            {
                target.localScale = Vector3.Lerp(peak, original, t / half);
                t += Time.deltaTime;
                yield return null;
            }
            target.localScale = original;
        }
    }
}
