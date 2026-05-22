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
    }
}
