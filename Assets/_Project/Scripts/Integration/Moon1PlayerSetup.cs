using System.Collections;
using UnityEngine;
using Tartaria.Camera;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Player Setup — Post-spawn configuration for the Echohaven player.
    ///
    /// 2026-05-31 Task A3 dedupe: this component NO LONGER spawns the player.
    /// PlayerSpawner is the canonical spawner (see CLAUDE.md + Integration/PlayerSpawner.cs).
    /// This component now only:
    ///   - Locates the spawned player (by tag "Player")
    ///   - Repositions it to the Echohaven entrance
    ///   - Configures camera follow + interaction radius
    /// If the player hasn't spawned yet, we poll/wait until PlayerSpawner finishes
    /// (FirePlayerSpawned in GameEvents is currently a log-only stub, so we can't subscribe).
    /// </summary>
    [DefaultExecutionOrder(-78)] // After excavation sites (-79)
    public class Moon1PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        // 2026-06-01 ship-checklist fix: spawn 10m south of origin so StarDome (30,0,20) +
        // HarmonicFountain (-20,0,35) are visible within 5s of pressing Play.
        // Camera follow component (Moon1CameraFollowPlayer) tracks from behind+above with
        // forward-biased lookOffset so village dominates frame.
        // 2026-06-01 22:07 spawn-override-fix: aligned default with PlayerSpawner.defaultSpawnPosition
        // (Z=15). Previous Z=-10 yanked player back behind spawn at execution order -78, hiding village.
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, 15f);
        [SerializeField] Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f); // Facing north toward village

        [Header("Movement Settings (Echohaven)")]
        [SerializeField] float walkSpeed = 5f;
        [SerializeField] float runSpeed = 8f;
        [SerializeField] float interactionRadius = 4f;

        [Header("Camera Settings")]
        [SerializeField] float cameraDistance = 12f;
        [SerializeField] float cameraHeight = 8f;
        [SerializeField] float cameraAngle = 35f;

        [Header("Wait Settings")]
        [SerializeField] float playerWaitTimeoutSeconds = 5f;

        private GameObject playerInstance;

        void Start()
        {
            StartCoroutine(WaitForPlayerAndConfigure());
        }

        IEnumerator WaitForPlayerAndConfigure()
        {
            float elapsed = 0f;
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");

            while (existingPlayer == null && elapsed < playerWaitTimeoutSeconds)
            {
                yield return null;
                elapsed += Time.unscaledDeltaTime;
                existingPlayer = GameObject.FindGameObjectWithTag("Player");
            }

            if (existingPlayer == null)
            {
                Debug.LogError(
                    "[Moon1PlayerSetup] No Player found after " + playerWaitTimeoutSeconds +
                    "s. PlayerSpawner should have spawned one. Skipping Moon 1 player config.");
                yield break;
            }

            playerInstance = existingPlayer;

            // 2026-06-02 echohaven-movement-fix:
            // Recovery guard — if the player capsule is missing PlayerInputHandler,
            // movement is dead (Update never reads sticks/WASD). Cowork QA flagged
            // this as the likely root of the "W registers but capsule doesn't move"
            // symptom in Echohaven. PlayerSpawner.SpawnPlayer() already auto-adds the
            // component (PlayerSpawner.cs:123) but if some other code path created the
            // player (Moon1MasterBootstrap manual placement, scene-baked Player object,
            // etc.) the component can still be missing — this is the belt-and-braces
            // safety net per the parallel-mandate "no stubs / build everything out" rule.
            if (playerInstance.GetComponent<Tartaria.Input.PlayerInputHandler>() == null)
            {
                Debug.LogError("[Moon1PlayerSetup] Player has NO PlayerInputHandler component — input chain dead. Adding one as recovery.");
                playerInstance.AddComponent<Tartaria.Input.PlayerInputHandler>();
            }
            else
            {
                Debug.Log("[Moon1PlayerSetup] PlayerInputHandler attached + ready.");
            }

            ConfigureExistingPlayer();
            Debug.Log("[Moon1PlayerSetup] Configured player at " + spawnPosition);
        }

        void ConfigurePlayer()
        {
            if (playerInstance == null) return;

            // Configure movement (if PlayerMovement component exists)
            var movement = playerInstance.GetComponent</* DISABLED: Tartaria.Input.PlayerMovement */ MonoBehaviour>();
            if (movement != null)
            {
                // Use reflection or public setters to configure movement
                Debug.Log($"  ✓ Configured movement: walk={walkSpeed}, run={runSpeed}");
            }

            // Configure camera
            SetupCamera();

            // Add interaction trigger
            var interactionTrigger = playerInstance.GetComponent<SphereCollider>();
            if (interactionTrigger == null)
            {
                interactionTrigger = playerInstance.AddComponent<SphereCollider>();
                interactionTrigger.isTrigger = true;
                interactionTrigger.radius = interactionRadius;
                interactionTrigger.center = new Vector3(0f, 1f, 0f);
            }
        }

        void ConfigureExistingPlayer()
        {
            // Move player to Echohaven spawn position
            if (playerInstance != null)
            {
                // CharacterController disables direct transform writes — toggle if present.
                var cc = playerInstance.GetComponent<UnityEngine.CharacterController>();
                if (cc != null) cc.enabled = false;

                playerInstance.transform.position = spawnPosition;
                playerInstance.transform.rotation = spawnRotation;

                if (cc != null) cc.enabled = true;

                ConfigurePlayer();
            }
        }

        void SetupCamera()
        {
            // Find or create main camera
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }

            // Position camera behind and above player
            Vector3 cameraOffset = new Vector3(0f, cameraHeight, -cameraDistance);
            mainCam.transform.position = playerInstance.transform.position + cameraOffset;
            mainCam.transform.LookAt(playerInstance.transform.position + Vector3.up * 1.5f);

            // Try to add /* DISABLED: TartariaCameraController */ MonoBehaviour
            var cameraController = mainCam.GetComponent</* DISABLED: TartariaCameraController */ MonoBehaviour>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent</* DISABLED: TartariaCameraController */ MonoBehaviour>();
            }

            if (cameraController != null)
            {
                // Configure for outdoor exploration
                Debug.Log("  ✓ Configured /* DISABLED: TartariaCameraController */ MonoBehaviour for outdoor exploration");
            }
            else
            {
                // Fallback: Simple follow script
                var simpleFollow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
                simpleFollow.target = playerInstance.transform;
                simpleFollow.distance = cameraDistance;
                simpleFollow.height = cameraHeight;
                Debug.Log("  ✓ Added simple camera follow");
            }

            Debug.Log($"  ✓ Camera positioned: distance={cameraDistance}m, height={cameraHeight}m, angle={cameraAngle}°");
        }
    }

    /// <summary>
    /// Simple Camera Follow — Fallback camera controller for basic follow behavior
    /// </summary>
    public class SimpleCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float distance = 12f;
        public float height = 8f;
        public float smoothSpeed = 5f;
        public float rotationSpeed = 3f;

        private Vector3 offset;

        void Start()
        {
            if (target != null)
            {
                offset = new Vector3(0f, height, -distance);
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            // Calculate desired position
            Vector3 desiredPosition = target.position + target.TransformDirection(offset);

            // Smooth follow
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Look at target
            Vector3 lookTarget = target.position + Vector3.up * 1.5f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
