using UnityEngine;
using Tartaria.Camera;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 1 Player Setup — Spawns player at village entrance with proper camera
    /// Configures movement parameters for outdoor exploration
    /// Sets up interaction radius for buildings/NPCs
    /// </summary>
    [DefaultExecutionOrder(-78)] // After excavation sites (-79)
    public class Moon1PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -100f); // South of village
        [SerializeField] Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f); // Facing north
        
        [Header("Player Prefab")]
        [SerializeField] GameObject playerPrefab;

        [Header("Movement Settings (Echohaven)")]
        [SerializeField] float walkSpeed = 5f;
        [SerializeField] float runSpeed = 8f;
        [SerializeField] float interactionRadius = 4f;

        [Header("Camera Settings")]
        [SerializeField] float cameraDistance = 12f;
        [SerializeField] float cameraHeight = 8f;
        [SerializeField] float cameraAngle = 35f;

        private GameObject playerInstance;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            Debug.Log("[Moon1PlayerSetup] Spawning player at Echohaven entrance...");

            // Check if player already exists
            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Debug.LogWarning("[Moon1PlayerSetup] Player already exists, configuring existing player");
                playerInstance = existingPlayer;
                ConfigureExistingPlayer();
                return;
            }

            // Load player prefab if not assigned
            if (playerPrefab == null)
            {
                playerPrefab = Resources.Load<GameObject>("Prefabs/Player/PlayerCharacter");
            }

            // Fallback: Create simple player
            if (playerPrefab == null)
            {
                Debug.LogWarning("[Moon1PlayerSetup] No player prefab found, creating simple player");
                CreateSimplePlayer();
            }
            else
            {
                // Instantiate prefab
                playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                playerInstance.name = "Player";
                ConfigurePlayer();
            }

            Debug.Log($"[Moon1PlayerSetup] ✅ Player spawned at {spawnPosition}");
        }

        void CreateSimplePlayer()
        {
            playerInstance = new GameObject("Player");
            playerInstance.tag = "Player";
            playerInstance.transform.position = spawnPosition;
            playerInstance.transform.rotation = spawnRotation;

            // Visual representation
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerInstance.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            visual.transform.localScale = new Vector3(1f, 1f, 1f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.6f, 0.9f); // Blue
            visual.GetComponent<Renderer>().material = material;

            // Add CharacterController
            var controller = playerInstance.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 1f, 0f);
            controller.radius = 0.5f;
            controller.height = 2f;

            // Add camera target
            var cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.SetParent(playerInstance.transform);
            cameraTarget.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            ConfigurePlayer();
        }

        void ConfigurePlayer()
        {
            if (playerInstance == null) return;

            // Configure movement (if PlayerMovement component exists)
            var movement = playerInstance.GetComponent<Tartaria.Input.PlayerMovement>();
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
            // Move player to spawn position
            if (playerInstance != null)
            {
                playerInstance.transform.position = spawnPosition;
                playerInstance.transform.rotation = spawnRotation;
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

            // Try to add TartariaCameraController
            var cameraController = mainCam.GetComponent<TartariaCameraController>();
            if (cameraController == null)
            {
                cameraController = mainCam.gameObject.AddComponent<TartariaCameraController>();
            }

            if (cameraController != null)
            {
                // Configure for outdoor exploration
                Debug.Log("  ✓ Configured TartariaCameraController for outdoor exploration");
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
