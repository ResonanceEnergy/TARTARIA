using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Gameplay
{
    /// <summary>
    /// Choir Harmonics Mini-Game — Moon 6 (Rhythmic Moon / Pipe Organ Requiem).
    ///
    /// Mechanic: Conduct a children's choir of spectral orphans from Moon 3.
    /// Lirael leads; the player aligns voice parts to harmonic targets.
    /// Each voice part must match a frequency band (register) and enter on cue.
    ///
    /// Flow:
    ///   1. Display N voice parts (3-5), each with a target frequency band
    ///   2. Cues scroll — player triggers each voice to enter at correct beat
    ///   3. Active voices must be held in tune (frequency drift over time)
    ///   4. Full choir in harmony → ionised mist rain + RS surge
    ///
    /// Scoring:
    ///   - Entry timing accuracy per voice
    ///   - Sustained harmony duration (combo meter)
    ///   - Register accuracy (how close each voice to target Hz)
    ///   - Transcendent bonus (98%+ overall = Lirael Silver Passage solo)
    ///
    /// Performance tiers: Bronze 60%, Silver 80%, Gold 90%, Transcendent 98%+
    ///
    /// Design ref: GDD §03C Moon 6 — Cymatic Requiem, Children's Choir,
    ///             Lirael choir-conducting, Performance Combat
    /// </summary>
    public class ChoirHarmonicsMiniGame : MonoBehaviour
    {
        [Header("Choir Config")]
        [SerializeField] int voiceCount = 4;
        [SerializeField] float performanceDuration = 40f;
        [SerializeField] float entryTimingWindow = 0.3f;       // seconds tolerance

        [Header("Frequency Bands")]
        [SerializeField] float bassBand = 110f;                 // A2
        [SerializeField] float tenorBand = 220f;                // A3
        [SerializeField] float altoBand = 330f;                 // E4
        [SerializeField] float sopranoBand = 432f;              // A4 (Tartarian concert pitch)

        [Header("Drift")]
        [SerializeField] float driftRate = 8f;                  // Hz per second max drift
        [SerializeField] float correctionRate = 20f;            // Hz per second correction speed
        [SerializeField] float inTuneThreshold = 5f;            // Hz from target = in tune

        [Header("Scoring")]
        [SerializeField] float perfectEntryRS = 3f;
        [SerializeField] float harmonyBonusRSPerSecond = 0.5f;
        [SerializeField] float maxTotalRS = 40f;

        // ─── State ───
        VoicePart[] _voices;
        float[] _cueBeats;                  // when each voice should enter (seconds from start)
        float _elapsed;
        float _harmonyTime;                 // total seconds all voices in tune simultaneously
        bool _isActive;
        bool _completed;
        int _voicesEntered;

        public bool IsActive => _isActive;
        public bool IsCompleted => _completed;
        public float Elapsed => _elapsed;
        public float HarmonyTime => _harmonyTime;
        public int VoicesEntered => _voicesEntered;

        public event System.Action<float, PerformanceTier> OnPerformanceCompleted; // RS, tier
        public event System.Action OnPerformanceFailed;
        public event System.Action<int> OnVoiceEntered;               // voice index
        public event System.Action<int> OnVoiceDrifted;               // voice index
        public event System.Action OnFullHarmonyAchieved;
        public event System.Action OnTranscendentMoment;              // 98%+ threshold hit

        void Update()
        {
            if (!_isActive) return;

            _elapsed += Time.deltaTime;

            UpdateVoiceDrift();
            CheckHarmony();

            if (_elapsed >= performanceDuration)
                CompletePerformance();
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Start the choir performance.</summary>
        public void StartPerformance()
        {
            StartPerformance(voiceCount, performanceDuration);
        }

        /// <summary>Start with specific config.</summary>
        public void StartPerformance(int voices, float duration)
        {
            voiceCount = Mathf.Clamp(voices, 2, 5);
            performanceDuration = duration;

            float[] bands = { bassBand, tenorBand, altoBand, sopranoBand, 528f };

            _voices = new VoicePart[voiceCount];
            _cueBeats = new float[voiceCount];

            for (int i = 0; i < voiceCount; i++)
            {
                float target = bands[i % bands.Length];
                _voices[i] = new VoicePart
                {
                    targetFrequency = target,
                    currentFrequency = target,
                    isActive = false,
                    isInTune = false,
                    entryAccuracy = 0f,
                    driftSeed = Random.Range(0f, 100f)
                };

                // Stagger entries: each voice enters ~4s apart
                _cueBeats[i] = 2f + i * (duration * 0.15f);
            }

            _elapsed = 0f;
            _harmonyTime = 0f;
            _isActive = true;
            _completed = false;
            _voicesEntered = 0;

            Debug.Log($"[ChoirHarmonics] Performance started: {voiceCount} voices, {duration}s");
        }

        /// <summary>
        /// Trigger a voice to enter. Player must time this to the cue beat.
        /// </summary>
        public void EnterVoice(int voiceIndex)
        {
            if (!_isActive || voiceIndex < 0 || voiceIndex >= voiceCount) return;

            ref var voice = ref _voices[voiceIndex];
            if (voice.isActive) return;     // already entered

            voice.isActive = true;
            _voicesEntered++;

            // Evaluate entry timing
            float cueTime = _cueBeats[voiceIndex];
            float timingError = Mathf.Abs(_elapsed - cueTime);

            voice.entryAccuracy = timingError <= entryTimingWindow ? 1f
                : timingError <= entryTimingWindow * 3f ? 0.5f
                : 0.2f;

            OnVoiceEntered?.Invoke(voiceIndex);
        }

        /// <summary>
        /// Correct a drifting voice. Called per frame while player holds correction input.
        /// voiceIndex = which voice, direction = -1 (lower) or +1 (higher).
        /// </summary>
        public void CorrectVoice(int voiceIndex, float direction)
        {
            if (!_isActive || voiceIndex < 0 || voiceIndex >= voiceCount) return;

            ref var voice = ref _voices[voiceIndex];
            if (!voice.isActive) return;

            voice.currentFrequency += direction * correctionRate * Time.deltaTime;
        }

        /// <summary>Get voice data for UI.</summary>
        public VoicePart GetVoice(int index)
        {
            if (index < 0 || index >= voiceCount) return default;
            return _voices[index];
        }

        /// <summary>Get cue time for a voice.</summary>
        public float GetCueTime(int index)
        {
            if (index < 0 || index >= voiceCount) return 0f;
            return _cueBeats[index];
        }

        /// <summary>Get the current performance score as a normalised value 0-1.</summary>
        public float GetCurrentScore()
        {
            if (_elapsed <= 0f) return 0f;
            return Mathf.Clamp01(_harmonyTime / (_elapsed * 0.8f));  // 80% harmony = perfect
        }

        // ─── Internal ────────────────────────────────

        void UpdateVoiceDrift()
        {
            for (int i = 0; i < voiceCount; i++)
            {
                ref var voice = ref _voices[i];
                if (!voice.isActive) continue;

                // Perlin noise drift — each voice drifts independently
                float noise = Mathf.PerlinNoise(voice.driftSeed + _elapsed * 0.5f, i * 10f);
                float drift = (noise - 0.5f) * 2f * driftRate * Time.deltaTime;
                voice.currentFrequency += drift;

                // Check tune status
                float diff = Mathf.Abs(voice.currentFrequency - voice.targetFrequency);
                bool wasInTune = voice.isInTune;
                voice.isInTune = diff <= inTuneThreshold;

                if (wasInTune && !voice.isInTune)
                    OnVoiceDrifted?.Invoke(i);
            }
        }

        void CheckHarmony()
        {
            if (_voicesEntered == 0) return;

            bool allInTune = true;
            for (int i = 0; i < voiceCount; i++)
            {
                if (_voices[i].isActive && !_voices[i].isInTune)
                {
                    allInTune = false;
                    break;
                }
            }

            // Only count harmony when at least 2 voices are active and all are in tune
            if (allInTune && _voicesEntered >= 2)
            {
                float prevHarmony = _harmonyTime;
                _harmonyTime += Time.deltaTime;

                // Fire full harmony event on first frame of all-in-tune
                if (prevHarmony == 0f || (_voicesEntered == voiceCount && prevHarmony < 0.1f))
                    OnFullHarmonyAchieved?.Invoke();
            }
        }

        void CompletePerformance()
        {
            _isActive = false;
            _completed = true;

            float score = GetCurrentScore();
            PerformanceTier tier = score switch
            {
                >= 0.98f => PerformanceTier.Transcendent,
                >= 0.90f => PerformanceTier.Gold,
                >= 0.80f => PerformanceTier.Silver,
                >= 0.60f => PerformanceTier.Bronze,
                _ => PerformanceTier.Failed
            };

            float totalRS = 0f;

            // Entry accuracy RS
            for (int i = 0; i < voiceCount; i++)
            {
                if (_voices[i].isActive)
                    totalRS += perfectEntryRS * _voices[i].entryAccuracy;
            }

            // Harmony duration RS
            totalRS += _harmonyTime * harmonyBonusRSPerSecond;

            // Tier multipliers
            totalRS *= tier switch
            {
                PerformanceTier.Transcendent => 2.0f,
                PerformanceTier.Gold => 1.5f,
                PerformanceTier.Silver => 1.2f,
                _ => 1.0f
            };

            totalRS = Mathf.Min(totalRS, maxTotalRS);

            // Golden ratio bonus at Transcendent
            if (tier == PerformanceTier.Transcendent)
            {
                totalRS *= 1.618f;
                OnTranscendentMoment?.Invoke();
            }

            Debug.Log($"[ChoirHarmonics] Performance complete! Score {score:P0}, Tier {tier}, RS {totalRS:F1}");

            // Companion reactions
            ServiceLocator.Lirael?.ConductChildrenChoir();
            ServiceLocator.Milo?.AddTrust(5f);

            HapticFeedbackManager.Instance?.PlayBuildingEmergence();
            OnPerformanceCompleted?.Invoke(totalRS, tier);
            ServiceLocator.GameLoop?.OnMiniGameCompleted(totalRS, "ChoirHarmonics");
        }
    }

    // ─── Data Types ──────────────────────────────

    public struct VoicePart
    {
        public float targetFrequency;
        public float currentFrequency;
        public bool isActive;
        public bool isInTune;
        public float entryAccuracy;
        public float driftSeed;
    }

    public enum PerformanceTier : byte
    {
        Failed = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
        Transcendent = 4
    }
}
