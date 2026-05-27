using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Korath — NPC Controller.
    ///
    /// Mysterious keeper of the Harmonic Archives. Connected to the original
    /// Tartarian builders. Speaks in riddles and frequencies. Guardian of the
    /// Day Out of Time — the galactic calendar's 13th Moon alignment event.
    ///
    /// Unlocks: Moon 5 (when player reaches RS 528 for the first time)
    /// Trust Arc: Enigmatic → Teacher → Revered → Harmonic Bond
    /// Role: Lore exposition, frequency teaching, Day Out of Time guardian
    ///
    /// Design refs: GDD §05 (Characters), §01 (Lore Bible), §17 (Day Out of Time)
    /// </summary>
    public class KorathController : MonoBehaviour
    {
        public static KorathController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 5f;
        float _trust;

        public float Trust => _trust;
        public KorathTrustLevel TrustLevel => _trust switch
        {
            < 20f => KorathTrustLevel.Enigmatic,
            < 45f => KorathTrustLevel.Teacher,
            < 70f => KorathTrustLevel.Revered,
            _ => KorathTrustLevel.HarmonicBond
        };

        // ─── State ───
        bool _introduced;
        bool _dayOutOfTimeRevealed;
        int _teachingsGiven;
        int _revelationsUnlocked;
        float _highestPlayerRS;

        // ─── Events ───
        public event System.Action<KorathTrustLevel> OnTrustChanged;
        public event System.Action OnIntroduced;
        public event System.Action OnDayOutOfTimeRevealed;
        public event System.Action<int> OnRevelationUnlocked; // revelation index

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _trust = initialTrust;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Public API ──────────────────────────────

        /// <summary>
        /// Trigger Korath's introduction. Requires RS >= 528.
        /// Called by GameLoopController when RS first crosses 528 threshold.
        /// </summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("korath_intro");
            OnIntroduced?.Invoke();
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Check if Korath should appear (RS threshold).</summary>
        public bool ShouldAppear(float currentRS)
        {
            return currentRS >= 528f || _introduced;
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
                    case KorathTrustLevel.Teacher:
                        DialogueManager.Instance?.PlayContextDialogue("korath_teaching");
                        break;
                    case KorathTrustLevel.Revered:
                        UnlockRevelation(0);
                        break;
                    case KorathTrustLevel.HarmonicBond:
                        RevealDayOutOfTime();
                        Input.HapticFeedbackManager.Instance?.PlayDiscovery();
                        break;
                }
            }
        }

        /// <summary>Request a teaching from Korath.</summary>
        public void RequestTeaching()
        {
            if (TrustLevel < KorathTrustLevel.Teacher)
            {
                // Korath speaks in cryptic fragments at low trust
                DialogueManager.Instance?.PlayLineById("korath_teach_01");
                return;
            }

            DialogueManager.Instance?.PlayContextDialogue("korath_teaching");
            _teachingsGiven++;
            AddTrust(3f);
        }

        /// <summary>Request a revelation (high trust only).</summary>
        public void RequestRevelation()
        {
            if (TrustLevel < KorathTrustLevel.Revered)
            {
                DialogueManager.Instance?.PlayContextDialogue("korath_teaching");
                return;
            }

            // Each revelation unlocks progressively deeper cosmological truths
            string[] revelationLines = new[]
            {
                "korath_revelation_aether_origin",      // 1: Aether is not energy -- it is memory
                "korath_revelation_mud_flood_truth",     // 2: The Flood was not water -- it was forgetting
                "korath_revelation_antenna_network",     // 3: Every spire was a node in a planetary mind
                "korath_revelation_frequency_war",       // 4: The old world fell to a war of frequencies
                "korath_revelation_archive_access",      // 5: The Archive is not a place -- it is a state
                "korath_revelation_cosmic_convergence",  // 6: Thirteen moons align once per epoch
                "korath_revelation_player_role",         // 7: You are not restoring Tartaria -- you ARE Tartaria
            };

            int index = Mathf.Min(_revelationsUnlocked, revelationLines.Length - 1);
            DialogueManager.Instance?.PlayLineById(revelationLines[index]);

            _revelationsUnlocked++;
            OnRevelationUnlocked?.Invoke(_revelationsUnlocked);
            AddTrust(5f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Track player RS for dynamic responses.</summary>
        public void UpdatePlayerRS(float rs)
        {
            if (rs > _highestPlayerRS)
            {
                _highestPlayerRS = rs;

                // RS milestone trust gains
                if (rs >= 528f && !_introduced)
                    Introduce();
                if (rs >= 1296f && !_dayOutOfTimeRevealed)
                    RevealDayOutOfTime();
            }
        }

        /// <summary>Ask about the Day Out of Time.</summary>
        public void AskAboutDayOutOfTime()
        {
            if (!_dayOutOfTimeRevealed)
            {
                DialogueManager.Instance?.PlayLineById("korath_teach_04");
                return;
            }
            DialogueManager.Instance?.PlayContextDialogue("day_out_of_time");
        }

        void UnlockRevelation(int index)
        {
            DialogueManager.Instance?.PlayContextDialogue("korath_revelation");
            _revelationsUnlocked = Mathf.Max(_revelationsUnlocked, index + 1);
            OnRevelationUnlocked?.Invoke(index);
        }

        void RevealDayOutOfTime()
        {
            if (_dayOutOfTimeRevealed) return;
            _dayOutOfTimeRevealed = true;
            DialogueManager.Instance?.PlayContextDialogue("day_out_of_time");
            OnDayOutOfTimeRevealed?.Invoke();
            Save.SaveManager.Instance?.MarkDirty();
            AchievementSystem.Instance?.CheckDayOutOfTime();
            Audio.AdaptiveMusicController.Instance?.PlayZoneShift();
            Debug.Log("[Korath] Day Out of Time revealed — end-game event unlocked.");
        }

        // ─── Notify from external systems ────────────

        /// <summary>Zone completion grants Korath trust.</summary>
        public void NotifyZoneComplete()
        {
            AddTrust(6f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Building restoration grants minor trust.</summary>
        public void NotifyBuildingRestored()
        {
            AddTrust(2f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        // ═══ Round 6 Production: Deeper Korath/Thorne escort + 17th Hour integration (star cartographer on rails) ═══
        /// <summary>Korath physical star-reader escort on train. Full DOTS bridge + 17th playtest path (ID=3).</summary>
        public void BoardTrainForKorathEscort(Vector3 railPosition, bool enable17thHour = false)
        {
            if (transform != null)
            {
                Vector3 korathOffset = new Vector3(-0.4f, 3.1f, 0.8f);
                transform.position = railPosition + korathOffset;
                transform.forward = Vector3.Lerp(transform.forward, new Vector3(0.2f, 0.9f, 0.3f), 0.7f);
            }
            CompanionManager.Instance?.TriggerCompanionTrainEscort(3, railPosition, enable17thHour);
            if (enable17thHour) CompanionManager.Instance?.Trigger17thHourCompanionEscort(3, railPosition);
            DialogueManager.Instance?.PlayContextDialogue(enable17thHour ? "korath_17th_hour_rail_stars" : "korath_train_escort_lore");
            Debug.Log($"[Korath] Deeper escort ID=3 DOTS bridge + 17th Hour star alignment. Train escort + solidification playtest ready.");
            Save.SaveManager.Instance?.MarkDirty();
        }

        public void Trigger17thHourStarAlignment()
        {
            AddTrust(14f);
            CompanionManager.Instance?.SyncCompanionToDOTS(3, true, transform ? transform.position : Vector3.zero, false, 0, false, true, 18);
            DialogueManager.Instance?.PlayContextDialogue("korath_17th_alignment");
            Debug.Log("[Korath] 17th Hour alignment — DOTS bond/escort VFX + calendar density.");
        }

        /// <summary>World Choice W3: Player allows Korath's sacrifice ritual.</summary>
        public void CompleteSacrificeRitual()
        {
            AddTrust(15f);
            Save.SaveManager.Instance?.MarkDirty();
            Debug.Log("[Korath] Sacrifice ritual completed by player choice.");
        }

        /// <summary>World Choice W3: Player prevents the sacrifice.</summary>
        public void PreventSacrifice()
        {
            AddTrust(-10f);
            Save.SaveManager.Instance?.MarkDirty();
            Debug.Log("[Korath] Sacrifice prevented by player choice.");
        }

        // ─── Save / Load ────────────────────────────

        public KorathSaveData GetSaveData()
        {
            return new KorathSaveData
            {
                trust = _trust,
                introduced = _introduced,
                dayOutOfTimeRevealed = _dayOutOfTimeRevealed,
                teachingsGiven = _teachingsGiven,
                revelationsUnlocked = _revelationsUnlocked,
                highestPlayerRS = _highestPlayerRS
            };
        }

        public void LoadSaveData(KorathSaveData data)
        {
            _trust = data.trust;
            _introduced = data.introduced;
            _dayOutOfTimeRevealed = data.dayOutOfTimeRevealed;
            _teachingsGiven = data.teachingsGiven;
            _revelationsUnlocked = data.revelationsUnlocked;
            _highestPlayerRS = data.highestPlayerRS;
        }
    }

    public enum KorathTrustLevel : byte
    {
        Enigmatic = 0,
        Teacher = 1,
        Revered = 2,
        HarmonicBond = 3
    }

    [System.Serializable]
    public class KorathSaveData
    {
        public float trust;
        public bool introduced;
        public bool dayOutOfTimeRevealed;
        public int teachingsGiven;
        public int revelationsUnlocked;
        public float highestPlayerRS;
    }
}
