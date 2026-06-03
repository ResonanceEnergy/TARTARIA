using System;
using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Audio
{
    /// <summary>
    /// Adaptive Music Controller — 4-layer RS-reactive music system.
    ///
    /// Layer 0 (RS 0-25):   Ambient drone, desolate, sparse (fades out)
    /// Layer 1 (RS 15-55):  Melodic fragments emerge, hope (fades in 15-50, fades out 50-55)
    /// Layer 2 (RS 40-75):  Full orchestral, harmonic richness
    /// Layer 3 (RS 65-100): Triumphant, golden cascade, choir
    /// Schumann (RS 48-100): 7.83 Hz resonance layer (crossfades with L1 at RS 48-55)
    /// Combat overlay:      Percussive rhythm, low pulse
    /// Boss overlay:        Dissonant tritone tension
    ///
    /// CROSSFADE FIX (RS 50): Layer 1 fades out 50-55 while Schumann fades in 48-100,
    /// preventing layer congestion and AudioMixer routing conflicts.
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("AdaptiveMusicController");
            DontDestroyOnLoad(go);
            go.AddComponent<AdaptiveMusicController>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            CreateAudioLayers();
        }

        void Start()
        {
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged += HandleStateChange;
            StartAllLayers();
            BindLayer2Events();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (GameStateManager.Instance != null)
                GameStateManager.Instance.OnStateChanged -= HandleStateChange;
            UnbindLayer2Events();
        }

        void Update()
        {
            _currentRS = Mathf.Lerp(_currentRS, _targetRS, crossfadeSpeed * Time.deltaTime);
            UpdateLayerVolumes();
            UpdateCombatOverlay();
            UpdateBossOverlay();
            UpdateLayer2Reactive();
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

        /// <summary>
        /// Moon 3 exclusive emotional peak — "The Aether Remembers" triumphant motif hook.
        /// Called by Moon3RailAudioManager on leviathan purify / escort victory.
        /// Boosts layers to max and plays golden 432-derived cascade.
        /// </summary>
        public void PlayAetherRemembersMotif()
        {
            SetResonanceScore(99f);
            ExitBossEncounter();
            PlayStinger(StingerType.BossDefeat);
            PlayStinger(StingerType.ZoneComplete);
            Debug.Log("[AdaptiveMusic] Moon 3 — THE AETHER REMEMBERS motif triggered (triumphant 432Hz layers).");
        }

        // ─── State Change Handler ────────────────────

        void HandleStateChange(GameState prev, GameState current)
        {
            // Map state transitions to music behaviors
            switch (current)
            {
                case GameState.Combat:
                    if (!_bossActive)
                        EnterCombat();
                    break;

                case GameState.Exploration:
                    ExitCombat();
                    ExitBossEncounter();
                    break;

                case GameState.Tuning:
                    // Tuning mini-game: reduce combat overlay, boost melodic layers
                    ExitCombat();
                    break;

                case GameState.Cinematic:
                    // Cinematics: fade combat, preserve ambient
                    ExitCombat();
                    break;

                case GameState.Paused:
                case GameState.Menu:
                    // Paused/Menu: lower all layers
                    break;

                case GameState.Loading:
                case GameState.Boot:
                    // Boot/Loading: silent or minimal ambient
                    break;
            }

            Debug.Log($"[AdaptiveMusicController] State {prev} → {current} — adjusted music layers.");
        }

        // ─── Layer Volume Control ────────────────────

        void UpdateLayerVolumes()
        {
            float l0 = RS2Volume(0f, 25f, inverse: true);

            // Layer 1: Fade in 15-50, fade out 50-55 (crossfade with Schumann)
            float l1In = RS2Volume(15f, 50f);
            float l1Out = 1f - RS2Volume(50f, 55f);
            float l1 = l1In * l1Out;

            float l2 = RS2Volume(40f, 75f);
            float l3 = RS2Volume(65f, 100f);

            SmoothVolume(_layer0Ambient,    l0 * masterVolume);
            SmoothVolume(_layer1Melodic,    l1 * masterVolume);
            SmoothVolume(_layer2Orchestral, l2 * masterVolume);
            SmoothVolume(_layer3Triumphant, l3 * masterVolume);

            // Schumann layer: Start at RS 48 for 2-point overlap with L1 fadeout
            float lSch = RS2Volume(48f, 100f);
            SmoothVolume(_schumannLayer, lSch * masterVolume * 0.6f);

            // Debug logging at RS 50 threshold (only log when crossing)
            if (_currentRS >= 49.5f && _currentRS <= 50.5f && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[AdaptiveMusic] RS {_currentRS:F1} — L1:{l1:F2} L2:{l2:F2} Sch:{lSch:F2}");
            }
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
        // --- Layer 2 (Reactive) — Moon 1 gap fix 2026-05-31 ---
        // POI discovery arpeggio, real-time tuning tone, combat percussive bed, restoration swell.
        // Wired to Tartaria.Core.GameEvents.
        AudioSource _l2OneShot, _l2TuningTone, _l2Percussive;
        AudioClip _clipDiscoveryArp, _clipTuningTone, _clipCombatPercussive, _clipRestorationSwell;
        float _l2TuningOffset = 1f, _l2TuningFade, _l2PercussiveFade;
        bool _l2CombatActive, _l2Bound;

        void BindLayer2Events()
        {
            if (_l2Bound) return;
            _l2Bound = true;
            _l2OneShot     = CreateLayer2Source("L2_OneShot",     0f);
            _l2TuningTone  = CreateLayer2Source("L2_TuningTone",  0f);
            _l2Percussive  = CreateLayer2Source("L2_Percussive",  0f);
            _clipDiscoveryArp     = GenDiscoveryArpeggio();
            _clipTuningTone       = GenTuningTone();
            _clipCombatPercussive = GenCombatPercussive();
            _clipRestorationSwell = GenRestorationSwell();
            _l2TuningTone.clip = _clipTuningTone; _l2TuningTone.loop = true; _l2TuningTone.Play();
            _l2Percussive.clip = _clipCombatPercussive; _l2Percussive.loop = true; _l2Percussive.Play();
            try { Tartaria.Core.GameEvents.OnPOIDiscovered    += HandlePOIDiscovered;    }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(BindLayer2Events)} failed to subscribe to GameEvents.OnPOIDiscovered: {ex.GetType().Name}: {ex.Message}\n  context: Layer 2 POI discovery arpeggio will not play\n{ex.StackTrace}");
                // Non-fatal: discovery arpeggio is silent this session; other L2 reactive layers still wire below.
            }
            try { Tartaria.Core.GameEvents.OnTuningProgress   += HandleTuningProgress;   }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(BindLayer2Events)} failed to subscribe to GameEvents.OnTuningProgress: {ex.GetType().Name}: {ex.Message}\n  context: Layer 2 tuning tone pitch/volume will not track puzzle progress\n{ex.StackTrace}");
                // Non-fatal: tuning tone stays at default pitch/volume; mini-game still playable without the audio feedback layer.
            }
            try { Tartaria.Core.GameEvents.OnCombatStarted    += HandleCombatEnter;      }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(BindLayer2Events)} failed to subscribe to GameEvents.OnCombatStarted: {ex.GetType().Name}: {ex.Message}\n  context: Layer 2 combat percussive bed will not fade in\n{ex.StackTrace}");
                // Non-fatal: combat percussive layer stays muted; gameplay still functions.
            }
            try { Tartaria.Core.GameEvents.OnCombatEnded      += HandleCombatExit;       }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(BindLayer2Events)} failed to subscribe to GameEvents.OnCombatEnded: {ex.GetType().Name}: {ex.Message}\n  context: Layer 2 combat percussive bed will not fade out if OnCombatStarted somehow fires\n{ex.StackTrace}");
                // Non-fatal: combat layer may stay active longer than intended; only matters if combat-enter subscription succeeded.
            }
            try { Tartaria.Core.GameEvents.OnBuildingRestored += HandleBuildingRestored; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(BindLayer2Events)} failed to subscribe to GameEvents.OnBuildingRestored: {ex.GetType().Name}: {ex.Message}\n  context: Layer 2 restoration swell one-shot will not play\n{ex.StackTrace}");
                // Non-fatal: restoration swell one-shot is silent; visual restoration cinematic still plays via Moon1CinematicMoments.
            }
        }

        void UnbindLayer2Events()
        {
            if (!_l2Bound) return;
            _l2Bound = false;
            try { Tartaria.Core.GameEvents.OnPOIDiscovered    -= HandlePOIDiscovered;    }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(UnbindLayer2Events)} failed to unsubscribe from GameEvents.OnPOIDiscovered: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown / controller destroy\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak across scene loads; logged for diagnosis.
            }
            try { Tartaria.Core.GameEvents.OnTuningProgress   -= HandleTuningProgress;   }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(UnbindLayer2Events)} failed to unsubscribe from GameEvents.OnTuningProgress: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown / controller destroy\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
            try { Tartaria.Core.GameEvents.OnCombatStarted    -= HandleCombatEnter;      }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(UnbindLayer2Events)} failed to unsubscribe from GameEvents.OnCombatStarted: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown / controller destroy\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
            try { Tartaria.Core.GameEvents.OnCombatEnded      -= HandleCombatExit;       }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(UnbindLayer2Events)} failed to unsubscribe from GameEvents.OnCombatEnded: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown / controller destroy\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
            try { Tartaria.Core.GameEvents.OnBuildingRestored -= HandleBuildingRestored; }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(AdaptiveMusicController)}] {nameof(UnbindLayer2Events)} failed to unsubscribe from GameEvents.OnBuildingRestored: {ex.GetType().Name}: {ex.Message}\n  context: scene teardown / controller destroy\n{ex.StackTrace}");
                // Non-fatal: stale subscription may leak; logged for diagnosis.
            }
        }

        AudioSource CreateLayer2Source(string n, float vol)
        {
            var go = new GameObject(n);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false; src.spatialBlend = 0f; src.volume = vol;
            return src;
        }

        void HandlePOIDiscovered(string poiId, int rsReward, Vector3 worldPos)
        {
            if (_l2OneShot != null && _clipDiscoveryArp != null) _l2OneShot.PlayOneShot(_clipDiscoveryArp, 0.85f);
        }

        void HandleTuningProgress(float frequencyOffset)
        {
            _l2TuningOffset = Mathf.Clamp01(frequencyOffset);
            _l2TuningFade = 1f;
        }

        void HandleCombatEnter() { _l2CombatActive = true; }
        void HandleCombatExit()  { _l2CombatActive = false; }

        void HandleBuildingRestored(string buildingId)
        {
            if (_l2OneShot != null && _clipRestorationSwell != null) _l2OneShot.PlayOneShot(_clipRestorationSwell, 0.9f);
        }

        void UpdateLayer2Reactive()
        {
            if (!_l2Bound) return;
            _l2TuningFade = Mathf.Lerp(_l2TuningFade, 0f, Time.deltaTime * 0.6f);
            if (_l2TuningTone != null)
            {
                float vol = Mathf.Clamp01(_l2TuningFade * (1f - _l2TuningOffset));
                _l2TuningTone.volume = vol * 0.5f;
                _l2TuningTone.pitch = Mathf.Lerp(0.85f, 1.15f, 1f - _l2TuningOffset);
            }
            float targetPerc = _l2CombatActive ? 0.55f : 0f;
            _l2PercussiveFade = Mathf.Lerp(_l2PercussiveFade, targetPerc, Time.deltaTime * 1.5f);
            if (_l2Percussive != null) _l2Percussive.volume = _l2PercussiveFade;
        }

        AudioClip GenSine(string name, float duration, System.Func<float, float> sampler)
        {
            const int sr = 44100;
            int count = Mathf.Max(1, Mathf.RoundToInt(sr * duration));
            var samples = new float[count];
            for (int i = 0; i < count; i++) samples[i] = sampler((float)i / count);
            var clip = AudioClip.Create(name, count, 1, sr, false);
            clip.SetData(samples, 0);
            return clip;
        }

        AudioClip GenDiscoveryArpeggio()
        {
            float[] notes = { 261.63f, 329.63f, 392f, 523.25f, 659.25f };
            return GenSine("L2_DiscoveryArp", 1.5f, t =>
            {
                int idx = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(t * notes.Length));
                float local = (t * notes.Length) - idx;
                float env = Mathf.Exp(-local * 4f);
                return Mathf.Sin(2f * Mathf.PI * notes[idx] * t * 1.5f) * env * 0.45f;
            });
        }

        AudioClip GenTuningTone() => GenSine("L2_TuningTone", 1f, t => Mathf.Sin(2f * Mathf.PI * 432f * t) * 0.35f);

        AudioClip GenCombatPercussive() => GenSine("L2_CombatPercussive", 4f, t =>
        {
            float beat = (t * 4f) % 1f;
            float kick = Mathf.Exp(-beat * 12f) * Mathf.Sin(2f * Mathf.PI * 60f * t * 4f);
            float diss = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.15f + Mathf.Sin(2f * Mathf.PI * 311f * t) * 0.15f;
            return (kick * 0.6f + diss * 0.25f);
        });

        AudioClip GenRestorationSwell() => GenSine("L2_RestorationSwell", 2.5f, t =>
        {
            float env = Mathf.SmoothStep(0f, 1f, t < 0.5f ? t * 2f : (1f - t) * 2f);
            float a = Mathf.Sin(2f * Mathf.PI * 261.63f * t);
            float b = Mathf.Sin(2f * Mathf.PI * 329.63f * t);
            float c = Mathf.Sin(2f * Mathf.PI * 392f * t);
            float d = Mathf.Sin(2f * Mathf.PI * 523.25f * t);
            return (a + b + c + d) * 0.18f * env;
        });

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
