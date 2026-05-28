using UnityEngine;

namespace Tartaria.Integration
    #pragma warning disable CS0414, CS0219 // Placeholder fields/vars for planned features
{
    /// <summary>
    /// Moon 12 Player Setup — The Umbral Sanctum
    /// Spawn at shadow gate with light/dark balance mechanics
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon12PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(0f, 2f, -85f);
        [SerializeField] float walkSpeed = 4.3f;
        [SerializeField] float runSpeed = 7.8f;

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
            Debug.Log($"[Moon12PlayerSetup] ✅ Player spawned at umbral sanctum");
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
                mainCam = new GameObject("MainCamera").AddComponent<UnityEngine.Camera>();
                mainCam.tag = "MainCamera";
            }

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();

            follow.target = player.transform;
            follow.distance = 11f;
            follow.height = 7f;
        }
    }
}
