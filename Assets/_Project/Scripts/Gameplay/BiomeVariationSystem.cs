using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Defines and applies biome-specific environmental variations.
    /// Modifies ambient color, fog density, particle effects, and audio profiles per biome.
    /// </summary>
    public class BiomeVariationSystem : MonoBehaviour
    {
        public enum BiomeType
        {
            Temperate,      // Default Echohaven
            Desert,         // Sandy, hot, dust storms
            Tundra,         // Snow, ice, wind
            Volcanic,       // Lava, ash, heat haze
            Crystalline,    // Glowing crystals, resonance hum
            Subterranean,   // Cave, bioluminescence, echo
            Aquatic,        // Underwater, kelp, currents
            Sky,            // Floating islands, wind, clouds
            Corrupted       // Reset influence, glitch effects
        }

        [System.Serializable]
        public class BiomeProfile
        {
            public BiomeType type;
            public Color ambientColor = Color.white;
            public Color fogColor = Color.gray;
            public float fogDensity = 0.01f;
            public float ambientIntensity = 1f;
            public GameObject[] particleEffectPrefabs;
            public AudioClip ambientLoop;
            public float windStrength = 0.5f;
            public float temperatureModifier = 0f; // -1 cold, +1 hot
        }

        [Header("Biome Profiles")]
        [SerializeField] private BiomeProfile[] biomeProfiles = new BiomeProfile[9];

        [Header("Current State")]
        [SerializeField] private BiomeType currentBiome = BiomeType.Temperate;
        [SerializeField] private float transitionDuration = 3f;

        private BiomeProfile activeBiomeProfile;
        private GameObject[] spawnedParticles;
        private float transitionProgress = 1f;
        private BiomeProfile previousBiome;

        void Awake()
        {
            InitializeDefaultProfiles();
            activeBiomeProfile = GetBiomeProfile(currentBiome);
        }

        void Start()
        {
            ApplyBiomeImmediate(currentBiome);
        }

        void Update()
        {
            if (transitionProgress < 1f)
            {
                transitionProgress += Time.deltaTime / transitionDuration;
                transitionProgress = Mathf.Clamp01(transitionProgress);
                LerpBiomeEffects(previousBiome, activeBiomeProfile, transitionProgress);
            }
        }

        /// <summary>
        /// Transition to a new biome over transitionDuration seconds.
        /// </summary>
        public void TransitionToBiome(BiomeType newBiome)
        {
            if (newBiome == currentBiome) return;

            Debug.Log($"[BiomeVariation] Transitioning from {currentBiome} to {newBiome}");

            previousBiome = activeBiomeProfile;
            currentBiome = newBiome;
            activeBiomeProfile = GetBiomeProfile(newBiome);
            transitionProgress = 0f;
        }

        /// <summary>
        /// Instantly apply a biome without transition.
        /// </summary>
        public void ApplyBiomeImmediate(BiomeType newBiome)
        {
            Debug.Log($"[BiomeVariation] Applying biome: {newBiome}");

            currentBiome = newBiome;
            activeBiomeProfile = GetBiomeProfile(newBiome);
            transitionProgress = 1f;

            ApplyAmbientLighting();
            ApplyFogSettings();
            SpawnParticleEffects();
            ApplyAmbientAudio();
        }

        private void LerpBiomeEffects(BiomeProfile from, BiomeProfile to, float t)
        {
            RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, t);
            RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(from.ambientIntensity, to.ambientIntensity, t);

            if (t >= 1f)
            {
                SpawnParticleEffects();
                ApplyAmbientAudio();
            }
        }

        private void ApplyAmbientLighting()
        {
            RenderSettings.ambientLight = activeBiomeProfile.ambientColor;
            RenderSettings.ambientIntensity = activeBiomeProfile.ambientIntensity;
        }

        private void ApplyFogSettings()
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = activeBiomeProfile.fogColor;
            RenderSettings.fogDensity = activeBiomeProfile.fogDensity;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
        }

        private void SpawnParticleEffects()
        {
            // Clear existing particles
            if (spawnedParticles != null)
            {
                foreach (var particle in spawnedParticles)
                {
                    if (particle != null) Destroy(particle);
                }
            }

            // Spawn new biome-specific particles
            if (activeBiomeProfile.particleEffectPrefabs != null && activeBiomeProfile.particleEffectPrefabs.Length > 0)
            {
                spawnedParticles = new GameObject[activeBiomeProfile.particleEffectPrefabs.Length];
                for (int i = 0; i < activeBiomeProfile.particleEffectPrefabs.Length; i++)
                {
                    var prefab = activeBiomeProfile.particleEffectPrefabs[i];
                    if (prefab != null)
                    {
                        spawnedParticles[i] = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                    }
                }
            }
        }

        private void ApplyAmbientAudio()
        {
            if (activeBiomeProfile.ambientLoop != null)
            {
                var audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.loop = true;
                    audioSource.spatialBlend = 0f; // 2D sound
                    audioSource.volume = 0.3f;
                }

                if (audioSource.clip != activeBiomeProfile.ambientLoop)
                {
                    audioSource.clip = activeBiomeProfile.ambientLoop;
                    audioSource.Play();
                }
            }
        }

        private BiomeProfile GetBiomeProfile(BiomeType type)
        {
            foreach (var profile in biomeProfiles)
            {
                if (profile != null && profile.type == type)
                    return profile;
            }

            Debug.LogWarning($"[BiomeVariation] No profile found for {type}, using Temperate default");
            return biomeProfiles[0] ?? new BiomeProfile { type = BiomeType.Temperate };
        }

        private void InitializeDefaultProfiles()
        {
            if (biomeProfiles == null || biomeProfiles.Length == 0)
            {
                biomeProfiles = new BiomeProfile[9];
            }

            // Temperate (Echohaven default)
            if (biomeProfiles[0] == null) biomeProfiles[0] = new BiomeProfile
            {
                type = BiomeType.Temperate,
                ambientColor = new Color(0.9f, 0.95f, 1f),
                fogColor = new Color(0.7f, 0.8f, 0.9f),
                fogDensity = 0.005f,
                ambientIntensity = 1f,
                windStrength = 0.3f,
                temperatureModifier = 0f
            };

            // Desert
            if (biomeProfiles[1] == null) biomeProfiles[1] = new BiomeProfile
            {
                type = BiomeType.Desert,
                ambientColor = new Color(1f, 0.9f, 0.7f),
                fogColor = new Color(0.9f, 0.85f, 0.6f),
                fogDensity = 0.008f,
                ambientIntensity = 1.3f,
                windStrength = 0.7f,
                temperatureModifier = 0.8f
            };

            // Tundra
            if (biomeProfiles[2] == null) biomeProfiles[2] = new BiomeProfile
            {
                type = BiomeType.Tundra,
                ambientColor = new Color(0.8f, 0.9f, 1f),
                fogColor = new Color(0.9f, 0.95f, 1f),
                fogDensity = 0.012f,
                ambientIntensity = 0.9f,
                windStrength = 0.9f,
                temperatureModifier = -0.9f
            };

            // Volcanic
            if (biomeProfiles[3] == null) biomeProfiles[3] = new BiomeProfile
            {
                type = BiomeType.Volcanic,
                ambientColor = new Color(1f, 0.5f, 0.3f),
                fogColor = new Color(0.4f, 0.2f, 0.1f),
                fogDensity = 0.015f,
                ambientIntensity = 0.7f,
                windStrength = 0.5f,
                temperatureModifier = 1f
            };

            // Crystalline
            if (biomeProfiles[4] == null) biomeProfiles[4] = new BiomeProfile
            {
                type = BiomeType.Crystalline,
                ambientColor = new Color(0.7f, 0.9f, 1f),
                fogColor = new Color(0.5f, 0.7f, 0.9f),
                fogDensity = 0.003f,
                ambientIntensity = 1.2f,
                windStrength = 0.2f,
                temperatureModifier = 0.2f
            };

            // Subterranean
            if (biomeProfiles[5] == null) biomeProfiles[5] = new BiomeProfile
            {
                type = BiomeType.Subterranean,
                ambientColor = new Color(0.3f, 0.4f, 0.5f),
                fogColor = new Color(0.1f, 0.15f, 0.2f),
                fogDensity = 0.02f,
                ambientIntensity = 0.4f,
                windStrength = 0.1f,
                temperatureModifier = -0.3f
            };

            // Aquatic
            if (biomeProfiles[6] == null) biomeProfiles[6] = new BiomeProfile
            {
                type = BiomeType.Aquatic,
                ambientColor = new Color(0.4f, 0.7f, 0.9f),
                fogColor = new Color(0.2f, 0.5f, 0.7f),
                fogDensity = 0.025f,
                ambientIntensity = 0.6f,
                windStrength = 0.4f,
                temperatureModifier = -0.2f
            };

            // Sky
            if (biomeProfiles[7] == null) biomeProfiles[7] = new BiomeProfile
            {
                type = BiomeType.Sky,
                ambientColor = new Color(0.9f, 0.95f, 1f),
                fogColor = new Color(0.8f, 0.9f, 1f),
                fogDensity = 0.001f,
                ambientIntensity = 1.4f,
                windStrength = 1f,
                temperatureModifier = -0.4f
            };

            // Corrupted
            if (biomeProfiles[8] == null) biomeProfiles[8] = new BiomeProfile
            {
                type = BiomeType.Corrupted,
                ambientColor = new Color(0.6f, 0.4f, 0.5f),
                fogColor = new Color(0.3f, 0.2f, 0.3f),
                fogDensity = 0.018f,
                ambientIntensity = 0.5f,
                windStrength = 0.6f,
                temperatureModifier = 0f
            };
        }

        public BiomeType GetCurrentBiome() => currentBiome;
        public float GetTemperatureModifier() => activeBiomeProfile?.temperatureModifier ?? 0f;
        public float GetWindStrength() => activeBiomeProfile?.windStrength ?? 0.5f;
    }
}
