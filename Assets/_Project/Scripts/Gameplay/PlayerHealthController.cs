using UnityEngine;
using System;
using System.Collections;
using Tartaria.Save;
using Tartaria.Core;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Manages player health, damage, death, and respawn mechanics.
    /// Integrates with shield, regeneration, checkpoint system, and save/load.
    /// </summary>
    public class PlayerHealthController : MonoBehaviour, ISaveDataProvider
    {
        public static PlayerHealthController Instance { get; private set; }

        [Header("Health Settings")]
        [SerializeField] float _startingHealth = 100f;
        [SerializeField] float _regenDelay = 5f;
        [SerializeField] float _regenRate = 1f;
        [SerializeField] float _respawnDelay = 3f;

        [Header("Damage Feedback")]
        [SerializeField] float _invulnerabilityDuration = 0.5f;

        // Public Properties
        public float CurrentHealth { get; private set; }
        public float MaxHealth => PlayerProgression.Instance != null ? PlayerProgression.Instance.MaxHP : _startingHealth;
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        public bool IsAlive => CurrentHealth > 0f;
        public bool IsInvulnerable { get; private set; }

        // Events
        public event Action<float, float> OnDamaged;        // (amount, remainingHealth)
        public event Action<float> OnHealed;                // amount
        public event Action<float> OnHealthChanged;         // healthPercent
        public event Action OnPlayerDeath;
        public event Action OnPlayerRespawned;

        // Internal State
        float _lastDamageTime;
        float _invulnerabilityEndTime;
        bool _isDead;
        Vector3 _lastCheckpointPosition = Vector3.zero;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentHealth = MaxHealth;
        }

        void OnEnable()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.RegisterProvider(this);
            }

            // Subscribe to building restoration for checkpoint updates
            GameEvents.OnBuildingRestoredTyped += OnBuildingRestored;
        }

        void OnDisable()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.UnregisterProvider(this);
            }

            GameEvents.OnBuildingRestoredTyped -= OnBuildingRestored;
        }

        void Start()
        {
            // Initialize checkpoint to spawn position
            _lastCheckpointPosition = transform.position;
        }

        void Update()
        {
            if (!IsAlive || _isDead) return;

            // Regeneration when out of combat
            if (Time.time - _lastDamageTime > _regenDelay && CurrentHealth < MaxHealth)
            {
                Heal(_regenRate * Time.deltaTime);
            }

            // Update invulnerability
            if (IsInvulnerable && Time.time >= _invulnerabilityEndTime)
            {
                IsInvulnerable = false;
            }
        }

        /// <summary>
        /// Apply damage to the player.
        /// </summary>
        public void TakeDamage(float amount, GameObject instigator = null)
        {
            if (!IsAlive || _isDead || IsInvulnerable) return;

            // Shield mitigation disabled (PlayerAbilityController not active)
            // TODO: Re-enable when ability system restored
            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            _lastDamageTime = Time.time;

            // Invulnerability frames after taking damage
            IsInvulnerable = true;
            _invulnerabilityEndTime = Time.time + _invulnerabilityDuration;

            Debug.Log($"[PlayerHealth] TakeDamage: {amount:F1} | Remaining: {CurrentHealth:F1}/{MaxHealth:F1} ({HealthPercent:P0})");

            // Fire events
            OnDamaged?.Invoke(amount, CurrentHealth);
            OnHealthChanged?.Invoke(HealthPercent);

            // Raise GameEvents for UI/systems
            GameEvents.RaisePlayerDamaged(amount, CurrentHealth);

            // Check for death
            if (CurrentHealth <= 0f && !_isDead)
            {
                Die(instigator);
            }
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive || _isDead) return;

            float previousHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

            if (CurrentHealth > previousHealth)
            {
                OnHealed?.Invoke(amount);
                OnHealthChanged?.Invoke(HealthPercent);
            }
        }

        /// <summary>
        /// Instantly kill the player.
        /// </summary>
        public void Kill(GameObject instigator = null)
        {
            if (_isDead) return;

            CurrentHealth = 0f;
            Die(instigator);
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        void Die(GameObject instigator = null)
        {
            _isDead = true;
            Debug.Log("[PlayerHealth] Player died. Respawning in " + _respawnDelay + "s...");

            OnPlayerDeath?.Invoke();
            GameEvents.RaisePlayerDeath();

            // Disable player control (if PlayerController exists)
            var playerController = GetComponent<CharacterController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            StartCoroutine(RespawnAfterDelay(_respawnDelay));
        }

        /// <summary>
        /// Respawn player after delay.
        /// </summary>
        IEnumerator RespawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Restore health
            CurrentHealth = MaxHealth;
            _isDead = false;
            IsInvulnerable = false;

            // Teleport to last checkpoint
            if (_lastCheckpointPosition != Vector3.zero)
            {
                transform.position = _lastCheckpointPosition;
                Debug.Log($"[PlayerHealth] Respawned at checkpoint: {_lastCheckpointPosition}");
            }
            else
            {
                Debug.LogWarning("[PlayerHealth] No checkpoint set. Respawning at current position.");
            }

            // Re-enable player control
            var playerController = GetComponent<CharacterController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            // Fire events
            OnPlayerRespawned?.Invoke();
            OnHealthChanged?.Invoke(HealthPercent);
            GameEvents.RaisePlayerRespawned();

            Debug.Log($"[PlayerHealth] Respawned with {CurrentHealth:F1}/{MaxHealth:F1} HP");
        }

        /// <summary>
        /// Update checkpoint when a building is restored.
        /// </summary>
        void OnBuildingRestored(BuildingRestoredEventArgs args)
        {
            if (args.Building != null)
            {
                _lastCheckpointPosition = args.Building.transform.position + Vector3.up * 2f; // Spawn above building
                Debug.Log($"[PlayerHealth] Checkpoint updated to: {args.Building.name}");
            }
        }

        /// <summary>
        /// Manually set checkpoint position.
        /// </summary>
        public void SetCheckpoint(Vector3 position)
        {
            _lastCheckpointPosition = position;
            Debug.Log($"[PlayerHealth] Checkpoint manually set to: {position}");
        }

        /// <summary>
        /// Force heal to full health.
        /// </summary>
        public void RestoreToFull()
        {
            CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(HealthPercent);
            Debug.Log("[PlayerHealth] Health restored to full.");
        }

        /// <summary>
        /// Set player invulnerability state (for cutscenes, giant mode, etc.).
        /// </summary>
        public void SetInvulnerable(bool value)
        {
            // TODO Phase 2: Implement invulnerability mechanic
            Debug.Log($"[PlayerHealth] Invulnerability: {value}");
        }

        #region ISaveDataProvider Implementation

        public string GetProviderKey() => "PlayerHealth";

        public object GetSaveData()
        {
            return new PlayerHealthData
            {
                currentHealth = CurrentHealth,
                checkpointPosition = _lastCheckpointPosition
            };
        }

        public void RestoreSaveData(object data)
        {
            if (data == null)
            {
                CurrentHealth = MaxHealth;
                _lastCheckpointPosition = Vector3.zero;
                Debug.Log("[PlayerHealth] No saved data - initialized to defaults");
                return;
            }

            if (data is string json)
            {
                try
                {
                    var healthData = JsonUtility.FromJson<PlayerHealthData>(json);
                    CurrentHealth = healthData.currentHealth > 0 ? healthData.currentHealth : MaxHealth;
                    _lastCheckpointPosition = healthData.checkpointPosition;
                    _isDead = false;
                    IsInvulnerable = false;

                    OnHealthChanged?.Invoke(HealthPercent);
                    Debug.Log($"[PlayerHealth] Loaded state: {CurrentHealth:F1}/{MaxHealth:F1} HP, Checkpoint: {_lastCheckpointPosition}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PlayerHealth] Failed to deserialize: {e.Message}");
                }
            }
        }

        #endregion

        // Debug Methods
        void OnGUI()
        {
            if (!Debug.isDebugBuild) return;

            GUILayout.BeginArea(new Rect(10, 150, 300, 150));
            GUILayout.Label($"<b>PLAYER HEALTH</b>");
            GUILayout.Label($"HP: {CurrentHealth:F1} / {MaxHealth:F1} ({HealthPercent:P0})");
            GUILayout.Label($"Alive: {IsAlive} | Invuln: {IsInvulnerable}");
            GUILayout.Label($"Checkpoint: {_lastCheckpointPosition}");
            GUILayout.Label($"Time Since Damage: {Time.time - _lastDamageTime:F1}s");

            if (GUILayout.Button("Take 20 Damage"))
            {
                TakeDamage(20f);
            }
            if (GUILayout.Button("Heal 30 HP"))
            {
                Heal(30f);
            }
            if (GUILayout.Button("Kill Player"))
            {
                Kill();
            }
            GUILayout.EndArea();
        }
    }

    /// <summary>
    /// Serializable data class for PlayerHealthController save/load.
    /// MUST be serializable by JsonUtility (no generics, no null collections).
    /// </summary>
    [Serializable]
    public class PlayerHealthData
    {
        public float currentHealth = 100f;
        public Vector3 checkpointPosition = Vector3.zero;
    }
}
