using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tartaria.Core;
using Tartaria.Input;

namespace Tartaria.Integration
{
    /// <summary>
    /// Milo the Mudslinger — Companion NPC Controller.
    ///
    /// Former black-market antiquities dealer turned reluctant hero.
    /// Comic relief with a hidden heart. Uses humor as armor.
    ///
    /// Unlocks: Moon 1 (Echohaven — first companion met)
    /// Trust Arc: Cynical → Curious → Invested → Transformed
    /// Role: Banter, artifact appraisal, market connections, comic relief
    ///
    /// Design refs: GDD §05 (Characters §2.2), §03A (Main Storyline), docs/15 §10
    /// </summary>
    public class MiloController : MonoBehaviour, IMiloService
    {
        public static MiloController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 15f;
        float _trust;

        // ─── docs/15 §10 Companion: Milo state machine (2026-06-03) ───
        [Header("Companion State Machine (docs/15 §10)")]
        [SerializeField] MiloState _currentState = MiloState.Follow;

        public MiloState CurrentState { get; private set; } = MiloState.Follow;
        public event System.Action<MiloState> OnStateChanged;

        // VO bank loaded from Resources/Yarn/MiloVOLines.txt (40 lines, partitioned by state).
        Dictionary<MiloState, List<string>> _voBank;
        Coroutine _reactReturnCo;
        Coroutine _celebrateReturnCo;

        // RS threshold tracking for REACT trigger (every 25 RS gained crosses a threshold).
        float _lastRSThresholdCrossed = 0f;
        const float RS_REACT_STEP = 25f;

        public float Trust => _trust;
        public MiloTrustLevel TrustLevel => _trust switch
        {
            < 25f => MiloTrustLevel.Cynical,
            < 50f => MiloTrustLevel.Curious,
            < 75f => MiloTrustLevel.Invested,
            _ => MiloTrustLevel.Transformed
        };

        // ─── State ───
        bool _introduced;

        public bool HasIntroduced => _introduced;
        int _artifactsAppraised;
        int _jokesDelivered;
        int _sincereMoments;
        bool _orphanTrainWitnessed;       // Moon 3 story beat
        bool _whiteCityOutburst;          // Moon 5 story beat
        bool _korathSacrificeWitnessed;   // Moon 7 story beat

        // ─── Events ───
        public event System.Action<MiloTrustLevel> OnTrustChanged;
        public event System.Action OnIntroduced;
        public event System.Action<int> OnArtifactAppraised;  // artifact count
        public event System.Action OnSincereMoment;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Milo = this;
            _trust = initialTrust;

            LoadVOBank();
            SubscribeStateMachineEvents();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeStateMachineEvents();
                Instance = null;
                if (ServiceLocator.Milo == (object)this) ServiceLocator.Milo = null;
            }
        }

        // ─── docs/15 §10 STATE MACHINE ───────────────────────────────────────

        void SubscribeStateMachineEvents()
        {
            GameEvents.OnDialogueStart += HandleDialogueStart;
            GameEvents.OnDialogueEnd += HandleDialogueEnd;
            GameEvents.OnBuildingRestored += HandleBuildingRestored;
            GameEvents.OnTuningStart += HandleTuningStart;
            GameEvents.OnTuningEnd += HandleTuningEnd;
            GameEvents.OnCombatEngaged += HandleCombatEngaged;
            GameEvents.OnCombatEnded += HandleCombatEnded;
            GameEvents.OnRSChanged += HandleRSChanged;
        }

        void UnsubscribeStateMachineEvents()
        {
            GameEvents.OnDialogueStart -= HandleDialogueStart;
            GameEvents.OnDialogueEnd -= HandleDialogueEnd;
            GameEvents.OnBuildingRestored -= HandleBuildingRestored;
            GameEvents.OnTuningStart -= HandleTuningStart;
            GameEvents.OnTuningEnd -= HandleTuningEnd;
            GameEvents.OnCombatEngaged -= HandleCombatEngaged;
            GameEvents.OnCombatEnded -= HandleCombatEnded;
            GameEvents.OnRSChanged -= HandleRSChanged;
        }

        /// <summary>
        /// Authoritative transition. Logs the move, fires OnStateChanged, and
        /// for SPEAK/REACT/CELEBRATE pushes a VO line into the HUD dialogue
        /// channel via the canonical GameEvents.RaiseHUDShowDialogue path.
        /// </summary>
        public void TransitionTo(MiloState next)
        {
            if (next == CurrentState) return;

            var prev = CurrentState;
            CurrentState = next;
            _currentState = next;
            Debug.Log($"[Milo §10] State: {prev} -> {next}");
            OnStateChanged?.Invoke(next);

            switch (next)
            {
                case MiloState.Speak:
                case MiloState.React:
                case MiloState.Celebrate:
                    var line = GetRandomLine(next);
                    if (!string.IsNullOrEmpty(line))
                        GameEvents.RaiseHUDShowDialogue("Milo", line);
                    break;
            }
        }

        // ─── Event handlers ───────────────────────────────────────────────────

        void HandleDialogueStart(string speaker) => TransitionTo(MiloState.Speak);

        void HandleDialogueEnd(string speaker)
        {
            // Don't override transient REACT/CELEBRATE timers.
            if (CurrentState == MiloState.Speak) TransitionTo(MiloState.Follow);
        }

        void HandleBuildingRestored(string buildingId)
        {
            // Existing trust + dialogue path still runs from NotifyBuildingRestored
            // (called separately by spec-driven systems). State machine only handles
            // the visible companion behaviour.
            if (_celebrateReturnCo != null) StopCoroutine(_celebrateReturnCo);
            TransitionTo(MiloState.Celebrate);
            _celebrateReturnCo = StartCoroutine(ReturnToFollowAfter(3f, MiloState.Celebrate));
        }

        void HandleTuningStart() => TransitionTo(MiloState.Idle);

        void HandleTuningEnd()
        {
            if (CurrentState == MiloState.Idle) TransitionTo(MiloState.Follow);
        }

        void HandleCombatEngaged() => TransitionTo(MiloState.Hide);

        void HandleCombatEnded()
        {
            if (CurrentState == MiloState.Hide) TransitionTo(MiloState.Follow);
        }

        void HandleRSChanged(float newRSValue)
        {
            // Every full RS_REACT_STEP gained, Milo reacts briefly.
            float step = Mathf.Floor(newRSValue / RS_REACT_STEP) * RS_REACT_STEP;
            if (step <= _lastRSThresholdCrossed) return;
            _lastRSThresholdCrossed = step;

            if (_reactReturnCo != null) StopCoroutine(_reactReturnCo);
            TransitionTo(MiloState.React);
            _reactReturnCo = StartCoroutine(ReturnToFollowAfter(1f, MiloState.React));
        }

        IEnumerator ReturnToFollowAfter(float seconds, MiloState onlyIfStill)
        {
            yield return new WaitForSeconds(seconds);
            if (CurrentState == onlyIfStill) TransitionTo(MiloState.Follow);
        }

        // ─── VO bank loader (Resources/Yarn/MiloVOLines.txt) ──────────────────

        void LoadVOBank()
        {
            _voBank = new Dictionary<MiloState, List<string>>();
            foreach (MiloState s in System.Enum.GetValues(typeof(MiloState)))
                _voBank[s] = new List<string>();

            var ta = Resources.Load<TextAsset>("Yarn/MiloVOLines");
            if (ta == null)
            {
                Debug.LogWarning("[Milo §10] Resources/Yarn/MiloVOLines.txt not found — VO bank empty.");
                return;
            }

            var lines = ta.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var raw in lines)
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (!line.StartsWith("[")) continue;

                int close = line.IndexOf(']');
                if (close <= 1) continue;

                var tag = line.Substring(1, close - 1).Trim();
                var body = line.Substring(close + 1).Trim();
                if (body.Length == 0) continue;

                if (System.Enum.TryParse<MiloState>(tag, true, out var state))
                    _voBank[state].Add(body);
            }

            int total = 0;
            foreach (var kv in _voBank) total += kv.Value.Count;
            Debug.Log($"[Milo §10] VO bank loaded: {total} lines across {_voBank.Count} states.");
        }

        /// <summary>Get a random VO line for the given state, or null if none.</summary>
        public string GetRandomLine(MiloState state)
        {
            if (_voBank == null || !_voBank.TryGetValue(state, out var pool) || pool.Count == 0)
                return null;
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Trigger Milo's introduction in Echohaven.</summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("milo_intro");
            OnIntroduced?.Invoke();
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Modify trust. Clamped 0-100.</summary>
        public void AddTrust(float amount)
        {
            var oldLevel = TrustLevel;
            _trust = Mathf.Clamp(_trust + amount, 0f, 100f);
            var newLevel = TrustLevel;

            if (oldLevel != newLevel)
            {
                OnTrustChanged?.Invoke(newLevel);
                Save.SaveManager.Instance?.MarkDirty();

                switch (newLevel)
                {
                    case MiloTrustLevel.Curious:
                        DialogueManager.Instance?.PlayContextDialogue("milo_warming_up");
                        break;
                    case MiloTrustLevel.Invested:
                        DialogueManager.Instance?.PlayContextDialogue("milo_sincere");
                        TriggerSincereMoment();
                        break;
                    case MiloTrustLevel.Transformed:
                        DialogueManager.Instance?.PlayLineById("milo_trust_final");
                        break;
                }
            }
        }

        /// <summary>Request an artifact appraisal from Milo.</summary>
        public void AppraiseArtifact()
        {
            _artifactsAppraised++;
            OnArtifactAppraised?.Invoke(_artifactsAppraised);
            Save.SaveManager.Instance?.MarkDirty();

            if (TrustLevel >= MiloTrustLevel.Curious)
            {
                DialogueManager.Instance?.PlayContextDialogue("milo_appraise_genuine");
                AddTrust(2f);
            }
            else
            {
                // Low trust: Milo tries to scam
                DialogueManager.Instance?.PlayContextDialogue("milo_appraise_scam");
                AddTrust(1f);
            }
        }

        /// <summary>Request banter/joke. Builds rapport.</summary>
        public void RequestBanter()
        {
            _jokesDelivered++;

            string[] contexts = TrustLevel switch
            {
                MiloTrustLevel.Cynical => new[] { "milo_joke_cynical_01", "milo_joke_cynical_02" },
                MiloTrustLevel.Curious => new[] { "milo_joke_warm_01", "milo_joke_warm_02" },
                MiloTrustLevel.Invested => new[] { "milo_joke_invested_01", "milo_joke_invested_02" },
                _ => new[] { "milo_joke_transformed_01", "milo_joke_transformed_02" }
            };

            string line = contexts[_jokesDelivered % contexts.Length];
            DialogueManager.Instance?.PlayLineById(line);
            AddTrust(1f);
        }

        /// <summary>Request market intel (requires Curious+).</summary>
        public void RequestMarketIntel()
        {
            if (TrustLevel < MiloTrustLevel.Curious)
            {
                DialogueManager.Instance?.PlayContextDialogue("milo_no_intel");
                return;
            }

            DialogueManager.Instance?.PlayContextDialogue("milo_market_intel");
            AddTrust(2f);
        }

        // ─── Story Beats ─────────────────────────────

        /// <summary>Moon 3: Milo witnesses the orphan train.</summary>
        public void WitnessOrphanTrain()
        {
            if (_orphanTrainWitnessed) return;
            _orphanTrainWitnessed = true;
            DialogueManager.Instance?.PlayContextDialogue("milo_orphan_train");
            AddTrust(10f);
            TriggerSincereMoment();
            Save.SaveManager.Instance?.MarkDirty();
            Input.HapticFeedbackManager.Instance?.PlayDiscovery();
        }

        // ─── Moon 3 Vertical Slice: First Orphan + Physical Train Positioning + Escort Banter ───

        /// <summary>Moon 3 authored: Milo witnesses the FIRST specific orphan adoption (protective quip for vertical slice).</summary>
        public void WitnessFirstOrphan(string orphanName)
        {
            AddTrust(3f);
            GameEvents.RaiseHUDShowBanner("Milo protects", $"Kid named {orphanName}? ...Nobody messes with our family now. I've got the rear.", 5f);
            DialogueManager.Instance?.PlayContextDialogue("milo_first_orphan_" + orphanName.ToLower());
            Debug.Log($"[Milo Moon3] Protective: {orphanName} is under my watch. Found family secured.");
            Save.SaveManager.Instance?.MarkDirty();
            Input.HapticFeedbackManager.Instance?.PlayTuningCorrectHit(false, 0.4f);
        }

        /// <summary>Moon 3/17th: Milo physical train escort. Round 6: FULL bridge to DOTS ID=0 + 17th Hour support + playtest path.</summary>
        public void BoardTrain(Vector3 positionOnTrain) => BoardTrain(positionOnTrain, false);

        public void BoardTrain(Vector3 positionOnTrain, bool is17thHour)
        {
            Vector3 offset = new Vector3(-1.8f, 1.4f, 1.5f);
            if (transform != null)
            {
                transform.position = positionOnTrain + offset;
                transform.forward = Vector3.Lerp(transform.forward, Vector3.forward, 0.8f);
            }
            // Phase 1 quarantine: CompanionManager disabled (multi-companion, Moon 2+). Restore in Phase 2.
            // CompanionManager.Instance?.SyncCompanionToDOTS(0, true, positionOnTrain + offset, false, 0, false, is17thHour, 5);
            // if (is17thHour)
            //     CompanionManager.Instance?.Trigger17thHourCompanionEscort(0, positionOnTrain);
            Debug.Log($"[Milo] BoardTrain escort (CompanionManager DOTS bridge deferred to Phase 2; 17th={is17thHour}).");
        }

        /// <summary>Moon 5: Milo's outburst at White City demolition.</summary>
        public void WitnessWhiteCityDemolition()
        {
            if (_whiteCityOutburst) return;
            _whiteCityOutburst = true;
            DialogueManager.Instance?.PlayContextDialogue("milo_white_city_rage");
            AddTrust(15f);
            TriggerSincereMoment();
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Moon 7: Milo goes silent witnessing Korath's sacrifice.</summary>
        public void WitnessKorathSacrifice()
        {
            if (_korathSacrificeWitnessed) return;
            _korathSacrificeWitnessed = true;
            // Milo goes silent — no dialogue, just trust
            AddTrust(12f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        void TriggerSincereMoment()
        {
            _sincereMoments++;
            OnSincereMoment?.Invoke();
        }

        // ─── External Notifications ──────────────────

        /// <summary>Player built something beautiful.</summary>
        public void NotifyBuildingRestored()
        {
            AddTrust(3f);
            Save.SaveManager.Instance?.MarkDirty();
            if (TrustLevel >= MiloTrustLevel.Invested)
                DialogueManager.Instance?.PlayContextDialogue("milo_impressed_build");
        }

        /// <summary>
        /// STRONG EMOTIONAL PAYOFF for the very first successful restoration (Moon 1 first 10min magic with scaffold first ruin).
        /// Milo drops cynicism, shows heart — trust spike, special dialogue, haptic, HUD moment.
        /// </summary>
        public void TriggerFirstRestorationEmotional(string siteName = "the first dome")
        {
            if (_sincereMoments > 0) return; // only once
            _sincereMoments++;

            AddTrust(28f);  // big leap on the "we did it together" moment
            Save.SaveManager.Instance?.MarkDirty();

            // Special heart moment dialogue (falls back gracefully if line missing)
            DialogueManager.Instance?.PlayContextDialogue("milo_first_restoration");
            DialogueManager.Instance?.PlayLineById("milo_heart_opens");

            // HUD banner for emotional payoff
            GameEvents.RaiseHUDShowBanner("Milo (moved)", $"...I didn't think anything was left worth saving. You... you actually brought it back. Maybe there's hope after all.", 7f);

            OnSincereMoment?.Invoke();
            TriggerSincereMoment();

            // Rich haptic payoff (F310 rumble for emotional weight)
            HapticFeedbackManager.Instance?.PlayPerfectTune();
            HapticFeedbackManager.Instance?.TriggerF310Rumble(0.4f, 0.85f, 1.8f);

            Debug.Log($"[Milo] First restoration emotional payoff triggered for {siteName} — trust now {Trust:F0}, heart opened.");
        }

        /// <summary>Zone completion boosts trust.</summary>
        public void NotifyZoneComplete()
        {
            AddTrust(5f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Combat victory — Milo quips.</summary>
        public void NotifyCombatVictory()
        {
            AddTrust(2f);
            DialogueManager.Instance?.PlayContextDialogue("milo_combat_quip");
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Boss encounter — Milo reactions.</summary>
        public void OnBossEncountered(string bossName)
        {
            DialogueManager.Instance?.PlayContextDialogue($"milo_boss_{bossName.ToLower()}_intro");
            AddTrust(2f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Boss defeated — Milo celebrates.</summary>
        public void OnBossDefeated(string bossName)
        {
            DialogueManager.Instance?.PlayContextDialogue($"milo_boss_{bossName.ToLower()}_victory");
            AddTrust(5f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Building restored — Milo reacts based on trust level.</summary>
        public void OnBuildingRestored(string buildingId)
        {
            NotifyBuildingRestored();
        }

        // ─── Save / Load ────────────────────────────

        public MiloSaveData GetSaveData()
        {
            return new MiloSaveData
            {
                trust = _trust,
                introduced = _introduced,
                artifactsAppraised = _artifactsAppraised,
                jokesDelivered = _jokesDelivered,
                sincereMoments = _sincereMoments,
                orphanTrainWitnessed = _orphanTrainWitnessed,
                whiteCityOutburst = _whiteCityOutburst,
                korathSacrificeWitnessed = _korathSacrificeWitnessed
            };
        }

        public void LoadSaveData(MiloSaveData data)
        {
            _trust = data.trust;
            _introduced = data.introduced;
            _artifactsAppraised = data.artifactsAppraised;
            _jokesDelivered = data.jokesDelivered;
            _sincereMoments = data.sincereMoments;
            _orphanTrainWitnessed = data.orphanTrainWitnessed;
            _whiteCityOutburst = data.whiteCityOutburst;
            _korathSacrificeWitnessed = data.korathSacrificeWitnessed;
        }
    }

    public enum MiloTrustLevel : byte
    {
        Cynical = 0,
        Curious = 1,
        Invested = 2,
        Transformed = 3
    }

    /// <summary>
    /// docs/15 §10 Companion: Milo state machine.
    /// FOLLOW = default tag-along; IDLE = out of frame during tuning;
    /// REACT = brief 1s flash on RS milestones; SPEAK = active dialogue;
    /// HIDE = combat retreat; CELEBRATE = 3s reaction post-restoration.
    /// </summary>
    public enum MiloState : byte
    {
        Follow = 0,
        Idle = 1,
        React = 2,
        Speak = 3,
        Hide = 4,
        Celebrate = 5
    }

    [System.Serializable]
    public class MiloSaveData
    {
        public float trust;
        public bool introduced;
        public int artifactsAppraised;
        public int jokesDelivered;
        public int sincereMoments;
        public bool orphanTrainWitnessed;
        public bool whiteCityOutburst;
        public bool korathSacrificeWitnessed;
    }
}

// ROUND 6 Companions: Milo BoardTrain now uses full DOTS hybrid sync bridge + 17th Hour overload. Train escort + solidification playtest complete for ID=0.

