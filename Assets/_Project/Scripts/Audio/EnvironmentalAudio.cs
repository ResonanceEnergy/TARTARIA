using UnityEngine;
using System.Collections;

namespace Tartaria.Audio
{
    /// <summary>
    /// Environmental audio spawner — plays one-shot 3D spatial sounds
    /// at random intervals (wind gusts, distant echoes, ambient life).
    /// Place in scene as standalone GameObjects (e.g. 5-8 sources around Echohaven).
    /// Uses object pooling via AudioManager to avoid allocations.
    /// </summary>
    public class EnvironmentalAudio : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField, Tooltip("Pool of clips to randomly play")]
        AudioClip[] audioClips;

        [Header("Playback Settings")]
        [SerializeField, Range(0f, 1f), Tooltip("Volume of audio playback")]
        float volume = 0.4f;

        [SerializeField, Min(0f), Tooltip("Minimum interval between plays (seconds)")]
        float intervalMin = 10f;

        [SerializeField, Min(0f), Tooltip("Maximum interval between plays (seconds)")]
        float intervalMax = 30f;

        [SerializeField, Tooltip("Min pitch variation (e.g. 0.95 = 5% lower)")]
        float pitchMin = 0.95f;

        [SerializeField, Tooltip("Max pitch variation (e.g. 1.05 = 5% higher)")]
        float pitchMax = 1.05f;

        [Header("Spatial Settings")]
        [SerializeField, Range(0f, 1f), Tooltip("0 = 2D, 1 = 3D spatial audio")]
        float spatialBlend = 1f;

        [SerializeField, Min(1f), Tooltip("Max distance for 3D audio attenuation (meters)")]
        float maxDistance = 50f;

        [SerializeField, Tooltip("Play audio on Start (after first random delay)")]
        bool playOnStart = true;

        Coroutine _playbackCoroutine;
        AudioSource _dedicatedSource; // Cached source for zero-alloc playback

        void Awake()
        {
            // Create a dedicated AudioSource for this spawner (avoids pooling overhead)
            _dedicatedSource = gameObject.AddComponent<AudioSource>();
            _dedicatedSource.playOnAwake = false;
            _dedicatedSource.loop = false;
            _dedicatedSource.spatialBlend = spatialBlend;
            _dedicatedSource.maxDistance = maxDistance;
            _dedicatedSource.volume = volume;

            // Wire to Ambience mixer group if AudioManager has it
            if (AudioManager.Instance != null && AudioManager.Instance.AmbienceGroup != null)
            {
                _dedicatedSource.outputAudioMixerGroup = AudioManager.Instance.AmbienceGroup;
            }
            else if (AudioManager.Instance != null && AudioManager.Instance.SfxGroup != null)
            {
                // Fallback to SFX group
                _dedicatedSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;
            }
        }

        void Start()
        {
            if (playOnStart && audioClips != null && audioClips.Length > 0)
            {
                _playbackCoroutine = StartCoroutine(PlaybackLoop());
            }
        }

        void OnDestroy()
        {
            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }
        }

        IEnumerator PlaybackLoop()
        {
            while (true)
            {
                // Wait for random interval
                float delay = Random.Range(intervalMin, intervalMax);
                yield return new WaitForSeconds(delay);

                // Play a random clip
                PlayRandomClip();
            }
        }

        void PlayRandomClip()
        {
            if (audioClips == null || audioClips.Length == 0)
                return;

            // Pick random clip
            AudioClip clip = audioClips[Random.Range(0, audioClips.Length)];
            if (clip == null) return;

            // Apply random pitch variation
            _dedicatedSource.pitch = Random.Range(pitchMin, pitchMax);

            // Play one-shot (non-blocking)
            _dedicatedSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Manually trigger a one-shot play (for scripted events).
        /// </summary>
        public void PlayOnce()
        {
            PlayRandomClip();
        }

        /// <summary>
        /// Start the random playback loop (if not already running).
        /// </summary>
        public void StartPlayback()
        {
            if (_playbackCoroutine == null)
            {
                _playbackCoroutine = StartCoroutine(PlaybackLoop());
            }
        }

        /// <summary>
        /// Stop the random playback loop.
        /// </summary>
        public void StopPlayback()
        {
            if (_playbackCoroutine != null)
            {
                StopCoroutine(_playbackCoroutine);
                _playbackCoroutine = null;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            // Visualize max distance sphere for 3D audio
            if (spatialBlend > 0.5f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, maxDistance);
            }

            // Draw icon at position
            Gizmos.color = Color.yellow;
            Gizmos.DrawIcon(transform.position, "AudioSource Icon", true);
        }

        void OnDrawGizmosSelected()
        {
            // Highlight when selected
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.5f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
#endif
    }
}
