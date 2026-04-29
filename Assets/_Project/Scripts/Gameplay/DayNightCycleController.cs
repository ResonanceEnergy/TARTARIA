using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Day/Night Cycle Controller — rotates the main Directional Light
    /// to simulate Tartaria's 17-hour day. Adjusts ambient light intensity
    /// and color based on time of day. Integrates with moon phase to
    /// modulate Aether yield.
    ///
    /// Attach this to the scene's Directional Light GameObject.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class DayNightCycleController : MonoBehaviour
    {
        [Header("Cycle Settings")]
        [SerializeField, Min(60f)] float cycleDuration = 17f * 60f; // 17 minutes in real-time = 1 Tartarian day
        [SerializeField] float initialTimeOfDay = 0.25f; // Start at dawn (6am equivalent)

        [Header("Sun Colors")]
        [SerializeField] Color dawnColor = new(1.0f, 0.7f, 0.5f, 1.0f);
        [SerializeField] Color noonColor = new(1.0f, 1.0f, 0.95f, 1.0f);
        [SerializeField] Color duskColor = new(1.0f, 0.6f, 0.4f, 1.0f);
        [SerializeField] Color nightColor = new(0.3f, 0.4f, 0.6f, 1.0f);

        [Header("Ambient Light")]
        [SerializeField, Range(0f, 1f)] float nightAmbient = 0.4f;
        [SerializeField, Range(0f, 2f)] float dayAmbient = 1.0f;

        Light _light;
        float _currentTime;
        int _lastMoonPhase = -1;

        void Awake()
        {
            _light = GetComponent<Light>();
            _currentTime = initialTimeOfDay * cycleDuration;
        }

        void Start()
        {
            int moonPhase = GameStateManager.Instance.CurrentMoonPhase;
            Debug.Log($"[DayNight] Initialized, cycle {cycleDuration}s, moon phase {moonPhase}");
        }

        void Update()
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= cycleDuration)
                _currentTime -= cycleDuration;

            float timeOfDay = _currentTime / cycleDuration; // 0 = midnight, 0.5 = noon

            // Rotate sun around X-axis: 0° at midnight, 180° at noon, 360° wraps
            float rotationDegrees = (timeOfDay * 360f) % 360f;
            transform.rotation = Quaternion.Euler(rotationDegrees, 170f, 0f);

            // Interpolate sun color through 4 phases
            Color sunColor;
            if (timeOfDay < 0.25f) // Midnight → Dawn
                sunColor = Color.Lerp(nightColor, dawnColor, timeOfDay / 0.25f);
            else if (timeOfDay < 0.5f) // Dawn → Noon
                sunColor = Color.Lerp(dawnColor, noonColor, (timeOfDay - 0.25f) / 0.25f);
            else if (timeOfDay < 0.75f) // Noon → Dusk
                sunColor = Color.Lerp(noonColor, duskColor, (timeOfDay - 0.5f) / 0.25f);
            else // Dusk → Midnight
                sunColor = Color.Lerp(duskColor, nightColor, (timeOfDay - 0.75f) / 0.25f);

            _light.color = sunColor;

            // Adjust ambient light based on time of day (peaks at noon)
            float dayProgress = 1f - Mathf.Abs(timeOfDay - 0.5f) * 2f; // 0 at midnight, 1 at noon
            RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbient, dayAmbient, dayProgress);

            // Poll moon phase changes (could affect Aether yield multiplier in other systems)
            int moonPhase = GameStateManager.Instance.CurrentMoonPhase;
            if (moonPhase != _lastMoonPhase)
            {
                _lastMoonPhase = moonPhase;
                Debug.Log($"[DayNight] Moon phase changed to {moonPhase}, Aether yield ×{GetAetherMultiplier():F2}");
            }
        }

        /// <summary>
        /// Returns Aether yield multiplier based on current moon phase.
        /// Range: 1.0 (New Moon) to 1.1 (Full Moon).
        /// </summary>
        public float GetAetherMultiplier()
        {
            int phase = GameStateManager.Instance.CurrentMoonPhase;
            // Simple linear mapping: phase 0 = 1.0x, phase 12 = 1.1x (assuming 13 moon campaign)
            return 1.0f + (phase / 13f) * 0.1f;
        }

        /// <summary>
        /// Gets normalized time of day (0 = midnight, 0.5 = noon, 1 = midnight next day).
        /// </summary>
        public float TimeOfDay => _currentTime / cycleDuration;

        /// <summary>
        /// Gets current day/night phase as a string (for UI/debugging).
        /// </summary>
        public string CurrentPhase
        {
            get
            {
                float t = TimeOfDay;
                if (t < 0.2f || t > 0.8f) return "Night";
                if (t < 0.35f) return "Dawn";
                if (t < 0.65f) return "Day";
                return "Dusk";
            }
        }
    }
}
