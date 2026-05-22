using UnityEngine;

namespace Tartaria.Core
{
    /// <summary>
    /// Screen Shake — adds camera shake for impact moments (boss hits, explosions, earthquakes).
    /// Attach to main camera or call ScreenShake.Shake() from anywhere.
    /// Uses golden ratio decay for natural feel.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        public static ScreenShake Instance { get; private set; }

        [Header("Shake Settings")]
        [SerializeField, Range(0f, 2f), Tooltip("Maximum shake intensity")]
        float maxIntensity = 0.5f;
        [SerializeField, Range(0f, 5f), Tooltip("Shake duration multiplier")]
        float durationScale = 1f;
        [SerializeField, Tooltip("Use golden ratio decay (φ-based natural falloff)")]
        bool useGoldenRatioDecay = true;

        Vector3 _originalPosition;
        float _trauma;
        float _traumaDecayRate = 2f;
        Camera _camera;

        const float PHI = 1.618033988749f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                Debug.LogWarning("[ScreenShake] No Camera component found, attaching to main camera.");
                _camera = Camera.main;
                if (_camera != null)
                {
                    transform.SetParent(_camera.transform);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
        }

        void Start()
        {
            if (_camera != null)
                _originalPosition = _camera.transform.localPosition;
        }

        void LateUpdate()
        {
            if (_trauma > 0f)
            {
                _trauma -= _traumaDecayRate * Time.deltaTime;
                _trauma = Mathf.Max(0f, _trauma);

                if (_camera != null)
                    ApplyShake();
            }
            else if (_camera != null && _camera.transform.localPosition != _originalPosition)
            {
                // Reset to original position when shake complete
                _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, _originalPosition, Time.deltaTime * 8f);
            }
        }

        void ApplyShake()
        {
            // Shake intensity uses trauma^2 for smoother falloff
            float shake = _trauma * _trauma;
            float intensity = shake * maxIntensity;

            // Golden ratio-based noise for natural feel
            float offsetX = (Mathf.PerlinNoise(Time.time * 15f, 0f) - 0.5f) * 2f * intensity;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 17f) - 0.5f) * 2f * intensity;
            float offsetZ = (Mathf.PerlinNoise(Time.time * 13f, Time.time * 11f) - 0.5f) * 0.5f * intensity;

            if (useGoldenRatioDecay)
            {
                // Apply φ-based dampening for aesthetically pleasing decay
                float phiFactor = 1f / (1f + _trauma * PHI);
                offsetX *= phiFactor;
                offsetY *= phiFactor;
                offsetZ *= phiFactor;
            }

            _camera.transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, offsetZ);
        }

        /// <summary>
        /// Trigger screen shake with specified intensity and duration.
        /// </summary>
        /// <param name="intensity">Shake strength (0-1)</param>
        /// <param name="duration">Shake duration in seconds</param>
        public static void Shake(float intensity = 0.5f, float duration = 0.5f)
        {
            if (Instance == null) return;

            float scaledIntensity = Mathf.Clamp01(intensity);
            float scaledDuration = duration * Instance.durationScale;

            // Trauma is additive, clamped to 1
            Instance._trauma = Mathf.Min(1f, Instance._trauma + scaledIntensity);
            Instance._traumaDecayRate = 1f / scaledDuration;
        }

        /// <summary>
        /// Preset: Light shake (player hit, small explosion)
        /// </summary>
        public static void LightShake() => Shake(0.25f, 0.3f);

        /// <summary>
        /// Preset: Medium shake (golem stomp, building collapse)
        /// </summary>
        public static void MediumShake() => Shake(0.5f, 0.6f);

        /// <summary>
        /// Preset: Heavy shake (boss impact, earthquake)
        /// </summary>
        public static void HeavyShake() => Shake(0.85f, 1.2f);

        /// <summary>
        /// Preset: Massive shake (leviathan roar, continental event)
        /// </summary>
        public static void MassiveShake() => Shake(1f, 2f);

        /// <summary>
        /// Stop all shake immediately
        /// </summary>
        public static void Stop()
        {
            if (Instance == null) return;
            Instance._trauma = 0f;
        }
    }
}
