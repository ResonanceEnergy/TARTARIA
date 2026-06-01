using UnityEngine;

namespace Tartaria.Integration
{
    /// <summary>
    /// Moon1CameraFollowPlayer — attaches to Main Camera at runtime and smoothly follows
    /// any GameObject tagged "Player" with a 3rd-person offset.
    ///
    /// Auto-bootstraps via RuntimeInitializeOnLoadMethod — no Editor menu needed.
    /// If Main Camera doesn't exist yet, polls for it. If Player doesn't exist yet
    /// (PlayerSpawner spawns at runtime), polls for it. Once both found, follows.
    ///
    /// 3rd-person over-the-shoulder default offset: behind & above the player,
    /// looking slightly down. Match the values to your gameplay feel.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(50)] // run after most gameplay scripts
    public class Moon1CameraFollowPlayer : MonoBehaviour
    {
        [Header("Follow offset (camera position relative to player)")]
        public Vector3 offset = new Vector3(0f, 6f, -10f);

        [Header("Look-at offset (point above player to focus on)")]
        public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);

        [Header("Smoothing")]
        [Range(0f, 1f)] public float positionLerp = 0.15f;
        [Range(0f, 1f)] public float rotationLerp = 0.20f;

        Transform _target;
        float _scanTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // Only run in Echohaven scene
            var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Echohaven_VerticalSlice") return;

            // Find Main Camera (may not exist yet on first Bootstrap; we'll retry on first Update)
            var cam = UnityEngine.Camera.main;
            if (cam == null) return;

            if (cam.GetComponent<Moon1CameraFollowPlayer>() == null)
            {
                cam.gameObject.AddComponent<Moon1CameraFollowPlayer>();
                Debug.Log("[Moon1CameraFollowPlayer] Bootstrapped on Main Camera");
            }
        }

        void Update()
        {
            // Lazy-find the player (PlayerSpawner may spawn after frame 0)
            if (_target == null)
            {
                _scanTimer -= Time.deltaTime;
                if (_scanTimer <= 0f)
                {
                    _scanTimer = 0.25f;
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null) _target = player.transform;
                }
                if (_target == null) return;
            }

            // Smooth follow
            Vector3 desiredPos = _target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, positionLerp);

            Quaternion desiredRot = Quaternion.LookRotation(
                (_target.position + lookOffset) - transform.position,
                Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp);
        }
    }
}
