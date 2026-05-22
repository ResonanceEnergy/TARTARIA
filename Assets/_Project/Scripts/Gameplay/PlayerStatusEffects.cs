using UnityEngine;
using System;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player status effect system for harmonic interference.
    /// Moon 2 Resonance Disruptors apply FrequencyScramble effect — locks player's tuning
    /// input to random frequencies for 1.8 seconds, preventing accurate restoration during combat.
    /// </summary>
    public class PlayerStatusEffects : MonoBehaviour
    {
        public static PlayerStatusEffects Instance { get; private set; }

        public event Action<StatusEffectType, float> OnStatusEffectApplied;
        public event Action<StatusEffectType> OnStatusEffectRemoved;

        // Active effect timers
        float _scrambleRemaining;
        float _stunRemaining;
        float _slowRemaining;

        // Scramble state
        bool _isScrambled;
        float _scrambledFrequency;

        // Public query API
        public bool IsScrambled => _isScrambled;
        public float ScrambledFrequency => _scrambledFrequency;
        public bool IsStunned => _stunRemaining > 0f;
        public bool IsSlowed => _slowRemaining > 0f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            // Tick down scramble
            if (_scrambleRemaining > 0f)
            {
                _scrambleRemaining -= dt;
                if (_scrambleRemaining <= 0f)
                {
                    _isScrambled = false;
                    _scrambledFrequency = 0f;
                    OnStatusEffectRemoved?.Invoke(StatusEffectType.FrequencyScramble);
                    Debug.Log("[StatusEffects] Frequency Scramble ended");
                }
            }

            // Tick down stun
            if (_stunRemaining > 0f)
            {
                _stunRemaining -= dt;
                if (_stunRemaining <= 0f)
                {
                    OnStatusEffectRemoved?.Invoke(StatusEffectType.Stun);
                    Debug.Log("[StatusEffects] Stun ended");
                }
            }

            // Tick down slow
            if (_slowRemaining > 0f)
            {
                _slowRemaining -= dt;
                if (_slowRemaining <= 0f)
                {
                    OnStatusEffectRemoved?.Invoke(StatusEffectType.Slow);
                    Debug.Log("[StatusEffects] Slow ended");
                }
            }
        }

        /// <summary>
        /// Apply frequency scramble effect (Moon 2 Resonance Disruptors).
        /// Locks tuning input to random freq for duration.
        /// </summary>
        public void ApplyFrequencyScramble(float duration = 1.8f)
        {
            _scrambleRemaining = duration;
            _isScrambled = true;
            // Generate random freq in audible range (Hz)
            _scrambledFrequency = UnityEngine.Random.Range(200f, 800f);
            
            OnStatusEffectApplied?.Invoke(StatusEffectType.FrequencyScramble, duration);
            
            // VFX + audio feedback
            Input.HapticFeedbackManager.Instance?.PlayDissonanceCorruptionHit();
            Audio.AudioManager.Instance?.PlaySFX2D("StatusEffect_Scramble");
            
            Debug.Log($"[StatusEffects] Frequency Scramble applied: {duration:F1}s, locked to {_scrambledFrequency:F0} Hz");
        }

        /// <summary>
        /// Apply stun effect (prevents all input).
        /// </summary>
        public void ApplyStun(float duration)
        {
            _stunRemaining = duration;
            OnStatusEffectApplied?.Invoke(StatusEffectType.Stun, duration);
            Input.HapticFeedbackManager.Instance?.PlayDissonanceCorruptionHit();
            Debug.Log($"[StatusEffects] Stun applied: {duration:F1}s");
        }

        /// <summary>
        /// Apply slow effect (reduces movement speed by 50%).
        /// </summary>
        public void ApplySlow(float duration)
        {
            _slowRemaining = duration;
            OnStatusEffectApplied?.Invoke(StatusEffectType.Slow, duration);
            Debug.Log($"[StatusEffects] Slow applied: {duration:F1}s");
        }

        /// <summary>
        /// Clear all status effects (used by save/load and boss phase transitions).
        /// </summary>
        public void ClearAll()
        {
            bool hadAny = _isScrambled || _stunRemaining > 0f || _slowRemaining > 0f;
            
            _scrambleRemaining = 0f;
            _isScrambled = false;
            _scrambledFrequency = 0f;
            
            _stunRemaining = 0f;
            _slowRemaining = 0f;

            if (hadAny)
            {
                Debug.Log("[StatusEffects] All effects cleared");
            }
        }
    }

    public enum StatusEffectType
    {
        FrequencyScramble,
        Stun,
        Slow
    }
}
