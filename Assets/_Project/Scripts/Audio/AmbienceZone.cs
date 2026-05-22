using UnityEngine;
using System.Collections;

namespace Tartaria.Audio
{
    /// <summary>
    /// Ambient audio zone — plays looping ambience when player enters,
    /// crossfades to new zone ambience on transition. 
    /// Attach to building/area trigger colliders in Echohaven.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AmbienceZone : MonoBehaviour
    {
        [Header("Ambience Settings")]
        [SerializeField, Tooltip("Looping ambience clip for this zone (e.g. building hum, outdoor wind)")]
        AudioClip ambienceClip;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of ambience playback")]
        float volume = 0.3f;

        [SerializeField, Min(0f), Tooltip("Crossfade duration when transitioning between zones (seconds)")]
        float crossfadeDuration = 2f;

        [SerializeField, Tooltip("Layer mask for entities that trigger zone entry (typically Player layer)")]
        LayerMask triggerMask = ~0;

        static AudioSource _currentAmbienceSource;
        static Coroutine _fadeCoroutine;
        static AmbienceZone _activeZone;

        AudioSource _zoneSource;

        void Awake()
        {
            // Ensure trigger is set (non-convex mesh colliders need to be triggers)
            var col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                Debug.LogWarning($"[AmbienceZone] {gameObject.name} collider is not a trigger — setting isTrigger=true", this);
                col.isTrigger = true;
            }

            // Create dedicated AudioSource for this zone
            _zoneSource = gameObject.AddComponent<AudioSource>();
            _zoneSource.playOnAwake = false;
            _zoneSource.loop = true;
            _zoneSource.spatialBlend = 0f; // 2D ambience (non-spatial)
            _zoneSource.clip = ambienceClip;
            _zoneSource.volume = 0f; // Start silent

            // Wire to Ambience mixer group if AudioManager has it
            if (AudioManager.Instance != null && AudioManager.Instance.AmbienceGroup != null)
            {
                _zoneSource.outputAudioMixerGroup = AudioManager.Instance.AmbienceGroup;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Check if triggering entity is on the correct layer
            if (((1 << other.gameObject.layer) & triggerMask) == 0)
                return;

            // If no zone is active, fade in immediately
            if (_activeZone == null)
            {
                EnterZone();
                return;
            }

            // If already in this zone, ignore
            if (_activeZone == this)
                return;

            // Crossfade from current zone to this one
            CrossfadeToZone(this);
        }

        void OnTriggerExit(Collider other)
        {
            // Check layer mask
            if (((1 << other.gameObject.layer) & triggerMask) == 0)
                return;

            // If this is the active zone, fade out
            if (_activeZone == this)
            {
                ExitZone();
            }
        }

        void EnterZone()
        {
            if (ambienceClip == null) return;

            _activeZone = this;
            _currentAmbienceSource = _zoneSource;

            // Stop any active fade coroutine
            if (_fadeCoroutine != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopCoroutine(_fadeCoroutine);
            }

            // Fade in
            if (AudioManager.Instance != null)
            {
                _fadeCoroutine = AudioManager.Instance.StartCoroutine(FadeInSource(_zoneSource, volume, crossfadeDuration));
            }
            else
            {
                // Fallback: instant fade-in if AudioManager is missing
                _zoneSource.volume = volume;
                _zoneSource.Play();
            }
        }

        void ExitZone()
        {
            if (_currentAmbienceSource != _zoneSource) return;

            // Stop any active fade
            if (_fadeCoroutine != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopCoroutine(_fadeCoroutine);
            }

            // Fade out
            if (AudioManager.Instance != null)
            {
                _fadeCoroutine = AudioManager.Instance.StartCoroutine(FadeOutSource(_zoneSource, crossfadeDuration));
            }
            else
            {
                _zoneSource.Stop();
                _zoneSource.volume = 0f;
            }

            _activeZone = null;
            _currentAmbienceSource = null;
        }

        static void CrossfadeToZone(AmbienceZone newZone)
        {
            if (newZone.ambienceClip == null) return;

            var oldSource = _currentAmbienceSource;
            var newSource = newZone._zoneSource;

            _activeZone = newZone;
            _currentAmbienceSource = newSource;

            // Stop any active fade
            if (_fadeCoroutine != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopCoroutine(_fadeCoroutine);
            }

            // Start crossfade coroutine
            if (AudioManager.Instance != null)
            {
                _fadeCoroutine = AudioManager.Instance.StartCoroutine(
                    CrossfadeSources(oldSource, newSource, newZone.volume, newZone.crossfadeDuration)
                );
            }
            else
            {
                // Fallback: instant switch
                if (oldSource != null)
                {
                    oldSource.Stop();
                    oldSource.volume = 0f;
                }
                newSource.volume = newZone.volume;
                newSource.Play();
            }
        }

        static IEnumerator FadeInSource(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;

            source.volume = 0f;
            source.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
                yield return null;
            }

            source.volume = targetVolume;
        }

        static IEnumerator FadeOutSource(AudioSource source, float duration)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
        }

        static IEnumerator CrossfadeSources(AudioSource oldSource, AudioSource newSource, float targetVolume, float duration)
        {
            if (newSource == null) yield break;

            float oldStartVolume = oldSource != null ? oldSource.volume : 0f;
            newSource.volume = 0f;
            newSource.Play();

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (oldSource != null)
                    oldSource.volume = Mathf.Lerp(oldStartVolume, 0f, t);

                newSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            // Finalize
            if (oldSource != null)
            {
                oldSource.Stop();
                oldSource.volume = 0f;
            }
            newSource.volume = targetVolume;
        }

        void OnDestroy()
        {
            // Clean up if this was the active zone
            if (_activeZone == this)
            {
                _activeZone = null;
                _currentAmbienceSource = null;
            }

            if (_fadeCoroutine != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                
                if (col is BoxCollider box)
                    Gizmos.DrawCube(box.center, box.size);
                else if (col is SphereCollider sphere)
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
            }
        }
#endif
    }
}
