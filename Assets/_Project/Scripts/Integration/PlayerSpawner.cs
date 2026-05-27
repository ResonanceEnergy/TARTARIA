using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Core;
using Tartaria.Core.Data;

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

        [Header("Visual Profile (optional)")]
        [SerializeField, Tooltip("ScriptableObject describing the active character visual " +
                                  "(mesh, animator, footsteps, VFX). Null = use prefab as-is.")]
        CharacterVisualProfile visualProfile;

        [Header("Input")]
        [SerializeField] InputActionAsset inputActions;

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

            // Determine spawn position from PlayerSpawn marker (set by EchohavenScenePopulator)
            var spawnMarker = GameObject.Find("PlayerSpawn");
            Vector3 spawnPos = spawnMarker != null
                ? spawnMarker.transform.position + spawnOffset
                : transform.position + spawnOffset;

            // M2 Save/Load UX polish: On resume (playTime > 0), override marker with saved player position
            // so load from MainMenu "Continue" or QuickLoad lands player exactly where they left off.
            // Coordinates with GameLoopController.OnAfterLoad (which also syncs ECS + MB post-spawn).
            var sm = Tartaria.Save.SaveManager.Instance;
            if (sm != null && sm.CurrentSave != null && sm.CurrentSave.header != null &&
                sm.CurrentSave.header.playTimeSeconds > 0.5f && sm.CurrentSave.player != null)
            {
                var p = sm.CurrentSave.player.position;
                spawnPos = new Vector3(p.x, Mathf.Max(p.y, 1.2f), p.z);
                Debug.Log($"[PlayerSpawner] M2 resume: overriding spawnPos with saved player pos {spawnPos}");
            }

            // If Player already exists (e.g. baked in scene or DontDestroyOnLoad), destroy it
            // and let the spawner create a fresh prefab instance at the correct position.
            // Relocation via transform.position doesn't stick with CharacterController.
            var existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                Debug.Log($"[PlayerSpawner] Destroying stale Player at {existing.transform.position}");
                Destroy(existing);
            }

            if (playerPrefab == null)
            {
                var player = CreateFallbackPlayer();
                player.transform.position = spawnPos;
                Debug.Log("[PlayerSpawner] No prefab assigned — spawned fallback player.");
                _spawned = true;
                Tartaria.Core.SceneLoader.OnPlayerSpawnComplete();
                return;
            }

            var instance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            instance.name = "Player";
            instance.tag = "Player";

            // Ensure PlayerInputHandler has inputActions (safety net if prefab reference is missing)
            if (inputActions != null)
            {
                var handler = instance.GetComponent<Input.PlayerInputHandler>();
                if (handler != null)
                {
                    var field = typeof(Input.PlayerInputHandler).GetField("inputActions",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null && field.GetValue(handler) == null)
                    {
                        field.SetValue(handler, inputActions);
                        handler.enabled = false;
                        handler.enabled = true; // Re-trigger OnEnable → SetupInputActions
                    }
                }
            }

            // Wire interactableLayer mask on the runtime player so Physics.Raycast
            // actually hits things. Without this, the mask is 0 and no interactable
            // is ever found — the #1 cause of "E does nothing".
            EnsureInteractableLayerMask(instance);

            // Apply visual profile (mesh / animator) — zero-op if profile is null.
            ApplyVisualProfile(instance);

            DontDestroyOnLoad(instance);
            _spawned = true;
            Debug.Log($"[PlayerSpawner] Player spawned at {instance.transform.position}");
            
            // Notify SceneLoader that player spawn is complete
            // (SceneLoader waits for this callback before transitioning to Exploration)
            Tartaria.Core.SceneLoader.OnPlayerSpawnComplete();
        }

        /// <summary>
        /// Applies a CharacterVisualProfile to the spawned player.
        /// Swaps mesh + animator without touching CharacterController, scripts or input.
        /// Null profile = no-op (keeps prefab default — capsule/placeholder).
        /// </summary>
        void ApplyVisualProfile(GameObject instance)
        {
            if (visualProfile == null) return;

            // Swap mesh: remove existing "PlayerMesh" child if any, instantiate new.
            if (visualProfile.HasMesh)
            {
                var existingMesh = instance.transform.Find("PlayerMesh");
                if (existingMesh != null) Destroy(existingMesh.gameObject);

                var mesh = Instantiate(visualProfile.meshPrefab, instance.transform);
                mesh.name = "PlayerMesh";
                mesh.transform.localPosition = new Vector3(0f, visualProfile.meshYOffset, 0f);
                mesh.transform.localRotation = Quaternion.identity;
                Debug.Log($"[PlayerSpawner] Applied mesh from profile: {visualProfile.characterId}");
            }

            // Swap animator controller (only if profile has one and player has Animator).
            if (visualProfile.HasAnimator)
            {
                var animator = instance.GetComponentInChildren<Animator>();
                if (animator != null)
                {
                    animator.runtimeAnimatorController = visualProfile.animatorController;
                    if (visualProfile.avatar != null) animator.avatar = visualProfile.avatar;
                    Debug.Log($"[PlayerSpawner] Applied animator: {visualProfile.animatorController.name}");
                }
            }
        }

        /// <summary>
        /// Sets the LayerMask field on PlayerInputHandler so interaction raycasts
        /// actually have layers to hit. Includes Building, Interactable, Trigger,
        /// Enemy and NPC tag layer if present.
        /// </summary>
        static void EnsureInteractableLayerMask(GameObject player)
        {
            var handler = player.GetComponent<Input.PlayerInputHandler>();
            if (handler == null) return;
            var field = typeof(Input.PlayerInputHandler).GetField("interactableLayer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null) return;

            int mask = 0;
            int building     = LayerMask.NameToLayer("Building");
            int interactable = LayerMask.NameToLayer("Interactable");
            int trigger      = LayerMask.NameToLayer("Trigger");
            int enemy        = LayerMask.NameToLayer("Enemy");
            if (building     >= 0) mask |= (1 << building);
            if (interactable >= 0) mask |= (1 << interactable);
            if (trigger      >= 0) mask |= (1 << trigger);
            if (enemy        >= 0) mask |= (1 << enemy);
            // Always include Default so prop-on-default-layer with IInteractable still works.
            mask |= (1 << 0);

            field.SetValue(handler, (LayerMask)mask);
            Debug.Log($"[PlayerSpawner] interactableLayer mask set to 0x{mask:X}");
        }

        /// <summary>
        /// Creates a minimal playable character when no prefab is assigned.
        /// Uses KayKit Knight prefab as fallback.
        /// </summary>
        GameObject CreateFallbackPlayer()
        {
            // Load KayKit Knight for default player
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Characters/KayKit/Char_Knight");
            GameObject player;
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                player.name = "Player";
            }
            else
            {
                Debug.LogError("[PlayerSpawner] CRITICAL: Char_Knight prefab missing for player");
                player = new GameObject("Player_MISSING_PREFAB");
            }

            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Player") >= 0
                ? LayerMask.NameToLayer("Player") : 0;

            // Remove default CapsuleCollider — CharacterController provides its own
            var col = player.GetComponent<Collider>();
            if (col != null) Destroy(col);

            player.AddComponent<CharacterController>();
            var handler = player.AddComponent<Input.PlayerInputHandler>();

            // Wire input actions if available
            if (inputActions != null)
            {
                // Use reflection to set private serialized field at runtime
                var field = typeof(Input.PlayerInputHandler).GetField("inputActions",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(handler, inputActions);
                    handler.enabled = false;
                    handler.enabled = true; // Re-trigger OnEnable → SetupInputActions
                }
            }

            DontDestroyOnLoad(player);
            return player;
        }
    }
}
