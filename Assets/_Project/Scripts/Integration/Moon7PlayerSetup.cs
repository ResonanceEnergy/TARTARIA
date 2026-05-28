using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 7 Player Setup — The Abyssal Depths
    /// Spawn at dive platform with swimming mechanics
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon7PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, -5f, 0f); // Surface platform
        [SerializeField] float walkSpeed = 3f; // Underwater resistance
        [SerializeField] float runSpeed = 5.5f;

        void Start()
        {
            SpawnPlayer();
        }

        void SpawnPlayer()
        {
            var existingPlayer = GameObject.Find("Player");
            if (existingPlayer != null)
            {
                existingPlayer.transform.position = spawnPosition;
                return;
            }

            var player = CreatePlayer();
            SetupCamera(player);

            Debug.Log($"[Moon7PlayerSetup] ✅ Player spawned at dive platform");
        }

        GameObject CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = spawnPosition;
            Destroy(player.GetComponent<Collider>());

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.5f;

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
                mainCam = new GameObject("MainCamera").AddComponent<Camera>();
                mainCam.tag = "MainCamera";
            }

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();

            follow.target = player.transform;
            follow.distance = 9f;
            follow.height = 5f;
        }
    }
}
