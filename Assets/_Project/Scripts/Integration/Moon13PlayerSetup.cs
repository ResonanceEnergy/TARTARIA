using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon 13 Player Setup — The Aether Convergence
    /// FINAL LEVEL — Spawn at spiral base with all mechanics active
    /// </summary>
    [DefaultExecutionOrder(-78)]
    public class Moon13PlayerSetup : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        [SerializeField] Vector3 spawnPosition = new Vector3(20f, 2f, 0f); // Spiral base
        [SerializeField] float walkSpeed = 5f; // Full power
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

            Debug.Log("═══════════════════════════════════════════════════════════════");
            Debug.Log("  ✨ MOON 13: THE AETHER CONVERGENCE — PLAYER SETUP ✨");
            Debug.Log("  FINAL LEVEL — ALL POWERS UNLOCKED");
            Debug.Log("═══════════════════════════════════════════════════════════════");

            var player = CreatePlayer();
            SetupCamera(player);

            Debug.Log($"[Moon13PlayerSetup] ✅ Player spawned at spiral base");
            Debug.Log($"  • Walk: {walkSpeed}m/s | Run: {runSpeed}m/s");
            Debug.Log($"  • Camera: 15m distance, 10m height (epic scale)");
            Debug.Log("═══════════════════════════════════════════════════════════════");
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

            mainCam.farClipPlane = 600f; // Extended for massive scale

            var follow = mainCam.GetComponent<SimpleCameraFollow>();
            if (follow == null) follow = mainCam.gameObject.AddComponent<SimpleCameraFollow>();

            follow.target = player.transform;
            follow.distance = 15f; // Epic cinematic view
            follow.height = 10f;
        }
    }
}
