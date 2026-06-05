using UnityEngine;
using System.Collections.Generic;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Player abilities and special powers system.
    /// Manages resonance abilities, skill cooldowns, resource costs.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerAbilities : MonoBehaviour
    {
        [Header("Resonance Abilities")]
        [SerializeField] bool canChannelRS = true;
        [SerializeField] float channelRatePerSecond = 50f;
        [SerializeField] float maxChannelCapacity = 1000f;

        [Header("Cooldowns")]
        [SerializeField] float dodgeCooldown = 1f;
        [SerializeField] float specialAbilityCooldown = 5f;

        float _lastDodgeTime;
        float _lastSpecialTime;

        // Buff multipliers (Moon 5 amplification fields)
        readonly Dictionary<string, float> _rsMultipliers = new();
        readonly Dictionary<string, float> _speedMultipliers = new();
        readonly Dictionary<string, float> _resistanceMultipliers = new();

        public bool CanChannelResonance => canChannelRS;
        public float ChannelRate => channelRatePerSecond;
        public float MaxChannelCapacity => maxChannelCapacity;

        public bool CanDodge => Time.time - _lastDodgeTime >= dodgeCooldown;
        public bool CanUseSpecial => Time.time - _lastSpecialTime >= specialAbilityCooldown;

        /// <summary>
        /// Attempt to channel RS from player's reserves.
        /// Returns amount actually channeled (may be less than requested).
        /// </summary>
        public float ChannelResonance(float amount, float deltaTime)
        {
            if (!CanChannelResonance) return 0f;

            float maxThisFrame = channelRatePerSecond * deltaTime;
            float actualAmount = Mathf.Min(amount, maxThisFrame);

            // P1.L3: route channel cost through canonical AetherFieldManager (Core/AetherFieldManager.cs:26)
            // so the RS bar actually depletes while the player channels. Clamp to available RS so we don't
            // overdraw and so the returned amount reflects what was really spent.
            var aether = AetherFieldManager.Instance;
            if (aether != null)
            {
                float available = aether.ResonanceScore;
                if (available <= 0f)
                {
                    return 0f;
                }
                actualAmount = Mathf.Min(actualAmount, available);
                aether.DeductRS(actualAmount);
                // Broadcast via GameEvents.FireRSChange (Core/GameEvents.cs:323) so HUD + AdaptiveMusic update.
                GameEvents.FireRSChange(-actualAmount);
            }
            else
            {
                Debug.LogWarning($"[PlayerAbilities] ChannelResonance: AetherFieldManager.Instance is null — channeled {actualAmount:F1} RS will not deplete the global RS bar.");
            }

            return actualAmount;
        }

        public void UseDodge()
        {
            _lastDodgeTime = Time.time;
        }

        public void UseSpecialAbility()
        {
            _lastSpecialTime = Time.time;
        }

        public float GetDodgeCooldownRemaining()
        {
            return Mathf.Max(0f, dodgeCooldown - (Time.time - _lastDodgeTime));
        }

        public float GetSpecialCooldownRemaining()
        {
            return Mathf.Max(0f, specialAbilityCooldown - (Time.time - _lastSpecialTime));
        }

        /// <summary>
        /// Consume RS from player reserves.
        /// Returns true if enough RS was available, false otherwise.
        /// </summary>
        public bool ConsumeRS(float amount)
        {
            var economy = EconomySystem.Instance;
            if (economy == null)
            {
                Debug.LogWarning("[PlayerAbilities] ConsumeRS: EconomySystem not found");
                return false;
            }
            return economy.SpendCurrency(CurrencyType.ResonanceShards, (int)amount);
        }

        /// <summary>
        /// Unlock 9-band energy ability (Moon 7 reward).
        /// </summary>
        public void Unlock9BandEnergy()
        {
            PlayerProgression.Instance?.UnlockFeature("9band_energy");
            Debug.Log("[PlayerAbilities] 9-Band Energy unlocked!");
        }

        /// <summary>
        /// Unlock harmonic rock cutting ability (Moon 7 reward).
        /// </summary>
        public void UnlockHarmonicRockCutting()
        {
            PlayerProgression.Instance?.UnlockFeature("harmonic_rock_cutting");
            Debug.Log("[PlayerAbilities] Harmonic Rock Cutting unlocked!");
        }

        // --- Buff multiplier system (Moon 5 amplification fields) ---

        public void AddRSMultiplier(string id, float multiplier)
        {
            _rsMultipliers[id] = multiplier;
            Debug.Log($"[PlayerAbilities] AddRSMultiplier: {id} = +{multiplier * 100}%");
        }

        public void AddSpeedMultiplier(string id, float multiplier)
        {
            _speedMultipliers[id] = multiplier;
            Debug.Log($"[PlayerAbilities] AddSpeedMultiplier: {id} = +{multiplier * 100}%");
        }

        public void AddResistanceMultiplier(string id, float multiplier)
        {
            _resistanceMultipliers[id] = multiplier;
            Debug.Log($"[PlayerAbilities] AddResistanceMultiplier: {id} = +{multiplier * 100}%");
        }

        public void RemoveRSMultiplier(string id)
        {
            _rsMultipliers.Remove(id);
            Debug.Log($"[PlayerAbilities] RemoveRSMultiplier: {id}");
        }

        public void RemoveSpeedMultiplier(string id)
        {
            _speedMultipliers.Remove(id);
            Debug.Log($"[PlayerAbilities] RemoveSpeedMultiplier: {id}");
        }

        public void RemoveResistanceMultiplier(string id)
        {
            _resistanceMultipliers.Remove(id);
            Debug.Log($"[PlayerAbilities] RemoveResistanceMultiplier: {id}");
        }

        /// <summary>
        /// Get total RS multiplier from all active buffs (1.0 = no buffs).
        /// </summary>
        public float GetTotalRSMultiplier()
        {
            float total = 1f;
            foreach (var mult in _rsMultipliers.Values)
                total += mult;
            return total;
        }

        /// <summary>
        /// Get total speed multiplier from all active buffs (1.0 = no buffs).
        /// </summary>
        public float GetTotalSpeedMultiplier()
        {
            float total = 1f;
            foreach (var mult in _speedMultipliers.Values)
                total += mult;
            return total;
        }

        /// <summary>
        /// Get total resistance multiplier from all active buffs (1.0 = no buffs).
        /// </summary>
        public float GetTotalResistanceMultiplier()
        {
            float total = 1f;
            foreach (var mult in _resistanceMultipliers.Values)
                total += mult;
            return total;
        }
    }
}
