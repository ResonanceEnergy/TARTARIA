using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Lirael — The Echo Child, Companion NPC Controller.
    ///
    /// Spectral orphan from the Mud Flood. Not a ghost — an Echo,
    /// a fragment of consciousness preserved in Aether. Remembers
    /// feelings and songs but not facts. Her simple observations
    /// often contain profound truth.
    ///
    /// Unlocks: Moon 1 (Echohaven — found near the first Aether node)
    /// Trust Arc: Whisper → Remembering → Singing → Manifested
    /// Role: Dissonance detection, children's lore, emotional compass
    ///
    /// Physicality evolves: translucent → semi-solid → solid (Moon 13)
    ///
    /// Design refs: GDD §05 (Characters §2.3), §03A (Main Storyline)
    /// </summary>
    public class LiraelController : MonoBehaviour, ILiraelService
    {
        public static LiraelController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 10f;
        float _trust;

        public float Trust => _trust;
        public LiraelTrustLevel TrustLevel => _trust switch
        {
            < 25f => LiraelTrustLevel.Whisper,
            < 50f => LiraelTrustLevel.Remembering,
            < 75f => LiraelTrustLevel.Singing,
            _ => LiraelTrustLevel.Manifested
        };

        // ─── State ───
        bool _introduced;
        float _solidity;                     // 0=translucent, 1=fully solid
        int _songsRemembered;
        int _dissonanceWarningsGiven;
        bool _orphanTrainRemembered;         // Moon 3 story beat
        bool _childrenChoirConducted;        // Moon 6 story beat
        bool _korathSongsLearned;            // Moon 7 story beat
        bool _fountainHealed;               // Moon 11 story beat
        bool _fullyManifested;              // Moon 13 finale

        // ─── Events ───
        public event System.Action<LiraelTrustLevel> OnTrustChanged;
        public event System.Action OnIntroduced;
        public event System.Action<float> OnSolidityChanged;     // 0-1
        public event System.Action OnDissonanceDetected;
        public event System.Action OnSongRemembered;
        public event System.Action OnFullyManifested;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            ServiceLocator.Lirael = this;
            _trust = initialTrust;
            _solidity = 0.1f;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if (ServiceLocator.Lirael == (object)this) ServiceLocator.Lirael = null;
            }
        }

        // ─── Public API ──────────────────────────────

        /// <summary>Trigger Lirael's introduction at the first Aether node.</summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("lirael_intro");
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
                    case LiraelTrustLevel.Remembering:
                        DialogueManager.Instance?.PlayContextDialogue("lirael_remembering");
                        UpdateSolidity(0.3f);
                        break;
                    case LiraelTrustLevel.Singing:
                        DialogueManager.Instance?.PlayContextDialogue("lirael_singing");
                        UpdateSolidity(0.6f);
                        break;
                    case LiraelTrustLevel.Manifested:
                        DialogueManager.Instance?.PlayLineById("lirael_trust_final");
                        TriggerFullManifestation();
                        break;
                }
            }
        }

        /// <summary>Lirael detects dissonance in the environment.</summary>
        public void DetectDissonance()
        {
            _dissonanceWarningsGiven++;
            OnDissonanceDetected?.Invoke();

            if (TrustLevel >= LiraelTrustLevel.Remembering)
                DialogueManager.Instance?.PlayContextDialogue("lirael_dissonance_warning");
            else
                DialogueManager.Instance?.PlayLineById("lirael_hum_dissonance");
        }

        /// <summary>Lirael remembers a song fragment. Call when RS threshold crossed.</summary>
        public void RememberSong()
        {
            _songsRemembered++;
            OnSongRemembered?.Invoke();
            AddTrust(4f);

            DialogueManager.Instance?.PlayContextDialogue("lirael_song_memory");

            // Each song slightly increases her solidity
            UpdateSolidity(Mathf.Min(_solidity + 0.05f, 1f));
        }

        /// <summary>Ask Lirael about the world. Her answers are half-questions.</summary>
        public void AskAboutWorld()
        {
            string[] contexts = TrustLevel switch
            {
                LiraelTrustLevel.Whisper => new[] { "lirael_whisper_01", "lirael_whisper_02" },
                LiraelTrustLevel.Remembering => new[] { "lirael_remember_01", "lirael_remember_02" },
                LiraelTrustLevel.Singing => new[] { "lirael_song_01", "lirael_song_02" },
                _ => new[] { "lirael_manifest_01", "lirael_manifest_02" }
            };

            string line = contexts[_songsRemembered % contexts.Length];
            DialogueManager.Instance?.PlayLineById(line);
            AddTrust(1f);
        }

        /// <summary>Check if Lirael can touch objects (Singing+ trust).</summary>
        public bool CanTouchObjects()
        {
            return TrustLevel >= LiraelTrustLevel.Singing || _korathSongsLearned;
        }

        /// <summary>Check if Lirael casts shadows (Moon 11+).</summary>
        public bool CastsShadows()
        {
            return _fountainHealed || _solidity >= 0.8f;
        }

        // ─── Story Beats ─────────────────────────────

        /// <summary>Moon 3: Lirael remembers the orphan train.</summary>
        public void RememberOrphanTrain()
        {
            if (_orphanTrainRemembered) return;
            _orphanTrainRemembered = true;
            DialogueManager.Instance?.PlayContextDialogue("lirael_orphan_train");
            AddTrust(10f);
            UpdateSolidity(Mathf.Min(_solidity + 0.1f, 1f));
        }

        /// <summary>Moon 6: Lirael conducts the children's choir.</summary>
        public void ConductChildrenChoir()
        {
            if (_childrenChoirConducted) return;
            _childrenChoirConducted = true;
            DialogueManager.Instance?.PlayContextDialogue("lirael_choir");
            AddTrust(12f);
            UpdateSolidity(0.55f);
        }

        /// <summary>Moon 7: Lirael learns Korath's songs.</summary>
        public void LearnKorathSongs()
        {
            if (_korathSongsLearned) return;
            _korathSongsLearned = true;
            DialogueManager.Instance?.PlayContextDialogue("lirael_korath_songs");
            AddTrust(10f);
            UpdateSolidity(0.7f);
        }

        /// <summary>Moon 11: Fountain healing makes Lirael semi-solid.</summary>
        public void HealAtFountain()
        {
            if (_fountainHealed) return;
            _fountainHealed = true;
            DialogueManager.Instance?.PlayContextDialogue("lirael_fountain_heal");
            AddTrust(15f);
            UpdateSolidity(0.85f);
        }

        // ─── External Notifications ──────────────────

        /// <summary>Something beautiful was restored.</summary>
        public void NotifyBuildingRestored()
        {
            AddTrust(3f);
            DialogueManager.Instance?.PlayContextDialogue("lirael_world_breath");
        }

        /// <summary>Zone completion.</summary>
        public void NotifyZoneComplete()
        {
            AddTrust(5f);
            RememberSong();
        }

        // ─── Internal Helpers ────────────────────────

        void UpdateSolidity(float newSolidity)
        {
            if (newSolidity <= _solidity) return;
            _solidity = newSolidity;
            OnSolidityChanged?.Invoke(_solidity);
        }

        void TriggerFullManifestation()
        {
            if (_fullyManifested) return;
            _fullyManifested = true;
            UpdateSolidity(1f);
            OnFullyManifested?.Invoke();
            Debug.Log("[Lirael] Fully manifested — solid, radiant, voice clear.");
        }

        // ─── Save / Load ────────────────────────────

        public LiraelSaveData GetSaveData()
        {
            return new LiraelSaveData
            {
                trust = _trust,
                introduced = _introduced,
                solidity = _solidity,
                songsRemembered = _songsRemembered,
                dissonanceWarningsGiven = _dissonanceWarningsGiven,
                orphanTrainRemembered = _orphanTrainRemembered,
                childrenChoirConducted = _childrenChoirConducted,
                korathSongsLearned = _korathSongsLearned,
                fountainHealed = _fountainHealed,
                fullyManifested = _fullyManifested
            };
        }

        public void LoadSaveData(LiraelSaveData data)
        {
            _trust = data.trust;
            _introduced = data.introduced;
            _solidity = data.solidity;
            _songsRemembered = data.songsRemembered;
            _dissonanceWarningsGiven = data.dissonanceWarningsGiven;
            _orphanTrainRemembered = data.orphanTrainRemembered;
            _childrenChoirConducted = data.childrenChoirConducted;
            _korathSongsLearned = data.korathSongsLearned;
            _fountainHealed = data.fountainHealed;
            _fullyManifested = data.fullyManifested;
        }
    }

    public enum LiraelTrustLevel : byte
    {
        Whisper = 0,
        Remembering = 1,
        Singing = 2,
        Manifested = 3
    }

    [System.Serializable]
    public class LiraelSaveData
    {
        public float trust;
        public bool introduced;
        public float solidity;
        public int songsRemembered;
        public int dissonanceWarningsGiven;
        public bool orphanTrainRemembered;
        public bool childrenChoirConducted;
        public bool korathSongsLearned;
        public bool fountainHealed;
        public bool fullyManifested;
    }
}
