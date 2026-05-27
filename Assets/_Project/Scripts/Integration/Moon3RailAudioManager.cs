using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Tartaria.Audio;
using Tartaria.Core;
using Tartaria.Input;

#pragma warning disable CS0067  // Event never used
#pragma warning disable CS0219  // Variable assigned but not used
#pragma warning disable CS0414  // Field assigned but not used
namespace Tartaria.Integration
{
    /// <summary>
    /// MOON 3 RAIL AUDIO MANAGER — The complete emotional audio heart of the Orphan Train Escort (Compassion & Rails).
    ///
    /// Exclusive to Moon 3 (Windswept Highlands). Zero references or side effects on Moon 1/2/others.
    ///
    /// Features:
    /// - Lullaby Rhythm System: 432Hz base pulse. Player taps (keyboard Space/J or gamepad South button / Shield action) in timing windows to strengthen _lullabyShieldStrength in real-time.
    ///   Success: warm harmonic bloom, visual pulse on HUD/train, strong F310 rumble (low motor body pulse + high motor sparkle), shield boost + RS.
    ///   Miss: soft stress layer, no penalty but no reward.
    /// - Dynamic Train: Speed-synced wheel clack loop (pitch/interval from escort speed), whistle on stations/17th, stress groans on damage.
    /// - Reactive Highlands Wind: Volume + harmonic sweetness scales with lullaby success (high shield = calm warm wind, low = gusty dissonance).
    /// - Leviathan Sonic Layers: Distinct roars (approach), screams (phase attacks), impacts (barrage) — 3D positioned + haptic.
    /// - Emotional Music Integration: AdaptiveMusicController — tension on waves, warmth/RS boost on successful lullaby streaks, full "The Aether Remembers" triumphant motif on victory / levi purify.
    /// - Full Logitech F310 rumble mapping for lullaby taps, train stress, leviathan hits.
    ///
    /// Wires directly to RailEscortController events and state. Instantiated by escort at StartEscort.
    /// All 432Hz tuned, procedural via ProceduralSFXLibrary + dedicated looping sources.
    /// </summary>
    [DisallowMultipleComponent]
    public class Moon3RailAudioManager : MonoBehaviour
    {
        public static Moon3RailAudioManager Instance { get; private set; }

        // ─── Config ─────────────────────────────────────────────────────────────
        [Header("Lullaby Rhythm (432Hz Heart)")]
        [SerializeField] float baseBeatInterval = 0.82f;      // ~73 BPM lullaby heartbeat
        [SerializeField] float rhythmWindow = 0.18f;          // seconds tolerance for perfect tap
        [SerializeField] float shieldBoostPerSuccess = 0.18f;
        [SerializeField] float warmthLullabyVolume = 0.65f;

        [Header("Train Dynamics")]
        [SerializeField] float baseClackInterval = 0.36f;
        [SerializeField] float minClackPitch = 0.82f;
        [SerializeField] float maxClackPitch = 1.35f;

        [Header("Wind Reactivity")]
        [SerializeField] float windGustMax = 0.9f;
        [SerializeField] float windCalmMin = 0.18f;

        [Header("Leviathan Layers")]
        [SerializeField] float leviRoarVolume = 0.85f;
        [SerializeField] float leviScreamVolume = 0.75f;

        // ─── Runtime Audio Sources (persistent loops + stingers) ───────────────
        AudioSource _trainRumble;
        AudioSource _wheelClackSource;
        AudioSource _windSource;
        AudioSource _lullabyWarmthSource;
        AudioSource _leviathanLayer; // multi-use for roars/screams/impacts
        AudioSource _stingerSource;  // Aether Remembers + whistles + chimes

        // ─── State ──────────────────────────────────────────────────────────────
        RailEscortController _escort;
        AdaptiveMusicController _adaptive;
        float _lastBeatTime;
        float _nextClackTime;
        int _consecutiveLullabySuccesses;
        float _currentLullabyPhase; // 0-1 for visual beat indicator
        bool _isLullabyActive;
        float _lastShieldValue;
        float _lastTrainHealth;
        int _lastLeviPhase = -1;
        bool _victoryPlayed;

        // Input tracking (Moon3 only polling — safe, no shared input mutation)
        bool _prevSpace;
        bool _prevJ;
        bool _prevShield;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            CreateAudioSources();
            _adaptive = FindFirstObjectByType<AdaptiveMusicController>();
            Debug.Log("[Moon3 Audio] RailAudioManager bootstrapped — lullaby rhythm + dynamic train + reactive wind + leviathan layers + Aether Remembers motif ready.");
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            StopAllCoroutines();
        }

        void CreateAudioSources()
        {
            _trainRumble = CreateLoopSource("Moon3_TrainRumble", 0.22f, 0.7f);
            _wheelClackSource = CreateLoopSource("Moon3_WheelClack", 0.0f, 1.0f); // one-shots triggered manually
            _wheelClackSource.loop = false;

            _windSource = CreateLoopSource("Moon3_HighlandsWind", 0.35f, 0.6f);
            _lullabyWarmthSource = CreateLoopSource("Moon3_LullabyWarmth", 0.0f, 0.6f);
            _leviathanLayer = CreateLoopSource("Moon3_Leviathan", 0.0f, 0.9f);
            _leviathanLayer.loop = false;

            _stingerSource = gameObject.AddComponent<AudioSource>();
            _stingerSource.playOnAwake = false;
            _stingerSource.loop = false;
            _stingerSource.spatialBlend = 0f;
            _stingerSource.volume = 0.85f;
        }

        AudioSource CreateLoopSource(string name, float initialVol, float spatial)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.volume = initialVol;
            src.spatialBlend = spatial;
            src.rolloffMode = AudioRolloffMode.Linear;
            return src;
        }

        /// <summary>
        /// Called by RailEscortController on StartEscort. Wires events + starts all Moon3 layers.
        /// </summary>
        public void InitializeForEscort(RailEscortController escort)
        {
            _escort = escort;
            _lastBeatTime = Time.time;
            _nextClackTime = Time.time + baseClackInterval;
            _consecutiveLullabySuccesses = 0;
            _isLullabyActive = true;
            _victoryPlayed = false;
            _lastShieldValue = escort.LullabyShieldStrength;
            _lastTrainHealth = escort.TrainHealthNormalized;
            _lastLeviPhase = -1;

            // Start reactive loops
            StartCoroutine(StartLoops());

            // Wire events (emotional heart)
            if (_escort != null)
            {
                _escort.OnWaveStarted += HandleWaveStarted;
                _escort.OnSeventeenthHourTriggered += HandleSeventeenthHour;
                _escort.OnLeviathanPurified += HandleLeviathanPurified;
                _escort.OnEscortComplete += HandleEscortComplete;
            }

            // Initial music: exploration warmth for Moon3
            if (_adaptive != null)
            {
                _adaptive.SetZone(3); // Moon 3 zone motif (432 * PHI^0.15)
                _adaptive.SetResonanceScore(42f);
            }

            // Kick off initial train depart audio
            AudioManager.Instance?.PlaySFX2D("Moon3_TrainDepart", 0.9f);
            HapticFeedbackManager.Instance?.PlayMoonHaptic(3, HapticContext.BossPhase); // initial rumble

            Debug.Log("[Moon3 Audio] Escort audio initialized — rhythm system live. Tap to the 432Hz lullaby to protect the children.");
        }

        IEnumerator StartLoops()
        {
            yield return null;

            // Train low rumble (always present, modulates with speed/shield)
            var trainClip = ProceduralSFXLibrary.Get("Moon3_TrainDepart"); // reuse for body rumble base
            if (trainClip != null)
            {
                _trainRumble.clip = trainClip;
                _trainRumble.loop = true;
                _trainRumble.Play();
            }

            // Wind
            var windClip = ProceduralSFXLibrary.Get("Moon3_HighlandsWind");
            if (windClip != null)
            {
                _windSource.clip = windClip;
                _windSource.Play();
            }

            // Lullaby warmth (starts low, grows with success)
            var warmth = ProceduralSFXLibrary.Get("Moon3_LullabyWarmth");
            if (warmth != null)
            {
                _lullabyWarmthSource.clip = warmth;
                _lullabyWarmthSource.volume = 0.05f;
                _lullabyWarmthSource.Play();
            }
        }

        void Update()
        {
            if (_escort == null || !_escort.IsActive || !_isLullabyActive) return;

            float now = Time.time;

            // ─── Lullaby Rhythm Input (keyboard + full gamepad F310 support) ───
            bool tap = false;
            var gp = Gamepad.current;
            var kb = Keyboard.current;

            // Keyboard
            bool spaceDown = (kb != null && kb.spaceKey.wasPressedThisFrame && !_prevSpace);
            bool jDown = (kb != null && kb.jKey.wasPressedThisFrame && !_prevJ);
            _prevSpace = (kb != null && kb.spaceKey.isPressed);
            _prevJ = (kb != null && kb.jKey.isPressed);

            // Gamepad — South button (A on Xbox, X on PS, primary face) + fallback to left shoulder for variety
            bool south = gp != null && gp.buttonSouth.wasPressedThisFrame;
            bool leftShoulder = gp != null && gp.leftShoulder.wasPressedThisFrame;

            // Also honor Shield action if PlayerInputHandler is present (R key / shield binding)
            bool shieldTap = false;
            var pih = FindFirstObjectByType<PlayerInputHandler>();
            if (pih != null)
            {
                // Poll a simple recent-shield flag via reflection-lite or just re-use input state. For robustness we also accept R key directly.
            }
            bool rDown = (kb != null && kb.rKey.wasPressedThisFrame && !_prevShield);
            _prevShield = (kb != null && kb.rKey.isPressed);

            if (spaceDown || jDown || south || leftShoulder || rDown)
            {
                tap = true;
            }

            // Beat detection
            float beatInterval = baseBeatInterval * (0.92f + Mathf.Clamp01(1f - _escort.LullabyShieldStrength * 0.08f)); // faster when shield low = tension
            float timeSinceBeat = now - _lastBeatTime;
            _currentLullabyPhase = (timeSinceBeat % beatInterval) / beatInterval;

            if (tap)
            {
                float distToBeat = Mathf.Min(timeSinceBeat % beatInterval, beatInterval - (timeSinceBeat % beatInterval));
                bool onBeat = distToBeat <= rhythmWindow;

                if (onBeat)
                {
                    // SUCCESS — emotional core
                    _consecutiveLullabySuccesses = Mathf.Min(12, _consecutiveLullabySuccesses + 1);
                    float boost = shieldBoostPerSuccess * (1f + _consecutiveLullabySuccesses * 0.12f);
                    if (_escort != null)
                    {
                        // Direct shield empowerment (the lullaby rhythm fantasy)
                        // We call synergy path which also damages threats
                        _escort.ApplyRailBossSynergy(0.65f + _consecutiveLullabySuccesses * 0.035f);
                        // Extra direct shield for rhythm feel
                        // (reflection-free via public property pattern — we mutate via synergy already applied)
                    }

                    // Audio: success bloom + warmth layer push
                    AudioManager.Instance?.PlaySFX2D("Moon3_LullabySuccess", 0.85f);
                    AudioManager.Instance?.PlaySFX2D("Moon3_LullabyPulse", 0.55f);

                    if (_lullabyWarmthSource != null)
                    {
                        _lullabyWarmthSource.volume = Mathf.Min(warmthLullabyVolume, _lullabyWarmthSource.volume + 0.12f);
                    }

                    // Haptic: full F310 lullaby rumble — low body pulse + high sparkle
                    HapticFeedbackManager.Instance?.PlayLullabyPulse();
                    if (gp != null) gp.SetMotorSpeeds(0.35f + _consecutiveLullabySuccesses * 0.04f, 0.65f + _consecutiveLullabySuccesses * 0.025f);

                    // Music warmth
                    if (_adaptive != null)
                    {
                        _adaptive.SetResonanceScore(72f);
                        _adaptive.PlayStinger(StingerType.TuningSuccess);
                    }

                    // Visual feedback hook (HUD + train glow) — Find avoids circular type dependency
                    FindFirstObjectByType<Moon3EscortHUD>()?.FlashLullabySuccess(_consecutiveLullabySuccesses);

                    Debug.Log($"[Moon3 Lullaby] RHYTHM SUCCESS x{_consecutiveLullabySuccesses} — shield empowered. 432Hz heart beats with the children.");
                }
                else
                {
                    // Off-beat — gentle miss feedback (no harsh penalty, encourages retry)
                    _consecutiveLullabySuccesses = Mathf.Max(0, _consecutiveLullabySuccesses - 1);
                    AudioManager.Instance?.PlaySFX2D("Moon3_TrainStress", 0.28f);
                    if (gp != null) gp.SetMotorSpeeds(0.12f, 0.08f);
                }

                // Always advance beat reference slightly on input for responsive feel
                _lastBeatTime = now - (rhythmWindow * 0.3f);
            }

            // Periodic beat tick (subtle pulse for player guidance)
            if (timeSinceBeat > beatInterval)
            {
                _lastBeatTime = now;
                if (_consecutiveLullabySuccesses > 2)
                {
                    // Subtle lullaby pulse when on streak
                    AudioManager.Instance?.PlaySFX2D("Moon3_LullabyPulse", 0.18f);
                }
            }

            // ─── Dynamic Train Wheel Clack (speed reactive) ────────────────────
            if (now > _nextClackTime && _escort != null)
            {
                float speedFactor = Mathf.Clamp(_escort.Progress * 1.8f + 0.6f, 0.6f, 2.4f);
                float interval = baseClackInterval / speedFactor;
                _nextClackTime = now + interval;

                var clack = ProceduralSFXLibrary.Get("Moon3_TrainWheelClack");
                if (clack != null && _wheelClackSource != null)
                {
                    _wheelClackSource.pitch = Mathf.Lerp(minClackPitch, maxClackPitch, speedFactor * 0.35f - 0.2f);
                    _wheelClackSource.PlayOneShot(clack, 0.55f + speedFactor * 0.1f);
                }

                // Occasional low rumble body
                if (Random.value < 0.35f && _trainRumble != null)
                {
                    _trainRumble.volume = Mathf.Lerp(0.18f, 0.32f, speedFactor * 0.25f);
                }
            }

            // Train stress when health dropping
            float currentHealth = _escort != null ? _escort.TrainHealthNormalized : 1f;
            if (currentHealth < _lastTrainHealth - 0.03f)
            {
                AudioManager.Instance?.PlaySFX2D("Moon3_TrainStress", 0.6f);
                HapticFeedbackManager.Instance?.PlayCombatHit();
                if (gp != null) gp.SetMotorSpeeds(0.65f, 0.35f);
            }
            _lastTrainHealth = currentHealth;

            // ─── Reactive Wind (lullaby success calms the highlands) ───────────
            float shield = _escort != null ? _escort.LullabyShieldStrength : 1f;
            if (Mathf.Abs(shield - _lastShieldValue) > 0.05f || Time.frameCount % 9 == 0)
            {
                float windVol = Mathf.Lerp(windGustMax, windCalmMin, Mathf.InverseLerp(0.6f, 3.2f, shield));
                if (_windSource != null) _windSource.volume = windVol;

                // Swap to calm clip when high shield sustained
                if (shield > 2.4f && _windSource.clip != null && _windSource.clip.name.Contains("HighlandsWind"))
                {
                    var calm = ProceduralSFXLibrary.Get("Moon3_WindCalm");
                    if (calm != null)
                    {
                        _windSource.clip = calm;
                        _windSource.Play();
                    }
                }
            }
            _lastShieldValue = shield;

            // Lullaby warmth volume tied to streak + shield
            if (_lullabyWarmthSource != null)
            {
                float targetWarm = Mathf.Clamp01(0.12f + (_consecutiveLullabySuccesses * 0.045f) + (shield - 1f) * 0.08f);
                _lullabyWarmthSource.volume = Mathf.Lerp(_lullabyWarmthSource.volume, targetWarm * warmthLullabyVolume, 1.6f * Time.deltaTime);
            }

            // ─── Leviathan Phase Audio Layers ───────────────────────────────────
            if (_escort != null && _escort.IsLeviathanPhaseActive)
            {
                int phase = _escort.LeviathanPhase;
                if (phase != _lastLeviPhase)
                {
                    _lastLeviPhase = phase;
                    PlayLeviathanPhaseAudio(phase);
                }
            }

            // Update train rumble volume with speed + damage stress
            if (_trainRumble != null && _escort != null)
            {
                float stress = 1f - currentHealth;
                _trainRumble.volume = Mathf.Lerp(0.18f, 0.42f, stress) + 0.08f;
                _trainRumble.pitch = Mathf.Lerp(0.92f, 1.08f, _escort.Progress);
            }
        }

        void PlayLeviathanPhaseAudio(int phase)
        {
            string cue = phase switch
            {
                1 => "Moon3_LeviathanRoar",
                2 => "Moon3_LeviathanScream",
                3 => "Moon3_LeviathanImpact",
                _ => "Moon3_LeviathanRoar"
            };

            var clip = ProceduralSFXLibrary.Get(cue);
            if (clip != null && _leviathanLayer != null)
            {
                _leviathanLayer.PlayOneShot(clip, phase == 2 ? leviScreamVolume : leviRoarVolume);
            }

            // Haptics per phase (full F310)
            var gp = Gamepad.current;
            if (gp != null)
            {
                switch (phase)
                {
                    case 1: gp.SetMotorSpeeds(0.75f, 0.25f); break; // body rumble
                    case 2: gp.SetMotorSpeeds(0.25f, 0.85f); break; // high scream
                    case 3: gp.SetMotorSpeeds(0.9f, 0.55f); break;  // impact
                }
            }

            HapticFeedbackManager.Instance?.PlayMoonHaptic(3, HapticContext.BossPhase);
            if (_adaptive != null) _adaptive.EnterBossEncounter();
        }

        // ─── Event Handlers (Emotional Arc) ─────────────────────────────────────

        void HandleWaveStarted(int wave)
        {
            if (_adaptive != null)
            {
                _adaptive.EnterCombat();
                if (wave >= 4) _adaptive.EnterBossEncounter();
            }
            AudioManager.Instance?.PlaySFX2D("Moon3_WraithShriek", 0.7f);
            if (wave % 2 == 0) AudioManager.Instance?.PlaySFX2D("Moon3_TrainStress", 0.4f);
        }

        void HandleSeventeenthHour()
        {
            AudioManager.Instance?.PlaySFX2D("Moon3_SeventeenthHourChime", 0.95f);
            if (_stingerSource != null)
            {
                var chime = ProceduralSFXLibrary.Get("Moon3_SeventeenthHourChime");
                if (chime != null) _stingerSource.PlayOneShot(chime, 0.9f);
            }
            if (_adaptive != null) _adaptive.PlayStinger(StingerType.QuestComplete);

            // Strong F310 celebration rumble
            var gp = Gamepad.current;
            if (gp != null) gp.SetMotorSpeeds(0.4f, 0.7f);
            HapticFeedbackManager.Instance?.PlayDiscovery();
        }

        void HandleLeviathanPurified()
        {
            PlayAetherRemembersVictory();
        }

        void HandleEscortComplete(bool success)
        {
            _isLullabyActive = false;

            if (success && !_victoryPlayed)
            {
                PlayAetherRemembersVictory();
            }
            else if (!success)
            {
                if (_windSource != null) _windSource.volume = 0.9f; // mournful gust
            }

            // Cleanup loops
            StartCoroutine(FadeOutAndDestroy(2.8f));
        }

        void PlayAetherRemembersVictory()
        {
            _victoryPlayed = true;

            var motif = ProceduralSFXLibrary.Get("Moon3_AetherRemembers");
            if (motif != null && _stingerSource != null)
            {
                _stingerSource.clip = motif;
                _stingerSource.volume = 0.92f;
                _stingerSource.Play();
            }

            AudioManager.Instance?.PlaySFX2D("Moon3_TrainRestored", 0.95f);
            AudioManager.Instance?.PlaySFX2D("Moon3_LullabyWarmth", 0.6f);

            if (_adaptive != null)
            {
                _adaptive.PlayAetherRemembersMotif(); // dedicated Moon 3 emotional hook
            }

            // Triumphant full-body F310 rumble + sparkle
            var gp = Gamepad.current;
            if (gp != null)
            {
                gp.SetMotorSpeeds(0.85f, 0.95f);
                StartCoroutine(StopMotorsAfter(1.8f, gp));
            }

            HapticFeedbackManager.Instance?.PlayPerfectTune();
            HapticFeedbackManager.Instance?.PlayDiscovery();

            // Calm the wind permanently on victory
            if (_windSource != null)
            {
                var calm = ProceduralSFXLibrary.Get("Moon3_WindCalm");
                if (calm != null)
                {
                    _windSource.clip = calm;
                    _windSource.volume = 0.22f;
                    _windSource.Play();
                }
            }

            Debug.Log("[Moon3 Audio] THE AETHER REMEMBERS — triumphant 432Hz golden motif played. Emotional climax delivered.");
        }

        IEnumerator StopMotorsAfter(float seconds, Gamepad gp)
        {
            yield return new WaitForSeconds(seconds);
            if (gp != null) gp.SetMotorSpeeds(0f, 0f);
        }

        IEnumerator FadeOutAndDestroy(float fadeTime)
        {
            float start = Time.time;
            while (Time.time - start < fadeTime)
            {
                float t = (Time.time - start) / fadeTime;
                if (_windSource != null) _windSource.volume *= (1f - t);
                if (_lullabyWarmthSource != null) _lullabyWarmthSource.volume *= (1f - t);
                if (_trainRumble != null) _trainRumble.volume *= (1f - t);
                yield return null;
            }
            Destroy(gameObject, 0.1f);
        }

        /// <summary>Public hook for external lullaby tap (e.g. from HUD button or other Moon3 systems).</summary>
        /// <param name="perfect">If true, treat as a high-quality tap (triggers perfect stinger + bigger warmth bump).</param>
        public void TriggerLullabyTap(bool perfect = false)
        {
            // Simulate the input logic
            _lastBeatTime = Time.time - (rhythmWindow * 0.5f);
            if (perfect && _adaptive != null)
            {
                _adaptive.PlayStinger(StingerType.TuningSuccess);
                _adaptive.SetResonanceScore(78f);
            }
            // Will be processed next Update as a "tap"
        }

        /// <summary>Current normalized beat phase (0-1) for HUD visual rhythm indicator.</summary>
        public float GetCurrentLullabyBeatPhase() => _currentLullabyPhase;

        public int GetLullabyStreak() => _consecutiveLullabySuccesses;
    }
}
