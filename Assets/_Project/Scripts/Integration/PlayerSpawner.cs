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
            // Magenta-fix: convert any default/error material on the spawned Player to URP/Lit
            try
            {
                var urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    foreach (var r in spawnedPlayer.GetComponentsInChildren<Renderer>(true))
                    {
                        var mats = r.sharedMaterials;
                        bool changed = false;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            if (mats[i] == null || mats[i].shader == null || mats[i].shader.name.Contains("Hidden/InternalErrorShader"))
                            {
                                mats[i] = new Material(urpLit) { name = "Player_URPLit_Fallback" };
                                mats[i].SetColor("_BaseColor", new Color(0.85f, 0.78f, 0.65f, 1f));
                                changed = true;
                            }
                        }
                        if (changed) r.sharedMaterials = mats;
                    }
                }
            }
            catch (System.Exception ex) { Debug.LogWarning($"[PlayerSpawner] Magenta-fix failed: {ex.Message}"); }
            spawnedPlayer.name = "Player";
            playerSpawned = true;

            // SAFETY: ensure movement components exist even if the prefab is incomplete.
            // 2026-05-30: was finding Player capsules with no PlayerInputHandler → couldn't walk.
            if (spawnedPlayer.GetComponent<UnityEngine.CharacterController>() == null)
            {
                var cc = spawnedPlayer.AddComponent<UnityEngine.CharacterController>();
                cc.center = new Vector3(0f, 1f, 0f);
                cc.radius = 0.5f;
                cc.height = 2f;
                Debug.Log("[PlayerSpawner] Auto-added missing CharacterController");
            }
            if (spawnedPlayer.GetComponent<Tartaria.Input.PlayerInputHandler>() == null)
            {
                spawnedPlayer.AddComponent<Tartaria.Input.PlayerInputHandler>();
                Debug.Log("[PlayerSpawner] Auto-added missing PlayerInputHandler (left stick + WASD now work)");
            }
            if (spawnedPlayer.GetComponent<Tartaria.Gameplay.GiantMode>() == null)
            {
                spawnedPlayer.AddComponent<Tartaria.Gameplay.GiantMode>();
                Debug.Log("[PlayerSpawner] Auto-added GiantMode (press G or RT to activate)");
            }
            if (spawnedPlayer.tag != "Player") spawnedPlayer.tag = "Player";

            // Notify systems
            GameEvents.FirePlayerSpawned(spawnedPlayer.transform.position);

            Debug.Log("[PlayerSpawner] ✅ Player spawned successfully!");
            return spawnedPlayer;
        }

        /// <summary>
        /// Lookup helpers used by other systems (e.g. MudGolemEnemy).
        /// </summary>
        public bool IsPlayerSpawned() => playerSpawned && spawnedPlayer != null;
        public GameObject GetPlayer() => spawnedPlayer;

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
    }
}
