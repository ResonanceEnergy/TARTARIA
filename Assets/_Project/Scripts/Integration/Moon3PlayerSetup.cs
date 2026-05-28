using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 3 Player Setup — The Verdant Labyrinth
    /// Spawn player at jungle entrance with machete and pathfinding
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon3PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -60f); // Outside maze entrance
        [SerializeField] float walkSpeed = 4.5f; // Slower in dense jungle
        [SerializeField] float runSpeed = 8f;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            var existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                Debug.Log("[Moon3PlayerSetup] Player already exists, repositioning...");
                existingPlayer.transform.position = spawnPosition;
                return;
            }

            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 3 PLAYER SETUP — The Verdant Labyrinth");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var player = CreatePlayer();
            SetupCamera(player);

            Debug.Log($"[Moon3PlayerSetup] ✅ Player spawned at {spawnPosition}");
            Debug.Log($"  • Walk: {walkSpeed}m/s | Run: {runSpeed}m/s");
            Debug.Log($"  • Camera: 10m distance, 6m height");
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

            mainCam.transform.position = spawnPosition + new Vector3(0f, 6f, -10f);
            mainCam.transform.LookAt(player.transform.position + Vector3.up * 1.5f);

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
            }

            follow.target = player.transform;
            follow.distance = 10f;
            follow.height = 6f;
            follow.rotationSpeed = 3f;
        }
    }
}
