using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 4 Player Setup — The Sunscorched Oasis
    /// Spawn at oasis edge with heat resistance mechanics
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon4PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -70f); // Desert approach
        [SerializeField] float walkSpeed = 4f; // Slower in heat
        [SerializeField] float runSpeed = 7f;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            var existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                Debug.Log("[Moon4PlayerSetup] Player already exists, repositioning...");
                existingPlayer.transform.position = spawnPosition;
                return;
            }

            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  🌙 MOON 4 PLAYER SETUP — The Sunscorched Oasis");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var player = CreatePlayer();
            SetupCamera(player);

            Debug.Log($"[Moon4PlayerSetup] ✅ Player spawned at {spawnPosition}");
            Debug.Log($"  • Walk: {walkSpeed}m/s | Run: {runSpeed}m/s");
            Debug.Log($"  • Camera: 12m distance, 8m height (wide desert view)");
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
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                var camObj = new GameObject("MainCamera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            mainCam.transform.position = spawnPosition + new Vector3(0f, 8f, -12f);
            mainCam.transform.LookAt(player.transform.position + Vector3.up * 1.5f);

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null)
            {
                follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();
            }

            follow.target = player.transform;
            follow.distance = 12f; // Wider view for open desert
            follow.height = 8f;
            follow.rotationSpeed = 3f;
        }
    }
}
