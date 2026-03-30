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
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX Pools")]
        [SerializeField] int sfxPoolSize = 16;

        AudioSource[] _sfxPool;
        int _sfxPoolIndex;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
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
        public AudioSource PlayTone(float frequencyHz, float volume = 0.3f)
        {
            var go = new GameObject("ToneGenerator");
            go.transform.SetParent(transform);

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = volume;
            source.spatialBlend = 0f;

            // Generate sine wave clip at specified frequency
            int sampleRate = AudioSettings.outputSampleRate;
            int samples = sampleRate; // 1 second of audio
            var audioClip = AudioClip.Create("Tone", samples, 1, sampleRate, false);

            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencyHz * i / sampleRate);
            }
            audioClip.SetData(data, 0);

            source.clip = audioClip;
            source.Play();

            return source;
        }

        /// <summary>
        /// Plays a named voice line at the given volume (stub — clips loaded from Resources/VoiceLines).
        /// </summary>
        public void PlayVoiceLine(string lineId, float volume = 1f)
        {
            var clip = Resources.Load<AudioClip>($"VoiceLines/{lineId}");
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
