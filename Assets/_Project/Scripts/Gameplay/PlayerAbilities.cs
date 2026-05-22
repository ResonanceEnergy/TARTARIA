using UnityEngine;
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

            // TODO: Integrate with ResonanceScoreSystem (ECS)
            // For now, just return the requested amount capped by channel rate

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
            // TODO: Integrate with ResonanceScoreSystem
            // For now, always return true (assume infinite RS for beta)
            Debug.Log($"[PlayerAbilities] ConsumeRS: {amount} RS consumed");
            return true;
        }

        /// <summary>
        /// Unlock 9-band energy ability (Moon 7 reward).
        /// </summary>
        public void Unlock9BandEnergy()
        {
            Debug.Log("[PlayerAbilities] 9-Band Energy unlocked!");
            // TODO: Set ability flags, update UI
        }

        /// <summary>
        /// Unlock harmonic rock cutting ability (Moon 7 reward).
        /// </summary>
        public void UnlockHarmonicRockCutting()
        {
            Debug.Log("[PlayerAbilities] Harmonic Rock Cutting unlocked!");
            // TODO: Set ability flags, update UI
        }

        // --- Temporary stat multiplier stubs (Moon 5 amplification fields) ---
        // TODO: Integrate with proper stat/buff system

        public void AddRSMultiplier(string id, float multiplier)
        {
            Debug.Log($"[PlayerAbilities] AddRSMultiplier: {id} = +{multiplier * 100}%");
            // TODO: Apply actual multiplier to RS generation
        }

        public void AddSpeedMultiplier(string id, float multiplier)
        {
            Debug.Log($"[PlayerAbilities] AddSpeedMultiplier: {id} = +{multiplier * 100}%");
            // TODO: Apply actual speed boost
        }

        public void AddResistanceMultiplier(string id, float multiplier)
        {
            Debug.Log($"[PlayerAbilities] AddResistanceMultiplier: {id} = +{multiplier * 100}%");
            // TODO: Apply actual damage resistance
        }

        public void RemoveRSMultiplier(string id)
        {
            Debug.Log($"[PlayerAbilities] RemoveRSMultiplier: {id}");
            // TODO: Remove multiplier
        }

        public void RemoveSpeedMultiplier(string id)
        {
            Debug.Log($"[PlayerAbilities] RemoveSpeedMultiplier: {id}");
            // TODO: Remove multiplier
        }

        public void RemoveResistanceMultiplier(string id)
        {
            Debug.Log($"[PlayerAbilities] RemoveResistanceMultiplier: {id}");
            // TODO: Remove multiplier
        }
    }
}
