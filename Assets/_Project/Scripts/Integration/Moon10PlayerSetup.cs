using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 10 Player Setup — The Temporal Rift
    /// Spawn at time vortex with temporal distortion effects
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon10PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -80f);
        [SerializeField] float walkSpeed = 4.5f;
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
                existingPlayer.transform.position = spawnPosition;
                return;
            }

            var player = CreatePlayer();
            SetupCamera(player);
            Debug.Log($"[Moon10PlayerSetup] ✅ Player spawned at temporal rift");
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
            follow.distance = 12f;
            follow.height = 8f;
        }
    }
}
