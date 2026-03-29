using UnityEngine;
using Tartaria.Core;

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
    /// Design refs: GDD §05 (Characters §2.2), §03A (Main Storyline)
    /// </summary>
    public class MiloController : MonoBehaviour, IMiloService
    {
        public static MiloController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 15f;
        float _trust;

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
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Trigger Milo's introduction in Echohaven.</summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("milo_intro");
            OnIntroduced?.Invoke();
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
        }

        /// <summary>Moon 5: Milo's outburst at White City demolition.</summary>
        public void WitnessWhiteCityDemolition()
        {
            if (_whiteCityOutburst) return;
            _whiteCityOutburst = true;
            DialogueManager.Instance?.PlayContextDialogue("milo_white_city_rage");
            AddTrust(15f);
            TriggerSincereMoment();
        }

        /// <summary>Moon 7: Milo goes silent witnessing Korath's sacrifice.</summary>
        public void WitnessKorathSacrifice()
        {
            if (_korathSacrificeWitnessed) return;
            _korathSacrificeWitnessed = true;
            // Milo goes silent — no dialogue, just trust
            AddTrust(12f);
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
            if (TrustLevel >= MiloTrustLevel.Invested)
                DialogueManager.Instance?.PlayContextDialogue("milo_impressed_build");
        }

        /// <summary>Zone completion boosts trust.</summary>
        public void NotifyZoneComplete()
        {
            AddTrust(5f);
        }

        /// <summary>Combat victory — Milo quips.</summary>
        public void NotifyCombatVictory()
        {
            AddTrust(2f);
            DialogueManager.Instance?.PlayContextDialogue("milo_combat_quip");
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
