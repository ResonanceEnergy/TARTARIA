using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// DayNightCycleController — 17-hour visual cycle (sped up for demo).
    /// </summary>
    public class DayNightCycleController : MonoBehaviour
    {
        public static DayNightCycleController Instance { get; private set; }

        [Header("Cycle Settings")]
        [SerializeField] private float cycleDurationSeconds = 300f; // 5 minutes for demo
        [SerializeField] private float currentTime = 0.5f; // Start at noon (0.5 = 12:00)
        [SerializeField] private bool pauseCycle = false;

        [Header("Lighting")]
        [SerializeField] private Light directionalLight;
        [SerializeField] private Gradient sunColorGradient;
        [SerializeField] private AnimationCurve sunIntensityCurve;

        [Header("Skybox")]
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;

        [Header("Aether Boost")]
        [SerializeField] private float nighttimeAetherBoost = 1.2f; // +20% at night

        private float _timeSpeed;
        private bool _wasNight;
        private float _lastAppliedBoost = 1f;

        /// <summary>
        /// Current Aether yield multiplier driven by the day/night cycle.
        /// Mirrors <see cref="Tartaria.Gameplay.DayNightController.AetherYieldMultiplier"/>
        /// so ExcavationSystem / AetherFieldSystem can read the boost regardless of
        /// which day-night controller is active in the scene.
        /// </summary>
        public static float AetherYieldMultiplier { get; private set; } = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            _timeSpeed = 1f / cycleDurationSeconds;

            // Find directional light if not assigned
            if (directionalLight == null)
            {
                directionalLight = FindFirstObjectByType<Light>(FindObjectsInactive.Exclude);
                if (directionalLight != null && directionalLight.type != LightType.Directional)
                    directionalLight = null;
            }

            // Initialize gradients if not assigned
            if (sunColorGradient == null)
            {
                sunColorGradient = new Gradient
                {
                    colorKeys = new GradientColorKey[]
                    {
                        new(new Color(0.2f, 0.2f, 0.3f), 0f),    // Night
                        new(new Color(1f, 0.6f, 0.4f), 0.25f),   // Dawn
                        new(Color.white, 0.5f),                  // Noon
                        new(new Color(1f, 0.5f, 0.3f), 0.75f),   // Dusk
                        new(new Color(0.2f, 0.2f, 0.3f), 1f)     // Night
                    }
                };
            }

            if (sunIntensityCurve == null)
            {
                sunIntensityCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 0.3f);
            }

            // Seed transition state so first day<->night flip fires the log
            _wasNight = currentTime < 0.25f || currentTime > 0.75f;
            AetherYieldMultiplier = _wasNight ? nighttimeAetherBoost : 1f;
            _lastAppliedBoost = AetherYieldMultiplier;

            Debug.Log($"[DayNightCycleController] ✅ 17-hour cycle initialized (night={_wasNight}, boost={_lastAppliedBoost:F2})");
        }

        void Update()
        {
            if (pauseCycle) return;

            // Advance time
            currentTime += _timeSpeed * Time.deltaTime;
            if (currentTime > 1f) currentTime = 0f;

            UpdateLighting();
            UpdateSkybox();
            UpdateAetherBoost();
        }

        void UpdateLighting()
        {
            if (directionalLight == null) return;

            // Rotate sun (0 = midnight, 0.5 = noon)
            float sunAngle = currentTime * 360f - 90f; // -90 offset so 0.5 = overhead
            directionalLight.transform.rotation = Quaternion.Euler(sunAngle, 0f, 0f);

            // Update color and intensity
            directionalLight.color = sunColorGradient.Evaluate(currentTime);
            directionalLight.intensity = sunIntensityCurve.Evaluate(currentTime);
        }

        void UpdateSkybox()
        {
            // Lerp between day and night skyboxes
            if (daySkybox != null && nightSkybox != null)
            {
                float dayWeight = sunIntensityCurve.Evaluate(currentTime);
                // Note: Skybox lerping requires custom shader or RenderSettings.skybox swapping
                // For now, just swap at threshold
                RenderSettings.skybox = dayWeight > 0.5f ? daySkybox : nightSkybox;
            }
        }

        void UpdateAetherBoost()
        {
            // Night = +20% Aether yield (0.0-0.25 and 0.75-1.0)
            bool isNight = currentTime < 0.25f || currentTime > 0.75f;
            float boost = isNight ? nighttimeAetherBoost : 1f;

            // Sprint 12 #3: previously commented out, restored.
            // Static field consumed by ExcavationSystem / AetherFieldSystem.
            // Mirrors the convention already in Tartaria.Gameplay.DayNightController.
            float previousBoost = AetherYieldMultiplier;
            AetherYieldMultiplier = boost;

            // Log only on day<->night transition so we can see the boost firing
            // without spamming Update.
            if (isNight != _wasNight)
            {
                Debug.Log($"[DayNightCycle] aether boost applied: {previousBoost:F2} → {boost:F2} (night={isNight})");
                _wasNight = isNight;
                _lastAppliedBoost = boost;
            }
        }

        public float GetCurrentTimeOfDay() => currentTime;
        public bool IsNighttime() => currentTime < 0.25f || currentTime > 0.75f;
        public void SetTime(float time) => currentTime = Mathf.Clamp01(time);
        public void PauseCycle(bool pause) => pauseCycle = pause;
    }
}
