using UnityEngine;

namespace Tartaria.Audio
{
    /// <summary>
    /// Central Audio Manager — handles spatial audio, SFX playback,
    /// and coordinates between adaptive music and gameplay events.
    ///
    /// All musical content uses A4 = 432 Hz tuning.
    /// Key frequencies: 7.83 Hz (Telluric), 432 Hz (Harmonic),
    ///                  528 Hz (Healing), 1296 Hz (Celestial)
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX Pools")]
        [SerializeField, Min(1)] int sfxPoolSize = 16;
        [SerializeField, Min(1)] int tonePoolSize = 4;

        AudioSource[] _sfxPool;
        int _sfxPoolIndex;
        AudioSource[] _tonePool;
        int _tonePoolIndex;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
            InitializeTonePool();
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (Instance == this) Instance = null;
        }

        void InitializeSFXPool()
        {
            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_Pool_{i}");
                go.transform.SetParent(transform);
                _sfxPool[i] = go.AddComponent<AudioSource>();
                _sfxPool[i].playOnAwake = false;
                _sfxPool[i].spatialBlend = 1.0f; // 3D
            }
        }

        void InitializeTonePool()
        {
            _tonePool = new AudioSource[tonePoolSize];
            for (int i = 0; i < tonePoolSize; i++)
            {
                var go = new GameObject($"Tone_Pool_{i}");
                go.transform.SetParent(transform);
                _tonePool[i] = go.AddComponent<AudioSource>();
                _tonePool[i].playOnAwake = false;
                _tonePool[i].loop = true;
                _tonePool[i].spatialBlend = 0f;
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Plays a one-shot SFX at a world position with spatial audio.
        /// </summary>
        public void PlaySFX(AudioClip clip, Vector3 position, float volume = 1.0f)
        {
            if (clip == null) return;

            var source = GetNextPooledSource();
            source.transform.position = position;
            source.spatialBlend = 1.0f;
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a non-spatial UI/feedback SFX.
        /// </summary>
        public void PlaySFX2D(AudioClip clip, float volume = 1.0f)
        {
            if (clip == null) return;

            var source = GetNextPooledSource();
            source.spatialBlend = 0f;
            source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Generates a pure sine wave at the given frequency.
        /// Used for tuning mini-game real-time audio feedback.
        /// </summary>
        public AudioSource PlayTone(float frequencyHz, float duration = 0f, float volume = 0.3f)
        {
            var source = _tonePool[_tonePoolIndex];
            _tonePoolIndex = (_tonePoolIndex + 1) % _tonePool.Length;

            // Stop previous tone on this slot
            source.Stop();

            // Destroy previous clip to avoid native memory leak
            if (source.clip != null) { Destroy(source.clip); source.clip = null; }

            source.volume = volume;

            // Generate sine wave clip at specified frequency
            int sampleRate = AudioSettings.outputSampleRate;
            bool hasFiniteDuration = duration > 0f;
            int samples = hasFiniteDuration
                ? Mathf.CeilToInt(sampleRate * duration)
                : sampleRate; // 1 second loop
            var audioClip = AudioClip.Create("Tone", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencyHz * i / sampleRate);
            }
            audioClip.SetData(data, 0);

            source.clip = audioClip;
            source.loop = !hasFiniteDuration;
            source.Play();

            if (hasFiniteDuration)
                StartCoroutine(StopToneAfter(source, duration));

            return source;
        }

        System.Collections.IEnumerator StopToneAfter(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (source != null && source.isPlaying)
            {
                source.Stop();
                if (source.clip != null) { Destroy(source.clip); source.clip = null; }
            }
        }

        /// <summary>
        /// Plays a named voice line at the given volume. Clips loaded from Resources/VoiceLines.
        /// </summary>
        readonly System.Collections.Generic.Dictionary<string, AudioClip> _voiceCache
            = new System.Collections.Generic.Dictionary<string, AudioClip>();

        public void PlayVoiceLine(string lineId, float volume = 1f)
        {
            if (!_voiceCache.TryGetValue(lineId, out var clip))
            {
                clip = Resources.Load<AudioClip>($"VoiceLines/{lineId}");
                if (clip != null) _voiceCache[lineId] = clip;
            }
            if (clip != null)
                PlaySFX2D(clip, volume);
        }

        AudioSource GetNextPooledSource()
        {
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;
            return source;
        }
    }
}
