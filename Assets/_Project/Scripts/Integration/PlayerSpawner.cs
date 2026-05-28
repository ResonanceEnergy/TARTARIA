using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// PlayerSpawner — CRITICAL MISSING SYSTEM
    /// Spawns player at designated position in scene.
    /// Called by EchohavenContentSpawner and other zone spawners.
    /// </summary>
    public class PlayerSpawner : MonoBehaviour
    {
        public static PlayerSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 defaultSpawnRotation = new Vector3(0f, 0f, 0f);

        [Header("Runtime State")]
        [SerializeField] private GameObject spawnedPlayer;
        [SerializeField] private bool playerSpawned = false;

        private Vector3 _currentSpawnPosition;
        private Vector3 _currentSpawnRotation;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _currentSpawnPosition = defaultSpawnPosition;
            _currentSpawnRotation = defaultSpawnRotation;
        }

        void Start()
        {
            // Auto-spawn player if not already spawned
            if (!playerSpawned && playerPrefab != null)
            {
                SpawnPlayer();
            }
            else if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] CRITICAL: No player prefab assigned! Player cannot spawn.");
            }
        }

        /// <summary>
        /// Set spawn position (called by zone content spawners).
        /// </summary>
        public void SetSpawnPosition(Vector3 position, Vector3? rotation = null)
        {
            _currentSpawnPosition = position;
            if (rotation.HasValue)
                _currentSpawnRotation = rotation.Value;

            Debug.Log($"[PlayerSpawner] Spawn position set: {position}");
        }

        /// <summary>
        /// Spawn player at current spawn position.
        /// </summary>
        public GameObject SpawnPlayer()
        {
            if (playerSpawned && spawnedPlayer != null)
            {
                Debug.LogWarning("[PlayerSpawner] Player already spawned!");
                return spawnedPlayer;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("[PlayerSpawner] Cannot spawn player - no prefab assigned!");
                return null;
            }

            Debug.Log($"[PlayerSpawner] Spawning player at {_currentSpawnPosition}");

            spawnedPlayer = Instantiate(playerPrefab, _currentSpawnPosition, Quaternion.Euler(_currentSpawnRotation));
            spawnedPlayer.name = "Player";
            playerSpawned = true;

            // Notify systems
            GameEvents.FirePlayerSpawned(spawnedPlayer);

            Debug.Log("[PlayerSpawner] ✅ Player spawned successfully!");
            return spawnedPlayer;
        }

        /// <summary>
        /// Despawn current player (for scene transitions).
        /// </summary>
        public void DespawnPlayer()
        {
            if (spawnedPlayer != null)
            {
                Destroy(spawnedPlayer);
                spawnedPlayer = null;
            }
            playerSpawned = false;

            Debug.Log("[PlayerSpawner] Player despawned");
        }

        /// <summary>
        /// Respawn player at current spawn position (after death).
        /// </summary>
        public void RespawnPlayer()
        {
            DespawnPlayer();
            SpawnPlayer();
            Debug.Log("[PlayerSpawner] Player respawned");
        }

        public GameObject GetPlayer() => spawnedPlayer;
        public bool IsPlayerSpawned() => playerSpawned && spawnedPlayer != null;
    }
}
