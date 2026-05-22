using UnityEngine;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Lullaby Buff Component — passive healing aura granted from Moon 3 Orphan Train Lullaby Crystal.
    /// Provides continuous 432 Hz harmonic healing to player.
    /// 
    /// Attach to Player GameObject after Moon 3 completion.
    /// </summary>
    [DisallowMultipleComponent]
    public class LullabyBuffComponent : MonoBehaviour
    {
        [Header("Healing Config")]
        [SerializeField, Tooltip("HP healed per second")]
        float healPerSecond = 1f;

        [SerializeField, Tooltip("Healing pulse frequency (Hz) — 432 Hz is Lullaby Crystal signature")]
        float healFrequency = 432f;

        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Optional particle system for visual aura")]
        ParticleSystem auraParticles;

        PlayerHealth _playerHealth;
        float _healTimer;
        float _healInterval;
        bool _initialized;

        void Awake()
        {
            _playerHealth = GetComponent<PlayerHealth>();
            if (_playerHealth == null)
            {
                Debug.LogError("[LullabyBuff] PlayerHealth component not found! Buff will not function.");
                enabled = false;
                return;
            }

            // Convert frequency to interval (seconds between heal ticks)
            // At 432 Hz, we pulse 432 times per second, but we only heal once per second
            // So we use 1 second interval, with the frequency as thematic metadata
            _healInterval = 1f / Mathf.Max(1f, healPerSecond);
            _initialized = true;

            Debug.Log($"[LullabyBuff] Activated — {healPerSecond} HP/sec at {healFrequency} Hz resonance");
        }

        void OnEnable()
        {
            if (!_initialized) return;

            // Start aura particles if assigned
            if (auraParticles != null && !auraParticles.isPlaying)
            {
                auraParticles.Play();
            }
        }

        void OnDisable()
        {
            // Stop aura particles
            if (auraParticles != null && auraParticles.isPlaying)
            {
                auraParticles.Stop();
            }
        }

        void Update()
        {
            if (!_initialized || _playerHealth == null || _playerHealth.IsDead)
                return;

            // Skip healing if already at max
            if (_playerHealth.CurrentHealth >= _playerHealth.MaxHealth)
                return;

            _healTimer += Time.deltaTime;

            if (_healTimer >= _healInterval)
            {
                // Heal 1 HP per tick
                _playerHealth.Heal(1);
                _healTimer = 0f;

                // Optional: play subtle heal sound at 432 Hz
                // Audio.AudioManager.Instance?.PlayTone(healFrequency, 0.05f, 0.1f);
            }
        }

        /// <summary>
        /// Set custom heal rate (HP/sec). Call after adding component.
        /// </summary>
        public void SetHealRate(float hpPerSecond)
        {
            healPerSecond = Mathf.Max(0.1f, hpPerSecond);
            _healInterval = 1f / healPerSecond;
            Debug.Log($"[LullabyBuff] Heal rate updated to {healPerSecond} HP/sec");
        }

        /// <summary>
        /// Set custom healing frequency (Hz). Thematic only, doesn't affect heal rate.
        /// </summary>
        public void SetFrequency(float frequencyHz)
        {
            healFrequency = Mathf.Max(1f, frequencyHz);
            Debug.Log($"[LullabyBuff] Frequency tuned to {healFrequency} Hz");
        }

        void OnDrawGizmosSelected()
        {
            // Draw healing aura sphere (visual indicator in Scene view)
            Gizmos.color = new Color(1f, 0.9f, 0.5f, 0.3f); // Golden glow
            Gizmos.DrawSphere(transform.position, 2f);
        }
    }
}
