using System.Collections;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Manages visual feedback for Moon 2 lunar alignment mechanics.
    /// Displays phase transitions, lunar resonance effects, and moon-phase-dependent environmental visuals.
    /// Phase cycle: 0=New Moon, 1-3=Waxing, 4=Full Moon, 5-7=Waning.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon2LunarVisualsManager : MonoBehaviour
    {
        public static Moon2LunarVisualsManager Instance { get; private set; }

        [Header("Lunar Phase Settings")]
        [SerializeField] float phaseTransitionDuration = 2f;
        [SerializeField] GameObject lunarGlowPrefab;
        [SerializeField] Light moonLight;

        int _currentPhase = 0; // 0-7
        bool _isTransitioning = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Sets the visual representation of the current lunar phase (0-7).</summary>
        public void SetPhaseVisual(int phase)
        {
            phase = Mathf.Clamp(phase, 0, 7);
            _currentPhase = phase;

            // Update moonlight intensity based on phase (Full Moon = brightest)
            if (moonLight != null)
            {
                float intensity = phase == 4 ? 1.0f : Mathf.Lerp(0.2f, 0.8f, 1f - Mathf.Abs(phase - 4) / 4f);
                moonLight.intensity = intensity;
            }

            Debug.Log($"[Moon2LunarVisualsManager] Lunar phase set to {phase} | Intensity: {moonLight?.intensity:F2}");
        }

        /// <summary>Plays the transition animation between lunar phases.</summary>
        public void PlayPhaseTransition()
        {
            if (_isTransitioning) return;
            StartCoroutine(PhaseTransitionCoroutine());
        }

        IEnumerator PhaseTransitionCoroutine()
        {
            _isTransitioning = true;
            Debug.Log("[Moon2LunarVisualsManager] Phase transition started");

            // Spawn lunar glow particle effect if available
            if (lunarGlowPrefab != null && VFXPoolManager.Instance != null)
            {
                var vfx = VFXPoolManager.Instance.SpawnParticle(lunarGlowPrefab, transform.position, Quaternion.identity, phaseTransitionDuration);
                if (vfx != null) vfx.Play();
            }

            // Simple fade transition (could integrate with post-processing bloom later)
            float elapsed = 0f;
            while (elapsed < phaseTransitionDuration)
            {
                elapsed += Time.deltaTime;
                // Visual effects hook (e.g., screen flash, skybox color shift)
                yield return null;
            }

            Debug.Log("[Moon2LunarVisualsManager] Phase transition complete");
            _isTransitioning = false;
        }

        /// <summary>Plays lunar shadow purge transformation VFX (golden burn effect).</summary>
        public void PlayLunarShadowPurgeCathedralTransformation(Vector3 position, float duration)
        {
            Debug.Log($"[Moon2LunarVisualsManager] Cathedral transformation at {position} | Duration: {duration}s");

            // Spawn golden purge VFX via VFXPoolManager if available
            if (VFXPoolManager.Instance != null && lunarGlowPrefab != null)
            {
                var vfx = VFXPoolManager.Instance.SpawnParticle(lunarGlowPrefab, position, Quaternion.identity, duration);
                if (vfx != null)
                {
                    var main = vfx.main;
                    main.startColor = new Color(1f, 0.84f, 0f); // Golden color
                    vfx.Play();
                }
            }
            else
            {
                Debug.LogWarning("[Moon2LunarVisualsManager] VFXPoolManager or lunarGlowPrefab not available — VFX not spawned");
            }
        }
    }
}
