using UnityEngine;
using Tartaria.Core;

namespace Tartaria.UI
{
    /// <summary>
    /// HUD Controller — manages the in-game overlay:
    ///   - RS gauge (bottom-center, circular with golden ratio spiral)
    ///   - Aether charge bar (top-right)
    ///   - Boss health bar (top-center, shown during boss encounters)
    ///   - Wave counter (top-center-left, shown during combat waves)
    ///   - Achievement toast (top-right stacked, fades after 3s)
    ///   - Moon-complete trophy banner (center, shown on zone victory)
    ///   - Interaction prompt (contextual, center-bottom)
    ///   - Tuning mini-game overlay
    ///   - Compass / zone indicator (top-center)
    ///
    /// Uses Unity UI Toolkit (retained mode) as specified in tech spec.
    /// </summary>
    public class HUDController : MonoBehaviour, IHUDService
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

        [Header("Boss Health Bar")]
        [SerializeField] RectTransform bossHealthPanel;
        [SerializeField] UnityEngine.UI.Image bossHealthFill;
        [SerializeField] TMPro.TextMeshProUGUI bossNameText;
        [SerializeField] Color bossHealthColor = new Color(0.8f, 0.15f, 0.1f);
        [SerializeField] Color bossHealthLowColor = new Color(0.9f, 0.3f, 0.05f);

        [Header("Wave Counter")]
        [SerializeField] RectTransform waveCounterPanel;
        [SerializeField] TMPro.TextMeshProUGUI waveCounterText;
        [SerializeField] TMPro.TextMeshProUGUI waveEnemiesText;

        [Header("Achievement Toast")]
        [SerializeField] RectTransform achievementToastPanel;
        [SerializeField] TMPro.TextMeshProUGUI achievementToastText;
        [SerializeField] float achievementDisplayDuration = 3f;

        [Header("Moon Trophy Banner")]
        [SerializeField] RectTransform moonTrophyPanel;
        [SerializeField] TMPro.TextMeshProUGUI moonTrophyText;
        [SerializeField] TMPro.TextMeshProUGUI moonTrophySubtext;
        [SerializeField] float trophyDisplayDuration = 5f;

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
        int _lastRSInt = -1;
        int _lastAetherInt = -1;

        // Boss health state
        float _bossHealthTarget;
        bool _bossBarVisible;

        // Wave counter state
        int _currentWave;
        int _totalWaves;
        int _enemiesRemaining;
        bool _waveCounterVisible;

        // Achievement toast state
        float _achievementTimer;
        bool _achievementVisible;

        // Moon trophy state
        float _trophyTimer;
        bool _trophyVisible;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.HUD = this;
        }

        void Start()
        {
            // Hide combat/achievement/trophy panels on start
            if (bossHealthPanel != null) bossHealthPanel.gameObject.SetActive(false);
            if (waveCounterPanel != null) waveCounterPanel.gameObject.SetActive(false);
            if (achievementToastPanel != null) achievementToastPanel.gameObject.SetActive(false);
            if (moonTrophyPanel != null) moonTrophyPanel.gameObject.SetActive(false);

            // Subscribe to accessibility changes for text scale / high contrast
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.OnSettingsChanged += HandleAccessibilityChanged;
        }

        void OnDestroy()
        {
            if (AccessibilityManager.Instance != null)
                AccessibilityManager.Instance.OnSettingsChanged -= HandleAccessibilityChanged;
        }

        void Update()
        {
            UpdateRSDisplay();
            UpdateAetherDisplay();
            UpdatePromptFade();
            UpdateBossHealthBar();
            UpdateAchievementToast();
            UpdateMoonTrophy();
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

            int rsInt = Mathf.RoundToInt(targetRS);
            if (rsInt != _lastRSInt)
            {
                _lastRSInt = rsInt;
                if (rsValueText != null)
                    rsValueText.text = rsInt.ToString();

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

            int aetherInt = Mathf.RoundToInt(_displayAether);
            if (aetherInt != _lastAetherInt)
            {
                _lastAetherInt = aetherInt;
                if (aetherValueText != null)
                    aetherValueText.text = $"{aetherInt}%";
            }
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

        // ─── Boss Health Bar ─────────────────────────

        public void ShowBossHealth(string bossName, float normalizedHealth)
        {
            _bossBarVisible = true;
            _bossHealthTarget = Mathf.Clamp01(normalizedHealth);
            if (bossHealthPanel != null)
                bossHealthPanel.gameObject.SetActive(true);
            if (bossNameText != null)
                bossNameText.text = bossName;
        }

        public void UpdateBossHealth(float normalizedHealth)
        {
            _bossHealthTarget = Mathf.Clamp01(normalizedHealth);
        }

        public void HideBossHealth()
        {
            _bossBarVisible = false;
            if (bossHealthPanel != null)
                bossHealthPanel.gameObject.SetActive(false);
        }

        void UpdateBossHealthBar()
        {
            if (!_bossBarVisible || bossHealthFill == null) return;

            bossHealthFill.fillAmount = Mathf.Lerp(bossHealthFill.fillAmount,
                _bossHealthTarget, Time.deltaTime * 5f);

            // Color shifts to orange-red at low health
            bossHealthFill.color = _bossHealthTarget < 0.25f
                ? Color.Lerp(bossHealthLowColor, bossHealthColor, _bossHealthTarget / 0.25f)
                : bossHealthColor;
        }

        // ─── Wave Counter ────────────────────────────

        public void ShowWaveCounter(int currentWave, int totalWaves, int enemiesRemaining)
        {
            _currentWave = currentWave;
            _totalWaves = totalWaves;
            _enemiesRemaining = enemiesRemaining;
            _waveCounterVisible = true;
            if (waveCounterPanel != null)
                waveCounterPanel.gameObject.SetActive(true);
            RefreshWaveText();
        }

        public void UpdateWaveEnemies(int remaining)
        {
            _enemiesRemaining = remaining;
            RefreshWaveText();
        }

        public void AdvanceWave(int newWave)
        {
            _currentWave = newWave;
            RefreshWaveText();
            if (waveCounterPanel != null)
                StartCoroutine(PulseScale(waveCounterPanel, 1.2f, 0.25f));
        }

        public void HideWaveCounter()
        {
            _waveCounterVisible = false;
            if (waveCounterPanel != null)
                waveCounterPanel.gameObject.SetActive(false);
        }

        void RefreshWaveText()
        {
            if (!_waveCounterVisible) return;
            if (waveCounterText != null)
                waveCounterText.text = LocalizationManager.Get("hud_wave", _currentWave + 1, _totalWaves);
            if (waveEnemiesText != null)
                waveEnemiesText.text = LocalizationManager.Get("hud_enemies_remaining", _enemiesRemaining);
        }

        // ─── Achievement Toast ───────────────────────

        public void ShowAchievementToast(string title)
        {
            _achievementVisible = true;
            _achievementTimer = achievementDisplayDuration;
            if (achievementToastPanel != null)
                achievementToastPanel.gameObject.SetActive(true);
            if (achievementToastText != null)
                achievementToastText.text = title;
            if (achievementToastPanel != null)
                StartCoroutine(PulseScale(achievementToastPanel, 1.1f, 0.2f));
        }

        void UpdateAchievementToast()
        {
            if (!_achievementVisible) return;
            _achievementTimer -= Time.deltaTime;
            if (_achievementTimer <= 0f)
            {
                _achievementVisible = false;
                if (achievementToastPanel != null)
                    achievementToastPanel.gameObject.SetActive(false);
            }
        }

        // ─── Moon Trophy Banner ──────────────────────

        public void ShowMoonTrophy(string moonName, string subtitle)
        {
            _trophyVisible = true;
            _trophyTimer = trophyDisplayDuration;
            if (moonTrophyPanel != null)
            {
                moonTrophyPanel.gameObject.SetActive(true);
                StartCoroutine(PulseScale(moonTrophyPanel, 1.15f, 0.4f));
            }
            if (moonTrophyText != null)
                moonTrophyText.text = moonName;
            if (moonTrophySubtext != null)
                moonTrophySubtext.text = subtitle;
        }

        void UpdateMoonTrophy()
        {
            if (!_trophyVisible) return;
            _trophyTimer -= Time.deltaTime;
            if (_trophyTimer <= 0f)
            {
                _trophyVisible = false;
                if (moonTrophyPanel != null)
                    moonTrophyPanel.gameObject.SetActive(false);
            }
        }

        // ─── Animation Utility ───────────────────────

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

        // ─── Accessibility Refresh ───────────────────

        void HandleAccessibilityChanged()
        {
            var am = AccessibilityManager.Instance;
            if (am == null) return;

            float scale = am.TextScale;

            // Apply text scale to all TMP text components
            if (rsValueText != null) rsValueText.fontSize = 24f * scale;
            if (aetherValueText != null) aetherValueText.fontSize = 18f * scale;
            if (zoneNameText != null) zoneNameText.fontSize = 20f * scale;
            if (interactionText != null) interactionText.fontSize = 18f * scale;
            if (bossNameText != null) bossNameText.fontSize = 22f * scale;
            if (waveCounterText != null) waveCounterText.fontSize = 20f * scale;
            if (waveEnemiesText != null) waveEnemiesText.fontSize = 16f * scale;
            if (achievementToastText != null) achievementToastText.fontSize = 18f * scale;
            if (moonTrophyText != null) moonTrophyText.fontSize = 28f * scale;
            if (moonTrophySubtext != null) moonTrophySubtext.fontSize = 18f * scale;

            // High contrast mode — boost text alpha if needed
            if (am.HighContrast)
            {
                ApplyHighContrast(rsValueText);
                ApplyHighContrast(aetherValueText);
                ApplyHighContrast(zoneNameText);
                ApplyHighContrast(bossNameText);
                ApplyHighContrast(achievementToastText);
                ApplyHighContrast(moonTrophyText);
            }

            Debug.Log($"[HUD] Accessibility refreshed: scale={scale}, highContrast={am.HighContrast}");
        }

        static void ApplyHighContrast(TMPro.TextMeshProUGUI text)
        {
            if (text == null) return;
            var c = text.color;
            text.color = new Color(c.r, c.g, c.b, 1f);
            text.fontStyle |= TMPro.FontStyles.Bold;
        }
    }
}
