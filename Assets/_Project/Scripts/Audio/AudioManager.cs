using UnityEngine;
using UnityEngine.Audio;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Central Audio Manager — handles spatial audio, SFX playback,
    /// and coordinates between adaptive music and gameplay events.
    ///
    /// All musical content uses A4 = 432 Hz tuning.
    /// Key frequencies: 7.83 Hz (Telluric), 432 Hz (Harmonic),
    ///                  528 Hz (Healing), 1296 Hz (Celestial)
    ///
    /// Mixer-aware: pool sources can be routed to AudioMixerGroups so player
    /// volume sliders work via Mixer.SetFloat. Cue library lookup via Play(cueId).
    /// </summary>
    [DisallowMultipleComponent]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Mixer (optional)")]
        [SerializeField, Tooltip("Master mixer — used to route SFX/Music/UI groups.")]
        AudioMixer mixer;
        [SerializeField] AudioMixerGroup musicGroup;
        [SerializeField] AudioMixerGroup sfxGroup;
        [SerializeField] AudioMixerGroup uiGroup;

        [Header("Mixer Snapshots (optional)")]
        [SerializeField, Tooltip("Default snapshot — applied for Exploration / Tuning / Menu / Cinematic.")]
        AudioMixerSnapshot explorationSnapshot;
        [SerializeField, Tooltip("Combat snapshot — typically ducks music, boosts SFX.")]
        AudioMixerSnapshot combatSnapshot;
        [SerializeField, Min(0f), Tooltip("Crossfade duration (seconds) between Exploration and Combat snapshots.")]
        float snapshotTransitionSeconds = 1.0f;

        [SerializeField, Tooltip("If true, snapshots auto-transition based on GameState (Combat -> Combat snapshot, else Exploration).")]
        bool autoTransitionWithGameState = true;

        AudioMixerSnapshot _activeSnapshot;

        [Header("Cue Library (optional)")]
        [SerializeField] AudioCueLibrary cueLibrary;

        [Header("Music")]
        [SerializeField, Tooltip("432 Hz ambient music for Exploration state")]
        AudioClip explorationMusic;
        [SerializeField, Range(0f, 1f), Tooltip("Music volume")]
        float musicVolume = 0.3f;

        [Header("SFX Pools")]
        [SerializeField, Min(1)] int sfxPoolSize = 16;
        [SerializeField, Min(1)] int tonePoolSize = 4;

        AudioSource[] _sfxPool;
        int _sfxPoolIndex;
        AudioSource[] _tonePool;
        int _tonePoolIndex;
        AudioSource _musicSource;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
            InitializeTonePool();
            InitializeMusicSource();
            ProceduralSFXLibrary.Initialize();
        }

        void Start()
        {
            // Subscribe to game state changes for music control
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
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
                if (sfxGroup != null) _sfxPool[i].outputAudioMixerGroup = sfxGroup;
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
                if (sfxGroup != null) _tonePool[i].outputAudioMixerGroup = sfxGroup;
            }
        }

        void InitializeMusicSource()
        {
            var go = new GameObject("Music_Source");
            go.transform.SetParent(transform);
            _musicSource = go.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;
            _musicSource.volume = musicVolume;
            if (musicGroup != null) _musicSource.outputAudioMixerGroup = musicGroup;
        }

        void OnGameStateChanged(GameState oldState, GameState newState)
        {
            // Play 432 Hz music when entering Exploration state
            if (newState == GameState.Exploration && explorationMusic != null && !_musicSource.isPlaying)
            {
                _musicSource.clip = explorationMusic;
                _musicSource.Play();
                Debug.Log($"[AudioManager] Playing 432 Hz exploration music: {explorationMusic.name}");
            }

            // Auto-transition mixer snapshots: Combat -> combatSnapshot, everything else -> explorationSnapshot.
            if (autoTransitionWithGameState)
            {
                SetCombatMode(newState == GameState.Combat);
            }
        }

        // ─── Mixer Snapshot API ──────────────────────

        /// <summary>
        /// Transitions to the Combat snapshot when <paramref name="combat"/> is true,
        /// otherwise transitions back to the Exploration snapshot. Crossfade time is
        /// controlled by <c>snapshotTransitionSeconds</c>. No-ops if snapshots aren't wired.
        /// </summary>
        public void SetCombatMode(bool combat)
        {
            var target = combat ? combatSnapshot : explorationSnapshot;
            TransitionToSnapshot(target);
        }

        /// <summary>
        /// Transitions to the named snapshot on the configured mixer.
        /// Falls back silently if mixer/snapshot is missing.
        /// </summary>
        public void TransitionToSnapshot(string snapshotName)
        {
            if (mixer == null || string.IsNullOrEmpty(snapshotName)) return;
            var snap = mixer.FindSnapshot(snapshotName);
            TransitionToSnapshot(snap);
        }

        public void TransitionToSnapshot(AudioMixerSnapshot snapshot)
        {
            if (snapshot == null || snapshot == _activeSnapshot) return;
            snapshot.TransitionTo(snapshotTransitionSeconds);
            _activeSnapshot = snapshot;
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

        // ─── Procedural SFX by Name ──────────────────

        /// <summary>
        /// Plays a named procedural SFX at a world position (3D spatial).
        /// Names: Footstep, Interact, Discovery, ResonancePulse, CombatHit, etc.
        /// </summary>
        public void PlaySFX(string name, Vector3 position, float volume = 1.0f)
        {
            var clip = ProceduralSFXLibrary.Get(name);
            if (clip != null) PlaySFX(clip, position, volume);
        }

        /// <summary>
        /// Plays a named procedural SFX as 2D (non-spatial, for UI/feedback).
        /// </summary>
        public void PlaySFX2D(string name, float volume = 1.0f)
        {
            var clip = ProceduralSFXLibrary.Get(name);
            if (clip != null) PlaySFX2D(clip, volume);
        }

        AudioSource GetNextPooledSource()
        {
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Length;
            return source;
        }

        // ─── Cue Library API (data-driven) ───────────

        /// <summary>
        /// Plays a cue by ID from the AudioCueLibrary. Falls back silently if cue/library missing.
        /// </summary>
        public void PlayCue(string cueId, Vector3? position = null)
        {
            if (cueLibrary == null) return;
            var cue = cueLibrary.Find(cueId);
            if (cue == null) { Debug.LogWarning($"[AudioManager] Cue not found: {cueId}"); return; }
            PlayCue(cue, position);
        }

        public void PlayCue(AudioCue cue, Vector3? position = null)
        {
            if (cue == null) return;
            var clip = cue.PickClip();
            if (clip == null) return;

            var source = GetNextPooledSource();
            source.clip = clip;
            source.spatialBlend = cue.spatialBlend;
            source.volume = cue.volume;
            source.pitch = cue.PickPitch();
            source.loop = cue.loop;
            if (cue.mixerGroup != null) source.outputAudioMixerGroup = cue.mixerGroup;
            else if (sfxGroup != null) source.outputAudioMixerGroup = sfxGroup;
            if (position.HasValue) source.transform.position = position.Value;
            source.Play();
        }

        // ─── Mixer Volume API (slider-friendly) ──────

        const float MinDb = -80f;
        const float MaxDb = 0f;

        /// <summary>Set a normalized [0..1] volume on a mixer-exposed parameter (e.g. "MusicVolume").</summary>
        public void SetMixerVolume(string exposedParam, float normalized01)
        {
            if (mixer == null || string.IsNullOrEmpty(exposedParam)) return;
            float clamped = Mathf.Clamp01(normalized01);
            // Log curve so 0.5 is perceptually mid; -80 dB at 0, 0 dB at 1.
            float db = clamped <= 0.0001f ? MinDb : Mathf.Log10(clamped) * 20f;
            db = Mathf.Clamp(db, MinDb, MaxDb);
            mixer.SetFloat(exposedParam, db);
        }

        public float GetMixerVolume(string exposedParam)
        {
            if (mixer == null || string.IsNullOrEmpty(exposedParam)) return 1f;
            return mixer.GetFloat(exposedParam, out var db)
                ? Mathf.Pow(10f, db / 20f)
                : 1f;
        }
    }
}
