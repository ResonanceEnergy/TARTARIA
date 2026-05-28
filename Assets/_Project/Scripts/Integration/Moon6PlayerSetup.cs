using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 6 Player Setup — The Molten Forge
    /// Spawn at forge entrance with heat shielding
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon6PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -60f);
        [SerializeField] float walkSpeed = 4f;
        [SerializeField] float runSpeed = 7.5f;

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

            Debug.Log($"[Moon6PlayerSetup] ✅ Player spawned at forge entrance");
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
            var mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = new GameObject("MainCamera").AddComponent<Camera>();
                mainCam.tag = "MainCamera";
            }

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();

            follow.target = player.transform;
            follow.distance = 10f;
            follow.height = 6f;
        }
    }
}
