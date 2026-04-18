using UnityEngine;
using Tartaria.Core;

namespace Tartaria.Integration
{
    /// <summary>
    /// Commander Thorne — NPC Controller.
    ///
    /// Grizzled veteran of the Resonance Wars. Led the Restoration militia
    /// through the dark years after the Fall. Scarred by the betrayal at
    /// Chronopolis. Pragmatic, protective, strategically minded.
    ///
    /// Unlocks: Moon 3 (Celestial Spire zone)
    /// Trust Arc: Guarded → Cautious → Ally → Brother-in-arms
    /// Role: Combat mentor, strategic intel, militia coordination
    ///
    /// Design refs: GDD §05 (Characters), §03A (Main Storyline)
    /// </summary>
    public class ThorneController : MonoBehaviour
    {
        public static ThorneController Instance { get; private set; }

        // ─── Trust System ───
        [Header("Trust")]
        [SerializeField] float initialTrust = 10f;
        float _trust;

        public float Trust => _trust;
        public ThorneTrustLevel TrustLevel => _trust switch
        {
            < 25f => ThorneTrustLevel.Guarded,
            < 50f => ThorneTrustLevel.Cautious,
            < 75f => ThorneTrustLevel.Ally,
            _ => ThorneTrustLevel.BrotherInArms
        };

        // ─── State ───
        bool _introduced;
        bool _militiaActive;
        int _combatBriefingsGiven;
        int _zonesSecuredTogether;

        // ─── Events ───
        public event System.Action<ThorneTrustLevel> OnTrustChanged;
        public event System.Action OnIntroduced;
        public event System.Action OnMilitiaActivated;

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

        /// <summary>Trigger Thorne's introduction sequence.</summary>
        public void Introduce()
        {
            if (_introduced) return;
            _introduced = true;
            DialogueManager.Instance?.PlayContextDialogue("thorne_intro");
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

                // Trust milestone dialogue
                switch (newLevel)
                {
                    case ThorneTrustLevel.Cautious:
                        DialogueManager.Instance?.PlayContextDialogue("thorne_guarded");
                        break;
                    case ThorneTrustLevel.Ally:
                        DialogueManager.Instance?.PlayContextDialogue("thorne_trusted");
                        ActivateMilitia();
                        break;
                    case ThorneTrustLevel.BrotherInArms:
                        DialogueManager.Instance?.PlayLineById("thorne_trust_02");
                        break;
                }
            }
        }

        /// <summary>Request a combat briefing from Thorne.</summary>
        public void RequestBriefing()
        {
            if (TrustLevel < ThorneTrustLevel.Cautious)
            {
                DialogueManager.Instance?.PlayContextDialogue("thorne_guarded");
                return;
            }

            DialogueManager.Instance?.PlayContextDialogue("thorne_combat_briefing");
            _combatBriefingsGiven++;

            // Each briefing grants minor trust
            AddTrust(2f);
        }

        /// <summary>Request strategic advice for current zone.</summary>
        public void RequestStrategy()
        {
            if (TrustLevel < ThorneTrustLevel.Ally)
            {
                DialogueManager.Instance?.PlayContextDialogue("thorne_guarded");
                return;
            }
            DialogueManager.Instance?.PlayContextDialogue("thorne_strategy");
        }

        /// <summary>Notify Thorne of a zone completion.</summary>
        public void NotifyZoneSecured()
        {
            _zonesSecuredTogether++;
            AddTrust(8f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        /// <summary>Notify Thorne of combat victory (happens via GameLoop).</summary>
        public void NotifyCombatVictory()
        {
            AddTrust(3f);
            Save.SaveManager.Instance?.MarkDirty();
        }

        public void ActivateMilitia()
        {
            if (_militiaActive) return;
            _militiaActive = true;
            OnMilitiaActivated?.Invoke();
            Audio.AudioManager.Instance?.PlaySFX2D("MilitiaActivated");
            Save.SaveManager.Instance?.MarkDirty();
            Debug.Log("[Thorne] Militia activated — zone defense patrols now active.");
        }

        // ─── Save / Load ────────────────────────────

        public ThorneSaveData GetSaveData()
        {
            return new ThorneSaveData
            {
                trust = _trust,
                introduced = _introduced,
                militiaActive = _militiaActive,
                combatBriefingsGiven = _combatBriefingsGiven,
                zonesSecuredTogether = _zonesSecuredTogether
            };
        }

        public void LoadSaveData(ThorneSaveData data)
        {
            _trust = data.trust;
            _introduced = data.introduced;
            _militiaActive = data.militiaActive;
            _combatBriefingsGiven = data.combatBriefingsGiven;
            _zonesSecuredTogether = data.zonesSecuredTogether;
        }
    }

    public enum ThorneTrustLevel : byte
    {
        Guarded = 0,
        Cautious = 1,
        Ally = 2,
        BrotherInArms = 3
    }

    [System.Serializable]
    public class ThorneSaveData
    {
        public float trust;
        public bool introduced;
        public bool militiaActive;
        public int combatBriefingsGiven;
        public int zonesSecuredTogether;
    }
}
