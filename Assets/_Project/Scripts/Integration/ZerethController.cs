using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Zereth — The Dissonant One, primary antagonist controller.
    ///
    /// Korath's brother. Giant-sized. Calibrated the original frequencies
    /// of Tartaria with flawless precision. Did not seek destruction —
    /// he sought transcendence beyond the Grid's single-frequency cage.
    ///
    /// Presence escalation across moons:
    ///   Moon 2:  Pipe organ records signed "Z." Lore hint only.
    ///   Moon 7:  Korath reveals: "My brother Zereth calibrated the frequencies."
    ///   Moon 9:  Prophecy Stones trigger Zereth voice responses.
    ///   Moon 10: Semi-regular voice presence. Counter-narrative begins.
    ///   Moon 12: Physical evidence — giant footprints in Trigger Room.
    ///   Moon 13: Full manifestation at the Nexus. Final confrontation.
    ///
    /// Not a health-bar boss. Zereth is defeated through resonance
    /// alignment — proving that harmony and freedom can coexist.
    ///
    /// Design refs: GDD §03C (Moon Mechanics), §17 (Day Out of Time)
    /// </summary>
    public class ZerethController : MonoBehaviour
    {
        public static ZerethController Instance { get; private set; }

        // ─── Presence System ───
        [Header("Presence")]
        [SerializeField] float dissonanceRadius = 30f;
        [SerializeField] float voiceVolumeCurve = 0.3f;

        float _presenceLevel;        // 0=dormant, 1=full manifestation
        int _prophecyStonesTriggered;
        int _voiceResponsesPlayed;
        bool _physicallyManifested;

        // ─── Confrontation Phases ───
        ZerethPhase _phase = ZerethPhase.Dormant;
        bool _korathRevelationHeard;
        bool _triggerRoomDiscovered;
        bool _finalConfrontationStarted;
        bool _redeemed;

        // ─── Dissonance Aura State ───
        float _dissonanceIntensity;
        float _dissonancePulseTimer;

        public float PresenceLevel => _presenceLevel;
        public ZerethPhase Phase => _phase;
        public bool IsPhysicallyManifested => _physicallyManifested;
        public bool IsRedeemed => _redeemed;
        public float DissonanceIntensity => _dissonanceIntensity;

        public event System.Action<ZerethPhase> OnPhaseChanged;
        public event System.Action<string> OnVoiceResponse;
        public event System.Action OnPhysicalManifestation;
        public event System.Action OnRedemption;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            if (_phase == ZerethPhase.Dormant) return;

            UpdateDissonancePulse();
            UpdatePresenceLevel();
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Moon 7: Korath reveals Zereth's identity. Awakens voice phase.
        /// </summary>
        public void OnKorathRevelation()
        {
            if (_korathRevelationHeard) return;
            _korathRevelationHeard = true;
            TransitionToPhase(ZerethPhase.Whispers);
            _presenceLevel = 0.2f;
        }

        /// <summary>
        /// Moon 9+: Player triggers a Prophecy Stone vision.
        /// Zereth responds with counter-narrative.
        /// </summary>
        public void OnProphecyStoneTriggered(int stoneIndex)
        {
            _prophecyStonesTriggered++;

            string responseId = stoneIndex switch
            {
                1 => "zereth_stone_paradise",
                3 => "zereth_stone_granite",
                6 => "zereth_stone_bells",
                _ => $"zereth_stone_{stoneIndex}"
            };

            DialogueManager.Instance?.PlayLineById(responseId);
            _voiceResponsesPlayed++;
            OnVoiceResponse?.Invoke(responseId);

            // Increase presence with each stone
            _presenceLevel = Mathf.Min(_presenceLevel + 0.08f, 0.8f);

            if (_prophecyStonesTriggered >= 3 && _phase < ZerethPhase.Voice)
                TransitionToPhase(ZerethPhase.Voice);
        }

        /// <summary>
        /// Moon 10+: Dissonance event in the world. Zereth speaks unbidden.
        /// </summary>
        public void TriggerDissonanceEvent(Vector3 worldPosition)
        {
            if (_phase < ZerethPhase.Voice) return;

            _dissonanceIntensity = 1f;

            // Apply dissonance aura
            VFXController.Instance?.PlayDissonancePulse(worldPosition, dissonanceRadius);
            HapticFeedbackManager.Instance?.PlayCombatHit();

            // Counter-narrative voice line
            string[] lines =
            {
                "zereth_dissonance_harmony",
                "zereth_dissonance_freedom",
                "zereth_dissonance_cage",
                "zereth_dissonance_transcend"
            };
            string line = lines[_voiceResponsesPlayed % lines.Length];
            DialogueManager.Instance?.PlayLineById(line);
            _voiceResponsesPlayed++;
            OnVoiceResponse?.Invoke(line);
        }

        /// <summary>
        /// Moon 12: Trigger Room discovery — giant footprints.
        /// Elevates to physical evidence phase.
        /// </summary>
        public void OnTriggerRoomDiscovered()
        {
            if (_triggerRoomDiscovered) return;
            _triggerRoomDiscovered = true;
            _presenceLevel = 0.9f;
            TransitionToPhase(ZerethPhase.Evidence);

            DialogueManager.Instance?.PlayLineById("zereth_trigger_room");
        }

        /// <summary>
        /// Moon 13: Full physical manifestation at the Nexus.
        /// Not a combat encounter — resonance alignment challenge.
        /// </summary>
        public void TriggerFinalManifestation()
        {
            if (_physicallyManifested) return;
            _physicallyManifested = true;
            _presenceLevel = 1f;
            _finalConfrontationStarted = true;
            TransitionToPhase(ZerethPhase.Manifested);

            // Giant-scale appearance
            transform.localScale = Vector3.one * 5f;

            OnPhysicalManifestation?.Invoke();
            DialogueManager.Instance?.PlayLineById("zereth_manifest_final");
            Debug.Log("[Zereth] Full manifestation at the Nexus. Radiating 9-Band transcendence energy.");
        }

        /// <summary>
        /// Resolve the confrontation through resonance alignment.
        /// Zereth's face shows ecstasy, not malice — he was ascending.
        /// </summary>
        public void ResolveConfrontation(float alignmentScore)
        {
            if (!_finalConfrontationStarted || _redeemed) return;

            // Alignment >= 0.9 = redemption path. Zereth's frequencies
            // merge with the Grid rather than breaking free of it.
            if (alignmentScore >= 0.9f)
            {
                _redeemed = true;
                _dissonanceIntensity = 0f;
                _phase = ZerethPhase.Redeemed;

                DialogueManager.Instance?.PlayLineById("zereth_redemption");
                OnRedemption?.Invoke();
                OnPhaseChanged?.Invoke(ZerethPhase.Redeemed);

                Debug.Log("[Zereth] Redeemed. Harmony and freedom coexist.");
            }
            else
            {
                // Partial alignment — dissonance persists but weakened
                _dissonanceIntensity = 1f - alignmentScore;
                DialogueManager.Instance?.PlayLineById("zereth_partial_align");
            }
        }

        /// <summary>
        /// Get current dissonance modifier for corruption systems.
        /// Higher presence = faster corruption spread if unchecked.
        /// </summary>
        public float GetCorruptionModifier()
        {
            if (_redeemed) return 0f;
            return _presenceLevel * 0.5f; // Up to 50% corruption boost
        }

        // ─── Internal ────────────────────────────────

        void TransitionToPhase(ZerethPhase newPhase)
        {
            if (newPhase <= _phase) return;
            _phase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
            Debug.Log($"[Zereth] Phase transition: {newPhase}");
        }

        void UpdateDissonancePulse()
        {
            if (_dissonanceIntensity <= 0f) return;

            _dissonancePulseTimer += Time.deltaTime;
            if (_dissonancePulseTimer >= 2f)
            {
                _dissonancePulseTimer = 0f;
                _dissonanceIntensity = Mathf.Max(0f, _dissonanceIntensity - 0.05f);

                // Lirael detects dissonance automatically
                if (_dissonanceIntensity > 0.3f)
                    LiraelController.Instance?.DetectDissonance();
            }
        }

        void UpdatePresenceLevel()
        {
            // Presence slowly grows once awakened (simulates growing influence)
            if (_phase >= ZerethPhase.Voice && _presenceLevel < 0.8f)
                _presenceLevel += 0.001f * Time.deltaTime;
        }

        // ─── Save / Load ────────────────────────────

        public ZerethSaveData GetSaveData()
        {
            return new ZerethSaveData
            {
                presenceLevel = _presenceLevel,
                phase = (int)_phase,
                prophecyStonesTriggered = _prophecyStonesTriggered,
                voiceResponsesPlayed = _voiceResponsesPlayed,
                korathRevelationHeard = _korathRevelationHeard,
                triggerRoomDiscovered = _triggerRoomDiscovered,
                physicallyManifested = _physicallyManifested,
                finalConfrontationStarted = _finalConfrontationStarted,
                redeemed = _redeemed
            };
        }

        public void LoadSaveData(ZerethSaveData data)
        {
            _presenceLevel = data.presenceLevel;
            _phase = (ZerethPhase)data.phase;
            _prophecyStonesTriggered = data.prophecyStonesTriggered;
            _voiceResponsesPlayed = data.voiceResponsesPlayed;
            _korathRevelationHeard = data.korathRevelationHeard;
            _triggerRoomDiscovered = data.triggerRoomDiscovered;
            _physicallyManifested = data.physicallyManifested;
            _finalConfrontationStarted = data.finalConfrontationStarted;
            _redeemed = data.redeemed;
        }
    }

    public enum ZerethPhase : byte
    {
        Dormant = 0,
        Whispers = 1,     // Moon 7: Korath reveals identity
        Voice = 2,         // Moon 9: Active voice responses
        Evidence = 3,      // Moon 12: Physical evidence found
        Manifested = 4,    // Moon 13: Full physical form
        Redeemed = 5       // Post-confrontation redemption
    }

    [System.Serializable]
    public class ZerethSaveData
    {
        public float presenceLevel;
        public int phase;
        public int prophecyStonesTriggered;
        public int voiceResponsesPlayed;
        public bool korathRevelationHeard;
        public bool triggerRoomDiscovered;
        public bool physicallyManifested;
        public bool finalConfrontationStarted;
        public bool redeemed;
    }
}
