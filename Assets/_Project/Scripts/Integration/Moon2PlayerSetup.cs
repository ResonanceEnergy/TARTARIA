using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 2 Player Setup — Spawns player at cavern entrance
    /// Adjusted for dark underground environment: slower speed, closer camera, flashlight
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon2PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -90f); // Entrance chamber
        [SerializeField] Quaternion spawnRotation = Quaternion.Euler(0f, 0f, 0f); // Facing north

        [Header("Movement Settings (Cavern)")]
        [SerializeField] float walkSpeed = 3.5f; // Slower (dark, uneven terrain)
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float interactionRadius = 3f;

        [Header("Camera Settings")]
        [SerializeField] float cameraDistance = 8f; // Closer (enclosed space)
        [SerializeField] float cameraHeight = 5f;
        [SerializeField] float cameraAngle = 30f;

        [Header("Flashlight")]
        [SerializeField] bool enableFlashlight = true;
        [SerializeField] Color flashlightColor = new Color(1f, 0.95f, 0.85f);
        [SerializeField] float flashlightIntensity = 3f;
        [SerializeField] float flashlightRange = 25f;
        [SerializeField] float flashlightAngle = 45f;

        private GameObject playerInstance;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            Debug.Log("[Moon2PlayerSetup] Spawning player at cavern entrance...");

            var existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                playerInstance = existingPlayer;
                ConfigureExistingPlayer();
                return;
            }

            CreatePlayer();
            Debug.Log($"[Moon2PlayerSetup] ✅ Player spawned at {spawnPosition}");
        }

        void CreatePlayer()
        {
            playerInstance = new GameObject("Player");
            playerInstance.tag = "Player";
            playerInstance.transform.position = spawnPosition;
            playerInstance.transform.rotation = spawnRotation;

            // Visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerInstance.transform);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.6f, 0.9f);
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

            SetupCamera();
            SetupFlashlight();

            // Add interaction trigger
            var trigger = playerInstance.GetComponent<SphereCollider>();
            if (trigger == null)
            {
                trigger = playerInstance.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = interactionRadius;
                trigger.center = new Vector3(0f, 1f, 0f);
            }

            Debug.Log($"  ✓ Movement: walk={walkSpeed}, run={runSpeed}");
        }

        void ConfigureExistingPlayer()
        {
            if (playerInstance != null)
            {
                playerInstance.transform.position = spawnPosition;
                playerInstance.transform.rotation = spawnRotation;
                ConfigurePlayer();
            }
        }

        void SetupCamera()
        {
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<UnityEngine.Camera>();
                camObj.tag = "MainCamera";
            }

            // Position camera closer for enclosed space
            Vector3 cameraOffset = new Vector3(0f, cameraHeight, -cameraDistance);
            mainCam.transform.position = playerInstance.transform.position + cameraOffset;
            mainCam.transform.LookAt(playerInstance.transform.position + Vector3.up * 1.5f);

            // Add simple follow
            var follow = mainCam.GetComponent<Moon1PlayerSetup.SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<Moon1PlayerSetup.SimpleCameraFollow>();
            }
            follow.target = playerInstance.transform;
            follow.distance = cameraDistance;
            follow.height = cameraHeight;
            follow.smoothSpeed = 7f; // Faster follow for tight spaces

            Debug.Log($"  ✓ Camera: distance={cameraDistance}m, height={cameraHeight}m");
        }

        void SetupFlashlight()
        {
            if (!enableFlashlight) return;

            var flashlightObj = new GameObject("Flashlight");
            flashlightObj.transform.SetParent(playerInstance.transform);
            flashlightObj.transform.localPosition = new Vector3(0f, 1.5f, 0.3f); // Head height, slightly forward
            flashlightObj.transform.localRotation = Quaternion.Euler(10f, 0f, 0f); // Slight downward angle

            var light = flashlightObj.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = flashlightColor;
            light.intensity = flashlightIntensity;
            light.range = flashlightRange;
            light.spotAngle = flashlightAngle;
            light.innerSpotAngle = flashlightAngle * 0.6f;
            light.shadows = LightShadows.Soft;

            Debug.Log($"  ✓ Flashlight: {flashlightIntensity} intensity, {flashlightRange}m range, {flashlightAngle}° angle");
        }
    }
}
