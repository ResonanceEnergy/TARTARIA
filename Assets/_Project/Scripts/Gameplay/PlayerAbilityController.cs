using UnityEngine;
using Tartaria.Input;
using Tartaria.Core;
// NOTE: Cannot use 'using Tartaria.AI;' - would create circular dependency (AI depends on Gameplay)

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Manages player special abilities: Harmonic Strike (F), Frequency Shield (Q), Aether Vision (V).
    /// Integrated with PlayerInputHandler events, EconomySystem RS costs, and GameEvents pub/sub.
    /// </summary>
    public class PlayerAbilityController : MonoBehaviour
    {
        [Header("Harmonic Strike (F)")]
        [SerializeField] float harmonicDamage = 50f;
        [SerializeField] float harmonicRadius = 5f;
        [SerializeField] float harmonicCooldown = 8f;
        [SerializeField] float harmonicRSCost = 20f; // P1.L3: real RS economy via AetherFieldManager
        [SerializeField] LayerMask enemyLayerMask = -1; // Default to all layers

        [Header("Frequency Shield (Q)")]
        [SerializeField] float shieldDuration = 5f;
        [SerializeField] float shieldCooldown = 12f;
        [SerializeField] float shieldRSCost = 15f; // P1.L3: real RS economy via AetherFieldManager

        [Header("Aether Vision (V)")]
        [SerializeField] bool aetherVisionEnabled = false;

        // Cooldown timers
        float _harmonicCooldownRemaining = 0f;
        float _shieldCooldownRemaining = 0f;

        // Shield state
        float _shieldEndTime = 0f;

        /// <summary>
        /// Public property for damage mitigation logic in PlayerHealth or other systems.
        /// </summary>
        public bool ShieldActive => Time.time < _shieldEndTime;

        /// <summary>
        /// Public property for UI or other systems to check Aether Vision state.
        /// </summary>
        public bool AetherVisionActive => aetherVisionEnabled;

        void OnEnable()
        {
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnHarmonicStrike += TryHarmonicStrike;
                PlayerInputHandler.Instance.OnFrequencyShield += TryFrequencyShield;
                // PlayerInputHandler.Instance.OnAetherVisionToggle += ToggleAetherVision; // Event missing (Phase 22)
            }
        }

        void OnDisable()
        {
            if (PlayerInputHandler.Instance != null)
            {
                PlayerInputHandler.Instance.OnHarmonicStrike -= TryHarmonicStrike;
                PlayerInputHandler.Instance.OnFrequencyShield -= TryFrequencyShield;
                // PlayerInputHandler.Instance.OnAetherVisionToggle -= ToggleAetherVision; // Event missing (Phase 22)
            }
        }

        void Update()
        {
            // Decrement cooldown timers
            if (_harmonicCooldownRemaining > 0f)
            {
                _harmonicCooldownRemaining -= Time.deltaTime;
            }

            if (_shieldCooldownRemaining > 0f)
            {
                _shieldCooldownRemaining -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Harmonic Strike (F): AOE damage to all enemies within 5m radius.
        /// Costs 20 RS, 8s cooldown, 50 damage per enemy.
        /// </summary>
        void TryHarmonicStrike()
        {
            // Check cooldown
            if (_harmonicCooldownRemaining > 0f)
            {
                Debug.Log($"Harmonic Strike on cooldown: {_harmonicCooldownRemaining:F1}s remaining");
                return;
            }

            // P1.L3: Check RS cost via canonical AetherFieldManager (Core/AetherFieldManager.cs:26)
            var aether = AetherFieldManager.Instance;
            if (aether == null)
            {
                Debug.LogWarning("[PlayerAbilityController] Harmonic Strike: AetherFieldManager.Instance is null — cannot read RS. Aborting cast.");
                return;
            }
            if (aether.ResonanceScore < harmonicRSCost)
            {
                Debug.Log($"[PlayerAbilityController] Not enough RS for Harmonic Strike (need {harmonicRSCost}, have {aether.ResonanceScore:F1})");
                return;
            }

            // P1.L3: Spend RS via canonical AetherFieldManager.DeductRS (Core/AetherFieldManager.cs:59)
            // and broadcast via GameEvents.FireRSChange so HUD/Music/economy monitor see the delta (Core/GameEvents.cs:323).
            aether.DeductRS(harmonicRSCost);
            GameEvents.FireRSChange(-harmonicRSCost);

            // AOE damage
            Collider[] hits = Physics.OverlapSphere(transform.position, harmonicRadius, enemyLayerMask);
            int enemiesHit = 0;

            foreach (Collider hit in hits)
            {
                // Use SendMessage to avoid Tartaria.AI circular dependency
                // MudGolemHealth.TakeDamage(float damage, GameObject instigator)
                hit.SendMessage("TakeDamage", harmonicDamage, SendMessageOptions.DontRequireReceiver);
                enemiesHit++;
            }

            Debug.Log($"Harmonic Strike hit {enemiesHit} enemies for {harmonicDamage} damage each");

            // Start cooldown
            _harmonicCooldownRemaining = harmonicCooldown;
        }

        /// <summary>
        /// Frequency Shield (Q): Damage mitigation for 5 seconds.
        /// Costs 15 RS, 12s cooldown. Other systems check ShieldActive property.
        /// </summary>
        void TryFrequencyShield()
        {
            // Check cooldown
            if (_shieldCooldownRemaining > 0f)
            {
                Debug.Log($"Frequency Shield on cooldown: {_shieldCooldownRemaining:F1}s remaining");
                return;
            }

            // P1.L3: Check RS cost via canonical AetherFieldManager (Core/AetherFieldManager.cs:26)
            var aether = AetherFieldManager.Instance;
            if (aether == null)
            {
                Debug.LogWarning("[PlayerAbilityController] Frequency Shield: AetherFieldManager.Instance is null — cannot read RS. Aborting cast.");
                return;
            }
            if (aether.ResonanceScore < shieldRSCost)
            {
                Debug.Log($"[PlayerAbilityController] Not enough RS for Frequency Shield (need {shieldRSCost}, have {aether.ResonanceScore:F1})");
                return;
            }

            // P1.L3: Spend RS via canonical AetherFieldManager.DeductRS (Core/AetherFieldManager.cs:59)
            // and broadcast via GameEvents.FireRSChange so HUD/Music/economy monitor see the delta (Core/GameEvents.cs:323).
            aether.DeductRS(shieldRSCost);
            GameEvents.FireRSChange(-shieldRSCost);

            // Activate shield
            _shieldEndTime = Time.time + shieldDuration;
            Debug.Log($"Frequency Shield activated for {shieldDuration}s");

            // Start cooldown
            _shieldCooldownRemaining = shieldCooldown;
        }

        /// <summary>
        /// Aether Vision (V): Toggle highlight of interactive objects.
        /// Emits GameEvents.OnAetherVisionToggled for renderer/UI systems to subscribe.
        /// </summary>
        void ToggleAetherVision()
        {
            aetherVisionEnabled = !aetherVisionEnabled;
            Debug.Log($"Aether Vision {(aetherVisionEnabled ? "enabled" : "disabled")}");

            // Emit event for other systems (renderers, UI, etc.)
            GameEvents.RaiseAetherVisionToggled(aetherVisionEnabled);
        }

        /// <summary>
        /// Public API for UI systems to display cooldown progress.
        /// </summary>
        public float GetHarmonicStrikeCooldownPercent()
        {
            if (harmonicCooldown <= 0f) return 1f;
            return Mathf.Clamp01(1f - (_harmonicCooldownRemaining / harmonicCooldown));
        }

        /// <summary>
        /// Public API for UI systems to display cooldown progress.
        /// </summary>
        public float GetFrequencyShieldCooldownPercent()
        {
            if (shieldCooldown <= 0f) return 1f;
            return Mathf.Clamp01(1f - (_shieldCooldownRemaining / shieldCooldown));
        }

        /// <summary>
        /// Public API for UI systems to display remaining shield time.
        /// </summary>
        public float GetShieldTimeRemaining()
        {
            return Mathf.Max(0f, _shieldEndTime - Time.time);
        }

        void OnDrawGizmosSelected()
        {
            // Visualize Harmonic Strike radius in editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, harmonicRadius);
        }
    }
}
