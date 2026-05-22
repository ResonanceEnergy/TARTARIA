using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Ambient Audio Zone — plays looping ambient SFX when player enters trigger volume.
    /// Auto-fades in/out on enter/exit. Attach to trigger collider GameObject.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AmbientAudioZone : MonoBehaviour
    {
        [Header("Audio Config")]
        [SerializeField] string audioClipName;  // Lookup by name in AudioManager
        [SerializeField, Range(0f, 1f)] float volume = 0.3f;
        [SerializeField] float fadeInDuration = 2f;
        [SerializeField] float fadeOutDuration = 1.5f;

        AudioSource _activeSource;
        bool _playerInZone;
        float _fadeTimer;
        float _targetVolume;

        void Awake()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInZone = true;
                StartAmbience();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInZone = false;
                StopAmbience();
            }
        }

        void Update()
        {
            if (_activeSource == null) return;

            // Fade in/out
            if (_fadeTimer > 0f)
            {
                _fadeTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(_fadeTimer / (_playerInZone ? fadeInDuration : fadeOutDuration));
                _activeSource.volume = Mathf.Lerp(0f, _targetVolume, t);

                if (_fadeTimer <= 0f && !_playerInZone)
                {
                    // Fade out complete, stop and destroy
                    _activeSource.Stop();
                    Destroy(_activeSource.gameObject);
                    _activeSource = null;
                }
            }
        }

        void StartAmbience()
        {
            if (_activeSource != null) return;  // Already playing

            if (string.IsNullOrEmpty(audioClipName))
            {
                Debug.LogWarning($"[AmbientAudioZone] No audio clip assigned on {gameObject.name}");
                return;
            }

            // Play looping SFX at zone center
            _activeSource = AudioManager.Instance?.PlayLoopingSFX(audioClipName, transform.position, volume);

            if (_activeSource != null)
            {
                _activeSource.volume = 0f;  // Start silent
                _targetVolume = volume;
                _fadeTimer = fadeInDuration;
                Debug.Log($"[AmbientAudioZone] Started {audioClipName} at {transform.position}");
            }
        }

        void StopAmbience()
        {
            if (_activeSource == null) return;

            // Start fade out
            _fadeTimer = fadeOutDuration;
            Debug.Log($"[AmbientAudioZone] Stopping {audioClipName}");
        }

        void OnDrawGizmos()
        {
            // Draw zone bounds
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = new Color(0.3f, 0.8f, 0.9f, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;

                if (collider is BoxCollider box)
                {
                    Gizmos.DrawCube(box.center, box.size);
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                }
            }
        }
    }
}
