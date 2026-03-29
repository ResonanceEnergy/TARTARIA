using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Veritas the Cathedral Organist — Companion NPC Controller.
    ///
    /// Ancient echo of the cathedral's organist, trapped mid-performance
    /// for centuries. Speaks in musical metaphors, precise and passionate.
    ///
    /// Unlocks: Moon 6 (Living Library — manifests at organ bench Day 3)
    /// Trust Arc: Fragment → Passage → Harmony → Transcendent
    /// Role: Organ technique teacher, performance combat partner, Requiem conductor
    ///
    /// Design refs: GDD §03C (Moon 6 Mechanics), §05 (Characters), §20 (Quest DB)
    /// </summary>
    public class VeritasController : MonoBehaviour
    {
        public static VeritasController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 0f;
        float _trust;

        public float Trust => _trust;
        public VeritasTrustLevel TrustLevel => _trust switch
        {
            < 25f => VeritasTrustLevel.Fragment,
            < 50f => VeritasTrustLevel.Passage,
            < 75f => VeritasTrustLevel.Harmony,
            _ => VeritasTrustLevel.Transcendent
        };

        // ─── State ───
        bool _introduced;
        int _lessonsGiven;
        int _performancesCompleted;
        int _registersRestored;            // 0-5 organ registers
        bool _requiemPerformed;            // Moon 6 climax
        bool _bellTowerAssisted;           // Moon 12 collaboration
        bool _finalNoteCompleted;          // Moon 13 unfinished piece

        // ─── Events ───
        public event System.Action<VeritasTrustLevel> OnTrustChanged;
        public event System.Action OnIntroduced;
        public event System.Action<int> OnRegisterRestored;     // register count
        public event System.Action OnRequiemPerformed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _trust = initialTrust;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Trigger Veritas manifestation at the organ bench.</summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("veritas_intro");
            OnIntroduced?.Invoke();
            Debug.Log("[Veritas] Manifested at organ bench.");
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

                switch (newLevel)
                {
                    case VeritasTrustLevel.Passage:
                        DialogueManager.Instance?.PlayContextDialogue("veritas_passage_unlocked");
                        break;
                    case VeritasTrustLevel.Harmony:
                        DialogueManager.Instance?.PlayContextDialogue("veritas_harmony_unlocked");
                        break;
                    case VeritasTrustLevel.Transcendent:
                        DialogueManager.Instance?.PlayLineById("veritas_trust_final");
                        break;
                }
            }
        }

        /// <summary>Teach the player an organ technique.</summary>
        public void GiveLesson()
        {
            _lessonsGiven++;

            string[] contexts = TrustLevel switch
            {
                VeritasTrustLevel.Fragment => new[] { "veritas_lesson_fragment_01", "veritas_lesson_fragment_02" },
                VeritasTrustLevel.Passage => new[] { "veritas_lesson_passage_01", "veritas_lesson_passage_02" },
                VeritasTrustLevel.Harmony => new[] { "veritas_lesson_harmony_01", "veritas_lesson_harmony_02" },
                _ => new[] { "veritas_lesson_transcendent_01", "veritas_lesson_transcendent_02" }
            };

            string line = contexts[_lessonsGiven % contexts.Length];
            DialogueManager.Instance?.PlayLineById(line);
            AddTrust(3f);
        }

        /// <summary>Restore an organ register (0-4). Trust bonus on each.</summary>
        public void RestoreRegister(int registerIndex)
        {
            if (registerIndex < 0 || registerIndex >= 5) return;
            _registersRestored = Mathf.Min(_registersRestored + 1, 5);
            OnRegisterRestored?.Invoke(_registersRestored);
            AddTrust(5f);

            if (_registersRestored >= 5)
            {
                DialogueManager.Instance?.PlayLineById("veritas_all_registers");
                AddTrust(10f);
            }
            else
            {
                DialogueManager.Instance?.PlayContextDialogue("veritas_register_restored");
            }
        }

        /// <summary>Complete a performance (combat or showcase).</summary>
        public void CompletePerformance(float accuracy)
        {
            _performancesCompleted++;

            if (accuracy >= 0.98f)
            {
                DialogueManager.Instance?.PlayContextDialogue("veritas_transcendent_performance");
                AddTrust(8f);
            }
            else if (accuracy >= 0.90f)
            {
                DialogueManager.Instance?.PlayContextDialogue("veritas_gold_performance");
                AddTrust(5f);
            }
            else if (accuracy >= 0.80f)
            {
                DialogueManager.Instance?.PlayContextDialogue("veritas_silver_performance");
                AddTrust(3f);
            }
            else
            {
                DialogueManager.Instance?.PlayContextDialogue("veritas_practice_more");
                AddTrust(1f);
            }
        }

        // ─── Story Beats ─────────────────────────────

        /// <summary>Moon 6 Climax: The Cymatic Requiem.</summary>
        public void PerformRequiem()
        {
            if (_requiemPerformed) return;
            _requiemPerformed = true;
            DialogueManager.Instance?.PlayContextDialogue("veritas_requiem");
            AddTrust(15f);
            OnRequiemPerformed?.Invoke();
        }

        /// <summary>Moon 12: Assist Lirael at Tower 6 with organ-tower hybrid.</summary>
        public void AssistBellTower()
        {
            if (_bellTowerAssisted) return;
            _bellTowerAssisted = true;
            DialogueManager.Instance?.PlayContextDialogue("veritas_bell_tower_assist");
            AddTrust(10f);
        }

        /// <summary>Moon 13: Play the final note of the unfinished Requiem.</summary>
        public void CompleteFinalNote()
        {
            if (_finalNoteCompleted) return;
            _finalNoteCompleted = true;
            DialogueManager.Instance?.PlayLineById("veritas_final_note");
            AddTrust(20f);
        }

        // ─── External Notifications ──────────────────

        /// <summary>Player restored a building — Veritas hears the harmonics.</summary>
        public void NotifyBuildingRestored()
        {
            AddTrust(2f);
            if (TrustLevel >= VeritasTrustLevel.Passage)
                DialogueManager.Instance?.PlayContextDialogue("veritas_building_harmonics");
        }

        /// <summary>Zone completion boost.</summary>
        public void NotifyZoneComplete()
        {
            AddTrust(5f);
        }

        // ─── Save / Load ────────────────────────────

        public VeritasSaveData GetSaveData()
        {
            return new VeritasSaveData
            {
                trust = _trust,
                introduced = _introduced,
                lessonsGiven = _lessonsGiven,
                performancesCompleted = _performancesCompleted,
                registersRestored = _registersRestored,
                requiemPerformed = _requiemPerformed,
                bellTowerAssisted = _bellTowerAssisted,
                finalNoteCompleted = _finalNoteCompleted,

                // GLC bridge fields
                trustTier = (int)TrustLevel,
                performanceAccuracy = _performancesCompleted > 0 ? 1f : 0f,
                bellTowerSyncComplete = _bellTowerAssisted,
                finalNoteDelivered = _finalNoteCompleted
            };
        }

        public void LoadSaveData(VeritasSaveData data)
        {
            _trust = data.trust;
            _introduced = data.introduced;
            _lessonsGiven = data.lessonsGiven;
            _performancesCompleted = data.performancesCompleted;
            _registersRestored = data.registersRestored;
            _requiemPerformed = data.requiemPerformed;
            _bellTowerAssisted = data.bellTowerAssisted || data.bellTowerSyncComplete;
            _finalNoteCompleted = data.finalNoteCompleted || data.finalNoteDelivered;
        }
    }

    public enum VeritasTrustLevel : byte
    {
        Fragment = 0,
        Passage = 1,
        Harmony = 2,
        Transcendent = 3
    }

    /// <summary>Alias used by GameLoopController save/load code.</summary>
    public enum VeritasTrustTier : byte
    {
        Fragment = 0,
        Passage = 1,
        Harmony = 2,
        Transcendent = 3
    }

    [System.Serializable]
    public class VeritasSaveData
    {
        public float trust;
        public bool introduced;
        public int lessonsGiven;
        public int performancesCompleted;
        public int registersRestored;
        public bool requiemPerformed;
        public bool bellTowerAssisted;
        public bool finalNoteCompleted;

        // Bridge fields consumed by GameLoopController save/load
        public int trustTier;
        public float performanceAccuracy;
        public bool bellTowerSyncComplete;
        public bool finalNoteDelivered;
    }
}
