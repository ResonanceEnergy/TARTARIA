using UnityEngine;
using Tartaria.Core;
using Tartaria.Audio;

namespace Tartaria.Integration
{
    /// <summary>
    /// AudioFeedbackController — Wires audio to all interactions.
    /// </summary>
    public class AudioFeedbackController : MonoBehaviour
    {
        public static AudioFeedbackController Instance { get; private set; }

        [Header("Audio Clips")]
        [SerializeField] private AudioClip footstepClip;
        [SerializeField] private AudioClip scanClip;
        [SerializeField] private AudioClip restoreClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip pickupClip;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // Subscribe to game events
            GameEvents.OnBuildingRestored += OnBuildingRestored;
            GameEvents.OnEnemyKilled += OnEnemyKilled;
            GameEvents.OnInventoryChanged += OnInventoryChanged;

            Debug.Log("[AudioFeedbackController] ✅ Audio feedback wired");
        }

        void OnDestroy()
        {
            GameEvents.OnBuildingRestored -= OnBuildingRestored;
            GameEvents.OnEnemyKilled -= OnEnemyKilled;
            GameEvents.OnInventoryChanged -= OnInventoryChanged;
        }

        void OnBuildingRestored(string buildingId)
        {
            PlaySFX(restoreClip, "Restore");
        }

        void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            PlaySFX(hitClip, "Hit");
        }

        void OnInventoryChanged()
        {
            PlaySFX(pickupClip, "Pickup");
        }

        public void PlayFootstep(Vector3 position)
        {
            PlaySFX(footstepClip, "Footstep", position);
        }

        public void PlayScan(Vector3 position)
        {
            PlaySFX(scanClip, "Scan", position);
        }

        public void PlayRestore(Vector3 position)
        {
            PlaySFX(restoreClip, "Restore", position);
        }

        public void PlayHit(Vector3 position)
        {
            PlaySFX(hitClip, "Hit", position);
        }

        public void PlayPickup(Vector3 position)
        {
            PlaySFX(pickupClip, "Pickup", position);
        }

        /// <summary>
        /// Play SFX by name (convenience method for Integration layer).
        /// </summary>
        public void PlaySFX(string sfxName, Vector3 position)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX3D(sfxName, position);
            }
        }

        public void PlaySFX(AudioClip clip, string fallbackName, Vector3? position = null)
        {
            if (AudioManager.Instance != null)
            {
                if (position.HasValue)
                    AudioManager.Instance.PlaySFX3D(fallbackName, position.Value);
                else
                    AudioManager.Instance.PlaySFX2D(fallbackName);
            }
            else if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position ?? UnityEngine.Camera.main.transform.position);
            }
        }
    }
}
