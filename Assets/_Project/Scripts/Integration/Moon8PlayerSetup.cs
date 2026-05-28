using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 8 Player Setup — The Celestial Spires
    /// Spawn on central spire with aerial navigation
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon8PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 102f, 0f); // Central spire top
        [SerializeField] float walkSpeed = 5f; // Lighter gravity
        [SerializeField] float runSpeed = 9f;

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

            Debug.Log($"[Moon8PlayerSetup] ✅ Player spawned on celestial spire");
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

            mainCam.farClipPlane = 500f; // Extended for sky view

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();

            follow.target = player.transform;
            follow.distance = 14f; // Wide aerial view
            follow.height = 10f;
        }
    }
}
