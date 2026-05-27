using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Day/Night Cycle — 17-hour Tartarian cycle (432 Hz = 17x solar hour)
    /// Rotates directional light, lerps skybox, +20% Aether yield at night
    /// </summary>
    public class DayNightController : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField, Tooltip("Seconds for one full 17-hour cycle")]
        float cycleDuration = 1020f; // 17 minutes realtime = 17 game hours

        [SerializeField, Tooltip("Start time (0=midnight, 0.5=noon)")]
        [Range(0f, 1f)] float startTime = 0.25f; // Start at dawn

        [Header("Lighting")]
        [SerializeField] Light directionalLight;
        [SerializeField] Gradient lightColorGradient;
        [SerializeField] AnimationCurve lightIntensityCurve;

        [Header("Skybox")]
        [SerializeField] Material daySkybox;
        [SerializeField] Material nightSkybox;

        [Header("Aether Modifier")]
        [SerializeField, Tooltip("Aether yield multiplier at night")]
        float nightAetherBonus = 1.2f;

        float _currentTime;

        void Start()
        {
            _currentTime = startTime;

            if (directionalLight == null)
            {
                directionalLight = GameObject.FindWithTag("MainLight")?.GetComponent<Light>();
                if (directionalLight == null)
                {
                    var sun = GameObject.Find("Directional Light");
                    if (sun != null) directionalLight = sun.GetComponent<Light>();
                }
            }

            if (directionalLight == null)
                Debug.LogWarning("[DayNight] No directional light found. Create one tagged 'MainLight'.");

            // Initialize gradients if not set
            if (lightColorGradient == null || lightColorGradient.colorKeys.Length == 0)
            {
                lightColorGradient = new Gradient();
                var colorKeys = new GradientColorKey[5];
                colorKeys[0] = new GradientColorKey(new Color(0.2f, 0.3f, 0.5f), 0.0f);   // Midnight blue
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.6f, 0.4f), 0.25f);    // Dawn orange
                colorKeys[2] = new GradientColorKey(new Color(1f, 0.95f, 0.9f), 0.5f);    // Noon white
                colorKeys[3] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.75f);    // Dusk orange
                colorKeys[4] = new GradientColorKey(new Color(0.1f, 0.2f, 0.4f), 1.0f);   // Night blue
                lightColorGradient.colorKeys = colorKeys;
            }

            if (lightIntensityCurve == null || lightIntensityCurve.length == 0)
            {
                lightIntensityCurve = AnimationCurve.EaseInOut(0f, 0.1f, 1f, 0.1f);
                lightIntensityCurve.AddKey(0.5f, 1.5f); // Peak at noon
            }

            Debug.Log($"[DayNight] Initialized 17-hour cycle ({cycleDuration}s realtime)");
        }

        void Update()
        {
            if (GameStateManager.Instance?.IsPaused == true) return;

            // Advance time
            _currentTime += Time.deltaTime / cycleDuration;
            if (_currentTime >= 1f) _currentTime -= 1f;

            UpdateLighting();
            UpdateSkybox();
            UpdateAetherModifier();
        }

        void UpdateLighting()
        {
            if (directionalLight == null) return;

            // Rotate light (0=midnight pointing down, 0.5=noon pointing down from opposite side)
            float angle = _currentTime * 360f - 90f; // -90 offset so noon is overhead
            directionalLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);

            // Update color and intensity
            directionalLight.color = lightColorGradient.Evaluate(_currentTime);
            directionalLight.intensity = lightIntensityCurve.Evaluate(_currentTime);
        }

        void UpdateSkybox()
        {
            if (daySkybox == null || nightSkybox == null) return;

            // Lerp between day/night skyboxes based on time
            // Night: 0.0-0.25 and 0.75-1.0
            // Day: 0.25-0.75
            float dayAmount = 0f;
            if (_currentTime < 0.25f)
                dayAmount = _currentTime / 0.25f; // Fade in from night
            else if (_currentTime < 0.75f)
                dayAmount = 1f; // Full day
            else
                dayAmount = 1f - ((_currentTime - 0.75f) / 0.25f); // Fade out to night

            RenderSettings.skybox.Lerp(nightSkybox, daySkybox, dayAmount);
            DynamicGI.UpdateEnvironment();
        }

        void UpdateAetherModifier()
        {
            // Night time: 0.0-0.25 and 0.75-1.0
            bool isNight = _currentTime < 0.25f || _currentTime > 0.75f;

            // TODO: Wire this to ExcavationSystem.AetherYieldMultiplier
            // For now, just log when it changes
            float currentBonus = isNight ? nightAetherBonus : 1f;

            // Store in static field for ExcavationSystem to read
            AetherYieldMultiplier = currentBonus;
        }

        public static float AetherYieldMultiplier { get; private set; } = 1f;

        public float GetCurrentTime() => _currentTime;
        public bool IsNight() => _currentTime < 0.25f || _currentTime > 0.75f;
        public bool IsDay() => !IsNight();

        void OnValidate()
        {
            if (cycleDuration < 60f) cycleDuration = 60f; // Min 1 minute per cycle
        }
    }
}
