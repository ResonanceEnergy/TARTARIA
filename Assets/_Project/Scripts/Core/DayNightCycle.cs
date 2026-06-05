using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Day/Night Cycle System — drives directional light rotation for ambient lighting.
    /// 24-hour cycle mapped to 24-minute real-time (1 hour = 1 minute).
    /// Attach to scene lighting rig or create as singleton.
    /// </summary>
    public class DayNightCycle : MonoBehaviour
    {
        [Header("Time Config")]
        [SerializeField, Range(0f, 24f)] float startTimeOfDay = 12f;  // Noon
        [SerializeField] float dayLengthMinutes = 24f;  // 24 real minutes = 24 game hours
        [SerializeField] bool enableCycle = true;

        [Header("Lighting")]
        [SerializeField] Light directionalLight;
        [SerializeField] Gradient sunColor;
        [SerializeField, Range(0f, 2f)] float lightIntensityMultiplier = 1f;

        float _currentTimeOfDay;
        float _timeScale;

        public float TimeOfDay => _currentTimeOfDay;
        public float TimeProgress => _currentTimeOfDay / 24f;  // 0-1
        public bool IsDaytime => _currentTimeOfDay >= 6f && _currentTimeOfDay < 18f;

        void Awake()
        {
            _currentTimeOfDay = startTimeOfDay;
            _timeScale = 24f / (dayLengthMinutes * 60f);  // game hours per real second

            if (directionalLight == null)
            {
                directionalLight = GetComponent<Light>();
            }

            if (sunColor == null)
            {
                // Default gradient: blue sunrise → white noon → orange sunset → dark night
                sunColor = new Gradient();
                var colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.3f, 0.5f), 0f);     // Night
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.7f, 0.4f), 0.25f);   // Sunrise
                colorKeys[2] = new GradientColorKey(Color.white, 0.5f);                  // Noon
                colorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.75f);   // Sunset
                colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.2f, 0.4f), 1f);    // Night

                var alphaKeys = new GradientAlphaKey[2];
                alphaKeys[0] = new GradientAlphaKey(1f, 0f);
                alphaKeys[1] = new GradientAlphaKey(1f, 1f);

                sunColor.SetKeys(colorKeys, alphaKeys);
            }
        }

        void Update()
        {
            if (!enableCycle) return;

            // Advance time
            _currentTimeOfDay += _timeScale * Time.deltaTime;
            if (_currentTimeOfDay >= 24f)
            {
                _currentTimeOfDay -= 24f;
            }

            UpdateLighting();
        }

        void UpdateLighting()
        {
            if (directionalLight == null) return;

            // Rotate directional light (sun arc across sky)
            float rotation = (_currentTimeOfDay / 24f) * 360f - 90f;  // Noon = overhead
            directionalLight.transform.rotation = Quaternion.Euler(rotation, 170f, 0f);

            // Update color gradient
            float timeNormalized = _currentTimeOfDay / 24f;
            directionalLight.color = sunColor.Evaluate(timeNormalized);

            // Update intensity (brighter at noon, dimmer at night)
            float intensityCurve = Mathf.Clamp01(1f - Mathf.Abs((_currentTimeOfDay - 12f) / 12f));  // Peak at noon
            intensityCurve = Mathf.Pow(intensityCurve, 0.5f);  // Soften curve
            directionalLight.intensity = intensityCurve * lightIntensityMultiplier;

            // Disable shadows at night (performance optimization)
            directionalLight.shadows = (intensityCurve > 0.1f) ? LightShadows.Soft : LightShadows.None;
        }

        public void SetTimeOfDay(float hour)
        {
            _currentTimeOfDay = Mathf.Clamp(hour, 0f, 24f);
            UpdateLighting();
        }

        public void SetCycleSpeed(float speedMultiplier)
        {
            _timeScale = (24f / (dayLengthMinutes * 60f)) * speedMultiplier;
        }

        public void PauseCycle()
        {
            enableCycle = false;
        }

        public void ResumeCycle()
        {
            enableCycle = true;
        }
    }
}
