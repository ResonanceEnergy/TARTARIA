using UnityEngine;
using System.Collections.Generic;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// PlayerAbilityManager — manages player abilities, cooldowns, resource costs.
    /// Tracks 4 ability slots (Q/E/R/Ultimate), handles casting, cooldown tick, RS cost.
    /// Integrates with Input System for ability input + HUD for cooldown display.
    /// 
    /// Ability Slots:
    /// - Ability1 (Q): Resonance Pulse (AOE, 10s CD, 20 RS)
    /// - Ability2 (E): Harmonic Shield (buff, 15s CD, 30 RS)
    /// - Ability3 (R): Frequency Shift (mobility, 8s CD, 15 RS)
    /// - Ultimate (T): Reality Anchor (transform, 60s CD, 100 RS)
    /// 
    /// Features:
    /// - Cooldown tracking (per-ability timer)
    /// - RS cost checking + consumption
    /// - Ability unlocks (via level/quest)
    /// - Cooldown reduction modifiers
    /// 
    /// Usage:
    /// - Define abilities in inspector or via RegisterAbility()
    /// - Call CastAbility(slot) from input handler
    /// - Subscribe to OnAbilityCast for VFX/SFX
    /// 
    /// GDD refs: §09 (Combat Abilities), §02 (Resonance System)
    /// </summary>
    public class PlayerAbilityManager : MonoBehaviour
    {
        public static PlayerAbilityManager Instance { get; private set; }

        [Header("Ability Definitions")]
        [SerializeField] AbilityData[] abilities;

        [Header("Cooldown Reduction")]
        [SerializeField] float cooldownReductionMultiplier = 1f;  // 0.8 = 20% CDR

        public event System.Action<int> OnAbilityCast;  // Ability slot index
        public event System.Action<int, float> OnCooldownUpdated;  // Slot, remaining time

        Dictionary<int, float> _cooldownTimers = new();  // Slot → remaining cooldown
        Dictionary<int, bool> _unlockedAbilities = new();  // Slot → unlocked

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Initialize abilities
            if (abilities != null)
            {
                for (int i = 0; i < abilities.Length; i++)
                {
                    _cooldownTimers[i] = 0f;
                    _unlockedAbilities[i] = abilities[i].unlockedByDefault;
                }
            }
        }

        void Update()
        {
            // Tick cooldowns
            foreach (var slot in new List<int>(_cooldownTimers.Keys))
            {
                if (_cooldownTimers[slot] > 0f)
                {
                    _cooldownTimers[slot] -= Time.deltaTime;

                    if (_cooldownTimers[slot] <= 0f)
                    {
                        _cooldownTimers[slot] = 0f;
                        Debug.Log($"[PlayerAbility] Ability {slot} ready");
                    }

                    OnCooldownUpdated?.Invoke(slot, _cooldownTimers[slot]);
                }
            }
        }

        /// <summary>
        /// Attempt to cast ability in slot.
        /// </summary>
        public bool CastAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length)
            {
                Debug.LogWarning($"[PlayerAbility] Invalid slot index: {slotIndex}");
                return false;
            }

            var ability = abilities[slotIndex];

            // Check if unlocked
            if (!_unlockedAbilities.GetValueOrDefault(slotIndex, false))
            {
                Debug.LogWarning($"[PlayerAbility] Ability '{ability.abilityName}' is locked");
                return false;
            }

            // Check cooldown
            if (_cooldownTimers[slotIndex] > 0f)
            {
                Debug.LogWarning($"[PlayerAbility] Ability '{ability.abilityName}' on cooldown ({_cooldownTimers[slotIndex]:F1}s)");
                return false;
            }

            // Check RS cost
            // TODO: Get current RS from ResonanceScoreTracker
            // if (currentRS < ability.rsCost) return false;

            // Cast ability
            ExecuteAbility(slotIndex, ability);

            // Start cooldown
            float adjustedCooldown = ability.cooldown * cooldownReductionMultiplier;
            _cooldownTimers[slotIndex] = adjustedCooldown;

            // Consume RS
            // TODO: ResonanceScoreTracker.Instance?.ConsumeRS(ability.rsCost);

            OnAbilityCast?.Invoke(slotIndex);

            Debug.Log($"[PlayerAbility] Cast '{ability.abilityName}' (CD: {adjustedCooldown:F1}s, Cost: {ability.rsCost} RS)");

            return true;
        }

        void ExecuteAbility(int slotIndex, AbilityData ability)
        {
            Debug.Log($"[PlayerAbility] Executing '{ability.abilityName}' effect");

            // Trigger ability-specific logic based on type
            switch (ability.abilityType)
            {
                case AbilityType.Damage:
                    // TODO: Apply damage in AOE
                    break;

                case AbilityType.Buff:
                    // TODO: Apply buff to player
                    break;

                case AbilityType.Mobility:
                    // TODO: Dash/teleport player
                    break;

                case AbilityType.Utility:
                    // TODO: Ability-specific utility
                    break;
            }

            // Play SFX
            if (!string.IsNullOrEmpty(ability.castSFX))
            {
                Audio.AudioManager.Instance?.PlaySFX(ability.castSFX, transform.position);
            }

            // Trigger VFX
            // TODO: ParticleEffectPool.Instance?.PlayEffect(ability.castVFX, transform.position);

            // Haptic feedback
            Input.HapticFeedbackManager.Instance?.OnAbilityCast();
        }

        /// <summary>
        /// Unlock ability by slot index.
        /// </summary>
        public void UnlockAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length) return;

            _unlockedAbilities[slotIndex] = true;

            Debug.Log($"[PlayerAbility] Unlocked '{abilities[slotIndex].abilityName}'");

            // TODO: Show UI notification
        }

        /// <summary>
        /// Check if ability is on cooldown.
        /// </summary>
        public bool IsOnCooldown(int slotIndex)
        {
            return _cooldownTimers.GetValueOrDefault(slotIndex, 0f) > 0f;
        }

        /// <summary>
        /// Get remaining cooldown time.
        /// </summary>
        public float GetCooldownRemaining(int slotIndex)
        {
            return _cooldownTimers.GetValueOrDefault(slotIndex, 0f);
        }

        /// <summary>
        /// Get cooldown progress (0-1, 1 = ready).
        /// </summary>
        public float GetCooldownProgress(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length) return 0f;

            float max = abilities[slotIndex].cooldown;
            float remaining = _cooldownTimers.GetValueOrDefault(slotIndex, 0f);

            return 1f - Mathf.Clamp01(remaining / max);
        }

        /// <summary>
        /// Reset all cooldowns (cheat/debug).
        /// </summary>
        public void ResetAllCooldowns()
        {
            foreach (var slot in new List<int>(_cooldownTimers.Keys))
            {
                _cooldownTimers[slot] = 0f;
            }

            Debug.Log("[PlayerAbility] All cooldowns reset");
        }

        [System.Serializable]
        public class AbilityData
        {
            public string abilityName = "Ability";
            public AbilityType abilityType = AbilityType.Damage;
            public float cooldown = 10f;
            public int rsCost = 20;
            public bool unlockedByDefault = true;
            public string castSFX = "";
            public string castVFX = "";
        }

        public enum AbilityType : byte
        {
            Damage = 0,
            Buff = 1,
            Mobility = 2,
            Utility = 3
        }
    }
}
