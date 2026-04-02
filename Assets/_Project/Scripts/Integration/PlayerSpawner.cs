using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Player Spawner — runtime component that instantiates the Player prefab
    /// at the designated spawn point when a gameplay scene loads.
    /// Place this on a spawn-point GameObject in each zone scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Player Prefab")]
        [SerializeField] GameObject playerPrefab;

        [Header("Spawn Settings")]
        [SerializeField] Vector3 spawnOffset = new(0f, 1f, 0f);

        bool _spawned;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            if (_spawned) return;

            // Don't double-spawn if Player already exists
            var existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                Debug.Log("[PlayerSpawner] Player already exists — skipping spawn.");
                _spawned = true;
                return;
            }

            if (playerPrefab == null)
            {
                // Fallback: create a greybox capsule player at runtime
                var player = CreateFallbackPlayer();
                player.transform.position = transform.position + spawnOffset;
                Debug.Log("[PlayerSpawner] No prefab assigned — spawned fallback player.");
                _spawned = true;
                return;
            }

            var instance = Instantiate(playerPrefab, transform.position + spawnOffset, Quaternion.identity);
            instance.name = "Player";
            instance.tag = "Player";
            DontDestroyOnLoad(instance);
            _spawned = true;
            Debug.Log($"[PlayerSpawner] Player spawned at {instance.transform.position}");
        }

        /// <summary>
        /// Creates a minimal playable character when no prefab is assigned.
        /// Capsule + CharacterController + PlayerInputHandler.
        /// </summary>
        GameObject CreateFallbackPlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player") >= 0
                ? LayerMask.NameToLayer("Player") : 0;

            // Remove default CapsuleCollider — CharacterController provides its own
            var col = player.GetComponent<Collider>();
            if (col != null) Destroy(col);

            player.AddComponent<CharacterController>();
            player.AddComponent<Input.PlayerInputHandler>();

            DontDestroyOnLoad(player);
            return player;
        }
    }
}
