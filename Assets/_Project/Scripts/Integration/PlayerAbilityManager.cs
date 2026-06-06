using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
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

            // P1.L3: Check RS cost via canonical AetherFieldManager (Core/AetherFieldManager.cs:26).
            // RunProgressTracker is a per-run stat aggregator (not the RS holder); the live RS economy
            // is on AetherFieldManager.Instance which is what HUD, AdaptiveMusic and Combat all read.
            var aether = AetherFieldManager.Instance;
            if (aether == null)
            {
                Debug.LogWarning($"[PlayerAbility] CastAbility '{ability.abilityName}': AetherFieldManager.Instance is null — cannot read RS. Aborting cast.");
                return false;
            }
            if (aether.ResonanceScore < ability.rsCost)
            {
                Debug.Log($"[PlayerAbility] Not enough RS for '{ability.abilityName}' (need {ability.rsCost}, have {aether.ResonanceScore:F1})");
                return false;
            }

            // P1.L3: Spend RS via canonical AetherFieldManager.DeductRS (Core/AetherFieldManager.cs:59)
            // and broadcast through GameEvents.FireRSChange so HUDLiveDataWiring and AdaptiveMusicController
            // see the delta (Core/GameEvents.cs:323, UI/HUDLiveDataWiring.cs:25).
            aether.DeductRS(ability.rsCost);
            GameEvents.FireRSChange(-(float)ability.rsCost);

            // Cast ability
            ExecuteAbility(slotIndex, ability);

            // Start cooldown
            float adjustedCooldown = ability.cooldown * cooldownReductionMultiplier;
            _cooldownTimers[slotIndex] = adjustedCooldown;

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
                    // Apply AOE damage (radius-based collision check)
                    var colliders = Physics.OverlapSphere(transform.position, 10f);
                    foreach (var col in colliders)
                    {
                        var health = col.GetComponent<AI.MudGolemHealth>();
                        if (health != null)
                        {
                            health.TakeDamage(50f, gameObject);  // Default damage value
                        }
                    }
                    break;

                case AbilityType.Buff:
                    // Apply buff to player (stat modifier system pending)
                    Debug.Log($"[PlayerAbility] Buff applied: {ability.abilityName}");
                    break;

                case AbilityType.Mobility:
                    // Dash/teleport player (movement controller integration)
                    var movement = GetComponent<Input.PlayerInputHandler>();
                    if (movement != null)
                    {
                        Vector3 dashDir = transform.forward * 10f;
                        transform.position += dashDir;
                        Debug.Log($"[PlayerAbility] Dashed {dashDir.magnitude}m");
                    }
                    break;

                case AbilityType.Utility:
                    // Utility ability (context-dependent behavior)
                    Debug.Log($"[PlayerAbility] Utility executed: {ability.abilityName}");
                    break;
            }

            // Play SFX
            if (!string.IsNullOrEmpty(ability.castSFX))
            {
                Audio.AudioManager.Instance?.PlaySFX(ability.castSFX, transform.position);
            }

            // Trigger VFX
            // Spawn ability VFX
            Core.ParticleEffectPool.Instance?.Spawn(ability.castVFX, transform.position, Quaternion.identity, 2f);

            // Haptic feedback (generic pulse for ability cast)
            // Input.HapticFeedbackManager.Instance?.PlayPulse(0.4f, 0.2f);
        }

        /// <summary>
        /// Unlock ability by slot index.
        /// </summary>
        public void UnlockAbility(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= abilities.Length) return;

            _unlockedAbilities[slotIndex] = true;

            Debug.Log($"[PlayerAbility] Unlocked '{abilities[slotIndex].abilityName}'");

            // Show ability unlocked notification (HUD integration)
            Debug.Log($"[PlayerAbility] Ability unlocked: {abilities[slotIndex].abilityName}");
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
