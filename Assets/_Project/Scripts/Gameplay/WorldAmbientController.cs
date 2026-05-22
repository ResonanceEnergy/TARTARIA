using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// WorldAmbientController — manages global lighting, fog, ambient audio, post-processing.
    /// Responds to DayNightCycle for time-of-day transitions.
    /// Handles Moon-specific ambience overrides (e.g. Moon 2 dissonance fog, Moon 4 forge glow).
    /// 
    /// Features:
    /// - Fog color/density curves
    /// - Directional light color/intensity curves
    /// - Skybox rotation
    /// - URP Post-Processing Volume blending
    /// - Ambient audio zones
    /// 
    /// Usage:
    /// - Attach to scene root GameObject
    /// - Link DayNightCycle reference
    /// - Define day/night curves for smooth transitions
    /// - Call SetMoonAmbience(moonIndex) to override with Moon-specific settings
    /// 
    /// GDD refs: §02 (Atmosphere), §05 (Immersion), §03 (Moon Mechanics)
    /// </summary>
    public class WorldAmbientController : MonoBehaviour
    {
        public static WorldAmbientController Instance { get; private set; }

        [Header("References")]
        [SerializeField] Light directionalLight;
        [SerializeField] Volume globalVolume;
        [SerializeField] DayNightCycle dayNightCycle;

        [Header("Fog Settings")]
        [SerializeField] bool fogEnabled = true;
        [SerializeField] Gradient fogColorGradient;  // Maps time (0-1) to color
        [SerializeField] AnimationCurve fogDensityCurve;  // Maps time (0-1) to density

        [Header("Lighting Settings")]
        [SerializeField] Gradient sunColorGradient;
        [SerializeField] AnimationCurve sunIntensityCurve;
        [SerializeField] float skyboxRotationSpeed = 1f;

        [Header("Ambient Audio")]
        [SerializeField] AudioClip dayAmbience;
        [SerializeField] AudioClip nightAmbience;
        [SerializeField] float ambienceVolume = 0.3f;
        [SerializeField] float ambienceFadeDuration = 5f;

        AudioSource _ambienceSource;
        int _currentMoonOverride = -1;
        MoonAmbiencePreset _activePreset;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Find directional light if not assigned
            if (directionalLight == null)
            {
                directionalLight = RenderSettings.sun;
            }

            // Setup ambient audio
            _ambienceSource = gameObject.AddComponent<AudioSource>();
            _ambienceSource.loop = true;
            _ambienceSource.playOnAwake = false;
            _ambienceSource.volume = ambienceVolume;
            _ambienceSource.spatialBlend = 0f;  // 2D audio

            Debug.Log("[WorldAmbient] Initialized");
        }

        void Start()
        {
            if (dayNightCycle == null)
            {
                dayNightCycle = DayNightCycle.Instance;
            }

            if (dayNightCycle == null)
            {
                Debug.LogWarning("[WorldAmbient] DayNightCycle not found, ambient updates disabled");
                enabled = false;
                return;
            }

            // Start day ambience
            if (dayAmbience != null)
            {
                _ambienceSource.clip = dayAmbience;
                _ambienceSource.Play();
            }

            // Initialize fog
            RenderSettings.fog = fogEnabled;
        }

        void Update()
        {
            if (dayNightCycle == null) return;

            // Get normalized time (0 = midnight, 0.5 = noon, 1 = midnight)
            float normalizedTime = dayNightCycle.GetNormalizedTimeOfDay();

            UpdateFog(normalizedTime);
            UpdateLighting(normalizedTime);
            UpdateSkybox(normalizedTime);
            UpdateAmbientAudio(normalizedTime);
        }

        void UpdateFog(float time)
        {
            if (!fogEnabled) return;

            // Apply fog color + density from curves
            if (fogColorGradient != null)
            {
                RenderSettings.fogColor = fogColorGradient.Evaluate(time);
            }

            if (fogDensityCurve != null)
            {
                RenderSettings.fogDensity = fogDensityCurve.Evaluate(time);
            }
        }

        void UpdateLighting(float time)
        {
            if (directionalLight == null) return;

            // Apply sun color + intensity from curves
            if (sunColorGradient != null)
            {
                directionalLight.color = sunColorGradient.Evaluate(time);
            }

            if (sunIntensityCurve != null)
            {
                directionalLight.intensity = sunIntensityCurve.Evaluate(time);
            }

            // Rotate sun (0 = sunrise, 0.5 = sunset, 1 = sunrise)
            float rotation = time * 360f - 90f;  // -90 offset for noon at top
            directionalLight.transform.rotation = Quaternion.Euler(rotation, 170f, 0f);
        }

        void UpdateSkybox(float time)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetFloat("_Rotation", time * 360f * skyboxRotationSpeed);
            }
        }

        void UpdateAmbientAudio(float time)
        {
            // Crossfade day/night ambience
            bool isNight = time < 0.25f || time > 0.75f;

            AudioClip targetClip = isNight ? nightAmbience : dayAmbience;

            if (_ambienceSource.clip != targetClip && targetClip != null)
            {
                StartCoroutine(CrossfadeAmbience(targetClip));
            }
        }

        System.Collections.IEnumerator CrossfadeAmbience(AudioClip newClip)
        {
            float elapsed = 0f;
            float startVolume = _ambienceSource.volume;

            // Fade out
            while (elapsed < ambienceFadeDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                _ambienceSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (ambienceFadeDuration * 0.5f));
                yield return null;
            }

            // Switch clip
            _ambienceSource.clip = newClip;
            _ambienceSource.Play();

            // Fade in
            elapsed = 0f;
            while (elapsed < ambienceFadeDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                _ambienceSource.volume = Mathf.Lerp(0f, ambienceVolume, elapsed / (ambienceFadeDuration * 0.5f));
                yield return null;
            }

            _ambienceSource.volume = ambienceVolume;
        }

        /// <summary>
        /// Set Moon-specific ambient override (fog, lighting, post-processing).
        /// </summary>
        public void SetMoonAmbience(int moonIndex)
        {
            if (_currentMoonOverride == moonIndex) return;

            _currentMoonOverride = moonIndex;

            _activePreset = GetMoonPreset(moonIndex);

            if (_activePreset != null)
            {
                ApplyMoonPreset(_activePreset);
                Debug.Log($"[WorldAmbient] Applied Moon {moonIndex} ambience preset");
            }
            else
            {
                Debug.LogWarning($"[WorldAmbient] No preset defined for Moon {moonIndex}");
            }
        }

        /// <summary>
        /// Clear Moon override, return to normal day/night cycle.
        /// </summary>
        public void ClearMoonAmbience()
        {
            _currentMoonOverride = -1;
            _activePreset = null;

            Debug.Log("[WorldAmbient] Cleared Moon ambience override");
        }

        void ApplyMoonPreset(MoonAmbiencePreset preset)
        {
            // Override fog
            if (preset.fogColor != Color.clear)
            {
                RenderSettings.fogColor = preset.fogColor;
            }

            if (preset.fogDensity > 0f)
            {
                RenderSettings.fogDensity = preset.fogDensity;
            }

            // Override lighting
            if (directionalLight != null && preset.sunColor != Color.clear)
            {
                directionalLight.color = preset.sunColor;
            }

            if (directionalLight != null && preset.sunIntensity > 0f)
            {
                directionalLight.intensity = preset.sunIntensity;
            }

            // TODO: Apply URP Volume Profile override
            // if (globalVolume != null && preset.volumeProfile != null)
            // {
            //     globalVolume.profile = preset.volumeProfile;
            // }
        }

        MoonAmbiencePreset GetMoonPreset(int moonIndex)
        {
            // Hardcoded presets for now, move to ScriptableObject later
            return moonIndex switch
            {
                2 => new MoonAmbiencePreset
                {
                    fogColor = new Color(0.4f, 0.1f, 0.6f, 1f),  // Purple dissonance
                    fogDensity = 0.05f,
                    sunColor = new Color(0.6f, 0.3f, 0.8f, 1f),
                    sunIntensity = 0.6f
                },
                4 => new MoonAmbiencePreset
                {
                    fogColor = new Color(1f, 0.5f, 0.2f, 1f),  // Orange forge glow
                    fogDensity = 0.03f,
                    sunColor = new Color(1f, 0.7f, 0.4f, 1f),
                    sunIntensity = 1.2f
                },
                10 => new MoonAmbiencePreset
                {
                    fogColor = new Color(0.3f, 0.3f, 0.4f, 1f),  // Industrial haze
                    fogDensity = 0.04f,
                    sunColor = new Color(0.8f, 0.8f, 0.9f, 1f),
                    sunIntensity = 0.8f
                },
                13 => new MoonAmbiencePreset
                {
                    fogColor = new Color(0.8f, 0.9f, 1f, 1f),  // Ethereal cyan
                    fogDensity = 0.02f,
                    sunColor = new Color(1f, 1f, 1f, 1f),
                    sunIntensity = 1.5f
                },
                _ => null
            };
        }

        class MoonAmbiencePreset
        {
            public Color fogColor = Color.clear;
            public float fogDensity = 0.03f;
            public Color sunColor = Color.clear;
            public float sunIntensity = 1f;
            // public VolumeProfile volumeProfile;  // URP post-processing override
        }
    }
}
