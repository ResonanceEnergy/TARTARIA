using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Adaptive Music Controller — 4-layer RS-reactive music system.
    ///
    /// Layer 0 (RS 0-25):   Ambient drone, desolate, sparse
    /// Layer 1 (RS 15-50):  Melodic fragments emerge, hope
    /// Layer 2 (RS 40-75):  Full orchestral, harmonic richness
    /// Layer 3 (RS 65-100): Triumphant, golden cascade, choir
    /// Combat overlay:      Percussive rhythm, low pulse
    /// Boss overlay:        Dissonant tritone tension
    ///
    /// All audio generated procedurally at runtime (prototype mode).
    /// Zone-specific motifs via golden-ratio frequency stepping.
    /// Budget: 0.5ms per frame.
    /// </summary>
    [DisallowMultipleComponent]
    public class AdaptiveMusicController : MonoBehaviour
    {
        public static AdaptiveMusicController Instance { get; private set; }

        [Header("Volume Settings")]
        [SerializeField, Tooltip("Master volume for all music layers")] float masterVolume = 0.6f;
        [SerializeField, Tooltip("Volume boost multiplier during combat")] float combatVolumeBoost = 1.2f;
        [SerializeField, Tooltip("Speed of layer crossfade transitions")] float crossfadeSpeed = 0.8f;

        // ─── Layers ───
        AudioSource _layer0Ambient;
        AudioSource _layer1Melodic;
        AudioSource _layer2Orchestral;
        AudioSource _layer3Triumphant;
        AudioSource _combatOverlay;
        AudioSource _bossOverlay;
        AudioSource _schumannLayer;
        AudioSource _stingerSource;

        // ─── State ───
        float _targetRS;
        float _currentRS;
        bool _combatActive;
        bool _bossActive;
        float _combatFade;
        float _bossFade;

        // ─── Zone Motif ───
        int _currentZone = -1;
        float _zoneBaseFreq = 432f;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            CreateAudioLayers();
        }

        void Start()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChange;
            StartAllLayers();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChange;
        }

        void Update()
        {
            _currentRS = Mathf.Lerp(_currentRS, _targetRS, crossfadeSpeed * Time.deltaTime);
            UpdateLayerVolumes();
            UpdateCombatOverlay();
            UpdateBossOverlay();
        }

        // ─── Public API — RS ─────────────────────────

        /// <summary>Update the RS value that drives layer blending.</summary>
        public void SetResonanceScore(float rs) => _targetRS = Mathf.Clamp(rs, 0f, 100f);

        /// <summary>Legacy API compat — routes to SetResonanceScore.</summary>
        public void UpdateResonanceScore(float rs) => SetResonanceScore(rs);

        // ─── Public API — Combat ─────────────────────

        public void EnterCombat() { _combatActive = true; }
        public void ExitCombat() { _combatActive = false; }

        /// <summary>Legacy stinger aliases.</summary>
        public void PlayCombatStart() => EnterCombat();
        public void PlayRestoration() => PlayStinger(StingerType.Discovery);
        public void PlayZoneShift() => PlayStinger(StingerType.ZoneComplete);
        public void PlayDiscovery() => PlayStinger(StingerType.Discovery);

        // ─── Public API — Boss ───────────────────────

        public void EnterBossEncounter() { _bossActive = true; _combatActive = false; }
        public void ExitBossEncounter() { _bossActive = false; }

        // ─── Public API — Zone ───────────────────────

        public void SetZone(int zoneIndex)
        {
            if (_currentZone == zoneIndex) return;
            _currentZone = zoneIndex;
            _zoneBaseFreq = 432f * Mathf.Pow(GoldenRatioValidator.PHI, zoneIndex * 0.05f);
            RegenerateProceduralAudio();
        }

        // ─── Public API — Stingers ──────────────────

        public void PlayStinger(StingerType type)
        {
            float freq, duration;
            switch (type)
            {
                case StingerType.Discovery:      freq = 528f;   duration = 0.8f; break;
                case StingerType.QuestComplete:   freq = 432f * GoldenRatioValidator.PHI; duration = 1.2f; break;
                case StingerType.TuningSuccess:   freq = 432f;   duration = 0.6f; break;
                case StingerType.TuningFail:      freq = 200f;   duration = 0.5f; break;
                case StingerType.BossPhase:       freq = 180f;   duration = 1.5f; break;
                case StingerType.BossDefeat:      freq = 1296f;  duration = 2f;   break;
                case StingerType.ZoneComplete:    freq = 528f * GoldenRatioValidator.PHI; duration = 2f; break;
                case StingerType.LevelUp:         freq = 864f;   duration = 1f;   break;
                default:                          freq = 432f;   duration = 0.5f; break;
            }
            AudioManager.Instance?.PlayTone(freq, duration);
        }

        // ─── State Change Handler ────────────────────

        void HandleStateChange(GameState prev, GameState current)
        {
            if (current == GameState.Combat && !_bossActive)
                EnterCombat();
            else if (current == GameState.Exploration)
                ExitCombat();
        }

        // ─── Layer Volume Control ────────────────────

        void UpdateLayerVolumes()
        {
            float l0 = RS2Volume(0f, 25f, inverse: true);
            float l1 = RS2Volume(15f, 50f);
            float l2 = RS2Volume(40f, 75f);
            float l3 = RS2Volume(65f, 100f);

            SmoothVolume(_layer0Ambient,    l0 * masterVolume);
            SmoothVolume(_layer1Melodic,    l1 * masterVolume);
            SmoothVolume(_layer2Orchestral, l2 * masterVolume);
            SmoothVolume(_layer3Triumphant, l3 * masterVolume);

            // Schumann layer fades in at RS 50+ (restoration progress)
            float lSch = RS2Volume(50f, 100f);
            SmoothVolume(_schumannLayer, lSch * masterVolume * 0.6f);
        }

        float RS2Volume(float start, float end, bool inverse = false)
        {
            float t = Mathf.InverseLerp(start, end, _currentRS);
            return inverse ? (1f - t) : t;
        }

        void SmoothVolume(AudioSource src, float target)
        {
            if (src == null) return;
            src.volume = Mathf.Lerp(src.volume, target, crossfadeSpeed * Time.deltaTime);
        }

        void UpdateCombatOverlay()
        {
            float target = _combatActive ? masterVolume * combatVolumeBoost : 0f;
            _combatFade = Mathf.Lerp(_combatFade, target, crossfadeSpeed * 2f * Time.deltaTime);
            if (_combatOverlay != null) _combatOverlay.volume = _combatFade;
        }

        void UpdateBossOverlay()
        {
            float target = _bossActive ? masterVolume * combatVolumeBoost * 1.3f : 0f;
            _bossFade = Mathf.Lerp(_bossFade, target, crossfadeSpeed * Time.deltaTime);
            if (_bossOverlay != null) _bossOverlay.volume = _bossFade;
        }

        // ─── Audio Layer Creation ────────────────────

        void CreateAudioLayers()
        {
            _layer0Ambient    = CreateLayer("Music_L0_Ambient");
            _layer1Melodic    = CreateLayer("Music_L1_Melodic");
            _layer2Orchestral = CreateLayer("Music_L2_Orchestral");
            _layer3Triumphant = CreateLayer("Music_L3_Triumphant");
            _combatOverlay    = CreateLayer("Music_CombatOverlay");
            _bossOverlay      = CreateLayer("Music_BossOverlay");
            _schumannLayer    = CreateLayer("Music_Schumann");

            _stingerSource = gameObject.AddComponent<AudioSource>();
            _stingerSource.loop = false;
            _stingerSource.playOnAwake = false;
            _stingerSource.spatialBlend = 0f;

            RegenerateProceduralAudio();
        }

        AudioSource CreateLayer(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.volume = 0f;
            src.spatialBlend = 0f;
            return src;
        }

        void StartAllLayers()
        {
            _layer0Ambient?.Play();
            _layer1Melodic?.Play();
            _layer2Orchestral?.Play();
            _layer3Triumphant?.Play();
            _combatOverlay?.Play();
            _bossOverlay?.Play();
            _schumannLayer?.Play();
        }

        // ─── Procedural Audio Generation ─────────────

        void RegenerateProceduralAudio()
        {
            int sr = 44100;
            int samples = sr * 8; // 8-second loops

            AssignClip(_layer0Ambient, GenTone(samples, sr, _zoneBaseFreq * 0.25f, 0.15f, WaveShape.Sine));

            AssignClip(_layer1Melodic, GenChord(samples, sr,
                new[] { _zoneBaseFreq, _zoneBaseFreq * 5f / 4f }, 0.1f));

            AssignClip(_layer2Orchestral, GenChord(samples, sr,
                new[] { _zoneBaseFreq, _zoneBaseFreq * 5f / 4f,
                        _zoneBaseFreq * 3f / 2f, _zoneBaseFreq * 2f }, 0.08f));

            AssignClip(_layer3Triumphant, GenChord(samples, sr,
                new[] { _zoneBaseFreq, _zoneBaseFreq * GoldenRatioValidator.PHI,
                        _zoneBaseFreq * 2f, _zoneBaseFreq * GoldenRatioValidator.PHI * 2f,
                        528f, 1296f }, 0.05f));

            AssignClip(_combatOverlay, GenTone(samples, sr, 80f, 0.2f, WaveShape.Square));

            AssignClip(_bossOverlay, GenChord(samples, sr,
                new[] { 180f, 180f * Mathf.Sqrt(2f) }, 0.12f));

            // Schumann resonance: 7.83 Hz AM-modulated onto audible carrier (313.2 Hz = 7.83 * 40)
            AssignClip(_schumannLayer, GenSchumannTone(samples, sr, 7.83f, 313.2f, 0.08f));
        }

        void AssignClip(AudioSource source, AudioClip newClip)
        {
            if (source == null) return;
            if (source.clip != null) Destroy(source.clip);
            source.clip = newClip;
        }

        AudioClip GenTone(int samples, int sr, float freq, float amp, WaveShape shape)
        {
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float v = shape switch
                {
                    WaveShape.Sine     => Mathf.Sin(2f * Mathf.PI * freq * t),
                    WaveShape.Triangle => Mathf.PingPong(t * freq * 2f, 1f) * 2f - 1f,
                    WaveShape.Square   => Mathf.Sin(2f * Mathf.PI * freq * t) >= 0 ? 1f : -1f,
                    _ => Mathf.Sin(2f * Mathf.PI * freq * t)
                };
                float env = Envelope(i, samples, sr);
                data[i] = v * amp * env;
            }
            var clip = AudioClip.Create($"Proc_{freq:F0}Hz", samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenChord(int samples, int sr, float[] freqs, float amp)
        {
            var data = new float[samples];
            float per = amp / freqs.Length;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float v = 0f;
                for (int f = 0; f < freqs.Length; f++)
                    v += Mathf.Sin(2f * Mathf.PI * freqs[f] * t) * per;
                data[i] = v * Envelope(i, samples, sr);
            }
            var clip = AudioClip.Create($"Chord_{freqs.Length}v", samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        float Envelope(int i, int total, int sr)
        {
            int fade = sr / 4;
            if (i < fade) return (float)i / fade;
            if (i > total - fade) return (float)(total - i) / fade;
            return 1f;
        }

        AudioClip GenSchumannTone(int samples, int sr, float modFreq, float carrierFreq, float amp)
        {
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sr;
                float carrier = Mathf.Sin(2f * Mathf.PI * carrierFreq * t);
                // AM modulation: (1 + depth * sin(mod)) * carrier
                float mod = 0.5f * (1f + Mathf.Sin(2f * Mathf.PI * modFreq * t));
                // Add first 3 Schumann harmonics as subtle carriers
                float h2 = Mathf.Sin(2f * Mathf.PI * (modFreq * 2f * 40f) * t) * 0.3f;
                float h3 = Mathf.Sin(2f * Mathf.PI * (modFreq * 3f * 40f) * t) * 0.15f;
                data[i] = (carrier + h2 + h3) * mod * amp * Envelope(i, samples, sr);
            }
            var clip = AudioClip.Create("Schumann_7.83Hz", samples, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        enum WaveShape { Sine, Triangle, Square }
    }

    public enum StingerType : byte
    {
        Discovery = 0,
        QuestComplete = 1,
        TuningSuccess = 2,
        TuningFail = 3,
        BossPhase = 4,
        BossDefeat = 5,
        ZoneComplete = 6,
        LevelUp = 7
    }
}
