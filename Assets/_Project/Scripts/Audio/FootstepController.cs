using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Footstep audio controller — triggers footstep sounds based on
    /// CharacterController movement. Supports surface type detection
    /// via raycasts and applies random pitch variation.
    /// Attach to Player prefab (requires CharacterController).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FootstepController : MonoBehaviour
    {
        [Header("Footstep Settings")]
        [SerializeField, Tooltip("Default footstep clip (used when no surface-specific clip is available)")]
        AudioClip defaultFootstepClip;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of footstep sounds")]
        float volume = 0.5f;

        [SerializeField, Tooltip("Min pitch variation (0.9 = 10% lower)")]
        float pitchMin = 0.9f;

        [SerializeField, Tooltip("Max pitch variation (1.1 = 10% higher)")]
        float pitchMax = 1.1f;

        [SerializeField, Min(0.1f), Tooltip("Time between footsteps when walking (seconds)")]
        float walkStepInterval = 0.5f;

        [SerializeField, Min(0.1f), Tooltip("Time between footsteps when running (seconds)")]
        float runStepInterval = 0.3f;

        [SerializeField, Min(0.01f), Tooltip("Minimum horizontal velocity to trigger footsteps (m/s)")]
        float velocityThreshold = 0.5f;

        [Header("Surface Detection")]
        [SerializeField, Tooltip("Max distance to raycast downward for surface detection")]
        float raycastDistance = 1.5f;

        [SerializeField, Tooltip("Layers to raycast against for surface detection")]
        LayerMask groundLayer = ~0;

        [Header("Surface Clips (optional)")]
        [SerializeField] AudioClip grassFootstep;
        [SerializeField] AudioClip stoneFootstep;
        [SerializeField] AudioClip metalFootstep;
        [SerializeField] AudioClip woodFootstep;

        CharacterController _controller;
        AudioSource _footstepSource;
        float _stepTimer;
        bool _isMoving;

        // Cached for zero-alloc checks
        Vector3 _lastPosition;
        float _horizontalSpeed;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();

            // Create dedicated AudioSource for footsteps (non-pooled, persistent)
            _footstepSource = gameObject.AddComponent<AudioSource>();
            _footstepSource.playOnAwake = false;
            _footstepSource.spatialBlend = 0f; // 2D (first-person perspective)
            _footstepSource.volume = volume;

            // Wire to Footsteps mixer group if AudioManager has it
            if (AudioManager.Instance != null && AudioManager.Instance.FootstepsGroup != null)
            {
                _footstepSource.outputAudioMixerGroup = AudioManager.Instance.FootstepsGroup;
            }

            _lastPosition = transform.position;
        }

        void Update()
        {
            // Calculate horizontal movement speed (no alloc)
            Vector3 currentPos = transform.position;
            Vector3 delta = currentPos - _lastPosition;
            delta.y = 0f; // Ignore vertical movement
            _horizontalSpeed = delta.magnitude / Time.deltaTime;
            _lastPosition = currentPos;

            // Check if moving
            _isMoving = _controller.isGrounded && _horizontalSpeed > velocityThreshold;

            if (!_isMoving)
            {
                _stepTimer = 0f;
                return;
            }

            // Advance step timer
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayFootstep();

                // Determine if walking or running (rough heuristic: >4 m/s = running)
                bool isRunning = _horizontalSpeed > 4f;
                _stepTimer = isRunning ? runStepInterval : walkStepInterval;
            }
        }

        void PlayFootstep()
        {
            // Detect surface type via raycast
            AudioClip clip = DetectSurfaceClip();

            if (clip == null)
            {
                // Fallback to default
                clip = defaultFootstepClip;
            }

            if (clip == null)
            {
                // No clips configured — use procedural fallback
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX2D("Footstep", volume);
                }
                return;
            }

            // Play with random pitch variation
            _footstepSource.pitch = Random.Range(pitchMin, pitchMax);
            _footstepSource.PlayOneShot(clip, volume);
        }

        AudioClip DetectSurfaceClip()
        {
            // Raycast downward from character center
            if (!Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                return null; // No ground detected
            }

            // Identify surface by tag or material name (extend as needed)
            string surfaceTag = hit.collider.tag;
            string materialName = hit.collider.sharedMaterial?.name.ToLower() ?? "";

            // Tag-based lookup
            switch (surfaceTag)
            {
                case "Grass": return grassFootstep;
                case "Stone": return stoneFootstep;
                case "Metal": return metalFootstep;
                case "Wood": return woodFootstep;
            }

            // Material name fallback (if tags not set)
            if (materialName.Contains("grass")) return grassFootstep;
            if (materialName.Contains("stone") || materialName.Contains("rock")) return stoneFootstep;
            if (materialName.Contains("metal")) return metalFootstep;
            if (materialName.Contains("wood")) return woodFootstep;

            return null;
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_controller == null) return;

            // Visualize raycast distance
            Gizmos.color = _controller.isGrounded ? Color.green : Color.red;
            Vector3 start = transform.position;
            Gizmos.DrawLine(start, start + Vector3.down * raycastDistance);
        }
#endif
    }
}
