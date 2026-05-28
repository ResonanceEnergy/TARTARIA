using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 5 Player Setup — The Frostbound Citadel
    /// Spawn at frozen gates with cold resistance and slower movement
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon5PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -80f); // Outside frozen gates
        [SerializeField] float walkSpeed = 3.5f; // Slowed by ice
        [SerializeField] float runSpeed = 6.5f;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            var existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                Debug.Log("[Moon5PlayerSetup] Player already exists, repositioning...");
                existingPlayer.transform.position = spawnPosition;
                return;
            }

            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 5 PLAYER SETUP — The Frostbound Citadel");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var player = CreatePlayer();
            SetupCamera(player);

            Debug.Log($"[Moon5PlayerSetup] ✅ Player spawned at {spawnPosition}");
            Debug.Log($"  • Walk: {walkSpeed}m/s | Run: {runSpeed}m/s (slowed by ice)");
            Debug.Log($"  • Camera: 11m distance, 7m height");
            Debug.Log("═══════════════════════════════════════════════════════════════");
        }

        GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = spawnPosition;
            player.transform.localScale = new Vector3(1f, 1f, 1f);

            Destroy(player.GetComponent<Collider>());

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;
            controller.center = Vector3.zero;

            var rb = player.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            return player;
        }

        void SetupCamera(GameObject player)
        {
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("MainCamera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            mainCam.transform.position = spawnPosition + new Vector3(0f, 7f, -11f);
            mainCam.transform.LookAt(player.transform.position + Vector3.up * 1.5f);

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
            }

            follow.target = player.transform;
            follow.distance = 11f;
            follow.height = 7f;
            follow.rotationSpeed = 2.5f; // Slightly slower in cold
        }
    }
}
