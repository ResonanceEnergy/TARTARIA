using UnityEngine;
using System.Collections;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Spawns and manages dynamic weather hazards that affect gameplay.
    /// Hazards can deal damage, slow movement, reduce visibility, etc.
    /// </summary>
    public class WeatherHazardSystem : MonoBehaviour
    {
        public enum HazardType
        {
            None,
            Sandstorm,      // Reduced visibility, wind push, periodic damage
            Blizzard,       // Freezing damage, slow movement, reduced visibility
            AshFallout,     // Fire damage over time, obscured vision
            LightningStorm, // Random lightning strikes, high burst damage
            AcidRain,       // Corrosion damage, equipment durability loss
            SolarFlare,     // Overheats equipment, vision whiteout
            VoidRift,       // Reality glitches, teleport player randomly
            ResonanceQuake  // Ground shake, knockdown, crystal shard rain
        }

        [System.Serializable]
        public class HazardProfile
        {
            public HazardType type;
            public float minDuration = 30f;
            public float maxDuration = 120f;
            public float damagePerSecond = 2f;
            public float movementSpeedMultiplier = 0.7f;
            public float visibilityReduction = 0.5f; // 0-1
            public GameObject hazardVFXPrefab;
            public AudioClip hazardAmbientSound;
            public Color fogColorOverride = Color.gray;
            public float fogDensityMultiplier = 2f;
        }

        [Header("Hazard Profiles")]
        [SerializeField] private HazardProfile[] hazardProfiles = new HazardProfile[8];

        [Header("Spawn Settings")]
        [SerializeField] private bool enableRandomHazards = true;
        [SerializeField] private float minTimeBetweenHazards = 180f;
        [SerializeField] private float maxTimeBetweenHazards = 600f;
        [SerializeField] private float hazardChance = 0.3f; // 30% chance per roll

        [Header("Current State")]
        [SerializeField] private HazardType activeHazard = HazardType.None;
        [SerializeField] private float hazardTimeRemaining = 0f;

        private HazardProfile activeHazardProfile;
        private GameObject spawnedHazardVFX;
        private AudioSource hazardAudioSource;
        private Coroutine hazardSpawnRoutine;
        private Color originalFogColor;
        private float originalFogDensity;

        void Awake()
        {
            InitializeDefaultProfiles();
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;
        }

        void Start()
        {
            if (enableRandomHazards)
            {
                hazardSpawnRoutine = StartCoroutine(RandomHazardSpawnLoop());
            }
        }

        void Update()
        {
            if (activeHazard != HazardType.None && hazardTimeRemaining > 0f)
            {
                hazardTimeRemaining -= Time.deltaTime;

                // Apply per-frame hazard effects
                ApplyHazardEffects();

                if (hazardTimeRemaining <= 0f)
                {
                    EndHazard();
                }
            }
        }

        private IEnumerator RandomHazardSpawnLoop()
        {
            while (true)
            {
                float waitTime = Random.Range(minTimeBetweenHazards, maxTimeBetweenHazards);
                yield return new WaitForSeconds(waitTime);

                if (Random.value < hazardChance && activeHazard == HazardType.None)
                {
                    var randomHazard = (HazardType)Random.Range(1, 9); // Skip None (0)
                    StartHazard(randomHazard);
                }
            }
        }

        /// <summary>
        /// Manually trigger a specific weather hazard.
        /// </summary>
        public void StartHazard(HazardType hazardType)
        {
            if (hazardType == HazardType.None)
            {
                EndHazard();
                return;
            }

            Debug.Log($"[WeatherHazard] Starting hazard: {hazardType}");

            activeHazard = hazardType;
            activeHazardProfile = GetHazardProfile(hazardType);

            float duration = Random.Range(activeHazardProfile.minDuration, activeHazardProfile.maxDuration);
            hazardTimeRemaining = duration;

            SpawnHazardVFX();
            ApplyFogEffects();
            PlayHazardAudio();

            GameEvents.FireWeatherHazardStarted((int)hazardType, duration);
        }

        /// <summary>
        /// Manually end the current hazard.
        /// </summary>
        public void EndHazard()
        {
            if (activeHazard == HazardType.None) return;

            Debug.Log($"[WeatherHazard] Ending hazard: {activeHazard}");

            var endedHazard = activeHazard;
            activeHazard = HazardType.None;
            activeHazardProfile = null;
            hazardTimeRemaining = 0f;

            ClearHazardVFX();
            RestoreFogSettings();
            StopHazardAudio();

            GameEvents.FireWeatherHazardEnded((int)endedHazard);
        }

        private void ApplyHazardEffects()
        {
            if (activeHazardProfile == null) return;

            // Find player and apply damage
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && activeHazardProfile.damagePerSecond > 0f)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    float damage = activeHazardProfile.damagePerSecond * Time.deltaTime;
                    health.TakeDamage((int)Mathf.Ceil(damage));
                }
            }

            // Movement speed reduction is handled by PlayerMovement reading GetMovementSpeedMultiplier()
        }

        private void SpawnHazardVFX()
        {
            ClearHazardVFX();

            if (activeHazardProfile?.hazardVFXPrefab != null)
            {
                spawnedHazardVFX = Instantiate(activeHazardProfile.hazardVFXPrefab, transform.position, Quaternion.identity, transform);
            }
        }

        private void ClearHazardVFX()
        {
            if (spawnedHazardVFX != null)
            {
                Destroy(spawnedHazardVFX);
                spawnedHazardVFX = null;
            }
        }

        private void ApplyFogEffects()
        {
            if (activeHazardProfile == null) return;

            RenderSettings.fogColor = activeHazardProfile.fogColorOverride;
            RenderSettings.fogDensity = originalFogDensity * activeHazardProfile.fogDensityMultiplier;
        }

        private void RestoreFogSettings()
        {
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
        }

        private void PlayHazardAudio()
        {
            if (activeHazardProfile?.hazardAmbientSound != null)
            {
                if (hazardAudioSource == null)
                {
                    hazardAudioSource = gameObject.AddComponent<AudioSource>();
                    hazardAudioSource.loop = true;
                    hazardAudioSource.spatialBlend = 0f;
                    hazardAudioSource.volume = 0.4f;
                }

                hazardAudioSource.clip = activeHazardProfile.hazardAmbientSound;
                hazardAudioSource.Play();
            }
        }

        private void StopHazardAudio()
        {
            if (hazardAudioSource != null && hazardAudioSource.isPlaying)
            {
                hazardAudioSource.Stop();
            }
        }

        private HazardProfile GetHazardProfile(HazardType type)
        {
            foreach (var profile in hazardProfiles)
            {
                if (profile != null && profile.type == type)
                    return profile;
            }

            Debug.LogWarning($"[WeatherHazard] No profile found for {type}, returning default");
            return new HazardProfile { type = type };
        }

        private void InitializeDefaultProfiles()
        {
            if (hazardProfiles == null || hazardProfiles.Length == 0)
            {
                hazardProfiles = new HazardProfile[8];
            }

            // Sandstorm
            if (hazardProfiles[0] == null) hazardProfiles[0] = new HazardProfile
            {
                type = HazardType.Sandstorm,
                minDuration = 45f,
                maxDuration = 90f,
                damagePerSecond = 1f,
                movementSpeedMultiplier = 0.8f,
                visibilityReduction = 0.6f,
                fogColorOverride = new Color(0.9f, 0.8f, 0.6f),
                fogDensityMultiplier = 3f
            };

            // Blizzard
            if (hazardProfiles[1] == null) hazardProfiles[1] = new HazardProfile
            {
                type = HazardType.Blizzard,
                minDuration = 60f,
                maxDuration = 120f,
                damagePerSecond = 1.5f,
                movementSpeedMultiplier = 0.6f,
                visibilityReduction = 0.7f,
                fogColorOverride = new Color(0.95f, 0.95f, 1f),
                fogDensityMultiplier = 4f
            };

            // AshFallout
            if (hazardProfiles[2] == null) hazardProfiles[2] = new HazardProfile
            {
                type = HazardType.AshFallout,
                minDuration = 30f,
                maxDuration = 90f,
                damagePerSecond = 2f,
                movementSpeedMultiplier = 0.9f,
                visibilityReduction = 0.5f,
                fogColorOverride = new Color(0.3f, 0.2f, 0.2f),
                fogDensityMultiplier = 2.5f
            };

            // LightningStorm
            if (hazardProfiles[3] == null) hazardProfiles[3] = new HazardProfile
            {
                type = HazardType.LightningStorm,
                minDuration = 30f,
                maxDuration = 60f,
                damagePerSecond = 0f, // Burst damage, not continuous
                movementSpeedMultiplier = 1f,
                visibilityReduction = 0.3f,
                fogColorOverride = new Color(0.4f, 0.4f, 0.5f),
                fogDensityMultiplier = 2f
            };

            // AcidRain
            if (hazardProfiles[4] == null) hazardProfiles[4] = new HazardProfile
            {
                type = HazardType.AcidRain,
                minDuration = 45f,
                maxDuration = 90f,
                damagePerSecond = 3f,
                movementSpeedMultiplier = 1f,
                visibilityReduction = 0.4f,
                fogColorOverride = new Color(0.6f, 0.7f, 0.5f),
                fogDensityMultiplier = 1.5f
            };

            // SolarFlare
            if (hazardProfiles[5] == null) hazardProfiles[5] = new HazardProfile
            {
                type = HazardType.SolarFlare,
                minDuration = 20f,
                maxDuration = 40f,
                damagePerSecond = 4f,
                movementSpeedMultiplier = 0.85f,
                visibilityReduction = 0.8f,
                fogColorOverride = new Color(1f, 0.95f, 0.8f),
                fogDensityMultiplier = 0.5f
            };

            // VoidRift
            if (hazardProfiles[6] == null) hazardProfiles[6] = new HazardProfile
            {
                type = HazardType.VoidRift,
                minDuration = 30f,
                maxDuration = 60f,
                damagePerSecond = 2.5f,
                movementSpeedMultiplier = 1f,
                visibilityReduction = 0.6f,
                fogColorOverride = new Color(0.2f, 0.1f, 0.3f),
                fogDensityMultiplier = 3f
            };

            // ResonanceQuake
            if (hazardProfiles[7] == null) hazardProfiles[7] = new HazardProfile
            {
                type = HazardType.ResonanceQuake,
                minDuration = 15f,
                maxDuration = 45f,
                damagePerSecond = 3f,
                movementSpeedMultiplier = 0.7f,
                visibilityReduction = 0.2f,
                fogColorOverride = new Color(0.7f, 0.8f, 1f),
                fogDensityMultiplier = 1f
            };
        }

        public HazardType GetActiveHazard() => activeHazard;
        public float GetMovementSpeedMultiplier() => activeHazardProfile?.movementSpeedMultiplier ?? 1f;
        public float GetVisibilityReduction() => activeHazardProfile?.visibilityReduction ?? 0f;
        public float GetTimeRemaining() => hazardTimeRemaining;

        void OnDestroy()
        {
            if (hazardSpawnRoutine != null)
            {
                StopCoroutine(hazardSpawnRoutine);
            }
        }
    }
}
